using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class MEAProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
			var uomsJson = DownloadJson.Download(UOMProvider.GetQueryUrlForSpanishUOM(ApplicationConfig.RefDbServiceURI, dateTimeProvider.CurrentLocalDate).AbsoluteUri);

			Program.PrintErrorMessage(new MEAParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(uomsJson, measureJson, Path.Combine(outputPath, "RefCusTariffZZ_MEA_ES.xml")));
		}
	}
}
