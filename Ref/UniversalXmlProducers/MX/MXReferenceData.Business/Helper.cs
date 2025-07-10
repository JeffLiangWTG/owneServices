using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public static class Helper
	{
		public static XmlWriterConfiguration GetRefExchangeRateZZWriterConfiguration(string codeType, string constantCurrency)
		{
			Argument.NotNullOrEmpty(codeType, nameof(codeType));

			var writerConfiguration = new XmlWriterConfiguration();

			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, codeType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);

			if (constantCurrency != null)
			{
				exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RX_NKExCurrency, true, constantCurrency);
			}
			else
			{
				exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			}

			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DataGroupingCodes.Mexico);

			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeTypeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeTypeConfiguration = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_CodeType, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_Description, false);
			codeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Mexico);
			codeTypeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZK_MaxLength, false, 0);
			writerConfiguration.IncludeEntityTypeConfiguration(codeTypeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType, bool hasAttributes = false, bool attributeAllowDuplicates = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, new DateTime(1900, 01, 01, 0, 0, 0));
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 0));
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Mexico);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			return writerConfiguration;
		}

		public static void ExportToXMLFile<T>(string outputFile, string dataSource, DateTime publicationDateTime, XmlWriterConfiguration xmlWriterConfig, IEnumerable<T> dataList, UpdateType? updateType = UpdateType.Full)
		{
			Argument.NotNull(dataList, nameof(dataList));
			var writer = GenerateXmlWriter(dataSource, publicationDateTime, xmlWriterConfig, dataList, updateType);
			if (writer != null)
			{
				var directoryName = Path.GetDirectoryName(outputFile);
				Directory.CreateDirectory(directoryName);
				writer.SaveXml(outputFile);
			}
		}

		static XmlWriter GenerateXmlWriter<T>(string dataSource, DateTime publicationDateTime, XmlWriterConfiguration xmlWriterConfig, IEnumerable<T> dataList, UpdateType? updateType = UpdateType.Full)
		{
			Argument.NotNull(dataList, nameof(dataList));
			Argument.NotNull(xmlWriterConfig, nameof(xmlWriterConfig));

			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType.Value);
			foreach (var code in dataList)
			{
				writer.PopulateData(code);
			}

			return writer;
		}
	}
}
