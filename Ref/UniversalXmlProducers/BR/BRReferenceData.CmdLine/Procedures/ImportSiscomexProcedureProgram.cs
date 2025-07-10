using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class ImportSiscomexProcedureProgram : BaseProgram
	{
		protected override void RunCore()
		{
			using (var client1 = HttpClientUtils.New())
			using (var client2 = HttpClientUtils.New())
			using (var client3 = HttpClientUtils.New())
			{
				var downloader1 = new PisCofinsLegalBasisCodeListDownloader();
				var pisCofinsLegalBasisCodeListDTO = downloader1.Download(client1);

				var downloader2 = new DutyLegalBasisCodeListDownloader();
				var dutyLegalBasisCodeListDownloaderDTO = downloader2.Download(client2);

				var downloader3 = new DeclarationTypeTaxRegimeDownloader();
				var declarationTypeTaxRegimeDownloaderDTO = downloader3.Download(client3);

				var outputFileName = GetOutputFilePath($"RefCusProcedure_BR_{Constants.ShipmentTypes.ImportSiscomex}.xml");

				using (var inputStreamPisCofinsLegalBasis = new MemoryStream(pisCofinsLegalBasisCodeListDTO))
				using (var inputStreamDutyLegalBasis = new MemoryStream(dutyLegalBasisCodeListDownloaderDTO))
				using (var inputStreamDeclarationTypeTaxRegime = new MemoryStream(declarationTypeTaxRegimeDownloaderDTO))
				{
					var publicationTime = DateTime.Now;

					new ImportSiscomexProcedureParser("BR Import Siscomex Procedure").ExportToXMLFile(inputStreamDutyLegalBasis, inputStreamPisCofinsLegalBasis, inputStreamDeclarationTypeTaxRegime, outputFileName, publicationTime);
					Console.WriteLine($"RefCusProcedure records generated to {outputFileName}");
				}
			}
		}
	}
}
