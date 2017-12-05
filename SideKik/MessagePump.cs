using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace SideKik
{
	public sealed class MessagePump
	{
		public delegate void Callback(XmlReader reader);

		private Connection _connection;
		private Dictionary<Guid, Callback> _requestHandlers = new Dictionary<Guid, Callback>();
		private Dictionary<string, Callback> _tagHandlers = new Dictionary<string, Callback>();
		private Dictionary<Tuple<string, string>, Callback> _typedTagHandlers = new Dictionary<Tuple<string, string>, Callback>();

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
			while (_connection.IsConnected)
			{
				var reader = _connection.GetReader();

				while (reader.NodeType != XmlNodeType.Element)
				{
					await reader.ReadAsync();
				}

				Callback callback = FindCallback(reader);
				if (callback != null)
				{
					callback.Invoke(reader);
				}
				else
				{

				}
			}
		}

		private Callback FindCallback(XmlReader reader)
		{
			string tag = reader.LocalName;
			Callback callback;
			if (_tagHandlers.TryGetValue(tag, out callback))
			{
				return callback;
			}

			string id = reader.GetAttribute("id");
			Guid idGuid;
			if (id != null && Guid.TryParse(id, out idGuid) && _requestHandlers.TryGetValue(idGuid, out callback))
			{
				return callback;
			}

			string type = reader.GetAttribute("type");
			if (_typedTagHandlers.TryGetValue(new Tuple<string, string>(tag, type), out callback))
			{
				return callback;
			}

			return null;
		}
	}
}