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
		public delegate void Callback(XmlReader reader);

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
				using (var reader = _connection.ReadXml())
				{
					reader.Read();
					if (reader.LocalName == "ack") continue;

					string id = reader.GetAttribute("id");
					Guid idGuid;
					Callback callback;
					if (id != null && Guid.TryParse(id, out idGuid) && _requests.TryGetValue(idGuid, out callback))
					{
						_requests.Remove(idGuid);
						callback(reader);
						continue;
					}

					string tag = reader.LocalName;
					if (_tagHandlers.TryGetValue(tag, out callback))
					{
						callback(reader);
						continue;
					}

					string type = reader.GetAttribute("type");
					if (_typedTagHandlers.TryGetValue(new Tuple<string, string>(tag, type), out callback))
					{
						callback(reader);
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
