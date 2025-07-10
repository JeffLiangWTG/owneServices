using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class PisCofinsReductionTariffProgram : GenericCheckUpdateProgram<(byte[], byte[])>
	{
		protected override (byte[], byte[]) DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var pisCofinsFile = PisCofinsReductionTariffDownloader.Download(client);
				var ncmFile = DownloadSubitemXml();
				return (pisCofinsFile, ncmFile);
			}
		}

		protected override string DataSourceFriendlyName => "BR Base Calculation Reduction Pis/Cofins";

		protected override string LogFileSuffix => Constants.PisCofinsBaseCalculationReductionLogName;

		protected override void ExportToXMLFile((byte[], byte[]) bFile)
		{
			using (var pisCofinsStream = new MemoryStream(bFile.Item1))
			using (var ncmStream = new MemoryStream(bFile.Item2))
			{
				var outputFileName = GetOutputFilePath("RefCusTariff_BR_PisCofinsReduction.xml");
				var parser = new PisCofinsReductionTariffParser("BR Pis Cofins Reduction Tariff");
				parser.ExportToXMLFile(pisCofinsStream, ncmStream, outputFileName, DateTime.Now);
			}
		}

		protected override void CheckDowloadContent((byte[], byte[]) downloadedContent)
		{
			if (downloadedContent.Item1 == null || downloadedContent.Item1.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog((byte[], byte[]) downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != BuildLogContent(downloadedContent);
		}

		public override void UpdateLogFile((byte[], byte[]) downloadedContent)
		{
			File.WriteAllText(LogFilePath, BuildLogContent(downloadedContent));
		}

		static string BuildLogContent((byte[], byte[]) downloadedContent)
		{
			return string.Join(Environment.NewLine, Encoding.UTF8.GetString(downloadedContent.Item1), downloadedContent.Item2);
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
