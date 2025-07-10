using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class TransitTimeComparer : BaseRateLineComparer
	{
		public TransitTimeComparer(RatingCriteria criteria)
		{
			transitTimeFromJobCriteria = criteria.JobDatesProvider.TransitTime;
		}

		readonly string transitTimeFromJobCriteria;

		public override int Compare(FastLine line1, FastLine line2) => Compare(line1, line2, transitTimeFromJobCriteria);

		protected int Compare(FastLine line1, FastLine line2, string transitTime)
		{
			if (!string.IsNullOrEmpty(transitTime))
			{
				var line1TransitTime = line1.ParentRateEntry.TI_TransitTime;
				var line2TransitTime = line2.ParentRateEntry.TI_TransitTime;

				if (line1TransitTime != line2TransitTime)
				{
					if (RatingConstants.TransitTimes.CheckIsSpecialValue(transitTime))
					{
						return CompareWithSpecialValue(line1TransitTime, line2TransitTime, transitTime);
					}

					return CompareWithNumberValue(line1TransitTime, line2TransitTime, false, transitTime);
				}
			}

			return 0;
		}

		int CompareWithSpecialValue(string line1TransitTime, string line2TransitTime, string transitTime)
		{
			return line2TransitTime == transitTime
				? -1
				: line1TransitTime == transitTime
				? 1
				: CompareWithNumberValue(line1TransitTime, line2TransitTime, true, transitTime);
		}

		int CompareWithNumberValue(string line1TransitTime, string line2TransitTime, bool preferSpecialValue, string transitTime)
		{
			var result = string.IsNullOrEmpty(line1TransitTime)
				? -1
				: string.IsNullOrEmpty(line2TransitTime)
				? 1
				: 0;
			if (result != 0)
			{
				return result;
			}

			var transitTimeDifference1 = GetTransitTimeDifference(line1TransitTime, transitTime);
			var transitTimeDifference2 = GetTransitTimeDifference(line2TransitTime, transitTime);

			result = Math.Abs(transitTimeDifference2) - Math.Abs(transitTimeDifference1);

			if (result == 0)
			{
				if (transitTimeDifference1 > 0 && transitTimeDifference2 < 0)
				{
					return 1;
				}

				if (transitTimeDifference1 < 0 && transitTimeDifference2 > 0)
				{
					return -1;
				}

				var transitTimeFromJobCriteriaSpecialFactor = preferSpecialValue ? 1 : -1;
				if (RatingConstants.TransitTimes.CheckIsSpecialValue(line1TransitTime))
				{
					return transitTimeFromJobCriteriaSpecialFactor;
				}

				if (RatingConstants.TransitTimes.CheckIsSpecialValue(line2TransitTime))
				{
					return -transitTimeFromJobCriteriaSpecialFactor;
				}
			}

			return result;
		}

		static int GetTransitTimeDifference(string lineTransitTime, string transitTimeFromJobCriteria)
		{
			var transitTimeFromJobCriteriaInt = RatingConstants.TransitTimes.ConvertToInt(transitTimeFromJobCriteria);
			var transitTime = RatingConstants.TransitTimes.ConvertToInt(lineTransitTime);

			if (transitTimeFromJobCriteriaInt >= 0 && transitTime >= 0)
			{
				return transitTimeFromJobCriteriaInt - transitTime;
			}

			return transitTime;
		}

		protected override string GetName() => (NoResString)"Transit Time"; // log message, subject to change, more for support people as of now
	}
}
