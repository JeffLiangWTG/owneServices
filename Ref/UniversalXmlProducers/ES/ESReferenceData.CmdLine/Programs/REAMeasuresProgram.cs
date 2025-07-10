using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class REAMeasuresProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
			var uomsJson = DownloadJson.Download(UOMProvider.GetQueryUrlForSpanishUOM(ApplicationConfig.RefDbServiceURI, dateTimeProvider.CurrentLocalDate).AbsoluteUri);

			Program.PrintErrorMessage(new REAMeasuresParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(uomsJson, measureJson, Path.Combine(outputPath, "RefCusTariffZZ_REA_Measures_ES.xml")));
		}
	}
}
