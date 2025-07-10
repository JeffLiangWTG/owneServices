using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "Minimum")]
	[CalculatorProperty(Calculator.Items.Operator.UNT, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", RelatedTo = "PerUnit")]
	public class MinimumOrPerUnitCalculator : Calculator
	{
		public MinimumOrPerUnitCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.MinimumOrPerUnit;

		#region Properties

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MIN]; }
			set { this[Calculator.Items.Operator.MIN] = value; }
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

			var minimumLine = QuotationLine.Minimum(Line, 0);
			var perUnitLine = QuotationLine.PerUnit(Line, 0);
			if (minimumLine == null)
			{
				result.Add(QuotationLine.PerUnit(Line, RateDescriptionFlags(flags)));
			}
			else if (perUnitLine == null)
			{
				result.Add(QuotationLine.Minimum(Line, RateDescriptionFlags(flags)));
			}
			else
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				result.Add(minimumLine);
				result.Add(perUnitLine);
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var min = MinimumCalculator.GetDocLineAmount(Line, isChargeCodeMinimum: false, isJobMinimum: false);
			var unit = UnitCalculator.GetDocLineAmount(Line);

			return min + unit;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			CalculatePerUnit(calcOutput, new Quantity(PerUnit, Unit));
			calcOutput.Minimum = Minimum;
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override ZString GetCloneCode(CompanyTariffOrCostBasedCalculator source)
		{
			if (source.IsSliding() || !source.BaseRateWithApplicableIncrease.IsEmpty)
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
				new ChargesSummaryItem(Calculator.Items.Operator.MIN, Minimum),
				new ChargesSummaryItem(Calculator.Items.Operator.UNT, PerUnit)
			};
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();
			var chargeable = new Quantity(1m, Unit);
			var rateInfo = RateInfo.CreateUNT(PerUnit / UnitMultiplier, Unit, Line.TL_RX_NKCurrency, UnitDescription(false), unitMultiplier: UnitMultiplier);
			result.Add(new PaymentBasis(chargeable, rateInfo, AdapterType.RateEntry, string.Empty));

			var minRateInfo = RateInfo.CreateMIN(Minimum, Line.TL_RX_NKCurrency, null, null);
			result.Add(new PaymentBasis(default, minRateInfo, AdapterType.RateEntry, string.Empty));

			return result;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => true;
	}
}

