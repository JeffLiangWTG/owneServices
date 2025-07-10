using System.IO;
using CargoWise.RefDbRepo.JPReferenceData.Business;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.CmdLine
{
	[StartupArgument(Business.Constants.ProgramFunctions.ExchangeRates)]
	public static class ExchangeRateProgram
	{
		public static void Run()
		{
			var exchangeRate = new DownloadExchangeRates();
			var (errors, exchangeRates, pdfPath) = exchangeRate.Download(AppConfig.ExchangeRate.Url, new HttpClientHelper());
			if (string.IsNullOrEmpty(errors))
			{
				var outputPathAndFile = Path.Combine(AppConfig.Shared.OutputDirectory, "RefExchangeRateZZ_JP.xml");

				var parser = new ExchangeRateParser();
				errors = parser.ConvertToXMLFile(exchangeRates, outputPathAndFile, pdfPath);
			}
			ErrorWriter.WriteError(errors);
		}
	}
}
