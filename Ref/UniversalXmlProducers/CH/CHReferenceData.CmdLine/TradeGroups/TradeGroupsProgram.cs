using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.TradeGroups;
using CargoWise.RefDbRepo.CHReferenceData.Services.TradeGroups;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.TradeGroups
{
	class TradeGroupsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefTradeGroupsZZ_CH.xml");
			var dataSource = "CH Trade Groups";
			using (var client = new HttpClient())
			{
				var download = DownloadTradeGroups.DownloadAndUnzip(client);
				var parser = new TradeGroupsParser(download);
				parser.ConvertToRefXML(outputFile, dataSource, DateTime.Today);
			}
		}
	}
}
