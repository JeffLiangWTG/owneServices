using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

class ExchangeRatesParserTests : TestCase
{
	[TestCaseSource(nameof(ConvertTestData))]
	public void TestConvert(DateTime currentDate, string resultManifestResource)
	{
		using var webServiceMockStream = ExecutingAssembly.GetManifestResourceStream(WebServiceManifest);
		using var expectedTestStream = ExecutingAssembly.GetManifestResourceStream(resultManifestResource);
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(currentDate);
		TestConvert(webServiceMockStream, expectedTestStream, DateProvider, resultPathAndFileName);
	}

	public static IEnumerable<TestCaseData> ConvertTestData
	{
		get
		{
			yield return new TestCaseData(new DateTime(2019, 6, 19), "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_ES.xml") { TestName = "{m} June 19th 2019" };
			yield return new TestCaseData(new DateTime(2019, 6, 20), "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_ES.xml") { TestName = "{m} June 20th 2019" };
			yield return new TestCaseData(new DateTime(2019, 7, 10), "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_ES.xml") { TestName = "{m} July 10th 2019" };
			yield return new TestCaseData(new DateTime(2019, 7, 24), "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_ES_August.xml") { TestName = "{m} July 24th 2019" };
		}
	}

	[Test]
	public void TestInvalidRatesByDate()
	{
		using var webServiceMockStream = ExecutingAssembly.GetManifestResourceStream(WebServiceManifest);
		var webServiceMockXMLDocument = new XmlDocument();
		webServiceMockXMLDocument.Load(webServiceMockStream);
		var invalidDateProvider = new Mock<IDateTimeProvider>();
		invalidDateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2029, 6, 24));
		var exchangeRatesParser = new ExchangeRatesParser(invalidDateProvider.Object);
		var exception = Assert.Throws<ExchangeRatesException>(() => exchangeRatesParser.ConvertRatesForPenultimateWednesdayToXMLFile(resultPathAndFileName, webServiceMockXMLDocument));
		Assert.That(exception.Message, Is.EqualTo("Unable to find the following date: 2029-06-20 in Exchange Rates Document. Download date: 2029-06-24"));
	}

	[Test]
	public void TestInvalidRatesToExport()
	{
		using var webServiceMockStream = ExecutingAssembly.GetManifestResourceStream("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Input.invalid-eurofxref-hist-90d.xml");
		var webServiceMockXMLDocument = new XmlDocument();
		webServiceMockXMLDocument.Load(webServiceMockStream);
		var dateProviderMock = new Mock<IDateTimeProvider>();
		dateProviderMock.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 24));
		var exchangeRatesParser = new ExchangeRatesParser(dateProviderMock.Object);
		var errorMessage = exchangeRatesParser.ConvertRatesForPenultimateWednesdayToXMLFile(resultPathAndFileName, webServiceMockXMLDocument);
		Assert.That(errorMessage, Does.Contain(@"Unable to import Exchange rate as missing attribute 'rate' or 'currency'. Details:
Rate: 
Currency: JPY
"));

		Assert.That(errorMessage, Does.Contain(@"Unable to import Exchange rate as missing attribute 'rate' or 'currency'. Details:
Rate: 1.526
Currency: 
"));

		Assert.That(errorMessage, Does.Contain(@"Unable to parse Exchange rate into a decimal. Details:
Rate: 15,5802
Currency: ZAR
"));
	}

	[Test]
	public void TestInvalidFormat()
	{
		using var webServiceMockStream = ExecutingAssembly.GetManifestResourceStream("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Input.different-format-euroxref-hist-90d.xml");
		using var expectedTestStream = ExecutingAssembly.GetManifestResourceStream("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_ES_July.xml");
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 7, 24));
		var exception = Assert.Throws<ExchangeRatesException>(() => TestConvert(webServiceMockStream, expectedTestStream, DateProvider, resultPathAndFileName));
		Assert.That(exception.Message, Is.EqualTo("XML Format has changed for Exchange Rates"));
	}

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		resultPathAndFileName = Path.Combine(FileHelper.OutputFolder, "RefExchangeRateZZ_ES.xml");
	}
	string resultPathAndFileName;

	protected override string TestClassName => nameof(ExchangeRatesParserTests);

	void TestConvert(Stream webServiceMockStream, Stream expectedTestStream, Mock<IDateTimeProvider> dateProviderMock, string resultPathAndFileName)
	{
		var webServiceMockXMLDocument = new XmlDocument();
		webServiceMockXMLDocument.Load(webServiceMockStream);
		var exchangeRatesParser = new ExchangeRatesParser(dateProviderMock.Object);
		exchangeRatesParser.ConvertRatesForPenultimateWednesdayToXMLFile(resultPathAndFileName, webServiceMockXMLDocument);
		using var resultStream = new FileStream(resultPathAndFileName, FileMode.Open);
		using var reader = new StreamReader(resultStream, Encoding.GetEncoding("UTF-8"));
		using var expectedReader = new StreamReader(expectedTestStream, Encoding.GetEncoding("UTF-8"));
		var expectedResult = expectedReader.ReadToEnd();
		var xml = reader.ReadToEnd();
		Assert.That(xml, Is.EqualTo(expectedResult));
	}

	const string WebServiceManifest = "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.ExchangeRates.TestFiles.Input.eurofxref-hist-90d.xml";
}
