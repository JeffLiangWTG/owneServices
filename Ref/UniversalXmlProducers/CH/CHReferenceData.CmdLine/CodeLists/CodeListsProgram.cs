using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists;

namespace CargoWise.RefDbRepo.CHReferenceData.CmdLine.CodeLists
{
	class CodeListsProgram
	{
		internal static void Run(string outputPath)
		{
			using (var client = new HttpClient())
			{
				var download = DownloadCodeLists.DownloadAndUnzip(client);
				ParseAndConvertToXML(outputPath, download);
				ParseAndConvertNonCustomsLawTypeConditionsToXML(outputPath, download);
				ParseAndConvertPermitauthorityConditionsToXML(outputPath, download);
			}
		}

		static void ParseAndConvertToXML(string outputPath, DownloadResult download)
		{
			var parser = new CodeListsParser(download);
			var outputFile = Path.Combine(outputPath, "RefCusCodeListZZ_CH_.xml");
			parser.ConvertToRefXML(outputFile, DateTime.Today);
		}

		static void ParseAndConvertNonCustomsLawTypeConditionsToXML(string outputPath, DownloadResult download)
		{
			var parser = new EdecNonCustomsLawTypeParser(download);
			parser.ConvertToRefXML(outputPath, DateTime.Today);
		}

		static void ParseAndConvertPermitauthorityConditionsToXML(string outputPath, DownloadResult download)
		{
			var parser = new EdecPermitAuthoritiesParser(download);
			parser.ConvertToRefXML(outputPath, DateTime.Today);
		}
	}
}
