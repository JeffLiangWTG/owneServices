using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Services.Testing
{
	[TestFixture]
	sealed class ApplicationConfigCmdLineTest
	{
		[Test]
		public void DownloadDir()
		{
			Assert.That(ApplicationConfig.DownloadDir, Is.EqualTo("..\\..\\UxmlFiles"));
		}

		[Test]
		public void TariffDataDownloadUrl()
		{
			Assert.That(ApplicationConfig.TariffDataDownloadUrl, Is.EqualTo(@"https://eservices.minfin.fgov.be/extTariffBrowser/XmlExtractions?lang=EN&amp"));
		}

		[Test]
		public void CircabcDownloadUrlPrefix()
		{
			Assert.That(ApplicationConfig.CircabcDownloadUrlPrefix, Is.EqualTo(@"https://circabc.europa.eu/rest/download/"));
		}

		[Test]
		public void WebSiteUrl()
		{
			Assert.That(ApplicationConfig.WebSiteUrl, Is.EqualTo(@"https://eservices.minfin.fgov.be/extTariffBrowser/resourceHomePage.xhtml?fileID=exchange_rates.html&amp;lang=EN"));
		}

		[Test]
		public void ExportTariffsDownloadURL()
		{
			Assert.That(ApplicationConfig.ExportTariffsDownloadURL, Is.EqualTo(@"https://stammdaten.znet-group.com/DETariffDataExport.xml"));
		}

		[Test]
		public void ExportTariffsUXmlFile()
		{
			Assert.That(ApplicationConfig.ExportTariffsUXmlFile, Is.EqualTo("EUNExportTariffs.xml"));
		}

		[Test]
		public void EUTariffBaseUrl()
		{
			Assert.That(ApplicationConfig.EUTariffBaseUrl, Is.EqualTo("https://circabc.europa.eu/ui/group/0e5f18c2-4b2f-42e9-aed4-dfe50ae1263b/library/ac2dee97-426f-4d5f-a63a-7d8760f29513"));
		}

		[Test]
		public void EUNomenclatureBaseUrl()
		{
			Assert.That(ApplicationConfig.EUNomenclatureBaseUrl, Is.EqualTo(@"https://circabc.europa.eu/ui/group/0e5f18c2-4b2f-42e9-aed4-dfe50ae1263b/library/6e7dc94f-70e4-44ed-9560-b9794979cb87"));
		}

		[Test]
		public void EUReferenceDataBaseUrl()
		{
			Assert.That(ApplicationConfig.EUReferenceDataBaseUrl, Is.EqualTo(@"https://circabc.europa.eu/ui/group/0e5f18c2-4b2f-42e9-aed4-dfe50ae1263b/library/18196799-4d76-46c1-8de4-bfab85abd76c"));
		}

		[Test]
		public void SectionDetailsUrl()
		{
			Assert.That(ApplicationConfig.SectionDetailsUrl, Is.EqualTo(@"http://ec.europa.eu/taxation_customs/dds2/taric/taric_consultation.jsp"));
		}

		[Test]
		public void DownloadsFolder()
		{
			Assert.That(ApplicationConfig.DownloadsFolder, Is.EqualTo("..\\..\\UxmlFiles\\Downloads_EUNTariffDataProducer"));
		}

		[Test]
		public void NomenclatureUXmlFile()
		{
			Assert.That(ApplicationConfig.NomenclatureUXmlFile, Is.EqualTo("..\\..\\UXmlFiles\\EUNNomenclature.xml"));
		}

		[Test]
		public void DownloadTimeoutInSeconds()
		{
			Assert.That(ApplicationConfig.DownloadTimeoutInSeconds, Is.EqualTo("30"));
		}

		[Test]
		public void SeleniumServerURL()
		{
			Assert.That(ApplicationConfig.SeleniumServerURL, Is.EqualTo(@"http://127.0.0.1:4444/wd/hub"));
		}

		[Test]
		public void SeleniumTimeOutInSeconds()
		{
			Assert.That(ApplicationConfig.SeleniumTimeOutInSeconds, Is.EqualTo("150"));
		}

		[Test]
		public void NctsCodesDownloadUrl()
		{
			Assert.That(ApplicationConfig.NctsCodesDownloadUrl, Is.EqualTo(@"https://financien.belgium.be/sites/default/files/Helpdesk_PLDA/NCTS-RW/P5/codelijsten_NCTSP5.zip"));
		}

		[Test]
		public void UCCCodeListDownloadUrl()
		{
			Assert.That(ApplicationConfig.UCCCodeListDownloadUrl, Is.EqualTo(@"https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_{Domain}_{CodeListType}.zip"));
		}

		[Test]
		public void WaitPageLoadingInSeconds()
		{
			Assert.That(ApplicationConfig.WaitPageLoadingInSeconds, Is.EqualTo("10"));
		}

		[Test]
		public void ClientSettingsProviderServiceUri()
		{
			Assert.That(ApplicationConfig.ClientSettingsProviderServiceUri, Is.EqualTo(""));
		}

		[Test]
		public void ServiceDir()
		{
			Assert.That(ApplicationConfig.ServiceDir, Is.EqualTo("..\\..\\UniversalXMLProducers\\BE\\"));
		}

		[Test]
		public void LoadInterval()
		{
			Assert.That(ApplicationConfig.LoadInterval, Is.EqualTo(10));
		}

		[Test]
		public void DownloadIntervalDaily()
		{
			Assert.That(ApplicationConfig.DownloadIntervalDaily, Is.EqualTo(1));
		}

		[Test]
		public void DownloadIntervalMonthly()
		{
			Assert.That(ApplicationConfig.DownloadIntervalMonthly, Is.EqualTo(450));
		}
	}
}
