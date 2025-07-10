using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class FiscalExchangeRatesBuilderTest
	{
		[Test]
		public void ConvertXmlFileToRefExchangeRateZZ()
		{
			var errorCollector = new StringBuilder();
			var builder = new FiscalExchangeRatesBuilder(errorCollector);

			var resultList = builder.ConvertRateNodesToRefExchangeRateZZ(ReferenceData, new DateTime(2021, 01, 18, 16, 15, 23));

			Assert.IsNotNull(resultList, "Rate nodes could not be converted, no results are returned");
			Assert.That(resultList.Count == 5, "Not all Rate nodes could be converted to ResCusCodeList");
			Assert.AreEqual("USD", resultList[0].ZZN_RX_NKExCurrency, "RefExchangeRateZZ is generated with wrong Currency Code");
			Assert.AreEqual(1.2064m, resultList[0].ZZN_Rate, "RefExchangeRateZZ should have '1.2064' as Rate");
			Assert.AreEqual(new DateTime(2021, 01, 18, 16, 15, 23), resultList[0].ZZN_StartDate, "Start date should be equal to '2021-01-18T16:15:23'");
			Assert.AreEqual(new DateTime(2079, 6, 6, 23, 59, 00), resultList[0].ZZN_EndDate, "End date should be equal to '2079-06-06T23:59:00'");
		}

		[Test]
		public void DuplicateRatesInXml()
		{
			var duplicateData = new List<RateNode>();
			duplicateData.AddRange(ReferenceData);
			duplicateData.Add(ReferenceData[0]);

			var errorCollector = new StringBuilder();
			var builder = new FiscalExchangeRatesBuilder(errorCollector);
			var refDataCollection = builder.ConvertRateNodesToRefExchangeRateZZ(duplicateData, new DateTime(2021, 01, 18, 16, 15, 23));

			Assert.That(errorCollector.ToString().StartsWith("RefExchangeRateZZ duplicate exists. Key: 'USD_20210118' Rate: 1.2064", StringComparison.InvariantCulture), "Duplicate data (USD_20210118) is not detected");
			Assert.AreEqual(5, refDataCollection.Count, "Resultset contains duplicate data, only 5 (unique) items should be in resultset");
		}

		[Test]
		public void InvalidDataInXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new FiscalExchangeRatesBuilder(errorCollector);
			var refDataCollection = builder.ConvertRateNodesToRefExchangeRateZZ(InvalidData, new DateTime(2021, 01, 18, 16, 15, 23));

			Assert.That(errorCollector.ToString().Contains("RefExchangeRateZZ validation error: Key '_20210118' Errors: ZZN_RX_NKExCurrency is required."), "Invalid data (missing currency code) is not detected.");
			Assert.That(errorCollector.ToString().Contains("RefExchangeRateZZ validation error: Key 'JPY_20210118' Errors: ZZN_Rate is required."), "Invalid data (missing exchange rate) is not detected.");
			Assert.AreEqual(2, refDataCollection.Count, "Resultset contains invalid data, only 2 items should be in resultset");
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new FiscalExchangeRatesBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;
			builder.BuildXml(new DateTime(2021, 01, 18, 13, 31, 25), ReferenceData, TempFolder);
			var expectedFileName = Path.Combine(TempFolder, "RefExchangeRateZZ_NL Fiscal Exchange Rates_133125000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.FiscalExchangeRates.Output.RefExchangeRateZZ_NLFiscalExchangeRates_133125000.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName}' could not be found");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Xml does not match the correct format.");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new List<RateNode>()
			{
				new RateNode()
				{
					Currency = "USD",
					Rate = 1.2064m
				},
				new RateNode()
				{
					Currency = "JPY",
					Rate = 125.18m
				},
				new RateNode()
				{
					Currency = "BNG",
					Rate = 1.9558m
				},
				new RateNode()
				{
					Currency = "AUD",
					Rate = 1.5721m
				},
				new RateNode()
				{
					Currency = "ZAR",
					Rate = 18.4919m
				}
			};
			InvalidData = new List<RateNode>()
			{
				new RateNode()
				{
					Currency = "",
					Rate = 1.2064m
				},
				new RateNode()
				{
					Currency = "JPY",
					Rate = 0.0000m
				},
				new RateNode()
				{
					Currency = "BNG",
					Rate = 1.9558m
				},
				new RateNode()
				{
					Currency = "ZAR",
					Rate = 18.4919m
				}
			};

			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		List<RateNode> ReferenceData, InvalidData;
	}
}
