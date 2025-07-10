using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public abstract class TariffXmlProducer : XmlProducer<RefCusTariff>
	{
		protected TariffXmlProducer(bool includeCompositeKey = true)
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration(includeCompositeKey));
		}
		public override abstract string FilePath { get; }
		public override abstract string DataSource { get; }
		protected abstract string TariffType { get; }
		protected abstract bool IsImport { get; }
		protected abstract bool IsExport { get; }

		public override void ExportToXmlInBatch(IEnumerable<RefCusTariff> collection, DateTime publishTime)
		{
			var groups = collection.GroupBy(x => x.ZZ1_TariffCode.Substring(0, 1));

			foreach (var group in groups)
			{
				var fileName = FilePath.Replace(".xml", $"_{group.Key}.xml");

				var dataSource = $"{DataSource} chapters {group.Key}0-{group.Key}9";

				InitializeWriter(publishTime, dataSource);
				ExportToXml(group, fileName);
			}
		}

		public IXmlWriterConfiguration GetWriterConfiguration(bool includeCompositeKey, bool allPropertiesIsData = true)
		{
			var propIsDataValue = allPropertiesIsData ? IsDataValue.True : IsDataValue.False;
			var uomConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_Type, true, propIsDataValue);
			uomConfiguration.IncludeColumn(x => x.ZZ8_UOM, false, propIsDataValue);
			uomConfiguration.IncludeColumn(x => x.ZZ8_ZZA_NKTradeGroup, true, propIsDataValue);
			uomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			uomConfiguration.IncludeColumn(x => x.ZZ8_ZZA_ZZZ_NKDataGrouping, true, propIsDataValue);

			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true, propIsDataValue);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false, propIsDataValue);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false, propIsDataValue);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00), propIsDataValue);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, TariffType, propIsDataValue);
			if (includeCompositeKey)
			{
				tariffConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false, propIsDataValue);
			}
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions, false, propIsDataValue);
			tariffConfiguration.IncludeColumn(x => x.RefCusRates, false, propIsDataValue);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs, false, propIsDataValue);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffLanguages, false, propIsDataValue);

			var tariffLanguageConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			tariffLanguageConfiguration.IncludeColumn(x => x.ZX7_Description, false, propIsDataValue);
			tariffLanguageConfiguration.IncludeColumn(x => x.ZX7_ZX6_NKLanguage, true, propIsDataValue);

			var rateUomConfiguration = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUomConfiguration.IncludeColumn(x => x.ZXG_UOM, true, propIsDataValue);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false, propIsDataValue);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00), propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false, propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true, propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true, propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true, propIsDataValue);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true, propIsDataValue);
			rateConfiguration.IncludeColumn(x => x.RefCusRateUOMs, false, propIsDataValue);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true, propIsDataValue);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, true, propIsDataValue);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false, propIsDataValue);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00), propIsDataValue);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true, propIsDataValue);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups, false, propIsDataValue);

			var excludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true, propIsDataValue);
			excludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);

			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true, propIsDataValue);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.ZX1_Comment, true, propIsDataValue);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false, propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false, propIsDataValue);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00), propIsDataValue);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, "EU Taric", propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZZS_ZZZ_NKDataGrouping, true, propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZZS_NKPreference, true, propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsImport, false, propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsExport, false, propIsDataValue);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true, propIsDataValue);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues, false, propIsDataValue);

			var conditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true, propIsDataValue);
			conditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, "EUN", propIsDataValue);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true, propIsDataValue);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(uomConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffLanguageConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateUomConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(excludedTradeGroupConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionValueConfiguration);

			return writerConfiguration;
		}
	}
}
