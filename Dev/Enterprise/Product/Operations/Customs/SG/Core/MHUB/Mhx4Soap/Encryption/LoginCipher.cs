using System;
using System.Security.Cryptography;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption
{
	class LoginCipher : IDisposable
	{
		readonly TripleDES tdes;

		public LoginCipher(string encryptionKey)
		{
			tdes = TripleDES.Create();
			tdes.Key = Convert.FromBase64String(encryptionKey);
			tdes.Mode = CipherMode.ECB;
			tdes.Padding = PaddingMode.PKCS7;
		}

		public byte[] Encrypt(byte[] value)
		{
			var cTransform = tdes.CreateEncryptor();
			return cTransform.TransformFinalBlock(value, 0, value.Length);
		}

		public byte[] Decrypt(byte[] value)
		{
			ICryptoTransform cTransform = tdes.CreateDecryptor();
			return cTransform.TransformFinalBlock(value, 0, value.Length);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				tdes.Dispose();
			}
		}
	}
}
