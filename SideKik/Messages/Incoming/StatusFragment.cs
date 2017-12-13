using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class StatusFragment : Fragment
	{
		public JabberID Subject { get; }
		public string Text { get; }

		public StatusFragment(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "status");

			//<status jid="SUBJECT">TEXT</status>

			Subject = JabberID.Parse(node.Attributes["jid"].Value);
			Text = node.InnerText;
		}
	}
}
