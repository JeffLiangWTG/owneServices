using System.Collections.Generic;
using System.Linq;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.GUI
{
	public class GlbPersonDeduplicationBatcher : GlbPersonDetailsViewDuplicationFinder, IDeduplicationBatcher
	{
		readonly List<ScoringResult> previousScores;
		readonly PatternMatchingResultModel[] resultModels;

		public GlbPersonDeduplicationBatcher(IDeduplicationGlowObject master, IEnumerable<IDeduplicationGlowObject> targetGlows, IEnumerable<ScoringResult> previousScores, PatternMatchingResultModel[] resultModels)
			: base(master as DeduplicationGlbPerson, targetGlows.Cast<DeduplicationGlbPerson>())
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
