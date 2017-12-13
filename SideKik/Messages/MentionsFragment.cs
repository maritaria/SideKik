using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages
{
	public sealed class MentionsFragment : Fragment
	{
		private List<JabberID> _mentionedBots = new List<JabberID>();
		public MentionsFragment(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "mention");

			/*
				<mention>
					<bot>whoslurking_ii6@talk.kik.com</bot>
				</mention>
			*/

			foreach (XmlNode child in node.ChildNodes)
			{
				_mentionedBots.Add(JabberID.Parse(child.InnerText));
			}
		}
	}
}
