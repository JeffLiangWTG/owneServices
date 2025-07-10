using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData
{
	public class WebpageTableToDataTable : IWebpageTableToDataTable
	{
		readonly HtmlAgilityPack.HtmlDocument doc;

		public WebpageTableToDataTable(string webpageHtml)
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
						foreach (var header in node.ChildNodes.Where(x => x.Name == "th"))
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
						for (int i = 0; i < tableData.Count; i++)
						{
							if (tableData[i]?.InnerHtml?.Contains("<br>") ?? false)
							{
								result.Rows[result.Rows.Count - 1][i] = tableData[i]?.InnerHtml?
									.Replace("<br>", "\r\n")
									.Replace("\r\n\r\n", "\r\n").Trim();
							}
							else
							{
								result.Rows[result.Rows.Count - 1][i] = tableData[i]?.InnerText?.Trim();
							}
						}
					}
				}
			}
			return result;
		}

		public DateTime ExtractPublishDate(string xpathPublishDate)
		{
			var publishDateString = doc.DocumentNode.SelectSingleNode(xpathPublishDate)?.InnerText;
			if (!string.IsNullOrEmpty(publishDateString))
			{
				publishDateString = Regex.Match(publishDateString, "[0-9]{4}-[0-9]{2}-[0-9]{2}").Value;
				if (DateTime.TryParseExact(publishDateString, "yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-GB"), DateTimeStyles.None, out var publishDate))
				{
					return publishDate;
				}
			}
			return DateTime.UtcNow;
		}
	}
}
