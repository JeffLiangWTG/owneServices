using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Configuration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	public class ApplicationConfigTest
	{
		[Test]
		public void ConfigEnvironment()
		{
			ApplicationConfig.ConfigEnvironment();

			Assert.IsNotNull(ApplicationConfig.SafeDbServiceUrl);
			Assert.IsNotNull(ApplicationConfig.CustomsWebUrl);
			Assert.IsNotNull(ApplicationConfig.CustomsWebUrlPublicationDate);
			Assert.IsNotNull(ApplicationConfig.SafeDbServiceUrlRequireSecureConnection);
			Assert.IsNotNull(ApplicationConfig.ITCustomsWebUrlRequireSecureConnection);
			Assert.IsNotNull(ApplicationConfig.PartialUrdXmlStoragePath);
			Assert.IsNotNull(ApplicationConfig.UrdXmlStoragePath);
			Assert.IsNotNull(ApplicationConfig.BatchSize);
			Assert.IsNotNull(ApplicationConfig.LastSyncXmlFilePath);
		}
	}
}
