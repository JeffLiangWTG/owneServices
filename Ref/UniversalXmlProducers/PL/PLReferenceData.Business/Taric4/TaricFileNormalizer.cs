using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

public static class TaricFileNormalizer
{
	const string OriginXPath = $"{Taric4Constants.PropertyNodes.Metainfo}/{Taric4Constants.PropertyNodes.OriginType}";
	static readonly IDictionary<string, string> SupportedGroupNodes = Taric4Constants.Collections.GetGroupNodesToItemNodes();

	public static void Normalize(string sourceFile, string targetFile)
	{
		var tempFileName = Path.Combine(Path.GetDirectoryName(targetFile), Path.GetRandomFileName());
		using (var reader = XmlReader.Create(sourceFile))
		using (var writer = XmlWriter.Create(tempFileName, new() { Indent = true }))
		{
			writer.WriteStartDocument();
			writer.WriteStartElement(null, "IsztarHistoryResponse", "http://www.mf.gov.pl/schematy/isztar/ecipSeapHistoriaObi/2014/01");
			writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");

			WriteResultsInfo(reader, writer);

			while (reader.ReadToFollowing("IsztarHistoryItem"))
			{
				WriteHistoryElement(reader, writer);
			}

			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		File.Move(tempFileName, targetFile, true);
	}

	static void WriteResultsInfo(XmlReader reader, XmlWriter writer)
	{
		if (!reader.ReadToFollowing("ResultsInfo"))
		{
			throw new XmlException("ResultsInfo element not found.");
		}

		writer.WriteStartElement("ResultsInfo", string.Empty);
		WriteChildElementIfFound("databaseDate");
		WriteChildElementIfFound("executionDate");
		writer.WriteEndElement();

		void WriteChildElementIfFound(string name) => writer.WriteElementString(
			name,
			reader.ReadToFollowing(name) && reader.Read()
				? reader.Value
				: throw new XmlException($"'{name}' element not found."));
	}

	static void WriteHistoryElement(XmlReader reader, XmlWriter writer)
	{
		if (ReadToDescendant(reader, out var groupName) && SupportedGroupNodes.TryGetValue(groupName, out var supportedItemNode) && reader.Read())
		{
			writer.WriteStartElement("IsztarHistoryItem", string.Empty);
			writer.WriteStartElement(groupName);
			while (reader.ReadToNextSibling(supportedItemNode))
			{
				var node = (XElement)XNode.ReadFrom(reader);
				if ((string)node.XPathSelectElement(OriginXPath) == GetAllowedOrigin(node.Name.LocalName))
				{
					node.WriteTo(writer);
				}
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		else
		{
			reader.Skip();
		}

		static string GetAllowedOrigin(string itemName) => itemName == Taric4Constants.ItemNodes.QuotaDefinition ? "T" : "N";
	}

	static bool ReadToDescendant(XmlReader reader, out string localName)
	{
		int parentDepth = reader.Depth;
		while (reader.Read() && reader.Depth > parentDepth)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				localName = reader.LocalName;
				return true;
			}
		}

		localName = null;
		return false;
	}
}
