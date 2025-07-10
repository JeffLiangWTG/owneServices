using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.TradeGroups;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.Tariffs
{
	class ImportTariffsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefImportTariffsZZ_CH.xml");
			using (var client = new HttpClient())
			{
				var masterDataDownload = DownloadMasterdata.DownloadAndUnzip(client);
				var tariffStructureDownload = DownloadTariffStructure.Download(client);
				var keyStructureDownload = DownloadKeyStructureImport.Download(client);
				var permitInformationDownload = DownloadPermitInformationImport.Download(client);
				var nonCustomsLawInformationDownload = DownloadNonCustomsLawInformationImport.Download(client);
				var customsFacilitiesDownload = DownloadCustomsFacilities.Download(client);
				var countryGroupsDownload = DownloadTradeGroups.DownloadAndUnzip(client);
				var parser = new ImportTariffsParser(masterDataDownload, tariffStructureDownload, keyStructureDownload, permitInformationDownload, nonCustomsLawInformationDownload, customsFacilitiesDownload, countryGroupsDownload, DateTime.Today);
				parser.ConvertToRefXML(outputFile);
			}
		}
	}
}
