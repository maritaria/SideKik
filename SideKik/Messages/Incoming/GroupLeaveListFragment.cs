using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class GroupLeaveListFragment : GroupFragment
	{
		private List<JabberID> _leavers = new List<JabberID>();
		public IEnumerable<JabberID> Leavers => _leavers;

		public GroupLeaveListFragment(XmlNode node) : base(node)
		{
			foreach (var child in node.ChildNodes.Cast<XmlNode>())
			{
				_leavers.Add(JabberID.Parse(child.InnerText));
			}
		}
	}
}