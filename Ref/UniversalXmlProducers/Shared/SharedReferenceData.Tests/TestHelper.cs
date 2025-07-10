using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System.Globalization;

namespace CargoWise.RefDbRepo.SharedReferenceData.Tests
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = GetResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			// Annoying VS XML editor adds \r\n at the end of the file when saving
			if (resourceDetails.ToLower(CultureInfo.CurrentCulture).EndsWith(".xml") && result.EndsWith("\r\n"))
			{
				result = result.Substring(0, result.Length - 2);
			}

			return result;
		}

		internal static void SimulateDownload(string fileName, string resourceDetails)
		{
			using (var stream = GetResourceStream(resourceDetails))
			using (var writer = new FileStream(fileName, FileMode.Create))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);

				writer.Flush();
			}
		}

		internal static Stream GetResourceStream(string resourceDetails)
		{
			var assembly = Assembly.GetExecutingAssembly();

			var resourceNames = assembly.GetManifestResourceNames();

			if (!resourceNames.Contains(resourceDetails))
			{
				throw new InvalidOperationException($"Resource '{resourceDetails}' not found. Possible values are:\n{string.Join("\n", resourceNames)}");
			}

			return assembly.GetManifestResourceStream(resourceDetails);
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
	}
}
