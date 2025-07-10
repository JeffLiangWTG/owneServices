using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Testing
{
	sealed class SessionCipherTest : TestCase
	{
		public void TestEncryption()
		{
			var initialData = new byte[] { 1, 2, 3, 4, 5 };
			//encryption testing example - session
			var sCipher = new SessionCipher();
			var key = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
			var salt = new byte[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
			//I pre-calculated the below value by running Encrypt once and noting down the resulting values, then putting them here
			var encoded_anticipated = new byte[] { 255, 158, 85, 17, 151, 105, 23, 190, 84, 244, 236, 52, 178, 226, 67, 136 };
			var encoded = sCipher.Encrypt(initialData, key, salt);
			Assert(encoded.SequenceEqual(encoded_anticipated));
			// let's try to decode now
			var decoded = sCipher.Decrypt(encoded, key, salt);
			Assert(decoded.SequenceEqual(initialData));
		}
	}
}
