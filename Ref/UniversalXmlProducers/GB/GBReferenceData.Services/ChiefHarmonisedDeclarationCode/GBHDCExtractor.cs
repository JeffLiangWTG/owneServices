using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.GBReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GBHarmonisedDeclarationCode
{
	public class GBHDCExtractor
	{
		public static IEnumerable<GBHDCData> Extract(string zipOutputFileName)
		{
			return ReadZipContent(zipOutputFileName);
		}
		static IEnumerable<GBHDCData> ReadZipContent(string zipOutputFileName)
		{
			if (File.Exists(zipOutputFileName))
			{
				using (var zipArchive = ZipFile.OpenRead(zipOutputFileName))
				{
					var entry = zipArchive.Entries.FirstOrDefault(x => x.FullName.Equals("content.xml", StringComparison.OrdinalIgnoreCase));
					using (var contentStream = entry.Open())
					{
						var contentXML = new XmlDocument();
						contentXML.Load(contentStream);
						return ExtractContentData(contentXML);
					}
				}
			}
			return null;
		}
		static IEnumerable<GBHDCData> ExtractContentData(XmlDocument contentXML)
		{
			var list = new List<GBHDCData>();
			foreach (XmlNode node in contentXML.GetElementsByTagName("table:table-row"))
			{
				if (node.ChildNodes.Count > 4)
				{
					var code = node.ChildNodes.Item(0).InnerText.Trim().ToUpper();

					if (code == "CODE")
					{
						continue;
					}

					var ie = string.IsNullOrWhiteSpace(node.ChildNodes.Item(1).InnerText) ? "BOTH" : node.ChildNodes.Item(1).InnerText.Trim().ToUpper();
					var level = string.IsNullOrWhiteSpace(node.ChildNodes.Item(2).InnerText) ? "BOTH" : node.ChildNodes.Item(2).InnerText.Trim().ToUpper();
					var des = MergeText(node.ChildNodes.Item(3));
					var det = MergeText(node.ChildNodes.Item(4));
					list.Add(new GBHDCData(code, ie, level, des, det));
				}
			}
			return list;
		}
		static string MergeText(XmlNode sourceNode)
		{
			var merged = "";
			for (var i = 0; i < sourceNode.ChildNodes.Count; i++)
			{
				XmlNode node = sourceNode.ChildNodes.Item(i);
				if (node.Name.Equals("text:p"))
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
