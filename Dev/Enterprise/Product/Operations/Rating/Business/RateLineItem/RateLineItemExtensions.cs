using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.Rating.Business
{
	public static class RateLineItemExtensions
	{
		public static bool RateOperatorIs(this IRateLineItem item, ZString itemType)
		{
			return item.TM_Type == itemType;
		}

		public static bool RateOperatorIsApplyToOrMNT(this IRateLineItem item)
		{
			return item.RateOperatorIsApplyTo() || item.Calculator() is MinimumCalculator
				&& item.RateOperatorIs(MinimumCalculator.Items.MinimumType);
		}

		public static Calculator Calculator(this IRateLineItem item)
		{
			return item.ParentRateLine != null && item.ParentRateLine.IsCalculatorInitialized ? item.ParentRateLine.Calculator : null;
		}

		public static bool RateOperatorIsPlus(this IRateLineItem item)
		{
			return item.RateOperatorIs(Enterprise.Rating.Business.Calculator.Items.Operator.Plus);
		}

		public static bool RateOperatorIsFirst(this IRateLineItem item)
		{
			return item.RateOperatorIs(FirstPlusAdditionalCalculator.Items.FST);
		}

		public static bool RateOperatorIsAdditional(this IRateLineItem item)
		{
			return item.RateOperatorIs(FirstPlusAdditionalCalculator.Items.ADD);
		}

		public static bool RateOperatorIsUseAccumulated(this IRateLineItem item)
		{
			return item.RateOperatorIs(CombinedCalculator.Items.UseAccumulated);
		}

		public static bool RateOperatorIsIncludeGST(this IRateLineItem item)
		{
			return item.RateOperatorIs(CalculatorConstants.Text.IncludeGST);
		}

		public static bool RateOperatorIsUseGreaterCharge(this IRateLineItem item)
		{
			return item.RateOperatorIs(PercentageBreaksCalculator.Items.GreaterCharge);
		}

		public static bool RateOperatorIsUseBreaksBasedOnValues(this IRateLineItem item)
		{
			return item.RateOperatorIs(PercentageBreaksCalculator.Items.BreaksBasedOnValues);
		}

		public static bool RateOperatorIsHigherChargeableLowerRate(this IRateLineItem item)
		{
			return item.RateOperatorIs(CombinedCalculator.Items.HigherChargeableLowerRate);
		}

		public static bool RateOperatorIsUseInclusiveBreaks(this IRateLineItem item)
		{
			return item.RateOperatorIs(CombinedCalculator.Items.UseInclusiveBreaks);
		}

		public static bool RateOperatorIsMultipleEquipmentsOverMaxWeightVolume(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.MultipleEquipmentsOverMaxWeightVolume);
		}

		public static bool RateOperatorIsExcludeHolidays(this IRateLineItem item)
		{
			return item.RateOperatorIs(TimeCalculator.Items.ExcludeHolidays);
		}

		public static bool RateOperatorIsEquipmentType(this IRateLineItem item)
		{
			return item.RateOperatorIs(CartageCalculator.Items.EquipmentType);
		}

		public static bool RateOperatorIsApplyTo(this IRateLineItem item)
		{
			return item.RateOperatorIs(CalculatorConstants.Type.ApplyTo)
				&& item.TM_Text != Enterprise.Rating.Business.Calculator.Items.Value.CalculationOrder;
		}

		public static bool RateOperatorIsSequenceItem(this IRateLineItem item)
		{
			return item.RateOperatorIs(CalculatorConstants.Type.ApplyTo)
				&& item.TM_Text == Enterprise.Rating.Business.Calculator.Items.Value.CalculationOrder;
		}

		public static bool RateOperatorIsRatePickRule(this IRateLineItem item)
		{
			return item.RateOperatorIs(HighestRateCalculator.Items.RatePickRule);
		}

		public static bool RequiresWeightBreak(this IRateLineItem item)
		{
			return item.RateOperatorIsMinus() || item.RateOperatorIsPlus();
		}

		public static bool RateOperatorIsBAS(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.Operator.BAS);
		}

		public static bool RateOperatorIsUNT(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.Operator.UNT);
		}

		public static bool RateOperatorIsMIN(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.Operator.MIN);
		}

		public static bool RateOperatorIsMAX(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.Operator.MAX);
		}

		public static bool RateOperatorIsPER(this IRateLineItem item)
		{
			return item.RateOperatorIs(CalculatorConstants.Type.PER);
		}

		public static bool RateOperatorIsMinus(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.Operator.Minus);
		}

		public static bool RateOperatorIsMinusOrPlus(this IRateLineItem item)
		{
			var type = item.TM_Type;
			return type == Business.Calculator.Items.Operator.Minus || type == Business.Calculator.Items.Operator.Plus;
		}

		public static bool RateOperatorIsCalculationOrder(this IRateLineItem item)
		{
			return item.TM_Type == CalculatorConstants.Type.ApplyTo && item.TM_Text == Business.Calculator.Items.Value.CalculationOrder;
		}

		public static bool RateOperatorIsAgencyFeeType(this IRateLineItem item)
		{
			return item.RateOperatorIs(AgencyCalculator.Items.AgencyFeeType);
		}

		public static bool RateOperatorIsAgencyLineType(this IRateLineItem item)
		{
			return item.RateOperatorIs(AgencyCalculator.Items.AgencyLineType);
		}

		public static bool RateOperatorIsACIZones(this IRateLineItem item)
		{
			return item.RateOperatorIs(CartageZoneDistanceCalculator.Items.ACIZoneData);
		}

		public static bool RateOperatorIsBreaksPer(this IRateLineItem item)
		{
			return item.RateOperatorIs(Business.Calculator.Items.BreaksPer);
		}

		public static bool RateOperatorIsNonPrintedFlag(this IRateLineItem item)
		{
			return item.RateOperatorIsUseAccumulated()
				   || item.RateOperatorIsHigherChargeableLowerRate()
				   || item.RateOperatorIsUseInclusiveBreaks()
				   || item.RateOperatorIsMultipleEquipmentsOverMaxWeightVolume()
				   || item.RateOperatorIsEquipmentType()
				   || item.RateOperatorIsExcludeHolidays()
				   || item.RateOperatorIsApplyTo()
				   || item.RateOperatorIsRatePickRule()
				   || item.RateOperatorIsCalculationOrder()
				   || item.RateOperatorIsAgencyFeeType()
				   || item.RateOperatorIsAgencyLineType()
				   || item.RateOperatorIsIncludeGST()
				   || item.RateOperatorIsUseGreaterCharge()
				   || item.RateOperatorIsUseBreaksBasedOnValues()
				   || item.RateOperatorIsACIZones()
				   || item.RateOperatorIsBreaksPer();
		}

		public static CostSell CostOrSell(this IRateLineItem item)
		{
			if (item.TM_Text == Business.Calculator.Items.Value.DisbursementApplyToTypes.Disbursements || item.TM_Text == Business.Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement)
			{
				return CostSell.Revenue;
			}

			var entry = item.ParentRateLine.ParentRateEntry;

			return entry.IsCostRate()
				? CostSell.Cost
				: CostSell.Revenue;
		}

		public static bool IsLowestBreakOf(this IRateLineItem item, IEnumerable<IRateLineItem> items)
		{
			var result = item.RateOperatorIsMinus();

			if (item.RateOperatorIsPlus())
			{
				result = !items.Any(x => x.RateOperatorIsMinus() || (x != item && x.RateOperatorIsPlus() && x.TM_Break <= item.TM_Break));
			}

			return result;
		}

		public static IRateLineItem NextRateLineItem(this IRateLineItem line, IEnumerable<IRateLineItem> items)
		{
			return line.Calculator() != null
				? Business.Calculator.GetSortedBreaks(items).FirstOrDefault(x => x.TM_Break >= line.TM_Break && x != line && x.RateOperatorIsPlus())
				: null;
		}

		public static IRateLineItem PrevRateLineItem(this IRateLineItem line, IEnumerable<IRateLineItem> items)
		{
			return line.Calculator() != null
				? Business.Calculator.GetSortedBreaks(items).LastOrDefault(x => x.TM_Break <= line.TM_Break && x != line && x.RateOperatorIsPlus())
				: null;
		}

		public static bool IsLowestPlusOf(this IRateLineItem line, IEnumerable<IRateLineItem> items)
		{
			return line.RateOperatorIsPlus() && !items.Where(x => x.RateOperatorIsPlus()).Any(x => x.TM_Break < line.TM_Break);
		}

		public static bool IsHighestPlusOf(this IRateLineItem line, IEnumerable<IRateLineItem> items)
		{
			return line.RateOperatorIsPlus() && line.NextRateLineItem(items) == null;
		}

		public static bool IsGlobal(this IRateLineItem item)
		{
			var parentRateEntry = item?.ParentRateLine?.ParentRateEntry;
			var result = parentRateEntry != null && (parentRateEntry.IsPublished || IsParentRatingHeaderGlobal(item, parentRateEntry.ParentRatingHeader.PK));

			return result;
		}

		static bool IsParentRatingHeaderGlobal(IRateLineItem item, ZGuid ratingHeaderPK)
		{
			return item.Factory.GetCachedValue("IsParentRatingHeaderGlobal." + ratingHeaderPK, () => item.Factory.Load<RatingHeader>(ratingHeaderPK).IsGlobal());
		}

		public static bool IsLowestBreak(this IRateLineItem item)
		{
			return item.IsLowestBreakOf(item.RateLineItemsFromSameGroup);
		}

		internal static bool IsLowestPlus(this IRateLineItem item)
		{
			return item.RateOperatorIsPlus() && item.IsLowestPlusOf(item.RateLineItemsFromSameGroup);
		}

		internal static ZDecimal NextWeightBreak(this IRateLineItem item)
		{
			var nextItem = item.NextRateLineItem(item.RateLineItemsFromSameGroup);
			return nextItem?.TM_Break ?? -1;
		}

		internal static bool IsHighestPlus(this IRateLineItem item)
		{
			return item.RateOperatorIsPlus() && item.IsHighestPlusOf(item.RateLineItemsFromSameGroup);
		}

		internal static bool IsOverPivotRate(this IRateLineItem iItem)
		{
			var item = iItem as RateLineItem;

			if (item != null
				&& item.ParentRateLine?.ParentRateEntry != null
				&& item.ParentRateLine.ParentRateEntry.IsULD()
				&& item.Calculator() != null
				&& item.Calculator().IsAccumulated
				&& IsHighestPlus(item))
			{
				var prevItem = item.PrevRateLineItem(item.RateLineItemsFromSameGroup);
				if (prevItem != null && prevItem.TM_FlatAmount == item.TM_FlatAmount && !item.TM_Value.IsEmpty)
				{
					return true;
				}
			}

			return false;
		}

		public static RateLineItemsLookups Lookups(this IRateLineItem item)
		{
			if (item is RateLineItem rateLineItemBizO)
			{
				return rateLineItemBizO.Lookups;
			}

			return new RateLineItemsLookups(item, item.Factory);
		}

		public static ZString GetUnitMultipleString(this IRateLineItem item, ZString overriddenValue)
		{
			if (!overriddenValue.IsEmpty)
			{
				return overriddenValue;
			}

			if (item.TM_UnitMultiple == 1m || item.TM_UnitMultiple == 0m)
			{
				return ZString.Empty;
			}

			return item.TM_UnitMultiple.ToString("f0");
		}
	}
}
