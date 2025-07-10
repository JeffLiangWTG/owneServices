using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	class NCTSCodeListProgram
	{
		public static void Run()
		{
			var instructions = new List<(string nctsCodeListName, string ZZKCodeType, List<(string, string)> attributesNeeded)>
			{
				(
					nctsCodeListName: Constants.NctsCodeListNames.AdditionalReferenceCodes,
					ZZKCodeType: Constants.ZZRefCusCodeList.NctsAdditionalCode,
					attributesNeeded: new List<(string, string)>
					{
						(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
						(Constants.AttributeNames.Level, Constants.AttributeValues.House),
						(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
					}
				),
				(
					nctsCodeListName: Constants.NctsCodeListNames.AdditionalInformationCodes,
					ZZKCodeType: Constants.ZZRefCusCodeList.NctsAdditionalInfoCode,
					attributesNeeded: new List<(string, string)>
					{
						(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
						(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
					}
				),
				(
					nctsCodeListName: Constants.NctsCodeListNames.PreviousDocumentCodesCL214,
					ZZKCodeType: Constants.ZZRefCusCodeList.NctsPreviousDocumentCode,
					attributesNeeded: new List<(string, string)>
					{
						(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
						(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
					}
				),
				(
					nctsCodeListName: Constants.NctsCodeListNames.PreviousDocumentCodesCL228,
					ZZKCodeType: Constants.ZZRefCusCodeList.NctsPreviousDocumentCode,
					attributesNeeded: new List<(string, string)>
					{
						(Constants.AttributeNames.Level, Constants.AttributeValues.House),
					}
				),
				(
					nctsCodeListName: Constants.NctsCodeListNames.SupportingDocumentCodes,
					ZZKCodeType: Constants.ZZRefCusCodeList.NctsSupportingDocumentCode,
					attributesNeeded: new List<(string, string)>
					{
						(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
						(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
					}
				),
				(
					nctsCodeListName: Constants.NctsCodeListNames.TransportDocumentCodes,
					ZZKCodeType: Constants.ZZRefCusCodeList.NctsTransportDocumentCode,
					attributesNeeded: new List<(string, string)>
					{
						(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
						(Constants.AttributeNames.Level, Constants.AttributeValues.House),
					}
				)
			};

			var downloadUrl = ApplicationConfig.NctsCodesDownloadUrl;
			var uri = new Uri(downloadUrl);
			var downloadFileName =Path.GetFileName(uri.LocalPath);
			var zipPathName = WebClientHelper.DownloadFile(downloadUrl, downloadFileName, 5 * 60000);

			var codeListFilePathDictionary = UCCCodeListHelper.ExtractCodeListFiles(zipPathName, instructions.Select(ins => ins.nctsCodeListName).ToArray(), Path.GetDirectoryName(zipPathName));

			var downloadDir = ApplicationConfig.DownloadDir;
			foreach (var codeListGroup in instructions.GroupBy(ins => ins.ZZKCodeType))
			{
				var codeType = codeListGroup.Key;

				var allZZCodeListRead = codeListGroup.Select(ncstCodeList =>
				{
					var file = codeListFilePathDictionary[ncstCodeList.nctsCodeListName];
					return UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(file, codeType, ncstCodeList.attributesNeeded);
				});
				var zzCodeListMerged = UCCCodeListHelper.Merge(allZZCodeListRead, codeList => codeList.ZZD_Code, UCCCodeListHelper.MergeRefCusCodeList);

				var xmlFileOutputFile = Path.Combine(downloadDir, $"RefCusCodeListZZ_BE_NCTS_{codeType}.xml");

				XMLGeneration.ExportToXMLFile($"BE Additional References NCTS - {codeType}",
					xmlFileOutputFile,
					XMLGeneration.GetRefNctsCodesWriterConfiguration(codeType),
					UCCCodeListHelper.GetPublicationDateTime(codeListGroup.Select(nctsCodeList => codeListFilePathDictionary[nctsCodeList.nctsCodeListName])),
					zzCodeListMerged);
			}
		}
	}
}
