using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class RequestFragment : Fragment
	{
		public bool DeliveryReceipt { get; }
		public bool ReadReceipt { get; }
		public RequestFragment(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "request");
			Contract.Requires(node.NamespaceURI == "kik:message:receipt");

			//<request d="false" r="false" xmlns="kik:message:receipt"/>

			DeliveryReceipt = bool.Parse(node.Attributes["d"].Value);
			ReadReceipt = bool.Parse(node.Attributes["r"].Value);
		}
	}
}
