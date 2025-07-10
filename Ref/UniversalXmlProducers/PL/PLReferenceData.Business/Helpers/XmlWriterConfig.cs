using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Helpers
{
	public static class XmlWriterConfig
	{
		public static void ExportToXmlFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDate, IEnumerable<T> data)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDate);
			writer.SetUpdateType(UpdateType.Full);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var item in data)
			{
				writer.PopulateData(item);
			}

			writer.SaveXml(outputFile);
		}

		#region RefCusTariff

		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusTariffConfiguration());
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusRateConfiguration());
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusRateUOMConfiguration());
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusApplicabilityConfiguration());
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusVATApplicabilityConfiguration());
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusConditionConfiguration());
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusConditionValueConfiguration());

			return writerConfiguration;
		}

		static EntityTypeConfiguration<RefCusTariff> GetRefCusTariffConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusTariff>(false);

			configuration.IncludeColumn(x => x.RefCusRates, isKeyColumn: false);
			configuration.IncludeColumn(x => x.RefCusVATApplicabilities, isKeyColumn: false);
			configuration.IncludeColumn(x => x.RefCusConditions, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZ1_TariffCode, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, isKeyColumn: true, Constants.ShipmentType.Import);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, isKeyColumn: true, Tariff.Constants.EuropeanUnionCode);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, isKeyColumn: true, Tariff.Constants.EuropeanUnionCode);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusRate> GetRefCusRateConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusRate>(true);

			configuration.IncludeColumn(x => x.RefCusApplicabilities, isKeyColumn: true);
			configuration.IncludeColumn(x => x.RefCusRateUOMs, isKeyColumn: false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, isKeyColumn: false, Tariff.Constants.ConstantEndDate);
			configuration.IncludeColumn(x => x.ZZ2_RateFormula, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZ2_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, isKeyColumn: true);
			configuration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCode);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCode);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusRateUOM> GetRefCusRateUOMConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusRateUOM>(true);

			configuration.IncludeColumn(x => x.ZXG_UOM, isKeyColumn: true);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusApplicability> GetRefCusApplicabilityConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusApplicability>(true);

			configuration.IncludeColumn(x => x.ZZT_AdditionalCode, isKeyColumn: true);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, isKeyColumn: false, Tariff.Constants.ConstantEndDate);
			configuration.IncludeColumn(x => x.ZZT_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, isKeyColumn: true, Tariff.Constants.EuropeanUnionCode);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusVATApplicability> GetRefCusVATApplicabilityConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusVATApplicability>(true);

			configuration.IncludeColumn(x => x.ZX5_AdditionalCode, isKeyColumn: true);

			configuration.IncludeColumnWithDefaultValue(x => x.ZX5_EndDate, isKeyColumn: false, Tariff.Constants.ConstantEndDate);
			configuration.IncludeColumn(x => x.ZX5_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCode);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCondition> GetRefCusConditionConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusCondition>(true);

			configuration.IncludeColumn(x => x.RefCusApplicabilities, isKeyColumn: true);
			configuration.IncludeColumn(x => x.RefCusConditionValues, isKeyColumn: true);
			configuration.IncludeColumn(x => x.ZX1_Comment, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZX1_ConditionValueTrueMeansStop, isKeyColumn: false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, isKeyColumn: false, Tariff.Constants.ConstantEndDate);
			configuration.IncludeColumn(x => x.ZX1_IsExport, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZX1_IsImport, isKeyColumn: false);
			configuration.IncludeColumnWithConstantValue(x => x.ZX1_Source, isKeyColumn: false, Tariff.Constants.ConstantEndDate);
			configuration.IncludeColumn(x => x.ZX1_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCode);
			configuration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCode);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusConditionValue> GetRefCusConditionValueConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusConditionValue>(true);

			configuration.IncludeColumnWithConstantValue(x => x.ZX3_LogicalORWithinGroup, isKeyColumn: false, "0");
			configuration.IncludeColumn(x => x.ZX3_Value, isKeyColumn: true);
			configuration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.EuropeanUnionCode);

			return configuration;
		}

		#endregion

		#region RefCusQuota

		public static XmlWriterConfiguration GetRefCusQuotaWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusQuotaConfiguration());

			return writerConfiguration;
		}

		static EntityTypeConfiguration<RefCusQuota> GetRefCusQuotaConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusQuota>(true);

			configuration.IncludeColumn(x => x.ZXQ_OrderNumber, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZXQ_ZZZ_NKDataGrouping, true, Constants.EuropeanUnionCode);
			configuration.IncludeColumn(x => x.ZXQ_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZXQ_EndDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZXQ_InitialAmount, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZXQ_UnitOfMeasure, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZXQ_Balance, isKeyColumn: false);

			return configuration;
		}

		#endregion

		#region RefCusCodeList

		public static XmlWriterConfiguration GetRefCusCodeListWithAttributeWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListWithAttributeConfiguration());
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListAttributeConfiguration());

			return xmlWriterConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWithLanguageWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListWithLanguageConfiguration());
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListLanguageConfiguration());

			return xmlWriterConfiguration;
		}

		public static XmlWriterConfiguration GetSimpleRefCusCodeListWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetSimpleRefCusCodeListConfiguration());

			return xmlWriterConfiguration;
		}

		static EntityTypeConfiguration<RefCusCodeList> GetSimpleRefCusCodeListConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeList>(true);

			configuration.IncludeColumn(x => x.ZZD_Code, isKeyColumn: true);
			configuration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCode);
			configuration.IncludeColumn(x => x.ZZD_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZD_Description, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZD_EndDate, isKeyColumn: false);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCodeList> GetRefCusCodeListWithAttributeConfiguration()
		{
			var configuration = GetSimpleRefCusCodeListConfiguration();
			configuration.IncludeColumn(x => x.RefCusCodeListAttributes, isKeyColumn: false);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCodeList> GetRefCusCodeListWithLanguageConfiguration()
		{
			var configuration = GetSimpleRefCusCodeListConfiguration();
			configuration.IncludeColumn(x => x.RefCusCodeListLanguages, isKeyColumn: false);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCodeListAttribute> GetRefCusCodeListAttributeConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);

			configuration.IncludeColumn(x => x.ZZE_Value, isKeyColumn: true);
			configuration.IncludeColumn(x => x.ZZE_ZXE_NKName, isKeyColumn: true);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCodeListLanguage> GetRefCusCodeListLanguageConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);

			configuration.IncludeColumnWithConstantValue(x => x.ZXA_ZX6_NKLanguage, isKeyColumn: true, Constants.CountryCode);
			configuration.IncludeColumn(x => x.ZXA_Description, isKeyColumn: false);

			return configuration;
		}

		#endregion

		#region RefExchangeRate

		public static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(string currencyType)
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefExchangeRateZZConfiguration(currencyType));

			return xmlWriterConfiguration;
		}

		static EntityTypeConfiguration<RefExchangeRateZZ> GetRefExchangeRateZZConfiguration(string currencyType)
		{
			var configuration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);

			configuration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, isKeyColumn: true, currencyType);
			configuration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, isKeyColumn: true, Constants.CountryCode);
			configuration.IncludeColumn(x => x.ZZN_StartDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZN_EndDate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZN_Rate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZN_AsPublished, isKeyColumn: false);

			return configuration;
		}

		#endregion

		#region RefCusProcedure

		public static XmlWriterConfiguration GetRefCusProcedureWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusProcedureConfiguration());

			return writerConfiguration;
		}

		static EntityTypeConfiguration<RefCusProcedure> GetRefCusProcedureConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusProcedure>(true);

			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, isKeyColumn: true, constantValue: Constants.CountryCode);
			configuration.IncludeColumn(x => x.ZZ6_ProcedureCode, isKeyColumn: true);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_PreviousProcedureCode, isKeyColumn: true, defaultValue: string.Empty);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_Concession, isKeyColumn: true, defaultValue: string.Empty);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_StartDate, isKeyColumn: false, defaultValue: DictionariesConstants.DefaultStartDateDateTime);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_EndDate, isKeyColumn: false, defaultValue: DictionariesConstants.DefaultEndDateDateTime);
			configuration.IncludeColumn(x => x.ZZ6_Description, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZ6_ShipmentType, isKeyColumn: false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_CalculateDuty, isKeyColumn: false, defaultValue: false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_LandedCost, isKeyColumn: false, defaultValue: false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_CalculateVAT, isKeyColumn: false, defaultValue: false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoWarehouse, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfWarehouse, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoTemporaryImport, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfTemporaryImport, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoTemporaryExport, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfTemporaryExport, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoInwardProcessing, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfInwardProcessing, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoOutwardProcessing, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutofOutwardProcessing, isKeyColumn: false, defaultValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_Category, isKeyColumn: false, constantValue: string.Empty);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_Group, isKeyColumn: false, constantValue: string.Empty);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IsTransit, isKeyColumn: false, constantValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IsGuaranteeConsumed, isKeyColumn: false, constantValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IsGuaranteeReleased, isKeyColumn: false, constantValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IntoVATWarehouse, isKeyColumn: false, constantValue: CusProcedure.Constants.NotApplicableXmlValue);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ6_OutOfVATWarehouse, isKeyColumn: false, constantValue: CusProcedure.Constants.NotApplicableXmlValue);

			return configuration;
		}

		#endregion
	}
}
