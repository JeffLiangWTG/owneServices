using System;
using System.Text;
using WTG.Foundation.Cryptography;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Helpers
{
	class OrgCarrierAccountMetaDataEncoder
	{
		static readonly byte[] vector = Encoding.ASCII.GetBytes("acaffc349fc97109");
		public static byte[] Encrypt(Guid metaDataPK, string ciphertext)
		{
			var keyBytes = ExtractIV(metaDataPK);
			var aes = new AESCryptographicProvider(keyBytes, vector);
			aes.Encrypt(Encoding.ASCII.GetBytes(ciphertext));
			return aes.Encrypt(Encoding.ASCII.GetBytes(ciphertext));
		}

		public static string Decrypt(Guid metaDataPK, byte[] encryptedData)
		{
			var keyBytes = ExtractIV(metaDataPK);
			var aes = new AESCryptographicProvider(keyBytes, vector);
			var decBytes = aes.Decrypt(encryptedData);
			var result = Encoding.ASCII.GetString(decBytes, 0, decBytes.Length);
			return result;
		}

		static byte[] ExtractIV(Guid iVPK)
		{
			var formattedIV = iVPK.ToString().Replace("-", string.Empty).Substring(5, 16);
			return Encoding.ASCII.GetBytes(formattedIV);
		}
	}
}
