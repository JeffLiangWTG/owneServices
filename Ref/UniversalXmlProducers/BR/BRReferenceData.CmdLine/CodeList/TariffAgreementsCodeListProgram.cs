using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffAgreementsCodeListProgram : GenericCheckUpdateProgram<string>
	{
		protected override string DataSourceFriendlyName => "Tariff Agreements file";

		protected override string LogFileSuffix => Constants.TariffAgreementsLogName;

		protected override string DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = AgreementsLAIACodeListDownloader.DownloadAttributesHtml(client);
				return bFile;
			}
		}

		protected override void ExportToXMLFile(string bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.TariffAgreementsCodes.Description}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.TariffAgreementsCodes.Description}.xml");

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.CmdLine.CodeList.Files.Agreements.xlsx"))
			{
				new TariffAgreementsCodeListParser($"BR {Constants.RefCusCodeTypes.TariffAgreementsCodes.Description} Code List").ExportToXMLFile((stream, bFile), outputFileName, typeOutputFileName);
			}
		}

		protected override void CheckDowloadContent(string downloadedContent)
		{
			if (downloadedContent == null || downloadedContent.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(string downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != BuildLogContent(downloadedContent);
		}

		public override void UpdateLogFile(string downloadedContent)
		{
			File.WriteAllText(LogFilePath, BuildLogContent(downloadedContent));
		}

		static string BuildLogContent(string downloadedContent)
		{
			return downloadedContent;
		}
	}
}
