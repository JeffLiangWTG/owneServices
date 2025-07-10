using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.Testing
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
		public void TestCustomsBaseURL()
		{
			Assert.That(ApplicationConfig.CustomsBaseURL, Is.EqualTo("http://www.zoll.de/"));
		}

		[Test]
		public void TestImportCodeListsDownloadUrlPrimary()
		{
			Assert.That(ApplicationConfig.ImportCodeListsDownloadUrlPrimary, Is.EqualTo("https://www.einfuhr.internetzollanmeldung.de/iza/codierungen/EINFUHR/10.1/xml/index.uri"));
		}

		[Test]
		public void TestImportCodeListsDownloadUrlSecondary()
		{
			Assert.That(ApplicationConfig.ImportCodeListsDownloadUrlSecondary, Is.EqualTo("https://www.einfuhr.internetzollanmeldung.de/iza/codierungen/EINFUHR/10.0/xml/index.uri"));
		}

		[Test]
		public void TestExportCodeListsDownloadUrlPrimary()
		{
			Assert.That(ApplicationConfig.ExportCodeListsDownloadUrlPrimary, Is.EqualTo("https://www.ausfuhrplus.internetzollanmeldung.de/iaap/codierungen/EX/3.0/xml/index.uri"));
		}

		[Test]
		public void TestExportCodeListsDownloadUrlSecondary()
		{
			Assert.That(ApplicationConfig.ExportCodeListsDownloadUrlSecondary, Is.EqualTo(""));
		}

		[Test]
		public void TestNctsCodeListsDownloadUrlPrimary()
		{
			Assert.That(ApplicationConfig.NctsCodeListsDownloadUrlPrimary, Is.EqualTo("https://www.versand.internetzollanmeldung.de/iva/codierungen/VER/10.1/xml/index.uri"));
		}

		[Test]
		public void TestNctsCodeListsDownloadUrlSecondary()
		{
			Assert.That(ApplicationConfig.NctsCodeListsDownloadUrlSecondary, Is.EqualTo(""));
		}

		[Test]
		public void TestEmcsCodeListsDownloadPageUrl()
		{
			Assert.That(ApplicationConfig.EmcsCodeListsDownloadPageUrl, Is.EqualTo("https://www.zoll.de/DE/Fachthemen/Steuern/Verbrauchsteuern/EMCS/EMCS-Publikationen/Fachl-Codelisten/fachl-codelisten_node.html"));
		}

		[Test]
		public void TestExchangeRatesStartURL()
		{
			Assert.That(ApplicationConfig.ExchangeRatesStartURL, Is.EqualTo("https://www.zoll.de/SiteGlobals/Functions/Kurse/KursExport.xml?view=xmlexportkursesearchresultzoll"));
		}

		[Test]
		public void TestMinDelayInSecondsBetweenAttemptsInCaseOfDownloadError()
		{
			Assert.That(ApplicationConfig.MinDelayInMilliSecondsBetweenAttemptsInCaseOfDownloadError, Is.EqualTo(60000));
		}

		[Test]
		public void TestMaxAttemptsCountInCaseOfDownloadError()
		{
			Assert.That(ApplicationConfig.MaxAttemptsCountInCaseOfDownloadError, Is.EqualTo(10));
		}

	}
}
