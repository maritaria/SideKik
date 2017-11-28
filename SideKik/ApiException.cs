using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SideKik
{
	public class ApiException : Exception
	{
		public ApiException()
		{
		}

		public ApiException(string message) : base(message)
		{
		}

		public ApiException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
