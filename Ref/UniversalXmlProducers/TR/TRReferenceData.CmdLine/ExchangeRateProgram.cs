using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class ExchangeRateProgram
	{
		public static void Run() => RunWithOutputPath(ApplicationConfig.OutputPath);

		static void RunWithOutputPath(string outputPath)
		{
			var outputDirectory = outputPath;
			var exchangeRates = DownloadExchangeRates.Download(ApplicationConfig.ExchangeRatesURL);
			var exchangeRateparser = new ExchangeRateParser(exchangeRates);
			Parallel.Invoke(
				() => Program.PrintErrorMessage(exchangeRateparser.ConvertToXMLFile(Path.Combine(outputDirectory, "RefExchangeRateZZ_TR_Export.xml"), Business.Constants.ExchangeRateTypes.Export)),
				() => Program.PrintErrorMessage(exchangeRateparser.ConvertToXMLFile(Path.Combine(outputDirectory, "RefExchangeRateZZ_TR_Import.xml"), Business.Constants.ExchangeRateTypes.Import))
			);
		}
	}
}
