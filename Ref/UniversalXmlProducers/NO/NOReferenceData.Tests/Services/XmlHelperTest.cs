using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tests
{
	sealed class XmlHelperTest
	{
		[Test]
		public void TestValidXml()
		{
			var validExchangeRateXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ExchangeRates.Testfiles.Input.valutakurs.xml");
			Assert.That(XmlHelper.ValidateXml(validExchangeRateXml, EmbeddedSchemaResources.ExchangeRateSchema), Is.Empty);
		}

		[Test]
		public void TestInvalidXml()
		{
			var invalidExchangeRateXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.valutakurs_notValid.xml");
			var validationError = XmlHelper.ValidateXml(invalidExchangeRateXml, EmbeddedSchemaResources.ExchangeRateSchema);
			Assert.That(validationError, Is.EqualTo("The element 'omregningskurser' has invalid child element 'omregningskurs_ZZ'. List of possible elements expected: 'omregningskurs'."));
		}
	}
}
