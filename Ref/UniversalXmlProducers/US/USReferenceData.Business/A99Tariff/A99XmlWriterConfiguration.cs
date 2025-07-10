using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class A99XmlWriterConfiguration : XmlWriterConfiguration
	{
		static DateTime MaxEndDate = new DateTime(2079, 06, 06, 23, 59, 00);

		public A99XmlWriterConfiguration()
		{
			BuildTariffConfiguration();
			BuildRelationshipConfiguration();
			BuildTariffAttributeConfiguration();
			BuildRateConfiguration();
			BuildConditionConfiguration();
			BuildApplicabilityConfiguration();
			BuildCusExcludedTradeGroupConfiguration();
			BuildConditionValueConfiguration();
		}

		void BuildCusExcludedTradeGroupConfiguration()
		{
			var excludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			IncludeEntityTypeConfiguration(excludedTradeGroupConfiguration);
		}

		void BuildApplicabilityConfiguration()
		{
			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, MaxEndDate);
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups);
			IncludeEntityTypeConfiguration(applicabilityConfiguration);
		}

		void BuildRateConfiguration()
		{
			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_ZY1_NKRateCode, true, Constants.RateTypes.DTY);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, Constants.RateTypes.DTY);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormula, false, 0);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, MaxEndDate);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities);
			IncludeEntityTypeConfiguration(rateConfiguration);
		}

		void BuildConditionConfiguration()
		{
			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate);
			conditionConfiguration.IncludeColumn(x => x.ZX1_Severity);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, MaxEndDate);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_IsImport, false, false);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_IsExport, false, false);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_Comment, false, string.Empty);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_Source, false, string.Empty);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			conditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues);
			IncludeEntityTypeConfiguration(conditionConfiguration);
		}

		void BuildConditionValueConfiguration()
		{
			var conditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);
			IncludeEntityTypeConfiguration(conditionValueConfiguration);
		}

		void BuildTariffAttributeConfiguration()
		{
			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value);
			IncludeEntityTypeConfiguration(tariffAttributeConfiguration);
		}

		void BuildRelationshipConfiguration()
		{
			var relationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			relationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);
			relationshipConfiguration.IncludeColumnWithDefaultValue(x => x.ZZH_ZZI_NKTariffType, true, Constants.TariffTypes.HSN);
			relationshipConfiguration.IncludeColumnWithDefaultValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			IncludeEntityTypeConfiguration(relationshipConfiguration);
		}

		void BuildTariffConfiguration()
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.HSN);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, MaxEndDate);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffRelationships);
			tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions);
			IncludeEntityTypeConfiguration(tariffConfiguration);
		}
	}
}
