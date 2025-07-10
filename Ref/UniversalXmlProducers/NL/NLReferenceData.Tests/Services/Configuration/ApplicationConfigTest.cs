using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class ApplicationConfigTest
	{
		[Test]
		public void OutputPath()
		{
			Assert.That(ApplicationConfig.OutputPath, Is.EqualTo(@"..\..\UXmlFiles\"));
		}

		[Test]
		public void DownloadUrlCodeBook()
		{
			Assert.That(ApplicationConfig.DownloadUrlCodeBook, Is.EqualTo(@"https://www.belastingdienst.nl/codeboek_sagitta/huidig/xml/onderdeel-codeboek%2C%20onderdeel%20aangiftebehandeling.zip"));
		}

		[Test]
		public void DownloadUrlFiscalExchangeRates()
		{
			Assert.That(ApplicationConfig.DownloadUrlFiscalExchangeRates, Is.EqualTo(@"https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"));
		}

		[Test]
		public void AdditionalInformationTableNumber()
		{
			Assert.That(ApplicationConfig.AdditionalInformationTableNumber, Is.EqualTo("239"));
		}

		[Test]
		public void DownloadUrlAdditionalSupplements()
		{
			Assert.That(ApplicationConfig.DownloadUrlAdditionalSupplements, Is.EqualTo(@"https://tarief.douane.nl/ite-tariff-public/#/taric/moreinfo/codelists/search?sd=[SearchDate]&ct=ADD&ql=nl&l=nl"));
		}

		[Test]
		public void DownloadUrlTariff()
		{
			Assert.That(ApplicationConfig.DownloadUrlTariff, Is.EqualTo(@"https://download.belastingdienst.nl/douane_sw/tariff/download_bestanden.xml"));
		}

		[Test]
		public void TariffOutputDirectory()
		{
			Assert.That(ApplicationConfig.TariffOutputDirectory, Is.EqualTo(@"..\..\NLTariffData\"));
		}

		[Test]
		public void DownloadUrlCodeBookDwuAangifteBehandeling()
		{
			Assert.That(ApplicationConfig.DownloadUrlCodeBookDwuAangifteBehandeling, Is.EqualTo(@"https://www.belastingdienst.nl/codeboek_sagitta/huidig/xml/onderdeel-codeboek%2C%20onderdeel%20dwu%20aangiftebehandeling.zip"));
		}

		public void SupportingDocumentTableNumber()
		{
			Assert.That(ApplicationConfig.SupportingDocumentTableNumber, Is.EqualTo("213"));
		}

		public void TransportDocumentTableNumber()
		{
			Assert.That(ApplicationConfig.TransportDocumentTableNumber, Is.EqualTo("754"));
		}

		public void ServiceDir()
		{
			Assert.That(ApplicationConfig.ServiceDir, Is.EqualTo("..\\..\\UniversalXMLProducers\\NL\\"));
		}
	}
}
