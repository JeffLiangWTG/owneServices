using System;
using System.Security.Cryptography;

namespace CargoWise.eServices.Encryption.Common
{
	public class RSADecryptor
	{
		#region Member Variables

		readonly string keyXml;

		#endregion

		#region Constructor

		public RSADecryptor(string rsaCryptoServiceKeyXml)
		{
			keyXml = rsaCryptoServiceKeyXml;
		}

		#endregion

		#region Methods

		public string Decrypt(string text)
		{
			var decryptedData = DecryptBinary(Convert.FromBase64String(text));
			var decryptedText = Constants.Converter.GetString(decryptedData);

			return decryptedText;
		}

		public byte[] DecryptBinary(byte[] data)
		{
			return Decrypt(data, Constants.KeySize, Constants.WithOAEPPadding);
		}

		#endregion

		#region Helpers

		public byte[] Decrypt(byte[] data, int keySize, bool withOAEPPadding)
		{
			using (var rsa = new RSACryptoServiceProvider(keySize))
			{
				rsa.FromXmlString(keyXml);
				return rsa.Decrypt(data, withOAEPPadding);
			}
		}

		#endregion
	}
}
