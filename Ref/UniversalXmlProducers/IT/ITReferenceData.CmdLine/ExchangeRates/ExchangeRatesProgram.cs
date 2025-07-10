using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.ITReferenceData.Services;

namespace CargoWise.RefDbRepo.ITReferenceData.CmdLine.ExchangeRates
{
	static class ExchangeRatesProgram
	{
		internal static void Run(string outputPath)
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			Console.WriteLine($"Start parsing monthly rates...");

			var meta = new ExchangeRateMetaData();
			meta.Source = ApplicationConfig.FetchUrl;
			meta.OutputPath = ApplicationConfig.OutputDirectory;
			meta.OutputFilePrefix = "RefExchangeRateZZ_IT_";
			meta.DataSourceName = "IT Exchange Rates";

			IExchangeRateMetaDataFetcher metaFetcher = new ItalyExchangeRateMetaDataFetcher(meta);
			meta = metaFetcher.Fetch();

			IExchangeRateMetaDataParser metaParser = new ItalyExchangeRateMetaDataParser(meta);
			meta = metaParser.Parse();
			meta = metaFetcher.FetchPublicationTime();

			IExchangeRateDataFetcher dataFetcher = new ItalyExchangeRateDataFetcher(meta);
			var data = dataFetcher.Fetch();
			var dataParser = new ItalyExchangeRateDataParser(meta, data);

			data = dataParser.Parse();
			var dataExporter = new ItalyExchangeRateDataExporter(meta, data);
			dataExporter.Export();

			Console.WriteLine("\tversion:" + meta.Version);
			Console.WriteLine("\tpdf url:" + meta.DataLocation);
			Console.WriteLine("\tpublication time:" + meta.PublicationTime);
			Console.WriteLine($"Finish parsing monthly rates. xml file is written in {meta.OutputPath}.");

			// daily rate used for ones missing from monthly pdf
			Console.WriteLine($"\n\nStart parsing daily rates...");

			var dailyRateMeta = new ExchangeRateMetaData();
			dailyRateMeta.OutputPath = ApplicationConfig.OutputDirectory;
			dailyRateMeta.OutputFilePrefix = "RefExchangeRateZZ_IT_Cont_";
			dailyRateMeta.DataSourceName = "Daily IT Exchange Rates";
			dailyRateMeta.DataLocation = ApplicationConfig.DailyRateUrl;

			var dailyRateParser = new ItalyDailyExchangeRateParser(dailyRateMeta);
			dailyRateParser.ParseMeta();

			var yearMonthForMonthly = meta.Version.Substring(0, 4);
			var yearMonthForDaily = dailyRateMeta.Version.Substring(0, 4);

			// when monthly rates parsed successfully and it has the same year and month for daily rates then parse the daily rates
			if (data.ProcessedData.Count > 0 && yearMonthForMonthly.Equals(yearMonthForDaily, StringComparison.OrdinalIgnoreCase))
			{
				var dailyRates = dailyRateParser.Parse();

				var missingCurrencyList = dailyRates.Where(x => !data.ProcessedData.Select(y => y.ZZN_RX_NKExCurrency).Contains(x.ZZN_RX_NKExCurrency));

				if (missingCurrencyList.Any())
				{
					var dailyRateData = new ExchangeRateData(null, missingCurrencyList.ToList());
					var dailyDataExporter = new ItalyExchangeRateDataExporter(dailyRateMeta, dailyRateData);
					dailyDataExporter.Export();

					Console.WriteLine("\tversion:" + dailyRateMeta.Version);
					Console.WriteLine("\tsource url:" + dailyRateMeta.DataLocation);
					Console.WriteLine("\tpublication time:" + dailyRateMeta.PublicationTime);
					Console.WriteLine($"Finish parsing daily rates. xml file is written in {dailyRateMeta.OutputPath}. \nPress any key to exit.");
				}
			}
		}
	}
}
