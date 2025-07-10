using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class NomenclatureGroupProgram : BaseProgram
	{
		protected override void RunCore()
		{
			var publicationTime = DateTime.Now;

			var outputFileName = GetOutputFilePath($"RefCusNomenclatureGroup_BR.xml");

			using (var inputStream = new MemoryStream(DownloadSubitemXml()))
			{
				new NomenclatureGroupParser(NomenclatureGroupConstants.DataSource).ExportToXMLFile(inputStream, outputFileName, publicationTime);
			}

			Console.WriteLine($"NomenclatureGroup records generated to {outputFileName}");
		}

		static byte[] DownloadSubitemXml()
		{
			using (var handler = new HttpClientHandler { UseCookies = false })
			using (var client = HttpClientUtils.New(handler))
			{
				var bXml = new TariffNCMSubitemDownloader().Download(client);
				Contract.Assume(bXml != null);
				return bXml;
			}
		}
	}
}
