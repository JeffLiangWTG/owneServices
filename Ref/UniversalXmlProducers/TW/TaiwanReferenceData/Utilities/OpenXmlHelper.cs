using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class OpenXmlHelper
	{
		public static XDocument GetDocument(byte[] odtData)
		{
			using (var memStream = new MemoryStream(odtData))
			using (var archive = new ZipArchive(memStream, ZipArchiveMode.Read))
			{
				var readEntry = archive.GetEntry("content.xml");
				using (XmlReader reader = XmlReader.Create(readEntry.Open()))
				{
					return XDocument.Load(reader);
				}
			}
		}

		public static class Namespaces
		{
			public static readonly XNamespace Office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
			public static readonly XNamespace Table = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
			public static readonly XNamespace Text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
		}
	}
}
