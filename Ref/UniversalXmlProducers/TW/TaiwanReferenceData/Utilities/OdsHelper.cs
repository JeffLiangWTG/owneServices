using System.IO;
using System.IO.Compression;
using System.Xml;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class OdsHelper
	{
		private static string[,] namespaces = new string[,]
		{
			{"table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0"},
			{"office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0"},
			{"style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0"},
			{"text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0"},
			{"draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0"},
			{"fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0"},
			{"dc", "http://purl.org/dc/elements/1.1/"},
			{"meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0"},
			{"number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0"},
			{"presentation", "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0"},
			{"svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0"},
			{"chart", "urn:oasis:names:tc:opendocument:xmlns:chart:1.0"},
			{"dr3d", "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0"},
			{"math", "http://www.w3.org/1998/Math/MathML"},
			{"form", "urn:oasis:names:tc:opendocument:xmlns:form:1.0"},
			{"script", "urn:oasis:names:tc:opendocument:xmlns:script:1.0"},
			{"ooo", "http://openoffice.org/2004/office"},
			{"ooow", "http://openoffice.org/2004/writer"},
			{"oooc", "http://openoffice.org/2004/calc"},
			{"dom", "http://www.w3.org/2001/xml-events"},
			{"xforms", "http://www.w3.org/2002/xforms"},
			{"xsd", "http://www.w3.org/2001/XMLSchema"},
			{"xsi", "http://www.w3.org/2001/XMLSchema-instance"},
			{"rpt", "http://openoffice.org/2005/report"},
			{"of", "urn:oasis:names:tc:opendocument:xmlns:of:1.2"},
			{"rdfa", "http://docs.oasis-open.org/opendocument/meta/rdfa#"},
			{"config", "urn:oasis:names:tc:opendocument:xmlns:config:1.0"}
		};

		public static XmlNamespaceManager InitializeXmlNamespaceManager(XmlDocument xmlDocument)
		{
			var nmsManager = new XmlNamespaceManager(xmlDocument.NameTable);

			for (int i = 0; i < namespaces.GetLength(0); i++)
			{
				nmsManager.AddNamespace(namespaces[i, 0], namespaces[i, 1]);
			}

			return nmsManager;
		}

		public static XmlDocument GetDocument(Stream inputStream)
		{
			using (var archive = new ZipArchive(inputStream, ZipArchiveMode.Read))
			{
				var readEntry = archive.GetEntry("content.xml");
				using (var reader = new StreamReader(readEntry.Open()))
				{
					var contentXml = new XmlDocument();
					contentXml.Load(reader);
					return contentXml;
				}
			}
		}

		public static XmlNodeList GetTableNodes(XmlDocument contentXmlDocument, XmlNamespaceManager nmsManager)
		{
			return contentXmlDocument.SelectNodes("/office:document-content/office:body/office:spreadsheet/table:table", nmsManager);
		}

		public static string GetTableName(XmlNode tableNode)
		{
			var attribute = (XmlAttribute)tableNode.Attributes.GetNamedItem("table:name");
			return attribute?.Value ?? string.Empty;
		}

		public static XmlNodeList GetRowNodes(XmlNode tableNode, XmlNamespaceManager nmsManager)
		{
			return tableNode.SelectNodes("table:table-row", nmsManager);
		}

		public static XmlNodeList GetCellNodes(XmlNode rowNode, XmlNamespaceManager nmsManager)
		{
			return rowNode.SelectNodes("table:table-cell", nmsManager);
		}

		public static string GetDataValue(XmlNode cellNode)
		{
			return cellNode.InnerText;
		}
	}
}
