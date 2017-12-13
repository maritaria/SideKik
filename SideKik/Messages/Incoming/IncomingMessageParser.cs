using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class IncomingMessageParser
	{


		public IncomingMessageParser(IncomingRouter router)
		{
			router.AddHandler("message", HandleData);
		}

		private Task HandleData(XmlNode data)
		{
			string type = data.Attributes["type"].Value;
			switch (type)
			{
				case "chat":
				case "groupchat":
					var message = new IncomingMessageData(data);
					return ParseMessage(message);
				default:
					throw new NotImplementedException("unknown message type: " + type);
					break;
			}
		}

		private Task ParseMessage(IncomingMessageData message)
		{
			var group = message.GetFragment<GroupFragment>();
			if (group != null)
			{
				Console.WriteLine($"Group: {group.GroupID}");
			}

			var body = message.GetFragment<BodyFragment>();
			if (body != null)
			{
				Console.WriteLine($"{message.From}: {body.Text}");
			}

			var content = message.GetFragment<ContentFragment>();
			if (content != null)
			{

			}

			var groupDetails = message.GetFragment<GroupDetailsFragment>();
			var status = message.GetFragment<StatusFragment>();
			if (groupDetails != null && status != null)
			{
				var newMember = status.Subject;
			}

			var isTyping = message.GetFragment<IsTypingFragment>();
			if (isTyping != null)
			{
				Console.WriteLine($"IsTyping: {message.From} {isTyping.State}");
			}

			var leaveList = message.GetFragment<GroupLeaveListFragment>();
			if (leaveList != null)
			{
				Console.WriteLine($"{leaveList.GroupID} Leavers:");
				foreach (JabberID leaver in leaveList.Leavers)
				{
					Console.WriteLine($"- {leaver}");
				}
			}
			return Task.FromResult(result: true);
		}
	}
}
