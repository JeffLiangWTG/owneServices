using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	internal static class RateToMeasureConditionHelper
	{
		internal static IRawMeasureConditionRecord ConvertCertificateRecord(IRawRateRecord rawRateRecord, string measureConditionCode, string[] conditionValues)
		{
			Argument.NotNull(rawRateRecord, nameof(rawRateRecord));
			Argument.IsTrue(conditionValues != null && conditionValues.Length >= 2, nameof(conditionValues));
			Argument.NotNullOrEmpty(measureConditionCode, nameof(measureConditionCode));

			if (string.IsNullOrEmpty(conditionValues[1]))
			{
				return null;
			}

			var secondValue = conditionValues[1].Trim();
			secondValue = secondValue.Replace("-", "");

			var matches = new Regex(@"^(\S+) \((\d{2})\)$").Matches(secondValue);
			if (matches.Count == 0 || matches[0].Groups.Count < 3)
			{
				return null;
			}

			var groups = matches[0].Groups;
			var certificateTypeCode = groups[1].ToString();
			var measureActionCode = groups[2].ToString();

			var comment = $"{rawRateRecord.Description2}";

			var rawMeasureConditionRecord = new RawMeasureConditionRecord(rawRateRecord.TariffHeader,
				rawRateRecord.AdditionalCode, rawRateRecord.OrderNumber, rawRateRecord.StartDate, rawRateRecord.EndDate,
				rawRateRecord.TradeGroup, rawRateRecord.MeasureTypeId, measureConditionCode, certificateTypeCode, string.Empty,
				string.Empty, string.Empty, measureActionCode, comment, rawRateRecord.IsImport);

			return rawMeasureConditionRecord;
		}

		internal static IRawMeasureConditionRecord ConvertInformationalCondition(IRawRateRecord rawRateRecord, string measureConditionCode, string actionCode, string condition)
		{
			var conditionFormulaExtractor = new ConditionFormulaExtractor(new MeasuringUnitTransformer());
			var formulaResult = conditionFormulaExtractor.GetFormula(condition, ApplicationConfig.GetRateCodeByMeasureTypeID(rawRateRecord.MeasureTypeId));
			var conditionAmount = "Not presented";
			if (formulaResult != null && formulaResult.Formula != "0")
			{
				conditionAmount = $"Apply the mentioned duty of {formulaResult.Formula}";
			}

			var rawMeasureConditionRecord = new RawMeasureConditionRecord(rawRateRecord.TariffHeader,
				rawRateRecord.AdditionalCode, rawRateRecord.OrderNumber, rawRateRecord.StartDate, rawRateRecord.EndDate,
				rawRateRecord.TradeGroup, rawRateRecord.MeasureTypeId, measureConditionCode, string.Empty, conditionAmount,
				string.Empty, string.Empty, actionCode, rawRateRecord.Description2, rawRateRecord.IsImport);

			return rawMeasureConditionRecord;
		}

		internal static IRawMeasureConditionRecord ConvertFormulaRecord(IRawRateRecord rawRateRecord, string measureConditionCode, string[] conditionValues)
		{
			Argument.NotNull(rawRateRecord, nameof(rawRateRecord));
			Argument.IsTrue(conditionValues != null && conditionValues.Length >= 2, nameof(conditionValues));
			Argument.NotNullOrEmpty(measureConditionCode, nameof(measureConditionCode));

			if (string.IsNullOrEmpty(conditionValues[0]))
			{
				return null;
			}

			var firstValue = conditionValues[0].Trim().Replace(",", string.Empty);

			var matches = new Regex(@"^(\d+.\d{3})( \w{3}){0,1}/(\w+)\((\d{2})\)$").Matches(firstValue);
			if (matches.Count == 0 || matches[0].Groups.Count < 4)
			{
				return null;
			}

			var groups = matches[0].Groups;
			var conditionAmount = groups[1].ToString();
			var monetaryUnit = groups[2].ToString().Trim();
			var measureUnit = groups[3].ToString();
			var measureActionCode = groups[4].ToString();
			var comment = $"{rawRateRecord.Description2}";

			var rawMeasureConditionRecord = new RawMeasureConditionRecord(rawRateRecord.TariffHeader,
				rawRateRecord.AdditionalCode, rawRateRecord.OrderNumber, rawRateRecord.StartDate, rawRateRecord.EndDate,
				rawRateRecord.TradeGroup, rawRateRecord.MeasureTypeId, measureConditionCode, string.Empty, conditionAmount,
				monetaryUnit, measureUnit, measureActionCode, comment, rawRateRecord.IsImport);

			return rawMeasureConditionRecord;
		}

		internal static IEnumerable<IRawRateRecord> GetRawRates(IEnumerable<IRawMeasureConditionRecord> conditionMeasureRecords)
		{
			Argument.NotNull(conditionMeasureRecords, nameof(conditionMeasureRecords));
			var result = new List<IRawRateRecord>();
			foreach (var item in conditionMeasureRecords)
			{
				var rate = $"{item.ConditionAmount} {item.MonetaryUnitCode} {item.MeasureUnit}";
				if (!string.IsNullOrEmpty(rate))
				{
					result.Add(new RawRateRecord(item.TariffHeader, item.AdditionalCode, item.OrderNumber, item.StartDate, item.EndDate, string.Empty, string.Empty, string.Empty
					, item.TradeGroup, item.MeasureTypeId, rate, string.Empty, rateCode:ApplicationConfig.STypeCertificateConditionMeasureRateCode));
				}
			}
			return result;
		}
	}
}

