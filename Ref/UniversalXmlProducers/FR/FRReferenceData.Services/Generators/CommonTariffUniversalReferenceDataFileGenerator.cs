using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class CommonTariffUniversalReferenceDataFileGenerator : TariffUniversalReferenceDataFileGenerator
	{
		public CommonTariffUniversalReferenceDataFileGenerator()
		{
		}

		protected override XmlWriterConfiguration GetXmlWriterConfigurationCore()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, UniversalDataHelper.Constants.Export);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.EuropeanUnion);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.EuropeanUnion);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAdditionalCodes);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);

			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_EndDate, false);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsExport, false, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsImport, false, false);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, UniversalDataHelper.Constants.RITA);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_LogicalANDWithinGroup, false, 0);
			conditionConfiguration.IncludeColumn(x => x.ZX1_Comment, false);
			conditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues, false);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionConfiguration);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups, false);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKSecondTradeGroup, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_ZZZ_NKSecondDataGrouping, true);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);

			var excludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			writerConfiguration.IncludeEntityTypeConfiguration(excludedTradeGroupConfiguration);

			var conditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			conditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			conditionValueConfiguration.IncludeColumnWithDefaultValue(x => x.ZX3_LogicalORWithinGroup, false, 0);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionValueConfiguration);

			var additionalCodeConfiguration = new EntityTypeConfiguration<RefCusTariffAdditionalCode>(true);
			additionalCodeConfiguration.IncludeColumn(x => x.ZY2_AdditionalCode, true);
			additionalCodeConfiguration.IncludeColumn(x => x.ZY2_Description, false);
			additionalCodeConfiguration.IncludeColumn(x => x.ZY2_ZY3_NKCategory, true);
			additionalCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZY2_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			additionalCodeConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			writerConfiguration.IncludeEntityTypeConfiguration(additionalCodeConfiguration);

			var uomConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true, enableExpirable: true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_UOM, true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_ZZA_NKTradeGroup, true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_ZZA_NKSecondTradeGroup, true);
			uomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZA_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			uomConfiguration.IncludeColumn(x => x.ZZ8_ZZA_ZZZ_NKSecondDataGrouping, true);
			uomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			uomConfiguration.IncludeColumn(x => x.ZZ8_StartDate, false);
			uomConfiguration.IncludeColumn(x => x.ZZ8_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(uomConfiguration);

			return writerConfiguration;
		}

		protected override RefCusTariff ProcessMeasuresForTariff(string tariffCode)
		{
			var ritaDataParser = new RITADataParser();
			exportTariffMeasuresList = ritaDataParser.GetMeasuresAndConditions(tariffCode, UniversalDataHelper.Constants.ExportFile).Where(m => m.IsExportSupported).ToList();
			SubstituteEmptyTaxCode(exportTariffMeasuresList);
			var conditions = GetExportConditions();
			var additionalCodes = GetAdditionalCodes();
			var uoms = GetUOMs(tariffCode);
			if (!string.IsNullOrEmpty(tariffCode) && conditions.Length > 0)
			{
				return new RefCusTariff()
				{
					ZZ1_TariffCode = tariffCode.Substring(0, 8),
					RefCusConditions = conditions,
					RefCusTariffAdditionalCodes = additionalCodes,
					RefCusTariffUOMs = uoms
				};
			}
			return null;
		}

		protected override RefCusTariffUOM[] GetUOMs(string tariffCode)
		{
			var result = new List<RefCusTariffUOM>();

			foreach (var measure in exportTariffMeasuresList.Where(m => m.IsExportUOM))
			{
				var tariff = measure.GetUOM();
				if (tariff != null)
				{
					result.Add(tariff);
				}
			}
			return result.DistinctBy(x => x.ZZ8_UOM).GroupBy(x => x.ZZ8_ZZA_NKTradeGroup).SelectMany(g => g).ToArray();
		}

		protected override RefCusTariffAdditionalCode[] GetAdditionalCodes()
		{
			var result = new List<RefCusTariffAdditionalCode>();

			foreach (var measure in exportTariffMeasuresList.Where(m => m.IsExportStatistical))
			{
				result.AddRange(measure.GetStatisticalAdditionalCodes());
			}

			return result.ToArray();
		}

		protected override string[] GetFilteredTariffList(string[] tariffList) => tariffList.GroupBy(t => t.Substring(0, 8)).Select(x => x.First()).ToArray();

		protected override string DataSource => "FR Tariff (8 digits Export)";

		protected override string FilePrefix => "FR_8DigitsExportTariffData";
	}
}

