using System;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffTributaryProgram : GenericCheckUpdateProgram<TariffTributary>
	{
		protected override string DataSourceFriendlyName => "Tariff Tributary TCCE";

		protected override string LogFileSuffix => Constants.TariffTributaryLogName;

		protected override TariffTributary DowloadContent()
		{
			using (var handler = new HttpClientHandler
			{
				ClientCertificateOptions = ClientCertificateOption.Manual,
				SslProtocols = SslProtocols.None,
				CheckCertificateRevocationList = true
			})
			{
				return TariffTributaryDownloader.DownLoadZipAndExtract(GetHttpClient(handler));
			}
		}

		protected override HttpClient GetHttpClient(HttpClientHandler handler) => HttpClientHelper.GetHttpClientWithCertificate(handler, false);

		protected override void CheckDowloadContent(TariffTributary downloadedContent)
		{
			if (downloadedContent == null)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override void ExportToXMLFile(TariffTributary bFile)
		{
			var parser = new TariffTributaryParser(DataSourceFriendlyName);
			parser.ExportToXMLFile(bFile);
		}

		protected override bool CompareDownloadedContentWithLog(TariffTributary downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != downloadedContent.dataGeracao;
		}

		public override void UpdateLogFile(TariffTributary downloadedContent)
		{
			File.WriteAllText(LogFilePath, downloadedContent.dataGeracao);
		}
	}
}
