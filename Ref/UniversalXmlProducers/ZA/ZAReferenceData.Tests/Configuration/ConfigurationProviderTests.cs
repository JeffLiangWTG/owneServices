using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Configuration
{
	[TestFixture]
	class ConfigurationProviderTests
	{
		[Test]
		public void CountryMatchingURI()
		{
			Assert.That(ConfigurationProvider.CountryMatchingURI, Is.EqualTo("https://refdbrepoupdate.wisecloud.zone/Staging/api/EntityMatcher/GetCodes?entityClass=COUNTRY&language=EN&useRecog=False"));
		}

		[Test]
		public void OutputFolder()
		{
			Assert.That(ConfigurationProvider.OutputFolder, Is.EqualTo(@"..\..\UXmlFiles\"));
		}

		[Test]
		public void RefDbServiceUri()
		{
			Assert.That(ConfigurationProvider.RefDbServiceURI, Is.EqualTo("http://localhost:37016/odata/"));
		}

		[Test]
		public void IsRefDbServiceSecure()
		{
			Assert.That(ConfigurationProvider.IsRefDbServiceSecure, Is.EqualTo(false));
		}

		[Test]
		public void InputFolder()
		{
			Assert.That(ConfigurationProvider.InputFolder, Is.EqualTo(@"..\..\UXmlFiles\ZA\"));
		}

		[Test]
		public void InputFile()
		{
			Assert.That(ConfigurationProvider.InputFile, Is.EqualTo(@"..\..\UXmlFiles\ZA\EDIFactMessage.txt"));
		}

		[Test]
		public void CarrierURLs()
		{
			Assert.That(ConfigurationProvider.CarrierBaseUrl, Is.EqualTo("https://tools.sars.gov.za/ACM_code_Tables/"));
		}
	}
}

