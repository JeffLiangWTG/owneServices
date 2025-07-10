using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class TariffDetachesProgram : BaseProgram
	{
		protected override void RunCore()
		{
			using (var inputStreamXml = new MemoryStream(DownloadSubitemXml()))
			{
				var publicationDate = DateTime.Now;

				Contract.Assume(inputStreamXml != null);

				var outputFileName = GetOutputFilePath("BR_CustomsTariffDetaches.xml");

				//TODO: Implement logic do convert TA-IMP-AnuenteWeb-atual-09042022.xlsx to Universal Reference Data XML

				Console.WriteLine($"BR_CustomsTariffDetaches file generated in: {outputFileName}");
			}
		}

		static byte[] DownloadSubitemXml()
		{
			using (var handler = new HttpClientHandler { UseCookies = false })
			using (var client = HttpClientUtils.New())
			{
				var bXml = new TariffDetachesDownloader().Download(client);
				Contract.Assume(bXml != null);
				return bXml;
			}
		}
	}
}
