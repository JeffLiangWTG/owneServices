using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.SEReferenceData.Business;
using CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.SEReferenceData.Tests;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Tests
{
	[TestFixture]
	class ExchangeRateParserTest
	{
		[Test]
		public void ExchangeRatesXML()
		{
			var monetaryExchangePeriods = TestHelperExchangeRates.GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.SEReferenceData.Tests.ExchangeRates.TestFiles.Input.IncrementalObjectTraderExport_2011.xml");
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SEReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_SE.xml");
			new ExchangeRateParser(monetaryExchangePeriods[0]).ConvertToXMLFile(outputFile, dateTimeProviderMock.Object);
			var actualXml = File.ReadAllText(outputFile);
			Assert.That(actualXml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void InvalidRate()
		{
			var monetaryExchangePeriods = TestHelperExchangeRates.GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.SEReferenceData.Tests.ExchangeRates.TestFiles.Input.IncrementalObjectTraderExport_2011Invalid.xml");
			var error = new ExchangeRateParser(monetaryExchangePeriods[0]).ConvertToXMLFile(outputFile, dateTimeProviderMock.Object);
			Assert.That(error, Does.Contain(@"Unable to import Exchange Rate due to invalid combination, Rate or Currency Code or Calculation unit. DETAILS:
Rate: -1.4
Currency: CZK
Calculation unit: 1"));
		}

		[Test]
		public void InvalidCalculationUnit()
		{
			var monetaryExchangePeriods = TestHelperExchangeRates.GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.SEReferenceData.Tests.ExchangeRates.TestFiles.Input.IncrementalObjectTraderExport_2011Invalid.xml");
			var error = new ExchangeRateParser(monetaryExchangePeriods[0]).ConvertToXMLFile(outputFile, dateTimeProviderMock.Object);
			Assert.That(error, Does.Contain(@"Unable to import Exchange Rate due to invalid combination, Rate or Currency Code or Calculation unit. DETAILS:
Rate: 0.10492849
Currency: CHF
Calculation unit: 0"));
		}

		[Test]
		public void InvalidCurrency()
		{
			var monetaryExchangePeriods = TestHelperExchangeRates.GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.SEReferenceData.Tests.ExchangeRates.TestFiles.Input.IncrementalObjectTraderExport_2011Invalid.xml");
			var error = new ExchangeRateParser(monetaryExchangePeriods[0]).ConvertToXMLFile(outputFile, dateTimeProviderMock.Object);
			Assert.That(error, Does.Contain(@"Unable to import Exchange Rate due to invalid combination, Rate or Currency Code or Calculation unit. DETAILS:
Rate: 0.11976048
Currency: 
Calculation unit: 100"));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			var outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Output");
			outputFile = Path.Combine(outputPath, "RefExchangeRateZZ_SE.xml");
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 1, 12));
		}
		Assembly assembly;
		string outputFile;
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
