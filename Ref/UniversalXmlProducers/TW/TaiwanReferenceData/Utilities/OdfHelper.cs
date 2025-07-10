using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class OdfHelper
	{
		public static DataTable[] ExtractTableToDataTables(byte[] data)
		{
			var dataTable = Array.Empty<DataTable>();
			if (data?.Length > 0)
			{
				var xmlDoc = GetContentXmlDocument(data);
				var nsManager = GetXmlNamespaceManager(xmlDoc);
				var wrapper = new OdfDocumentWrapper(xmlDoc, nsManager);

				var xmlTables = FindTable(wrapper);
				if (xmlTables != null)
				{
					dataTable = CreateDataTable(xmlTables, wrapper);
				}
			}
			return dataTable;
		}

		public static XmlNodeList ExtractTextHNodeList(byte[] data)
		{
			XmlNodeList textHNodeList = null;
			if (data?.Length > 0)
			{
				var xmlDoc = GetContentXmlDocument(data);
				var nsManager = GetXmlNamespaceManager(xmlDoc);
				textHNodeList = xmlDoc.SelectNodes("/office:document-content/office:body/office:text/text:h", nsManager);
			}
			return textHNodeList;
		}

		static DataTable[] CreateDataTable(XmlNodeList xmlTables, OdfDocumentWrapper wrapper)
		{
			var nsManager = wrapper.NSManager;
			var dataTables = new List<DataTable>();

			foreach (XmlNode xmlTable in xmlTables)
			{
				var dataTable = new DataTable();

				var rows = GetRowNodes(xmlTable, nsManager).ToArray();
				foreach (var row in rows)
				{
					var cells = GetCellNodes(row, nsManager).ToArray();
					var cellCount = cells.Length;

					while (dataTable.Columns.Count < cellCount)
					{
						dataTable.Columns.Add();
					}

					var dataRow = dataTable.Rows.Add();
					for (var j = 0; j < cellCount; j++)
					{
						dataRow[j] = GetDataValue(cells[j], wrapper);
					}
				}

				dataTables.Add(dataTable);
			}

			return dataTables.ToArray();
		}

		static XmlNodeList FindTable(OdfDocumentWrapper wrapper)
		{
			var xmlDoc = wrapper.Document;
			var nsManager = wrapper.NSManager;

			var nodes = GetTableNodesFromText(xmlDoc, nsManager);
			if (nodes.Count == 0)
			{
				nodes = GetTableNodesFromSpreadsheet(xmlDoc, nsManager);
			}

			return nodes;
		}

		static IEnumerable<XmlNode> GetRowNodes(XmlNode tableNode, XmlNamespaceManager nsManager)
		{
			var list = new List<XmlNode>();

			var headerRows = tableNode.SelectNodes("table:table-header-rows/table:table-row", nsManager);
			if (headerRows != null)
			{
				list.AddRange(headerRows.Cast<XmlNode>());
			}

			var rows = tableNode.SelectNodes("table:table-row", nsManager);
			if (rows != null)
			{
				list.AddRange(rows.Cast<XmlNode>());
			}

			return list;
		}

		static string GetDataValue(XmlNode cellNode, OdfDocumentWrapper wrapper)
		{
			var textNodes = new List<XmlNode>();
			textNodes.AddRange(cellNode.SelectNodes("text:p", wrapper.NSManager).Cast<XmlNode>().Where(x => !wrapper.TextLineThroughStyleNames.Contains(x.Attributes?["text:style-name"]?.Value)));
			textNodes.AddRange(cellNode.SelectNodes("text:h", wrapper.NSManager).Cast<XmlNode>().Where(x => !wrapper.TextLineThroughStyleNames.Contains(x.Attributes?["text:style-name"]?.Value)));
			return string.Join("\n", textNodes.Select(x => x.InnerText));
		}

		static IEnumerable<XmlNode> GetCellNodes(XmlNode rowNode, XmlNamespaceManager nsManager)
		{
			var nodes = rowNode.SelectNodes("table:covered-table-cell|table:table-cell", nsManager);
			foreach (XmlNode node in nodes)
			{
				if (node.Name == "table:covered-table-cell" && int.TryParse(node.Attributes["table:number-columns-repeated"]?.Value, out var numberRowsRepeated))
				{
					for (var i = 0; i < numberRowsRepeated - 1; i++)
					{
						yield return node;
					}
				}
				yield return node;
			}
		}

		static XmlNodeList GetTableNodesFromSpreadsheet(XmlDocument contentXmlDocument, XmlNamespaceManager nsManager)
		{
			return contentXmlDocument.SelectNodes("/office:document-content/office:body/office:spreadsheet/table:table", nsManager);
		}

		static XmlNodeList GetTableNodesFromText(XmlDocument contentXmlDocument, XmlNamespaceManager nsManager)
		{
			return contentXmlDocument.SelectNodes("/office:document-content/office:body/office:text/table:table", nsManager);
		}

		public static XmlNamespaceManager GetXmlNamespaceManager(XmlDocument xmlDocument)
		{
			var nsManager = new XmlNamespaceManager(xmlDocument.NameTable);

			for (var i = 0; i < namespaces.GetLength(0); i++)
			{
				nsManager.AddNamespace(namespaces[i, 0], namespaces[i, 1]);
			}

			return nsManager;
		}

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

		static XmlDocument GetContentXmlDocument(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
			{
				var contentEntry = archive.GetEntry("content.xml");
				using (var reader = new StreamReader(contentEntry.Open()))
				{
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(reader);
					return xmlDocument;
				}
			}
		}
	}
}
