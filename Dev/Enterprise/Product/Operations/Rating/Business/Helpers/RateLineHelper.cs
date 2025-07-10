using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public static class RateLineHelper
	{
		public static string GetUnitMultipleAsString(ZDecimal weightVolumeMultiple)
		{
			return weightVolumeMultiple == 0m ? ZString.Empty : (ZString)weightVolumeMultiple.ToString("f0", CultureInfo.CurrentCulture);
		}

		public static bool IsRateCategoryUnknownForJobServiceSpotEntry(IRateLine rateLine)
			=> (rateLine.ParentRateEntry != null && rateLine.ParentRateEntry.IsJobServiceSpotEntry && rateLine.ParentRateEntry.TI_RateCategory == string.Empty);

		public static bool RequiresChargeableFromJob(IRateLine rateLine)
		{
			if (rateLine.TL_Rounding == RatingRoundingTypes.Chargeable)
			{
				return true;
			}

			if (rateLine.TL_Rounding == RatingRoundingTypes.DefaultFromRegistry && rateLine.ParentRateEntry != null)
			{
				var defaultRounding = DataRegistryRating.Instance.DefaultRounding.GetDefaultRounding(rateLine.ParentRateEntry.TI_RateCategory);
				return defaultRounding.RoundingType == RatingRoundingTypes.Chargeable;
			}

			return false;
		}

		public static void UpdateAllRateLinesStartDate(IEnumerable<RateLine> rateLines, ZDate rateEntryStartDate)
		{
			foreach (var rateLine in rateLines)
			{
				if (IsStartDateInTheRangeOfRateLine(rateLine, rateEntryStartDate))
				{
					rateLine.TL_RateStartDate = rateEntryStartDate;
				}
			}
		}

		public static void UpdateAllRateLinesEndDate(IEnumerable<RateLine> rateLines, ZDate rateEntryEndDate)
		{
			foreach (var rateLine in rateLines)
			{
				if (IsEndDateInTheRangeOfRateLine(rateLine, rateEntryEndDate))
				{
					rateLine.TL_RateEndDate = rateEntryEndDate;
				}
			}
		}

		static bool IsStartDateInTheRangeOfRateLine(RateLine rateLine, ZDate rateEntryStartDate)
		{
			var startDate = rateLine.TL_RateStartDate;
			var endDate = rateLine.TL_RateEndDate;
			return startDate != ZDate.Empty && startDate < rateEntryStartDate && (endDate == ZDate.Empty || endDate >= rateEntryStartDate);
		}

		static bool IsEndDateInTheRangeOfRateLine(RateLine rateLine, ZDate rateEntryEndDate)
		{
			var startDate = rateLine.TL_RateStartDate;
			var endDate = rateLine.TL_RateEndDate;
			return endDate != ZDate.Empty && endDate > rateEntryEndDate && (startDate == ZDate.Empty || startDate <= rateEntryEndDate);
		}
	}
}
