using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE
{
	internal class SEMeasureToRawRecordConverter
	{
		readonly measure[] measureList;
		internal SEMeasureToRawRecordConverter(measure[] measureList)
		{
			Argument.NotNull(measureList, nameof(measureList));
			this.measureList = measureList;
		}

		internal void AddRawMeasureConditionRecordList(List<IRawMeasureConditionRecord> measureConditionRecords)
		{
			if (measureConditionRecords == null)
			{
				return;
			}
			foreach (var measure in measureList)
			{
				if (measure.measureCondition != null)
				{
					foreach (var condition in measure.measureCondition)
					{
						Argument.NotNullOrEmpty(measure.goodsNomenclatureCode, nameof(measure.goodsNomenclatureCode));
						Argument.NotNullOrEmpty(measure.geographicalAreaId, nameof(measure.geographicalAreaId));
						Argument.NotNullOrEmpty(measure.measureType, nameof(measure.measureType));
						Argument.NotNullOrEmpty(condition.conditionCodeId, nameof(condition.conditionCodeId));
						Argument.NotNullOrEmpty(condition.actionCode, nameof(condition.actionCode));

						if (string.IsNullOrEmpty(condition.certificateCode)
							|| string.IsNullOrEmpty(condition.certificateType)
							|| !ApplicationConfig.MeasureConditionValidMeasureTypeIds.Contains(measure.measureType)
							|| condition.dutyAmountSpecified
							|| (condition.measureConditionComponent != null && condition.measureConditionComponent.Any(x => x.dutyAmountSpecified)))
						{
							continue;
						}

						var additionalCode = measure.additionalCodeType + measure.additionalCodeId;
						additionalCode = string.IsNullOrEmpty(additionalCode) ? null : additionalCode;

						var certificateTypeCode = condition.certificateType + condition.certificateCode;
						certificateTypeCode = string.IsNullOrEmpty(certificateTypeCode) ? null : certificateTypeCode;

						measureConditionRecords.Add(new RawMeasureConditionRecord(
							measure.goodsNomenclatureCode,
							additionalCode,
							measure.quotaOrderNumber,
							measure.dateStart,
							TransformDateEnd(measure.dateEnd),
							measure.geographicalAreaId,
							measure.measureType,
							condition.conditionCodeId,
							certificateTypeCode,
							condition.dutyAmount.ToString(CultureInfo.InvariantCulture),
							condition.monetaryUnitCode,
							condition.measurementUnitCode,
							condition.actionCode,
							string.Empty));
					}
				}
			}
		}

		internal void AddRawMeasureExclusionRecordList(List<IRawMeasureExclusionRecord> exclusionConditionRecords)
		{
			if (exclusionConditionRecords == null)
			{
				return;
			}
			foreach (var measure in measureList)
			{
				if (measure.measureExcludedGeographicalArea != null)
				{
					foreach (var exclusion in measure.measureExcludedGeographicalArea)
					{
						Argument.NotNullOrEmpty(measure.goodsNomenclatureCode, nameof(measure.goodsNomenclatureCode));
						Argument.NotNullOrEmpty(measure.geographicalAreaId, nameof(measure.geographicalAreaId));
						Argument.NotNullOrEmpty(exclusion.geographicalAreaId, nameof(exclusion.geographicalAreaId));

						var additionalCode = measure.additionalCodeType + measure.additionalCodeId;
						additionalCode = string.IsNullOrEmpty(additionalCode) ? null : additionalCode;

						var monthlyRecords = exclusionConditionRecords
							.Where(x => x.TariffHeader == measure.goodsNomenclatureCode
							&& x.AdditionalCode == additionalCode
							&& x.OrderNumber == measure.quotaOrderNumber
							&& x.TradeGroup == measure.geographicalAreaId
							&& x.MeasureTypeId == measure.measureType
							&& x.StartDate == measure.dateStart
							&& x.ExcludedTradeGroup == exclusion.geographicalAreaId);
						if (monthlyRecords.Any())
						{
							foreach (var diffRecord in monthlyRecords.ToList())
							{
								exclusionConditionRecords.Remove(diffRecord);
							}
						}

						exclusionConditionRecords.Add(new RawMeasureExclusionRecord(
							measure.goodsNomenclatureCode,
							additionalCode,
							measure.quotaOrderNumber,
							measure.dateStart,
							TransformDateEnd(measure.dateEnd),
							string.Empty,
							string.Empty,
							measure.geographicalAreaId,
							measure.measureType,
							exclusion.geographicalAreaId));
					}
				}
			}
		}

		static DateTime TransformDateEnd(DateTime endDate) => endDate == DateTime.MinValue ? new DateTime(2079, 06, 06, 23, 59, 00) : endDate.MidnightToEndOfDay();
	}
}
