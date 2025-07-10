using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Services;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class TariffFileDownloader : IFileDownloader
	{
		public TariffFileDownloader(string rootUrl, string detailUrl, string conditionUrl)
		{
			this.rootUrl = rootUrl;
			this.detailUrl = detailUrl;
			this.conditionUrl = conditionUrl;
		}
		readonly string rootUrl;
		readonly string detailUrl;
		readonly string conditionUrl;

		public (DateTime, string) GetTradeGroupEffectiveDateAndUrl(string html = null, string detailHtml = null)
		{
			var downloadPage = GetHtmlDocument(html ?? GetHTMLString());

			var pdfDetailUrl = GetUrl(downloadPage, IsPdfDetailUrl);
			var pdfDetailHtml = detailHtml ?? WebScraper.GetUrlDownload(pdfDetailUrl);

			downloadPage = GetHtmlDocument(pdfDetailHtml);

			return (GetPdfFilePublicationTime(downloadPage), GetUrl(downloadPage, IsPdfFileUrl));
		}

		public (DateTime, string) GetConditionExcelModifiedDateAndUrl(string html = null, string detailHtml = null)
		{
			var downloadPage = GetHtmlDocument(html ?? WebScraper.GetUrlDownload(conditionUrl));

			var excelDetailUrl = GetUrl(downloadPage, IsConditionDetailUrl);
			var excelDetailHtml = detailHtml ?? WebScraper.GetUrlDownload(excelDetailUrl);

			downloadPage = GetHtmlDocument(excelDetailHtml);

			return (GetExcelFilePublicationTime(downloadPage), GetUrl(downloadPage, IsExcelFileUrl));
		}

		public (DateTime, string) GetLastEditDateAndAccessDbUrl(string html = null)
		{
			var downloadPage = GetHtmlDocument(html ?? GetHTMLString());
			return (GetZipFilePublicationTime(downloadPage), GetUrl(downloadPage, IsZipFileUrl));
		}

		public bool DownloadFile(string fileURL, string fileName)
		{
			try
			{
				var result = WebScraper.DownloadFile(fileURL, fileName);
				return result;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"File downloads failed. URL: {fileURL}.\n{ex.Message}");
			}
		}

		static DateTime GetZipFilePublicationTime(HtmlAgilityPack.HtmlDocument doc)
		{
			var nodes = doc.DocumentNode.SelectNodes($"//dl");
			DateTime newDate = DateTime.Now;
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					if (node.SelectSingleNode(".//dt").InnerText.Contains("modified:"))
					{
						var newDateString = node.SelectSingleNode(".//dd/time").InnerText.Trim();
						if (DateTime.TryParse(newDateString, out DateTime newPublishDate))
						{
							newDate = newPublishDate;
						}
						break;
					}
				}
			}
			return newDate;
		}

		static DateTime GetPdfFilePublicationTime(HtmlAgilityPack.HtmlDocument doc)
		{
			DateTime newDate = DateTime.Now;
			var nodes = doc.DocumentNode.SelectNodes($"//td");
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					var parentNode = node.ParentNode;
					if (parentNode != null && parentNode.Name.Equals("tr", StringComparison.Ordinal)
						&& parentNode.ChildNodes.Any(x => x.InnerText.ToUpper(CultureInfo.InvariantCulture).Trim().Equals(ListOfCountriesAndApplicableTariffTreatments, StringComparison.Ordinal)))
					{
						if (DateTime.TryParse(node.InnerText.Trim(), out DateTime newPublishDate))
						{
							newDate = newPublishDate;
						}
						break;
					}
				}
			}
			return newDate;
		}

		static DateTime GetExcelFilePublicationTime(HtmlAgilityPack.HtmlDocument doc)
		{
			DateTime newDate = DateTime.Now;
			var nodes = doc.DocumentNode.SelectNodes($"//li");
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					var nodeString = node.InnerText.ToUpper(CultureInfo.InvariantCulture).Trim();
					if (nodeString.Contains("RECORD MODIFIED:"))
					{
						if (DateTime.TryParse(nodeString.Replace("RECORD MODIFIED:", "").Trim(), out DateTime newPublishDate))
						{
							newDate = newPublishDate;
						}
						break;
					}
				}
			}
			return newDate;
		}

		string GetUrl(HtmlAgilityPack.HtmlDocument doc, Func<HtmlAgilityPack.HtmlNode, string> func)
		{
			var links = doc.DocumentNode.SelectNodes($"//a");
			var url = string.Empty;
			if (links != null)
			{
				foreach (var link in links)
				{
					string href = func(link);
					if (!string.IsNullOrEmpty(href))
					{
						url = AppendRootURL(href);
						break;
					}
				}
			}
			return url;
		}

		string IsZipFileUrl(HtmlAgilityPack.HtmlNode link)
		{
			var href = link.GetAttributeValue("href", "");
			if (href.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && href.Contains("01-99"))
			{
				return href;
			}
			return string.Empty;
		}

		string IsPdfDetailUrl(HtmlAgilityPack.HtmlNode link)
		{
			if (link.InnerText.ToUpper(CultureInfo.InvariantCulture).Equals("CUSTOMS TARIFF BY CHAPTER", StringComparison.Ordinal))
			{
				return link.GetAttributeValue("href", "");
			}
			return string.Empty;
		}

		string IsConditionDetailUrl(HtmlAgilityPack.HtmlNode link)
		{
			if (link.InnerText.ToUpper(CultureInfo.InvariantCulture).Trim().Equals("DATA ELEMENT MATCHING CRITERIA TABLES", StringComparison.Ordinal))
			{
				return link.GetAttributeValue("href", "");
			}
			return string.Empty;
		}

		string IsPdfFileUrl(HtmlAgilityPack.HtmlNode link)
		{
			var href = link.GetAttributeValue("href", "");
			if (href.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && CheckLinkNode(link))
			{
				return href;
			}
			return string.Empty;
		}

		string IsExcelFileUrl(HtmlAgilityPack.HtmlNode link)
		{
			var href = link.GetAttributeValue("href", "");
			if (href.ToUpper(CultureInfo.InvariantCulture).Trim().Contains("ALL-PGA-PROGRAMS-CBSA-SW-MATCHING-CRITERIA") && href.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
			{
				return link.GetAttributeValue("href", "");
			}
			return string.Empty;
		}

		static bool CheckLinkNode(HtmlAgilityPack.HtmlNode node)
		{
			var parentNode = node.ParentNode;
			if (parentNode != null && parentNode.Name.Equals("td", StringComparison.Ordinal))
			{
				parentNode = parentNode.ParentNode;
				if (parentNode != null && parentNode.Name.Equals("tr", StringComparison.Ordinal))
				{
					return parentNode.ChildNodes.Any(x => x.InnerText.ToUpper(CultureInfo.InvariantCulture).Trim().Equals(ListOfCountriesAndApplicableTariffTreatments, StringComparison.Ordinal));
				}
			}
			return false;
		}

		string GetHTMLString()
		{
			int currentYear = DateTime.Now.Year;
			var currentURL = string.Format(CultureInfo.InvariantCulture, detailUrl, currentYear);
			var nextURL = string.Format(CultureInfo.InvariantCulture, detailUrl, currentYear + 1);

			var html = WebScraper.GetUrlDownload(nextURL);
			if (string.IsNullOrEmpty(html))
			{
				html = WebScraper.GetUrlDownload(currentURL);
			}

			if (string.IsNullOrEmpty(html))
			{
				throw new InvalidOperationException($"The webpage download failed. URLs: {currentURL} and {nextURL}");
			}
			return html;
		}

		static HtmlAgilityPack.HtmlDocument GetHtmlDocument(string html)
		{
			var downloadPage = new HtmlAgilityPack.HtmlDocument();
			downloadPage.LoadHtml(html);
			return downloadPage;
		}

		string AppendRootURL(string url)
		{
			var result = url;
			if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				result = rootUrl + url;
			}
			return result;
		}

		const string ListOfCountriesAndApplicableTariffTreatments = "LIST OF COUNTRIES AND APPLICABLE TARIFF TREATMENTS";
	}
}
