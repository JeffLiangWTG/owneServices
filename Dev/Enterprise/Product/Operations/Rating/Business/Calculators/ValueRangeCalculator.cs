using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Items.Operator.Minus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Items.Operator.Plus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(CalculatorConstants.Type.ApplyTo, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1", InitialValue = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods, RelatedTo = "ApplyTo")]
	public class ValueRangeCalculator : Calculator
	{
		public ValueRangeCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.ValueRange;

		#region Properties

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MIN]; }
			set { this[Calculator.Items.Operator.MIN] = value; }
		}

		#region ApplyTo

		public ZString ApplyTo
		{
			get { return (ZString)this[CalculatorConstants.Type.ApplyTo]; }
			set { this[CalculatorConstants.Type.ApplyTo] = value; }
		}

		#endregion

		#endregion

		#region Validation

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			ValidateGridCalculator(lineItem, false, false, false);
		}

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);

			if (lineItem.RateOperatorIsApplyTo())
			{
				MandatoryValidation.CheckEntered(lineItem.TM_TextInfo);
				ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, ApplyToList);
			}
		}

		#endregion

		#region Lists

		protected ValueApplyToListCodeDescriptionPairList fApplyToList;
		protected ValueApplyToListCodeDescriptionPairList ApplyToList
		{
			get
			{
				if (fApplyToList == null)
				{
					fApplyToList = ValueApplyToListCodeDescriptionPairList.NewValueApplyToList(Line.Country().PK);
				}
				return fApplyToList;
			}
		}

		public override CodeDescriptionPairList List1 => ApplyToList;

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (IsSliding())
			{
				var items = GetSortedBreaks(Line.ChildRateLineItems);
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				result.Add(QuotationLine.Minimum(Line, 0));

				foreach (RateLineItem item in items)
				{
					if (item.RateOperatorIsMinusOrPlus())
					{
						result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, ItemBreakDescription(item), (NoResString)ZString.Empty));
					}
				}
			}
			else
			{
				result.Add(QuotationLine.Minimum(Line, RateDescriptionFlags(flags)));
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var min = QuotationLine.GetValue(Calculator.Items.Operator.MIN, Line.ChildRateLineItems);

			var result = new DocLineAmount();

			if (IsSliding())
			{
				var items = GetSortedBreaks(Line.ChildRateLineItems);
				result.SetMin(Line.TL_RX_NKCurrency, min);

				foreach (RateLineItem item in items)
				{
					if (item.RateOperatorIsMinusOrPlus())
					{
						result.SetSliding(Line.TL_RX_NKCurrency, ItemBreakDescription(item), "", 0, item.TM_RelevantValue);
					}
				}
			}
			else
			{
				result.SetMin(Line.TL_RX_NKCurrency, min);
			}

			return result;
		}

		protected override ZString BreakUnitDescription => Line.TL_RX_NKCurrency;

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var item = GetBreakItem(Line.ChildRateLineItems, parameters);
			var criteria = parameters.Criteria;
			if (item == null)
			{
				calcOutput.IsEmpty = true;
				return;
			}

			calcOutput.Minimum = Minimum;
			var lowerRange = item.IsLowestBreak() ? ZDecimal.Zero : item.TM_Break;

			var description = ZString.Empty;

			var isHighestPlus = item.IsHighestPlus();
			if (isHighestPlus)
			{
				description = Res.GetString("cfdf7cdf-e48d-4ae2-a18b-f4a6235b7fe7", "{0} or more", BreakItems(lowerRange, false));
			}
			else
			{
				var higherRangeItem = SortedBreaks.FirstOrDefault(x => x != item && x.TM_Break >= item.TM_Break && x.RateOperatorIsPlus());
				var higherRange = higherRangeItem != null ? higherRangeItem.TM_Break : ZDecimal.Zero;

				description = Res.GetString("cf78804b-138a-40a0-9730-6085f198c3a2", "{0} - {1}", BreakItems(lowerRange, false), BreakItems(higherRange, true));
			}

			if (item.TM_RelevantValue == 0 && isHighestPlus)
			{
				description = description + ": " + Res.GetString("38a4aeb6-dfe1-4184-b839-d666a33b5072", "On Request");
			}

			var rateInfo = RateInfo.CreateFLT(item.TM_RelevantValue, Line.TL_RX_NKCurrency, description);
			var amount = ChargeableAmount(parameters).Amount;
			var basis = criteria.CreatePaymentBasis(rateInfo,
				new Quantity(amount, Line.TL_RX_NKCurrency),
				Line.TL_RX_NKCurrency,
				ChargeableItems());

			calcOutput.Add(basis);
		}

		protected override bool HasUnitMultiple => false;

		protected override Quantity ChargeableAmountInternal(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			return parameters.Criteria.GetApplicableValue(parameters.Logger, Line.FindRateLineItem(CalculatorConstants.Type.ApplyTo));
		}

		protected override ZString BreakItems(ZDecimal value, bool isHigherRange)
		{
			if (isHigherRange)
			{
				value -= 0.01m;
			}

			return Line.TL_RX_NKCurrency + " " + value.ToString("f2", Culture.CurrentCompanyCountryCulture);
		}

		ZString ChargeableItems()
		{
			var result = ValueApplyToListCodeDescriptionPairList.GetDescriptionFromCode(ApplyTo, Line.Country().PK);
			return result.IsEmpty ? (ZString)"?" : result;
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			clone.RateLineItems.RemoveAndDeleteAll();
			SetClonedMinimum(ctbCalc, clone, RateLineBizO.RateLineItems, rateTypeToUpdate);
			foreach (var item in Line.ChildRateLineItems.Cast<RateLineItem>())
			{
				if (item.RateOperatorIsMIN())
				{
					continue;
				}

				var newItem = clone.RateLineItems.CloneItem(item);
				newItem.UpdateRateValue(
					x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.BaseRateWithApplicableIncrease, 2),
					rateTypeToUpdate);
			}
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

