using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business
{
	public static class Helper
	{
		public static T DeserializeFromString<T>(string xmlData)
		{
			T result;
			var serializer = new XmlSerializer(typeof(T));
			using (var stringReader = new StringReader(xmlData))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				result = (T)serializer.Deserialize(xmlReader);
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
				var maximumDateTime = Constants.MaximumDateTime;
				if (result && dateTime > maximumDateTime)
				{
					dateTime = maximumDateTime;
				}
			}
			return (result, dateTime);
		}

		public static string KeepNumerics(this string originalString)
		{
			var result = new StringBuilder(originalString.Length);
			foreach (char c in originalString)
			{
				if ("0123456789,".Contains(c))
				{
					result.Append(c);
				}
			}
			return result.ToString();
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList)
		{
			var writer = new Common.UniversalXmlWriter.XmlWriter(xmlWriterConfig);
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

		public static bool CheckExpiredDateTime(DateTime endDate, string timeZoneID)
		{
			TimeZoneInfo.ConvertTimeToUtc(endDate, TimeZoneInfo.FindSystemTimeZoneById(timeZoneID));
			return endDate < DateTime.UtcNow;
		}

		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "IMP");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffNationalCodes, false);

			var nationalCodesConfiguration = new EntityTypeConfiguration<RefCusTariffNationalCode>(true);
			nationalCodesConfiguration.IncludeColumn(x => x.ZZW_NationalCode, true);
			nationalCodesConfiguration.IncludeColumn(x => x.ZZW_Description, false);
			nationalCodesConfiguration.IncludeColumn(x => x.ZZW_ZZF_NKTaxOrFeeCode, false);
			nationalCodesConfiguration.IncludeColumn(x => x.ZZW_StartDate, false);
			nationalCodesConfiguration.IncludeColumn(x => x.ZZW_EndDate, false);
			nationalCodesConfiguration.IncludeColumnWithConstantValue(x => x.ZZW_ZZZ_NKDataGrouping, true, "DE");

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(nationalCodesConfiguration);

			return writerConfiguration;
		}
	}
}
