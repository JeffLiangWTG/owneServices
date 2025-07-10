using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ConditionGenerator : IConditionGenerator
	{
		readonly IConditionValueDescriptionExtractor conditionValueDescriptionExtractor;
		readonly string[] _ratioMeasureConditions = { "M", "R", "U" };

		public ConditionGenerator(IConditionValueDescriptionExtractor conditionValueDescriptionExtractor)
		{
			Argument.NotNull(conditionValueDescriptionExtractor, nameof(conditionValueDescriptionExtractor));

			this.conditionValueDescriptionExtractor = conditionValueDescriptionExtractor;
		}

		string GenerateComment(IGroupedMeasureConditionKey measureConditionGroupRecord, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords)
		{
			Argument.NotNull(measureConditionGroupRecord, nameof(measureConditionGroupRecord));
			Argument.NotNull(measureConditionRecords, nameof(measureConditionRecords));

			var firstMeasureRecord = measureConditionRecords.FirstOrDefault();

			var measureConditionCode = firstMeasureRecord.MeasureConditionCode;
			var comment = conditionValueDescriptionExtractor.GetComment(measureConditionCode);

			return string.IsNullOrEmpty(comment) ? measureConditionCode : comment;
		}

		public RefCusCondition Convert(IGroupedMeasureConditionKey groupedMeasureConditionKey, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<string> excludedTradeGroups, IEnumerable<IRawRateRecord> uomRecords)
		{
			Argument.NotNull(groupedMeasureConditionKey, nameof(groupedMeasureConditionKey));
			Argument.NotNull(measureConditionRecords, nameof(measureConditionRecords));
			RefCusExcludedTradeGroup[] refCusExcludedTradeGroups = null;
			if (excludedTradeGroups != null && excludedTradeGroups.Any())
			{
				refCusExcludedTradeGroups = excludedTradeGroups.Select(x => new RefCusExcludedTradeGroup
				{
					ZZC_ZZA_NKTradeGroup = x
				}).ToArray();
			}

			var refCusApplicability = new RefCusApplicability
			{
				ZZT_AdditionalCode = groupedMeasureConditionKey.AdditionalCode,
				ZZT_OrderNumber = groupedMeasureConditionKey.OrderNumber,
				ZZT_StartDate = groupedMeasureConditionKey.StartDate,
				ZZT_EndDate = groupedMeasureConditionKey.EndDate,
				ZZT_ZZA_NKTradeGroup = groupedMeasureConditionKey.TradeGroup,
				RefCusExcludedTradeGroups = refCusExcludedTradeGroups
			};
			var refCusApplicabilities = new[] { refCusApplicability };

			if (groupedMeasureConditionKey.MeasureConditionCode == ApplicationConfig.NotApplicableForNoOtherCondition)
			{
				return new RefCusCondition
				{
					ZX1_ZX2_NKConditionType = groupedMeasureConditionKey.MeasureTypeId,
					ZX1_StartDate = groupedMeasureConditionKey.StartDate,
					ZX1_EndDate = groupedMeasureConditionKey.EndDate,
					ZX1_Comment = ApplicationConfig.NoOtherConditionAppliedComment,
					ZX1_ZZS_NKPreference = groupedMeasureConditionKey.MeasureTypeId,
					ZX1_IsImport = groupedMeasureConditionKey.IsImport,
					ZX1_IsExport = !groupedMeasureConditionKey.IsImport,
					RefCusApplicabilities = new[] { refCusApplicability }
				};
			}
			else
			{
				var supplimentaryUnit = uomRecords?.Where(x => x.TradeGroup == groupedMeasureConditionKey.TradeGroup).FirstOrDefault()?.Rate;

				var refCusConditionValues = _ratioMeasureConditions.Contains(groupedMeasureConditionKey.MeasureConditionCode)
					? ConditionGeneratorHelper.GetRatioConditionValues(groupedMeasureConditionKey, measureConditionRecords, supplimentaryUnit)
					: ConditionGeneratorHelper.GetNonRatioConditionValues(groupedMeasureConditionKey, measureConditionRecords);

				if (refCusConditionValues == null || !refCusConditionValues.Any())
				{
					return null;
				}
				else
				{
					var comment = GenerateComment(groupedMeasureConditionKey, measureConditionRecords);
					return new RefCusCondition
					{
						ZX1_ZX2_NKConditionType = groupedMeasureConditionKey.MeasureTypeId,
						ZX1_StartDate = groupedMeasureConditionKey.StartDate,
						ZX1_EndDate = groupedMeasureConditionKey.EndDate,
						ZX1_Comment = comment,
						ZX1_ZZS_NKPreference = groupedMeasureConditionKey.MeasureTypeId,
						ZX1_IsImport = groupedMeasureConditionKey.IsImport,
						ZX1_IsExport = !groupedMeasureConditionKey.IsImport,
						RefCusConditionValues = refCusConditionValues.ToArray(),
						RefCusApplicabilities = new[] { refCusApplicability }
					};
				}
			}
		}
	}
}
