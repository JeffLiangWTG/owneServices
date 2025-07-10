using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.MXReferenceData.Business;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.MXReferenceData.CmdLine
{
	public class CustomsFacilitiesProgram : BaseCheckUpdateProgram<IEnumerable<CustomsSectionDTO>>
	{
		public CustomsFacilitiesProgram()
		{
		}

		protected override string DataSourceFriendlyName => "MX - Customs Facilities";

		protected override string LogFileSuffix => Constants.MXCustomsFacilitiesLog;

		private static string CodeType => Constants.CodeTypes.Codes.CustomsFacilities;

		public override void UpdateLogFile(IEnumerable<CustomsSectionDTO> downloadedContent)
		{
			File.WriteAllText(LogFilePath, ObjectToJson(downloadedContent));
		}

		protected override void CheckDowloadContent(IEnumerable<CustomsSectionDTO> downloadedContent)
		{
			if (!downloadedContent.Any())
			{
				throw new InvalidOperationException($"The application was unable to verify the last version {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(IEnumerable<CustomsSectionDTO> downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != ObjectToJson(downloadedContent);
		}

		protected override IEnumerable<CustomsSectionDTO> DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				return CustomsSectionDownloader.Download(client);
			}
		}

		protected override void ExportToXMLFile(IEnumerable<CustomsSectionDTO> downloadedContent)
		{
			var outputFile = GetOutputFilePath($"RefCusCodeList_MX_{CodeType}.xml");
			var outputTypeFile = GetOutputFilePath($"RefCusCodeType_MX_{CodeType}.xml");

			var parser = new CustomsFacilitiesParser(DataSourceFriendlyName);
			parser.ExportToXMLFile(downloadedContent, outputFileName: outputFile, outputTypeFileName: outputTypeFile);
		}

		static string ObjectToJson(IEnumerable<CustomsSectionDTO> dTOs) => JsonConvert.SerializeObject(dTOs);
	}
}

