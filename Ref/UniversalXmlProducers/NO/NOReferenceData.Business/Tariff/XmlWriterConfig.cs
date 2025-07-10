using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff
{
	sealed class XmlWriterConfig
	{
		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, isKeyColumn: true);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, isKeyColumn: true, "HSN");
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusRates, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_Description, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_EndDate, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusConditions, isKeyColumn: false);
			refCusTariffConfiguration.IncludeColumn(x => x.RefCusVATApplicabilities, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffConfiguration);

			var refCusRateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			refCusRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, isKeyColumn: true);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, isKeyColumn: false);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, isKeyColumn: true);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, isKeyColumn: true);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, isKeyColumn: true);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, isKeyColumn: true);
			refCusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, isKeyColumn: false);
			refCusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, isKeyColumn: false);
			refCusRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormulaDerivedFrom, isKeyColumn: false, string.Empty);
			refCusRateConfiguration.IncludeColumn(x => x.RefCusRateUOMs, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateConfiguration);

			var refCusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			refCusApplicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_ZZA_NKTradeGroup, true, Constants.TradeGroupCodes.AllCountries);
			refCusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, isKeyColumn: false);
			refCusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, isKeyColumn:false);
			refCusApplicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_AdditionalCode, isKeyColumn: true, string.Empty);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusApplicabilityConfiguration);

			var refCusTariffUomConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			refCusTariffUomConfiguration.IncludeColumn(x => x.ZZ8_Type, isKeyColumn: true);
			refCusTariffUomConfiguration.IncludeColumn(x => x.ZZ8_UOM, isKeyColumn: false);
			refCusTariffUomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, isKeyColumn: false, Constants.CountryCodes.Norway);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffUomConfiguration);

			var refCusRateUOMsConfiguration = new EntityTypeConfiguration<RefCusRateUOM>(true);
			refCusRateUOMsConfiguration.IncludeColumn(x => x.ZXG_UOM, isKeyColumn: true);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateUOMsConfiguration);

			var refCusConditionsConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			refCusConditionsConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusConditionsConfiguration.IncludeColumn(x => x.ZX1_IsImport, isKeyColumn: false);
			refCusConditionsConfiguration.IncludeColumn(x => x.ZX1_StartDate, isKeyColumn: false);
			refCusConditionsConfiguration.IncludeColumn(x => x.ZX1_EndDate, isKeyColumn: false);
			refCusConditionsConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, isKeyColumn: true);
			refCusConditionsConfiguration.IncludeColumn(x => x.RefCusConditionValues, isKeyColumn: false);
			refCusConditionsConfiguration.IncludeColumn(x => x.RefCusApplicabilities, isKeyColumn: true);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionsConfiguration);

			var refCusConditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			refCusConditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusConditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_NKValueType, isKeyColumn: true, "FRM");
			refCusConditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, isKeyColumn: true);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusConditionValueConfiguration);

			var refCusVATApplicabilityConfiguration = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			refCusVATApplicabilityConfiguration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, isKeyColumn: true);
			refCusVATApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			refCusVATApplicabilityConfiguration.IncludeColumn(x => x.ZX5_Description, isKeyColumn: false);
			refCusVATApplicabilityConfiguration.IncludeColumn(x => x.ZX5_EndDate, isKeyColumn: false);
			refCusVATApplicabilityConfiguration.IncludeColumn(x => x.ZX5_StartDate, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusVATApplicabilityConfiguration);

			return writerConfiguration;
		}
	}
}
