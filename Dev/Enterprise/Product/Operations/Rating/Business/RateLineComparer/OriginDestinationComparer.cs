using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class OriginDestinationComparer : BaseRateLineComparer
	{
		public OriginDestinationComparer(RatingCriteria criteria)
		{
			this.Criteria = criteria;
		}

		public RatingCriteria Criteria { get; }

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (ServiceAutoRater.UseServiceAutoRaterToCompareOriginDestination(Criteria, line1, line2))
			{
				return 0;
			}

			var result = IsOriginDestinationOverridden(line2.Line, line1.Line, Criteria) - IsOriginDestinationOverridden(line1.Line, line2.Line, Criteria);
			if (Criteria != null && result == 0 && (!string.IsNullOrWhiteSpace(Criteria.RateOriginCode) || !string.IsNullOrWhiteSpace(Criteria.RateDestinationCode)))
			{
				var currentEntryBelongToJobRateOriginDestinationValue = GetRateEntryBelongToJobRateOriginDestinationValue(line1.Line, Criteria);
				var overridenEntryBelongToJobRateOriginDestinationValue = GetRateEntryBelongToJobRateOriginDestinationValue(line2.Line, Criteria);

				if (currentEntryBelongToJobRateOriginDestinationValue > overridenEntryBelongToJobRateOriginDestinationValue)
				{
					result -= 1;
				}
				else if (currentEntryBelongToJobRateOriginDestinationValue < overridenEntryBelongToJobRateOriginDestinationValue)
				{
					result += 1;
				}
			}
			return result;
		}

		public static int IsOriginDestinationOverridden(IRateLine rateLine, IRateLine overrideLine, RatingCriteria criteria)
		{
			var currentEntry = rateLine.ParentRateEntry;
			var overrideEntry = overrideLine.ParentRateEntry;

			var currentEntryOrigin = currentEntry.Origin();
			var currentEntryDestination = currentEntry.Destination();
			var overrideEntryOrigin = overrideEntry.Origin();
			var overrideEntryDestination = overrideEntry.Destination();

			var isOriginOverridden = currentEntryOrigin.IsLessSpecificThan(overrideEntryOrigin, criteria?.Origin);
			var isRateOriginOverridden = currentEntryOrigin.IsLessSpecificThan(overrideEntryOrigin, criteria?.RateOrigin);

			var isDestinationOverridden = currentEntryDestination.IsLessSpecificThan(overrideEntryDestination, criteria?.Destination);
			var isRateDestinationOverridden = currentEntryDestination.IsLessSpecificThan(overrideEntryDestination, criteria?.RateDestination);

			if (currentEntry.IsOriginEntry() || currentEntry.IsFreightEntry())
			{
				if (isOriginOverridden)
				{
					return 4;
				}
				if (isRateOriginOverridden)
				{
					return 3;
				}
				if (isDestinationOverridden)
				{
					return 2;
				}
				if (isRateDestinationOverridden)
				{
					return 1;
				}
			}
			else if (currentEntry.IsDestinationEntry())
			{
				if (isDestinationOverridden)
				{
					return 4;
				}
				if (isRateDestinationOverridden)
				{
					return 3;
				}
				if (isOriginOverridden)
				{
					return 2;
				}
				if (isRateOriginOverridden)
				{
					return 1;
				}
			}

			return 0;
		}

		static int GetRateEntryBelongToJobRateOriginDestinationValue(IRateLine rateLine, RatingCriteria criteria)
		{
			var result = 0;
			var rateEntry = rateLine.ParentRateEntry;

			if (rateEntry.Origin()?.CompletelyCovers(criteria.RateOrigin) ?? true)
			{
				result++;
			}

			if (rateEntry.Destination()?.CompletelyCovers(criteria.RateDestination) ?? true)
			{
				result++;
			}

			return result;
		}

		protected override string GetName()
		{
			return (NoResString)"Origin Destination"; // log message, subject to change, more for support people as of now
		}
	}
}
