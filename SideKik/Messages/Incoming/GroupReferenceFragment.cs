using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class GroupReferenceFragment : GroupFragment
	{
		public GroupReferenceFragment(XmlNode node) : base(node)
		{
		}
	}
}