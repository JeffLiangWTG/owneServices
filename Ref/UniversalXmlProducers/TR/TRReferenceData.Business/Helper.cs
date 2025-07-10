using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public static class Helper
	{
		public static bool IsNullOrEmpty(this string input) => string.IsNullOrWhiteSpace(input);

		public static (bool SuccessfullyParsed, DateTime DateTime) GetDateTime(this string dateTimeAsString, string format)
		{
			var result = false;
			var dateTime = DateTime.MinValue;

			if (!dateTimeAsString.IsNullOrEmpty() && !format.IsNullOrEmpty())
			{
				result = DateTime.TryParseExact(dateTimeAsString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				var maximumSmallDateTime = Constants.MaximumSmallDateTime;
				if (result && dateTime > maximumSmallDateTime)
				{
					dateTime = maximumSmallDateTime;
				}
			}
			return (result, dateTime);
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList, Dependency dependencies = null)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);
			if (dependencies != null)
			{
				writer.SetDependency(dependencies);
			}

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}

		public static bool Matches(this IEnumerable<string> list, string stringToCheck) => list.Any(prefix => !prefix.IsNullOrEmpty() && stringToCheck.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));
	}
}
