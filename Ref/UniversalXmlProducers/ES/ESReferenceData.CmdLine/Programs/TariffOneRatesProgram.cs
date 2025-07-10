using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using static CargoWise.RefDbRepo.ESReferenceData.Services.TariffOneProvider;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class TariffOneRatesProgram
	{
		public static void Run(string outputPath)
		{
			var tariffOneProvider = new TariffOneProvider(outputPath, ApplicationConfig.TariffOneURL);
			var dataExportJson = tariffOneProvider.GetLatestDataFileName();
			var errors = tariffOneProvider.CreateOrUpdateDataFilesIfNeeded(dataExportJson);

			if (string.IsNullOrEmpty(errors))
			{
				var dateTimeProvider = new DateTimeProvider();

				var measuresJson = tariffOneProvider.GetFileStream(TariffOneFiles.measures);

				var pathTariffRates = Path.Combine(outputPath, "RefCusTariffZZ_Rates_ES.xml");
				var result = new ESTariffRatesParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(measuresJson, pathTariffRates);

				Program.PrintLogMessage(result.logs);
				Program.PrintErrorMessage(result.errors);
			}
		}
	}
}
