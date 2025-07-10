using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cud
{
	public static class CudUniversalReferenceDataXmlGenerator
	{
		static class MetaData
		{
			public const string DataSource = "PL Exchange Rate CUD";
			public const string RateType = "CUD";
		}

		public static bool GenerateCudUniversalReferenceData()
		{
			try
			{
				var data = CudExchangeRate.GetCudExRate();

				if (data == null)
				{
					Console.Error.WriteLine("Empty data received.");
					return false;
				}

				var defaultOutputFullPath = CommonHelper.GetOutputFilePath(CudConstants.CudUniversalReferenceDataXmlFilename);
				ExportToXmlFile(data, DateTime.UtcNow, defaultOutputFullPath);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return false;
			}

			return true;
		}

		internal static void ExportToXmlFile(CudExRate data, DateTime dateTime, string outputFullPath)
		{
			GetEndDateForXmlDate(dateTime, out var endDate);
			var refCusRateData = GetRefExchangeRateZZFromCudExRate(data, endDate);
			XmlWriterConfig.ExportToXmlFile(MetaData.DataSource, outputFullPath, XmlWriterConfig.GetRefExchangeRateWriterConfiguration(MetaData.RateType), DateTime.UtcNow, refCusRateData);
		}

		internal static IEnumerable<RefExchangeRateZZ> GetRefExchangeRateZZFromCudExRate(CudExRate data, DateTime endDate)
		{
			if (data?.ExRateData == null
				|| data.ExRateData.Count == 0)
			{
				yield break;
			}

			foreach (var item in data.ExRateData)
			{
				yield return new RefExchangeRateZZ
				{
					ZZN_Rate = Convert.ToDecimal(item.ExRate, CultureInfo.InvariantCulture),
					ZZN_RX_NKExCurrency = CudConstants.CurrencyCodeEUR,
					ZZN_StartDate = item.StartDate,
					ZZN_EndDate = endDate
				};
			};
		}

		internal static void GetEndDateForXmlDate(DateTime date, out DateTime endDate)
		{
			DateTime searchedDate = CudExchangeRate.GetPrioPenultimateDayOfTheMonth(date);

			if (searchedDate <= date)
			{
				searchedDate = searchedDate.AddMonths(1);
			}

			endDate = new DateTime(searchedDate.Year, searchedDate.Month, DateTime.DaysInMonth(searchedDate.Year, searchedDate.Month), 23, 59, 00);
		}
	}
}
