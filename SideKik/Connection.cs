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
		private XmlReader _xmlReader;
		private StreamWriter _writer;
		private XmlWriter _xmlWriter;
		private string _lastRead;
		public string LastRead => _lastRead;

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

			string response = Read();
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
			_xmlReader = XmlReader.Create(_reader, new XmlReaderSettings
			{
				CloseInput = true,
				ConformanceLevel = ConformanceLevel.Fragment,
				IgnoreComments = true,
				IgnoreWhitespace = true,
			});
			_xmlWriter = XmlWriter.Create(_writer, new XmlWriterSettings
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

		public string Read()
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
			if (result == "</stream:stream>" || result == "</k>")
			{
				DisposeStream();
				throw new ApiException("server closed connection");
			}
			Debug.WriteLine("READ " + result);
			_lastRead = result;
			return result;
		}

		public XmlReader ReadXml()
		{
			string response = Read();
			StringReader reader = new StringReader(response);
			return XmlReader.Create(reader, new XmlReaderSettings
			{
				CloseInput = true,
				ConformanceLevel = ConformanceLevel.Fragment,
				IgnoreComments = true,
				IgnoreWhitespace = true,
			});
		}

		public XmlNode ReadNode()
		{

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

			using (var ack = ReadXml())
			{
				ack.Read();
				if (ack.LocalName != "ack")
				{
					throw new ApiException("expected ack tag");
				}
				if (ack.GetAttribute("id") != guid)
				{
					throw new ApiException("acknowledgement for different query, is someone else using the stream?");
				}
			}

			LoginResult login = new LoginResult();
			login.xdata = new Dictionary<string, string>();
			using (var response = ReadXml())
			{
				response.Read();
				if (response.LocalName != "iq")
				{
					throw new ApiException("expected iq tag");
				}
				if (response.GetAttribute("id") != guid)
				{
					throw new ApiException("response for different query, is someone else using the stream?");
				}
				switch (response.GetAttribute("type"))
				{
					case "result": break;
					case "error": throw new ApiException("server returned an error");
					default: throw new ApiException("unknown response type");
				}
				response.Read();
				if (response.LocalName != "query")
				{
					throw new ApiException("expected query tag");
				}
				if (response.NamespaceURI != "jabber:iq:register")
				{
					throw new ApiException("expected jabber:iq:register namespace");
				}
				response.Read();
				while (response.NodeType != XmlNodeType.EndElement)
				{
					switch (response.LocalName)
					{
						case "node": login.JabberID = new JabberID(response.ReadElementContentAsString(), "talk.kik.com"); break;
						case "email":
							login.IsEmailConfirmed = bool.Parse(response.GetAttribute("confirmed"));
							login.Email = response.ReadElementContentAsString();
							break;

						case "username": login.Username = response.ReadElementContentAsString(); break;
						case "first": login.Firstname = response.ReadElementContentAsString(); break;
						case "last": login.Lastname = response.ReadElementContentAsString(); break;
						case "xdata":
							response.Read();
							while (response.NodeType != XmlNodeType.EndElement)
							{
								if (response.LocalName != "record") throw new ApiException("expected a record");
								login.xdata.Add(response.GetAttribute("pk"), response.ReadElementContentAsString());
							}
							response.ReadEndElement();
							break;

						case "xiphias":
							response.Skip();
							break;

						default: response.Skip(); break;
					}
				}
				response.ReadEndElement();
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
			answerReader.Read();

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