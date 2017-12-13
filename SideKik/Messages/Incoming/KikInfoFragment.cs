using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class KikInfo : Fragment
	{
		public DateTime Timestamp { get; }
		public string App { get; }
		public bool Qos { get; }
		public bool Hop { get; }
		public bool Push { get; }

		public KikInfo(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "kik");

			//<kik qos="true" app="all" hop="true" timestamp="1510865418472" push="false"/>

			var timestampMillis = long.Parse(node.Attributes["timestamp"].Value);
			Timestamp = UnixTime.DateTimeFromUnixTimestampMillis(timestampMillis);
			App = node.Attributes["app"].Value;
			Qos = bool.Parse(node.Attributes["qos"].Value);
			if (node.Attributes["hop"] != null)
			{
				Hop = bool.Parse(node.Attributes["hop"].Value);
			}
			Push = bool.Parse(node.Attributes["push"].Value);
		}
	}
}
