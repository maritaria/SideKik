using Microsoft.VisualStudio.TestTools.UnitTesting;
using SideKik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideKik.Tests
{
	[TestClass()]
	public class JabberIDTests
	{
		[TestMethod()]
		public void Parse_WithoutResource()
		{
			JabberID jid = JabberID.Parse("abc@def");

			Assert.IsNotNull(jid);
			Assert.AreEqual("abc", jid.Node);
			Assert.AreEqual("def", jid.Domain);
			Assert.IsNull(jid.Resource);
		}

		[TestMethod()]
		public void Parse_WithResource()
		{
			JabberID jid = JabberID.Parse("abc@def/ghi");

			Assert.IsNotNull(jid);
			Assert.AreEqual("abc", jid.Node);
			Assert.AreEqual("def", jid.Domain);
			Assert.AreEqual("ghi", jid.Resource);
		}
	}
}