using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using XmlWriter = CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriter;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CNRefCusTariffUniversalXMLWriter
	{
		readonly XmlWriter writer;
		readonly CNRefCusTariffChecker checker;

		bool IsPartial { get; }

		public CNRefCusTariffUniversalXMLWriter(DateTime publicationTime, bool isPartial = false)
		{
			IsPartial = isPartial;
			checker = new CNRefCusTariffChecker();
			writer = new XmlWriter(GetWriterConfiguration());
			writer.SetDataSource($"CN{(IsPartial ? " Partial" : "")} Tariff");
			writer.SetPublicationTime(publicationTime);
			writer.SetUpdateType(IsPartial ? UpdateType.Partial : UpdateType.Full);
		}

		public void Write(IEnumerable<RefCusTariff> refCusTariffs)
		{
			Write(refCusTariffs, Constants.FileNames.Xml_CN_RefCusTariff);
		}

		public void Write(IEnumerable<RefCusTariff> refCusTariffs, string fileName)
		{
			var hsnTariffCount = refCusTariffs.Where(x => x.ZZ1_ZZI_NKTariffType == "HSN").Count();
			var ciqTariffCount = refCusTariffs.Where(x => x.ZZ1_ZZI_NKTariffType == "CIQ").Count();

			GlobalOption.Instance.Log.Info($"<======================> {hsnTariffCount} HSN, {ciqTariffCount} CIQ Tariffs in total <======================>");

			var outputFile = GlobalOption.Instance.Setting.GetFullOutputFileName(fileName);
			var concurrentBag = new ConcurrentBag<RefCusTariff>(refCusTariffs);

			GlobalOption.Instance.Log.Info($"Start to write '{outputFile}'");
			foreach (var refCusTariff in concurrentBag.OrderBy(x => x.ZZ1_TariffCode).ThenByDescending(x => x.ZZ1_ZZI_NKTariffType))
			{
				if (checker.CheckDuplicated(refCusTariff))
				{
					writer.PopulateData(refCusTariff);
				}
			}
			writer.SaveXml(outputFile);

			GlobalOption.Instance.OutputFiles.Add(outputFile);
			GlobalOption.Instance.Log.Info($"Finish writing '{outputFile}'");
		}

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "CN");
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "CN");
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZF_NKTaxOrFeeCode);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffRelationships);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions);
			tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffLanguages);

			var relationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			relationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, "HSN");
			relationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, "CN");
			relationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);

			var uomConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			uomConfiguration.IncludeColumn(x => x.ZZ8_UOM);
			uomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, "CN");

			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, "CN");
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, "CN");
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			conditionConfiguration.IncludeColumn(x => x.ZX1_Comment, true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsImport);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsExport);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ConditionValueTrueMeansStop);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues);

			var conditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_LogicalORWithinGroup, false, 0);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			conditionValueConfiguration.IncludeColumnWithDefaultValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, "CN");

			var tariffValueConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffValueConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffValueConfiguration.IncludeColumn(x => x.ZZ3_Value, false);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, "CN");
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, "CN");
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, "CN");
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);

			var excludedConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, "CN");

			var languageConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			languageConfiguration.IncludeColumn(x => x.ZX7_Description, false);
			languageConfiguration.IncludeColumnWithConstantValue(x => x.ZX7_ZX6_NKLanguage, true, "EN");

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(relationshipConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(uomConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionValueConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffValueConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(excludedConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(languageConfiguration);

			return writerConfiguration;
		}
	}
}
