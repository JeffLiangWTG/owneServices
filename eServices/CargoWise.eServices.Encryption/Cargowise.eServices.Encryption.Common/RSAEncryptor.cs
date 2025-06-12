using System;
using System.Security.Cryptography;

namespace CargoWise.eServices.Encryption.Common
{
	public class RSAEncryptor
	{
		#region Member Variables

		readonly string publicKeyXml;

		#endregion

		#region Constructor

		public RSAEncryptor(string rsaCryptoServicePublicKeyXml)
		{
			publicKeyXml = rsaCryptoServicePublicKeyXml;
		}

		#endregion

		#region Methods

		public string Encrypt(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentNullException(nameof(text));
			}

			var data = Constants.Converter.GetBytes(text);

			try
			{
				var encryptedData = EncryptBinary(data);
				var encryptedText = Convert.ToBase64String(encryptedData);

				return encryptedText;
			}
			catch(Exception exception)
			{
				var message =
					$"The specified text: '{text}' contains ({data.Length}) bytes. That is not supported since it has exceeded the maximum allowed size ({Constants.MaxLengthBinary}) in binary format.\r\nError: {exception.Message}";

				throw new NotSupportedException(message);
			}
		}

		public byte[] EncryptBinary(byte[] data)
		{
			return Encrypt(data, Constants.KeySize, Constants.WithOAEPPadding);
		}

		#endregion

		#region Helpers

		byte[] Encrypt(byte[] data, int keySize, bool withOAEPPadding)
		{
			using (var rsa = new RSACryptoServiceProvider(keySize))
			{
				rsa.FromXmlString(publicKeyXml);
				return rsa.Encrypt(data, withOAEPPadding);
			}
		}

		#endregion
	}
}
