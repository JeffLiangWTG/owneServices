using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class AIEMProgram
	{
		public static void Run(string outputPath)
		{
			var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
			var footnotesJson = DownloadJson.Download(ApplicationConfig.CanaryIslandFootnotesURL);
			Program.PrintErrorMessage(new AIEMParser(new DateTimeProvider(), ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(measureJson, footnotesJson, Path.Combine(outputPath, "RefAIEMZZ_ES.xml")));
		}
	}
}
