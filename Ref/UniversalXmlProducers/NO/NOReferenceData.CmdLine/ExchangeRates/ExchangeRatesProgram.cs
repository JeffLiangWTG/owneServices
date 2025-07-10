using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.NOReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.ExchangeRates
{
	static class ExchangeRatesProgram
	{
		internal static void Run()
		{
			var resourceSearchUrl = new System.Uri(ApplicationConfig.ResourceSearchUrl + ApplicationConfig.ResourceExchangeRatesFilename);
			var client = new HttpClient();

			var (errors, xmlData, resourceData) = DownloadContent.Download<omregningKursListe>(client, resourceSearchUrl, "CargoWise.RefDbRepo.NOReferenceData.Services.ExchangeRates.valutakurs.xsd");
			if (!Program.PrintErrorMessage(errors))
			{
				var outputFilePath = Path.Combine(ApplicationConfig.OutputDirectory, "RefExchangeRateZZ_NO.xml");
				ExchangeRateParser.ConvertToXmlFile(xmlData, resourceData.LastModified, outputFilePath);
			}
		}
	}
}
