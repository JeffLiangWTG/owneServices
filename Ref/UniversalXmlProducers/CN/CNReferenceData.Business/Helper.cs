using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class Helper
	{
		public static XmlWriterConfiguration GetRefExchangeRateZZConfiguration(DateTime startDate, DateTime endDate)
		{
			var exchangeRateZZConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateZZConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateZZConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "CN");
			exchangeRateZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_StartDate, true, startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_EndDate, false, endDate);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateZZConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType, DateTime publicationDateTime)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, publicationDateTime);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 0));
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CN");

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			return writerConfiguration;
		}

		public static XmlWriter GenerateXmlWriter(string dataSource, string codeType, DateTime publicationDateTime, List<RefCusCodeList> refCusCodeList)
		{
			Argument.NotNull(refCusCodeList, nameof(refCusCodeList));

			var xmlWriterConfig = GetRefCusCodeListWriterConfiguration(codeType, publicationDateTime);
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);

			foreach (var code in refCusCodeList)
			{
				writer.PopulateData(code);
			}

			return writer;
		}

		public static void ExportToXMLFile(string outputFile, string dataSource, string codeType, DateTime publicationDateTime, List<RefCusCodeList> refCusCodeList)
		{
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));
			Argument.NotNull(refCusCodeList, nameof(refCusCodeList));

			var writer = GenerateXmlWriter(dataSource, codeType, publicationDateTime, refCusCodeList);
			if (writer != null)
			{
				var directoryName = Path.GetDirectoryName(outputFile);
				Directory.CreateDirectory(directoryName);
				writer.SaveXml(outputFile);

				GlobalOption.Instance.OutputFiles.Add(outputFile);
			}
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> entities, UpdateType updateType = UpdateType.Full)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var entity in entities)
			{
				writer.PopulateData(entity);
			}
			writer.SaveXml(outputFile);

			GlobalOption.Instance.OutputFiles.Add(outputFile);
		}
	}
}
