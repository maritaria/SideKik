using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class IncomingMessageData
	{
		private readonly Dictionary<Type, Fragment> _fragments = new Dictionary<Type, Fragment>();

		public Guid ID { get; }
		public JabberID From { get; }
		public JabberID To { get; }
		public string Type { get; }

		public IEnumerable<Fragment> Fragments => _fragments.Values;

		private IncomingMessageData()
		{
		}

		public IncomingMessageData(XmlNode source)
		{
			Contract.Requires(source != null);

			ID = Guid.Parse(source.Attributes["id"].Value);
			From = JabberID.Parse(source.Attributes["from"].Value);
			To = JabberID.Parse(source.Attributes["to"].Value);
			Type = source.Attributes["type"].Value;

			Contract.Assume(_fragments != null);
			foreach (XmlNode child in source.ChildNodes)
			{
				switch (child.LocalName)
				{
					case "body": _fragments.Add(typeof(BodyFragment), new BodyFragment(child)); break;
					case "kik": _fragments.Add(typeof(KikInfo), new KikInfo(child)); break;
					case "request": _fragments.Add(typeof(RequestFragment), new RequestFragment(child)); break;
					case "is-typing": _fragments.Add(typeof(IsTypingFragment), new IsTypingFragment(child)); break;
					case "preview": _fragments.Add(typeof(PreviewFragment), new PreviewFragment(child)); break;
					case "g":
						var groupFragment = GroupFragment.Parse(child);
						_fragments.Add(typeof(GroupFragment), groupFragment);
						_fragments.Add(groupFragment.GetType(), groupFragment);
						break;
					default: break;
					case "status": _fragments.Add(typeof(StatusFragment), new StatusFragment(child)); break;
				}
			}
		}

		public T GetFragment<T>() where T : Fragment
		{
			Fragment result;
			if (_fragments.TryGetValue(typeof(T), out result))
			{
				return (T)result;
			}
			else
			{
				return null;
			}
		}
	}
}