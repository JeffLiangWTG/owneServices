using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.MXReferenceData.Business;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using FluentFTP;

namespace CargoWise.RefDbRepo.MXReferenceData.CmdLine
{
	public class TariffRatesProgram : BaseCheckUpdateProgram<Dictionary<string, string>>
	{
		protected override Dictionary<string, string> DowloadContent()
		{
			using (var client = GetConnection)
			{
				return TariffRatesDownloader.DownloadXmls(client);
			}
		}

		protected override string DataSourceFriendlyName => "Tariff Rates";

		protected override string LogFileSuffix => Constants.MXTariffRatesLog;

		public override void UpdateLogFile(Dictionary<string, string> downloadedContent)
		{
			var log = DictionaryToText(downloadedContent);
			File.WriteAllText(LogFilePath, log);
		}

		protected override void CheckDowloadContent(Dictionary<string, string> downloadedContent)
		{
			if (downloadedContent?.Keys.Where(key => downloadedContent[key]?.Length == 0).Any() ?? true)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(Dictionary<string, string> downloadedContent)
		{
			var downloadedText = DictionaryToText(downloadedContent);
			var fileLog = File.ReadAllText(LogFilePath);
			return !downloadedText.Equals(fileLog, StringComparison.Ordinal);
		}

		protected override void ExportToXMLFile(Dictionary<string, string> downloadedContent)
		{
			foreach (var fileName in downloadedContent.Keys)
			{
				File.WriteAllText(GetOutputFilePath(fileName), downloadedContent[fileName]);
				Console.WriteLine($"Xml file: {fileName}, generated.");
			}
		}

		protected static IFtpClient GetConnection
		{
			get
			{
				var conn = new FtpClient(ConfigurationProvider.TariffRate.Host, ConfigurationProvider.TariffRate.UserName, ConfigurationProvider.TariffRate.Password, port: 990);
				conn.Config.EncryptionMode = FtpEncryptionMode.Implicit;
				conn.ValidateCertificate += (control, e) => {
					e.Accept = true;
				};
				return conn;
			}
		}

		protected static string DictionaryToText(Dictionary<string, string> downloadedContent) => string.Join(",", downloadedContent?.Keys.Select(key => (string)downloadedContent[key]));
	}
}
