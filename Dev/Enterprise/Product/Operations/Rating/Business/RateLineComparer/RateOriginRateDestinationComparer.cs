using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class RateOriginRateDestinationComparer : BaseRateLineComparer
	{
		public RateOriginRateDestinationComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (_Rating.Cost)
			{
				return 0;
			}

			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;

			return IsRateOriginRateDestinationOverriden(entry2, entry1, criteria) - IsRateOriginRateDestinationOverriden(entry1, entry2, criteria);
		}

		public static int IsRateOriginRateDestinationOverriden(IRateEntry currentEntry, IRateEntry overrideEntry, RatingCriteria criteria)
		{
			var currentEntryRateOrigin = currentEntry.RateOrigin();
			var currentEntryRateDestination = currentEntry.RateDestination();
			var overridenEntryRateOrigin = overrideEntry.RateOrigin();
			var overridenEntryRateDestination = overrideEntry.RateDestination();

			var isRateOriginOverridden = currentEntryRateOrigin.IsLessSpecificThan(overridenEntryRateOrigin, criteria?.RateOrigin);
			var isRateDestinationOverridden = currentEntryRateDestination.IsLessSpecificThan(overridenEntryRateDestination, criteria?.RateDestination);

			if (currentEntry.IsOriginEntry() || currentEntry.IsFreightEntry())
			{
				if (isRateOriginOverridden)
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
				if (isRateDestinationOverridden)
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

		protected override string GetName()
		{
			return (NoResString)"Rate Origin Rate Destination"; // log message, subject to change, more for support people as of now
		}
	}
}
