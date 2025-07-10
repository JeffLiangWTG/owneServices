using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.Tariffs
{
	class ExportTariffsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefExportTariffsZZ_CH.xml");
			using (var client = new HttpClient())
			{
				var masterDataDownload = DownloadPassarMasterData.DownloadAndUnzip(client);
				var tariffStructureDownload = DownloadTariffStructure.Download(client);
				var keyStructureDownload = DownloadKeyStructureExport.Download(client);
				var permitInformationDownload = DownloadPermitInformationExport.Download(client);
				var nonCustomsLawInformationDownload = DownloadNonCustomsLawInformationExport.Download(client);
				var parser = new ExportTariffsParser(masterDataDownload, tariffStructureDownload, keyStructureDownload, permitInformationDownload, nonCustomsLawInformationDownload, DateTime.Today);
				parser.ConvertToRefXML(outputFile);
			}
		}
	}
}
