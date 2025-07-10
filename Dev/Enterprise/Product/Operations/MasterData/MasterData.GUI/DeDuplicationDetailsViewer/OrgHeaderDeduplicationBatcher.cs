using System.Collections.Generic;
using System.Linq;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.GUI
{
	class OrgHeaderDeduplicationBatcher : OrgHeaderDetailsViewDuplicationFinder, IDeduplicationBatcher
	{
		readonly List<ScoringResult> previousScores;
		readonly PatternMatchingResultModel[] resultModels;

		public OrgHeaderDeduplicationBatcher(IDeduplicationGlowObject header, IEnumerable<IDeduplicationGlowObject> targetGlows, IEnumerable<ScoringResult> previousScores, PatternMatchingResultModel[] resultModels)
			: base(header as DeduplicationOrgHeader, targetGlows.Cast<DeduplicationOrgHeader>())
		{
			this.previousScores = previousScores?.ToList() ?? new List<ScoringResult>();
			this.resultModels = resultModels;
		}

		public List<ScoringResult> ScoreResults()
		{
			ScoringResults.Clear();
			TargetGlows = ExcludeGlowTargetsFromScoredResults(previousScores);
			var results = TargetGlows.Any() ? ScoreResults(resultModels).ToList() : new List<ScoringResult>();
			previousScores.AddRange(results);

			return results;
		}
	}
}
