using System;
using System.Data;
using System.Linq;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class HtmlDocumentWrapper
	{
		public HtmlDocumentWrapper(string webpageHtml)
		{
			doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(webpageHtml);
		}

		readonly HtmlAgilityPack.HtmlDocument doc;

		public DataTable GetTableData(string tableXPath)
		{
			var result = new DataTable();

			var tableNode = doc.DocumentNode.SelectNodes(tableXPath)?.FirstOrDefault();
			var tableRows = tableNode?.SelectNodes("./tr");
			if (tableRows != null && tableRows.Count > 1)
			{
				var headers = tableRows[0].SelectNodes("./th");
				foreach (var header in headers)
				{
					result.Columns.Add(header.InnerText.Trim().ToUpperInvariant());
				}

				var columnCount = result.Columns.Count;
				for (var idx = 1; idx < tableRows.Count; idx++)
				{
					var row = tableRows[idx];
					var tableRowData = row.SelectNodes("./th | ./td");
					var dataLength = tableRowData.Count;
					if (dataLength > 0)
					{
						var dataRow = result.Rows.Add();
						for (int i = 0; i < columnCount; i++)
						{
							dataRow[i] = i < dataLength ? tableRowData[i].InnerText.Trim() : string.Empty;
						}
					}
				}
			}
			else
			{
				Console.WriteLine($"A table with data was not found at '{tableXPath}'.");
			}

			return result;
		}

		public string GetLastModifiedDate(string lastModifiedXPath)
		{
			string lastModifiedDate = null;

			var lastModifiedNode = doc.DocumentNode.SelectNodes(lastModifiedXPath)?.FirstOrDefault();
			if (lastModifiedNode != null)
			{
				lastModifiedDate = lastModifiedNode.InnerText.Replace("Last updated ", string.Empty);
			}
			else
			{
				Console.WriteLine($"A valid node was not found at '{lastModifiedXPath}'.");
			}

			return lastModifiedDate ?? string.Empty;
		}
	}
}
