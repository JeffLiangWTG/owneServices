using NUnit.Framework;
using System;
using CargoWise.eServices.Encryption.Server.Encryptor;

namespace CargoWise.eServices.Encryption.Client.Encryptor.Tests
{
	[TestFixture]
	public class EhubServerEncryptorFixtures
	{
		int maxAllowedAsciiLength = 86;

		[Test]
		public void TestEncrypt_Null_ArgumentNullException()
		{
			Assert.That(() => EhubServerEncryptor.Encrypt(null), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void TestEncrypt_EmptyString_ArgumentNullException()
		{
			Assert.That(() => EhubServerEncryptor.Encrypt(string.Empty), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void TestEncrypt_OversizedString_NotSupportedException()
		{
			Assert.That(() => EhubServerEncryptor.Encrypt(new string('x', maxAllowedAsciiLength + 1)), Throws.TypeOf<NotSupportedException>());
		}

		[Test]
		public void TestEncrypt_LegalText_Successful()
		{
			for (var i = 1; i <= maxAllowedAsciiLength; i++)
			{
				var text = new string('x', i);
				var encrypted = EhubServerEncryptor.Encrypt(text);

				Assert.IsFalse(string.IsNullOrEmpty(encrypted));
				Assert.AreEqual(172, encrypted.Length);
			}
		}
	}
}
