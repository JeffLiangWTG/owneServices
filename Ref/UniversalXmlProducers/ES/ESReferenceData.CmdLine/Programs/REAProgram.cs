using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class REAProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
			var codesJson = DownloadJson.Download(ApplicationConfig.CanaryIslandCodesURL);
			var uomsJson = DownloadJson.Download(UOMProvider.GetQueryUrlForSpanishUOM(ApplicationConfig.RefDbServiceURI, dateTimeProvider.CurrentLocalDate).AbsoluteUri);

			Program.PrintErrorMessage(new REAParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(uomsJson, measureJson, codesJson, Path.Combine(outputPath, "RefCusTariffZZ_REA_ES.xml"), Path.Combine(outputPath, "RefCusCodeListZZ_REA_ES.xml")));
		}
	}
}
