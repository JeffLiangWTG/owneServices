using System;
using CargoWise.RefDbRepo.CAReferenceData.Services;
using System.Text;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	public abstract class FileDownloader
	{
		protected FileDownloader(PreProcessChecker checker)
		{
			this.checker = checker;
		}

		internal readonly PreProcessChecker checker;

		protected abstract string RootURL { get; }

		protected virtual string GetCountryVersion(string url) => string.Empty;

		protected virtual string LinkNodeXpath => $"//a";

		protected abstract bool CheckIsDownloadFileNode(string href);

		public StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		public bool DownloadFile(string url, string fileName, bool passCheck = false, string html = "", bool downloadPdf = true)
		{
			var result = false;
			var downloadPage = new HtmlDocument();
			downloadPage.LoadHtml(string.IsNullOrEmpty(html) ? WebScraper.GetHtml(url) : html);

			if (CheckPublishedVersion(downloadPage, passCheck))
			{
				Console.WriteLine("Nothing new published since last process. Skip processing this time.");
			}
			else if(!downloadPdf)
			{
				try
				{
					downloadPage.Save(fileName);
					result = true;
				}
				catch (Exception ex)
				{
					ErrorBuilder.AppendLine($"File downloads failed. URL: {url}.\n{ex.Message}");
				}
			}
			else
			{
				var countryVersion = GetCountryVersion(url);

				var links = downloadPage.DocumentNode.SelectNodes(LinkNodeXpath);
				if (links != null)
				{
					var downloadFileURL = GetDownloadFileURL(links);

					if (string.IsNullOrEmpty(downloadFileURL))
					{
						ErrorBuilder.AppendLine($"{countryVersion}The data file node is not found in the web page, the page layout may have changed.");
					}
					else
					{
						try
						{
							result = WebScraper.DownloadFile(downloadFileURL, fileName);
							if (!result)
							{
								ErrorBuilder.AppendLine($"{countryVersion}File downloads failed.");
							}
						}
						catch (Exception ex)
						{
							ErrorBuilder.AppendLine($"{countryVersion}File downloads failed. URL: {downloadFileURL}.\n{ex.Message}");
						}
					}
				}
				else
				{
					ErrorBuilder.AppendLine($"{countryVersion}The a node is not found in the web page, the page layout may have changed.");
				}
			}
			return result;
		}

		bool CheckPublishedVersion(HtmlDocument doc, bool passCheck = false)
		{
			if (passCheck)
			{
				return false;
			}
			var paragraphs = doc.DocumentNode.SelectNodes($"//dl");
			foreach (var paragraph in paragraphs)
			{
				if (paragraph.SelectSingleNode(".//dt").InnerText.Contains("modified:"))
				{
					var newDateString = paragraph.SelectSingleNode(".//dd/time").InnerText.Trim();
					if (DateTime.TryParse(newDateString, out DateTime newPublishDate))
					{
						PublicationTime = newPublishDate;
					}
					break;
				}
			}
			return !checker.UpdateLastPublishDate(PublicationTime);
		}

		public DateTime PublicationTime { get; set; }

		string GetDownloadFileURL(HtmlNodeCollection links)
		{
			var downloadFileURL = string.Empty;
			foreach (var link in links)
			{
				var href = link.GetAttributeValue("href", "");
				if (CheckIsDownloadFileNode(href))
				{
					downloadFileURL = AppendRootLink(href);
					break;
				}
			}
			return downloadFileURL;
		}

		protected string AppendRootLink(string url)
		{
			var result = url;
			if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				result = RootURL + url;
			}
			return result;
		}
	}
}
