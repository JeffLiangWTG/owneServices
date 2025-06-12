using System;
using System.IO;
using System.Xml;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	static class XmlStreamHelper
	{
		public static void CloneXmlStream(XmlReader reader, XmlWriter writer, string newNsPrefix, string newNamespace)
		{
			if (reader == null) { throw new ArgumentNullException("reader"); }
			if (writer == null) { throw new ArgumentNullException("writer"); }

			writer.WriteStartElement(newNsPrefix, reader.LocalName, newNamespace);
			writer.WriteAttributes(reader, false);

			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						writer.WriteNode(reader.ReadSubtree(), false);
						break;

					case XmlNodeType.Text:
						writer.WriteString(reader.Value);
						break;

					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
						writer.WriteWhitespace(reader.Value);
						break;

					case XmlNodeType.CDATA:
						writer.WriteCData(reader.Value);
						break;

					case XmlNodeType.EntityReference:
						writer.WriteEntityRef(reader.Name);
						break;

					case XmlNodeType.XmlDeclaration:
					case XmlNodeType.ProcessingInstruction:
						writer.WriteProcessingInstruction(reader.Name, reader.Value);
						break;

					case XmlNodeType.DocumentType:
						writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
						break;

					case XmlNodeType.Comment:
						writer.WriteComment(reader.Value);
						break;
				}
			}
			writer.WriteEndElement();
		}

		public static string GetElementAsString(this XmlReader reader, string name, string namespaceName = "")
		{
			if (reader.ReadToFollowing(name, namespaceName))
			{
				return reader.ReadElementContentAsString();
			}

			return string.Empty;
		}

		public static Stream GetElementAsStream(this XmlReader reader, string name, string namespaceName = "")
		{

			if (reader.LocalName != name || reader.NamespaceURI != namespaceName)
			{
				reader.ReadToFollowing(name, namespaceName);
			}

			if (!reader.EOF)
			{
				reader.Read();
				var result = new Microsoft.BizTalk.Streaming.VirtualStream(Microsoft.BizTalk.Streaming.VirtualStream.MemoryFlag.AutoOverFlowToDisk);
				reader.WriteToStream(result);
				return result;
			}

			return null;
		}
	}
}
