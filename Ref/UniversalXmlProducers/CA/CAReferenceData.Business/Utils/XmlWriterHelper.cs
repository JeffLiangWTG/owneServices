using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	class XMLWriterHelper
	{
		public static XmlWriterConfiguration GetRefCusCodelistConfiguration(string codeType, bool isKeyColumnForAttribueValue = false)
		{
			var codelistConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codelistConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinDateTime);
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaxDateTime);
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codelistConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, isKeyColumnForAttribueValue);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(codelistConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusRateTypeConfiguration()
		{
			var rateType = new EntityTypeConfiguration<RefCusRateType>(true);
			rateType.IncludeColumn(x => x.ZZR_RateType, true);
			rateType.IncludeColumn(x => x.ZZR_Description, false);
			rateType.IncludeColumnWithConstantValue(x => x.ZZR_IsPayable, false, true);
			rateType.IncludeColumnWithConstantValue(x => x.ZZR_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			rateType.IncludeColumn(x => x.RefCusRateCodes, false);

			var rateCode = new EntityTypeConfiguration<RefCusRateCode>(true);
			rateCode.IncludeColumn(x => x.ZY1_RateCode, true);
			rateCode.IncludeColumn(x => x.ZY1_Description, false);
			rateCode.IncludeColumnWithConstantValue(x => x.ZY1_ZZZ_NKDataGrouping, false, Constants.DefaultValues.CountryCodeCanada);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(rateType);
			writerConfiguration.IncludeEntityTypeConfiguration(rateCode);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTariffConfiguration()
		{
			var refCusTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);

			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.CanadaHarmonizedTariff);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			refCusTariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffLanguages);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusRates);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusConditions);

			var refCusTariffLanguageConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			refCusTariffLanguageConfiguration.IncludeColumn(x => x.ZX7_Description, false);
			refCusTariffLanguageConfiguration.IncludeColumnWithDefaultValue(x => x.ZX7_ZX6_NKLanguage, true, Constants.DefaultValues.FRLanguage);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, Constants.DefaultValues.MaxDateTime);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.DefaultValues.MaxDateTime);

			var refCusTariffUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			refCusTariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_Type, false, Constants.UOMTypeCU1);
			refCusTariffUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			refCusTariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);

			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value, true);

			var refCusConditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_ZX2_NKConditionType, true, Constants.ConditionType.PGA);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_StartDate, false, Constants.DefaultValues.MinDateTime);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, false, Constants.DefaultValues.CountryCodeCanada);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusConditionConfiguration.IncludeColumn(x => x.RefCusConditionValues);

			var refCusConditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffLanguageConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffUOMConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionValueConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetHarmonizedRefCusTariffConfiguration()
		{
			var refCusTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);

			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			refCusTariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.CanadaHarmonizedTariff);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusRates);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusConditions);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, Constants.DefaultValues.MaxDateTime);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, Constants.RateTypeDty);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_NKRateCode, true, Constants.RateTypeDty);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.DefaultValues.MaxDateTime);

			var refCusTariffUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			refCusTariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_Type, false, Constants.UOMTypeCU1);
			refCusTariffUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			refCusTariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);

			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value, true);

			var refCusConditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_NKConditionType, true, Constants.ConditionType.PGA);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_StartDate, false, Constants.DefaultValues.MinDateTime);
			refCusConditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, false, Constants.DefaultValues.CountryCodeCanada);
			refCusConditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusConditionConfiguration.IncludeColumn(x => x.RefCusConditionValues);

			var refCusConditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffUOMConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionValueConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(DateTime startDate, DateTime endDate)
		{
			var exchangeRate = new EntityTypeConfiguration<RefExchangeRateZZ>(true);

			exchangeRate.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, Constants.ExchangeRate.DefaultRateType);
			exchangeRate.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DefaultValues.CountryCodeCanada);

			exchangeRate.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, true, startDate.Date, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRate.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, endDate);

			exchangeRate.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRate.IncludeColumn(x => x.ZZN_Rate, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRate);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusPreferenceWriterConfiguration()
		{
			var preference = new EntityTypeConfiguration<RefCusPreference>(true);

			preference.IncludeColumnWithConstantValue(x => x.ZZS_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			preference.IncludeColumn(x => x.ZZS_Preference, true);
			preference.IncludeColumn(x => x.ZZS_Description, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(preference);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTradeGroupWriterConfiguration()
		{
			var tradeGroup = new EntityTypeConfiguration<RefCusTradeGroup>(true);

			tradeGroup.IncludeColumnWithDefaultValue(x => x.ZZA_StartDate, false, Constants.DefaultValues.MinDateTime);
			tradeGroup.IncludeColumnWithDefaultValue(x => x.ZZA_EndDate, false, Constants.DefaultValues.MaxDateTime);

			tradeGroup.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);

			tradeGroup.IncludeColumn(x => x.ZZA_TradeGroup, true);
			tradeGroup.IncludeColumn(x => x.ZZA_Description);
			tradeGroup.IncludeColumn(x => x.RefCusTradeGroupCountries);

			var tradeGroupCountry = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);

			tradeGroupCountry.IncludeColumnWithDefaultValue(x => x.ZZB_StartDate, false, Constants.DefaultValues.MinDateTime);
			tradeGroupCountry.IncludeColumnWithDefaultValue(x => x.ZZB_EndDate, false, Constants.DefaultValues.MaxDateTime);

			tradeGroupCountry.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_Description);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroup);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountry);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetWriterConfigurationForCASIMADataAndCASurtax()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffCfg = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffCfg.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffCfg.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffCfg.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "CA");
			tariffCfg.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "CA");
			tariffCfg.IncludeColumn(x => x.ZZ1_Description);
			tariffCfg.IncludeColumn(x => x.ZZ1_StartDate);
			tariffCfg.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			tariffCfg.IncludeColumn(x => x.RefCusRates);
			tariffCfg.IncludeColumn(x => x.RefCusTariffRelationships);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffCfg);

			var relationshipCfg = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			relationshipCfg.IncludeColumn(x => x.ZZH_TariffCode, true);
			relationshipCfg.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, "HSN");
			relationshipCfg.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, "CA");
			writerConfiguration.IncludeEntityTypeConfiguration(relationshipCfg);

			var rateCfg = new EntityTypeConfiguration<RefCusRate>(true);
			rateCfg.IncludeColumn(x => x.ZZ2_StartDate, isKeyColumn: true, justification: "RefCusRate could have overlapped boundary dates, so it's eligible to have ZZ2_StartDate as key property");
			rateCfg.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			rateCfg.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateCfg.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, "CA");
			rateCfg.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, "CA");
			rateCfg.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, "ADD");
			rateCfg.IncludeColumn(x => x.ZZ2_RateFormula);
			rateCfg.IncludeColumn(x => x.ZZ2_RX_NKCurrencyOverride, isKeyColumn: true);
			rateCfg.IncludeColumn(x => x.RefCusApplicabilities, true);

			writerConfiguration.IncludeEntityTypeConfiguration(rateCfg);

			var appCfg = new EntityTypeConfiguration<RefCusApplicability>(true);
			appCfg.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			appCfg.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, "CA");
			appCfg.IncludeColumn(x => x.ZZT_StartDate);
			appCfg.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			writerConfiguration.IncludeEntityTypeConfiguration(appCfg);

			return writerConfiguration;
		}
	}
}
