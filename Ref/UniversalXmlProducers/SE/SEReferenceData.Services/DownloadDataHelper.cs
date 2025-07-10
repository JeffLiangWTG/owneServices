using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.SEReferenceData.Services
{
	public static class DownloadDataHelper
	{
		public static string FindLatestTotalFilename(HttpClient httpClient, string startOfFilename, string fileRepositoryUrl)
		{
			var result = string.Empty;

			var uri = new Uri(fileRepositoryUrl);
			var html = DownloadContent(httpClient, uri);
			var filename = string.Empty;

			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var filenameStart = startOfFilename + "_";

			var allFilenames = doc.DocumentNode.Descendants().Select(x => x.GetAttributeValue("href", string.Empty));
			foreach (var singleFilename in allFilenames)
			{
				if (singleFilename.StartsWith(filenameStart, StringComparison.InvariantCulture))
				{
					if (string.Compare(FindModifiedDateFromFilename(singleFilename), FindModifiedDateFromFilename(filename), StringComparison.Ordinal) > 0)
					{
						filename = singleFilename;
					}
				}
			}

			if (filename != null && filename.Length > 0)
			{
				result = Path.Combine(fileRepositoryUrl, filename);
			}
			return result;
		}

		public static string FindModifiedDateFromFilename(string filename)
		{
			var result = string.Empty;
			var p1 = 1 + filename.LastIndexOf("_", System.StringComparison.InvariantCultureIgnoreCase);
			var p2 = filename.IndexOf(".xml.gz.pgp", System.StringComparison.InvariantCultureIgnoreCase);
			var len = filename.Length;
			if (0 < p1 && p1 < p2 && p2 < len)
			{
				result = filename.Substring(p1, p2 - p1);
			}
			return result;
		}

		static string DownloadContent(HttpClient httpClient, Uri uri)
		{
			var content = string.Empty;
			var response = httpClient.GetAsync(uri).Result;
			if (response.IsSuccessStatusCode)
			{
				content = response.Content.ReadAsStringAsync().Result;
			}
			return content;
		}
	}
}
