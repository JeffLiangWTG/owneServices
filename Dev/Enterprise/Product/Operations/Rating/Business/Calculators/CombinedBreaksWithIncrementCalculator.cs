using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class CombinedBreaksWithIncrementCalculator : BaseCombinedCalculator
	{
		public CombinedBreaksWithIncrementCalculator(IRateLine master)
			: base(master)
		{
			InitializeCalculationMethod(master);
		}

		public const string Code = RatingCalculatorCodes.CombinedWithIncrement;

		public bool LabourHourCalculationEnabled { get; private set; }

		public ZDecimal BreakHourRate
		{
			// Get the Hour Rate from the first Rate Line Item (Labour Hour Rate will be configured in the first Rate Line Item only)
			get => Line.ChildRateLineItems.FirstOrDefault(i => i.RateOperatorIsMinus())?.TM_BreakHourRate ?? 0;
		}

		public event Action<bool> RateSelected;

		public override void NotifyChanged(object changedBusinessObject)
		{
			var line = changedBusinessObject as IRateLine;

			if (line != null && line.ChargeCode != null)
			{
				InitializeCalculationMethod(line);

				// Resetting the ratelines as if the charge code group changed from Labour Hour to Material Rate calculations we need to reset the columns in the CBIControl
				ResetRateLineItems();

				// Notify CBIControl to update the layout
				RateSelected?.Invoke(LabourHourCalculationEnabled);
			}
		}

		void InitializeCalculationMethod(IRateLine line)
		{
			if (line != null)
			{
				var chargeCode = line.ChargeCode;
				LabourHourCalculationEnabled = chargeCode != null && chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.LabourHourRate;
			}
		}

		void ResetRateLineItems()
		{
			if (Line.ChildRateLineItems != null && Line.ChildRateLineItems.Any())
			{
				foreach (var item in Line.ChildRateLineItems.Select(i => i as RateLineItem))
				{
					if (LabourHourCalculationEnabled)
					{
						item.TM_RelevantValue = 0;
					}
					else
					{
						item.TM_BreakHour = 0;
						item.TM_BreakHourRate = 0;
					}
				}
			}
		}

		#region Calculation

		protected override (IEnumerable<CalculationResult> results, string error) CalculateResult(AutoRatingCalculatorParameters parameters)
		{
			var lineUnit = Line.TL_WeightVolume;
			var amount = GetChargeableAmount(parameters, lineUnit);
			if (!amount.HasValue)
			{
				var emptyResult = new CalculationResult(Line, Enumerable.Empty<PaymentBasis>(), Unit);
				return (new[] { emptyResult }, string.Empty);
			}

			var calcLog = new CalculationLog();
			InitializeCalculationLog(this, calcLog, parameters, Line, lineUnit);

			var calcOutput = new CalculatorOutput(parameters, calcLog);

			var allPayments = new List<PaymentBasis>();

			var breakInfo = GetBreakInfo(parameters, amount.Value);
			if (breakInfo.FoundBreak)
			{
				var unitCount = GetUnitCount(parameters);

				var rateInfo = RateInfo.CreateUNT(
					breakInfo.Amount,
					QuantityUnit.JU,
					Line.TL_RX_NKCurrency,
					unitDescription: breakInfo.UnitDescription,
					unitMultiplier: UnitMultiplier);

				var basis = calcOutput.Parameters.Criteria.CreatePaymentBasis(rateInfo, new Quantity(unitCount.Value.Amount, UnitDescriptionInternal("UNT", addPlural: true, addContainerCode: true)), null, null);
				calcOutput.Add(basis);
			}

			if (calcOutput.PaymentBases.Any())
			{
				allPayments.AddRange(calcOutput.PaymentBases);
			}

			var result = new CalculationResult(Line, allPayments, Unit);
			SetAttributes(result, parameters, Line, lineUnit, breakInfo.LabourHours);
			return (new[] { result }, string.Empty);
		}

		BreakInfo GetBreakInfo(AutoRatingCalculatorParameters parameters, Quantity chargeableAmount) => LabourHourCalculationEnabled ? GetLabourBreakInfo(parameters, chargeableAmount) : GetMaterialBreakInfo(parameters, chargeableAmount);

		BreakInfo GetMaterialBreakInfo(AutoRatingCalculatorParameters parameters, Quantity chargeableAmount)
		{
			var item = GetBreakItem(Line.ChildRateLineItems, parameters, chargeableAmount);
			if (item == null)
			{
				return new() { FoundBreak = false };
			}
			var unitDescription = AdjustMaterialPaymentDescriptions(item.TM_Break, item.TM_Type);
			return new() { FoundBreak = true, Amount = item.TM_Value, UnitDescription = unitDescription };
		}

		BreakInfo GetLabourBreakInfo(AutoRatingCalculatorParameters parameters, Quantity chargeableAmount)
		{
			var specifiedLabourHoursTimeSpan = GetLabourHours(parameters);
			var specifiedLabourHours = specifiedLabourHoursTimeSpan.HasValue ? (decimal)specifiedLabourHoursTimeSpan.Value.TotalHours : 0;

			string unitDescription;
			decimal labourHours;
			if (specifiedLabourHoursTimeSpan.HasValue)
			{
				labourHours = specifiedLabourHours;
				unitDescription = AdjustLabourRatePaymentDescriptions(specifiedLabourHours, withoutBreakInfo: true);
			}
			else
			{
				var item = GetBreakItem(Line.ChildRateLineItems, parameters, chargeableAmount);
				if (item == null)
				{
					return new() { FoundBreak = false };
				}

				labourHours = item.TM_BreakHour;
				unitDescription = AdjustLabourRatePaymentDescriptions(item.TM_BreakHour, breakValue: item.TM_Break, operatorSign: item.TM_Type);
			}
			var amount = labourHours * BreakHourRate;

			return new() { FoundBreak = true, Amount = amount, LabourHours = labourHours, UnitDescription = unitDescription };
		}

		#endregion

		#region Implementation

		Quantity? GetChargeableAmount(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			var measureType = GetCalculationMeasureType(unit);
			Quantity? chargeableAmount;
			switch(measureType)
			{
				case MeasureType.Area:
					chargeableAmount = GetArea(parameters);
					break;
				case MeasureType.Length:
					chargeableAmount = GetLength(parameters);
					break;
				default:
					chargeableAmount = GetUnitCount(parameters);
					break;
			}
			return chargeableAmount;
		}

		MeasureType GetCalculationMeasureType(ZString unit)
		{
			MeasureType measureType;
			switch (unit)
			{
				case QuantityUnit.CM2:
					measureType = MeasureType.Area;
					break;
				case QuantityUnit.CM:
					measureType = MeasureType.Length;
					break;
				default:
					measureType = MeasureType.Unit;
					break;
			}
			return measureType;
		}

		Quantity? GetUnitCount(AutoRatingCalculatorParameters parameters)
		{
			var part = GetPart(parameters, MeasureType.Unit);
			return part != null ? new Quantity((ZDecimal)part.UnitCount, QuantityUnit.JU) : null;
		}

		Quantity? GetArea(AutoRatingCalculatorParameters parameters)
		{
			var part = GetPart(parameters, MeasureType.Area);
			return part != null ? new Quantity(part.AreaMeasure.Actual, QuantityUnit.CM2) : null;
		}

		Quantity? GetLength(AutoRatingCalculatorParameters parameters)
		{
			var part = GetPart(parameters, MeasureType.Length);
			return part != null ? new Quantity(part.LengthMeasure.Actual, QuantityUnit.CM) : null;
		}

		TimeSpan? GetLabourHours(AutoRatingCalculatorParameters parameters)
		{
			var part = GetPart(parameters, MeasureType.Unit);
			return part?.Time?.Span;
		}

		IRateablePart GetPart(AutoRatingCalculatorParameters parameters, MeasureType measureType)
		{
			var parts = parameters.Criteria?.RateableMeasures.GetPartList(measureType);
			return parts != null && parts.Count > 0 ? parts[0] : null;
		}

		#endregion

		#region Payment Description

		string AdjustMaterialPaymentDescriptions(decimal breakValue, string operatorSign)
		{
			return Res.GetString("f47ac10b-58cc-4372-a567-0e02b2c3d479", "{0} for {1}{2} {3}", UnitDescriptionInternal("UNT", addPlural: true, addContainerCode: true), breakValue, operatorSign, Line.TL_WeightVolume);
		}

		string AdjustLabourRatePaymentDescriptions(decimal breakRate, bool withoutBreakInfo = false, decimal? breakValue = 0, string operatorSign = null)
		{
			var unitDescription = UnitDescriptionInternal("UNT", addPlural: true, addContainerCode: true);
			return withoutBreakInfo ?
				Res.GetString("3f4a8c08-5d55-41d7-89d2-9fd7550ac3f9", "{0} {1} Hour(s)/{2} @ {3} {4}/Hour(s)", unitDescription, breakRate, unitDescription, Line.TL_RX_NKCurrency, BreakHourRate.ToString(2))
				: Res.GetString("a3b2c1d4-e5f6-7890-ab12-cd34ef56gh78", "{0} {1} Hour(s)/{2} @ {3} {4}/Hour(s) for {5}{6} {7}", unitDescription, breakRate, unitDescription, Line.TL_RX_NKCurrency, BreakHourRate.ToString(2), breakValue, operatorSign, Line.TL_WeightVolume);
		}

		#endregion

		static void SetAttributes(
			CalculationResult result,
			AutoRatingCalculatorParameters parameters,
			IRateLine calculatorLine,
			string unit, decimal? hour)
		{
			if (hour != null)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.BreakHours, hour.ToString());
			}
			SetAttributes(result, parameters, calculatorLine, unit);
		}
	}

	class BreakInfo
	{
		public bool FoundBreak { get; set; }
		public decimal Amount { get; set; }
		public string UnitDescription { get; set; }
		public decimal? LabourHours { get; set; }
	}
}

