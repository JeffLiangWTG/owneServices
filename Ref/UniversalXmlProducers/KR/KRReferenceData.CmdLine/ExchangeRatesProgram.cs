using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class ExchangeRatesProgram
	{
		public static void Run(string outputPath)
		{
			var today = DateTime.Now;

			Task.WhenAll(
				ConvertRates(today, Path.Combine(outputPath, Constants.OutputFileName.ExportExchangeRate), Constants.DataSources.ExportExchangeRate, ExchangeRateTypes.Export),
				ConvertRates(today, Path.Combine(outputPath, Constants.OutputFileName.ImportExchangeRate), Constants.DataSources.ImportExchangeRate, ExchangeRateTypes.Import)).Wait();
		}

		static async Task ConvertRates(DateTime requestDate, string destinationFileName, string dataSource, ExchangeRateTypes exportOrImport)
		{
			using (var client = new HttpClient())
			{
				var ratesXML = await DownloadExchangeRates.Download(exportOrImport, requestDate, client);
				var exchangeRateParser = new ExchangeRateParser(ratesXML);
				exchangeRateParser.ConvertToXMLFile(destinationFileName, dataSource, exportOrImport);
			}
		}
	}
	
}
