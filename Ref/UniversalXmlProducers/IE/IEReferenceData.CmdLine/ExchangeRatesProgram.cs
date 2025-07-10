using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.IEReferenceData.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.CmdLine
{
	static class ExchangeRatesProgram
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "<Pending>")]
		public static void Run(string outputPath)
		{
			var downloadUrl = DownloadExchangeRates.GetLatestExchangeRateURL(ApplicationConfig.Instance.ExchangeRatesStartURL);
			var isCsv = downloadUrl.ExchangeRateUri.AbsoluteUri.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

			Dictionary<string, decimal> exchangeRates;
			var outputPathAndFile = Path.Combine(outputPath, "RefExchangeRateZZ_IE.xml");
			if (isCsv)
			{
				exchangeRates = DownloadExchangeRates.GetExchangeRatesDataFromCsv(downloadUrl.ExchangeRateUri.AbsoluteUri);
				Console.Error.WriteLine(new ExchangeRatesProducer().ConvertToXmlFile(exchangeRates, new DateTimeProvider(), downloadUrl.ExchangeRateDate, outputPathAndFile));
			}
			else
			{
				var htmlWeb = new HtmlWeb();
				var htmlDocument = htmlWeb.Load(downloadUrl.ExchangeRateUri);
				var exchangeRateDate = DownloadExchangeRates.GetMonthAndYearRatesRelateTo(htmlDocument);
				exchangeRates = DownloadExchangeRates.GetExchangeRatesData(downloadUrl.ExchangeRateUri.AbsoluteUri, htmlDocument);
				Console.Error.WriteLine(new ExchangeRatesProducer().ConvertToXmlFile(exchangeRates, new DateTimeProvider(), exchangeRateDate, outputPathAndFile));
			}
		}
	}
}
