using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public sealed class DownloadManager : IDownloadManager
	{
		public DownloadManager(IWebClientWrapper webClient)
		{
			WebClient = webClient;
		}

		readonly IWebClientWrapper WebClient;

		#region CSV
		string IDownloadManager.GetCSVContent(ICDSPortSource cdsPortSource)
		{
			try
			{
				var html = WebClient.GetContent(cdsPortSource.PageURL);
				var page = new CDSWebpageTableToDataTable(html);

				var url = page.ExtractUrlFromAnchor(cdsPortSource.AnchorText)?.OriginalString;
				if (string.IsNullOrEmpty(url))
				{
					url = ExtractCsvUrlFromHtmlPage(html);
				}
				if (string.IsNullOrEmpty(url))
				{
					throw new ApplicationException($"CSV download link '{cdsPortSource.AnchorText}' not found.");
				}
				return WebClient.GetContent(url);
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Processing of {cdsPortSource.PageURL} failed. ", ex);
			}
		}

		static string ExtractCsvUrlFromHtmlPage(string html)
		{
			var result = string.Empty;
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);

			var anchor = doc.DocumentNode.SelectNodes("//a[contains (@href, '.csv') and substring(@href, string-length(@href) - 3) = '.csv']")?.FirstOrDefault();

			if (anchor != null)
			{
				result = anchor.GetAttributeValue("href", string.Empty);
			}

			return result;
		}

		#endregion

		#region Binary
		byte[] IDownloadManager.GetBinaryData(ICDSPortSource cdsPortSource)
		{
			try
			{
				var html = WebClient.GetContent(cdsPortSource.PageURL);
				var page = new CDSWebpageTableToDataTable(html);
				var url = page.ExtractUrlFromAnchor(cdsPortSource.AnchorText)?.OriginalString;
				if (string.IsNullOrEmpty(url))
				{
					var message = $"Anchor text '{cdsPortSource.AnchorText}' was not found on the page at '{cdsPortSource.PageURL}'";
					throw new ApplicationException(message);
				}
				return WebClient.GetContentAsByteArray(url);
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Processing of {cdsPortSource.PageURL} failed. ", ex);
			}
		}
		#endregion
	}
}
