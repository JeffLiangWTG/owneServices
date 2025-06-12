using System;
using System.IO;
using System.Security.Cryptography;

namespace RSAKeyGen
{
	class Program
	{
		static void Main(string[] args)
		{
			var exePath = AppDomain.CurrentDomain.BaseDirectory;
			var publicKeyFile = Path.Combine(exePath, "RSA-public-key.xml");
			var privateKeyFile = Path.Combine(exePath, "RSA-private-key.xml");

			const int RSAType = 1;
			var cryptoServiceProvider = new RSACryptoServiceProvider(new CspParameters(RSAType));

			SaveToKeyFile(cryptoServiceProvider, publicKeyFile);
			SaveToKeyFile(cryptoServiceProvider, privateKeyFile, true);

			cryptoServiceProvider.Clear();
		}

		static void SaveToKeyFile(RSACryptoServiceProvider cryptoServiceProvider,  string filePath, bool includePrivateParameters = false)
		{
			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				using (var sw = new StreamWriter(fs))
				{
					sw.Write(cryptoServiceProvider.ToXmlString(includePrivateParameters));
					sw.Flush();
				}
			}
		}
	}
}
