using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Cus;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Cus
{
	[TestFixture]
	sealed class CusUniversalReferenceDataXmlGeneratorTest
	{
		[Test]
		public void GenerateCusUniversalReferenceDataFileContentTest()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var expectedDataTable = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cus.TestFiles.Output.UniversalReferenceData.xml"));

			using (var streamReaderA = new StreamReader(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cus.TestFiles.Input.a141z200722.xml")))
			using (var streamReaderB = new StreamReader(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cus.TestFiles.Input.b029z200722.xml")))
			{
				var tableAXmlTextReader = XDocument.Load(streamReaderA);
				var tableBXmlTextReader = XDocument.Load(streamReaderB);

				if (null == tableAXmlTextReader)
				{
					Assert.Fail("Could not read from table A test file.");
				}
				if (null == tableBXmlTextReader)
				{
					Assert.Fail("Could not read from table B test file.");
				}

				var dataTableA = XmlParser.DeserializeFromString<tabela_kursow>(tableAXmlTextReader.ToString());
				var dataTableB = XmlParser.DeserializeFromString<tabela_kursow>(tableBXmlTextReader.ToString());

				var dataTable = new tabela_kursow
				{
					pozycja = new List<tabela_kursowPozycja>(),
					data_publikacji = dataTableA.data_publikacji
				};

				dataTable.pozycja.AddRange(dataTableA.pozycja);
				dataTable.pozycja.AddRange(dataTableB.pozycja);

				var tempFile = Path.GetTempFileName();
				CusUniversalReferenceDataXmlGenerator.ExportToXmlFile(dataTable, dataTable.data_publikacji, tempFile);
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
		}

		[Test]
		public void TestGetRefExchangeRateZZFromTabelaKursow()
		{
			Assert.Multiple(() =>
			{
				var testDate = new DateTime(2020, 09, 10);

				SetExpectedStartAndEndDate(testDate, out var expectedStartDate, out var expectedEndDate);
				var input = new tabela_kursow();
				var result = CusUniversalReferenceDataXmlGenerator.GetRefExchangeRateZZFromTabelaKursow(input, expectedStartDate, expectedEndDate).ToArray();
				Assert.That(result, Is.Not.Null, "with empty input empty list should be returned");
				Assert.That(result.Length, Is.EqualTo(0), "with empty input empty list should be returned");

				input.pozycja = new List<tabela_kursowPozycja>()
				{
					new tabela_kursowPozycja()
					{
						kod_waluty = "ABC",
						kurs_sredni = "123,4",
						nazwa_waluty = "ASD",
						przelicznik = 100.0m
					},
					new tabela_kursowPozycja()
					{
						kod_waluty = "POI",
						kurs_sredni = "987,6",
						nazwa_waluty = "PLM",
						przelicznik = 1.0m
					},
					new tabela_kursowPozycja()
					{
						kod_waluty = "ZXC",
						kurs_sredni = "534,12",
						nazwa_waluty = "ZAQ",
						przelicznik = 10000.0m
					}
				};

				result = CusUniversalReferenceDataXmlGenerator.GetRefExchangeRateZZFromTabelaKursow(input, expectedStartDate, expectedEndDate).ToArray();
				var resultItemCound = result.Length;
				Assert.That(resultItemCound, Is.EqualTo(3), "For each tabela_kursowPozycja 1 RefCusRateZZ should be generated");
				for (var i = 0; i < resultItemCound; i++)
				{
					var inputItem = input.pozycja[i];
					Assert.That(result[i].ZZN_RX_NKExCurrency, Is.EqualTo(inputItem.kod_waluty), $"{i} - ZZN_RX_NKExCurrency");
					var expectedCurrencyRate = Convert.ToDecimal(inputItem.kurs_sredni.Replace(',', '.'), CultureInfo.InvariantCulture) / inputItem.przelicznik;
					Assert.That(result[i].ZZN_Rate, Is.EqualTo(expectedCurrencyRate), $"{i} - ZZN_Rate");
					Assert.That(result[i].ZZN_AsPublished, Is.EqualTo(inputItem.kurs_sredni), $"{i} - ZZN_AsPublished");
					Assert.That(result[i].ZZN_StartDate, Is.EqualTo(expectedStartDate), $"{i} - ZZN_StartDate");
					Assert.That(result[i].ZZN_EndDate, Is.EqualTo(expectedEndDate), $"{i} - ZZN_EndDate");
				}
			});
		}

		[Test]
		public void TestGetStartAndEndDateForXmlDate()
		{
			var testDate = new DateTime(2020, 09, 10);

			SetExpectedStartAndEndDate(testDate, out var expectedStartDate, out var expectedEndDate);
			CusUniversalReferenceDataXmlGenerator.GetStartAndEndDateForXmlDate(testDate, out var resultStartDate, out var resultEndDate);

			Assert.AreEqual(expectedStartDate, resultStartDate);
			Assert.AreEqual(expectedEndDate, resultEndDate);

			testDate = new DateTime(2020, 09, 17);

			SetExpectedStartAndEndDate(testDate, out expectedStartDate, out expectedEndDate);
			CusUniversalReferenceDataXmlGenerator.GetStartAndEndDateForXmlDate(testDate, out resultStartDate, out resultEndDate);

			Assert.AreEqual(expectedStartDate, resultStartDate);
			Assert.AreEqual(expectedEndDate, resultEndDate);

			testDate = new DateTime(2020, 09, 30);

			SetExpectedStartAndEndDate(testDate, out expectedStartDate, out expectedEndDate);
			CusUniversalReferenceDataXmlGenerator.GetStartAndEndDateForXmlDate(testDate, out resultStartDate, out resultEndDate);

			Assert.AreEqual(expectedStartDate, resultStartDate);
			Assert.AreEqual(expectedEndDate, resultEndDate);
		}

		void SetExpectedStartAndEndDate(DateTime date, out DateTime expectedStartDate, out DateTime expectedEndDate)
		{
			if (date >= CusExchangeRate.GetPenultimateWednesday(date))
			{
				expectedStartDate = new DateTime(date.Year, date.Month + 1, 1);
				expectedEndDate = new DateTime(date.Year, date.Month + 1, DateTime.DaysInMonth(date.Year, date.Month + 1));
			}
			else
			{
				expectedStartDate = new DateTime(date.Year, date.Month, 1);
				expectedEndDate = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
			}
		}
	}
}
