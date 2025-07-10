using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class ContainerQualityComparer : BaseRateLineComparer
	{
		public ContainerQualityComparer(RatingCriteria criteria)
		{
			Criteria = criteria;
		}

		protected override string GetName() => (NoResString)"Container Quality";

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (Criteria.IsContainerised && line1.ParentRateEntry.IsSpotEntry == line2.ParentRateEntry.IsSpotEntry)
			{
				var qualities = Criteria.RateableMeasures.GetDistinctContainerQualities() ?? Enumerable.Empty<string>();

				if (qualities.Any() && !qualities.Any(x => string.IsNullOrEmpty(x)))
				{
					return GetIndex(line1) - GetIndex(line2);
				}
			}

			return 0;
		}

		int GetIndex(FastLine line)
		{
			if (line.ParentRateEntry is WiseEntry wiseEntry && !string.IsNullOrEmpty(wiseEntry.ContainerQuality))
			{
				return 1;
			}

			return 0;
		}

		RatingCriteria Criteria { get; }
	}
}
