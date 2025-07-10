using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class GSTPTariffProgram : BaseProgram
	{
		protected override void RunCore()
		{
			var bFiles = DownloadContent();

			if (bFiles == null || bFiles.Count != 2 || bFiles.Where(t => t.Length == 0).Any())
			{
				throw new InvalidOperationException($"The application was unable to download all the files.");
			}

			var outputFileName = GetOutputFilePath("RefCusTariff_BR_GSTP.xml");

			using (var streamFileGSTP = new MemoryStream(bFiles[0]))
			using (var streamFileNCM = new MemoryStream(bFiles[1]))
			using (var streamFileTEC = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.CmdLine.Tariffs.DataSource.tec_20211126.xlsx"))
			{
				new GSTPTariffParser("BR GSTP Tariff").ExportToXMLFile(streamFileGSTP, streamFileNCM, streamFileTEC, outputFileName, DateTime.Now);
				Console.WriteLine($"RefCusTariffGSTP records generated to {outputFileName}");
			}
		}

		static HttpClient GetHttpClient => HttpClientUtils.New();

		static List<byte[]> DownloadContent()
		{
			List<byte[]> bFiles = new List<byte[]>();
			bFiles.Add(DownloadGSTPFile());
			bFiles.Add(DownloadNCMFile());

			return bFiles;
		}

		static byte[] DownloadGSTPFile()
		{
			using (var client = GetHttpClient)
			{
				var bFile = GlobalSystemOfTradePreferencesDownloader.Download(client);
				return bFile;
			}
		}

		static byte[] DownloadNCMFile()
		{
			using (var client = GetHttpClient)
			{
				var downloader = new TariffTableNCMDownloader();
				var bFile = downloader.DownloadTableNCM(client);
				return bFile;
			}
		}
	}
}
