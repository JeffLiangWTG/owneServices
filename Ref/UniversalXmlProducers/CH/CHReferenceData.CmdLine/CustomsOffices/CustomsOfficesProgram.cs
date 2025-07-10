using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business.CustomsOffices;
using CargoWise.RefDbRepo.CHReferenceData.Services.CustomsOffices;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.ExchangeRates
{
	class CustomsOfficesProgram
	{
		internal static void Run(string outputPath)
		{
			var outputFile = Path.Combine(outputPath, "RefCustomsOfficesZZ_CH.xml");
			var dataSource = "CH CUSCH Code List";
			using (var client = new HttpClient())
			{
				var download = DownloadCustomsOffices.DownloadAndUnzip(client);
				var rates = new CustomsOfficesParser(download);
				rates.ConvertToRefXML(outputFile, dataSource, DateTime.Today);
			}
		}
	}
}
