using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SideKik.Marieke
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.Error.WriteLine("Expected arguments: <username> <password>");
				Environment.ExitCode = 1;
				return;
			}

			string username = args[0];
			string password = args[1];

			Console.WriteLine("KikBot starting");
			var kik = new Kik(username, password);
			kik.Run();
			Console.WriteLine("KikBot started");
		}
	}
}