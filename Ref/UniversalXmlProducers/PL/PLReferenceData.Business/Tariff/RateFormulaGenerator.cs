using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	class RateFormulaGenerator
	{
		public static string GenerateRateFormulaFromMeasure(measure measureData)
		{
			const string emptyRateFormula = "0";
			var measureComponent = measureData.measureComponent;
			if (measureComponent is null)
			{
				return emptyRateFormula;
			}

			string retv = string.Empty;

			foreach (var item in measureComponent)
			{
				var dutyExpressionId = item.dutyExpression?.dutyExpressionId;
				if (string.IsNullOrEmpty(dutyExpressionId))
				{
					continue;
				}

				switch (dutyExpressionId)
				{
					case Constants.DutyExpressionId.ExpressionId_01:
						retv = OnExpressionId_01GetValue(item);
						break;
					case Constants.DutyExpressionId.ExpressionId_04:
						retv = OnExpressionId_04GetSum(item, retv);
						break;
					case Constants.DutyExpressionId.ExpressionId_15:
						retv = OnExpressionId_MinimumGetMax(item, retv);
						break;
					case Constants.DutyExpressionId.ExpressionId_17:
					case Constants.DutyExpressionId.ExpressionId_35:
						retv = OnExpressionId_MaximumGetMin(item, retv);
						break;
					case Constants.DutyExpressionId.ExpressionId_37:
					case Constants.DutyExpressionId.NotApplicable_95:
					case Constants.DutyExpressionId.NotApplicable_96:
					case Constants.DutyExpressionId.NotApplicable_AN:
					case Constants.DutyExpressionId.NotApplicable_AZ:
						retv = emptyRateFormula;
						break;
					default:
						throw new Exception($"New duty expression id handler is required : {dutyExpressionId}");
				}
			}

			return retv;
		}

		public static string GetFullMeasurementUnitCode(measureComponent measureComponentData)
		{
			if (measureComponentData is null)
			{
				return string.Empty;
			}

			var measurementUnitCode = measureComponentData.measurementUnit?.measurementUnitCode;
			var measurementUnitQualifierCode = measureComponentData.measurementUnitQualifier?.measurementUnitQualifierCode ?? string.Empty;

			return string.IsNullOrEmpty(measurementUnitCode)
				? string.Empty
				: $"{measurementUnitCode}{measurementUnitQualifierCode}";
		}

		internal static string OnMeasureUnitCode(measureComponent measureComponentData)
		{
			string fullMeasurementUnitCode = GetFullMeasurementUnitCode(measureComponentData);
			return string.IsNullOrEmpty(fullMeasurementUnitCode)
			? $"{(measureComponentData.dutyAmount / 100.0m):0.#########} * {Constants.CW1ValueForDuty}"
			: $"{measureComponentData.dutyAmount:0.#########} * [{fullMeasurementUnitCode}]";
		}

		internal static string OnExpressionId_01GetValue(measureComponent measureComponentData)
		{
			return $"{OnMeasureUnitCode(measureComponentData)}";
		}

		internal static string OnExpressionId_04GetSum(measureComponent measureComponentData, string formula)
		{
			return $"{formula} + {OnMeasureUnitCode(measureComponentData)}";
		}

		internal static string OnExpressionId_MinimumGetMax(measureComponent measureComponentData, string formula)
		{
			return $"MAX({OnMeasureUnitCode(measureComponentData)}, {formula})";
		}

		internal static string OnExpressionId_MaximumGetMin(measureComponent measureComponentData, string formula)
		{
			return $"MIN({OnMeasureUnitCode(measureComponentData)}, {formula})";
		}
	}
}
