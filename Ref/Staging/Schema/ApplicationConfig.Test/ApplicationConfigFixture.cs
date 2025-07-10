using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig.Test
{
	[TestFixture]
	class ApplicationConfigFixture
	{
		[Test]
		public void TestApplicationConfig()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.Staging.ApplicationConfig.Test.config.json");
			Assert.That(ApplicationConfig.CipherPublicKeyName, Is.EqualTo("cipher_public_for_test.key"));
			Assert.That(ApplicationConfig.CipherPrivateKeyName, Is.EqualTo("cipher_private_for_test.key"));
			Assert.That(ApplicationConfig.AesKeyName, Is.EqualTo("aes_key_for_test.bin"));
		}

	}
}
