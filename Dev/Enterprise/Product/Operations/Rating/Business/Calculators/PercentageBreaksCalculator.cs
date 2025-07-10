using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(PercentageBreaksCalculator.Items.BreaksBasedOnValues, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool4", RelatedTo = "UseBreaksBasedOnValues")]
	[CalculatorProperty(CalculatorConstants.Text.IncludeGST, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool5", RelatedTo = "IncludeGST")]
	public class PercentageBreaksCalculator : BaseCombinedCalculator, IDependentCalculator
	{
		public PercentageBreaksCalculator(IRateLine master)
			: base(master)
		{ }

		protected override void PerformPostItemCreationActions()
		{
			base.PerformPostItemCreationActions();
			Bool4Info.ValueChanged += (sender, args) => (Line as RateLine)?.ResetWeightVolumeIfNeeded();
		}

		public const string Code = RatingCalculatorCodes.PercentageBreaks;

		#region Properties

		public ZString ValueApplyTo
		{
			get
			{
				var result = ZString.Empty;

				foreach (var item in Line.ChildRateLineItems)
				{
					if (item.RateOperatorIsApplyTo())
					{
						if (!result.IsEmpty)
						{
							return Calculator.Items.Value.Multiple;
						}
						else
						{
							result = item.TM_Text;
						}
					}
				}

				return result;
			}
		}

		public ZBool HasMultipleApplyTo
		{
			get { return ValueApplyTo == Calculator.Items.Value.Multiple; }
		}

		public ZBool UseBreaksBasedOnValues
		{
			get { return (ZBool)this[PercentageBreaksCalculator.Items.BreaksBasedOnValues]; }
			set { this[PercentageBreaksCalculator.Items.BreaksBasedOnValues] = value; }
		}

		public ZBool IncludeGST
		{
			get { return (ZBool)this[CalculatorConstants.Text.IncludeGST]; }
			set { this[CalculatorConstants.Text.IncludeGST] = value; }
		}

		public override ZBool UseHigherChargeableLowerRateRule
		{
			get { return false; }
			set { }
		}

		#endregion

		#region Validation

		public override void ValidateTM_Value(RateLineItem lineItem)
		{
			if (!lineItem.RequiresWeightBreak())
			{
				if (!(lineItem.IsDeleted || lineItem.RateOperatorIsNonPrintedFlag()) && lineItem.TM_Value.IsEmpty)
				{
					lineItem.TM_ValueInfo.AddWarning(ErrorMessages.RatePriceIsZero);
				}
			}
		}

		public override void ValidateTM_BreakMinimum(RateLineItem lineItem)
		{
			if (lineItem.TM_BreakMinimum.IsEmpty)
			{
				if (lineItem.Parent.Uses(CalculatorType.PercentageBreaks) && lineItem.RequiresWeightBreak())
				{
					lineItem.TM_BreakMinimumInfo.AddWarning(ErrorMessages.PercentIsZero);
				}
			}
		}

		public override void ValidateTM_AC(RateLineItem lineItem)
		{
			base.ValidateTM_AC(lineItem);
			((IDependentCalculator)this).ValidateTM_AC(lineItem);
		}

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);
			((IDependentCalculator)this).ValidateTM_Text(lineItem);
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			ZString additionalDescription = UseBreaksBasedOnValues ? BreaksAreBasedOnValuesText : "";
			result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags), additionalDescription));

			foreach (var item in Line.ChildRateLineItems)
			{
				if (Line.ChargeCode.AC_SuppressOnQuoteIfZero && item.RateOperatorIsMinus() && item.RateOperatorIsBreaksPer() && item.TM_RelevantValue.IsEmpty
					&& item.TM_BreakMinimum.IsEmpty & item.TM_FlatAmount.IsEmpty)
				{
					continue;
				}

				if (!item.RequiresWeightBreak())
				{
					continue;
				}

				var percent = item.TM_BreakMinimum;

				if (!(percent.IsEmpty && item.TM_FlatAmount.IsEmpty) && !ValueApplyTo.IsEmpty)
				{
					result.Add(QuotationLine.New(Line, ItemBreakDescription(item)));

					foreach (var applyToItem in Line.ChildRateLineItems)
					{
						if (applyToItem.RateOperatorIsApplyTo())
						{
							if (!percent.IsEmpty)
							{
								var lineType = QuotationLineType.Mandatory;

								var unit = GetPercentageUnit(applyToItem);

								lineType |= QuotationLineType.NoCurrency;
								if ((percent - ZArchitecture.Core.Utilities.Round(percent, 2)) != 0m)
								{
									lineType |= QuotationLineType.f4;
								}

								result.Add(QuotationLine.NewWithValue(Line, lineType, percent, ZString.Empty, unit));
							}
						}
					}

					var flatAmountLine = QuotationLine.NewWithValue(Line, 0, item.TM_FlatAmount, FlatAmountText, (NoResString)ZString.Empty);
					if (flatAmountLine != null)
					{
						flatAmountLine.Shift();
					}

					result.Add(flatAmountLine);
				}
			}

			result.Add(QuotationLine.BaseRate(Line, 0));
			result.Add(QuotationLine.Minimum(Line, 0));
			result.Add(QuotationLine.Maximum(Line));

			return result;
		}

		static string BreaksAreBasedOnValuesText => Res.GetString("c79b1856-b7a8-4388-9d04-b907e9967559", "- Breaks are based on values");

		static string FlatAmountText => Res.GetString("d23bcfd2-0331-40e5-9475-f40584dea33b", "Flat Amount");

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			foreach (var item in Line.ChildRateLineItems)
			{
				if (Line.ChargeCode.AC_SuppressOnQuoteIfZero
					&& item.RateOperatorIsMinus()
					&& item.RateOperatorIsBreaksPer()
					&& item.TM_RelevantValue.IsEmpty
					&& item.TM_BreakMinimum.IsEmpty
					&& item.TM_FlatAmount.IsEmpty)
				{
					continue;
				}

				if (!item.RequiresWeightBreak())
				{
					continue;
				}

				var percent = item.TM_BreakMinimum;

				if (!(percent.IsEmpty && item.TM_FlatAmount.IsEmpty) && !ValueApplyTo.IsEmpty)
				{
					var itemBreakDescription = ItemBreakDescription(item);

					var hasSetFlat = false;

					foreach (var applyToItem in Line.ChildRateLineItems)
					{
						if (applyToItem.RateOperatorIsApplyTo())
						{
							if (!percent.IsEmpty)
							{
								var unit = GetPercentageUnit(applyToItem);
								result.SetSliding(Line.TL_RX_NKCurrency, itemBreakDescription, unit, percent, !hasSetFlat ? item.TM_FlatAmount : 0);
								hasSetFlat = true;
							}
						}
					}
				}
			}

			var flat = QuotationLine.GetValue(Calculator.Items.Operator.BAS, Line.ChildRateLineItems);
			result.SetFlat(Line.TL_RX_NKCurrency, flat);

			var min = QuotationLine.GetValue(Calculator.Items.Operator.MIN, Line.ChildRateLineItems);
			result.SetMin(Line.TL_RX_NKCurrency, min);

			var max = QuotationLine.GetValue(Calculator.Items.Operator.MAX, Line.ChildRateLineItems);
			result.SetMax(Line.TL_RX_NKCurrency, max);

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			calcOutput.BaseRate = GetValueByType(Line.ChildRateLineItems, Calculator.Items.Operator.BAS);
			calcOutput.Minimum = GetValueByType(Line.ChildRateLineItems, Calculator.Items.Operator.MIN);
			calcOutput.Maximum = GetValueByType(Line.ChildRateLineItems, Calculator.Items.Operator.MAX);

			IRateLineItem greaterChargeItem;
			var monetaryValue = this.GetValueCalculatorAppliesTo(parameters, out greaterChargeItem, IncludeGST, false, false);

			var rateLineItemToUse = !UseBreaksBasedOnValues
				? GetBreakItem(Line.ChildRateLineItems, parameters)
				: GetBreakItem(Line.ChildRateLineItems, parameters, monetaryValue);

			var criteria = parameters.Criteria;
			if (rateLineItemToUse == null)
			{
				calcOutput.IsEmpty = true;
				return;
			}

			var percent = rateLineItemToUse.TM_BreakMinimum;
			var flatAmount = rateLineItemToUse.TM_FlatAmount;

			var results = new List<PaymentBasis>();

			var rateInfo = RateInfo.CreatePER(percent, Line.TL_RX_NKCurrency);
			var chargeableMoneyDescription = this.GetChargeableDescription(greaterChargeItem, monetaryValue);

			if (UseBreaksBasedOnValues)
			{
				chargeableMoneyDescription = chargeableMoneyDescription + " " + Res.GetString("cb3e6ef9-6a58-4d9c-92cb-da4b2eb9b803", "- Breaks are based on values");
			}
			var calculationResult = parameters.Criteria.CreatePaymentBasis(rateInfo, monetaryValue, null, chargeableMoneyDescription);

			if (!flatAmount.IsEmpty)
			{
				var flatRateInfo = RateInfo.CreateFLT(flatAmount, Line.TL_RX_NKCurrency);
				results.Add(criteria.CreatePaymentBasis(flatRateInfo, new Quantity(0, monetaryValue.Unit)));
			}

			results.Add(calculationResult);
			calcOutput.Add(results);
		}

		#endregion

		IRateLine IDependentCalculator.Master => Line;

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => !Bool4;
	}
}

