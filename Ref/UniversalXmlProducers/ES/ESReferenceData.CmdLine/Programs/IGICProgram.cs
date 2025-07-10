using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class IGICProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var measureJson = DownloadJson.Download(ApplicationConfig.CanaryIslandMeasuresURL);
			var footnotesJson = DownloadJson.Download(ApplicationConfig.CanaryIslandFootnotesURL);
			var taxOrFeesJson = DownloadJson.Download(TaxOrFeeProvider.GetQueryUrlForSpanishTaxOrFee(ApplicationConfig.RefDbServiceURI, "IG", dateTimeProvider.CurrentLocalDate).AbsoluteUri);

			Program.PrintErrorMessage(new IGICParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(measureJson, footnotesJson, Path.Combine(outputPath, "RefIGICZZ_ES.xml"), taxOrFeesJson));
		}
	}
}
