using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class TARICTariffUniversalReferenceDataFileGenerator : TariffUniversalReferenceDataFileGenerator
	{
		public TARICTariffUniversalReferenceDataFileGenerator()
		{
		}

		protected override XmlWriterConfiguration GetXmlWriterConfigurationCore()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, UniversalDataHelper.Constants.Import);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.EuropeanUnion);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.EuropeanUnion);
			tariffConfiguration.IncludeColumn(x => x.RefCusVATApplicabilities);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions);
			tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAdditionalCodes);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);

			var vatApplicabilityConfiguration = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_StartDate, false);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_EndDate, false);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_AdditionalCode, true);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_Description, false);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_VATCategory, false);
			vatApplicabilityConfiguration.IncludeColumn(x => x.ZX5_ZZA_NKTradeGroup, true);
			vatApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			vatApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZA_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			writerConfiguration.IncludeEntityTypeConfiguration(vatApplicabilityConfiguration);

			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_EndDate, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsExport, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsImport, false);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, UniversalDataHelper.Constants.RITA);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_LogicalANDWithinGroup, false, 0);
			conditionConfiguration.IncludeColumn(x => x.ZX1_Comment, false);
			conditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues, false);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionConfiguration);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate);
			rateConfiguration.IncludeColumn(x => x.ZZ2_EndDate);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
			rateConfiguration.IncludeColumn(x => x.RefCusRateUOMs);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);

			var rateUOMConfiguration = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUOMConfiguration.IncludeColumn(x => x.ZXG_UOM, true);
			writerConfiguration.IncludeEntityTypeConfiguration(rateUOMConfiguration);

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
			importTariffMeasuresList = ritaDataParser.GetMeasuresAndConditions(tariffCode, UniversalDataHelper.Constants.ImportFile).Where(m => m.IsImportSupported).ToList();
			SubstituteEmptyTaxCode(importTariffMeasuresList);

			exportTariffMeasuresList = ritaDataParser.GetMeasuresAndConditions(tariffCode, UniversalDataHelper.Constants.ExportFile).Where(m => m.IsExportSupported).ToList();
			SubstituteEmptyTaxCode(exportTariffMeasuresList);

			if (!string.IsNullOrEmpty(tariffCode))
			{
				var vatApplicabilities = GetVATApplicabilities();
				var conditions = GetImportConditions().Concat(GetExportConditions()).ToArray();
				var rates = new RefCusRate[] { };
				try
				{
					rates = GetRates(updateType);
				}
				catch (MissingInfoException)
				{
					if (updateType == UpdateType.Partial)
					{
						throw;
					}
				}
				catch (FormatException)
				{
					if (updateType == UpdateType.Partial)
					{
						throw;
					}
				}

				var additionalCodes = GetAdditionalCodes();
				var uoms = GetUOMs(tariffCode);
				if (vatApplicabilities.Length != 0 || conditions.Length != 0 || rates.Length != 0 || additionalCodes.Length != 0 || uoms.Length != 0)
				{
					return new RefCusTariff()
					{
						ZZ1_TariffCode = tariffCode,
						RefCusVATApplicabilities = vatApplicabilities,
						RefCusConditions = conditions,
						RefCusRates = rates,
						RefCusTariffAdditionalCodes = additionalCodes,
						RefCusTariffUOMs = uoms
					};
				}
			}
			return null;
		}

		protected override RefCusTariffUOM[] GetUOMs(string tariffCode)
		{
			var result = new List<RefCusTariffUOM>();

			foreach (var measure in importTariffMeasuresList.Where(m => m.IsImportUOM).Concat(exportTariffMeasuresList.Where(m => m.IsExportUOM)))
			{
				var tariff = measure.GetUOM();
				if (tariff != null)
				{
					result.Add(tariff);
				}
			}

			return result.DistinctBy(x => x.ZZ8_UOM).GroupBy(x => x.ZZ8_ZZA_NKTradeGroup).SelectMany(g => g).ToArray();
		}

		RefCusVATApplicability[] GetVATApplicabilities()
		{
			var applicabilities = new List<RefCusVATApplicability>();

			foreach (var measure in importTariffMeasuresList.Where(m => m.IsVAT && !string.IsNullOrEmpty(m.ApplicationTerritory)))
			{
				applicabilities.AddRange(measure.GetVatApplicabilities());
			}

			return applicabilities.ToArray();
		}

		protected override RefCusRate[] GetRates(UpdateType updateType)
		{
			var rates = new List<RefCusRate>();

			foreach (var measure in importTariffMeasuresList.Where(m => m.IsDevelopment || m.IsExcise || m.IsPrecalculated || m.IsRedevance || m.IsGrantingOfSea))
			{
				if (string.IsNullOrEmpty(measure.TradeGroup) || string.IsNullOrEmpty(measure.TaxCode))
				{
					continue;
				}

				rates.AddRange(measure.GetRate());
			}

			foreach (var measure in exportTariffMeasuresList)
			{
				if (string.IsNullOrEmpty(measure.TaxCode))
				{
					continue;
				}

				rates.AddRange(measure.GetRate());
			}

			return rates.ToArray();
		}

		protected override RefCusTariffAdditionalCode[] GetAdditionalCodes()
		{
			var result = new List<RefCusTariffAdditionalCode>();

			foreach (var measure in importTariffMeasuresList.Concat(exportTariffMeasuresList).DistinctBy(x => x.MeasureType + x.SupplementaryCode).Where(m => m.IsStatistical))
			{
				result.AddRange(measure.GetStatisticalAdditionalCodes());
			}

			return result.ToArray();
		}

		protected override string DataSource => "FR Tariff";

		protected override string FilePrefix => "FRTariffData";
	}
}
