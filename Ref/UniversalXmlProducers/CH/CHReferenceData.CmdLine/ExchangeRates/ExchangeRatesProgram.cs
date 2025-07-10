using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Services.ExchangeRates;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.ExchangeRates
{
	class ExchangeRatesProgram
	{
		internal static void Run(string outputPath)
		{
			using (var client = new HttpClient())
			{
				var download = DownloadExchangeRates.Download(client);
				var rates = new ExchangeRatesParser(download);
				Program.PrintErrorMessage(rates.ConvertToRefXML(Path.Combine(outputPath, "RefExchangeRateZZ_CH.xml")));
			}
		}
	}
}
