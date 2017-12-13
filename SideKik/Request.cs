using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SideKik.Messages.Incoming;

namespace SideKik
{
	public sealed class Request
	{
		public Guid ID { get; } = Encryption.CreateRequestGuid();
		public DateTime Created { get; } = DateTime.UtcNow;
		public RequestState State { get; private set; } = RequestState.Created;

		public async Task Answer(XmlNode answer)
		{
			State = RequestState.Answered;
			if (answer.Attributes["type"].Value != "error")
			{
				await OnAnswer?.Invoke(answer);
			}
			else
			{
				await OnError?.Invoke(answer);
			}
		}

		public event IncomingRouter.MessageHandler OnAnswer;
		public event IncomingRouter.MessageHandler OnError;
	}
}
