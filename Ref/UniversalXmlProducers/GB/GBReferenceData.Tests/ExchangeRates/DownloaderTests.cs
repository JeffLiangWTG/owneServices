using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	internal class DownloaderTests
	{
		[Test]
		public void GetDownloadURLs()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new SharedReferenceData.Business.Common.Tests.CommonHelpers.DateTimeProvider();
			var downloader = new DownloaderForTest(dateTimeProvider, null, errorCollector);

			dateTimeProvider.TestDateTime = new DateTime(2022, 1, 31);
			var urls = downloader.GetDownloadURLs_Exposed().ToList();
			Assert.That(urls, Is.Not.Null);
			Assert.That(urls.Count, Is.EqualTo(2));
			Assert.That(urls[0], Is.EqualTo("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-01.xml"));
			Assert.That(urls[1], Is.EqualTo("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-02.xml"));

			dateTimeProvider.TestDateTime = new DateTime(2021, 12, 14);
			urls = downloader.GetDownloadURLs_Exposed().ToList();
			Assert.That(urls.Count, Is.EqualTo(2));
			Assert.That(urls[0], Is.EqualTo("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2021-12.xml"));
			Assert.That(urls[1], Is.EqualTo("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-01.xml"));
		}

		[Test]
		public void DownloadContent()
		{
			var errorCollector = new StringBuilder();
			var webClientWrapper = new Mock<IWebClientWrapper>();

			var virtualData = "Virtual Data downloaded";
			var virtualDate = new DateTime(2022, 1, 27, 13, 14, 15);

			webClientWrapper.Setup(x => x.GetDatedContent(It.IsAny<string>())).Returns((virtualDate, virtualData));
			var downloader = new DownloaderForTest(null, webClientWrapper.Object, errorCollector);

			var content = downloader.DownloadXml_Exposed("www.whocares.com");
			Assert.That(content.Content, Is.EqualTo(virtualData));
			Assert.That(content.LastModified, Is.EqualTo(virtualDate));
		}

		[Test]
		public void DownloadContent_ErrorLogged()
		{
			var errorCollector = new StringBuilder();
			var webClientWrapper = new Mock<IWebClientWrapper>();

			var xmlUrl = "www.notvalidurl.com/crash.xml";

			webClientWrapper.Setup(x => x.GetDatedContent(xmlUrl)).Callback<string>((url) => { throw new Exception("The web client crashed"); });
			var downloader = new DownloaderForTest(null, webClientWrapper.Object, errorCollector);

			var data = downloader.DownloadXml_Exposed(xmlUrl);
			Assert.That(data.Content, Is.EqualTo(string.Empty));
			Assert.That(errorCollector.ToString(), Is.EqualTo("Failed to download xml from URL www.notvalidurl.com/crash.xml\r\n"));
		}

		[Test]
		public void FetchExchangeRates()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new SharedReferenceData.Business.Common.Tests.CommonHelpers.DateTimeProvider();
			var webClientWrapper = new Mock<IWebClientWrapper>();

			dateTimeProvider.TestDateTime = new DateTime(2022, 1, 27);

			var janRates = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0122.xml");
			var febRates = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0222.xml");

			var janDate = new DateTime(2022, 1, 31, 13, 14, 15);
			var febDate = new DateTime(2022, 2, 3, 14, 15, 16);

			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-01.xml")).Returns((janDate, janRates));
			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-02.xml")).Returns((febDate, febRates));

			var downloader = new DownloaderForTest(dateTimeProvider, webClientWrapper.Object, errorCollector);

			var exchangeData = downloader.GetExchangeRates();
			var rates = exchangeData.Rates.ToList();

			Assert.That(rates, Is.Not.Null);
			Assert.That(rates.Count, Is.EqualTo(5));
			Assert.That(exchangeData.PublishDate, Is.EqualTo(febDate));
		}

		[Test]
		public void FetchExchangeRatesInvalidData()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new SharedReferenceData.Business.Common.Tests.CommonHelpers.DateTimeProvider();
			var webClientWrapper = new Mock<IWebClientWrapper>();

			dateTimeProvider.TestDateTime = new DateTime(2022, 1, 27);

			var janRates = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0122.xml");
			var febRates = "Something went wrong and we got some dodgy data";

			var janDate = new DateTime(2022, 1, 31, 13, 14, 15);
			var febDate = new DateTime(2022, 2, 3, 14, 15, 16);

			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-01.xml")).Returns((janDate, janRates));
			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-02.xml")).Returns((febDate, febRates));

			var downloader = new DownloaderForTest(dateTimeProvider, webClientWrapper.Object, errorCollector);

			var exchangeData = downloader.GetExchangeRates();
			var rates = exchangeData.Rates.ToList();

			Assert.That(rates, Is.Not.Null);
			Assert.That(rates.Count, Is.EqualTo(3));
			Assert.That(errorCollector.ToString(), Is.EqualTo("Failed to parse xml content from URL https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-02.xml\r\n"));
			Assert.That(exchangeData.PublishDate, Is.EqualTo(janDate));
		}

		[Test]
		public void FetchExchangeRatesWebException()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new SharedReferenceData.Business.Common.Tests.CommonHelpers.DateTimeProvider();
			var webClientWrapper = new Mock<IWebClientWrapper>();

			dateTimeProvider.TestDateTime = new DateTime(2022, 1, 27);

			var febRates = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0222.xml");
			var marIncorrectXml = "<NonRates>Got some unexpected xml data</NonRates>";

			var febDate = new DateTime(2022, 2, 3, 14, 15, 16);
			var marDate = new DateTime(2022, 3, 1);

			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-01.xml")).Callback<string>((url) => { throw new Exception("The web client crashed"); });
			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-02.xml")).Returns((febDate, febRates));
			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-03.xml")).Returns((marDate, marIncorrectXml));

			var downloader = new DownloaderForTest(dateTimeProvider, webClientWrapper.Object, errorCollector);

			var exchangeData = downloader.GetExchangeRates();
			var rates = exchangeData.Rates.ToList();

			Assert.That(rates, Is.Not.Null);
			Assert.That(rates.Count, Is.EqualTo(2));
			Assert.That(exchangeData.PublishDate, Is.EqualTo(febDate));

			errorCollector.Clear();
			dateTimeProvider.TestDateTime = new DateTime(2022, 3, 27);
			exchangeData = downloader.GetExchangeRates();
			rates = exchangeData.Rates.ToList();

			Assert.That(rates, Is.Not.Null);
			Assert.That(rates.Count, Is.EqualTo(0));
			Assert.That(errorCollector.ToString(), Is.EqualTo("'exchangeRateMonthList' element missing from XML for URL https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-03.xml\r\n"));
			Assert.That(exchangeData.PublishDate, Is.EqualTo(DateTime.MinValue));
		}

		[Test]
		public void ExtractFileDates()
		{
			var errorCollector = new StringBuilder();
			var downloader = new DownloaderForTest(null, null, errorCollector);
			var startDate = new DateTime(2022, 1, 1);
			var endDate = new DateTime(2022, 1, 31, 23, 59, 0);

			var element = TestHelper.GetXmlElement("exchangeRateMonthList", "CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0122.xml");
			var dates = downloader.ExtractFileDates_Exposed(element);
			Assert.That(dates.StartDate, Is.EqualTo(startDate));
			Assert.That(dates.EndDate, Is.EqualTo(endDate));
			Assert.That(dates.Success, Is.True);

			element = new XElement("exchangeRateMonthList");
			dates = downloader.ExtractFileDates_Exposed(element);
			Assert.That(dates.Success, Is.False);
			Assert.That(errorCollector.ToString(), Is.EqualTo("Could not extract date range as 'period' attribute is missing\r\n"));
			errorCollector.Clear();

			element.Add(new XAttribute("Period", "Some rubbish whish is not a set of dates"));
			dates = downloader.ExtractFileDates_Exposed(element);
			Assert.That(dates.Success, Is.False);
			Assert.That(errorCollector.ToString(), Is.EqualTo("Could not extract date range from period 'Some rubbish whish is not a set of dates'\r\n"));
		}

		[Test]
		public void ConvertXMLToModel()
		{
			var errorCollector = new StringBuilder();
			var downloader = new DownloaderForTest(null, null, errorCollector);
			var startDate = new DateTime(2022, 1, 1);
			var endDate = new DateTime(2022, 1, 31);

			var element = TestHelper.GetXmlElement("exchangeRate", "CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0122.xml");
			var model = downloader.ConvertXElementToModel_Exposed(startDate, endDate, element);

			Assert.That(model.StartDate, Is.EqualTo(startDate));
			Assert.That(model.EndDate, Is.EqualTo(endDate));
			Assert.That(model.Currency, Is.EqualTo("CAD"));
			Assert.That(model.Rate, Is.EqualTo(1.7223m));
		}
	}

	class DownloaderForTest : Downloader
	{
		public DownloaderForTest(IDateTimeProvider dateTimeProvider, IWebClientWrapper webClientWapper, StringBuilder errorCollector) : base(dateTimeProvider, webClientWapper, errorCollector) { }

		public IEnumerable<string> GetDownloadURLs_Exposed() => base.GetDownloadURLs();
		public (DateTime LastModified, string Content) DownloadXml_Exposed(string url) => base.DownloadXml(url);
		public (DateTime StartDate, DateTime EndDate, bool Success) ExtractFileDates_Exposed(XElement element) => base.ExtractFileDates(element);
		public ExchangeRate ConvertXElementToModel_Exposed(DateTime startdate, DateTime endDate, XElement element) => ConvertXElementToModel(startdate, endDate, element);
	}
}
