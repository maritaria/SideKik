using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages
{
	public class IncomingMessage
	{
		public Guid ID { get; private set; }
		public JabberID From { get; private set; }
		public JabberID To { get; private set; }
		public string Type { get; private set; }

		public IncomingMessage(XmlNode source)
		{
			Contract.Requires(source != null);
			ID = Guid.Parse(source.Attributes["id"].Value);
			From = JabberID.Parse(source.Attributes["from"].Value);
			To = JabberID.Parse(source.Attributes["to"].Value);
			Type = source.Attributes["type"].Value;
		}

		private IncomingMessage()
		{

		}
	}
}
