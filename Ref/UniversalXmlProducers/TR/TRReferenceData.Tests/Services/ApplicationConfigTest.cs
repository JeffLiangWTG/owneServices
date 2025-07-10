using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	sealed class ApplicationConfigTest
	{
		[Test]
		public void TestOutputPath()
		{
			Assert.That(ApplicationConfig.OutputPath, Is.EqualTo("..\\..\\UXmlFiles"));
		}

		[Test]
		public void TestExchangeRatesURL()
		{
			Assert.That(ApplicationConfig.ExchangeRatesURL, Is.EqualTo("https://www.tcmb.gov.tr/kurlar/today.xml"));
		}

		[Test]
		public void TestResPath()
		{
			Assert.That(ApplicationConfig.ResPath, Is.EqualTo("Res"));
		}

		[Test]
		public void TestRefDbServiceURI()
		{
			Assert.That(ApplicationConfig.RefDbServiceURI, Is.EqualTo("https://refdbrepoupdate-uat.wtg.zone/Update/odata/"));
		}

		[Test]
		public void TestIsRefDbServiceSecure()
		{
			Assert.That(ApplicationConfig.IsRefDbServiceSecure, Is.EqualTo("false"));
		}
	}
}
