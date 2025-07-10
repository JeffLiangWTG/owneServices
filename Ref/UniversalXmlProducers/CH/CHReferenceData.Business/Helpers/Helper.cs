using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using XmlWriter = CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriter;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	internal static class Helper
	{
		internal static T DeserializeXML<T>(byte[] content)
		{
			using (var reader = XmlReader.Create(new MemoryStream(content)))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(reader);
			}
		}

		internal static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> list)
			=> ExportToXMLFile<T>(dataSource, outputFile, xmlWriterConfig, publicationDateTime, list, UpdateType.Full);

		internal static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> list, UpdateType updateType)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var item in list)
			{
				writer.PopulateData(item);
			}
			writer.SaveXml(outputFile);
		}

		internal static DateTime EndOfDay(this DateTime date) => date.Date.AddHours(23).AddMinutes(59);

		internal static DateTime Truncate(this DateTime date)
		{
			if (date < MinimumDateTime)
			{
				date = MinimumDateTime;
			}
			else if (date > MaximumDateTime)
			{
				date = MaximumDateTime;
			}
			return date;
		}

		internal static DateTime Min(DateTime date1, DateTime date2)
		{
			return date1 < date2 ? date1 : date2;
		}

		internal static DateTime Max(DateTime date1, DateTime date2)
		{
			return date1 > date2 ? date1 : date2;
		}

		internal static string Truncate(this string value, int maxLength)
		{
			if (value.Length > maxLength)
			{
				value = value.Substring(0, maxLength);
			}
			return value;
		}

		internal static string PadLeftMissingZero(this string value)
		{
			return !string.IsNullOrEmpty(value) && value[0] == '.' ? $"0{value}" : value;
		}

		internal static readonly DateTime MinimumDateTime = new DateTime(1900, 01, 01, 00, 00, 00);

		internal static readonly DateTime MaximumDateTime = new DateTime(2079, 06, 06, 00, 00, 00);

		internal static bool IsUselessDescription(string desc)
		{
			bool result = false;

			if (string.IsNullOrEmpty(desc) || (desc.Length <= 2) || (desc == "f - f") || (desc == "i - i") || (desc == "e - e") ||
				desc.StartsWith("fr_", StringComparison.InvariantCulture) || desc.StartsWith("it_", StringComparison.InvariantCulture) || desc.StartsWith("en_", StringComparison.InvariantCulture) ||
				desc.StartsWith("fra_", StringComparison.InvariantCulture) || desc.StartsWith("ita_", StringComparison.InvariantCulture) || desc.StartsWith("eng_", StringComparison.InvariantCulture))
			{
				result = true;
			}
			return result;
		}

		internal static void SetDynamicListItem<T>(this List<T> keys, int level, T value)
		{
			while (level >= keys.Count)
				keys.Add(default);
			keys[level] = value;
		}
	}
}
