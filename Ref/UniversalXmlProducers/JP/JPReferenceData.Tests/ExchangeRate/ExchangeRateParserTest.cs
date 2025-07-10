using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Business;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ExchangeRateParserTest
	{
		[Test]
		public void TestExchangeRates()
		{
			var downloader = new DownloadExchangeRates();
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			var expectedFileUrl = "https://www.customs.go.jp/english/kawase/kawase2023/kouji-rate-english20230423-20230429.pdf";
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRate\TestFiles\Input\kouji-rate-english20230423-20230429.pdf");
			var basePagePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRate\TestFiles\Input\RateOfExchange_JapanCustoms.html");

			using (var mockBasePageFileStream = new FileStream(basePagePath, FileMode.Open))
			using (var mockPDFFileStream = new FileStream(inputFilePath, FileMode.Open))
			{
				string htmlContent;
				using (var reader = new StreamReader(mockBasePageFileStream))
				{
					htmlContent = reader.ReadToEnd();
				}

				mockHttpClientHelper.Setup(x => x.GetAsync(expectedFileUrl)).Returns(Task.FromResult<Stream>(mockPDFFileStream));
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(AppConfig.ExchangeRate.Url)).Returns(Task.FromResult(htmlContent));

				var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);
				Assert.That(expectedFileUrl.Equals(sourcePath, System.StringComparison.Ordinal));

				new ExchangeRateParser().ConvertToXMLFile(exchangeRates, outputFile, sourcePath);
			}

			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.JPReferenceData.Tests.ExchangeRate.TestFiles.Output.ExpectJPRefExchangeRateZZ.xml");
			Assert.That(expectedImportXML, Is.EqualTo(File.ReadAllText(outputFile)));
		}

		[Test]
		public void TestInvalidDateFormat()
		{
			var exchangeRateDetails = new List<IExchangeRateDetails>() { new ExchangeRateDetailsProvider("CNY", 6.66m )};
			var exchangeRatesProvider = new ExchangeRatesProvider("20239999","20230101", exchangeRateDetails);
			var error = new ExchangeRateParser().ConvertToXMLFile(exchangeRatesProvider, string.Empty, string.Empty);

			Assert.That(error, Does.StartWith("Unable to parse Start or End date. Start date string: 20239999. End state string: 20230101"));
			Assert.That(outputFile, Does.Not.Exist);

			exchangeRatesProvider = new ExchangeRatesProvider("20230101", "2023006", exchangeRateDetails);
			error = new ExchangeRateParser().ConvertToXMLFile(exchangeRatesProvider, string.Empty, string.Empty);

			Assert.That(error, Does.StartWith("Unable to parse Start or End date. Start date string: 20230101. End state string: 2023006"));
			Assert.That(outputFile, Does.Not.Exist);
		}

		[Test]
		public void TestEmptyCurrencyCode()
		{
			var details1 = new ExchangeRateDetailsProvider("USD", 1m);
			var details2 = new ExchangeRateDetailsProvider("", 1m);
			var rates = new ExchangeRatesProvider("20200713", "20200719", new List<IExchangeRateDetails>() { details1, details2 });
			var errors = new ExchangeRateParser().ConvertToXMLFile(rates, outputFile, "path1");
			Assert.That(errors, Does.Contain(@"Unable to import Exchange Rate due to empty Code, Rate or Duplicate Code.
Source PDF Details: path1
DETAILS:
Code: 
Rate: 1
"));
			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.JPReferenceData.Tests.ExchangeRate.TestFiles.Output.NoCurrencyCodeRefExchangeRateZZ.xml");
			Assert.That(expectedImportXML, Is.EqualTo(File.ReadAllText(outputFile)));
		}

		[Test]
		public void TestEmptyExchangeRate()
		{
			var details1 = new ExchangeRateDetailsProvider("USD", 1m);
			var details2 = new ExchangeRateDetailsProvider("CNY", 0);
			var rates = new ExchangeRatesProvider("20200713", "20200719", new List<IExchangeRateDetails>() { details1, details2 });
			var errors = new ExchangeRateParser().ConvertToXMLFile(rates, outputFile, "path1");
			Assert.That(errors, Does.Contain(@"Unable to import Exchange Rate due to empty Code, Rate or Duplicate Code.
Source PDF Details: path1
DETAILS:
Code: CNY
Rate: 0
"));

			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.JPReferenceData.Tests.ExchangeRate.TestFiles.Output.NoCurrencyRateRefExchangeRateZZ.xml");
			Assert.That(expectedImportXML, Is.EqualTo(File.ReadAllText(outputFile)));
		}

		[Test]
		public void TestDuplicateCurrency()
		{
			var details1 = new ExchangeRateDetailsProvider("USD", 1m);
			var details2 = new ExchangeRateDetailsProvider("USD", 2m);
			var rates = new ExchangeRatesProvider("20200713", "20200719", new List<IExchangeRateDetails>() { details1, details2 });
			var errors = new ExchangeRateParser().ConvertToXMLFile(rates, outputFile, "path1");
			Assert.That(errors, Does.Contain(@"Unable to import Exchange Rate due to empty Code, Rate or Duplicate Code.
Source PDF Details: path1
DETAILS:
Code: USD
Rate: 2
"));

			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.JPReferenceData.Tests.ExchangeRate.TestFiles.Output.DuplicateCurrencyExchangeRateZZ.xml");
			Assert.That(expectedImportXML, Is.EqualTo(File.ReadAllText(outputFile)));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			var outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRate\TestFiles\Output");
			outputFile = Path.Combine(outputPath, "RefExchangeRateZZ_JP.xml");
		}

		Assembly assembly;
		string outputFile;

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
