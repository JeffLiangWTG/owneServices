using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business
{
	public class OrgHeaderDetailsViewDuplicationFinder : DetailsViewDuplicationFinder<IOrgHeader>
	{
		public OrgHeaderDetailsViewDuplicationFinder(DeduplicationOrgHeader header, IEnumerable<DeduplicationOrgHeader> targetGlows, bool shouldUseCache = true)
			: base(shouldUseCache)
		{
			if (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value && header != null && (header).ShouldRunDeduplication)
			{
				MasterGlow = header;
				TargetGlows = targetGlows;
			}
		}

		protected override IOrgHeader MasterGlow { get; }

		protected override Guid GetGlowPK(IOrgHeader glowModel)
		{
			return glowModel.OH_PK;
		}

		protected override ScoringResult ScoreGlowModel(IOrgHeader masterGlow, IOrgHeader targetGlow)
		{
			return TargetScorerController.Score(masterGlow, targetGlow, true);
		}

		protected override (string Part1, string Part2) GenerateCacheSubkey(IOrgHeader targetGlow)
		{
			var countryCode = string.Empty;
			if (targetGlow.OH_RL_NKClosestPort?.Length >= 2)
			{
				countryCode = targetGlow.OH_RL_NKClosestPort.Substring(0, 2);
			}

			return (targetGlow.OH_FullName, countryCode);
		}

		protected override Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel)
		{
			return patternMatchingResultModel.OrgPK;
		}
	}
}
