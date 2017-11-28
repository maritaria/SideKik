using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideKik
{
	public static class Validation
	{
		[Pure]
		public static bool IsValidUsername(string username)
		{
			Contract.Requires<ArgumentNullException>(username != null);
			Contract.Ensures(!Contract.Result<bool>() || username.Length >= 2);
			Contract.Ensures(!Contract.Result<bool>() || username.Length <= 32);

			return username.Length >= 2 && username.Length <= 32 && username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
		}
	}
}
