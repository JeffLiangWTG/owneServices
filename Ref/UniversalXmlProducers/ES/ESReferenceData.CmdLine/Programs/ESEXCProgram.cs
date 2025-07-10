using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine;

public static class ESEXCProgram
{
	public static void Run(string outputPath)
	{
		var dateTimeProvider = new DateTimeProvider();

		var measureJson = DownloadJson.Download(ApplicationConfig.MeasuresURL);
		var codesJson = DownloadJson.Download(ApplicationConfig.MeasuresCodesURL);
		var exciseJson = DownloadJson.Download(ApplicationConfig.CanaryIslandExciseURL);
		var uomsJson = DownloadJson.Download(UOMProvider.GetQueryUrlForSpanishUOM(ApplicationConfig.RefDbServiceURI, dateTimeProvider.CurrentLocalDate).AbsoluteUri);

		Program.PrintErrorMessage(new ESEXCParser(dateTimeProvider).ConvertToXMLFile(uomsJson, measureJson, codesJson, exciseJson, Path.Combine(outputPath, "Ref_ESEXC_ZZ_ES.xml")));
	}
}
