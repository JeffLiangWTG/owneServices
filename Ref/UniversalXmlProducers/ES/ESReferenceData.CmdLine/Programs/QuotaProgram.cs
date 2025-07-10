using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class QuotaProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
			var footnotesJson = DownloadJson.Download(ApplicationConfig.CanaryIslandFootnotesURL);
			var uomsJson = DownloadJson.Download(UOMProvider.GetQueryUrlForSpanishUOM(ApplicationConfig.RefDbServiceURI, dateTimeProvider.CurrentLocalDate).AbsoluteUri);

			Program.PrintErrorMessage(new QuotaParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(uomsJson, measureJson, footnotesJson, Path.Combine(outputPath, "RefCusTariffZZ_Quotas_ES.xml")));
		}
	}
}
