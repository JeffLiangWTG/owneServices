using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	internal static class EUCodeListProgram
	{
		public static void Run()
		{
			var functionsToRun = GetFunctionsToRun(ApplicationConfig.UCCCodeListDownloadUrl);

			Parallel.Invoke(functionsToRun.Select(x => x.Value).ToArray());
		}

		internal static Dictionary<string, Action> GetFunctionsToRun(string downloadUrl)
		{
			var codeLists = new IUCCCodeListDetails[] {
				new ExportTransportDocumentType(),
				new ExportPreviousDocumentType(),
				new ExportAdditionalInformation(),
				new ExportAdditionalReference(),
				new ExportSupportingDocumentType()
			};

			var downloader = new DownloadUCCCodeLists(codeLists);
			var (errors, extractedCodeLists) = downloader.Download(downloadUrl);

			if (string.IsNullOrEmpty(errors))
			{
				return extractedCodeLists.ToDictionary(x => x.Key, GetAction);
			}
			else
			{
				Console.Error.WriteLine(errors);
				return new Dictionary<string, Action>();
			}

			Action GetAction(KeyValuePair<string, List<IExtractedUCCCodeList>> rawListsForCodeType)
			{
				var allZZCodeListRead = new List<RefCusCodeList>();
				var datasource = "";
				var publicationDateTime = DateTimeOffset.Now.LocalDateTime;

				foreach (var extractedCodeList in rawListsForCodeType.Value)
				{
					allZZCodeListRead = allZZCodeListRead.Union(UCCCodeListHelper.ReadCodeListCodesXmlIntoResults(extractedCodeList.FileName, extractedCodeList.CodeType, extractedCodeList.AttributeValues)).ToList();
					datasource = extractedCodeList.DataSource;
					publicationDateTime = extractedCodeList.DownloadDate;
				}

				return () => ProduceXml(rawListsForCodeType.Key, datasource, allZZCodeListRead, publicationDateTime);
			}

			void ProduceXml(string codeType, string datasource, List<RefCusCodeList> cusCodeList, DateTime publicationDateTime)
			{
				XMLGeneration.ExportToXMLFile(datasource,
					Path.Combine(ApplicationConfig.DownloadDir, $"RefCusCodeListZZ_BE_{codeType}.xml"),
					XMLGeneration.GetRefNctsCodesWriterConfiguration(codeType),
					publicationDateTime,
					cusCodeList);
			}
		}
	}
}
