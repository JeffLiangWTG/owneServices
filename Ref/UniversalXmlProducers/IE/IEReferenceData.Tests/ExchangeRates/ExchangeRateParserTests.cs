using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Tests
{
	[TestFixture]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "<Pending>")]
	class ExchangeRateParserTests
	{
		[Test]
		public void ExchangeRateXml_September2023Format()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\september-2023.aspx");
			var htmlDocument = htmlWeb.Load(inputFilePath);
			var exchangeRates = DownloadExchangeRates.GetExchangeRatesData(inputFilePath, htmlDocument);
			var exchangeRateDate = DownloadExchangeRates.GetMonthAndYearRatesRelateTo(htmlDocument);
			parser.ConvertToXmlFile(exchangeRates, dateTimeProviderMock.Object, exchangeRateDate, outputFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE202309.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXml));
		}

		[Test]
		public void ExchangeRateXml_April2023Format()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\april-2023.aspx");
			var htmlDocument = htmlWeb.Load(inputFilePath);
			var exchangeRates = DownloadExchangeRates.GetExchangeRatesData(inputFilePath, htmlDocument);
			var exchangeRateDate = DownloadExchangeRates.GetMonthAndYearRatesRelateTo(htmlDocument);
			parser.ConvertToXmlFile(exchangeRates, dateTimeProviderMock.Object, exchangeRateDate, outputFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE202304.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXml));
		}

		[Test]
		public void ExchangeRateXml_March2023Format()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\march-2023.aspx");
			var htmlDocument = htmlWeb.Load(inputFilePath);
			var exchangeRates = DownloadExchangeRates.GetExchangeRatesData(inputFilePath, htmlDocument);
			var exchangeRateDate = DownloadExchangeRates.GetMonthAndYearRatesRelateTo(htmlDocument);
			parser.ConvertToXmlFile(exchangeRates, dateTimeProviderMock.Object, exchangeRateDate, outputFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE202303.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXml));
		}

		[Test]
		public void ExchangeRateXml_February2024Format()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\february-2024.aspx");
			var htmlDocument = htmlWeb.Load(inputFilePath);
			var exchangeRates = DownloadExchangeRates.GetExchangeRatesData(inputFilePath, htmlDocument);
			var exchangeRateDate = DownloadExchangeRates.GetMonthAndYearRatesRelateTo(htmlDocument);
			parser.ConvertToXmlFile(exchangeRates, dateTimeProviderMock.Object, exchangeRateDate, outputFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE202402.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXml));
		}

		[Test]
		public void ExchangeRateXml_January2025Format()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\january-2025.csv");
			var exchangeRates = DownloadExchangeRates.GetExchangeRatesDataFromCsv(inputFilePath);
			parser.ConvertToXmlFile(exchangeRates, dateTimeProviderMock.Object, new DateTime(2025, 1, 1), outputFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE202501.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXml));
		}

		[Test]
		public void ExchangeRateXml_January2021Format()
		{
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input\january-2021.aspx");
			var htmlDocument = htmlWeb.Load(inputFilePath);
			var exchangeRates = DownloadExchangeRates.GetExchangeRatesData(inputFilePath, htmlDocument);
			var exchangeRateDate = DownloadExchangeRates.GetMonthAndYearRatesRelateTo(htmlDocument);
			parser.ConvertToXmlFile(exchangeRates, dateTimeProviderMock.Object, exchangeRateDate, outputFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXml));
		}

		[Test]
		public void InvaildCurrencyDescription()
		{
			var currencyWithInvaildCode = new Dictionary<string, decimal>
			{
				{ "invalidCode", 1.2m }
			};
			var errors = parser.ConvertToXmlFile(currencyWithInvaildCode, dateTimeProviderMock.Object, "01/08/20", outputFile);
			var expectedError = @"Unable to import Exchange Rate due to invalid Description or empty Rate.
DETAILS:
invalidCode - 1.2
";
			Assert.That(errors, Is.EqualTo(expectedError));
		}

		[Test]
		public void InvaildCurrencyRate()
		{
			var currencyWithInvaildCode = new Dictionary<string, decimal>
					{
						{ "US Dollar", 0m }
					};
			var errors = parser.ConvertToXmlFile(currencyWithInvaildCode, dateTimeProviderMock.Object, "01/08/20", outputFile);
			var expectedError = @"Unable to import Exchange Rate due to invalid Description or empty Rate.
DETAILS:
US Dollar - 0
";
			Assert.That(errors, Is.EqualTo(expectedError));
		}

		[Test]
		public void InvaildRecordsNotIncludedInXml()
		{
			var ratesWithInvaildCodeAndRate = new Dictionary<string, decimal>
					{
						{ "invalidCode", 1.2m },
						{ "US Dollar", 0m },
						{ "Sterling", 0.91123m },
						{ "Japanese Yen", 123.94m },
						{ "Omani Rial", 0.4463m },
					};
			parser.ConvertToXmlFile(ratesWithInvaildCodeAndRate, dateTimeProviderMock.Object, "01/08/20", outputFile);
			var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.IEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_IE_Invalid.xml");
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}

		[Test]
		public void InvaildExchangeRateDate()
		{
			var errors = parser.ConvertToXmlFile(new Dictionary<string, decimal>(), dateTimeProviderMock.Object, "01-08-20", outputFile);
			Assert.That(errors, Is.EqualTo("Invalid format of Exchange Rate Date: 01-08-20\r\n"));
			FileAssert.DoesNotExist(outputFile);
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			htmlWeb = new HtmlWeb();
			var outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"IE\ExchangeRates\TestFiles\Output");
			outputFile = Path.Combine(outputPath, "RefExchangeRateZZ_IE.xml");
			parser = new ExchangeRatesProducer();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 1, 13));
		}
		Assembly assembly;
		HtmlWeb htmlWeb;
		string outputFile;
		ExchangeRatesProducer parser;
		Mock<IDateTimeProvider> dateTimeProviderMock;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}
	}
}
