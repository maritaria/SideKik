using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class BodyFragment : Fragment
	{
		public string Text { get; }
		public BodyFragment(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "body");

			Text = node.InnerText;
		}
	}
}
