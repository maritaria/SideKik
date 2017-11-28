using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace SideKik
{
	public struct JabberID
	{
		public static readonly string UserDomain = "talk.kik.com";
		public static readonly string GroupDomain = "groups.kik.com";

		private string _node;
		private string _domain;

		public string Node
		{
			get { return _node; }
			set
			{
				Contract.Requires<ArgumentNullException>(value != null);
				_node = value;
			}
		}

		public string Domain
		{
			get { return _domain; }
			set
			{
				Contract.Requires<ArgumentNullException>(value != null);
				_domain = value;
			}
		}

		public string Resource { get; set; }

		public bool IsGroup => Domain == GroupDomain;
		public bool IsUser => Domain == UserDomain;

		public JabberID(string node, string domain)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			Contract.Requires<ArgumentNullException>(domain != null);
			_node = node;
			_domain = domain;
			Resource = null;
		}

		public JabberID(string node, string domain, string resource)
			: this(node, domain)
		{
			Resource = resource;
		}

		public JabberID(JabberID jabberID, string resource)
			: this(jabberID.Node, jabberID.Domain, resource)
		{
		}

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(Resource))
			{
				return $"{Node}@{Domain}/{Resource}";
			}
			else
			{
				return $"{Node}@{Domain}";
			}
		}

		public static JabberID Parse(string v)
		{
			Contract.Requires<ArgumentNullException>(v != null);
			int nodeSplitterPos = v.IndexOf('@');
			if (nodeSplitterPos < 0)
			{
				throw new ArgumentException("no @ splitter");
			}
			string node = v.Substring(0, nodeSplitterPos);
			string remainder = v.Substring(nodeSplitterPos + 1);

			int domainSplitterPos = remainder.IndexOf('/');
			if (domainSplitterPos < 0)
			{
				return new JabberID(node, remainder);
			}
			string domain = remainder.Substring(0, domainSplitterPos);
			string resource = remainder.Substring(domainSplitterPos + 1);
			return new JabberID(node, domain, resource);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is JabberID)) return false;
			JabberID other = (JabberID)obj;
			return Node == other.Node && Domain == other.Domain && Resource == other.Resource;
		}

		public override int GetHashCode()
		{
			int code = 0;
			code ^= (Node != null) ? Node.GetHashCode() : 1;
			code >>= 2;
			code ^= (Domain != null) ? Domain.GetHashCode() : 1;
			code >>= 2;
			code ^= (Resource != null) ? Resource.GetHashCode() : 1;
			return code;
		}
	}
}