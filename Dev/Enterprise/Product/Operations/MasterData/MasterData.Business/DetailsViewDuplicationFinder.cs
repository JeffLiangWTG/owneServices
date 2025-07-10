using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public abstract class DetailsViewDuplicationFinder<TGlow> : ISupportDuplicationFinder
	{
		const string DebuggerParticipantName = "DuplicationFinder";

		readonly bool shouldUseCache;
		readonly IDeduplicationDebuggerParticipant debuggerParticipant;

		protected DetailsViewDuplicationFinder(bool shouldUseCache)
		{
			this.shouldUseCache = shouldUseCache;
			debuggerParticipant = new DeduplicationDebuggerParticipant(DebuggerParticipantName);
			DeduplicationUtils.DebuggerHubInstance.Register(debuggerParticipant);
			ScoringResults = new List<ScoringResult>();
		}

		protected IEnumerable<TGlow> ExcludeGlowTargetsFromScoredResults(IEnumerable<ScoringResult> previousScores)
		{
			if (TargetGlows != null)
			{
				var excludedPks = previousScores.Select(p => p.TargetPK);
				return TargetGlows.Where(t => !excludedPks.Contains(GetGlowPK(t)));
			}
			else
			{
				return Enumerable.Empty<TGlow>();
			}
		}

		protected IEnumerable<TGlow> TargetGlows { get; set; }

		protected abstract TGlow MasterGlow { get; }

		protected abstract Guid GetGlowPK(TGlow glowModel);

		protected abstract ScoringResult ScoreGlowModel(TGlow masterGlow, TGlow targetGlow);

		protected abstract (string Part1, string Part2) GenerateCacheSubkey(TGlow targetGlow);

		protected abstract Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel);

		protected virtual int MaxScoringResult => 4;

		public Type TargetType => typeof(TGlow);
		public bool IsProxied { get; set; }
		public bool ShouldStopProcessing { get; set; }
		public List<ScoringResult> ScoringResults { get; }

		DuplicationStatus ISupportDuplicationFinder.LastRunStatus { get; set; }

#if DEBUG
		internal
#endif
		protected IEnumerable<ScoringResult> ScoreResults(IEnumerable<PatternMatchingResultModel> patternMatchingResults)
		{
			var resultScorer = new DuplicationFinderResultsScorer<TGlow>(shouldUseCache, MasterGlow, MaxScoringResult, debuggerParticipant, this);

			return resultScorer.ScoringResults(TargetGlows, patternMatchingResults, GetGlowPK, GetPatternMatchingResultModelParentPK, GenerateCacheSubkey, ScoreGlowModel, CancellationToken.None);
		}

		public Task FindDuplicates()
		{
			return Task.CompletedTask;
		}

		public void RequestToCancel()
		{
		}
	}
}
