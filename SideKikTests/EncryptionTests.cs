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
	public class EncryptionTests
	{
		[TestMethod()]
		public void ConvertToKikGuid()
		{
			Dictionary<Guid, Guid> tests = new Dictionary<Guid, Guid>();
			tests.Add(Guid.Parse("8a8b503c-daf6-4c9d-8a93-b87204c6c9ed"), Guid.Parse("8a8b503c-daf6-4c9d-8a93-b87204c6c9ad"));
			tests.Add(Guid.Parse("1c2a6e90-d1f4-4c07-b0d5-5dbb939f9754"), Guid.Parse("1c2a6e90-d1f4-4c07-b0d5-7dfb939d9354"));
			tests.Add(Guid.Parse("7833f812-d40b-44dd-9548-3cfc8120f1b5"), Guid.Parse("7833f812-d40b-44dd-9549-3cfc8920f1f5"));
			tests.Add(Guid.Parse("08ee45eb-7fa8-4131-a884-0985ace26bbd"), Guid.Parse("08ee45eb-7fa8-4131-a884-09c52de06fbd"));
			tests.Add(Guid.Parse("35564316-69f7-40b6-bb5d-6d100c030558"), Guid.Parse("35564316-69f7-40b6-bb5d-4d500c030558"));
			tests.Add(Guid.Parse("b84a7ee1-2f0f-4427-8e3e-e6d0399f5771"), Guid.Parse("b84a7ee1-2f0f-4427-863e-e6d0b99f7771"));
			tests.Add(Guid.Parse("ac4ffdcb-8067-49ef-a040-c57281b8b44b"), Guid.Parse("ac4ffdcb-8067-49ef-a840-e57289b8944b"));
			tests.Add(Guid.Parse("9010b2b8-cd77-4e20-aa9c-5355dc42d92d"), Guid.Parse("9010b2b8-cd77-4e20-aa9c-5315dd40dd2d"));
			tests.Add(Guid.Parse("828cec0d-8cf9-49c7-917f-ea65c4814741"), Guid.Parse("828cec0d-8cf9-49c7-917f-ca6544814741"));
			tests.Add(Guid.Parse("3818a750-6811-450b-94b5-d693796fb84d"), Guid.Parse("3818a750-6811-450b-94b5-d693706fba4d"));

			tests.Add(Guid.Parse("64663001-6678-49fc-9361-113320ee9601"), Guid.Parse("64663001-6678-49fc-9b61-113328ee9601"));
			tests.Add(Guid.Parse("40cd29ce-d561-4d5e-8b4a-dda57ebd3f4d"), Guid.Parse("40cd29ce-d561-4d5e-8b4b-dda57ebd3f0d"));
			tests.Add(Guid.Parse("683ba907-0750-49e7-8a3c-924b986e10e9"), Guid.Parse("683ba907-0750-49e7-823c-b24b106e10e9"));
			tests.Add(Guid.Parse("38610e6a-fe40-4f8f-9ab6-42d60b6a874a"), Guid.Parse("38610e6a-fe40-4f8f-9ab7-62d6036a874a"));
			tests.Add(Guid.Parse("9b437dd2-d614-4509-9a4f-e82742a86a83"), Guid.Parse("9b437dd2-d614-4509-9a4f-c827c2a86e83"));
			tests.Add(Guid.Parse("8fcf4c70-dd7d-46ba-a474-cd9210de441e"), Guid.Parse("8fcf4c70-dd7d-46ba-ac74-ed9290de441e"));
			tests.Add(Guid.Parse("0332a846-7434-406e-a5e3-56db60c437f3"), Guid.Parse("0332a846-7434-406e-a5e3-76db68c417f3"));
			tests.Add(Guid.Parse("5e6c56b1-5747-4bf2-b379-8300de4e19ca"), Guid.Parse("5e6c56b1-5747-4bf2-b379-8300564c39ca"));
			tests.Add(Guid.Parse("4d3621b1-9f8a-45f5-9628-ff01b5ae52fc"), Guid.Parse("4d3621b1-9f8a-45f5-9628-df01bdae72fc"));

			tests.Add(Guid.Parse("dcd5394e-7836-4517-864f-d33038358c17"), Guid.Parse("dcd5394e-7836-4517-864f-d37038378c17"));
			tests.Add(Guid.Parse("1475584c-3700-46f8-9bdd-7bb6cbc5b9b4"), Guid.Parse("1475584c-3700-46f8-9bdd-5bb6c3c5b9b4"));
			tests.Add(Guid.Parse("ca499f72-55e6-4fd2-9e79-d78d05d0f0e1"), Guid.Parse("ca499f72-55e6-4fd2-9e79-d78d0dd0f2e1"));
			tests.Add(Guid.Parse("510d69e8-c1d9-47ac-a80b-fda44317d769"), Guid.Parse("510d69e8-c1d9-47ac-a80b-dda4c315f769"));
			tests.Add(Guid.Parse("5a149042-be7a-45c1-8c1d-649922da90b0"), Guid.Parse("5a149042-be7a-45c1-8c1d-449923da94b0"));
			tests.Add(Guid.Parse("b73ac304-4dce-4013-9441-a05d496085e8"), Guid.Parse("b73ac304-4dce-4013-9c41-805dc960a5e8"));
			tests.Add(Guid.Parse("4bbf87bc-8ec9-4b46-adf0-23fab4c4629a"), Guid.Parse("4bbf87bc-8ec9-4b46-adf1-23fabcc4629a"));
			tests.Add(Guid.Parse("bfcd8818-4a4c-4a10-8f0b-3d5fd6a05df5"), Guid.Parse("bfcd8818-4a4c-4a10-8f0b-3d1fd7a259f5"));
			tests.Add(Guid.Parse("3785fdff-3885-4846-8a0f-4c0bf6098cd5"), Guid.Parse("3785fdff-3885-4846-8a0f-6c0b7e0b8cd5"));

			foreach (KeyValuePair<Guid, Guid> test in tests)
			{
				Assert.AreEqual<Guid>(test.Value, Encryption.ConvertToKikGuid(test.Key), "source: " + test.Key);
			}

		}

		[TestMethod()]
		public void CompressKikMapHash()
		{
			string source = "helloworld";
			byte[] bytes = Encryption.Sha256(Encoding.UTF8.GetBytes(source));

			Assert.AreEqual<int>(4493839, Encryption.CompressKikMapHash(bytes));
		}

		[TestMethod()]
		public void HashKikMap()
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			dict.Add("a", "b");
			dict.Add("c", "d");
			dict.Add("e", "f");
			dict.Add("g", "h");
			dict.Add("i", "j");
			dict.Add("k", "l");

			int hashcode = Encryption.HashKikMap(dict, -1964139357, 7);

			Assert.AreEqual(-651571844, hashcode);
		}

		[TestMethod()]
		public void ToKikHashMap()
		{
			/*
				p="8d1a05aef5c82d95aa88f9d94ed68b85"
				cv="eb42ddfbf562754350de5c5bb0085d477ff5ae5a"
				n="1"
				v="11.1.1.12218"
				conn="WIFI"
				to="talk.kik.com"
				lang="en_US"
				from="mari_bot_3gn@talk.kik.com/CAN167da12427ee4dc4a36b40e8debafc25"
				sid="1c2a6e90-d1f4-4c07-b0d5-7dfb939d9354"
				signed="sNVZWeszJorMN5_N9SZoUeeAOSJ8p9VRM86dzEr9pCTO65nB5j4Ca8yZiBvwzOGzUXfT5ffbGYtzXI1TGrmdBQ"
				ts="1496333389122"
			*/

			Dictionary<string, string> dict = new Dictionary<string, string>();
			dict.Add("lang", "en_US");
			dict.Add("from", "mari_bot_3gn@talk.kik.com/CAN167da12427ee4dc4a36b40e8debafc25");
			dict.Add("sid", "1c2a6e90-d1f4-4c07-b0d5-7dfb939d9354");
			dict.Add("signed", "sNVZWeszJorMN5_N9SZoUeeAOSJ8p9VRM86dzEr9pCTO65nB5j4Ca8yZiBvwzOGzUXfT5ffbGYtzXI1TGrmdBQ");
			dict.Add("ts", "1496333389122");

			dict.Add("p", "8d1a05aef5c82d95aa88f9d94ed68b85");
			dict.Add("cv", "eb42ddfbf562754350de5c5bb0085d477ff5ae5a");
			dict.Add("n", "1");
			dict.Add("v", "11.1.1.12218");
			dict.Add("conn", "WIFI");
			dict.Add("to", "talk.kik.com");

			List<string> expectedOrder = new List<string>();
			expectedOrder.Add("p");
			expectedOrder.Add("cv");
			expectedOrder.Add("n");
			expectedOrder.Add("v");
			expectedOrder.Add("conn");
			expectedOrder.Add("to");
			expectedOrder.Add("lang");
			expectedOrder.Add("from");
			expectedOrder.Add("sid");
			expectedOrder.Add("signed");
			expectedOrder.Add("ts");

			var output = Encryption.ToKikHashMap(dict);
			Assert.AreEqual(expectedOrder.Count, output.Count);
			for (int i = 0; i < output.Count; i++)
			{
				Assert.AreEqual(expectedOrder[i], output[i].Key);
			}
		}
	}
}