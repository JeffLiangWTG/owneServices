using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class VATProgram
	{
		public static void Run(string outputPath)
		{
			var dateTimeProvider = new DateTimeProvider();

			var measureJson = DownloadJson.Download(ApplicationConfig.MeasuresURL);
			var footnotesJson = DownloadJson.Download(ApplicationConfig.FootnotesURL);
			var taxOrFeesJson = DownloadJson.Download(TaxOrFeeProvider.GetQueryUrlForSpanishTaxOrFee(ApplicationConfig.RefDbServiceURI, "IV", dateTimeProvider.CurrentLocalDate).AbsoluteUri);

			Program.PrintErrorMessage(new VATParser(dateTimeProvider, ApplicationConfig.RefDbServiceURI).ConvertToXMLFile(measureJson, footnotesJson, Path.Combine(outputPath, "RefVATZZ_ES.xml"), taxOrFeesJson));
		}
	}
}
