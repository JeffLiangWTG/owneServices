using System.Security.Cryptography;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util
{
	static class SecretKeyGenerator
	{
		public static byte[] GenerateSecretKey(byte[] key, int keyLengthInBits, byte[] salt)
		{
			const int iterations = 100;

			using (var rfc2898 = new WTG.Foundation.Cryptography.Algorithms.Rfc2898DeriveBytes(key, salt, iterations, HashAlgorithmName.SHA1))
			{
				return rfc2898.GetBytes(keyLengthInBits / 8);
			}
		}
	}
}
