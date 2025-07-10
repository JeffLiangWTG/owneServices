using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using static CargoWise.RefDbRepo.ESReferenceData.Services.TariffOneProvider;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class TariffOneProgram
	{
		public static void Run(string outputPath)
		{
			var tariffOneProvider = new TariffOneProvider(outputPath, ApplicationConfig.TariffOneURL);
			var dataExportJson = tariffOneProvider.GetLatestDataFileName();
			var errors = tariffOneProvider.CreateOrUpdateDataFilesIfNeeded(dataExportJson);

			if (string.IsNullOrEmpty(errors))
			{
				var dateTimeProvider = new DateTimeProvider();

				var nomenclaturesJson = tariffOneProvider.GetFileContent(TariffOneFiles.nomenclatures);
				var sectionsJson = tariffOneProvider.GetFileContent(TariffOneFiles.sections);

				var pathTariffs = Path.Combine(outputPath, "RefCusTariffZZ_Tariffs_ES.xml");
				var pathNomenclatures = Path.Combine(outputPath, "RefCusTariffZZ_NomenclatureGroups_ES.xml");
				var result = new ESTariffParser(dateTimeProvider).ConvertToXMLFile(nomenclaturesJson, sectionsJson, pathTariffs, pathNomenclatures);

				Program.PrintLogMessage(result.logs);
				Program.PrintErrorMessage(result.errors);
			}
		}
	}
}
