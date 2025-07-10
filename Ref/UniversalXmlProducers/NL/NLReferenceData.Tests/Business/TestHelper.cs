using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			// Annoying VS XML editor adds \r\n at the end of the file when saving
			if (resourceDetails.ToUpper(CultureInfo.InvariantCulture).EndsWith(".XML", StringComparison.InvariantCulture) && result.EndsWith("\r\n", System.StringComparison.InvariantCulture))
			{
				result = result.Substring(0, result.Length - 2);
			}

			return result;
		}

		internal static bool SimulateDownload(string fileName, string resourceDetails)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var writer = new FileStream(fileName, FileMode.Create))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);

				writer.Flush();
				return true;
			}
		}

		internal static XmlDocument GetResourceContentAsXmlDocument(string resourceString)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceString));
			return xmlDoc;
		}

		internal static XElement GetXmlElement(string elementName, string resourceDetails, int skip = 0)
		{
			using (var xmlReader = XmlReader.Create(GetResourceStream(resourceDetails)))
			{
				for (int i = 0; i < skip; i++)
				{
					// Skip elements for specific testing
					xmlReader.ReadToFollowing(elementName);
				}

				xmlReader.ReadToFollowing(elementName);

				if (XNode.ReadFrom(xmlReader) is XElement element)
				{
					return element;
				}
				else
				{
					throw new Exception("Unit Test XML content not read correctly");
				}
			}
		}
		internal static Stream GetResourceStream(string resourceDetails)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails);
		}
	}
}
