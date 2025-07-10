using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.UNT, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "PerUnit")]
	public sealed class UnitCalculator : Calculator
	{
		public UnitCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Unit;

		#region Properties

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
			result.Add(QuotationLine.PerUnit(Line, RateDescriptionFlags(flags)));
			return result;
		}

		public override DocLineAmount GetDocLineAmount() => GetDocLineAmount(Line);

		static ResourceString PerText(string weightVolumeDescription) => ResString.GetMultilingualString("DD0BAC30-C0F7-42E2-B934-6CBE41CA5FE9", "per {0}", weightVolumeDescription);

		public static DocLineAmount GetDocLineAmount(IRateLine rateLine)
		{
			var result = new DocLineAmount();

			var unit = QuotationLine.GetValue(Calculator.Items.Operator.UNT, rateLine.ChildRateLineItems);
			var unitDescription = rateLine.Calculator.UnitDescription(true);
			result.SetUnit(rateLine.TL_RX_NKCurrency, PerText(unitDescription), unit);

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			CalculatePerUnit(calcOutput, new Quantity(PerUnit, Unit));
		}

		public override bool SupportsProductLineUnitFactor => true;

		public override bool SupportsPackageLineUnitFactor => true;

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override ZString GetCloneCode(CompanyTariffOrCostBasedCalculator source)
		{
			if (source.IsSliding())
			{
				return CombinedCalculator.Code;
			}
			else if (!source.BaseRateWithApplicableIncrease.IsEmpty)
			{
				return FlatPlusPerUnitCalculator.Code;
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

			return result;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => true;
	}
}

