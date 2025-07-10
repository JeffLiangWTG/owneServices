using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TariffGenerator : ITariffGenerator
	{
		readonly IRateGenerator rateGenerator;
		readonly IConditionGenerator conditionGenerator;

		public TariffGenerator(IRateGenerator rateGenerator, IConditionGenerator conditionGenerator)
		{
			Argument.NotNull(rateGenerator, nameof(rateGenerator));

			this.rateGenerator = rateGenerator;
			this.conditionGenerator = conditionGenerator;
		}

		public RefCusTariff GenerateRawTariff(string tariffHeader, IEnumerable<IRawRateRecord> rateRecords, IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<IRawRateRecord> uomRecords)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNull(rateRecords, nameof(rateRecords));
			var rawTariffHeader = new RefCusTariff
			{
				ZZ1_TariffCode = tariffHeader
			};

			var groupedRateRecords = rateRecords.GroupBy(x => new { x.MeasureTypeId, x.AdditionalCode, x.OrderNumber, x.Rate, x.StartDate, x.EndDate, x.ReductionIndicator, x.RateCode });

			var tariffRates = new List<RefCusRate>();
			foreach (var groupedRateRecord in groupedRateRecords)
			{
				var key = groupedRateRecord.Key;

				IEnumerable<IRawMeasureExclusionRecord> filterdMeasureExclusionRecords = null;

				if (measureExclusionRecords != null && measureExclusionRecords.Any())
				{
					filterdMeasureExclusionRecords = measureExclusionRecords.Where(
						x => x.MeasureTypeId == key.MeasureTypeId
							 && ((string.IsNullOrEmpty(x.AdditionalCode) && string.IsNullOrEmpty(key.AdditionalCode)) || (x.AdditionalCode == key.AdditionalCode))
							 && ((string.IsNullOrEmpty(x.OrderNumber) && string.IsNullOrEmpty(key.OrderNumber)) || (x.OrderNumber == key.OrderNumber))
							 && x.StartDate == key.StartDate
							 && x.EndDate == key.EndDate
					);
				}

				var data = new GroupedRateRecord(key.MeasureTypeId, key.AdditionalCode, key.OrderNumber, key.Rate, key.StartDate, key.EndDate, key.ReductionIndicator, key.RateCode);
				var rates = rateGenerator.Convert(data, groupedRateRecord.Select(x => x.TradeGroup), filterdMeasureExclusionRecords, measureConditionRecords);
				tariffRates.AddRange(rates);
			}

			rawTariffHeader.RefCusRates = tariffRates.ToArray();

			var refCusConditions = GenerateConditions(measureConditionRecords, measureExclusionRecords, uomRecords);
			if (refCusConditions != null)
			{
				rawTariffHeader.RefCusConditions = refCusConditions.ToArray();
			}

			return rawTariffHeader;
		}

		public RefCusTariff GenerateRawTariffWithonlyMeasureConditions(string tariffHeader, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawRateRecord> uomRecords)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNull(measureConditionRecords, nameof(measureConditionRecords));
			var rawTariffHeader = new RefCusTariff
			{
				ZZ1_TariffCode = tariffHeader
			};

			var refCusConditions = GenerateConditions(measureConditionRecords, measureExclusionRecords, uomRecords);
			if (refCusConditions != null)
			{
				rawTariffHeader.RefCusConditions = refCusConditions.ToArray();
			}

			return rawTariffHeader;
		}

		IEnumerable<RefCusCondition> GenerateConditions(IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawRateRecord> uomRecords = null)
		{
			var refCusConditions = new List<RefCusCondition>();
			if (measureConditionRecords == null || !measureConditionRecords.Any())
			{
				return refCusConditions;
			}
			//don't create condition for measureConditionCode = 'F' and measureConditionCode = 'S'
			var filteredMeasureConditions = measureConditionRecords.Where(x => x.MeasureConditionCode != "F" && x.MeasureConditionCode != "S");
			var groupedMeasureConditions = filteredMeasureConditions.GroupBy(x => new { x.MeasureTypeId, x.AdditionalCode, x.OrderNumber, x.StartDate, x.EndDate, x.TradeGroup, x.MeasureConditionCode, x.Comment, x.IsImport });

			foreach (var groupedMeasureCondition in groupedMeasureConditions)
			{
				var key = groupedMeasureCondition.Key;

				IEnumerable<string> excludedTradeGroups = null;
				if (measureExclusionRecords != null && measureExclusionRecords.Any())
				{
					excludedTradeGroups = measureExclusionRecords.Where(
						x => x.MeasureTypeId == key.MeasureTypeId
							 && x.AdditionalCode == key.AdditionalCode
							 && x.OrderNumber == key.OrderNumber
							 && x.StartDate == key.StartDate
							 && x.EndDate == key.EndDate
							 && x.TradeGroup == key.TradeGroup
					).Select(y => y.ExcludedTradeGroup);
				}
				var groupedMeasureConditionKey = new GroupedMeasureConditionKey(key.MeasureTypeId, key.AdditionalCode, key.OrderNumber, key.StartDate, key.EndDate, key.TradeGroup, key.MeasureConditionCode, key.Comment, key.IsImport);

				var condition = conditionGenerator.Convert(groupedMeasureConditionKey, groupedMeasureCondition.ToList(), excludedTradeGroups, uomRecords);
				if (condition != null)
				{
					refCusConditions.Add(condition);
				}
			}

			return refCusConditions;
		}
	}
}
