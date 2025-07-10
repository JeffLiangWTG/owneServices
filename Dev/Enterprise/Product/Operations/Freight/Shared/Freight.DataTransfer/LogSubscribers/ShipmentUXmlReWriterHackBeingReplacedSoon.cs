using System.IO;
using System.Xml;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;
using XmlWriter = System.Xml.XmlWriter;

namespace Enterprise.Freight.DataTransfer
{
	public sealed class ShipmentUXmlReWriterHackBeingReplacedSoon : IXmlWriter
	{
		public ShipmentUXmlReWriterHackBeingReplacedSoon(string userDefinedXmlNamespace)
		{
			this.userDefinedXmlNamespace = userDefinedXmlNamespace;
		}
		readonly string userDefinedXmlNamespace;

		public void WriteXML(IDataObject dataStructure, SubStreamableStream outputStream, string nameSpace = null, IDataOverrideProvider overrideProvider = null)
		{
			var writer = new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter();

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				writer.WriteXML(dataStructure, stream, nameSpace, overrideProvider);
				ReplaceXmlWithNamespace(stream, outputStream, userDefinedXmlNamespace);
			}

			outputStream.Seek(0, SeekOrigin.Begin);
		}

		#region Implementations

		void ReplaceXmlWithNamespace(SubStreamableStream stream, Stream outputStream, string xmlNamespace)
		{
			using (var reader = XmlReader.Create(stream))
			{
				var ws = new XmlWriterSettings();
				ws.Indent = true;

				using (var writer = XmlWriter.Create(outputStream, ws))
				{
					while (reader.Read())
					{
						switch (reader.NodeType)
						{
							case XmlNodeType.Element:
								ProcessElement(reader, writer, xmlNamespace);
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
							case XmlNodeType.EndElement:
								writer.WriteFullEndElement();
								break;
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		void ProcessElement(XmlReader reader, XmlWriter writer, string ns)
		{
			writer.WriteStartElement(reader.Prefix, reader.LocalName, ns);

			if (reader.AttributeCount > 0 && reader.GetAttribute("xmlns") != null)
			{
				while (reader.MoveToNextAttribute())
				{
					if (reader.Name == "xmlns")
					{
						writer.WriteAttributeString("xmlns", ns);
					}
					else
					{
						writer.WriteAttributeString(reader.Name, reader.Value);
					}
				}
			}
			else
			{
				writer.WriteAttributes(reader, false);
			}

			if (reader.IsEmptyElement)
			{
				writer.WriteEndElement();
			}

			reader.MoveToElement();
		}

		#endregion
	}
}
