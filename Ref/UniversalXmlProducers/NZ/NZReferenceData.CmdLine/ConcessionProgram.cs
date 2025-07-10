using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using FluentFTP;

namespace CargoWise.RefDbRepo.NZReferenceData.CmdLine
{
	sealed class ConcessionProgram : NZProgram
	{
		public override string ProgramName => "Concession";

		protected override void RunCore()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), "NZ" + nameof(ConcessionProgram));
			var concessionTempDir = Path.Combine(tempDir, "Concession");
			var persistentDirectory = Path.Combine(ApplicationConfig.OutputDirectory, "NZCustomsProcessingData");
			var portalDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			Directory.CreateDirectory(concessionTempDir);

			using (var client = new HttpClient())
			{
				var downloader = new TariffListFileDownloader();
				downloader.Download(client, ApplicationConfig.ConcessionDataListUrl, concessionTempDir);
			}

			var consolidatedListOfApprovalsPath = string.Empty;
			using (var ftpClient = new FtpClient(ApplicationConfig.SeleniumFtpAddress))
			{
				consolidatedListOfApprovalsPath = FTPDownloader.DownloadWithFallback(
					ftpClient,
					ApplicationConfig.ConsolidatedListOfApprovalsFileName,
					persistentDirectory,
					ApplicationConfig.SeleniumFtpDownloadsNZAddress,
					portalDirPath,
					(Exception ex) =>
				{
					var reasonBuilder = new StringBuilder();
					reasonBuilder.AppendLine("Cannot download ConsolidatedListOfApprovals from ftp, now try to use local one. Information as below:")
						.AppendLine(CultureInfo.InvariantCulture, $"  ConsolidatedListOfApprovalsFileName: {ApplicationConfig.ConsolidatedListOfApprovalsFileName}")
						.AppendLine(CultureInfo.InvariantCulture, $"  SeleniumFtpAddress: {ApplicationConfig.SeleniumFtpAddress}")
						.AppendLine(CultureInfo.InvariantCulture, $"  SeleniumFtpDownloadsNZAddress: {ApplicationConfig.SeleniumFtpDownloadsNZAddress}");
					var messageId = AddSendToEmailMessage(reasonBuilder.ToString(), ex);
					Logger.LogError($"Cannot download ConsolidatedListOfApprovals from ftp, now try to use local one. Relevent message: {messageId}");
				});
			}
			Logger.LogInfo($"ConsolidatedListOfApprovals file path: {consolidatedListOfApprovalsPath}");

			var filePaths = new BuildersFilePath[] {
				new BuildersFilePath(Path.Combine(concessionTempDir, "Concession_Details.csv"), BuilderFilePathSymbol.ConcessionDetail),
				new BuildersFilePath(Path.Combine(concessionTempDir, "Concession_Rates.csv"), BuilderFilePathSymbol.ConcessionRate),
				new BuildersFilePath(Path.Combine(concessionTempDir, "Concession_to_Tariff.csv"), BuilderFilePathSymbol.ConcessionToTariff),
				new BuildersFilePath(Path.Combine(concessionTempDir, "time_stamp.txt"), BuilderFilePathSymbol.PublicationTime),
				new BuildersFilePath(consolidatedListOfApprovalsPath, BuilderFilePathSymbol.ConsolidatedListOfApprovalsJson),
				new BuildersFilePath(Path.Combine(portalDirPath, ApplicationConfig.ConcessionOverrideFileName), BuilderFilePathSymbol.ConcessionOverride),
			};

			var processingDataManager = new ProcessingDataManager<NZConcessionProcessingData>(persistentDirectory, "NZConcessionProcessingData.json");
			var parser = new ConcessionParser(Logger);
			var parseResult = parser.Parse(filePaths, new DateProvider(), processingDataManager.ProcessingData);

			if (parseResult)
			{
				parser.ExportToXMLFile(ApplicationConfig.OutputDirectory);
				processingDataManager.SaveData();
				Directory.Delete(tempDir, true);
			}
			Logger.LogInfo($"NZ Concessions data process completed.");
		}
	}
}
