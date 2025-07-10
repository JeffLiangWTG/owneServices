using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class CombinedCalculator : BaseCombinedCalculator
	{
		public CombinedCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Combined;

		internal override bool TM_BreakEditable(RateLineItem item)
		{
			// In CMB calculator, TM_Break column is used to specify minimum chargeable weight on MIN line item.
			if (item.RateOperatorIsMIN())
			{
				var lineItems = item.Parent.RateLineItems.Cast<IRateLineItem>().ToArray();

				var editable = lineItems.Any(i => i.RateOperatorIsUNT() || i.RateOperatorIsMinus() || i.RateOperatorIsPlus());
				if (!editable)
				{
					item.TM_BreakInfo.ClearValue();
				}

				return editable;
			}

			// Logic for Plus/Minus types is implemented in the base calculator
			return base.TM_BreakEditable(item);
		}

		protected override void ValidateTM_BreakCore(RateLineItem item)
		{
			if (item.RateOperatorIsMIN() && TM_BreakEditable(item))
			{
				if (!item.TM_Value.IsEmpty && !item.TM_Break.IsEmpty)
				{
					item.TM_BreakInfo.AddError(ErrorMessages.MinChargeableAmountIsNotAllowedIfMinRateSpecified);
				}

				MandatoryValidation.CheckNotNegative(item.TM_BreakInfo);
				return;
			}

			base.ValidateTM_BreakCore(item);
		}

		public override void ValidateTM_Value(RateLineItem lineItem)
		{
			if (lineItem.RateOperatorIsMIN() && TM_BreakEditable(lineItem) && !lineItem.TM_Break.IsEmpty)
			{
				return;
			}

			base.ValidateTM_Value(lineItem);
		}

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var itemList = base.CheckOrCreateItems().ToList();
			var master = Line as RateLine;

			if (master != null && !((ISupportDataImporting)master).IsImportingData)
			{
				itemList.AddRange(master.RateLineItems.AddAIRFreightLineItems());
			}

			return itemList;
		}

		#endregion

		#region Costs Comparer Charges Summary

		protected override IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			var result = new List<ChargesSummaryItem>();

			if (!Minimum.IsEmpty)
			{
				result.Add(new ChargesSummaryItem(Calculator.Items.Operator.MIN, Minimum));
			}
			if (!BaseRate.IsEmpty)
			{
				result.Add(new ChargesSummaryItem(Calculator.Items.Operator.BAS, BaseRate));
			}

			if (IsSliding())
			{
				var operatorsList = new List<string>(new string[] { Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus });
				foreach (IRateLineItem lineItem in Line.ChildRateLineItems)
				{
					if (operatorsList.Contains(lineItem.TM_Type))
					{
						result.Add(new ChargesSummaryItem(lineItem.TM_Type, lineItem.TM_Break, lineItem.TM_RelevantValue, lineItem.TM_FlatAmount));
					}
				}
			}
			else if (!PerUnit.IsEmpty)
			{
				result.Add(new ChargesSummaryItem(Calculator.Items.Operator.UNT, PerUnit));
			}

			return result;
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();

			if (!Minimum.IsEmpty)
			{
				result.Add(new PaymentBasis(default, RateInfo.CreateMIN(Minimum, Line.TL_RX_NKCurrency, null, null), AdapterType.RateEntry, string.Empty));
			}

			if (!BaseRate.IsEmpty)
			{
				result.Add(new PaymentBasis(default, RateInfo.CreateFLT(BaseRate, Line.TL_RX_NKCurrency, null, null), AdapterType.RateEntry, string.Empty));
			}

			if (IsSliding())
			{
				var minimumBreakRate = Line.ChildRateLineItems.Where(x => x.RateOperatorIsMinusOrPlus()).OrderBy(x => x.TM_Break).FirstOrDefault();

				if (minimumBreakRate != null)
				{
					var chargeable = new Quantity(1, Unit);
					var rateInfo = RateInfo.CreateUNT(minimumBreakRate.TM_RelevantValue, Unit, Line.TL_RX_NKCurrency, ZString.Format("{0}{1} {2}", minimumBreakRate.RateOperatorIsPlus() ? ">" : "<", minimumBreakRate.TM_Break, Unit));
					result.Add(new PaymentBasis(chargeable, rateInfo, AdapterType.RateEntry, string.Empty));
				}
			}
			else if (!PerUnit.IsEmpty)
			{
				var chargeable = new Quantity(1, Unit);
				var perUnitRateInfo = RateInfo.CreateUNT(PerUnit, Unit, Line.TL_RX_NKCurrency);
				result.Add(new PaymentBasis(chargeable, perUnitRateInfo, AdapterType.RateEntry, string.Empty));
			}

			return result;
		}

		#endregion

		#region Calculator Description Conversion

		protected override bool IsCalculatorDescriptionSupported => true;

		protected override string ConvertCalculatorDescription()
		{
			var convertor = new CombinedCalculatorDescriptionConverter(this);
			return convertor.Convert();
		}

		#endregion

		public override bool SupportsPacksWeightUnitFactor => Line.ParentRateEntry?.IsClientRate() ?? false;

		public override bool SupportsProductLineUnitFactor => true;

		public override bool SupportsPackageLineUnitFactor => true;

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => true;
	}
}

