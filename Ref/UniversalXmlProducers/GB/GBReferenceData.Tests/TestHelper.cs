using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests
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
			if (resourceDetails.ToLower(CultureInfo.CurrentCulture).EndsWith(".xml") && result.EndsWith("\r\n"))
			{
				result = result.Substring(0, result.Length - 2);
			}

			return result;
		}

		internal static byte[] ReadManifestResourceContentBytes(string resourceDetails)
		{
			var data = new byte[] { };

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var memStream = new MemoryStream())
			{
				stream.CopyTo(memStream);

				data = memStream.ToArray();
			}

			return data;
		}

		internal static void SimulateDownload(string fileName, string resourceDetails)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var writer = new FileStream(fileName, FileMode.Create))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);

				writer.Flush();
			}
		}

		internal static long GetManifestResourceLength(string resourceDetails)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			{
				return stream.Length;
			}
		}

		internal static Stream GetResourceStream(string resourceDetails)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails);
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

		internal static string ConvertToCsv<T>(IEnumerable<T> items, Func<T, string[]> selector)
		{
			var stringBuilder = new StringBuilder();
			foreach (var item in items)
			{
				var values = selector(item).Select(value => EscapeCsvValue(value));
				stringBuilder.AppendLine(string.Join(",", values));
			}
			return stringBuilder.ToString();
		}

		static string EscapeCsvValue(string value)
		{
			return value.Contains(",") || value.Contains("\"") ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
		}
	}
}
