using System;
using System.IO;
using System.Security.Cryptography;

namespace Enterprise.MasterFiles.Services.KeysGenerator
{
	class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is not part of enterprise")]
		static void Main(string[] args)
		{
			RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();

			//provide public and private RSA params
			string privateName = string.Format("privatekey_{0}.xml", DateTime.Now.ToString("ddMMyyyyHHmmss"));

			using (StreamWriter writer = new StreamWriter(privateName))
			{
				string publicPrivateKeyXML = rsaProvider.ToXmlString(true);
				writer.Write(publicPrivateKeyXML);
			}

			//provide public only RSA params
			string publicName = string.Format("publickey_{0}.xml", DateTime.Now.ToString("ddMMyyyyHHmmss"));

			using (StreamWriter writer = new StreamWriter(publicName))
			{
				string publicOnlyKeyXML = rsaProvider.ToXmlString(false);
				writer.Write(publicOnlyKeyXML);
			}

			Console.WriteLine("Files created.");
			Console.WriteLine(privateName);
			Console.WriteLine(publicName);
			Console.ReadLine();
		}
	}
}
