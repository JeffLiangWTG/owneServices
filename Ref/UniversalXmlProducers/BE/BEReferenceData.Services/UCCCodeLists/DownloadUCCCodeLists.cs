using System;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public class DownloadUCCCodeLists
	{
		public DownloadUCCCodeLists(IUCCCodeListDetails[] codeListsToExtract)
		{
			if (codeListsToExtract == null || codeListsToExtract.Length == 0)
			{
				throw new ArgumentException("The codes to extract should not be null or empty", nameof(codeListsToExtract));
			}
			this.codeListsToExtract = codeListsToExtract;
		}
		readonly IUCCCodeListDetails[] codeListsToExtract;

		public (string Error, IReadOnlyDictionary<string, List<IExtractedUCCCodeList>> ExtractedCodeList) Download(string url)
		{
			var errorText = string.Empty;
			var extractedCodeLists = new Dictionary<string, List<IExtractedUCCCodeList>>();
			var realUrl = url;

			foreach (var list in codeListsToExtract)
			{
				try
				{
					realUrl = url.Replace("{Domain}", list.Domain).Replace("{CodeListType}", list.CodeListType);
					var uri = new Uri(realUrl);
					var downloadFileName = Path.GetFileName(uri.LocalPath);
					var zipPathName = WebClientHelper.DownloadFile(realUrl, downloadFileName, 5 * 60000);

					var extractedCodeList = UCCCodeListHelper.ExtractUCCCodeListFile(zipPathName, list, Path.GetDirectoryName(zipPathName));
					if (extractedCodeLists.ContainsKey(list.CodeType))
					{
						extractedCodeLists[list.CodeType].Add(extractedCodeList);
					} else
					{
						extractedCodeLists[list.CodeType] = new List<IExtractedUCCCodeList>(new[] { extractedCodeList });
					}
				}
				catch (FileNotFoundException ex)
				{
					errorText = $"Unable to Download the BE CodeLists ZIP from the following URL: {realUrl} /r/n {ex.Message}";
				}
			}
			return (errorText, extractedCodeLists);
		}
	}
}
