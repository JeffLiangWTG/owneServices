using System.Linq;
using Enterprise.Customs.SG.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Testing
{
	sealed class LoginCipherTest : TestCase
	{
		public void TestEncryption()
		{
			//encryption testing example - login
			var lCipher = new LoginCipher(new MHUBSettingsProvider().EncryptionKey);
			var initialData = new byte[] { 1, 2, 3, 4, 5 };
			//I pre-calculated the below value by running Encrypt once and noting down the resulting values, then putting them here
			var encoded_anticipated = new byte[] { 8, 255, 159, 17, 163, 20, 153, 161 };
			// now let's try to calculate the result again using the latest code (and see if smth changes)
			var encoded = lCipher.Encrypt(initialData);
			// success?
			Assert(encoded.SequenceEqual(encoded_anticipated));
			// let's try to decode now
			var decoded = lCipher.Decrypt(encoded_anticipated);
			// success?
			Assert(decoded.SequenceEqual(initialData));
		}
	}
}
