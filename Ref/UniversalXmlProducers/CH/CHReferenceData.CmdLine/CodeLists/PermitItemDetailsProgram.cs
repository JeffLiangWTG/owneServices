using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.CodeLists
{
	class PermitItemDetailsProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefPermitItemDetailsZZ_CH.xml");
			using (var client = new HttpClient())
			{
				var download = DownloadPermitItemDetails.DownloadAndUnzip(client);
				var parser = new PermitItemDetailsParser(download);
				parser.ConvertToRefXML(outputFile, DateTime.Now);
			}
		}
	}
}
