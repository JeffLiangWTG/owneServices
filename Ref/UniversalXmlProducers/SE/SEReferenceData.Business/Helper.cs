using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.SEReferenceData.Business
{
	public static class Helper
	{
		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}

		public static (bool SuccessfullyParsed, DateTime DateTime) GetDateTime(this string dateTimeAsString, string format)
		{
			var result = false;
			var dateTime = DateTime.MinValue;

			if (!string.IsNullOrEmpty(dateTimeAsString) && !string.IsNullOrEmpty(format))
			{
				result = DateTime.TryParseExact(dateTimeAsString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				if (result && dateTime > Constants.MaximumDateTime)
				{
					dateTime = Constants.MaximumDateTime;
				}
				if (result && dateTime < Constants.MinimumDateTime)
				{
					dateTime = Constants.MinimumDateTime;
				}
			}

			return (result, dateTime);
		}

		public static DateTime FixIfMissingEndDate(this DateTime inputDate)
		{
			return inputDate == Constants.AnnoDominiOne ? Constants.MaximumDateTime : inputDate;
		}
	}
}
