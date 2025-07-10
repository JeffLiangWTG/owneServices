using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business
{
	public class GlbPersonDetailsViewDuplicationFinder : DetailsViewDuplicationFinder<IGlbPerson>
	{
		public GlbPersonDetailsViewDuplicationFinder(IGlbPerson person, IEnumerable<IGlbPerson> targetGlows, bool shouldUseCache = true)
			: base(shouldUseCache)
		{
			if (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value && person != null && ((DeduplicationGlbPerson)person).ShouldRunDeduplication)
			{
				MasterGlow = person;
				TargetGlows = targetGlows;
			}
		}

		protected override IGlbPerson MasterGlow { get; }

		protected override Guid GetGlowPK(IGlbPerson glowModel)
		{
			return glowModel.PER_PK;
		}

		protected override ScoringResult ScoreGlowModel(IGlbPerson masterGlow, IGlbPerson targetGlow)
		{
			return TargetScorerController.Score(masterGlow, targetGlow, true);
		}

		protected override (string Part1, string Part2) GenerateCacheSubkey(IGlbPerson targetGlow)
		{
			//????
			var countryCode = string.Empty;
			if (targetGlow.PER_RN_NKCountry?.Length >= 2)
			{
				countryCode = targetGlow.PER_RN_NKCountry.Substring(0, 2);
			}

			return (targetGlow.PER_FullName, countryCode);
		}

		protected override Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel)
		{
			return patternMatchingResultModel.PersonPK;
		}

		protected override int MaxScoringResult => 50;
	}
}
