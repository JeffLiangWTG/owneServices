using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Items.RatePickRule, RateLineItem.Schema.TM_Text, MapTo = "String1", IsMandatory = true, InitialValue = Items.HighestRate, RelatedTo = "RatePickRule")]
	public class HighestRateCalculator : Calculator
	{
		public HighestRateCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.HighestRate;

		#region Overrides

		protected override bool IsBreakUnitAvailableCore()
		{
			return true;
		}

		#endregion

		#region Properties

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Items.Operator.MIN]; }
			set { this[Items.Operator.MIN] = value; }
		}

		public ZString RatePickRule
		{
			get { return (ZString)this[Items.RatePickRule]; }
			set { this[Items.RatePickRule] = value; }
		}

		public bool EnableAsFreightedMode
		{
			get { return Line.ChargeCode != null && Line.ChargeCode.PK != Env.Registry.FreightChargeCode; }
		}

		protected override bool HasUnitMultiple
		{
			get
			{
				if (weightVolumeMultiple == 1)
				{
					return Line.TL_WeightVolumeMultiple != 0;
				}

				return weightVolumeMultiple != 0;
			}
		}

		protected override ZDecimal UnitMultiplier
		{
			get
			{
				if (weightVolumeMultiple != 1 && weightVolumeMultiple != 0)
				{
					return weightVolumeMultiple;
				}

				return Line.TL_WeightVolumeMultiple == 0 ? 1 : Line.TL_WeightVolumeMultiple;
			}
		}
		// Used to "pass" correct UnitMultiplier to base methods. Yuk.
		ZDecimal weightVolumeMultiple;

		public override CodeDescriptionPairList List1
		{
			get
			{
				return Line.Factory.GetCachedValue("RatePickRules", () => new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Items.HighestRate, string.Empty),
					new CodeDescriptionPair(Items.AsFreightedHighestRateWhenMin, Items.AsFreightedHighestRateWhenMinDescription),
					new CodeDescriptionPair(Items.AsFreightedDontApplyWhenMin, Items.AsFreightedDontApplyWhenMinDescription)
				});
			}
		}

		#endregion

		#region Validation

		#region TM_Type

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			base.ValidateTM_TypeCore(lineItem);

			if (lineItem.TM_TypeInfo.HasErrors())
			{
				return;
			}

			if (lineItem.RateOperatorIsRatePickRule())
			{
				return;
			}

			var currentIndex = RateLineBizO.RateLineItems.IndexOf(lineItem);
			if (currentIndex < 0)
			{
				return;
			}

			switch (lineItem.TM_Type)
			{
				case Items.Operator.MIN:
					foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>().Where(x => x != lineItem))
					{
						if (item.RateOperatorIs(lineItem.TM_Type))
						{
							lineItem.TM_TypeInfo.AddError(ErrorMessages.DuplicateMINNotAllowed);
							item.Validation.ValidateTM_Type();
						}
					}
					break;

				case Items.Operator.UNT:

					var items = RateLineBizO.RateLineItems.Cast<RateLineItem>().Where(x => x != lineItem && x.RateOperatorIs(Items.Operator.UNT)).ToList();

					if (items.Count > 1)
					{
						lineItem.TM_TypeInfo.AddError(ErrorMessages.MoreThanTwoUNTNotAllowed);
						foreach (var item in items)
						{
							item.Validation.ValidateTM_Type();
						}
					}

					break;

				default:
					lineItem.TM_TypeInfo.AddError(ErrorMessages.InvalidRateOperator);
					break;
			}
		}

		#endregion

		#region TM_BreakWeightVolume

		public override void ValidateTM_BreakWeightVolume(RateLineItem lineItem)
		{
			base.ValidateTM_BreakWeightVolume(lineItem);

			if (lineItem.RateOperatorIsUNT())
			{
				MandatoryValidation.CheckEntered(lineItem.TM_BreakWeightVolumeInfo);
				foreach (var anotherLineItem in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					if (anotherLineItem != lineItem && anotherLineItem.RateOperatorIsUNT())
					{
						if (BothUnitsAreWeight(lineItem.TM_BreakWeightVolume, anotherLineItem.TM_BreakWeightVolume) ||
							BothUnitsAreVolume(lineItem.TM_BreakWeightVolume, anotherLineItem.TM_BreakWeightVolume))
						{
							lineItem.TM_BreakWeightVolumeInfo.AddError(ErrorMessages.SameUNTNotAllowed);
						}
						else
						{
							anotherLineItem.Validation.ValidateTM_BreakWeightVolume();
						}
						return;
					}
				}

				if (Line.RequiresWeightVolume() && Line.TL_WeightVolume != lineItem.TM_BreakWeightVolume)
				{
					lineItem.TM_BreakWeightVolumeInfo.AddError(Res.GetString("b1d2e916-6dd9-44e8-986a-a6ab6ecc59fb", "Calculator Unit should match Rate Line Unit."));
				}
			}

			ListValidation.ErrorIfInvalidCode(lineItem.TM_BreakWeightVolumeInfo);
		}

		static bool BothUnitsAreWeight(ZString unit1, ZString unit2)
		{
			return Constants.Weight.ContainsCode(unit1) && Constants.Weight.ContainsCode(unit2);
		}

		static bool BothUnitsAreVolume(ZString unit1, ZString unit2)
		{
			return Constants.Volume.ContainsCode(unit1) && Constants.Volume.ContainsCode(unit2);
		}

		#endregion

		#region TM_Text

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);

			if ((lineItem.TM_Text == HighestRateCalculator.Items.AsFreightedHighestRateWhenMin
				|| lineItem.TM_Text == HighestRateCalculator.Items.AsFreightedDontApplyWhenMin)
				&& Env.Registry.FreightChargeCode == Guid.Empty)
			{
				var error = Res.GetString("98ffad23-6116-41a5-8f04-bd6d14620e1a", "As Freighted unit type can not be calculated as no Freight charge code has been nominated. Please nominate a Freight Charge Code in Maintain>System>Registry>AutoRating>Charge Codes>Freight>Freight Charge Code");
				lineItem.TM_TextInfo.AddWarning(error);
			}
		}

		#endregion

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (Line.ChildRateLineItems.Any(item => !item.RateOperatorIsRatePickRule()))
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				result.Add(QuotationLine.New(Line, HighestRateDescriptionText));

				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					if (item.RateOperatorIsUNT())
					{
						var rateDescription = Constants.Weight.ContainsCode(item.TM_BreakWeightVolume)
							? WeightRateDescriptionText
							: VolumeRateDescriptionText;

						QuotationLine weightVolumeLine;
						if (item.TM_RelevantValue != 0)
						{
							var unitDescription = UnitDescriptionInternal(item.TM_BreakWeightVolume, unitMultiplier: item.TM_UnitMultiple);
							var units = GetUnitText(unitDescription);

							weightVolumeLine = QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, rateDescription, units);
						}
						else
						{
							weightVolumeLine = QuotationLine.New(Line, rateDescription);
						}

						if (weightVolumeLine != null)
						{
							weightVolumeLine.Shift();
							result.Add(weightVolumeLine);
						}

						if (item.TM_FlatAmount != 0)
						{
							var flatAmountLine = QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item.TM_FlatAmount, FlatAmountText, (NoResString)ZString.Empty);
							if (flatAmountLine != null)
							{
								flatAmountLine.Shift();
								flatAmountLine.Shift();
								result.Add(flatAmountLine);
							}
						}
					}
				}

				var minimumLine = QuotationLine.Minimum(Line, 0);
				if (minimumLine != null)
				{
					minimumLine.Shift();
					result.Add(minimumLine);
				}
			}

			return result;
		}

		public static string HighestRateDescriptionText => Res.GetString("befdf319-c892-474f-9e85-2a6f04da0da6", "The Highest Rate Of");
		static string WeightRateDescriptionText => Res.GetString("f40e2c20-3f57-48a0-9d5d-e737bf6372d8", "Weight Rate");
		static string VolumeRateDescriptionText => Res.GetString("93cede5b-d39f-4f80-8364-386d5023ff91", "Volume Rate");
		static ResourceString GetUnitText(string unit) => ResString.GetMultilingualString("9cf56228-95f1-43f0-bdf7-6ab044eb03de", "per {0}", unit);

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			if (Line.ChildRateLineItems.Any(item => !item.RateOperatorIsRatePickRule()))
			{
				var description = HighestRateDescriptionText;

				var volumeUnit = "";
				var volumeRate = 0m;
				var volumeFlat = 0m;
				var weightUnit = "";
				var weightRate = 0m;
				var weightFlat = 0m;

				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					if (item.RateOperatorIsUNT())
					{
						var rateDescription = Constants.Weight.ContainsCode(item.TM_BreakWeightVolume)
							? WeightRateDescriptionText
							: VolumeRateDescriptionText;

						if (item.TM_RelevantValue != 0)
						{
							var unitDescription = UnitDescriptionInternal(item.TM_BreakWeightVolume, unitMultiplier: item.TM_UnitMultiple);
							var units = GetUnitText(unitDescription);
							if (Constants.Weight.ContainsCode(item.TM_BreakWeightVolume))
							{
								weightUnit = units;
								weightRate = item.TM_RelevantValue;
							}
							else
							{
								volumeUnit = units;
								volumeRate = item.TM_RelevantValue;
							}
						}

						if (item.TM_FlatAmount != 0)
						{
							if (Constants.Weight.ContainsCode(item.TM_BreakWeightVolume))
							{
								weightFlat = item.TM_FlatAmount;
							}
							else
							{
								volumeFlat = item.TM_FlatAmount;
							}
						}
					}
				}

				result.SetHighest(Line.TL_RX_NKCurrency, HighestRateDescriptionText, weightUnit, weightRate, weightFlat, volumeUnit, volumeRate, volumeFlat);

				var minimum = QuotationLine.GetValue(Calculator.Items.Operator.MIN, Line.ChildRateLineItems);
				result.SetMin(Line.TL_RX_NKCurrency, minimum);
			}

			return result;
		}

		public static string FlatAmountText => Res.GetString("32af35e6-8f4b-441f-963c-12ace4532153", "Flat Amount");

		#endregion

		#region Calculation

		#region WeightVolume

		protected internal override IEnumerable<ZString> GetUnits(AutoRatingCalculatorParameters parameters)
		{
			var allUnits = Line.ChildRateLineItems
				.Where(i => i.TM_Type == Items.Operator.UNT)
				.Select(i => i.TM_BreakWeightVolume).ToList();

			return allUnits;
		}

		#endregion

		class ItemCalculation
		{
			internal ItemCalculation(IRateLineItem item, List<PaymentBasis> payments)
			{
				Item = item;
				Payments = payments;
				Amount = payments.Calculate().amount;
			}
			internal IRateLineItem Item { get; }
			internal decimal Amount { get; }
			internal List<PaymentBasis> Payments { get; }
		}

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var results = GetCalculationResults(calcOutput);
			if (results.Count == 0)
			{
				return;
			}

			var maxResult = results.OrderByDescending(item => item.Amount).First();
			var minResult = results.FirstOrDefault(item => item.Amount != maxResult.Amount);

			if (maxResult.Payments.Count == 0)
			{
				return;
			}

			var calcLog = calcOutput.CalculationLog;
			calcLog.Unit = maxResult.Payments.FirstOrDefault(x => x.RateInfo.Type == RateInfo.RateInfoType.UNT).Chargeable.Unit;

			if (minResult != null)
			{
				var step = calcLog.Steps.FirstOrDefault(x => x.IsPerUnit && x.Result == minResult.Amount);

				if (step != null)
				{
					calcLog.Steps.Remove(step);
				}
			}

			calcOutput.Set(maxResult.Payments);
		}

		List<ItemCalculation> GetCalculationResults(CalculatorOutput calcOutput)
		{
			var results = new List<ItemCalculation>();
			var linesToProcess = Line.ChildRateLineItems.ToList();
			var addAsFreightedDescription = false;

			if (RatePickRule != Items.HighestRate)
			{
				if (Env.Registry.FreightChargeCode == Guid.Empty)
				{
					return results;
				}

				var frtAutoRateInfo = GetFirstFreightRateInfo(calcOutput.Parameters);

				if (frtAutoRateInfo == null)
				{
					return results;
				}

				if (frtAutoRateInfo.IsCalculatedWithMinimumRate && RatePickRule == Items.AsFreightedDontApplyWhenMin)
				{
					return results;
				}

				if (!frtAutoRateInfo.IsCalculatedWithMinimumRate)
				{
					linesToProcess = linesToProcess //Here we must also disregard MIN / MAX payment bases. Need an extension method here I beleive
						.Where(item => item.TM_Type == Items.Operator.UNT && frtAutoRateInfo.Bases.All(x => x.Chargeable.Unit == item.TM_BreakWeightVolume || x.RateInfo.Type == RateInfo.RateInfoType.MIN || x.RateInfo.Type == RateInfo.RateInfoType.MAX) || item.TM_Type == Items.Operator.MIN).ToList();

					if (linesToProcess.All(item => item.TM_Type != Items.Operator.UNT))
					{
						return results;
					}
					addAsFreightedDescription = true;
				}
			}

			foreach (var item in linesToProcess)
			{
				ProcessLineItem(calcOutput, item, addAsFreightedDescription);
				if (calcOutput.PaymentBases.Any())
				{
					var basisList = new List<PaymentBasis>(calcOutput.PaymentBases);
					results.Add(new ItemCalculation(item, basisList));
					calcOutput.Set(new List<PaymentBasis>());
				}
			}

			return results;
		}

		void ProcessLineItem(CalculatorOutput calcOutput, IRateLineItem item, bool addAsFreightedDescription)
		{
			// Can only have one MIN operator
			if (item.RateOperatorIsMIN())
			{
				calcOutput.Minimum = item.TM_RelevantValue;
			}

			if (item.RateOperatorIsUNT())
			{
				AmountFromWeightVolumeItem(calcOutput, item, addAsFreightedDescription);
			}
		}

		void AmountFromWeightVolumeItem(CalculatorOutput calcOutput, IRateLineItem item, bool addAsFreightedDescription)
		{
			var perUnit = item.TM_RelevantValue;
			var unit = item.TM_BreakWeightVolume;
			weightVolumeMultiple = (ZDecimal)item.TM_UnitMultiple;

			var rateInfoDescription = addAsFreightedDescription
				? Res.GetString("1c593fb2-8cf8-425d-8803-9356e5781dbc", "As Freighted")
				: null;

			if (string.IsNullOrEmpty(unit))
			{
				throw new CalculationException("Missing required Unit value");
			}

			CalculatePerUnit(calcOutput, new Quantity(perUnit, unit, description: rateInfoDescription));
			AddFlatAmountToLastCalculation(calcOutput, item.TM_FlatAmount, rateInfoDescription);
		}

		#endregion

		#region NotifyChanged

		bool isSynchronizingUnitValues;

		public override void NotifyChanged(object changedBusinessObject)
		{
			if (!isSynchronizingUnitValues)
			{
				isSynchronizingUnitValues = true;

				if (Line.RequiresWeightVolume())
				{
					var line = changedBusinessObject as RateLine;
					if (line != null)
					{
						UpdateRateLineItem(line);
					}
					else
					{
						var item = changedBusinessObject as RateLineItem;
						if (item != null)
						{
							UpdateRateLine(item);
						}
					}
				}
				else
				{
					SetDefaultRateLineValues();
				}

				isSynchronizingUnitValues = false;
			}
		}

		void SetDefaultRateLineValues()
		{
			RateLineBizO.TL_WeightVolume = ZString.Empty;
			RateLineBizO.TL_WeightVolumeMultiple = 0;
			RateLineBizO.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;

			if (!RateLineBizO.UseOnlyActualWeightMeasure)
			{
				RateLineBizO.UseOnlyActualWeightMeasure = true;
			}
		}

		void UpdateRateLine(RateLineItem rateLineItem)
		{
			if (rateLineItem.TM_Type != Calculator.Items.Operator.UNT || !RateLineBizO.RateLineItems.Contains(rateLineItem))
			{
				return;
			}

			if (rateLineItem.TM_BreakWeightVolume != RateLineBizO.TL_WeightVolume)
			{
				RateLineBizO.TL_WeightVolume = rateLineItem.TM_BreakWeightVolume;
			}

			if (rateLineItem.TM_UnitMultiple != RateLineBizO.TL_WeightVolumeMultiple)
			{
				if (rateLineItem.TM_UnitMultiple == 1 && RateLineBizO.TL_WeightVolumeMultiple == 0)
				{
					return;
				}

				RateLineBizO.TL_WeightVolumeMultiple = rateLineItem.TM_UnitMultiple == 1 ? 0 : (ZDecimal)rateLineItem.TM_UnitMultiple;
			}
		}

		void UpdateRateLineItem(RateLine rateLine)
		{
			var rateLineItem = RateLineBizO.RateLineItems.Cast<RateLineItem>().FirstOrDefault(item => item.TM_Type == Calculator.Items.Operator.UNT);

			if (rateLineItem != null && rateLine.TL_WeightVolume != rateLineItem.TM_BreakWeightVolume)
			{
				rateLineItem.TM_BreakWeightVolume = rateLine.TL_WeightVolume;
			}

			if (rateLineItem != null && rateLine.TL_WeightVolumeMultiple != rateLineItem.TM_UnitMultiple)
			{
				rateLineItem.TM_UnitMultiple = (ZInt)rateLine.TL_WeightVolumeMultiple;
			}
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();
			foreach (var item in Line.ChildRateLineItems)
			{
				if (item.RateOperatorIsUNT())
				{
					var rateInfoPerUnit = RateInfo.CreateUNT(item.TM_RelevantValue, item.TM_BreakWeightVolume, Line.TL_RX_NKCurrency, null);
					var chargeablePerUnit = new Quantity(1, item.TM_BreakWeightVolume);
					result.Add(new PaymentBasis(chargeablePerUnit, rateInfoPerUnit, AdapterType.RateEntry, string.Empty));

					if (!item.TM_FlatAmount.IsEmpty)
					{
						var rateInfo = RateInfo.CreateFLT(item.TM_FlatAmount, Line.TL_RX_NKCurrency, null, null);
						result.Add(new PaymentBasis(default, rateInfo, AdapterType.RateEntry, string.Empty));
					}
				}

				if (item.RateOperatorIsMIN())
				{
					var minRateInfo = RateInfo.CreateMIN(item.TM_RelevantValue, Line.TL_RX_NKCurrency, null, null);
					result.Add(new PaymentBasis(default, minRateInfo, AdapterType.RateEntry, string.Empty));
				}
			}

			return result;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => true;
	}
}

