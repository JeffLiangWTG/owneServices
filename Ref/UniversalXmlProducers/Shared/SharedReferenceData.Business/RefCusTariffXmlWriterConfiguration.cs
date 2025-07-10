using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public class RefCusTariffXmlWriterConfiguration : XmlWriterConfiguration
	{
		public RefCusTariffXmlWriterConfiguration(bool includeDescription)
		{
			BuildTariffConfiguration(includeDescription);
			BuildRelationshipConfiguration();
			BuildTariffAttributeConfiguration();
			BuildRateConfiguration();
			BuildApplicabilityConfiguration();
			BuildCusExcludedTradeGroupConfiguration();
		}

		void BuildTariffConfiguration(bool includeDescription)
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZZ_NKDataGrouping, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_StartDate, true, Constants.DefaultValues.MinDateTime);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.DefaultValues.MaxDateTime);
			if (includeDescription)
			{
				tariffConfiguration.IncludeColumn(x => x.ZZ1_Description);
			}
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffRelationships);
			tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			IncludeEntityTypeConfiguration(tariffConfiguration);
		}

		void BuildRelationshipConfiguration()
		{
			var relationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			relationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);
			relationshipConfiguration.IncludeColumn(x => x.ZZH_ZZI_NKTariffType, true);
			relationshipConfiguration.IncludeColumn(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true);
			IncludeEntityTypeConfiguration(relationshipConfiguration);
		}

		void BuildTariffAttributeConfiguration()
		{
			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value);
			IncludeEntityTypeConfiguration(tariffAttributeConfiguration);
		}

		void BuildRateConfiguration()
		{
			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZZ_NKDataGrouping, true);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormula, false, 0);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_StartDate, true, Constants.DefaultValues.MinDateTime);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, Constants.DefaultValues.MaxDateTime);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities);
			IncludeEntityTypeConfiguration(rateConfiguration);
		}

		void BuildApplicabilityConfiguration()
		{
			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_StartDate, true, Constants.DefaultValues.MinDateTime);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.DefaultValues.MaxDateTime);
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups);
			IncludeEntityTypeConfiguration(applicabilityConfiguration);
		}

		void BuildCusExcludedTradeGroupConfiguration()
		{
			var excludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true);
			IncludeEntityTypeConfiguration(excludedTradeGroupConfiguration);
		}
	}
}
