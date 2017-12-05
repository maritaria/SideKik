using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik
{
	public sealed class MessagePump
	{
		public delegate void Callback(XmlNode reader);

		private Connection _connection;
		private Dictionary<Guid, Callback> _requests = new Dictionary<Guid, Callback>();
		private Dictionary<Tuple<string, string>, Callback> _typedTagHandlers = new Dictionary<Tuple<string, string>, Callback>();
		private Dictionary<string, Callback> _tagHandlers = new Dictionary<string, Callback>();

		public MessagePump(Connection conn)
		{
			Contract.Requires(conn != null);
			_connection = conn;
		}

		public void AddHandler(Guid requestID, Callback callback)
		{
			_requests.Add(requestID, callback);
		}

		public void AddHandler(string tag, Callback callback)
		{
			_tagHandlers.Add(tag, callback);
		}

		public void AddHandler(string tag, string type, Callback callback)
		{
			_typedTagHandlers.Add(new Tuple<string, string>(tag, type), callback);
		}

		public void RemoveHandler(Guid requestID)
		{
			_requests.Remove(requestID);
		}

		public void Run()
		{
			while (_connection.IsConnected)
			{
				foreach (var node in _connection.ReadNodes())
				{
					if (node.LocalName == "ack") continue;

					string id = node.Attributes["id"]?.InnerText;
					Guid idGuid;
					Callback callback;
					if (id != null && Guid.TryParse(id, out idGuid) && _requests.TryGetValue(idGuid, out callback))
					{
						_requests.Remove(idGuid);
						callback(node);
						continue;
					}

					string tag = node.LocalName;
					if (_tagHandlers.TryGetValue(tag, out callback))
					{
						callback(node);
						continue;
					}

					string type = node.Attributes["type"].InnerText;
					if (_typedTagHandlers.TryGetValue(new Tuple<string, string>(tag, type), out callback))
					{
						callback(node);
						continue;
					}
					//throw new NotImplementedException();
				}
			}
		}

		private class ActiveRequest
		{
			public DateTime Started = DateTime.Now;
			public bool Acknowledged = false;
			public bool Answered = false;
			public Action<XmlReader> Handler;
		}
	}
}
