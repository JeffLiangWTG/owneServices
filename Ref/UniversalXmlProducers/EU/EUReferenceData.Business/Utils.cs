using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public static class Utils
	{
		public enum DefaultDateTime
		{
			Min, Max, Now, Empty
		}

		public static string GetDateTime(string input, DefaultDateTime defaultDateTime)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(input))
			{
				result = input;
			}
			else
			{
				switch (defaultDateTime)
				{
					case DefaultDateTime.Min:
						result = Constants.MinimumDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
						break;
					case DefaultDateTime.Max:
						result = Constants.MaximumDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
						break;
					case DefaultDateTime.Now:
						result = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
						break;
					default:
						break;
				}
			}

			return result;
		}

		public static string GetFromOADate(object input)
		{
			var result = string.Empty;
			if (input is double)
			{
				var parsed = DateTime.FromOADate((double)input);
				if (parsed < Constants.MinimumDateTime)
				{
					parsed = Constants.MinimumDateTime;
				}
				else if (parsed > Constants.MaximumDateTime)
				{
					parsed = Constants.MaximumDateTime;
				}
				result = parsed.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
			}
			return result;
		}

		public static (bool SuccessfullyParsed, DateTime DateTime) GetDateTimeFromString(this string dateTimeAsString, string format)
		{
			var result = false;
			var dateTime = Constants.MinimumDateTime;

			if (!string.IsNullOrEmpty(dateTimeAsString) && !string.IsNullOrEmpty(format))
			{
				result = DateTime.TryParseExact(dateTimeAsString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				var maximumDateTime = Constants.MaximumDateTime;
				if (result && dateTime > maximumDateTime)
				{
					dateTime = maximumDateTime;
				}
			}
			return (result, dateTime);
		}

		public static (XDocument SourceXml, DateTime PublicationTime) GetXmlFromZipFileAndPublicationTime(string downloadFilePath, string containsValue)
		{
			var filePath = Path.GetFullPath(downloadFilePath);
			using (var archive = ZipFile.OpenRead(filePath))
			{
				XDocument result = null;
				var xmlEntry = archive.Entries.FirstOrDefault(x => x.FullName.Contains(containsValue));
				if (xmlEntry == null)
				{
					return (null, DateTime.MinValue);
				}

				using (var reader = new StreamReader(xmlEntry.Open()))
				{
					result = XDocument.Load(reader);
				}
				return (result, xmlEntry?.LastWriteTime.DateTime ?? DateTime.Now);
			}
		}

		public static XElement FindXElementByAttribute(XDocument xmlDoc, XName xName, string attributeName, string attributeValue)
		{
			return xmlDoc?.Descendants(xName)?.FirstOrDefault(x => x.Attribute(attributeName)?.Value == attributeValue);
		}

		public static string FindXElementValueByAttribute(XElement xElement, XName xName, string attributeName, string attributeValue)
		{
			return xName != null ? xElement?.Descendants(xName)?.FirstOrDefault(x => x.Attribute(attributeName)?.Value == attributeValue)?.Value : null;
		}

		public static string FindXElementValueFirstOne(XElement xElement, XName xName)
		{
			return xName != null ? xElement?.Descendants(xName)?.First().Value : null;
		}
	}
}
