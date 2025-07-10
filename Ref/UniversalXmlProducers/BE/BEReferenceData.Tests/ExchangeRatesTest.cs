using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class ExchangeRatesTest
	{
		[Test]
		public void TestRetrieveRefExchangeRateZZMultipleResultForMonth()
		{
			var exchangeRates = ExcelParser.ReadExchangeRateXlsIntoResults(Path.Combine(testFilesInputPath, "listed_currencies.xlsx"), new DateTime(2016, 1, 1), new DateTime(2016, 1, 31));
			var foundRUJan1 = false;
			var foundRUJan20 = false;
			var foundRUJan27 = false;

			foreach (var rate in exchangeRates)
			{
				if (rate.ZZN_RN_NKCountry == "RU" && rate.ZZN_RX_NKExCurrency == "RUB")
				{
					if (rate.ZZN_StartDate.Day == 1)
					{
						foundRUJan1 = true;
						Assert.AreEqual(new decimal(77.13250), rate.ZZN_Rate, "Exchange rate for RUB between January 1st 2016 and January 20th 2016 is incorrect.");
						Assert.AreEqual(19, rate.ZZN_EndDate.Day, "Exchange rate for RUB starting on January 1st should have ended on the 19th.");
						Assert.AreEqual(1, rate.ZZN_EndDate.Month, "Exchange rate in January should have ended in January.");
					}
					else if (rate.ZZN_StartDate.Day == 20)
					{
						foundRUJan20 = true;
						Assert.AreEqual(new decimal(82.30980), rate.ZZN_Rate, "Exchange rate for RUB between January 20th 2016 and January 27th 2016 is incorrect.");
						Assert.AreEqual(26, rate.ZZN_EndDate.Day, "Exchange rate for RUB starting on January 20th should have ended on the 26th.");
						Assert.AreEqual(1, rate.ZZN_EndDate.Month, "Exchange rate in January should have ended in January.");
					}
					else if (rate.ZZN_StartDate.Day == 27)
					{
						foundRUJan27 = true;
						Assert.AreEqual(new decimal(88.20050), rate.ZZN_Rate, "Exchange rate for RUB between January 27th 2016 and January 31st 2016 is incorrect.");
						Assert.AreEqual(31, rate.ZZN_EndDate.Day, "Exchange rate for RUB starting on January 27th should have ended on the 31st.");
						Assert.AreEqual(1, rate.ZZN_EndDate.Month, "Exchange rate in January should have ended in January.");
					}
				}
			}

			if (!foundRUJan1)
			{
				Assert.Fail("Did not found the exchange rate for RU/RUB starting on January 1st.");
			}
			if (!foundRUJan20)
			{
				Assert.Fail("Did not found the exchange rate for RU/RUB starting on January 20th.");
			}
			if (!foundRUJan27)
			{
				Assert.Fail("Did not found the exchange rate for RU/RUB starting on January 27th.");
			}
		}

		[Test]
		public void TestGenerateXMLSingleExchangeRatePerMonth()
		{
			TestGenerateXMLForMonth(2020, 9);
		}

		[Test]
		public void TestGenerateXMLMultipleExchangeRatesPerMonth_enUS()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			TestGenerateXMLForMonth(2016, 1);
		}

		[Test]
		public void TestGenerateXMLMultipleExchangeRatesPerMonth_nlBE()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-BE");
			TestGenerateXMLForMonth(2016, 1);
		}

		void TestGenerateXMLForMonth(int year, int month)
		{
			var fileName = "RefExchangeRateZZ_BE_" + year + "_" + month + ".xml";

			var expectedTestStream = File.ReadAllText(Path.Combine(testFilesOutputPath, fileName));
			var publicationTime = new DateTime(2020, 9, 1);
			var outputFile = Path.Combine(outputPath, fileName);

			var start = new DateTime(year, month, 1);
			var end = start.AddMonths(1).AddSeconds(-1);

			var exchangeRates = ExcelParser.ReadExchangeRateXlsIntoResults(Path.Combine(testFilesInputPath, "listed_currencies.xlsx"), start, end);
			exchangeRates.AddRange(ExcelParser.ReadExchangeRateXlsIntoResults(Path.Combine(testFilesInputPath, "unlisted_currencies.xlsx"), start, end));

			XMLGeneration.ExportToXMLFile("BE Exchange Rates", outputFile, XMLGeneration.GetRefExchangeRateWriterConfiguration(), publicationTime, exchangeRates);

			var converterResultStream = File.ReadAllText(outputFile);

			BEReferenceDataTestHelper.AssertEqualXML(converterResultStream, expectedTestStream);
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Input");
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Output");
		}

		Assembly assembly;
		string outputPath;
		string testFilesInputPath;
		string testFilesOutputPath;
	}
}
