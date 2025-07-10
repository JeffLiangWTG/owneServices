using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Cud;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Cud
{
	[TestFixture]
	sealed class CudUniversalReferenceDataXmlGeneratorTest
	{
		[Test]
		public void TestExportToXmlFile()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var expectedDataTable = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Output.CudResultExample.xml"));

			var xmlData = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.eurofxref-hist-90d.xml"));
			var data = XmlParser.Deserialize<Envelope>(xmlData).Cube;

			var date = new DateTime(2020, 09, 29);

			var dataToParse = CudExchangeRate.FlattenCubeDataToDictionary(data);
			var parsedData = CudExchangeRate.ParseXmlDataFromDate(dataToParse, date, false);

			var tempFile = Path.GetTempFileName();
			CudUniversalReferenceDataXmlGenerator.ExportToXmlFile(parsedData, date, tempFile);
			var result = XDocument.Load(tempFile);

			var expected = RemovePublicationTimeFromXmlContent(expectedDataTable.ToString());
			var resultString = RemovePublicationTimeFromXmlContent(result.ToString());
			Assert.AreEqual(expected, resultString);

			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}

			string RemovePublicationTimeFromXmlContent(string content)
			{
				return content.Remove(content.IndexOf("<PublicationTime>"), "<PublicationTime>YYYY-MM-DDThh:mm:ss</PublicationTime>".Length);
			}
		}

		[Test]
		public void GetRefExchangeRateZZFromCudExRate()
		{
			Assert.Multiple(() =>
			{
				var date = new DateTime(2024, 01, 22);
				var input = new CudExRate();
				var result = CudUniversalReferenceDataXmlGenerator.GetRefExchangeRateZZFromCudExRate(input, date).ToArray();
				Assert.That(result, Is.Not.Null, "with empty input empty list should be returned");
				Assert.That(result.Length, Is.EqualTo(0), "with empty input empty list should be returned");

				input.ExRateData = new List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						ExRate = "123.4",
						StartDate = new DateTime(2024, 01, 01)
					},
					new CudExRateSingle()
					{
						ExRate = "5346",
						CurrencyCode = "asd",
						StartDate = new DateTime(2024, 01, 02)
					},
				};

				result = CudUniversalReferenceDataXmlGenerator.GetRefExchangeRateZZFromCudExRate(input, date).ToArray();
				var resultItemCound = result.Length;
				Assert.That(resultItemCound, Is.EqualTo(2), "For each CudExRateSingle 1 RefCusRateZZ should be generated");
				for (var i = 0; i < resultItemCound; i++)
				{
					var inputItem = input.ExRateData[i];
					var expectedRate = Convert.ToDecimal(inputItem.ExRate, CultureInfo.InvariantCulture);
					Assert.That(result[i].ZZN_Rate, Is.EqualTo(expectedRate), $"{i} - ZZN_Rate");
					Assert.That(result[i].ZZN_RX_NKExCurrency, Is.EqualTo("EUR"), $"{i} - ZZN_RX_NKExCurrency");
					Assert.That(result[i].ZZN_EndDate, Is.EqualTo(date), $"{i} - ZZN_EndDate");
					Assert.That(result[i].ZZN_StartDate, Is.EqualTo(inputItem.StartDate), $"{i} - ZZN_StartDate");
				}
			});
		}

		[TestCase(1, 01, 2020)]
		[TestCase(11, 01, 2020)]
		[TestCase(11, 02, 2020)]
		[TestCase(22, 01, 2020)]
		[TestCase(30, 01, 2020)]
		[TestCase(30, 10, 2020)]
		public void TestGetEndDateForXmlDate_TestCases(int day, int month, int year)
		{
			var testData = new DateTime(year, month, day);

			var expectedEndDate = GetExpectedEndDate(testData);
			CudUniversalReferenceDataXmlGenerator.GetEndDateForXmlDate(testData, out var resultEndDate);
			Assert.AreEqual(expectedEndDate, resultEndDate);
		}

		[Test]
		public void TestGetEndDateForXmlDate()
		{
			var testData = new DateTime(2020, 10, 30);

			var expectedEndDate = new DateTime(testData.Year, testData.Month + 1, DateTime.DaysInMonth(testData.Year, testData.Month + 1), 23, 59, 00);
			CudUniversalReferenceDataXmlGenerator.GetEndDateForXmlDate(testData, out var resultEndDate);
			Assert.AreEqual(expectedEndDate, resultEndDate);

			testData = new DateTime(2020, 10, 20);

			expectedEndDate = new DateTime(testData.Year, testData.Month, DateTime.DaysInMonth(testData.Year, testData.Month), 23, 59, 00);
			CudUniversalReferenceDataXmlGenerator.GetEndDateForXmlDate(testData, out resultEndDate);
			Assert.AreEqual(expectedEndDate, resultEndDate);
		}

		DateTime GetExpectedEndDate(DateTime date)
		{
			var searchedDate = CudExchangeRate.GetPrioPenultimateDayOfTheMonth(date);

			if (searchedDate <= date)
			{
				searchedDate = searchedDate.AddMonths(1);
			}

			return new DateTime(searchedDate.Year, searchedDate.Month, DateTime.DaysInMonth(searchedDate.Year, searchedDate.Month), 23, 59, 00);
		}
	}
}
