using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	public class CDSWebpageTableToDataTable : IWebpageTableToDataTable
	{
		readonly HtmlAgilityPack.HtmlDocument doc;

		public CDSWebpageTableToDataTable(string webpageHtml)
		{
			doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(webpageHtml);
		}

		public DataTable ExtractDatatableFromGrid(string headerXPath, string dataXPath)
		{
			var result = new DataTable();
			var headers = doc.DocumentNode.SelectNodes(headerXPath);
			if (headers != null)
			{
				foreach (var node in headers)
				{
					if (node.ChildNodes != null)
					{
						foreach (var header in node.ChildNodes.Where(x => x.Name == "th" || x.Name == "td"))
						{
							result.Columns.Add(header.InnerText.Trim());
						}
					}
				}
			}

			if (result.Columns.Count == 0)
			{
				return result;
			}

			var data = doc.DocumentNode.SelectNodes(dataXPath);
			if (data != null)
			{
				foreach (var node in data)
				{
					if (node.ChildNodes != null)
					{
						result.Rows.Add();
						var tableData = node.ChildNodes.Where(x => x.Name == "td")?.ToList();
						for (int i = 0; i < result.Columns.Count; i++)
						{
							var text = HtmlAgilityPack.HtmlEntity.DeEntitize(tableData[i]?.InnerText);
							result.Rows[result.Rows.Count - 1][i] = Regex.Replace(text, @"\s{2,}", " ").Trim();
						}
					}
				}
			}
			return result;
		}

		public DateTime ExtractPublishDate(string xpathPublishDate)
		{
			return ExtractPublishDate(xpathPublishDate, DateTime.UtcNow);
		}

		public DateTime ExtractPublishDate(string xpathPublishDate, DateTime defaultDate)
		{
			var publishDateString = doc.DocumentNode.SelectSingleNode(xpathPublishDate)?.InnerText;
			if (!string.IsNullOrEmpty(publishDateString))
			{
				publishDateString = Regex.Match(publishDateString.Trim(), "[A-Za-z0-9 ]+").Value.Replace("Last updated ", string.Empty);
				if (DateTime.TryParseExact(publishDateString, "d MMMM yyyy", CultureInfo.CreateSpecificCulture("en-GB"), DateTimeStyles.None, out var publishDate))
				{
					return publishDate;
				}
			}
			return defaultDate;
		}

		public Uri ExtractUrlFromAnchor(string anchorText)
		{
			var anchor = doc.DocumentNode.SelectNodes("//*[local-name()='a']")?.FirstOrDefault(x => x.InnerText.Trim() == anchorText);
			if (anchor != null)
			{
				return new Uri(anchor.GetAttributeValue("href", string.Empty), UriKind.RelativeOrAbsolute);
			}

			return null;
		}

		public Uri ExtractSingleUrlFromFileExtension(string fileExtension)
		{
			if (!fileExtension.StartsWith('.'))
			{
				fileExtension = fileExtension.Insert(0, ".");
			}

			var url = doc.DocumentNode
				.SelectNodes("//*[local-name()='a']")
				.Select(x => x.GetAttributeValue("href", string.Empty))
				.Where(x => x.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
				.Distinct()
				.ToArray();

			if (url.Length == 1)
			{
				return new Uri(url[0], UriKind.RelativeOrAbsolute);
			}

			return null;
		}
	}
}
