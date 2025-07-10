using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class LocationsProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();
			var locationsToExport = new LocationsProvider().GetLocationItems(ApplicationConfig.LocationsURL, ApplicationConfig.ElementoQueryAeatURL, dateTimeProvider.CurrentLocalDate);
			Program.PrintErrorMessage(new LocationsParser(dateTimeProvider).ConvertRecordsToXMLFile(locationsToExport, Path.Combine(outputPath, "Ref_Locations_ZZ_ES.xml")));
		}
	}
}
