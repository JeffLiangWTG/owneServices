using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NPOI.SS.UserModel;
using XmlWriter = CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriter;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public static class Helper
	{
		public static T DeserializeFromString<T>(string xmlData)
		{
			T result;
			var serializer = new XmlSerializer(typeof(T));
			using (var strReader = new StringReader(xmlData))
			{
				using (var xmlReader = XmlReader.Create(strReader))
				{
					result = (T)serializer.Deserialize(xmlReader);
				}
			}
			return result;
		}

		public static (bool SuccessfullyParsed, DateTime DateTime) GetDateTime(this string dateTimeAsString, string format)
		{
			var result = false;
			var dateTime = DateTime.MinValue;

			if (!string.IsNullOrEmpty(dateTimeAsString) && !string.IsNullOrEmpty(format))
			{
				result = DateTime.TryParseExact(dateTimeAsString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				var maximumDateTime = DateTime.MaxValue;
				if (result && dateTime > maximumDateTime)
				{
					dateTime = maximumDateTime;
				}
			}
			return (result, dateTime);
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> refDBEntities)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in refDBEntities)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}

		public static DateTime GetDateFromRow(IRow row, MappingEntityType entityType, string rateDateName)
		{
			var rateDateProperty = entityType.Properties.FirstOrDefault(x => x.Name == rateDateName);
			var dateCell = row.GetCell(rateDateProperty.ExcelColumn);
			dateCell?.SetCellType(CellType.String);
			var dateValue = dateCell?.StringCellValue ?? string.Empty;

			DateTime.TryParseExact(dateValue, rateDateProperty.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result);
			return result.Date;
		}
	}
}
