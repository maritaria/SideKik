using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class IncomingRouter
	{
		public delegate Task MessageHandler(XmlNode message);
		public delegate Task MessageMiddleware(XmlNode message, MessageHandler continuation);

		private XmlReader _reader;
		private Dictionary<Guid, MessageHandler> _requestHandlers = new Dictionary<Guid, MessageHandler>();
		private Dictionary<string, MessageHandler> _tagHandlers = new Dictionary<string, MessageHandler>();

		public MessageMiddleware Middleware;

		public IncomingRouter(XmlReader reader)
		{
			Contract.Requires(reader != null);
			_reader = reader;
		}

		public void AddHandler(Guid requestID, MessageHandler callback)
		{
			Contract.Requires(callback != null);

			_requestHandlers.Add(requestID, callback);
		}

		public void AddHandler(string tag, MessageHandler callback)
		{
			Contract.Requires(!string.IsNullOrEmpty(tag));
			Contract.Requires(callback != null);

			_tagHandlers.Add(tag, callback);
		}

		public void RemoveHandler(Guid requestID)
		{
			_requestHandlers.Remove(requestID);
		}

		public async Task Run(CancellationToken cancel)
		{
			while (!cancel.IsCancellationRequested)
			{
				while (_reader.NodeType != XmlNodeType.Element)
				{
					await _reader.ReadAsync();
					cancel.ThrowIfCancellationRequested();
				}
				Contract.Assume(_reader.Depth == 0);
				var doc = await ReadNextMessage(_reader);
				FireCallback(doc.FirstChild);
			}
		}

		private static async Task<XmlDocument> ReadNextMessage(XmlReader reader)
		{
			var result = new XmlDocument();
			using (var messageReader = reader.ReadSubtree())
			{
				// Reader starts before the initial node, read to put the reader on the node being read
				await messageReader.ReadAsync();
				// Read the document async
				var xml = await messageReader.ReadOuterXmlAsync();
				result.LoadXml(xml);
			}

			return result;
		}

		private Task FireCallback(XmlNode message)
		{
			MessageHandler callback = FindCallback(message);
			if (callback != null)
			{
				return UseCallback(message, callback);
			}
			else
			{
				//No handler found
				return Task.FromResult(result: true);
			}
		}

		private MessageHandler FindCallback(XmlNode message)
		{
			MessageHandler callback;

			string id = message.Attributes["id"]?.Value;
			Guid idGuid;
			if (id != null && Guid.TryParse(id, out idGuid) && _requestHandlers.TryGetValue(idGuid, out callback))
			{
				return callback;
			}

			string tag = message.LocalName;
			if (_tagHandlers.TryGetValue(tag, out callback))
			{
				return callback;
			}

			return null;
		}

		private Task UseCallback(XmlNode message, MessageHandler callback)
		{
			if (Middleware != null)
			{
				return Middleware.Invoke(message, callback);
			}
			else
			{
				return callback(message);
			}
		}
	}
}