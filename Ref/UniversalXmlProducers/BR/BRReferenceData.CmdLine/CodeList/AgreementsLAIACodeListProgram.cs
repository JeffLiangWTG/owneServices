using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class AgreementsLAIACodeListProgram : GenericCheckUpdateProgram<(byte[], string)>
	{
		protected override (byte[], string) DowloadContent()
		{
			using (var client = GetHttpClient())
			using (var client2 = GetHttpClient())
			{
				return (AgreementsLAIACodeListDownloader.Download(client), AgreementsLAIACodeListDownloader.DownloadAttributesHtml(client2));
			}
		}

		protected override string DataSourceFriendlyName => "Duty LAIA Agreement Codes file";

		protected override string LogFileSuffix => Constants.LaiaAgreementsLogName;

		protected override void ExportToXMLFile((byte[], string) bFile)
		{
			using (var stream = new MemoryStream(bFile.Item1))
			{
				var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.DutyLaiaAgreementCodes.Code}.xml");
				var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.DutyLaiaAgreementCodes.Code}.xml");

				new AgreementsLAIACodeListParser($"BR {Constants.RefCusCodeTypes.DutyLaiaAgreementCodes.Description} Code List").ExportToXMLFile((stream, bFile.Item2), outputFileName, typeOutputFileName);
			}
		}

		protected override void CheckDowloadContent((byte[], string) downloadedContent)
		{
			if (downloadedContent.Item1 == null || downloadedContent.Item1.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog((byte[], string) downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != BuildLogContent(downloadedContent);
		}

		public override void UpdateLogFile((byte[], string) downloadedContent)
		{
			File.WriteAllText(LogFilePath, BuildLogContent(downloadedContent));
		}

		static string BuildLogContent((byte[], string) downloadedContent)
		{
			return string.Join(Environment.NewLine, Encoding.UTF8.GetString(downloadedContent.Item1), downloadedContent.Item2);
		}
	}
}
