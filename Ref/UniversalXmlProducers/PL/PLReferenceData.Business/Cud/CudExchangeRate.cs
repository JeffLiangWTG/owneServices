using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Services.Cud;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cud
{
	class CudExchangeRate
	{
		public static CudExRate GetCudExRate()
		{
			var dataToParse = GetXmlReaderEUBankExRateData();

			return GetRateDataAsOfRate(dataToParse, DateTime.UtcNow);
		}

		static CubeCube[] GetXmlReaderEUBankExRateData()
		{
			var dataToParse = DownloadEuExRate.GetEUBankExRateData(CudConstants.EUCentralBankExchangeRateXmlUrl);

			var result = Helpers.XmlParser.Deserialize<Envelope>(dataToParse);

			var retv = result.Cube;

			return retv;
		}

		protected static DateTime GetDataDateTime(DateTime targetDate)
		{
			DateTime retv;

			retv = GetPrioPenultimateDayOfTheMonth(targetDate);

			if (targetDate.Day < retv.Day)
			{
				// use date from last month
				targetDate = targetDate.AddMonths(-1);
				retv = GetPrioPenultimateDayOfTheMonth(targetDate);
			}

			return retv;
		}

		public static DateTime GetPrioPenultimateDayOfTheMonth(DateTime date)
		{
			DateTime retv = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)).AddDays(-2);

			if (retv.DayOfWeek != DayOfWeek.Saturday && retv.DayOfWeek != DayOfWeek.Sunday)
			{
				return retv; // prio penultimate day of the month is business day (Monday - Firday)
			}
			else if (retv.DayOfWeek == DayOfWeek.Saturday)
			{
				return retv.AddDays(-1); // prio penultimate day of the month is Saturday
			}

			return retv.AddDays(-2); // prio penultimate day of the month is Sunday
		}

		protected static CudExRate GetRateDataAsOfRate(CubeCube[] dataToParse, DateTime runningDate)
		{
			var dateRateDict = FlattenCubeDataToDictionary(dataToParse);

			var basicDataDate = GetDataDateTime(runningDate);
			var basicData = ParseXmlDataFromDate(dateRateDict, basicDataDate, false);

			var midMonthDate = GetFirstWorkingDayAfter14DayOfTheMonth(runningDate);
			CudExRate correctionData = null;
			if (basicData.PublicationDate < midMonthDate)
			{
				correctionData = ParseXmlDataFromDate(dateRateDict, midMonthDate, true);
			}

			var retv = GetCorrectedCurrencyData(basicData, correctionData);

			return retv;
		}

		protected static DateTime GetFirstWorkingDayAfter14DayOfTheMonth(DateTime date)
		{
			var dateToCheck = new DateTime(date.Year, date.Month, 14);

			var offset = 1;
			if (dateToCheck.DayOfWeek == DayOfWeek.Friday)
			{
				offset = 3;
			}
			else if (dateToCheck.DayOfWeek == DayOfWeek.Saturday)
			{
				offset = 2;
			}

			return dateToCheck.AddDays(offset);
		}

		public static Dictionary<DateTime, double> FlattenCubeDataToDictionary(CubeCube[] data)
		{
			var plnRateDict = new Dictionary<DateTime, double>();
			data.SelectMany(dateInfo => dateInfo.Cube, (dateInfo, currencyRateInfo) => new { dateInfo, currencyRateInfo })
				.Where(flattened => flattened.currencyRateInfo.currency == CudConstants.SupportedCurrencyCodes.PolishZloty)
				.ToList().ForEach(flattened =>
				{
					var rateDate = flattened.dateInfo.time;
					var rateValue = flattened.currencyRateInfo.rate;
					if (!plnRateDict.ContainsKey(rateDate))
					{
						plnRateDict.Add(rateDate, rateValue);
					}
				});
			return plnRateDict;
		}

		public static CudExRate ParseXmlDataFromDate(Dictionary<DateTime, double> dateRateDict, DateTime targetDate, bool isGettingMidmonthCorrectionData)
		{
			CudExRate result = new CudExRate();
			if (dateRateDict.ContainsKey(targetDate))
			{
				result.PublicationDate = targetDate;
				result.AddSingle(CudConstants.SupportedCurrencyCodes.PolishZloty, dateRateDict[targetDate], GetStartDateForThePublishedDate(targetDate, isGettingMidmonthCorrectionData));
			}
			else
			{
				if (!isGettingMidmonthCorrectionData)
				{
					if (dateRateDict.Keys.Any(k => k > targetDate))
					{
						result = ParseXmlDataFromDate(dateRateDict, targetDate.AddDays(-1), false);
					}
					else
					{
						var beginningOfTheMonth = targetDate.AddDays(1 - targetDate.Day);
						result = ParseXmlDataFromDate(dateRateDict, GetDataDateTime(beginningOfTheMonth), false);
					}
				}
				else
				{
					if (dateRateDict.Keys.Any(k => k > targetDate))
					{
						result = ParseXmlDataFromDate(dateRateDict, targetDate.AddDays(1), true);
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		protected static DateTime GetStartDateForThePublishedDate(DateTime publishedDate, bool isMidMonthCorrection)
		{
			if (isMidMonthCorrection)
			{
				return publishedDate;
			}
			else
			{
				return new DateTime(publishedDate.Year, publishedDate.Month, 1).AddMonths(1);
			}
		}

		protected static CudExRate GetCorrectedCurrencyData(CudExRate oldData, CudExRate newData)
		{
			var result = oldData;
			if (newData == null || oldData.PublicationDate > newData.PublicationDate)
			{
				result = oldData;
			}
			else
			{
				foreach (var oldCurrencyData in oldData.ExRateData)
				{
					var currency = oldCurrencyData.CurrencyCode;
					var matchingInNewData = newData.ExRateData.FirstOrDefault(single => single.CurrencyCode == currency);
					if (matchingInNewData != null)
					{
						var oldExRate = Convert.ToDecimal(oldCurrencyData.ExRate, CultureInfo.InvariantCulture);

						var upperCorrectionValue = oldExRate * CudConstants.UpperDifferenceMultiplier;
						var lowerCorrectionValue = oldExRate * CudConstants.LowerDifferenceMultiplier;

						var newExRate = Convert.ToDecimal(matchingInNewData.ExRate, CultureInfo.InvariantCulture);

						if ((upperCorrectionValue <= newExRate) ||
							(lowerCorrectionValue >= newExRate))
						{
							oldData.PublicationDate = newData.PublicationDate;
							oldCurrencyData.ExRate = matchingInNewData.ExRate;
							oldCurrencyData.StartDate = matchingInNewData.StartDate;
						}
					}
				}
			}
			return result;
		}
	}
}
