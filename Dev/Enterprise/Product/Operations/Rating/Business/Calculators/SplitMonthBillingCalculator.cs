using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.Minus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.Plus, RateLineItem.Schema.TM_RelevantValue)]
	public class SplitMonthBillingCalculator : Calculator
	{
		public SplitMonthBillingCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.SplitMonthBilling;

		#region Properties

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MIN]; }
			set { this[Calculator.Items.Operator.MIN] = value; }
		}

		#endregion

		#region Validation

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			ValidateGridCalculator(lineItem, false, true, false);
		}

		protected override void ValidateTM_BreakCore(RateLineItem item)
		{
			if (item.TM_Break > 31)
			{
				item.TM_BreakInfo.AddError(Res.GetString("a9cd25dc-590c-4174-9141-7eb3ce59a418", "Number of Days in month should be less or equal to 31."));
			}
			else if (item.TM_Type != Calculator.Items.Operator.MIN && item.TM_Type != Calculator.Items.Operator.BAS && item.TM_Break < 1)
			{
				item.TM_BreakInfo.AddError(Res.GetString("e7b0147f-1db7-4928-9b04-83ef5786a74c", "Number of Days in month should be greater than 0."));
			}
			else
			{
				base.ValidateTM_BreakCore(item);
			}
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (IsSliding())
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				result.Add(QuotationLine.Minimum(Line, 0));
				result.Add(QuotationLine.BaseRate(Line, 0));

				foreach (var item in Line.ChildRateLineItems.Cast<RateLineItem>().Where(i => i.RateOperatorIsMinusOrPlus()))
				{
					var breakUnit = ItemBreakDescription(item);
					var perUnit = PerText(UnitDescription(false));

					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, breakUnit, perUnit));
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
			var docLineAmount = new DocLineAmount();
			if (IsSliding())
			{
				var min = QuotationLine.GetValue(Calculator.Items.Operator.MIN, Line.ChildRateLineItems);
				docLineAmount.SetMin(Line.TL_RX_NKCurrency, min);

				var baseRate = QuotationLine.GetValue(Calculator.Items.Operator.BAS, Line.ChildRateLineItems);
				docLineAmount.SetFlat(Line.TL_RX_NKCurrency, baseRate);

				foreach (var item in Line.ChildRateLineItems.Cast<RateLineItem>().Where(i => i.RateOperatorIsMinusOrPlus()))
				{
					var breakUnit = ItemBreakDescription(item);
					var perUnit = PerText(UnitDescription(false));
					docLineAmount.SetSliding(Line.TL_RX_NKCurrency, breakUnit, perUnit, item.TM_RelevantValue, 0);
				}
			}
			else
			{
				var min = QuotationLine.GetValue(Calculator.Items.Operator.MIN, Line.ChildRateLineItems);
				docLineAmount.SetMin(Line.TL_RX_NKCurrency, min);
			}
			return docLineAmount;
		}

		static ResourceString PerText(string unit) => ResString.GetMultilingualString("dd5559a7-8276-419f-bf6f-55b169db85e6", "per {0}", unit);

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			calcOutput.Minimum = Minimum;
			calcOutput.BaseRate = BaseRate;
			var parameters = calcOutput.Parameters;
			var item = GetBreakItem(Line.ChildRateLineItems, parameters);
			if (item == null)
			{
				calcOutput.IsEmpty = true;
				return;
			}

			var chargeable = ChargeableAmount(parameters);
			if (chargeable.IsEmpty)
			{
				return;
			}

			string greaterThanText;
			string lessThanText;
			var chargeGroup = Line.ChargeCode.AC_ChargeGroup;
			if (chargeGroup == ChargeCodeGroupList.Codes.WHSInwards)
			{
				greaterThanText = Res.GetString("9889fd79-ad0a-437d-8941-94f25de0708b", "Received After") + " ";
				lessThanText = Res.GetString("1388abde-8d31-4eb7-bcf2-93d9fb3c5fd1", "Received Before") + " ";
			}
			else if (chargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
			{
				greaterThanText = Res.GetString("aae66b0d-75ca-4bfb-b442-712935d9f2ab", "Released After") + " ";
				lessThanText = Res.GetString("dbb01ad1-301f-48fb-8118-9075104f1730", "Released Before") + " ";
			}
			else
			{
				greaterThanText = Res.GetString("930f5e2a-26db-4f4c-9839-805898c0550b", "Greater Than") + " ";
				lessThanText = Res.GetString("af0a481c-93ec-4043-a51f-5f99049cce92", "Less Than") + " ";
			}

			var rate = item.TM_RelevantValue / UnitMultiplier;
			var rateInfo = RateInfo.CreateUNT(rate, GetUnit(parameters), Line.TL_RX_NKCurrency, UnitDescription(false), unitMultiplier: UnitMultiplier);

			var lowerRange = item.IsLowestBreak() ? ZDecimal.Zero : item.TM_Break;

			var descriptionBuilder = new ZStringBuilder();

			var isHighestPlus = item.IsHighestPlus();
			if (isHighestPlus)
			{
				descriptionBuilder.Append(greaterThanText + BreakItems(lowerRange, false));
			}
			else
			{
				var higherRangeItem = SortedBreaks.FirstOrDefault(x => x != item && x.TM_Break >= item.TM_Break && x.RateOperatorIsPlus());
				var higherRange = higherRangeItem?.TM_Break ?? ZDecimal.Zero;

				if (lowerRange != 0)
				{
					descriptionBuilder.Append(greaterThanText + BreakItems(lowerRange, false) + " ");
				}

				descriptionBuilder.Append(lessThanText + BreakItems(higherRange, true));
			}

			if (chargeable.Amount.IsEmpty && isHighestPlus)
			{
				descriptionBuilder.Append(Res.GetString("37060eb0-650c-46a9-8410-85b10b0ae5fb", "On Request"));
			}

			var unitDescription = UnitDescriptionInternal(chargeable.Unit, addPlural: true, addContainerCode: true);
			var basis = parameters.Criteria.CreatePaymentBasis(rateInfo, chargeable, unitDescription, descriptionBuilder.ToString());
			basis.UnroundedChargeable = UnroundedChargeable;
			calcOutput.Add(basis);
		}

		protected override bool HasUnitMultiple => false;

		Quantity TimeAmount(AutoRatingCalculatorParameters parameters)
		{
			return TimeAmountInternal(parameters, TimeUnit);
		}

		protected override Quantity ChargeableAmountForBreakSearch(AutoRatingCalculatorParameters parameters)
		{
			return TimeAmount(parameters);
		}

		public override ZString BreakUnit => TimeUnit;

		public ZString TimeUnit => QuantityUnit.DY;

		protected override ZString LessThanText
		{
			get
			{
				var result = base.LessThanText;
				if (Line != null)
				{
					if (Line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards)
					{
						result = Res.GetString("bb9f7e31-2e8e-48c9-9cf3-43eb469bc4b4", "Received before");
					}
					else if (Line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
					{
						result = Res.GetString("ce885537-5834-424c-9095-4b2bec59e3f5", "Released before");
					}
				}
				return result;
			}
		}

		protected override ZString MoreThanText
		{
			get
			{
				var result = base.MoreThanText;
				if (Line != null)
				{
					if (Line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards)
					{
						result = Res.GetString("f2ea85a1-744c-4d19-a384-9a3d573ee65d", "Received after");
					}
					else if (Line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
					{
						result = Res.GetString("e02c4c3d-6058-4b3f-85cb-a4c5b12bb1bd", "Released after");
					}
				}
				return result;
			}
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			clone.RateLineItems.RemoveAndDeleteAll();
			var masterLines = Line.ChildRateLineItems.Cast<RateLineItem>().ToArray();
			SetClonedMinimum(ctbCalc, clone, masterLines, rateTypeToUpdate);

			foreach (var item in masterLines.Where(x => !x.RateOperatorIsMIN()))
			{
				var newItem = clone.RateLineItems.CloneItem(item);
				newItem.UpdateRateValue(
					x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.BaseRateWithApplicableIncrease, 2),
					rateTypeToUpdate);
			}
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

