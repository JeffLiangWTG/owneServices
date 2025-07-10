using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Utilities
{
	public static class XmlHash
	{
		public static string HashStream(Stream inputStream, string[] excludeElements)
		{
			var originalPosition = inputStream.Position;
			var tempFilePath = Path.GetTempFileName();

			try
			{
				using var outputStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite);
				RemoveElementsFromXML(inputStream, outputStream, excludeElements);
				var hash = StreamHashCalculator.CalculateHash(outputStream);
				inputStream.Position = originalPosition; // reset input stream position
				return hash;
			}
			finally
			{
				if (File.Exists(tempFilePath))
				{
					File.Delete(tempFilePath);
				}
			}
		}

		static void RemoveElementsFromXML(Stream inputStream, Stream outputStream, string[] excludeElements)
		{
			var excludeSet = new HashSet<string>(excludeElements ?? [], StringComparer.OrdinalIgnoreCase);
			var settings = new XmlReaderSettings { IgnoreWhitespace = true };

			using (var reader = XmlReader.Create(inputStream, settings))
			using (var writer = XmlWriter.Create(outputStream, new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Encoding = Encoding.UTF8
			}))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && excludeSet.Contains(reader.Name))
					{
						// Skip the entire subtree of the excluded element
						reader.ReadSubtree().Close();
						continue;
					}

					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							writer.WriteStartElement(reader.Name);
							if (reader.HasAttributes)
							{
								while (reader.MoveToNextAttribute())
								{
									writer.WriteAttributeString(reader.Name, reader.Value);
								}
								reader.MoveToElement();
							}
							if (reader.IsEmptyElement)
							{
								writer.WriteEndElement();
							}
							break;

						case XmlNodeType.Text:
							writer.WriteString(reader.Value);
							break;

						case XmlNodeType.CDATA:
							writer.WriteCData(reader.Value);
							break;

						case XmlNodeType.EndElement:
							writer.WriteEndElement();
							break;

						case XmlNodeType.XmlDeclaration:
						case XmlNodeType.ProcessingInstruction:
						case XmlNodeType.Comment:
						case XmlNodeType.DocumentType:
							// Skip these node types
							break;
					}
				}

			}
			outputStream.Flush();
			outputStream.Position = 0;
		}
	}
}
