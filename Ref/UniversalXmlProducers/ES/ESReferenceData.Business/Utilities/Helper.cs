using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public static class Helper
	{
		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList, UpdateType updateType = UpdateType.Full)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile, false); //TODO: set validation to true on WI00433229
		}

		public static (bool SuccessfullyParsed, DateTime DateTime) GetDateTime(this string dateTimeAsString, string format)
		{
			var result = false;
			var dateTime = DateTime.MinValue;

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

		public static string GetDateWithEsFormat(DateTime date) => date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
	}
}
