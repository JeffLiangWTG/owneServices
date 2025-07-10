using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.ChiefHarmonisedDeclarationCode
{
	public static class ChiefHarmonisedDeclarationCodeExtractor
	{
		public static IEnumerable<ChiefHarmonisedData> Extract(string zipOutputFileName)
		{
			return ReadZipContent(zipOutputFileName);
		}
		static IEnumerable<ChiefHarmonisedData> ReadZipContent(string zipOutputFileName)
		{
			if (File.Exists(zipOutputFileName))
			{
				using (var zipArchive = ZipFile.OpenRead(zipOutputFileName))
				{
					var entry = zipArchive.Entries.FirstOrDefault(x => x.FullName.Equals("content.xml", StringComparison.OrdinalIgnoreCase));
					if (entry != null)
					{
						using (var contentStream = entry.Open())
						{
							var contentXML = new XmlDocument();
							contentXML.Load(contentStream);
							return ExtractContentData(contentXML);
						}
					}
					else
					{
						throw new FileNotFoundException("Downloaded Zip file for Chief Harmonised Data has changed structure and does not contain a content.xml file.");
					}
				}
			}
			else
			{
				throw new FileNotFoundException($"Downloaded Zip file for Chief Harmonised Data does not exist. Path: {zipOutputFileName}");
			}
		}

		static IEnumerable<ChiefHarmonisedData> ExtractContentData(XmlDocument contentXML)
		{
			var list = new List<ChiefHarmonisedData>();
			foreach (XmlNode node in contentXML.GetElementsByTagName("table:table-row"))
			{
				var nodeCount = node.ChildNodes.Count;
				if (nodeCount >= 6)
				{
					var code = node.ChildNodes.Item(0).InnerText.Trim().ToUpper(CultureInfo.CurrentCulture);
					if (code == "CODE")
					{
						continue;
					}

					var importExport = GetImportExportValue(node.ChildNodes.Item(1).InnerText.Trim().ToUpper(CultureInfo.CurrentCulture));
					var level = GetLevelValue(node.ChildNodes.Item(2).InnerText.Trim().ToUpper(CultureInfo.CurrentCulture));
					var description = MergeText(node.ChildNodes.Item(3));
					var details = MergeText(node.ChildNodes.Item(4));

					//Code C690 has blank Import/Export and Level and only 1 cell table in the XML with a repeated 2 on the end.
					if (code == "C690")
					{
						importExport = "BOTH";
						level = "BOTH";
						description = MergeText(node.ChildNodes.Item(2));
						details = MergeText(node.ChildNodes.Item(3));
					}

					list.Add(new ChiefHarmonisedData(code, importExport, level, description, details));
				}
			}
			return list;
		}

		static string GetImportExportValue(string nodeValue) => nodeValue == "IMPORT" || nodeValue == "EXPORT" ? nodeValue : "BOTH";

		static string GetLevelValue(string nodeValue) => nodeValue == "HEADER" || nodeValue == "ITEM" ? nodeValue : "BOTH";

		static string MergeText(XmlNode sourceNode)
		{
			var merged = "";
			for (var i = 0; i < sourceNode.ChildNodes.Count; i++)
			{
				var node = sourceNode.ChildNodes.Item(i);
				if (node.Name.Equals("text:p", StringComparison.Ordinal))
				{
					merged = merged + node.InnerText;
					if (i < sourceNode.ChildNodes.Count - 1)
					{
						merged = merged + "\n";
					}
				}
			}
			return merged;
		}
	}
}
