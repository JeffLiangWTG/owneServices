using CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefAccessorialDataProducer.Test
{
	public class AppConfigurationProviderTest
	{
		[Test]
		public void TestAppConfiguration()
		{
			Assert.That(AppConfigurationProvider.AppConfiguration, Is.Not.Null);
			Assert.That(AppConfigurationProvider.AppConfiguration.OutputPath, Is.EqualTo("..\\..\\UXmlFiles\\"));
			Assert.That(AppConfigurationProvider.AppConfiguration.WebServiceBaseUrl, Is.EqualTo("https://cmb.wisegrid.net/"));
			Assert.That(AppConfigurationProvider.AppConfiguration.TenantId, Is.EqualTo("1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1"));
			Assert.That(AppConfigurationProvider.AppConfiguration.ClientId, Is.EqualTo("5afccf59-7230-4f0b-9a90-a6b0f2cb8549"));
			Assert.That(AppConfigurationProvider.AppConfiguration.ServiceId, Is.EqualTo("218c5cfe-1ce3-4205-92f0-fde1805d659d"));
			Assert.That(AppConfigurationProvider.AppConfiguration.PrivateKeyFileName, Is.EqualTo("PrivateKey.key"));
			Assert.That(AppConfigurationProvider.AppConfiguration.CertificateFileName, Is.EqualTo("Certificate.cer"));
		}
	}
}
