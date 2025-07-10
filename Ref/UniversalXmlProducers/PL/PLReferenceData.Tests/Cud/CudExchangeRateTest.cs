using System;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Cud;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Cud
{
	[TestFixture]
	sealed class CudExchangeRateTest : CudExchangeRate
	{
		[TestCase(29, 12, 2020)]
		[TestCase(27, 11, 2020)]
		[TestCase(29, 10, 2020)]
		[TestCase(28, 09, 2020)]
		[TestCase(28, 08, 2020)]
		[TestCase(29, 07, 2020)]
		[TestCase(26, 06, 2020)]
		[TestCase(29, 05, 2020)]
		[TestCase(28, 04, 2020)]
		[TestCase(27, 03, 2020)]
		[TestCase(27, 02, 2020)]
		[TestCase(29, 01, 2020)]
		public void TestGetPrioPenultimateDayOfTheMonth(int day, int month, int year)
		{
			var expectedPenultimateWednesday = new DateTime(year, month, day);
			var result = GetPrioPenultimateDayOfTheMonth(expectedPenultimateWednesday);

			Assert.AreEqual(expectedPenultimateWednesday, result);
		}

		[Test]
		public void TestGetStartDateForThePublishedDate()
		{
			var testData = new DateTime(2020, 01, 1);
			var expected = new DateTime(2020, 01, 1);
			var result = GetStartDateForThePublishedDate(testData, true);

			Assert.AreEqual(expected, result);

			testData = new DateTime(2020, 01, 15);
			expected = new DateTime(2020, 01, 15);
			result = GetStartDateForThePublishedDate(testData, true);

			Assert.AreEqual(expected, result);

			testData = new DateTime(2019, 12, 30);
			expected = new DateTime(2020, 01, 1);
			result = GetStartDateForThePublishedDate(testData, false);

			Assert.AreEqual(expected, result);

			testData = new DateTime(2020, 01, 30);
			expected = new DateTime(2020, 02, 1);
			result = GetStartDateForThePublishedDate(testData, false);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestGetDataDateTime()
		{
			var dataSet1 = new DateTime(2020, 02, 26);
			DateTime result;

			result = GetDataDateTime(dataSet1);

			Assert.AreEqual(result.Month, 1);
			Assert.AreEqual(result.Day, 29);
			Assert.AreEqual(result.Year, 2020);

			var dataSet2 = new DateTime(2020, 02, 28);

			result = GetDataDateTime(dataSet2);

			Assert.AreEqual(result.Month, 2);
			Assert.AreEqual(result.Day, 27);
			Assert.AreEqual(result.Year, 2020);

			var dataSet3 = new DateTime(2020, 02, 27);

			result = GetDataDateTime(dataSet3);

			Assert.AreEqual(result.Month, 2);
			Assert.AreEqual(result.Day, 27);
			Assert.AreEqual(result.Year, 2020);
		}

		[Test]
		public void TestParseXmlDataFromDate()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 09, 24),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>() {
					new CudExRateSingle() {
						CurrencyCode = "PLN",
						ExRate = "4.5293"
					}
				}
			};

			var xmlData = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.eurofxref-hist-90d.xml"));
			var data = XmlParser.Deserialize<Envelope>(xmlData).Cube;

			var date = new DateTime(2020, 09, 24);

			var dataToParse = FlattenCubeDataToDictionary(data);
			var result = ParseXmlDataFromDate(dataToParse, date, true);

			Assert.AreEqual(expected.PublicationDate, result.PublicationDate);
			Assert.AreEqual(expected.ExRateData.Count, result.ExRateData.Count);

			for (var i = 0; i < expected.ExRateData.Count; i++)
			{
				Assert.AreEqual(expected.ExRateData[i].CurrencyCode, result.ExRateData[i].CurrencyCode);
				Assert.AreEqual(expected.ExRateData[i].ExRate, result.ExRateData[i].ExRate);
			}
		}

		[Test]
		public void TestGetFirstWorkingDayAfter14DayOfTheMonth()
		{
			var testData = new DateTime(2020, 10, 15);
			var expected = new DateTime(2020, 10, 15);
			var result = GetFirstWorkingDayAfter14DayOfTheMonth(testData);

			Assert.AreEqual(result, expected);

			testData = new DateTime(2020, 08, 14);
			expected = new DateTime(2020, 08, 17);
			result = GetFirstWorkingDayAfter14DayOfTheMonth(testData);

			Assert.AreEqual(result, expected);

			testData = new DateTime(2020, 06, 14);
			expected = new DateTime(2020, 06, 15);
			result = GetFirstWorkingDayAfter14DayOfTheMonth(testData);

			Assert.AreEqual(result, expected);

			testData = new DateTime(2020, 11, 14);
			expected = new DateTime(2020, 11, 16);
			result = GetFirstWorkingDayAfter14DayOfTheMonth(testData);

			Assert.AreEqual(result, expected);
		}

		[Test]
		public void TestGetCorrectedCurrencyData()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 10, 07),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = CudConstants.SupportedCurrencyCodes.PolishZloty,
						ExRate = "4.30"
					},
					new CudExRateSingle()
					{
						CurrencyCode = "random1",
						ExRate = "32.9999"
					},
					new CudExRateSingle()
					{
						CurrencyCode = "random2",
						ExRate = "3.5434"
					}
				}
			};

			var oldData = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 10, 06),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = CudConstants.SupportedCurrencyCodes.PolishZloty,
						ExRate = "4.30"
					},
					new CudExRateSingle()
					{
						CurrencyCode = "random1",
						ExRate = "2.5423"
					},
					new CudExRateSingle()
					{
						CurrencyCode = "random2",
						ExRate = "5.5423"
					}
				}
			};

			var newData = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 10, 07),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = CudConstants.SupportedCurrencyCodes.PolishZloty,
						ExRate = "4.35"
					},
					new CudExRateSingle()
					{
						CurrencyCode = "random1",
						ExRate = "32.9999"
					},
					new CudExRateSingle()
					{
						CurrencyCode = "random2",
						ExRate = "3.5434"
					}
				}
			};

			var result = GetCorrectedCurrencyData(oldData, newData);

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.PublicationDate, result.PublicationDate);
			Assert.AreEqual(expected.ExRateData.Count, result.ExRateData.Count);

			for (var i = 0; i < expected.ExRateData.Count; i++)
			{
				Assert.AreEqual(expected.ExRateData[i].CurrencyCode, result.ExRateData[i].CurrencyCode);
				Assert.AreEqual(expected.ExRateData[i].ExRate, result.ExRateData[i].ExRate);
			}
		}
		[Test]
		public void TestGetRateDataAsOfRate_MidMonthUpdate()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var dataToParse = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.midMonthUpdateData.xml"));
			var data = XmlParser.Deserialize<Envelope>(dataToParse).Cube;

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 11, 16),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = "PLN",
						ExRate = "9.9999",
						StartDate = new DateTime(2020, 11, 16)
					}
				}
			};

			var runningDate = new DateTime(2020, 11, 16); // monday
			var result = GetRateDataAsOfRate(data, runningDate);

			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);

			runningDate = new DateTime(2020, 11, 17);
			result = GetRateDataAsOfRate(data, runningDate);

			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);
		}

		[Test]
		public void TestGetRateDataAsOfRate()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var dataToParse = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.eurofxref-hist-90d.xml"));
			var data = XmlParser.Deserialize<Envelope>(dataToParse).Cube;

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 09, 28),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = "PLN",
						ExRate = "4.5502",
						StartDate = new DateTime(2020, 10, 1)
					}
				}
			};

			var runningDate = new DateTime(2020, 09, 29);
			var result = GetRateDataAsOfRate(data, runningDate);

			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);

			runningDate = new DateTime(2020, 10, 09);
			result = GetRateDataAsOfRate(data, runningDate);

			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);

			expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 08, 28),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = "PLN",
						ExRate = "4.3921",
						StartDate = new DateTime(2020, 09, 1)
					}
				}
			};

			runningDate = new DateTime(2020, 09, 15);
			result = GetRateDataAsOfRate(data, runningDate);

			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);

			runningDate = new DateTime(2020, 09, 27);
			result = GetRateDataAsOfRate(data, runningDate);

			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);
		}

		void CheckIfExpectedAndResultForCudExRateAreTheSame(CudExRate expected, CudExRate result)
		{
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.ExRateData);
			Assert.IsNotNull(result.PublicationDate);
			Assert.AreNotSame(DateTime.MinValue, result.PublicationDate);
			Assert.AreEqual(expected.PublicationDate, result.PublicationDate);
			Assert.AreEqual(expected.ExRateData.Count, result.ExRateData.Count);

			for (var i = 0; i < expected.ExRateData.Count; i++)
			{
				Assert.AreEqual(expected.ExRateData[i].CurrencyCode, result.ExRateData[i].CurrencyCode);
				Assert.AreEqual(expected.ExRateData[i].StartDate, result.ExRateData[i].StartDate);
				Assert.AreEqual(expected.ExRateData[i].ExRate, result.ExRateData[i].ExRate);
			}
		}

		[Test]
		public void TestGetRateDataAsOfRate_EndOfMonthMissingData_WithNextDaysData()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var dataToParse = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.missingDataEndOfMonthUpdateData_withNextDaysData.xml"));
			var data = XmlParser.Deserialize<Envelope>(dataToParse).Cube;

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 09, 27),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = "PLN",
						ExRate = "4.5502",
						StartDate = new DateTime(2020, 10, 01)
					}
				}
			};

			var runningDate = new DateTime(2020, 09, 28);
			var result = GetRateDataAsOfRate(data, runningDate);
			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);
		}

		[Test]
		public void TestGetRateDataAsOfRate_EndOfMonthMissingData_WithoutNextDaysData()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var dataToParse = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.missingDataEndOfMonthUpdateData_withoutNextDaysData.xml"));
			var data = XmlParser.Deserialize<Envelope>(dataToParse).Cube;

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 08, 28),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = "PLN",
						ExRate = "4.3921",
						StartDate = new DateTime(2020, 09, 01)
					}
				}
			};

			var runningDate = new DateTime(2020, 09, 25);
			var result = GetRateDataAsOfRate(data, runningDate);
			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);

			runningDate = new DateTime(2020, 09, 29);
			result = GetRateDataAsOfRate(data, runningDate);
			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);
		}

		[Test]
		public void TestGetRateDataAsOfRate_MidMonthUpdateMissingData()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var dataToParse = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.missingMidMonthUpdateData.xml"));
			var data = XmlParser.Deserialize<Envelope>(dataToParse).Cube;

			var expected = new CudExRate()
			{
				PublicationDate = new DateTime(2020, 10, 29),
				ExRateData = new System.Collections.Generic.List<CudExRateSingle>()
				{
					new CudExRateSingle()
					{
						CurrencyCode = "PLN",
						ExRate = "1.1111",
						StartDate = new DateTime(2020, 11, 01)
					}
				}
			};

			var runningDate = new DateTime(2020, 11, 16);
			var result = GetRateDataAsOfRate(data, runningDate);
			CheckIfExpectedAndResultForCudExRateAreTheSame(expected, result);
		}
	}
}
