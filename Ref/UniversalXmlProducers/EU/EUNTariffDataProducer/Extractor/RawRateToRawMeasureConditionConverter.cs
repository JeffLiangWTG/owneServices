using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawRateToRawMeasureConditionConverter : IRawRateToRawMeasureConditionConverter
	{
		public IEnumerable<IRawMeasureConditionRecord> Convert(IRawRateRecord rawRateRecord)
		{
			Argument.NotNull(rawRateRecord, nameof(rawRateRecord));
			var rawMeasureConditionRecords = new List<IRawMeasureConditionRecord>();

			if (!string.IsNullOrEmpty(rawRateRecord.Rate) && rawRateRecord.Rate.Contains("Cond:"))
			{
				var rate = rawRateRecord.Rate.Replace("Cond:", string.Empty);

				var conditions = rate.Split(';');

				foreach (var condition in conditions)
				{
					if (string.IsNullOrEmpty(condition))
					{
						continue;
					}

					var trimmedCondition = condition.Trim();
					var measureConditionCode = trimmedCondition.Substring(0, trimmedCondition.IndexOf(' '));

					trimmedCondition = trimmedCondition.TrimStart(measureConditionCode.ToCharArray());
					var internalValues = trimmedCondition.Split(':');

					if (internalValues.Length < 2)
					{
						continue;
					}

					var firstValue = internalValues[0].Trim();
					IRawMeasureConditionRecord rawMeasureConditionRecord;
					if (firstValue.StartsWith("cert", StringComparison.Ordinal))
					{
						rawMeasureConditionRecord = RateToMeasureConditionHelper.ConvertCertificateRecord(rawRateRecord, measureConditionCode, internalValues);
					}
					else if (firstValue.Equals("(01)", StringComparison.Ordinal) && measureConditionCode == "A")
					{
						var actionCode = firstValue.Replace("(", "").Replace(")", "");
						rawMeasureConditionRecord = RateToMeasureConditionHelper.ConvertInformationalCondition(rawRateRecord, measureConditionCode, actionCode, condition);
					}
					else
					{
						rawMeasureConditionRecord = RateToMeasureConditionHelper.ConvertFormulaRecord(rawRateRecord, measureConditionCode, internalValues);
					}

					if (rawMeasureConditionRecord != null)
					{
						rawMeasureConditionRecords.Add(rawMeasureConditionRecord);
					}
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(rawRateRecord.AdditionalCode))
				{
					rawMeasureConditionRecords.Add(new RawMeasureConditionRecord(rawRateRecord.TariffHeader, rawRateRecord.AdditionalCode, rawRateRecord.OrderNumber, rawRateRecord.StartDate, rawRateRecord.EndDate, rawRateRecord.TradeGroup, rawRateRecord.MeasureTypeId, ApplicationConfig.NotApplicableForNoOtherCondition, string.Empty, string.Empty, string.Empty, string.Empty, ApplicationConfig.NotApplicableForNoOtherCondition, rawRateRecord.Description2, rawRateRecord.IsImport));
				}
			}

			return rawMeasureConditionRecords;
		}

		public IEnumerable<IRawRateRecord> Convert(IEnumerable<IRawMeasureConditionRecord> rawMeasureConditions)
		{
			Argument.NotNull(rawMeasureConditions, nameof(rawMeasureConditions));
			var result = new List<IRawRateRecord>();
			var sMeasureConditionRecords = rawMeasureConditions.Where(x => x.MeasureConditionCode == "S" && x.MeasureAction == "27");
			if (sMeasureConditionRecords.Any())
			{
				result.AddRange(RateToMeasureConditionHelper.GetRawRates(sMeasureConditionRecords));
			}
			return result;
		}
	}
}
