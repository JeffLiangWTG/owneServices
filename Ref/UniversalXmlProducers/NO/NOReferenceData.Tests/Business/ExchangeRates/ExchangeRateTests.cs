using System.IO;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.ExchangeRates.Tests
{
	sealed class ExchangeRateTests
	{
		[Test]
		public void TestCompareUniversalXml()
		{
			var xmlData = XmlHelper.ReadDeserializedManifestResourceContent<omregningKursListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ExchangeRates.Testfiles.Input.valutakurs.xml");
			ExchangeRateParser.ConvertToXmlFile(xmlData, "02/02/2022 00:00:36", outputTempFileForTest);

			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);
			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ExchangeRates.Testfiles.Output.RefExchangeRateZZ_NO.xml");

			Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
		}

		[Test]
		public void TestEmptyCurrency()
		{
			AssertTradeGroupError(string.Empty, "AUSTRALSKE DOLLAR", "6,410", 1, "2022-01-31", "2022-02-06");
		}

		[Test]
		public void TestEmptyRate()
		{
			AssertTradeGroupError("AUD", "AUSTRALSKE DOLLAR", string.Empty, 1, "2022-01-31", "2022-02-06");
		}

		[Test]
		public void TestZeroFactor()
		{
			AssertTradeGroupError("AUD", "AUSTRALSKE DOLLAR", "6,410", 0, "2022-01-31", "2022-02-06");
		}

		[Test]
		public void TestNegativeFactor()
		{
			AssertTradeGroupError("AUD", "AUSTRALSKE DOLLAR", "6,410", -1, "2022-01-31", "2022-02-06");
		}

		[Test]
		public void TestInvalidStartDate()
		{
			AssertTradeGroupError("AUD", "AUSTRALSKE DOLLAR", "6,410", 1, "2022-31-32", "2022-02-06");
		}

		[Test]
		public void TestInvalidEndDate()
		{
			AssertTradeGroupError("AUD", "AUSTRALSKE DOLLAR", "6,410", 1, "2022-01-31", "2022-33-34");
		}

		[Test]
		public void TestInvalidModifiedDateTime()
		{
			var exchangeRate = SetupExchangeRateRecord("AUD", "AUSTRALSKE DOLLAR", "6,410", 1, "2022-01-31", "2022-02-06");
			var errors = ExchangeRateParser.ConvertToXmlFile(exchangeRate, "32/13/2022 00:87:36", outputTempFileForTest);
			Assert.That(errors, Does.StartWith("Failed to parse lastUpdated DateTime 32/13/2022 00:87:36"));
		}

		[SetUp]
		public void Setup()
		{
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}
		string outputTempFileForTest;

		void AssertTradeGroupError(string currency, string description, string rate, short factor, string startDate, string endDate)
		{
			var expectedErrorMessage = $@"Unable to parse Exchange Rate due to empty currency, zero/negative rate or invalid Dates.
DETAILS:
Currency: {currency}
Rate: {rate}
Factor: {factor}
Start Date: {startDate}
End Date: {endDate}
";
			var exchangeRate = SetupExchangeRateRecord(currency, description, rate, factor, startDate, endDate);
			var errors = ExchangeRateParser.ConvertToXmlFile(exchangeRate, "03/06/2022 14:35:36", outputTempFileForTest);
			Assert.That(errors, Is.EqualTo(expectedErrorMessage).NoClip);
		}

		static omregningKursListe SetupExchangeRateRecord(string currency, string description, string rate, short factor, string startDate, string endDate)
		{
			return new omregningKursListe()
			{
				ConversionRate = new Omregningskurs[]
				{
					new Omregningskurs()
					{
						CurrencyCode = currency,
						CurrencyDescription = description,
						CurrencyRate = rate,
						Multiplier = factor,
						DateStart = startDate,
						DateEnd= endDate
					}
				}
			};
		}
	}
}
