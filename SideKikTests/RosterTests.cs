using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SideKik.Tests
{
	[TestClass()]
	public class RosterTests
	{
		[TestMethod()]
		public void Read()
		{
			using (XmlReader reader = XmlReader.Create("xml/Roster.xml", new XmlReaderSettings
			{
				IgnoreComments = true,
				IgnoreWhitespace = true,
				CloseInput = true,
			}))
			{
				reader.Read();
				reader.Read();
				var result = Roster.Read(reader);
				Assert.IsNotNull(result);
			}
		}
	}
}
