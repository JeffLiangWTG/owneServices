using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine;

public static class CANEXCProgram
{
	public static void Run(string outputPath)
	{
		var dateTimeProvider = new DateTimeProvider();

		var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
		var codesJson = DownloadJson.Download(ApplicationConfig.CanaryIslandCodesURL);
		var exciseJson = DownloadJson.Download(ApplicationConfig.CanaryIslandExciseURL);
		var uomsJson = DownloadJson.Download(UOMProvider.GetQueryUrlForSpanishUOM(ApplicationConfig.RefDbServiceURI, dateTimeProvider.CurrentLocalDate).AbsoluteUri);

		Program.PrintErrorMessage(new CANEXCParser(dateTimeProvider).ConvertToXMLFile(uomsJson, measureJson, codesJson, exciseJson, Path.Combine(outputPath, "Ref_CANEXC_ZZ_ES.xml")));
	}
}
