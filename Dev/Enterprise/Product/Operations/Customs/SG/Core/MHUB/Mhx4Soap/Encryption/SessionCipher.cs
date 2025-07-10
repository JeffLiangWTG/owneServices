using System.Security.Cryptography;
using Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption
{
	class SessionCipher
	{
		public byte[] Encrypt(byte[] bytes, byte[] encryptionKey, byte[] encryptionSalt)
		{
			using var cipher = GetCipher();
			var unlockKey = SecretKeyGenerator.GenerateSecretKey(encryptionKey, 256, encryptionSalt);
			cipher.Key = unlockKey;
			ICryptoTransform decryptTransform = cipher.CreateEncryptor();
			return decryptTransform.TransformFinalBlock(bytes, 0, bytes.Length);
		}

		public byte[] Decrypt(byte[] encryptedValue, byte[] encryptionKey, byte[] encryptionSalt)
		{
			using var cipher = GetCipher();
			var unlockKey = SecretKeyGenerator.GenerateSecretKey(encryptionKey, 256, encryptionSalt);
			cipher.Key = unlockKey;
			ICryptoTransform decryptTransform = cipher.CreateDecryptor();
			return decryptTransform.TransformFinalBlock(encryptedValue, 0, encryptedValue.Length);
		}

		Aes GetCipher()
		{
			var aes = Aes.Create();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.IV = new byte[16];

			return aes;
		}
	}
}
