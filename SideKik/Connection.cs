using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace SideKik
{
	public class Connection : IDisposable
	{
		public static readonly int ReadBlockSize = 16384;

		private SslStream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;

		public EnvironmentProfile Profile { get; private set; }

		[Pure]
		public bool IsConnected => _stream != null;

		public void Connect(EnvironmentProfile profile)
		{
			Contract.Requires<ArgumentNullException>(profile != null);
			Contract.Requires<InvalidOperationException>(!IsConnected, "already connected");
			Contract.Ensures(IsConnected);
			Contract.Ensures(Profile == profile);
			Contract.EnsuresOnThrow<ArgumentException>(!IsConnected);
			Contract.EnsuresOnThrow<ApiException>(!IsConnected);

			SetupConnection(profile);

			Write("<k anon=\"\">");

			var response = Read();
			if (response != "<k ok=\"1\">")
			{
				DisposeStream();
				throw new ApiException("server did not ok the connection");
			}
			Profile = profile;
		}

		private void SetupConnection(EnvironmentProfile profile)
		{
			var tcpStream = new TcpClient(profile.EndpointHostname, profile.EndpointPort);
			_stream = new SslStream(tcpStream.GetStream(), false);
			try
			{
				_stream.AuthenticateAsClient(profile.EndpointHostname);
			}
			catch (Exception ex)
			{
				DisposeStream();
				throw new ApiException("unable to connect to backend", ex);
			}

			_reader = new StreamReader(_stream, Encoding.UTF8);
			_writer = new StreamWriter(_stream, Encoding.UTF8);
			_writer.AutoFlush = true;
		}

		public void Close()
		{
			Contract.Ensures(!IsConnected);

			if (_stream != null)
			{
				Write("</k>");
				DisposeStream();
			}
		}

		public void Write(string data)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires<InvalidOperationException>(IsConnected, "must be connected");
			Contract.Ensures(IsConnected);
			Contract.EnsuresOnThrow<ApiException>(!IsConnected);

			try
			{
				Debug.WriteLine("WRITE " + data);
				_writer.Write(data);
			}
			catch (IOException ex)
			{
				DisposeStream();
				throw new ApiException("unable to send to server", ex);
			}
		}

		public XmlWriter WriteXml()
		{
			return XmlWriter.Create(_writer, new XmlWriterSettings
			{
				CheckCharacters = true,
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Fragment,
				Encoding = Encoding.UTF8,
				Indent = false,
				OmitXmlDeclaration = true,
				WriteEndDocumentOnClose = false,
			});
		}

		private string Read()
		{
			Contract.Requires<InvalidOperationException>(IsConnected, "must be connected");
			Contract.Ensures(Contract.Result<string>() != null);
			Contract.Ensures(Contract.Result<string>().Length > 0);
			Contract.Ensures(IsConnected);
			Contract.EnsuresOnThrow<ApiException>(!IsConnected);

			char[] data = new char[ReadBlockSize];
			int dataOffset = 0;

			while (dataOffset == 0 || data[dataOffset - 1] != '>')
			{
				int spaceAvailable = data.Length - dataOffset;
				Contract.Assert(spaceAvailable >= 0);
				if (spaceAvailable == 0)
				{
					Array.Resize(ref data, data.Length * 2);
				}
				int bytesRead;
				try
				{
					bytesRead = _reader.Read(data, dataOffset, data.Length - dataOffset);
					if (bytesRead == 0)
					{
						throw new EndOfStreamException("received zero bytes, server has gone away");
					}
				}
				catch (IOException ex)
				{
					DisposeStream();
					throw new ApiException($"error while communicating with server: {ex.Message}", ex);
				}
				dataOffset += bytesRead;
				Contract.Assert(dataOffset <= data.Length);
			}
			string result = new string(data, 0, dataOffset);
			if (result.EndsWith("</stream:stream>") || result.EndsWith("</k>"))
			{
				DisposeStream();
				throw new ApiException("server closed connection");
			}
			Debug.WriteLine("READ " + result);
			return result;
		}

		private XmlReader ReadXml()
		{
			string response = Read();
			StringReader reader = new StringReader(response);
			var xmlReader = XmlReader.Create(reader, new XmlReaderSettings
			{
				CloseInput = true,
				ConformanceLevel = ConformanceLevel.Fragment,
				IgnoreComments = true,
				IgnoreWhitespace = true,
			});
			xmlReader.Read();
			return xmlReader;
		}

		public IEnumerable<XmlNode> ReadNodes()
		{
			string data = Read();
			var dataReader = new StringReader(data);
			var reader = XmlReader.Create(dataReader, new XmlReaderSettings
			{
				CloseInput = true,
				ConformanceLevel = ConformanceLevel.Fragment,
				IgnoreComments = true,
				IgnoreWhitespace = true,
				ValidationType = ValidationType.None,
			});
			reader.Read();
			while (reader.NodeType != XmlNodeType.None)
			{
				Contract.Assert(reader.NodeType == XmlNodeType.Element);
				var node = new XmlDocument().ReadNode(reader);
				yield return node;
			}
		}

		public LoginResult Login(string username, string password)
		{
			Contract.Requires(Validation.IsValidUsername(username));
			Contract.Requires(password != null);
			Contract.Requires(IsConnected);

			string guid = Guid.NewGuid().ToString();
			var writer = WriteXml();
			writer.WriteStartElement("iq");
			{
				writer.WriteAttributeString("type", "set");
				writer.WriteAttributeString("id", guid);
				writer.WriteStartElement("query", "jabber:iq:register");
				{
					writer.WriteElementString("username", username);
					writer.WriteElementString("passkey-u", Encryption.HashPassword(username, password));
					writer.WriteElementString("device-id", Profile.DeviceID.ToString());
					writer.WriteElementString("version", Profile.Version);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.Close();

			var ack = ReadNodes().Single();
			{
				if (ack.LocalName != "ack")
				{
					throw new ApiException("expected ack tag");
				}
				if (ack.Attributes["id"].InnerText != guid)
				{
					throw new ApiException("acknowledgement for different query, is someone else using the stream?");
				}
			}

			LoginResult login = new LoginResult();
			login.xdata = new Dictionary<string, string>();
			var response = ReadNodes().Single();

			Contract.Assert(response.LocalName == "iq");
			Contract.Assert(response.ChildNodes.Count == 1);
			Contract.Assert(response.ChildNodes[0].LocalName == "query");
			foreach (XmlNode child in response.ChildNodes[0].ChildNodes)
			{
				switch (child.LocalName)
				{
					case "node": login.JabberID = new JabberID(child.InnerText, JabberID.UserDomain); break;
					case "email":
						login.IsEmailConfirmed = bool.Parse(child.Attributes["confirmed"].InnerText);
						login.Email = child.InnerText;
						break;
					case "username": login.Username = child.InnerText; break;
					case "first": login.Firstname = child.InnerText; break;
					case "last": login.Lastname = child.InnerText; break;
					case "xdata":
						foreach (XmlNode xdata in child.ChildNodes)
						{
							Contract.Assert(xdata.LocalName == "record");
							login.xdata.Add(xdata.Attributes["pk"].InnerText, xdata.InnerText);
						}
						break;
					default: break;
				}
			}

			//login stage 2: rebuild connection

			Close();
			SetupConnection(Profile);

			string sid = "1c2a6e90-d1f4-4c07-b0d5-7dfb939d9354";// Encryption.ConvertToKikGuid(Guid.NewGuid()).ToString();
			var jid = login.JabberID;
			var jidWithResource = new JabberID(login.JabberID, "CAN" + Profile.DeviceID);

			string passwordKey = Encryption.HashPassword(username, password);
			var timestamp = "1496333389122";
			string cv = Encryption.HashWithHMAC(Profile, timestamp + ":" + jid);
			string signature = Encryption.RsaSign(Profile, $"{jid}:{Profile.Version}:{timestamp}:{sid}");
			signature = signature.Substring(0, signature.Length - 2);

			Dictionary<string, string> attribs = new Dictionary<string, string>();
			attribs.Add("p", passwordKey);
			attribs.Add("cv", cv);
			attribs.Add("n", "1");
			attribs.Add("v", Profile.Version);
			attribs.Add("conn", "WIFI");
			attribs.Add("to", "talk.kik.com");
			attribs.Add("lang", "en_US");
			attribs.Add("from", jidWithResource.ToString());
			attribs.Add("sid", sid);
			attribs.Add("signed", signature);
			attribs.Add("ts", timestamp);

			string packet = "<k";
			var sorted = Encryption.ToKikHashMap(attribs);
			foreach (KeyValuePair<string, string> attr in sorted)
			{
				packet += $" {attr.Key}=\"{attr.Value}\"";
			}
			packet += ">";
			Write(packet);

			var answerReader = ReadXml();
			if (answerReader.LocalName != "k")
			{
				throw new ApiException("expected k tag response");
			}
			if (answerReader.GetAttribute("ok") != "1")
			{
				throw new ApiException("server did not ok the session");
			}

			return login;
		}

		public struct LoginResult
		{
			public JabberID JabberID;
			public string Email;
			public bool IsEmailConfirmed;
			public string Username;
			public string Firstname;
			public string Lastname;
			public Dictionary<string, string> xdata;
		}

		private void DisposeStream()
		{
			Contract.Ensures(!IsConnected);
			if (_stream != null)
			{
				try
				{
					_stream.Dispose();
				}
				finally
				{
					_stream = null;
				}
			}
		}

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.
				DisposeStream();
				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~Connection() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(!IsConnected || _reader != null);
			Contract.Invariant(!IsConnected || _writer != null);
			Contract.Invariant(!IsConnected || Profile != null);
		}
	}
}