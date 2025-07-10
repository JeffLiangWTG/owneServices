using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "BaseRate")]
	[CalculatorProperty(Calculator.Items.Operator.UNT, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", RelatedTo = "PerUnit")]
	public class FlatPlusPerUnitCalculator : Calculator
	{
		public FlatPlusPerUnitCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.FlatPlusPerUnit;

		#region Properties

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		public ZDecimal PerUnit
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.UNT]; }
			set { this[Calculator.Items.Operator.UNT] = value; }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();
			var baseRateLine = QuotationLine.BaseRate(Line, 0);
			var perUnitLine = QuotationLine.PerUnit(Line, 0);

			if (baseRateLine == null)
			{
				result.Add(QuotationLine.PerUnit(Line, RateDescriptionFlags(flags)));
			}
			else if (perUnitLine == null)
			{
				result.Add(QuotationLine.BaseRate(Line, RateDescriptionFlags(flags)));
			}
			else
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				result.Add(baseRateLine);
				result.Add(perUnitLine);
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var unit = UnitCalculator.GetDocLineAmount(Line);
			var flat = FlatCalculator.GetDocLineAmount(Line);

			return unit + flat;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			CalculatePerUnit(calcOutput, new Quantity(PerUnit, Unit));
			calcOutput.BaseRate = BaseRate;
		}

		public override bool SupportsProductLineUnitFactor => true;

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override ZString GetCloneCode(CompanyTariffOrCostBasedCalculator source)
		{
			if (source.IsSliding())
			{
				return CombinedCalculator.Code;
			}
			else
			{
				return Code;
			}
		}

		#endregion

		#region Costs Comparer Charges Summary

		protected override IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			return new ChargesSummaryItem[]
			{
				new ChargesSummaryItem(Calculator.Items.Operator.BAS, BaseRate),
				new ChargesSummaryItem(Calculator.Items.Operator.UNT, PerUnit)
			};
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();
			var rateInfo = RateInfo.CreateFLT(BaseRate, Line.TL_RX_NKCurrency, null, null);
			result.Add(new PaymentBasis(default, rateInfo, AdapterType.RateEntry, string.Empty));

			var chargeable = new Quantity(1m, Unit);
			var rateInfoPerUnit = RateInfo.CreateUNT(PerUnit / UnitMultiplier, Unit, Line.TL_RX_NKCurrency, UnitDescription(false), unitMultiplier: UnitMultiplier);
			result.Add(new PaymentBasis(chargeable, rateInfoPerUnit, AdapterType.RateEntry, string.Empty));

			return result;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => true;
	}
}

