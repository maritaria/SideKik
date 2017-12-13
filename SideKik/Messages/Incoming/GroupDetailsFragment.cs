using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class GroupDetailsFragment : GroupFragment
	{
		private HashSet<JabberID> _members = new HashSet<JabberID>();
		private HashSet<JabberID> _admins = new HashSet<JabberID>();
		private HashSet<JabberID> _banned = new HashSet<JabberID>();

		public bool IsPublic { get; }
		public string Code { get; }
		public string Name { get; }
		public string Picture { get; }
		public DateTime PictureTimestamp { get; }
		public JabberID Owner { get; }
		public IEnumerable<JabberID> Members => _members;
		public IEnumerable<JabberID> Admins => _admins;
		public IEnumerable<JabberID> Banned => _banned;

		public GroupDetailsFragment(XmlNode node) : base(node)
		{
			Contract.Requires(node != null);
			/*
				<g is-public="true" jid="[GROUP_JID]">
					<code>#[PUBLIC_GROUP_HASHCODE]</code>
					<n>[GROUP_DISPLAY_NAME]</n>
					<pic ts="1505911808105">[URL]</pic>
					<m>[NEW_MEMBER_JID]</m><!-- the new member -->
					<m a="1">[BOT_JID]</m><!-- bot, also an admin -->
					<m a="1">[USER_JID]</m><!-- some admin -->
					<m a="1" s="1">[USER_JID]</m><!-- the owner -->
				</g>
			 */

			IsPublic = bool.Parse(node.Attributes["is-public"]?.Value);
			foreach (var child in node.ChildNodes.Cast<XmlNode>())
			{
				switch (child.LocalName)
				{
					case "code": Code = child.InnerText; break;
					case "n": Name = child.InnerText; break;
					case "pic":
						Picture = child.InnerText;
						PictureTimestamp = UnixTime.DateTimeFromUnixTimestampMillis(long.Parse(child.Attributes["ts"].Value));
						break;

					case "m":
						bool isOwner = child.Attributes["s"]?.Value == "1";
						bool isAdmin = isOwner || child.Attributes["a"]?.Value == "1";
						var id = JabberID.Parse(child.InnerText);
						_members.Add(id);
						if (isAdmin) _admins.Add(id);
						if (isOwner) Owner = id;
						break;

					case "b": _banned.Add(JabberID.Parse(child.InnerText)); break;
					default: throw new NotImplementedException(); break;
				}
			}
		}
	}
}