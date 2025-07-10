using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public class ImportTariffWriter
	{
		public ImportTariffWriter(Func<IXmlWriter> writerFunc)
		{
			this.writerFunc = writerFunc;
		}
		readonly Func<IXmlWriter> writerFunc;

		public ImportTariffWriter() : this(() => new XmlWriter(GetConfiguration()))
		{ }

		public void Write(IEnumerable<RefCusTariff> tariffs, DateTime publicationTime, string output)
		{
			foreach (var tariffsPerChapter in tariffs.GroupBy(x => x.ZZ1_TariffCode.Substring(0, 1)))
			{
				var writer = writerFunc();
				writer.SetPublicationTime(publicationTime);
				writer.SetUpdateType(UpdateType.Partial);
				writer.SetDataSource($"Import Daily EUN Tariffs chapters {tariffsPerChapter.Key}0-{tariffsPerChapter.Key}9");
				foreach (var tariff in tariffsPerChapter.OrderBy(x => x.ZZ1_TariffCode))
				{
					writer.PopulateData(tariff);
				}
				writer.SaveXml(output.Replace(".xml", $"_{publicationTime:yyyyMMdd}_{tariffsPerChapter.Key}.xml"));
			}
		}

		static XmlWriterConfiguration GetConfiguration()
		{
			var result = new XmlWriterConfiguration();
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "IMP");
			tariffConfiguration.IncludeColumn(x => x.RefCusRates, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs, false);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, "EUN");
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, "EUN");
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			rateConfiguration.IncludeColumn(x => x.RefCusRateUOMs, false);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			applicabilityConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, "EUN");
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups, false);

			var excludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, "EUN");

			var rateUomConfiguration = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUomConfiguration.IncludeColumn(x => x.ZXG_UOM, true);

			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, "EUN");
			conditionConfiguration.IncludeColumn(x => x.ZX1_Comment, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_Source, false, "EU Taric");
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZZS_ZZZ_NKDataGrouping, true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZZS_NKPreference, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsImport, false, true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsExport, false, false);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, "EUN");
			conditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues, false);

			var conditionValueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			conditionValueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, "EUN");
			conditionValueConfiguration.IncludeColumn(x => x.ZX3_Value, true);

			var tariffUomConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			tariffUomConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			tariffUomConfiguration.IncludeColumn(x => x.ZZ8_UOM, true);
			tariffUomConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, "EUN");
			tariffUomConfiguration.IncludeColumn(x => x.ZZ8_ZZA_NKTradeGroup, true);

			result.IncludeEntityTypeConfiguration(tariffConfiguration);
			result.IncludeEntityTypeConfiguration(rateConfiguration);
			result.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			result.IncludeEntityTypeConfiguration(excludedTradeGroupConfiguration);
			result.IncludeEntityTypeConfiguration(rateUomConfiguration);
			result.IncludeEntityTypeConfiguration(conditionConfiguration);
			result.IncludeEntityTypeConfiguration(conditionValueConfiguration);
			result.IncludeEntityTypeConfiguration(tariffUomConfiguration);

			return result;
		}
	}
}
