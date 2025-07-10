using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "BaseRate")]
	public class FlatCalculator : Calculator
	{
		public FlatCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Flat;

		#region Properties

		protected override bool ShouldAddBaseRateDescriptionEvenIfEmpty
		{
			get { return true; }
		}

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();
			result.Add(QuotationLine.NewWithValue(Line, RateDescriptionFlags(flags), Calculator.Items.Operator.BAS, RatingDataRegistry.Instance.BaseRateText.Value, Env.Registry.Rating.FlatFeeMultilingualText));

			return result;
		}

		public override DocLineAmount GetDocLineAmount() => GetDocLineAmount(Line);

		public static DocLineAmount GetDocLineAmount(IRateLine rateLine)
		{
			var result = new DocLineAmount();

			var baseRate = QuotationLine.GetValue(Calculator.Items.Operator.BAS, rateLine.ChildRateLineItems);
			result.SetFlat(rateLine.TL_RX_NKCurrency, baseRate);

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			calcOutput.BaseRate = BaseRate;
		}

		#endregion

		public override bool SupportsPackageLineUnitFactor => true;

		#region Costs Comparer Charges Summary

		protected override IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			return new ChargesSummaryItem[]
			{
				new ChargesSummaryItem(Calculator.Items.Operator.BAS, BaseRate)
			};
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();
			var rateInfo = RateInfo.CreateFLT(BaseRate, Line.TL_RX_NKCurrency, null, null);
			result.Add(new PaymentBasis(default, rateInfo, AdapterType.RateEntry, string.Empty));

			return result;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => true;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

