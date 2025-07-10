using System.IO;
using CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
{
	public static class ExchangeRatesProgram
	{
		public static void Run(string outputPath, IDownloadExchangeRates downloader)
		{
			var incrementalDailyRepositoryUrl = ApplicationConfig.IncrementalDailyRepositoryUrl;
			var latestTraderExportIncrementalFile = downloader.FindLatestIncrementFile(incrementalDailyRepositoryUrl);
			if (!string.IsNullOrEmpty(latestTraderExportIncrementalFile))
			{
				var url = Path.Combine(incrementalDailyRepositoryUrl, latestTraderExportIncrementalFile);
				var monetaryExchangePeriods = downloader.DownloadLatestIncrementalAndExtract(url);
				var dateTimeProvider = new DateTimeProvider();
				foreach (var monetaryExchangePeriod in monetaryExchangePeriods)
				{
					var outputFile = Path.Combine(outputPath, $"RefExchangeRateZZ_SE_{monetaryExchangePeriod.SID}.xml");
					Program.PrintErrorMessage(new ExchangeRateParser(monetaryExchangePeriod).ConvertToXMLFile(outputFile, dateTimeProvider));
				}
			}
			else
			{
				Program.PrintErrorMessage("Unable to find any Incremental Files for the Exchange Rates going back two months.");
			}
		}
	}
}
