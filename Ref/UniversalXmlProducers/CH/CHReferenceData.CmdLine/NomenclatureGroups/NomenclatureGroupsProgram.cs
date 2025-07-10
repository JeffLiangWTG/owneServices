using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.NomenclatureGroups;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.NomenclatureGroups
{
	class NomenclatureGroupsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefNomenclatureGroupsZZ_CH.xml");
			using (var client = new HttpClient())
			{
				var download = DownloadTariffStructure.Download(client);
				var rates = new NomenclatureGroupsParser(download);
				rates.ConvertToRefXML(outputFile, DateTime.Today);
			}
		}
	}
}
