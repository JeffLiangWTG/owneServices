using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class HtmlTableLoader
	{
		public async static Task<(TResult AdditionalResult, IEnumerable<ExtractedHtmlTable> ValueTables)> Read<TResult>(string uri, Func<HtmlNode, TResult> getAdditionalResult = null)
		{
			(TResult, IEnumerable<ExtractedHtmlTable>) result = default;
			string html;
			using (var loader = TextLoader.New(new Uri(uri), ApplicationConfig.Instance.DefaultHttpUserAgent))
			{
				html = await loader.LoadAsync();
			}
			if (html != null)
			{
				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);
				var documentNode = htmlDocument.DocumentNode;
				if (documentNode != null)
				{
					result = (
						getAdditionalResult == null || documentNode == null ? default : getAdditionalResult.Invoke(documentNode),
						documentNode.SelectNodes(XPathExpression.Compile("//table"))?.Select(ExtractedHtmlTable.From) ?? Enumerable.Empty<ExtractedHtmlTable>()
					);
				}
			}

			return result;
		}

		internal static bool TryGetDate(this string input, out DateTime? dateTime)
		{
			DateTime parsed;
			var succeed = !string.IsNullOrEmpty(input) && DateTime.TryParseExact(input, "D", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
				|| DateTime.TryParse(input, out parsed);

			if (!succeed)
			{
				parsed = default;
			}
			dateTime = parsed;

			return succeed;
		}

		internal static string GetText(this HtmlNode node)
		{
			var innerText = node.InnerText;
			var decoded = WebUtility.HtmlDecode(innerText);
			var removedLineBreaking = Regex.Replace(decoded, @"[\r\n\s]+", " ");
			var trimmed = removedLineBreaking.Trim();
			return trimmed;
		}

		[SuppressMessage("Design", "CA1814", Justification = "HTML tables are better to set as multidimensional array.")]
		internal static string[][] ToJaggedArray(this string[,] input)
		{
			int rows = input.GetLength(0);
			int cols = input.GetLength(1);

			string[][] jaggedArray = new string[rows][];

			for (int i = 0; i < rows; i++)
			{
				jaggedArray[i] = new string[cols];

				for (int j = 0; j < cols; j++)
				{
					jaggedArray[i][j] = input[i, j];
				}
			}
			return jaggedArray;
		}
	}

	[SuppressMessage("Design", "CA1814", Justification = "HTML tables are better to set as multidimensional array.")]
	public class ExtractedHtmlTable
	{
		public static ExtractedHtmlTable From(HtmlNode tableNode)
		{
			var captionText = tableNode.SelectSingleNode(XPathExpression.Compile("caption"))?.GetText() ?? string.Empty;
			var headers = tableNode.SelectSingleNode(XPathExpression.Compile("tr[th or ancestor::thead]"))?.SelectNodes(XPathExpression.Compile("th|td")).Select(HtmlTableLoader.GetText).ToArray()
				?? new string[] { };

			var body = ExtractTable(tableNode);

			return new ExtractedHtmlTable(captionText, headers, body);
		}

		static string[,] ExtractTable(HtmlNode tableNode)
		{
			var tableElements = tableNode.SelectNodes(XPathExpression.Compile(".//tr[ancestor::tbody or not(ancestor::thead)]"))
				.Select(
					row => row.SelectNodes(XPathExpression.Compile("th|td")).ToArray()
				)
				.ToArray();

			var valueColumnCount = tableElements.Max(row => row.Length);
			var tableValues = new string[tableElements.Length, valueColumnCount];

			for (var tableRow = 0; tableRow < tableElements.Length; tableRow++)
			{
				var tableRowElement = tableElements[tableRow];
				for (var tableCol = 0; tableCol < tableRowElement.Length; tableCol++)
				{
					var tableCell = tableRowElement[tableCol];

					var valueCol = tableCol;
					while (valueCol < valueColumnCount && !string.IsNullOrWhiteSpace(tableValues[tableRow, valueCol]))
					{
						valueCol++;
					}

					var cellText = tableCell.GetText();
					tableValues[tableRow, valueCol] = cellText;

					const string rowspan = nameof(rowspan);
					if (tableCell.Attributes.Contains(rowspan) && int.TryParse(tableCell.Attributes[rowspan].Value, out var rowspanValue) && rowspanValue > 0)
					{
						for (var idxToApplyRowSpan = tableRow + 1; idxToApplyRowSpan < tableRow + rowspanValue; idxToApplyRowSpan++)
						{
							tableValues[idxToApplyRowSpan, valueCol] = cellText;
						}
					}

					const string colspan = nameof(colspan);
					if (tableCell.Attributes.Contains(colspan) && int.TryParse(tableCell.Attributes[colspan].Value, out var colspanValue) && colspanValue > 0)
					{
						for (var idxToApplyColSpan = tableRow + 1; idxToApplyColSpan < valueCol + colspanValue; idxToApplyColSpan++)
						{
							tableValues[tableRow, idxToApplyColSpan] = cellText;
						}
					}
				}
			}

			return tableValues;
		}

		ExtractedHtmlTable(string caption, string[] headers, string[,] body)
		{
			Caption = caption;
			Headers = headers;
			Body = body.ToJaggedArray();
		}
		public string Caption { get; }
		public string[] Headers { get; }
		public string[][] Body { get; }
	}
}
