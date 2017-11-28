using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SideKik
{
	public sealed class Roster
	{
		public HashSet<Group> Groups { get; } = new HashSet<Group>();
		public HashSet<Contact> Contacts { get; } = new HashSet<Contact>();

		public static Roster Read(XmlReader reader)
		{
			if (reader.LocalName != "query")
				throw new Exception("Reader should be at the start of the <query> tag");

			reader.Read();

			var result = new Roster();

			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case "g": result.Groups.Add(Group.Read(reader)); break;
					case "item": result.Contacts.Add(Contact.Read(reader)); break;
				}
			}

			return result;
		}

		public sealed class Group
		{
			public JabberID ID { get; set; }
			public bool IsPublic { get; set; }
			public string Hashtag { get; set; }
			public string Name { get; set; }
			public Dictionary<JabberID, Role> Members { get; } = new Dictionary<JabberID, Role>();
			public Uri ProfilePicture { get; set; }

			public static Group Read(XmlReader reader)
			{
				var result = new Group();
				result.IsPublic = bool.Parse(reader.GetAttribute("is-public"));
				result.ID = JabberID.Parse(reader.GetAttribute("jid"));

				reader.Read();
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					switch (reader.LocalName)
					{
						case "code": result.Hashtag = reader.ReadElementContentAsString(); break;
						case "n": result.Name = reader.ReadElementContentAsString(); break;
						case "pic": result.ProfilePicture = new Uri(reader.ReadElementContentAsString()); break;
						case "m":
							var role = Role.Member;
							if (reader.GetAttribute("s") == "1") role = Role.SuperAdmin;
							else if (reader.GetAttribute("a") == "1") role = Role.Admin;
							var member_jid = JabberID.Parse(reader.ReadElementContentAsString());
							result.Members.Add(member_jid, role);
							break;
						case "b":
							var banned_jid = JabberID.Parse(reader.ReadElementContentAsString());
							result.Members.Add(banned_jid, Role.Banned);
							break;
						default: throw new NotImplementedException();
					}
				}
				reader.ReadEndElement();
				return result;
			}

			public enum Role
			{
				Banned,
				Member,
				Admin,
				SuperAdmin,
			}
		}

		public sealed class Contact
		{
			public JabberID ID { get; set; }
			public string Username { get; set; }
			public string DisplayName { get; set; }
			public Uri ProfilePicture { get; set; }
			public bool Verified { get; set; } = false;


			public static Contact Read(XmlReader reader)
			{
				var result = new Contact();
				result.ID = JabberID.Parse(reader.GetAttribute("jid"));

				reader.Read();
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					switch (reader.LocalName)
					{
						case "username": result.Username = reader.ReadElementContentAsString(); break;
						case "display-name": result.DisplayName = reader.ReadElementContentAsString(); break;
						case "pic": result.ProfilePicture = new Uri(reader.ReadElementContentAsString()); break;
						case "verified": result.Verified = true; reader.Skip(); break;
						case "pubkey": reader.Skip(); break;
						default: throw new NotImplementedException();
					}
				}
				reader.ReadEndElement();
				return result;
			}
		}
	}
}