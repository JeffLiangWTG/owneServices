using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.Tariffs
{
	class AdditionalTaxesTariffsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefAdditionalTaxesTariffsZZ_CH.xml");
			using (var client = new HttpClient())
			{
				var masterDataDownload = DownloadMasterdata.DownloadAndUnzip(client);
				var keyStructureAdditionalTaxesDownload = DownloadKeyStructureAdditionalTaxes.Download(client);
				var parser = new AdditionalTaxesTariffsParser(masterDataDownload, keyStructureAdditionalTaxesDownload, DateTime.Today);
				parser.ConvertToRefXML(outputFile);
			}
		}
	}
}
