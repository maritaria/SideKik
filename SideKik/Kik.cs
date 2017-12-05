using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace SideKik
{
	public sealed class Kik
	{
		private Connection _conn;
		private MessagePump _pump;

		public Connection.LoginResult UserInfo { get; }

		public Kik(string username, string password)
		{
			_conn = new Connection();
			_conn.Connect(EnvironmentProfile.Latest);
			UserInfo = _conn.Login(username, password);
			_pump = new MessagePump(_conn);
			_pump.AddHandler("message", "groupchat", HandleGroupChat);
		}

		public void Run()
		{
			_pump.Run();
		}

		private void HandleGroupChat(XmlReader reader)
		{
#warning TODO: Reform

			var node = Message.Read(reader);

			/*
			var root = new XmlDocument().ReadNode(reader);
			var from = JabberID.Parse(root.Attributes["from"].Value);
			var group = JabberID.Parse(root.ChildNodes.Cast<XmlNode>().First(node => node.LocalName == "g").Attributes["jid"].Value);

			var isTypingNode = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.LocalName == "is-typing");
			if (isTypingNode != null)
			{
				HandleIsTyping(group, from, root, isTypingNode);
				return;
			}

			var statusNode = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.LocalName == "status");
			if (statusNode != null)
			{
				HandleGroupStatusUpdate(root, from, group, statusNode);
				return;
			}

			var bodyNode = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.LocalName == "body");
			HandleMessage(root, from, group, bodyNode);
			//*/
		}

		private void HandleGroupStatusUpdate(XmlNode root, JabberID from, JabberID group, XmlNode statusNode)
		{
			string message = statusNode.InnerText;
			if (message.EndsWith("has joined the chat"))
			{
				HandleMemberJoined(group, JabberID.Parse(statusNode.Attributes["jid"].InnerText));
			}
			else if (message.EndsWith("has left the chat"))
			{
				HandleMemberLeft(group, JabberID.Parse(statusNode.Attributes["jid"].InnerText));
			}
		}

		private void HandleMemberJoined(JabberID group, JabberID memberID)
		{
			Console.WriteLine($"[{group}] +++ {memberID}");
			SendMessage(group, File.ReadAllText("intro.txt"));
		}

		private void HandleMemberLeft(JabberID group, JabberID memberID)
		{
			Console.WriteLine($"[{group}] --- {memberID}");
			SendMessage(group, File.ReadAllText("outro.txt"));
		}

		private void HandleIsTyping(JabberID group, JabberID from, XmlNode root, XmlNode isTypingNode)
		{
			Console.WriteLine($"[{group}] {from} is-typing");
		}

		private void HandleMessage(XmlNode root, JabberID from, JabberID group, XmlNode bodyNode)
		{
			if (bodyNode != null)
			{
				Console.WriteLine($"[{group}] {from}: {bodyNode.InnerText}");
			}
			else
			{
			}
		}

		public Request SendMessage(JabberID groupID, string message)
		{
			Contract.Requires(groupID.IsGroup);

			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<message type=\"groupchat\" to=\"{groupID}\" id=\"{request.ID}\" cts=\"{timestamp}\">");
			packet.Append($"<body>{message}</body>");
			packet.Append($"<kik push=\"true\" qos=\"true\" timestamp=\"{timestamp}\" />");
			packet.Append($"<request xmlns=\"kik:message:receipt\" r=\"true\" d=\"true\" />");
			packet.Append($"</message>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request RemoveFriend(JabberID friendID)
		{
			Contract.Requires(friendID.IsUser);

			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:iq:friend\">");
			packet.Append($"<remove jid=\"{friendID}\" />");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request PromoteToAdmin(JabberID group, JabberID member)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:groups:admin\">");
			packet.Append($"<g jid=\"{group}\">");
			packet.Append($"<m a=\"1\">{member}</m>");
			packet.Append($"</g>");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request DemoteToUser(JabberID group, JabberID member)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:groups:admin\">");
			packet.Append($"<g jid=\"{group}\">");
			packet.Append($"<m a=\"0\">{member}</m>");
			packet.Append($"</g>");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request AddToGroup(JabberID group, JabberID member)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:groups:admin\">");
			packet.Append($"<g jid=\"{group}\">");
			packet.Append($"<m>{member}</m>");
			packet.Append($"</g>");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request RemoveFromGroup(JabberID group, JabberID member)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:groups:admin\">");
			packet.Append($"<g jid=\"{group}\">");
			packet.Append($"<m r=\"1\">{member}</m>");
			packet.Append($"</g>");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request BanFromGroup(JabberID group, JabberID member)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:groups:admin\">");
			packet.Append($"<g jid=\"{group}\">");
			packet.Append($"<b>{member}</b>");
			packet.Append($"</g>");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request UnbanFromGroup(JabberID group, JabberID member)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:groups:admin\">");
			packet.Append($"<g jid=\"{group}\">");
			packet.Append($"<b r=\"1\">{member}</b>");
			packet.Append($"</g>");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request AddFriend(JabberID user)
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"set\" id=\"{request.ID}\">");
			packet.Append($"<query xmlns=\"kik:iq:friend\">");
			packet.Append($"<add jid=\"{user}\" />");
			packet.Append($"</query>");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}

		public Request GetRoster()
		{
			var request = new Request();
			var timestamp = request.Created.ToUnixMillisecondsTimestamp();

			var packet = new StringBuilder();
			packet.Append($"<iq type=\"get\" id=\"{request.ID}\">");
			packet.Append($"<query p=\"8\" xmlns=\"jabber:iq:roster\" />");
			packet.Append($"</iq>");

			_pump.AddHandler(request.ID, request.Answer);
			_conn.Write(packet.ToString());

			return request;
		}
	}
}