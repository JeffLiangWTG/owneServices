using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.DEReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.DEReferenceData.Services.ExchangeRates;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine
{
	class ExchangeRatesProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var today = DateTime.Now;
			var startDate = new DateTime(today.Year, today.Month, 1);
			var endDate = startDate.AddMonths(1);

			var startDataForIATType = new DateTime(today.Year, today.Month, 10);
			var endDateForIATType = startDataForIATType.AddMonths(1);

			Task.WhenAll(
				ConvertRates(startDate, endDate, Path.Combine(outputPath, "RefExchangeRateZZ_DE_Listed.xml"), "DE Listed Exchange Rates", DownloadExchangeRates.ListedKursartValue, dateTimeProvider),
				ConvertRates(startDate, endDate, Path.Combine(outputPath, "RefExchangeRateZZ_DE_UnListed.xml"), "DE UnListed Exchange Rates", DownloadExchangeRates.UnListedKursartValue, dateTimeProvider),
				ConvertRates(startDataForIATType, endDateForIATType, Path.Combine(outputPath, "RefExchangeRateZZ_DE_IATA.xml"), "DE IATA Exchange Rates", DownloadExchangeRates.IATAKursartValue, dateTimeProvider)).Wait();
		}

		static async Task ConvertRates(DateTime startDate, DateTime endDate, string destFileName, string dataSource, string kursartValue, DateTimeProvider dateTimeProvider)
		{
			using (var client = new HttpClient())
			{
				var ratesXML = await DownloadExchangeRates.Download(kursartValue, startDate, endDate, client, dateTimeProvider);
				var exchangerateParser = new ExchangeRateParser(ratesXML);
				Program.PrintErrorMessage(exchangerateParser.ConvertToXMLFile(destFileName, dataSource, kursartValue));
			}
		}
	}
}
