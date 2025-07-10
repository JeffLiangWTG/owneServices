using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class HSNTariffProgram : BaseProgram
	{
		protected override void RunCore()
		{
			using (var inputStreamXls = new MemoryStream(DownloadTableXls()))
			using (var inputStreamXml = new MemoryStream(DownloadSubitemXml()))
			{
				var publicationDate = DateTime.Now;
				var outputFileName = GetOutputFilePath("RefCusTariff_BR_HSN.xml");

				var parser = new HSNTariffParser("BR HSN Tariff");
				parser.ExportToXMLFile(inputStreamXls, inputStreamXml, outputFileName, publicationDate);

				Console.WriteLine($"BR HSN Tariff file generated in: {outputFileName}");
			}
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

		static byte[] DownloadTableXls()
		{
			using (var client = HttpClientUtils.New())
			{
				var bXlsx = new TariffTableNCMDownloader().DownloadTableNCM(client);
				Contract.Assume(bXlsx != null);
				return bXlsx;
			}
		}
	}
}
