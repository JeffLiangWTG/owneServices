using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.CodeLists
{
	class PassarCodeListsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefPassarCodeListsZZ_CH.xml");
			using (var client = new HttpClient())
			{
				var download = DownloadPassarCodeLists.DownloadAndUnzip(client);
				var parser = new PassarCodeListsParser(download);
				parser.ConvertToRefXML(outputFile, DateTime.Now);
				ParseAndConvertConditionTypesToXML(outputPath, download);
			}
		}

		static void ParseAndConvertConditionTypesToXML(string outputPath, DownloadResult download)
		{
			var parser = new PassarConditionTypesParser(download);
			parser.ConvertToRefXML(outputPath, DateTime.Today);
		}
	}
}
