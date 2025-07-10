using CargoWise.RefDbRepo.GBReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests
{
	[TestFixture]
	class ApplicationConfigTests
	{
		[Test]
		public void TestSupportingDocumentCodeUrls()
		{
			Assert.That(ConfigurationProvider.SupportingDocumentCodeNationalUrl, Is.EqualTo("https://www.gov.uk/guidance/data-element-23-documents-and-other-reference-codes-national-of-the-customs-declaration-service-cds"));
			Assert.That(ConfigurationProvider.SupportingDocumentCodeUnionUrl, Is.EqualTo("https://www.gov.uk/government/publications/data-element-23-documents-and-other-reference-codes-union-of-the-customs-declaration-service-cds"));
			Assert.That(ConfigurationProvider.SupportingDocumentCodeStatusUrl, Is.EqualTo("https://www.gov.uk/guidance/data-element-23-document-status-codes-of-the-customs-declaration-service-cds"));
		}

		[Test]
		public void TestAdditionalInformationUrl()
		{
			Assert.That(ConfigurationProvider.AdditionalInformationUrl, Is.EqualTo("https://www.gov.uk/guidance/additional-information-ai-statement-codes-for-data-element-22-of-the-customs-declaration-service-cds"));
		}

		[Test]
		public void TestChiefHarmonisedDeclarationCodeUrl()
		{
			Assert.That(ConfigurationProvider.ChiefHarmonisedDeclarationCodeUrl, Is.EqualTo("https://www.gov.uk/government/publications/uk-trade-tariff-document-certificate-and-authorisation-codes-for-harmonised-declarations"));
		}

		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(ConfigurationProvider.OutputDirectory, Is.EqualTo(@"..\..\UXmlFiles"));
		}

		[Test]
		public void TestUKOfficeCodesLandingPageUrl()
		{
			Assert.That(ConfigurationProvider.UKOfficeCodesLandingPageUrl, Is.EqualTo("https://www.gov.uk/government/publications/uk-customs-office-codes-for-data-element-512-of-the-customs-declaration-service"));
		}

		[Test]
		public void TestUKOfficeCodesDataAnchorRegex()
		{
			Assert.That(ConfigurationProvider.UKOfficeCodesDataAnchorRegex, Is.EqualTo(@"\<a.*href=""(.*\.ods)"">UK customs office codes for the Customs Declaration Service"));
		}

		[Test]
		public void TestTariffAuthorisationUrl()
		{
			Assert.That(ConfigurationProvider.TariffAuthorisationUrl, Is.EqualTo("https://api.service.hmrc.gov.uk/oauth/token"));
		}

		[Test]
		public void TestTariffUrls()
		{
			Assert.That(ConfigurationProvider.TariffDailyUrl, Is.EqualTo("https://api.service.hmrc.gov.uk/bulk-data-download/list/TARIFF-DAILY"));
			Assert.That(ConfigurationProvider.TariffMonthlyUrl, Is.EqualTo("https://api.service.hmrc.gov.uk/bulk-data-download/list/TARIFF-MONTHLY"));
			Assert.That(ConfigurationProvider.TariffAnnualUrl, Is.EqualTo("https://api.service.hmrc.gov.uk/bulk-data-download/list/TARIFF-ANNUAL"));
		}

		[Test]
		public void TestTariffFileExclusionList()
		{
			Assert.That(ConfigurationProvider.TariffFileExclusionList, Is.EqualTo("ExcludedTestFile.gzip"));
		}

		[Test]
		public void TestTariffClientConfig()
		{
			Assert.That(ConfigurationProvider.TariffClientId, Is.EqualTo("W9yWr4y8fdlBWuyU7S4r2URzX2Ea"));
			Assert.That(ConfigurationProvider.TariffClientSecret, Is.EqualTo("eb0e95cf-7537-48d2-bf30-4d2642252c37"));
			Assert.That(ConfigurationProvider.TariffClientScope, Is.EqualTo(""));
		}

		[Test]
		public void TestReferenceDbServiceConfig()
		{
			Assert.That(ConfigurationProvider.RefDbServiceURI, Is.EqualTo("https://refdbrepoupdate-uat.wtg.zone/Update/odata/"));
			Assert.That(ConfigurationProvider.IsRefDbServiceSecure, Is.EqualTo(false));
		}

		[Test]
		public void TestExchangeRateBaseUrl()
		{
			Assert.That(ConfigurationProvider.ExchangeRateBaseUrl, Is.EqualTo("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_{0}.xml"));
		}

		[Test]
		public void TestExchangeRateMonthFormat()
		{
			Assert.That(ConfigurationProvider.ExchangeRateMonthFormat, Is.EqualTo("yyyy-MM"));
		}

		[Test]
		public void TestCDSErrorCodeConfig()
		{
			Assert.That(ConfigurationProvider.CDSErrorCodeUrl, Is.EqualTo("https://www.gov.uk/government/publications/customs-declaration-service-error-codes"));
		}

		[Test]
		public void TestCDSErrorCodeAnchorText()
		{
			Assert.That(ConfigurationProvider.CDSErrorCodeAnchorText, Is.EqualTo("Customs Declaration Service — error codes"));
		}

		[Test]
		public void TestTariffHistoricalPeriod()
		{
			Assert.That(ConfigurationProvider.TariffHistoricalPeriod, Is.EqualTo(0));
		}

		[Test]
		public void TestTariffUseMostRecentAnnualFileOnly()
		{
			Assert.That(ConfigurationProvider.TariffUseMostRecentAnnualFileOnly, Is.True);
		}

		[Test]
		public void TestTariffUseSpecifiedAnnualFileOnly()
		{
			Assert.That(ConfigurationProvider.TariffUseSpecifiedAnnualFileOnly, Is.EqualTo("tariff_yearlyExtract_v1_20240101T000000"));
		}
	}
}
