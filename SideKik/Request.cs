using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SideKik
{
	public sealed class Request
	{
		public Guid ID { get; } = Encryption.CreateRequestGuid();
		public DateTime Created { get; } = DateTime.UtcNow;
		public RequestState State { get; private set; } = RequestState.Created;

		public void Answer(XmlReader reader)
		{
			State = RequestState.Answered;
			if (reader.GetAttribute("type") != "error")
			{
				OnAnswer?.Invoke(reader);
			}
			else
			{
				OnError?.Invoke(reader);
			}
		}

		public event MessagePump.Callback OnAnswer;
		public event MessagePump.Callback OnError;
	}
}
