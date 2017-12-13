using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class IsTypingFragment : Fragment
	{
		public bool State { get; }

		public IsTypingFragment(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "is-typing");

			//<is-typing val="true"/>

			State = bool.Parse(node.Attributes["val"].Value);
		}
	}
}