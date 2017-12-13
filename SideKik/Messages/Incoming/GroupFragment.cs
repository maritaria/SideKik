using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public abstract class GroupFragment : Fragment
	{
		public JabberID GroupID { get; }

		public GroupFragment(XmlNode node)
		{
			GroupID = JabberID.Parse(node.Attributes["jid"].Value);
		}

		public static GroupFragment Parse(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "g");

			if (node.ChildNodes.Count != 0)
			{
				if (node.ChildNodes.Cast<XmlNode>().All(n => n.LocalName == "l"))
				{
					//leave list
					return new GroupLeaveListFragment(node);
				}
				else
				{
					//Roster
					return new GroupDetailsFragment(node);
				}
			}
			else
			{
				//Just a group reference
				return new GroupReferenceFragment(node);
			}
		}
	}
}
