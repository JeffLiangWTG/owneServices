using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class OrgHeaderMatchEngineFinder : OrgHeaderDuplicationFinder
	{
		public OrgHeaderMatchEngineFinder(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords, ZGuid onlyScoreTargetsWithThisOrgPK)
			: base(header, shouldUseCache, useMaxRecords)
		{
			OnlyScoreTargetsWithThisOrgPK = onlyScoreTargetsWithThisOrgPK;
		}

		public OrgHeaderMatchEngineFinder(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords)
			: this(header, shouldUseCache, useMaxRecords, ZGuid.Empty)
		{
		}

		ZGuid OnlyScoreTargetsWithThisOrgPK { get; }

		protected override ScoringResult ScoreGlowModel(IOrgHeader master, IOrgHeader target)
		{
			return TargetScorerController.MatchEngineScore(master, target, false);
		}

		protected override double GetScoreThresholdFromConfidenceRating()
		{
			return DeduplicationUtils.GetUXMLOrgMatchExcludeScoreThreshold();
		}

		public ScoringResult MockExactAddressMatch(OrgHeader matchedOrg, OrgAddress matchedAddress)
		{
			var mockScore = new ScoringResult()
			{
				MasterType = typeof(IOrgHeader),
				TargetType = typeof(IOrgHeader),
				MasterPK = MasterGlow.OH_PK,
				TargetPK = matchedOrg.PK.ToGuid(),
				Score = 1,
				ExecutionTime = new TimeSpan()
			};
			var namesChildResult = new ScoringResult()
			{
				MasterType = typeof(MultiSourceCompanyName),
				TargetType = typeof(MultiSourceCompanyName),
				MultiSourceMasterType = typeof(IOrgHeader),
				MultiSourceTargetType = typeof(IOrgHeader),
				MasterPK = MasterGlow.OH_PK,
				TargetPK = matchedOrg.PK.ToGuid(),
				Score = 1,
				ExecutionTime = new TimeSpan()
			};
			var addressChildResult = new ScoringResult()
			{
				MasterType = typeof(IOrgAddress),
				TargetType = typeof(IOrgAddress),
				MasterPK = MasterGlow.OrgAddresses.First().OA_PK,
				TargetPK = matchedAddress.PK.ToGuid(),
				Score = 1,
				ExecutionTime = new TimeSpan()
			};

			var childResults = (List<ScoringResult>)mockScore.ChildResults;
			childResults.Add(namesChildResult);
			childResults.Add(addressChildResult);

			var mockResults = new List<ScoringResult>() { mockScore } as IEnumerable<ScoringResult>;
			DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, mockResults, "ScoringResults", new TimeSpan(), MasterGlow);

			return mockScore;
		}

		protected override IEnumerable<PatternMatchingResultModel> GetPatternMatchingResultModels()
		{
			var results = base.GetPatternMatchingResultModels();
			if (results != null && !OnlyScoreTargetsWithThisOrgPK.Equals(ZGuid.Empty))
			{
				results = results.Where(result => result.OrgPK == OnlyScoreTargetsWithThisOrgPK);
			}

			return results;
		}
	}
}
