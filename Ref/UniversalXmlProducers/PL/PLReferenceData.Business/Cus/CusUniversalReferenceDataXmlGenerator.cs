using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cus
{
	public static class CusUniversalReferenceDataXmlGenerator
	{
		static class MetaData
		{
			public const string DataSource = "PL Exchange Rate CUS";
			public const string RateType = "CUS";
		}

		public static bool GenerateCusUniversalReferenceData()
		{
			try
			{
				var dataTable = CusExchangeRate.GetExchangeRateXmlData();

				if (dataTable == null)
				{
					Console.Error.WriteLine("Empty data received.");
					return false;
				}

				var defaultOutputFullPath = CommonHelper.GetOutputFilePath(CusConstants.CusUniversalReferenceDataXmlFilename);
				ExportToXmlFile(dataTable, DateTime.UtcNow, defaultOutputFullPath);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return false;
			}

			return true;
		}

		internal static void ExportToXmlFile(tabela_kursow dataTable, DateTime dateTime, string outputFullPath)
		{
			GetStartAndEndDateForXmlDate(dateTime, out var startDate, out var endDate);
			var refCusRateData = GetRefExchangeRateZZFromTabelaKursow(dataTable, startDate, endDate);
			XmlWriterConfig.ExportToXmlFile(MetaData.DataSource, outputFullPath, XmlWriterConfig.GetRefExchangeRateWriterConfiguration(MetaData.RateType), DateTime.UtcNow, refCusRateData);
		}

		internal static IEnumerable<RefExchangeRateZZ> GetRefExchangeRateZZFromTabelaKursow(tabela_kursow data, DateTime startDate, DateTime endDate)
		{
			if (data?.pozycja == null
				|| data.pozycja.Count == 0)
			{
				yield break;
			}

			foreach (var item in data.pozycja)
			{
				yield return new RefExchangeRateZZ
				{
					ZZN_AsPublished = item.kurs_sredni,
					ZZN_Rate = Convert.ToDecimal(item.kurs_sredni.Replace(',', '.'), CultureInfo.InvariantCulture) / item.przelicznik,
					ZZN_RX_NKExCurrency = item.kod_waluty,
					ZZN_StartDate = startDate,
					ZZN_EndDate = endDate,
				};
			};
		}

		internal static void GetStartAndEndDateForXmlDate(DateTime date, out DateTime startDate, out DateTime endDate)
		{
			var searchedDate = CusExchangeRate.GetPenultimateWednesday(date);
			if (searchedDate <= date)
			{
				searchedDate = searchedDate.AddMonths(1);
			}
			startDate = new DateTime(searchedDate.Year, searchedDate.Month, 1);
			endDate = new DateTime(searchedDate.Year, searchedDate.Month, DateTime.DaysInMonth(searchedDate.Year, searchedDate.Month));
		}
	}
}
