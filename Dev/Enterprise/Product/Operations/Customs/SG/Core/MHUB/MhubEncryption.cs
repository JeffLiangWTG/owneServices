using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.Customs.SG.V4.MHUB
{
	static class MhubEncryption
	{
		public static string EncryptData(string textToEncrypt, string encryptionKey)
		{
			string result;
			var dESBytes = Convert.FromBase64String(encryptionKey);
			ASCIIEncoding encoding = new ASCIIEncoding();
			byte[] bytesToEncode = encoding.GetBytes(textToEncrypt);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.SetLength(0);
				TripleDES tdes = TripleDES.Create();
				tdes.Mode = CipherMode.ECB;

				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, tdes.CreateEncryptor(dESBytes, null), CryptoStreamMode.Write))
				{
					cryptoStream.Write(bytesToEncode, 0, bytesToEncode.Length);
					cryptoStream.FlushFinalBlock();

					memoryStream.Position = 0;
					byte[] encryptedBytes = new Byte[memoryStream.Length];
					int offset = 0;
					while (offset < memoryStream.Length)
					{
						offset += memoryStream.Read(encryptedBytes, offset, (int)memoryStream.Length - offset);
					}
					result = System.Convert.ToBase64String(encryptedBytes);
				}
			}

			return result;
		}
	}
}
