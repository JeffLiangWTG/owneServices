using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.RateCodes
{
	class RateCodesProgram
	{
		internal static void Run(string outputPath)
		{
			var feeOutputFile = Path.Combine(outputPath, "RefRateCodesZZ_CH_FEE.xml");
			var adtOutputFile = Path.Combine(outputPath, "RefRateCodesZZ_CH_ADT.xml");
			using (var client = new HttpClient())
			{
				var masterdataDownload = DownloadMasterdata.DownloadAndUnzip(client);
				new FeeRateCodesParser(masterdataDownload).ConvertToRefXML(feeOutputFile);
				new AdditionalTaxRateCodesParser(masterdataDownload).ConvertToRefXML(adtOutputFile);
			}
		}
	}
}
