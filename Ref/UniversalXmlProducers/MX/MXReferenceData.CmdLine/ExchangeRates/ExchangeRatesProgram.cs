using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.MXReferenceData.Business;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using FluentFTP;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.MXReferenceData.CmdLine
{
	public class ExchangeRatesProgram : BaseCheckUpdateProgram<Dictionary<string, string>>
	{
		protected override Dictionary<string, string> DowloadContent()
		{
			using (var client = GetConnection)
			{
				return ExchangeRatesDownloader.DownloadXmls(client);
			}
		}

		protected override string DataSourceFriendlyName => "Exchange Rates";

		protected override string LogFileSuffix => Constants.MXExchangeRatesLog;

		protected override void CheckDowloadContent(Dictionary<string, string> downloadedContent)
		{
			if (downloadedContent?.Keys.Where(key => downloadedContent[key]?.Length == 0).Any() ?? true)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override void ExportToXMLFile(Dictionary<string, string> downloadedContent)
		{
			ExportToXMLFile(Constants.ExchangeRateTypes.Customs, downloadedContent.FirstOrDefault(x => x.Key == Constants.ExchangeRateFiles.CTARC_DEPAIS).Value);
			ExportToXMLFile(Constants.ExchangeRateTypes.CustomsExport, downloadedContent.FirstOrDefault(x => x.Key == Constants.ExchangeRateFiles.CTARC_TIPCAM).Value);
		}

		void ExportToXMLFile(string exchangeRateType, string sourceString)
		{
			var uxmlFileName = $"RefExchangeRateZZ_MX_{exchangeRateType}.xml";
			var uxmlOutputFileName = GetOutputFilePath(uxmlFileName);
			var parser = new ExchangeRatesParser("MX Customs Exchange Rate");

			if (!sourceString.IsNullOrEmpty())
			{
				using (var inputStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sourceString)))
				{
					parser.ExportToXMLFile(inputStream, uxmlOutputFileName, exchangeRateType, DateTime.Now);
					Console.WriteLine($"Xml file :{uxmlFileName}, generated.");
				}
			}
		}

		protected override bool CompareDownloadedContentWithLog(Dictionary<string, string> downloadedContent)
		{
			var downloadedText = DictionaryToText(downloadedContent);
			var fileLog = File.ReadAllText(LogFilePath);
			return !downloadedText.Equals(fileLog, StringComparison.Ordinal);
		}

		public override void UpdateLogFile(Dictionary<string, string> downloadedContent)
		{
			var log = DictionaryToText(downloadedContent);
			File.WriteAllText(LogFilePath, log);
		}

		protected static IFtpClient GetConnection
		{
			get
			{
				var conn = new FtpClient(ConfigurationProvider.ExchangeRate.Host, ConfigurationProvider.ExchangeRate.UserName, ConfigurationProvider.ExchangeRate.Password, port: 990);
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
