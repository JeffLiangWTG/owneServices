using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public abstract class NctsManyToOneCodeListXMLProducer
	{
		protected NctsManyToOneCodeListXMLProducer()
		{
			ErrorBuilder = new StringBuilder();
		}

		public async Task<string> DownloadAndConvertAllRefCusCodeListXMLs(IHttpClientHelper httpClientHelper, string outputPath)
		{
			var extractedCodeLists = new List<ExtractedCodeListProvider>();

			foreach (var codeListDetail in CodeListDetails)
			{
				var nctsCodeListDownloader = new NctsCodeListDownloader(ErrorBuilder, httpClientHelper, codeListDetail);
				var downloadedItems = await nctsCodeListDownloader.DownloadAndConvertToRefCusCodeList();
				extractedCodeLists.AddRange(downloadedItems);
			}

			var result = UnifyAndMergeRefCusCodeLists(extractedCodeLists);
			if (result.Any())
			{
				var publicationTime = extractedCodeLists.Max(x => x.PublicationTime);
				var randomCodeListDetail = CodeListDetails.First();
				var codeType = randomCodeListDetail.CodeType;
				XmlWriterHelper.ExportToXMLFile(randomCodeListDetail.DataSource,
					Path.Combine(outputPath, $"RefCusCodeListZZ_EUN_{codeType}.xml"),
					XmlWriterHelper.GetRefNctsCodesWriterConfiguration(codeType, extractedCodeLists.Any(x => x.SupportsAttributes)),
					publicationTime,
					result,
					Common.UniversalXmlWriter.UpdateType.Full,
					GetDependencies(RefCusCodeTypeProducer?.PublicationDate)
					);

				if (RefCusCodeTypeProducer != null)
				{
					RefCusCodeTypeProducer.GenerateFile(outputPath);
				}
			}

			return ErrorBuilder.ToString();
		}

		protected virtual IEnumerable<RefCusCodeList> UnifyAndMergeRefCusCodeLists(List<ExtractedCodeListProvider> extractedCodeLists)
		{
			var result = Enumerable.Empty<RefCusCodeList>();
			if (extractedCodeLists.Any())
			{
				result = extractedCodeLists.First().ParsedXml;
				foreach (var codeList in extractedCodeLists.Skip(1).Select(x => x.ParsedXml))
				{
					result = result.Union(codeList);
				}
				result = NCTSCodeListParser.Merge(new[] { result }, codeList => codeList.ZZD_Code, NCTSCodeListParser.MergeRefCusCodeList);
			}
			return result;
		}

		protected abstract IUCCExportCodeListDetail[] CodeListDetails { get; }

		protected StringBuilder ErrorBuilder { get; set; }

		protected virtual Dependency[] GetDependencies(DateTime? dependencyDateTime) => Array.Empty<Dependency>();

		protected virtual RefCusCodeTypeProducer RefCusCodeTypeProducer => null;
	}
}
