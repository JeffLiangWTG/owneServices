using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	public static class ODTFileHelper
	{
		public static DataTable GetTableFromCellContent(byte[] odtData, params string[] headerTags) => GetTableFromCellContent(odtData, null, headerTags);

		public static DataTable GetTableFromCellContent(byte[] odtData, StringBuilder errorCollector, params string[] headerTags)
		{
			var dataTable = new DataTable();
			if (odtData?.Length > 0)
			{
				var xmlDoc = GetDocument(odtData);
				var nsManager = InitializeXmlNamespaceManager(xmlDoc);

				foreach (var headerTag in headerTags)
				{
					(XmlNode xmlTable, XmlNode headerRow) = FindTable(xmlDoc, nsManager, headerTag);
					if (xmlTable != null && headerRow != null)
					{
						dataTable = CreateDataTable(xmlTable, headerRow, nsManager);
						break;
					}
					else
					{
						errorCollector?.AppendLine(CultureInfo.InvariantCulture, $"Could not find the data tag in the source file, looked for '{string.Join("', '", headerTags)}'");
					}
				}
			}
			return dataTable;
		}

		static DataTable CreateDataTable(XmlNode xmlTable, XmlNode headerRow, XmlNamespaceManager nsManager)
		{
			var dataTable = new DataTable();
			var columns = GetCellNodesNonEmptyValue(headerRow, nsManager);
			if (columns.Any())
			{
				dataTable.Columns.AddRange(columns.Select(x => new DataColumn(x)).ToArray());
				var rowNodes = GetRowNodes(xmlTable, nsManager);
				var headerRowIndex = rowNodes.IndexOf(headerRow);

				foreach (var row in rowNodes.Skip(headerRowIndex + 1))
				{
					var cells = GetCellNodes(row, nsManager);
					if (cells.Count > 0)
					{
						var rowData = cells.SelectMany(x => GetDataValues(x, nsManager)).ToList();
						if (rowData.Any(x => !string.IsNullOrEmpty(x)))
						{
							dataTable.Rows.Add(rowData.Take(columns.Length).ToArray());
						}
					}
				}
			}
			return dataTable;
		}

		static (XmlNode table, XmlNode headerRow) FindTable(XmlDocument xmlDoc, XmlNamespaceManager nsManager, string headerTag)
		{
			var nodes = GetTableNodesText(xmlDoc, nsManager);
			if (nodes.Count == 0)
			{
				nodes = GetTableNodesSpreadsheet(xmlDoc, nsManager);
			}

			foreach (XmlNode table in nodes)
			{
				var headerRow = GetHeaderRowNode(table, nsManager);
				if (IsRowContainsHeaderTag(headerRow, nsManager, headerTag))
				{
					return (table, headerRow);
				}

				var rowNodes = GetRowNodes(table, nsManager);
				foreach (var row in rowNodes)
				{
					if (IsRowContainsHeaderTag(row, nsManager, headerTag))
					{
						return (table, row);
					}
				}
			}

			return (null, null);
		}

		static bool IsRowContainsHeaderTag(XmlNode rowNode, XmlNamespaceManager nsManager, string headerTag)
		{
			if (rowNode != null)
			{
				var cells = GetCellNodes(rowNode, nsManager);
				if (cells.Count > 0 && GetDataValue(cells[0], nsManager) == headerTag)
				{
					return true;
				}
			}
			return false;
		}

		static XmlDocument GetDocument(byte[] odtData)
		{
			using (var memStream = new MemoryStream(odtData))
			using (var archive = new ZipArchive(memStream, ZipArchiveMode.Read))
			{
				var readEntry = archive.GetEntry("content.xml");
				using (var reader = new StreamReader(readEntry.Open()))
				{
					var xmlContent = new XmlDocument();
					xmlContent.Load(reader);
					return xmlContent;
				}
			}
		}

		static XmlNodeList GetTableNodesText(XmlDocument contentXmlDocument, XmlNamespaceManager nsManager)
		{
			return contentXmlDocument.SelectNodes("/office:document-content/office:body/office:text/table:table", nsManager);
		}

		static XmlNodeList GetTableNodesSpreadsheet(XmlDocument contentXmlDocument, XmlNamespaceManager nsManager)
		{
			return contentXmlDocument.SelectNodes("/office:document-content/office:body/office:spreadsheet/table:table", nsManager);
		}

		static XmlNode GetHeaderRowNode(XmlNode tableNode, XmlNamespaceManager nsManager)
		{
			return tableNode.SelectSingleNode("table:table-header-rows[1]/table:table-row[1]", nsManager);
		}

		static IList<XmlNode> GetRowNodes(XmlNode tableNode, XmlNamespaceManager nsManager)
		{
			return tableNode.SelectNodes("table:table-row", nsManager).ToList();
		}

		static IList<XmlNode> GetCellNodes(XmlNode rowNode, XmlNamespaceManager nsManager)
		{
			return rowNode.SelectNodes("table:table-cell", nsManager).ToList();
		}

		static string GetDataValue(XmlNode cellNode, XmlNamespaceManager nsManager)
		{
			var textNodes = cellNode.SelectNodes("text:p", nsManager).ToList();

			return string.Join("\n", textNodes.Select(x => x.InnerText));
		}

		static string[] GetCellNodesNonEmptyValue(XmlNode rowNode, XmlNamespaceManager nsManager)
		{
			return GetCellNodes(rowNode, nsManager)
				.Select(cellNode => GetDataValue(cellNode, nsManager))
				.Where(value => !string.IsNullOrEmpty(value))
				.ToArray();
		}

		/// <summary>
		/// Reads the "table:number-columns-repeated" attribute of the cell node and makes copies of the value accordingly
		/// </summary>
		static string[] GetDataValues(XmlNode cellNode, XmlNamespaceManager nsManager)
		{
			var value = GetDataValue(cellNode, nsManager);
			var copies = int.Parse(cellNode.Attributes["table:number-columns-repeated"]?.Value ?? "1", CultureInfo.CurrentCulture);
			var result = new string[copies];
			for (var i = 0; i < copies; i++)
			{
				result[i] = value;
			}
			return result;
		}

		static IList<XmlNode> ToList(this XmlNodeList nodeList)
		{
			return nodeList.Cast<XmlNode>().ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional")]
		static string[,] namespaces = new string[,]
		{
			{ "table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0" },
			{ "office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0" },
			{ "style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0" },
			{ "text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0" },
			{ "draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" },
			{ "fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" },
			{ "dc", "http://purl.org/dc/elements/1.1/" },
			{ "meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0" },
			{ "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0" },
			{ "presentation", "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" },
			{ "svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" },
			{ "chart", "urn:oasis:names:tc:opendocument:xmlns:chart:1.0" },
			{ "dr3d", "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0" },
			{ "math", "http://www.w3.org/1998/Math/MathML" },
			{ "form", "urn:oasis:names:tc:opendocument:xmlns:form:1.0" },
			{ "script", "urn:oasis:names:tc:opendocument:xmlns:script:1.0" },
			{ "ooo", "http://openoffice.org/2004/office" },
			{ "ooow", "http://openoffice.org/2004/writer" },
			{ "oooc", "http://openoffice.org/2004/calc" },
			{ "dom", "http://www.w3.org/2001/xml-events" },
			{ "xforms", "http://www.w3.org/2002/xforms" },
			{ "xsd", "http://www.w3.org/2001/XMLSchema" },
			{ "xsi", "http://www.w3.org/2001/XMLSchema-instance" },
			{ "rpt", "http://openoffice.org/2005/report" },
			{ "of", "urn:oasis:names:tc:opendocument:xmlns:of:1.2" },
			{ "rdfa", "http://docs.oasis-open.org/opendocument/meta/rdfa#" },
			{ "config", "urn:oasis:names:tc:opendocument:xmlns:config:1.0" }
		};

		static XmlNamespaceManager InitializeXmlNamespaceManager(XmlDocument xmlDocument)
		{
			var nsManager = new XmlNamespaceManager(xmlDocument.NameTable);

			for (int i = 0; i < namespaces.GetLength(0); i++)
			{
				nsManager.AddNamespace(namespaces[i, 0], namespaces[i, 1]);
			}

			return nsManager;
		}
	}
}
