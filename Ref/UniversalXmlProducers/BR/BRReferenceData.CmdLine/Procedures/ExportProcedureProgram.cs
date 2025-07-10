using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class ExportProcedureProgram : BaseProgram
	{
		protected override void RunCore()
		{
			using (var client = HttpClientUtils.New())
			{
				var downloader = new ExportProcedureDownloader();
				var cpcDTO = downloader.Download(client);

				var outputFileName = GetOutputFilePath($"RefCusProcedure_BR_{Constants.ShipmentTypes.Export}.xml");

				using (var inputStream = new MemoryStream(cpcDTO))
				using (var reader = new StreamReader(inputStream, CodePagesEncodingProvider.Instance.GetEncoding(1252)))
				{
					var publicationTime = DateTime.ParseExact(reader.ReadLine().Replace(",", ""), "dd/MM/yyyy HH:mm", null);

					var parser = new ExportProcedureParser("BR Export Procedure");
					parser.ExportToXMLFile(reader, outputFileName, publicationTime);
					Console.WriteLine($"RefCusCodeProcedure records generated to {outputFileName}");
				}
			}
		}
	}
}
