using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class ExchangeRatesProgram
	{
		public static void Run(string outputPath)
		{
			var xmlDocument = DownloadExchangeRates.Download(ApplicationConfig.ExchangeRatesURL);
			Program.PrintErrorMessage(new ExchangeRatesParser(new DateTimeProvider()).ConvertRatesForPenultimateWednesdayToXMLFile(Path.Combine(outputPath, "RefExchangeRateZZ_ES.xml"), xmlDocument));
		}
	}
}
