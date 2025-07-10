using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class SACUTradeGroupProgram : GenericCheckUpdateProgram<string[]>
	{
		protected override string[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var str = SACUTradeGroupDownloader.Download(client);
				return str;
			}
		}

		protected override void CheckDowloadContent(string[] downloadedContent)
		{
			if (downloadedContent == null || downloadedContent.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
			}
		}

		protected override string DataSourceFriendlyName => "SACU Trade Group";

		protected override string LogFileSuffix => Constants.SACUTradeGroupLogName;

		protected override void ExportToXMLFile(string[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusTradeGroup_BR_{Constants.RefCusTradeGroup.SACU.Code}.xml");
			new SACUTradeGroupParser($"BR {DataSourceFriendlyName}").ExportToXMLFile(bFile, outputFileName);
		}

		protected override bool CompareDownloadedContentWithLog(string[] downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != string.Join("\r\n", downloadedContent);
		}

		public override void UpdateLogFile(string[] downloadedContent)
		{
			File.WriteAllText(LogFilePath, string.Join("\r\n", downloadedContent));
		}
	}
}
