using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik
{
	public sealed class MessagePump
	{
		public delegate Task Callback(XmlNode message);
		public delegate Task Middleware(XmlNode message, Callback continuation);

		private Connection _connection;
		private Dictionary<Guid, Callback> _requestHandlers = new Dictionary<Guid, Callback>();
		private Dictionary<string, Callback> _tagHandlers = new Dictionary<string, Callback>();
		private Dictionary<Tuple<string, string>, Callback> _typedTagHandlers = new Dictionary<Tuple<string, string>, Callback>();

		public Callback Handler;

		public MessagePump(Connection conn)
		{
			Contract.Requires(conn != null);
			_connection = conn;
		}

		public void AddHandler(Guid requestID, Callback callback)
		{
			Contract.Requires(callback != null);

			_requestHandlers.Add(requestID, callback);
		}

		public void AddHandler(string tag, Callback callback)
		{
			Contract.Requires(!string.IsNullOrEmpty(tag));
			Contract.Requires(callback != null);

			_tagHandlers.Add(tag, callback);
		}

		public void AddHandler(string tag, string type, Callback callback)
		{
			Contract.Requires(!string.IsNullOrEmpty(tag));
			Contract.Requires(!string.IsNullOrEmpty(type));
			Contract.Requires(callback != null);

			_typedTagHandlers.Add(new Tuple<string, string>(tag, type), callback);
		}

		public void RemoveHandler(Guid requestID)
		{
			_requestHandlers.Remove(requestID);
		}

		public async void Run()
		{
			var reader = _connection.GetReader();
			while (_connection.IsConnected)
			{
				while (reader.NodeType != XmlNodeType.Element)
				{
					await reader.ReadAsync();
				}
				var message = ReadNode(reader, new XmlDocument());
				message.OwnerDocument.AppendChild(message);
				Contract.Assume(reader.Depth == 0);
				Callback callback = FindCallback(message);
				if (callback != null)
				{
					callback(message);
				}
				else
				{

				}
				while (reader.Depth != 0)
				{
					await reader.ReadAsync();
				}
			}
		}

		private XmlNode ReadNode(XmlReader reader, XmlDocument doc)
		{
			//Create element
			var result = doc.CreateElement(reader.LocalName);

			//Read attributes
			if (reader.HasAttributes)
			{
				reader.MoveToFirstAttribute();
				do
				{
					var attr = doc.CreateAttribute(reader.Name);
					attr.Value = reader.Value;
					result.Attributes.Append(attr);
				}
				while (reader.MoveToNextAttribute());
				reader.MoveToElement();
			}

			if (!reader.IsEmptyElement)
			{
				//Read children
				int parentDepth = reader.Depth;
				reader.Read();
				Contract.Assert(reader.Depth == parentDepth + 1);
				if (reader.NodeType == XmlNodeType.Text)
				{

				}
				while (reader.Depth > parentDepth)
				{
					Contract.Assert(reader.Depth == parentDepth + 1);
					var child = ReadNode(reader, doc);
					result.AppendChild(child);
					reader.Read();
				}
				Contract.Assert(reader.Depth == parentDepth);
			}
			else
			{

			}

			return result;
		}

		private Callback FindCallback(XmlNode message)
		{
			string tag = message.LocalName;
			Callback callback;
			if (_tagHandlers.TryGetValue(tag, out callback))
			{
				return callback;
			}

			string id = message.Attributes["id"]?.Value;
			Guid idGuid;
			if (id != null && Guid.TryParse(id, out idGuid) && _requestHandlers.TryGetValue(idGuid, out callback))
			{
				return callback;
			}

			string type = message.Attributes["type"]?.Value;
			if (_typedTagHandlers.TryGetValue(new Tuple<string, string>(tag, type), out callback))
			{
				return callback;
			}

			return null;
		}
	}
}