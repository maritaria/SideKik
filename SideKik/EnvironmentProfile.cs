using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace SideKik
{
	public sealed class EnvironmentProfile
	{
		public string EndpointHostname { get; private set; }
		public int EndpointPort { get; private set; }
		public string Version { get; private set; }
		public string EncryptionPrivateKeyPem { get; private set; }
		public string DeviceID { get; } = "167da12427ee4dc4a36b40e8debafc25";

		private EnvironmentProfile(string hostname, int port, string version, string encryptionPrivateKey)
		{
			Contract.Requires<ArgumentNullException>(hostname != null);
			Contract.Requires<ArgumentOutOfRangeException>(port > 0 && port <= 65335, "port must be in the valid port range (1-65335)");
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(encryptionPrivateKey != null);
			EndpointHostname = hostname;
			EndpointPort = port;
			Version = version;
			EncryptionPrivateKeyPem = encryptionPrivateKey;
		}

		[ContractInvariantMethod]
		private void Invariants()
		{
			Contract.Invariant(EndpointHostname != null);
			Contract.Invariant(EndpointPort >= 0);
			Contract.Invariant(EndpointPort <= 65335);
			Contract.Invariant(Version != null);
			Contract.Invariant(EncryptionPrivateKeyPem != null);
		}

		public static EnvironmentProfile Reliable { get; } = new EnvironmentProfile("talk11100an.kik.com", 5223, "11.1.1.12218",
			"-----BEGIN RSA PRIVATE KEY-----\nMIIBPAIBAAJBANEWUEINqV1KNG7Yie9GSM8t75ZvdTeqT7kOF40kvDHIp" +
				"/C3tX2bcNgLTnGFs8yA2m2p7hKoFLoxh64vZx5fZykCAwEAAQJAT" +
				"/hC1iC3iHDbQRIdH6E4M9WT72vN326Kc3MKWveT603sUAWFlaEa5T80GBiP/qXt9PaDoJWcdKHr7RqDq" +
				"+8noQIhAPh5haTSGu0MFs0YiLRLqirJWXa4QPm4W5nz5VGKXaKtAiEA12tpUlkyxJBuuKCykIQbiUXHEwzFYbMHK5E" +
				"/uGkFoe0CIQC6uYgHPqVhcm5IHqHM6/erQ7jpkLmzcCnWXgT87ABF2QIhAIzrfyKXp1ZfBY9R0H4pbboHI4uatySKc" +
				"Q5XHlAMo9qhAiEA43zuIMknJSGwa2zLt/3FmVnuCInD6Oun5dbcYnqraJo=\n-----END RSA PRIVATE KEY----- "
		);

		public static EnvironmentProfile Latest { get; } = new EnvironmentProfile("talk11330an.kik.com", 5223, "11.39.0.19149",
			/*
			"-----BEGIN RSA PRIVATE KEY-----\nMIIBVgIBADANBgkqhkiG9w0BAQEFAASCAUAwggE8AgEAAkEA0RZQQg2pXUo0btiJ" +
			"70ZIzy3vlm91N6pPuQ4XjSS8Mcin8Le1fZtw2AtOcYWzzIDabanuEqgUujGHri9n" +
			"Hl9nKQIDAQABAkBP+ELWILeIcNtBEh0foTgz1ZPva83fbopzcwpa95PrTexQBYWV" +
			"oRrlPzQYGI/+pe309oOglZx0oevtGoOr7yehAiEA+HmFpNIa7QwWzRiItEuqKslZ" +
			"drhA+bhbmfPlUYpdoq0CIQDXa2lSWTLEkG64oLKQhBuJRccTDMVhswcrkT+4aQWh" +
			"7QIhALq5iAc+pWFybkgeoczr96tDuOmQubNwKdZeBPzsAEXZAiEAjOt/IpenVl8F" +
			"j1HQfiltugcji5q3JIpxDlceUAyj2qECIQDjfO4gySclIbBrbMu3/cWZWe4IicPo" +
			"66fl1txieqtomg==\n-----END RSA PRIVATE KEY----- "
			//*/
			"-----BEGIN RSA PRIVATE KEY-----\nMIIBPAIBAAJBANEWUEINqV1KNG7Yie9GSM8t75ZvdTeqT7kOF40kvDHIp" +
				"/C3tX2bcNgLTnGFs8yA2m2p7hKoFLoxh64vZx5fZykCAwEAAQJAT" +
				"/hC1iC3iHDbQRIdH6E4M9WT72vN326Kc3MKWveT603sUAWFlaEa5T80GBiP/qXt9PaDoJWcdKHr7RqDq" +
				"+8noQIhAPh5haTSGu0MFs0YiLRLqirJWXa4QPm4W5nz5VGKXaKtAiEA12tpUlkyxJBuuKCykIQbiUXHEwzFYbMHK5E" +
				"/uGkFoe0CIQC6uYgHPqVhcm5IHqHM6/erQ7jpkLmzcCnWXgT87ABF2QIhAIzrfyKXp1ZfBY9R0H4pbboHI4uatySKc" +
				"Q5XHlAMo9qhAiEA43zuIMknJSGwa2zLt/3FmVnuCInD6Oun5dbcYnqraJo=\n-----END RSA PRIVATE KEY----- "
		);
	}
}