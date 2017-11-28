using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace SideKik
{
	public static class Encryption
	{
		[Pure]
		public static byte[] Sha1(byte[] data)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Ensures(Contract.Result<byte[]>().Length == 20);
			return SHA1.Create().ComputeHash(data);
		}

		[Pure]
		public static byte[] Sha256(byte[] data)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Ensures(Contract.Result<byte[]>().Length > 0);
			return SHA256.Create().ComputeHash(data);
		}

		[Pure]
		public static byte[] Md5(byte[] data)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Ensures(Contract.Result<byte[]>().Length > 0);
			return MD5.Create().ComputeHash(data);
		}

		[Pure]
		public static string HashPassword(string username, string password)
		{
			Contract.Requires<ArgumentException>(Validation.IsValidUsername(username));
			Contract.Requires<ArgumentNullException>(password != null);
			Contract.Ensures(Contract.Result<string>().Length == 32);
			var sha1Password = Hexify(Sha1(Encoding.UTF8.GetBytes(password)));
			var salt = Encoding.UTF8.GetBytes(username.ToLower() + "niCRwL7isZHny24qgLvy");
			var hasher = new Rfc2898DeriveBytes(sha1Password, salt, 8192);
			var key = hasher.GetBytes(16);
			string result = Hexify(key);
			return result;
		}

		[Pure]
		public static string Hexify(byte[] bytes)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Ensures(Contract.Result<string>().Length == bytes.Length * 2);
			StringBuilder hex = new StringBuilder(bytes.Length * 2);
			foreach (byte b in bytes)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		[Pure]
		public static byte[] Unhexify(string hex)
		{
			int NumberChars = hex.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}

		[Pure]
		public static string HashWithHMAC(EnvironmentProfile profile, string data)
		{
			Contract.Requires(profile != null);
			Contract.Requires(data != null);
			var hmacAlgo = HMAC.Create();
			hmacAlgo.Key = CreateHmacKey(profile);
			byte[] hmacHash = hmacAlgo.ComputeHash(Encoding.UTF8.GetBytes(data));
			string result = Hexify(hmacHash);
			return result;
		}

		[Pure]
		public static byte[] CreateHmacKey(EnvironmentProfile profile)
		{
			Contract.Requires(profile != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);
			byte[] versionBytes = Encoding.UTF8.GetBytes(profile.Version);
			byte[] apkSignature = Unhexify(
				"308203843082026CA00302010202044C23D625300D06092A864886F70D0101050500308183310B3009060355" +
				"0406130243413110300E060355040813074F6E746172696F3111300F0603550407130857617465726C6F6F31" +
				"1D301B060355040A13144B696B20496E74657261637469766520496E632E311B3019060355040B13124D6F62" +
				"696C6520446576656C6F706D656E74311330110603550403130A43687269732042657374301E170D31303036" +
				"32343232303331375A170D3337313130393232303331375A308183310B30090603550406130243413110300E" +
				"060355040813074F6E746172696F3111300F0603550407130857617465726C6F6F311D301B060355040A1314" +
				"4B696B20496E74657261637469766520496E632E311B3019060355040B13124D6F62696C6520446576656C6F" +
				"706D656E74311330110603550403130A4368726973204265737430820122300D06092A864886F70D01010105" +
				"000382010F003082010A0282010100E2B94E5561E9A2378B657E66507809FB8E58D9FBDC35AD2A2381B8D4B5" +
				"1FCF50360482ECB31677BD95054FAAEC864D60E233BFE6B4C76032E5540E5BC195EBF5FF9EDFE3D99DAE8CA9" +
				"A5266F36404E8A9FCDF2B09605B089159A0FFD4046EC71AA11C7639E2AE0D5C3E1C2BA8C2160AFA30EC8A0CE" +
				"4A7764F28B9AE1AD3C867D128B9EAF02EF0BF60E2992E75A0D4C2664DA99AC230624B30CEA3788B23F5ABB61" +
				"173DB476F0A7CF26160B8C51DE0970C63279A6BF5DEF116A7009CA60E8A95F46759DD01D91EFCC670A467166" +
				"A9D6285F63F8626E87FBE83A03DA7044ACDD826B962C26E627AB1105925C74FEB77743C13DDD29B55B31083F" +
				"5CF38FC29242390203010001300D06092A864886F70D010105050003820101009F89DD384926764854A4A641" +
				"3BA98138CCE5AD96BF1F4830602CE84FEADD19C15BAD83130B65DC4A3B7C8DE8968ACA5CDF89200D6ACF2E75" +
				"30546A0EE2BCF19F67340BE8A73777836728846FAD7F31A3C4EEAD16081BED288BB0F0FDC735880EBD8634C9" +
				"FCA3A6C505CEA355BD91502226E1778E96B0C67D6A3C3F79DE6F594429F2B6A03591C0A01C3F14BB6FF56D75" +
				"15BB2F38F64A00FF07834ED3A06D70C38FC18004F85CAB3C937D3F94B366E2552558929B98D088CF1C45CDC0" +
				"340755E4305698A7067F696F4ECFCEEAFBD720787537199BCAC674DAB54643359BAD3E229D588E324941941E" +
				"0270C355DC38F9560469B452C36560AD5AB9619B6EB33705");

			byte[] classesDexSha1Digest = Encoding.UTF8.GetBytes("aCDhFLsmALSyhwi007tvowZkUd0=");

			var sourceBytes = Encoding.UTF8.GetBytes("hello")
				.Concat(apkSignature)
				.Concat(versionBytes)
				.Concat(classesDexSha1Digest)
				.Concat(Encoding.UTF8.GetBytes("bar")).ToArray();

			var bytes = SHA1.Create().ComputeHash(sourceBytes);
			string base64 = Convert.ToBase64String(bytes);
			return Encoding.UTF8.GetBytes(base64);
		}

		[Pure]
		public static string RsaSign(EnvironmentProfile profile, string data)
		{
			Contract.Requires(profile != null);
			Contract.Requires(data != null);
			var signatureSource = Encoding.UTF8.GetBytes(data);
			using (var reader = new StringReader(profile.EncryptionPrivateKeyPem))
			{
				AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
				ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");
				sig.Init(true, keyPair.Private);
				sig.BlockUpdate(signatureSource, 0, signatureSource.Length);
				byte[] signatureBytes = sig.GenerateSignature();
				string signature = Convert.ToBase64String(signatureBytes).Replace("+", "-").Replace("/", "_");
				return signature;
			}
		}

		public static Guid ConvertToKikGuid(Guid original)
		{
			byte[] bytes_raw = original.ToByteArray();
			byte[] bytes = RemapGuidBytes(bytes_raw);

			ulong most_significant_bits = BitConverter.ToUInt64(bytes.Take(8).Reverse().ToArray(), 0);
			ulong least_significant_bits = BitConverter.ToUInt64(bytes.Skip(8).Take(8).Reverse().ToArray(), 0);

			uint i8 = (uint)((0xF000000000000000 & most_significant_bits) >> 62);
			int i2, i3;
			switch (i8)
			{
				case 0: i3 = 3; i2 = 6; break;
				case 1: i3 = 2; i2 = 5; break;
				case 2: i3 = 7; i2 = 1; break;
				case 3: i3 = 9; i2 = 5; break;
				default: throw new NotSupportedException();
			}

			var j1 = ((0xFFFFFFFFFF000000 & most_significant_bits) >> 22);
			var j2 = ((0xFF0000 & most_significant_bits) >> 16);
			var j3 = (j1 ^ j2);
			var j4 = ((65280 & most_significant_bits) >> 8);
			ulong j = j3 ^ j4;

			var q1 = (GetBit(most_significant_bits, i2) + 1);
			var q2 = (GetBit(most_significant_bits, i3) << 1);
			int i5 = (int)(q1 | q2);

			int i = 1;
			for (int i4 = 0; i4 < 6; i4++)
			{
				i = (i + (i5 * 7)) % 60;

				int i_inc2 = i + 2;
				ulong i_inc2_shifted = ((ulong)1 << i_inc2);
				ulong i_inv = (i_inc2_shifted ^ ulong.MaxValue);
				least_significant_bits = (least_significant_bits & i_inv) | ((GetBit(j, i4)) << i_inc2);
			}

			byte[] newLowerBytes = BitConverter.GetBytes(least_significant_bits).Reverse().ToArray();
			Array.Copy(newLowerBytes, 0, bytes, 8, 8);
			byte[] output_bytes = RemapGuidBytes(bytes);
			return new Guid(output_bytes);
		}

		public static Guid CreateRequestGuid()
		{
			return ConvertToKikGuid(Guid.NewGuid());
		}

		private static byte[] RemapGuidBytes(byte[] bytes_raw)
		{
			return new byte[] {
				bytes_raw[3], bytes_raw[2], bytes_raw[1], bytes_raw[0],
				bytes_raw[5],bytes_raw[4],
				bytes_raw[7],bytes_raw[6],
				bytes_raw[8],bytes_raw[9],bytes_raw[10],bytes_raw[11],
				bytes_raw[12],bytes_raw[13],bytes_raw[14],bytes_raw[15],
			};
		}

		private static ulong GetBit(ulong j, int i)
		{
			ulong mask = (ulong)(1 << i);
			if (i > 32)
				return ((j >> 32) & mask) >> i;
			else
				return (mask & j) >> i;
		}

		public static List<KeyValuePair<string, string>> ToKikHashMap(Dictionary<string, string> source)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(source);
			List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
			List<string> keys = new List<string>(source.Keys);
			keys.Sort();
			for (int i = 0; i < source.Count; i++)
			{
				int hashCode = HashKikMap(dictionary, -1964139357, 7);
				hashCode = hashCode % dictionary.Count;
				if (hashCode < 0)
				{
					hashCode += dictionary.Count;
				}
				string selectedKey = keys[hashCode];
				keys.RemoveAt(hashCode);
				result.Add(new KeyValuePair<string, string>(selectedKey, dictionary[selectedKey]));
				dictionary.Remove(selectedKey);
			}

			return result;

			throw new NotImplementedException();
		}

		internal static int HashKikMap(Dictionary<string, string> source, int hashCodeBase, int hashCodeOffset)
		{
			List<string> keys = new List<string>(source.Keys);
			keys.Sort();
			string dictionaryForward = "";
			foreach (string key in keys)
				dictionaryForward += key + source[key];
			string dictionaryBackward = "";
			foreach (string key in keys.Reverse<string>())
			{
				dictionaryBackward += key + source[key];
			}

			byte[] bytesForward = Encoding.UTF8.GetBytes(dictionaryForward);
			byte[] bytesBackward = Encoding.UTF8.GetBytes(dictionaryBackward);

			int hash0 = CompressKikMapHash(Sha256(bytesForward));
			int hash1 = CompressKikMapHash(Sha1(bytesForward));
			int hash2 = CompressKikMapHash(Md5(bytesForward));
			int hash3 = CompressKikMapHash(Sha256(bytesBackward));
			int hash4 = CompressKikMapHash(Sha1(bytesBackward));
			int hash5 = CompressKikMapHash(Md5(bytesBackward));

			return (((hashCodeBase ^ (hash0 << hashCodeOffset)) ^ (hash5 << (hashCodeOffset * 2))) ^ (hash1 << hashCodeOffset)) ^ hash0;
		}

		internal static int CompressKikMapHash(byte[] digest)
		{
			long j = 0;
			for (int i = 0; i < digest.Length; i += 4)
			{
				long b4 = ByteToSignedInt(digest[i + 3]) << 24;
				long b3 = ByteToSignedInt(digest[i + 2]) << 16;
				long b2 = ByteToSignedInt(digest[i + 1]) << 8;
				long b1 = ByteToSignedInt(digest[i + 0]) << 0;
				long modifier = b4 | b3 | b2 | b1;
				j ^= modifier;
			}
			return (int)j;
		}

		private static int ByteToSignedInt(byte value)
		{
			if (value > 127)
				return (256 - value) * (-1);
			else
				return value;
		}

		public static long ToUnixTimestamp(this DateTime value)
		{
			return (long)(value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		}

		public static long ToUnixMillisecondsTimestamp(this DateTime value)
		{
			return (long)((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds * 1000);
		}

		public static long CurrentUnixTimestamp()
		{
			return DateTime.UtcNow.ToUnixTimestamp();
		}
	}
}