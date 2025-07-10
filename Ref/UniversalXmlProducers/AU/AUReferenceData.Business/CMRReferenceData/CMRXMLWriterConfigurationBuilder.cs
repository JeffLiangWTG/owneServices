using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class CMRXMLWriterConfigurationBuilder
	{
		public static XmlWriterConfiguration BuildRefCusCodeListConfiguration(string codeType, DateTime defaultStartDate, string attributeName = null)
		{
			var configuration = new XmlWriterConfiguration();

			var codeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, defaultStartDate);
			codeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			configuration.IncludeEntityTypeConfiguration(codeListConfig);

			if (!string.IsNullOrEmpty(attributeName))
			{
				codeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

				var attributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				attributeConfig.IncludeColumn(x => x.ZZE_Value, true);
				attributeConfig.IncludeColumnWithDefaultValue(x => x.ZZE_ZXE_NKName, true, attributeName);
				configuration.IncludeEntityTypeConfiguration(attributeConfig);
			}

			return configuration;
		}

		public static XmlWriterConfiguration BuildRefCusCodeListConfiguration(string codeType, string dataGrouping, string attributeName = null)
		{
			var configuration = new XmlWriterConfiguration();

			var codeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfig.IncludeColumn(x => x.ZZD_Description);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.RefData_Common.MinimumDateTime);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, dataGrouping);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			configuration.IncludeEntityTypeConfiguration(codeListConfig);

			if (!string.IsNullOrEmpty(attributeName))
			{
				codeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

				var attributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				attributeConfig.IncludeColumn(x => x.ZZE_Value, true);
				attributeConfig.IncludeColumnWithConstantValue(x => x.ZZE_ZXE_NKName, true, attributeName);
				configuration.IncludeEntityTypeConfiguration(attributeConfig);
			}

			return configuration;
		}


		public static XmlWriterConfiguration BuildRefCusCodeListExpirableConfiguration(string codeType, DateTime defaultStartDate, string attributeName = null)
		{
			var configuration = new XmlWriterConfiguration();

			var codeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, defaultStartDate);
			codeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping);
			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			configuration.IncludeEntityTypeConfiguration(codeListConfig);

			if (!string.IsNullOrEmpty(attributeName))
			{
				codeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

				var attributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true, enableExpirable: true);
				attributeConfig.IncludeColumn(x => x.ZZE_Value, true);
				attributeConfig.IncludeColumnWithDefaultValue(x => x.ZZE_ZXE_NKName, true, attributeName);
				attributeConfig.IncludeColumnWithConstantValue(x => x.ZZE_StartDate, false, defaultStartDate);
				attributeConfig.IncludeColumnWithDefaultValue(x => x.ZZE_EndDate, false, Constants.RefData_Common.MaximumDateTime);
				configuration.IncludeEntityTypeConfiguration(attributeConfig);
			}

			return configuration;
		}

		public static XmlWriterConfiguration BuildRefCusTariffAttributeConfiguration(string attributeName, string tariffType)
		{
			var configuration = new XmlWriterConfiguration();

			var cusTariffConfig = new EntityTypeConfiguration<RefCusTariff>(false);
			cusTariffConfig.IncludeColumn(x => x.RefCusTariffAttributes, false);
			cusTariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, tariffType);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGrouping);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGrouping);
			configuration.IncludeEntityTypeConfiguration(cusTariffConfig);

			var attributeConfig = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			attributeConfig.IncludeColumnWithConstantValue(x => x.ZZ3_Name, true, attributeName);
			attributeConfig.IncludeColumn(x => x.ZZ3_Value, true);
			configuration.IncludeEntityTypeConfiguration(attributeConfig);

			return configuration;
		}

		public static XmlWriterConfiguration BuildRefCusTariffUOMConfiguration(string dataGrouping = Constants.DataGrouping)
		{
			var configuration = new XmlWriterConfiguration();

			var cusTariffConfig = new EntityTypeConfiguration<RefCusTariff>(true);
			cusTariffConfig.IncludeColumn(x => x.RefCusTariffAttributes, false);
			cusTariffConfig.IncludeColumn(x => x.RefCusTariffUOMs, false);
			cusTariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			cusTariffConfig.IncludeColumn(x => x.ZZ1_Description, false);
			cusTariffConfig.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, true);
			cusTariffConfig.IncludeColumn(x => x.ZZ1_StartDate, false);
			cusTariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_IAMUnique, true, 0);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.IMP);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, dataGrouping);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, dataGrouping);
			cusTariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZF_NKTaxOrFeeCode, true, Constants.TaxOrFeeCodes.GST);
			configuration.IncludeEntityTypeConfiguration(cusTariffConfig);

			var attributeConfig = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			attributeConfig.IncludeColumn(x => x.ZZ3_Name, true);
			attributeConfig.IncludeColumn(x => x.ZZ3_Value, true);
			configuration.IncludeEntityTypeConfiguration(attributeConfig);

			var uomConfig = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uomConfig.IncludeColumn(x => x.ZZ8_UOM, true);
			uomConfig.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, dataGrouping);
			uomConfig.IncludeColumnWithDefaultValue(x => x.ZZ8_Type, true, Constants.TariffUOMTypes.CU1);
			configuration.IncludeEntityTypeConfiguration(uomConfig);

			return configuration;
		}

		public static XmlWriterConfiguration BuildRefCusTariffUOMTestConfiguration() => BuildRefCusTariffUOMConfiguration(Constants.DataGroupingTest);

		public static DateTime GetDefaultStartDate(bool isAutoExpiryOn, DateTime publishedDate, DateTime? frequentStartDate = null)
		{
			return isAutoExpiryOn ? publishedDate : (frequentStartDate ?? Constants.RefData_Common.MinimumDateTime);
		}

		public static XmlWriterConfiguration BuildRefCusTradeGroupsConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tradeGroupsConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupsConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.DataGrouping);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, isKeyColumn: true);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_Description, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_StartDate, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumnWithDefaultValue(x => x.ZZA_EndDate, isKeyColumn: false, Constants.RefData_Common.MaximumDateTime);
			tradeGroupsConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsConfiguration);

			var tradeGroupsCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, isKeyColumn: true);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_StartDate, isKeyColumn: false);
			tradeGroupsCountryConfiguration.IncludeColumnWithDefaultValue(x => x.ZZB_EndDate, isKeyColumn: false, Constants.RefData_Common.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsCountryConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration BuildCMRSeaImpendingArrivalsConfiguration(string dataGrouping = Constants.DataGrouping)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refVesselZZConfiguration = new EntityTypeConfiguration<RefVesselZZ>(true);
			refVesselZZConfiguration.IncludeColumn(x => x.ZZO_Code, isKeyColumn: true);
			refVesselZZConfiguration.IncludeColumn(x => x.ZZO_LloydsNumber, isKeyColumn: true);
			refVesselZZConfiguration.IncludeColumnWithConstantValue(x => x.ZZO_ZZZ_NKDataGrouping, isKeyColumn: true, dataGrouping);
			refVesselZZConfiguration.IncludeColumn(x => x.RefVesselArrivals);
			writerConfiguration.IncludeEntityTypeConfiguration(refVesselZZConfiguration);

			var refVesselArrivalConfiguration = new EntityTypeConfiguration<RefVesselArrival>(true);
			refVesselArrivalConfiguration.IncludeColumn(x => x.ZYA_VoyageNumber, isKeyColumn: true);
			refVesselArrivalConfiguration.IncludeColumn(x => x.ZYA_ArrivalDate, isKeyColumn: false);
			refVesselArrivalConfiguration.IncludeColumn(x => x.ZYA_ArrivalPort, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(refVesselArrivalConfiguration);

			return writerConfiguration;
		}
	}
}
