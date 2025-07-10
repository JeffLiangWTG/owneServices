using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class CustomsExchangeRatesBuilderTest
	{
		[Test]
		public void TestExchangeRateToRefExchangeRateZZ()
		{
			var errorCollector = new StringBuilder();
			var builder = new CustomsExchangeRatesBuilder(errorCollector);

			var resultList = builder.ConvertRateNodesToRefExchangeRateZZ(referenceData, new DateTime(2021, 01, 18, 16, 15, 23));

			Assert.IsNotNull(resultList, "Exchange Rates could not be converted, no results are returned");
			Assert.That(resultList.Count == 4, "Not all Exchange Rates could be converted to ResCusCodeList");
			Assert.AreEqual("GBP", resultList[0].ZZN_RX_NKExCurrency, "RefExchangeRateZZ is generated with wrong Currency Code");
			Assert.AreEqual(0.767100m, resultList[0].ZZN_Rate, "RefExchangeRateZZ should have '0.767100m' as Rate");
			Assert.AreEqual(new DateTime(2021, 01, 18, 16, 15, 23), resultList[0].ZZN_StartDate, "Start date should be equal to '2021-01-18T16:15:23'");
			Assert.AreEqual(new DateTime(2021, 01, 31, 23, 59, 00), resultList[0].ZZN_EndDate, "End date should be equal to '2021-01-31T23:59:00'");
		}

		[Test]
		public void TestExchangeRateToXML()
		{

			var errorCollector = new StringBuilder();
			var builder = new CustomsExchangeRatesBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;
			builder.BuildXml(new DateTime(2021, 01, 18, 13, 31, 25), referenceData, tempFolder);
			var expectedFileName = Path.Combine(tempFolder, "RefExchangeRateZZ_NL Customs Exchange Rates_133125000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.CustomsExchangeRates.Output.RefExchangeRateZZ_CustomsExchangeRate_Correct.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName}' could not be found");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Xml does not match the correct format.");
		}

		[Test]
		public void TestInvalidDataInExchangeRates()
		{
			var errorCollector = new StringBuilder();
			var builder = new CustomsExchangeRatesBuilder(errorCollector);
			var refDataCollection = builder.ConvertRateNodesToRefExchangeRateZZ(invalidData, new DateTime(2021, 01, 18, 16, 15, 23));

			Assert.That(errorCollector.ToString().Contains("RefExchangeRateZZ validation error: Key '_20210118' Errors: ZZN_RX_NKExCurrency is required."), "Invalid data (missing currency code) is not detected.");
			Assert.That(errorCollector.ToString().Contains("RefExchangeRateZZ validation error: Key 'PLN_20210118' Errors: ZZN_Rate is required."), "Invalid data (missing exchange rate) is not detected."); 
			Assert.AreEqual(2, refDataCollection.Count, "Result set contains invalid data, only 2 items should be in resultset");
		}

		[Test]
		public void TestInvalidDateInExchangeRates()
		{
			var errorCollector = new StringBuilder();
			var builder = new CustomsExchangeRatesBuilder(errorCollector);
			var refDataCollection = builder.ConvertRateNodesToRefExchangeRateZZ(referenceData.GetRange(0, 1), new DateTime());

			Assert.That(errorCollector.ToString().Contains("RefExchangeRateZZ validation error: Key 'GBP_00010101' Errors: ZZN_StartDate is required"), "Invalid data (missing start date) is not detected.");
			Assert.AreEqual(0, refDataCollection.Count, "Invalid date provided, result set should be empty");
		}

		[Test]
		public void TesDuplicateRatesInXML()
		{
			var duplicateData = new List<ExchangeRate>();
			duplicateData.AddRange(referenceData);
			duplicateData.Add(referenceData[0]);

			var errorCollector = new StringBuilder();
			var builder = new CustomsExchangeRatesBuilder(errorCollector);
			var refDataCollection = builder.ConvertRateNodesToRefExchangeRateZZ(duplicateData, new DateTime(2021, 01, 18, 16, 15, 23));

			Assert.That(errorCollector.ToString().StartsWith("RefExchangeRateZZ duplicate exists. Key: 'GBP_20210118' Rate: 0.767100", StringComparison.InvariantCulture), "Duplicate data (GBP_20210118) is not detected");
			Assert.AreEqual(4, refDataCollection.Count, "Result set contains duplicate data, only 4 (unique) items should be in resultset");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			referenceData = new List<ExchangeRate>
			{
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 0.767100m,
					CurrencyTypeCode = "GBP"
				},
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 4.3353964m,
					CurrencyTypeCode = "PLN"
				},
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 4.9728388m,
					CurrencyTypeCode = "RON"
				},
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 24.259433m,
					CurrencyTypeCode = "CZK"
				}
			};
			invalidData = new List<ExchangeRate>
			{
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 0.767100m,
					CurrencyTypeCode = ""
				},
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 0.0000m,
					CurrencyTypeCode = "PLN"
				},
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 4.9728388m,
					CurrencyTypeCode = "RON"
				},
				new ExchangeRate
				{
					FunctionCode = "94",
					RateNumeric = 24.259433m,
					CurrencyTypeCode = "CZK"
				}
			};

			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;
		List<ExchangeRate> referenceData, invalidData;
	}
}
