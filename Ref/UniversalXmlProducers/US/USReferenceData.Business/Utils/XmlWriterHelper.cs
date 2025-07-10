using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public static class XmlWriterHelper
	{
		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, List<T> dataList, UpdateType updateType = UpdateType.Full)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in dataList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}

		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration(string tariffType)
		{
			var refCusTariff = new EntityTypeConfiguration<RefCusTariff>(true);
			refCusTariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariff.IncludeColumn(x => x.ZZ1_Description, false);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, tariffType);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			refCusTariff.IncludeColumn(x => x.ZZ1_StartDate, false);
			refCusTariff.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			refCusTariff.IncludeColumn(x => x.RefCusTariffUOMs, false);
			refCusTariff.IncludeColumn(x => x.RefCusConditions);
			refCusTariff.IncludeColumn(x => x.RefCusTariffAttributes);

			var refCusTariffUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			refCusTariffUOMConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			refCusTariffUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			refCusTariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, false, Constants.USCountryCode);

			var refCusConditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_NKConditionType, true, Constants.ConditionType.PGA);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_StartDate, false, Constants.DefaultValues.MinDateTime);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, false, Constants.USCountryCode);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			refCusConditionConfiguration.IncludeColumn(x => x.RefCusConditionValues);

			var refCusConditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);

			var refCusTariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			refCusTariffAttributeConfiguration.IncludeColumnWithConstantValue(x => x.ZZ3_Name, true, Constants.AttributeNames.EV1);
			refCusTariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariff);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffUOMConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionValueConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetNewWatchTariffWriterConfiguration(string attributeName, string attributeValue)
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.HSN);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, Constants.DefaultValues.MinDateTime);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes, false);

			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumnWithConstantValue(x => x.ZZ3_Name, true, attributeName);
			tariffAttributeConfiguration.IncludeColumnWithConstantValue(x => x.ZZ3_Value, false, attributeValue);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType, DateTime? startDate = null, bool enableAttribute = true, bool enableTransport = false, bool isKeyColumnForAttributeValue = true)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, startDate ?? new DateTime(1900, 01, 01, 0, 0, 0));
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 0));
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.USCountryCode);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			if (enableAttribute)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes);
				var refCusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				refCusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, isKeyColumnForAttributeValue);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);
			}

			if (enableTransport)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes);
				var refCusCodeOrAttributeTransportMode = new EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode>(true);
				refCusCodeOrAttributeTransportMode.IncludeColumn(o => o.ZZU_TransportMode, true);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeOrAttributeTransportMode);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(DateTime startDate, DateTime endDate)
		{
			var exchangeRate = new EntityTypeConfiguration<RefExchangeRateZZ>(true);

			exchangeRate.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, Constants.ExchangeRate.DefaultRateType);
			exchangeRate.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.USCountryCode);

			exchangeRate.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, true, startDate.Date, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRate.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, endDate.Date);

			exchangeRate.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRate.IncludeColumn(x => x.ZZN_Rate, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRate);

			return writerConfiguration;
		}
	}
}
