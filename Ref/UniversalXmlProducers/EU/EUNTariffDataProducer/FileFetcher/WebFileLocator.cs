using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class WebFileLocator : IWebFileLocator
	{
		readonly IWebDriverHelper webDriverHelper;
		readonly IEnumerable<string> filesToLocate;
		readonly int waitLoadingSeconds = ApplicationConfig.WaitPageLoadingInSeconds;
		readonly int maxOfReloadLatestYearPage = ApplicationConfig.MaxOfReloadLatestYearPage;
		readonly string cookiesButtonText = "Accept all cookies";

		static IWebFileInfo ErrorRecord(string target, string basePage, string extra = null) => new WebFileInfo("Error Record", new InvalidOperationException($"Unable to read {target} from {basePage}. {extra}"));

		public WebFileLocator(IWebDriverHelper webDriverHelper, IEnumerable<string> filesToLocate)
		{
			Argument.NotNull(webDriverHelper, nameof(webDriverHelper));
			Argument.NotNull(filesToLocate, nameof(filesToLocate));

			this.webDriverHelper = webDriverHelper;
			this.filesToLocate = filesToLocate;
		}

		HtmlDocument LoadLatestFolder(HtmlDocument htmlDocument, int nodeToSkip = 0, string[] folderNames = null)
		{
			if (!TryGetRecordSetNodes(htmlDocument, out var recordSetNodes))
			{
				return null;
			}

			var firstNode = folderNames?.Any() ?? false
				? recordSetNodes.Where(x => folderNames.Any(y => x.InnerText.Contains(y))).Skip(nodeToSkip).FirstOrDefault()
				: recordSetNodes.Skip(nodeToSkip).FirstOrDefault();

			var descendantNodes = firstNode?.Descendants("a");
			if (descendantNodes == null || !descendantNodes.Any())
			{
				return null;
			}

			var latestNode = descendantNodes.First();
			var latestNodeInnerText = latestNode.InnerText;

			var latestFolderHtmlDocument = new HtmlDocument();
			webDriverHelper.AcceptCookiesForTaricWebSite(cookiesButtonText);
			latestFolderHtmlDocument.LoadHtml(webDriverHelper.GetWebPageByLinkText(latestNodeInnerText, waitLoadingSeconds));

			return latestFolderHtmlDocument;
		}

		HtmlDocument LoadLatestFolderWithRetry(string basePage, int maxOfReload, string[] folderNames = null)
		{
			var availableYearsPage = new HtmlDocument();
			HtmlDocument latestYearPage = null;

			while (maxOfReload > 0 && latestYearPage == null)
			{
				maxOfReload--;
				var webPageContent = webDriverHelper.GetWebPage(basePage, waitLoadingSeconds);

				//If Loading text is coming on webPageContent, it means the page is stuck on Loading, when the page actually loads, the driver never returns "Loading..."
				//We should keep trying until Loading is not present.
				if (webPageContent.Contains("<cbc-app style=\"display: flex; flex-flow: column\">Loading...</cbc-app>"))
				{
					Console.WriteLine($"The page is stuck in Loading, refreshing the page '{basePage}' for the {maxOfReloadLatestYearPage - maxOfReload} time");
					continue;
				}

				availableYearsPage.LoadHtml(webPageContent);
				latestYearPage = LoadLatestFolder(availableYearsPage);
			}

			var latestFolder = latestYearPage != null ? LoadLatestFolder(latestYearPage, ApplicationConfig.MonthsToSkipForTest, folderNames) : null;

			return latestFolder is null || NotEmpty(latestFolder)
				? latestFolder
				: LoadFromPreviousMonth() ?? LoadFromPreviousYear();

			bool NotEmpty(HtmlDocument document) => TryGetRecordSetNodes(document, out _);

			HtmlDocument LoadFromPreviousMonth() => LoadLatestFolder(latestYearPage, 1 + ApplicationConfig.MonthsToSkipForTest, folderNames);

			HtmlDocument LoadFromPreviousYear() => LoadLatestFolder(availableYearsPage, 1) is HtmlDocument previousYearPage ? LoadLatestFolder(previousYearPage, 0, folderNames) : null;
		}


		static bool TryGetRecordSetNodes(HtmlDocument htmlDocument, out HtmlNodeCollection recordSetNodes)
		{
			recordSetNodes = htmlDocument.DocumentNode.SelectNodes("//tr[contains(@class, 'row')]");
			return recordSetNodes != null && recordSetNodes.Count > 0;
		}

		public IEnumerable<IWebFileInfo> GetLocationOfLatestFiles(string basePage) => GetLocationOfLatestFiles(basePage, takeLatestOfDuplicatedFiles: false);

		public IEnumerable<IWebFileInfo> GetLocationOfLatestFiles(string basePage, bool takeLatestOfDuplicatedFiles)
		{
			var latestMonthWebPage = LoadLatestFolderWithRetry(basePage, maxOfReloadLatestYearPage);
			if (latestMonthWebPage == null)
			{
				return new[] { ErrorRecord(nameof(latestMonthWebPage), basePage) };
			}

			var webFileInfos = new List<IWebFileInfo>();
			foreach (var fileToLocate in filesToLocate)
			{
				var comparer = takeLatestOfDuplicatedFiles ? new ContentRecordModifiedDateComparer() : null;
				var webFileInfo = SetWebFileInfo(latestMonthWebPage, fileToLocate, comparer);
				webFileInfos.Add(webFileInfo);
			}
			return webFileInfos;
		}

		public IEnumerable<IWebFileInfo> GetLocationOfSpecificFiles(string basePage, string[] folderNames, Regex searchPatternRegex)
		{
			return FileFetcherRetry.Execute<IEnumerable<IWebFileInfo>>(() =>
			{
				var latestMonthWebPage = LoadLatestFolderWithRetry(basePage, maxOfReloadLatestYearPage, folderNames);
				if (latestMonthWebPage == null)
				{
					var extra = "folderNames are:" + string.Join(",", folderNames);
					return new[] { ErrorRecord(nameof(latestMonthWebPage), basePage, extra) };
				}

				var webFileInfos = new List<IWebFileInfo>();
				var filenames = latestMonthWebPage.DocumentNode.SelectNodes("//a")?
					.Where(x => !filesToLocate.Any(y => x.InnerText.StartsWith(y, StringComparison.Ordinal))
					&& searchPatternRegex.IsMatch(x.InnerText))?
					.Select(x => x.InnerText)?.ToList() ?? new List<string>();
				foreach (var fileToLocate in filenames)
				{
					var webFileInfo = SetWebFileInfo(latestMonthWebPage, fileToLocate);
					webFileInfos.Add(webFileInfo);
				}

				return webFileInfos;
			});
		}

		public static IWebFileInfo SetWebFileInfo(HtmlDocument htmlDocument, string fileName, IComparer<HtmlNode> comparer = null)
		{
			var nodes = htmlDocument.DocumentNode.SelectNodes("//a")?.Where(x => x.InnerText.StartsWith(fileName, StringComparison.OrdinalIgnoreCase))?.ToList();
			if (nodes == null || nodes.Count == 0)
			{
				return new WebFileInfo(fileName, new ArgumentException($"Unable to retrieve HTML link node with text '{fileName}'."));
			}

			HtmlNode fileNameNode = SelectSingleNode(nodes, comparer);
			var details = fileNameNode.Attributes["href"].Value;
			var contentArray = details.Split('/');
			var contentID = contentArray[contentArray.Length - 2];
			if (string.IsNullOrEmpty(contentID))
			{
				return new WebFileInfo(fileName, new ArgumentException($"{fileName} download path not found"));
			}

			var trNode = fileNameNode.ParentNode?.ParentNode?.ParentNode;
			var lastModificationNode = trNode?.SelectSingleNode("td[contains(@class, 'cell-last-modification')]");
			if (lastModificationNode == null)
			{
				return new WebFileInfo(fileName, new ArgumentException($"{fileName} last modification time not found"));
			}
			var lastModification = DateTime.ParseExact(lastModificationNode.InnerText.Trim(), "yyyy MM dd, HH:mm", CultureInfo.InvariantCulture);
			var downloadPath = ApplicationConfig.CircabcDownloadUrlPrefix + contentID;
			return new WebFileInfo(fileName, downloadPath, lastModification);
		}

		static HtmlNode SelectSingleNode(IEnumerable<HtmlNode> nodes, IComparer<HtmlNode> comparer)
		{
			if (comparer != null)
			{
				nodes = nodes.OrderBy(x => x, comparer);
			}

			return nodes.FirstOrDefault();
		}
	}
}
