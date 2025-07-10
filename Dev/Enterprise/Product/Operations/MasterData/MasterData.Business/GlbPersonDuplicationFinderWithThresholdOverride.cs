using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class GlbPersonDuplicationFinderWithThresholdOverride : GlbPersonDuplicationFinder
	{
		const ConfidenceRating DefaultThreshold = ConfidenceRating.None;

		public GlbPersonDuplicationFinderWithThresholdOverride(GlbPerson person, bool useMaxRecords, ConfidenceRating thresholdOverride = DefaultThreshold)
			: base(person, useMaxRecords)
		{
			ThresholdOverride = thresholdOverride;
		}

		public GlbPersonDuplicationFinderWithThresholdOverride(GlbPerson person, bool useMaxRecords, IDeduplicationStrategy strategy, ConfidenceRating thresholdOverride = DefaultThreshold)
			: base(person, useMaxRecords, strategy)
		{
			ThresholdOverride = thresholdOverride;
		}

		public GlbPersonDuplicationFinderWithThresholdOverride(GlbPerson person, bool shouldUseCache, bool useMaxRecords, ConfidenceRating thresholdOverride = DefaultThreshold)
			: base(person, shouldUseCache, useMaxRecords)
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
