using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Services;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Tests
{
	[TestFixture]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "<Pending>")]
	class DownloadExchangeRatesTests
	{
		[Test]
		public void GetLatestExchangeRateURL()
		{
			var pathToTestFile = Path.Combine(baseInputFolder, "index.aspx");
			var downloadUrl = DownloadExchangeRates.GetLatestExchangeRateURL(pathToTestFile).ExchangeRateUri;
			var expectedUri = new Uri("file:///en/customs-traders-and-agents/importing-and-exporting/exchange-rates/2020/august-2020.aspx");
			Assert.That(downloadUrl, Is.EqualTo(expectedUri));
		}

		[Test]
		public void GetLatestExchangeRateInvalidURL()
		{
			var exception = Assert.Throws<ExchangeRatesException>(() => DownloadExchangeRates.GetLatestExchangeRateURL("https://InvalidURL.com"));
			Assert.That(exception.Message, Does.Contain("Unable to access Exchange Rate index page"));
		}

		[Test]
		public void GetLatestExchangeRateURL_2024()
		{
			var pathToTestFile = Path.Combine(baseInputFolder, "index-2024.aspx");
			var downloadUrl = DownloadExchangeRates.GetLatestExchangeRateURL(pathToTestFile).ExchangeRateUri;
			var expectedUri = new Uri("file:///en/customs/businesses/importing-exporting/exchange-rates/2024/december.aspx");
			Assert.That(downloadUrl, Is.EqualTo(expectedUri));
		}

		[Test]
		public void GetLatestExchangeRateURL_2025()
		{
			var pathToTestFile = Path.Combine(baseInputFolder, "index-2025.aspx");
			var downloadUrl = DownloadExchangeRates.GetLatestExchangeRateURL(pathToTestFile).ExchangeRateUri;
			var expectedUri = new Uri("file:///en/customs/businesses/importing-exporting/exchange-rates/2025/january.csv");
			Assert.That(downloadUrl, Is.EqualTo(expectedUri));
		}

		[Test]
		public void GetExchangeRatesInvalidData()
		{
			var pathToTestFile = Path.Combine(baseInputFolder, "august-2020-invaildExchangeData.aspx");
			var exception = Assert.Throws<ExchangeRatesException>(() => DownloadExchangeRates.GetExchangeRatesData(pathToTestFile, htmlWeb.Load(pathToTestFile)));
			Assert.That(exception.Message, Does.Contain("Unable to generate exchange rate table from URL:"));
			Assert.That(exception.Message, Does.Contain("The input string '' was not in a correct format"));
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			baseInputFolder = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\");
			htmlWeb = new HtmlWeb();
		}
		string baseInputFolder;
		HtmlWeb htmlWeb;
	}
}
