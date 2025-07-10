using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.Business
{
	public static class Helper
	{
		public static DateTime GetDateTime(this string dateTimeAsString, string format)
		{
			var dateTime = DateTime.MinValue;

			if (!string.IsNullOrEmpty(dateTimeAsString) && !string.IsNullOrEmpty(format))
			{
				if (DateTime.TryParseExact(dateTimeAsString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
				{
					var maximumDateTime = Constants.MaximumDateTime;
					if (dateTime > maximumDateTime)
					{
						dateTime = maximumDateTime;
					}
				}
			}
			return dateTime;
		}

		public static void ExportToXmlFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList, string dependingOn = null)
		{
			ExportToXmlFile(dataSource, outputFile, xmlWriterConfig, publicationDateTime, UpdateType.Full, codeList, dependingOn);
		}

		public static void ExportToXmlFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, UpdateType updateType, IEnumerable<T> codeList, string dependingOn = null)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);
			if (dependingOn != null)
			{
				writer.SetDependency(new Dependency(dependingOn, publicationDateTime));
			}

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}
	}
}
