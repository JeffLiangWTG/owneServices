using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class OrgHeaderDuplicationFinderWithThresholdOverride : OrgHeaderDuplicationFinder
	{
		const ConfidenceRating DefaultThreshold = ConfidenceRating.None;

		public OrgHeaderDuplicationFinderWithThresholdOverride(OrgHeader header, bool useMaxRecords, IDeduplicationStrategy strategy, ConfidenceRating thresholdOverride = DefaultThreshold)
			: base(header, useMaxRecords, strategy)
		{
			ThresholdOverride = thresholdOverride;
		}

		public OrgHeaderDuplicationFinderWithThresholdOverride(OrgHeader header, bool shouldUseCache, bool useMaxRecords, ConfidenceRating thresholdOverride = DefaultThreshold)
			: base(header, shouldUseCache, useMaxRecords)
		{
			ThresholdOverride = thresholdOverride;
		}

		public OrgHeaderDuplicationFinderWithThresholdOverride(DeduplicationOrgHeader deduplicationOrgHeader, bool shouldUseCache, bool useMaxRecords, ConfidenceRating thresholdOverride = DefaultThreshold)
			: base(deduplicationOrgHeader, shouldUseCache, useMaxRecords)
		{
			ThresholdOverride = thresholdOverride;
		}

		public ConfidenceRating ThresholdOverride { get; }

		protected override double GetScoreThresholdFromConfidenceRating()
		{
			return ScoringResult.ConfidenceRatingToScoreThreshold(ThresholdOverride);
		}
	}
}
