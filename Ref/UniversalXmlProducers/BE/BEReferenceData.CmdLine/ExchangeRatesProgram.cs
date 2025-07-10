using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	class ExchangeRatesProgram
	{
		public static void Run()
		{
			var today = DateTime.Now;
			var currentYear = today.Year;
			var currentMonth = today.Month;
			var startDate = new DateTime(currentYear, currentMonth, 1);
			var endDate = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth), 23, 59, 59);

			var downloadUrl = "https://eservices.minfin.fgov.be/extTariffBrowser/FileResourceForHomePageServlet?fname=listed_currencies.xlsx&amp;lang=EN";

			var downloadFileAbsolutePath = WebClientHelper.DownloadFile(downloadUrl, "listed_currencies.xlsx");
			var currentMonthList = ExcelParser.ReadExchangeRateXlsIntoResults(downloadFileAbsolutePath, startDate, endDate);
			var nextMonthList = ExcelParser.ReadExchangeRateXlsIntoResults(downloadFileAbsolutePath, startDate.AddMonths(1), endDate.AddMonths(1));
			var listed = currentMonthList.Union(nextMonthList).ToList();
			File.Delete(downloadFileAbsolutePath);

			downloadUrl = "https://eservices.minfin.fgov.be/extTariffBrowser/FileResourceForHomePageServlet?fname=unlisted_currencies.xlsx&amp;lang=EN";
			downloadFileAbsolutePath = WebClientHelper.DownloadFile(downloadUrl, "unlisted_currencies.xlsx");

			currentMonthList = ExcelParser.ReadExchangeRateXlsIntoResults(downloadFileAbsolutePath, startDate, endDate);
			nextMonthList = ExcelParser.ReadExchangeRateXlsIntoResults(downloadFileAbsolutePath, startDate.AddMonths(1), endDate.AddMonths(1));
			var unlisted = currentMonthList.Union(nextMonthList).ToList();
			File.Delete(downloadFileAbsolutePath);

			XMLGeneration.GenerateXml("BE Exchange Rates", "RefExchangeRateZZ_BE.xml", listed.Union(unlisted).ToList());
		}
	}
}
