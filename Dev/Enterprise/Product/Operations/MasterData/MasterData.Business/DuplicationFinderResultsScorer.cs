using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DuplicationFinderResultsScorer<TGlow>
	{
		readonly bool shouldUseCache;
		readonly TGlow MasterGlow;
		readonly int MaxScoringResult;
		readonly IDeduplicationDebuggerParticipant debuggerParticipant;

		public DuplicationFinderResultsScorer(
			bool shouldUseCache,
			TGlow masterGlow,
			int maxScoringResult,
			IDeduplicationDebuggerParticipant debuggerParticipant,
			ISupportDuplicationFinder duplicationFinder)
		{
			this.shouldUseCache = shouldUseCache;
			MasterGlow = masterGlow;
			MaxScoringResult = maxScoringResult;
			this.debuggerParticipant = debuggerParticipant;
			DuplicationFinder = duplicationFinder;
		}

		ISupportDuplicationFinder DuplicationFinder { get; }

		double excludeScoreThreshold;
		Func<double> excludeScoreThresholdRegistryReader = () => ScoringResult.ConfidenceRatingToScoreThreshold(DeduplicationUtils.GetExcludeConfidenceRatingResult());

		public void SetExcludeScoreThresholdReader(Func<double> registryReader)
		{
			excludeScoreThresholdRegistryReader = registryReader;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<ScoringResult> ScoringResults(
			IEnumerable<TGlow> targetGlows,
			IEnumerable<PatternMatchingResultModel> patternMatchingResults,
			Func<TGlow, Guid> getGlowPK,
			Func<PatternMatchingResultModel, Guid> getPatternMatchingResultModelParentPK,
			Func<TGlow, (string Part1, string Part2)> generateCacheSubkey,
			Func<TGlow, TGlow, ScoringResult> scoreGlowModel,
			CancellationToken token)
		{
			var debuggerScoringResults = new List<ScoringResult>();
			var debuggerTimespan = new TimeSpan();
			var materializedMatchingResults = patternMatchingResults.ToArray();
			var materializedTargetGlows = targetGlows.ToDictionary(getGlowPK);

			if (materializedTargetGlows.Any() && !token.IsCancellationRequested)
			{
				// TODO - JobDocAddress scoring requires the TargetScorer to have a method to compare orgs and job  doc addresses as the parent org is not relevant to the comparison

				var masterGlowPK = getGlowPK(MasterGlow);
				excludeScoreThreshold = excludeScoreThresholdRegistryReader();

				if (shouldUseCache)
				{
					foreach (var result in materializedMatchingResults)
					{
						if (materializedTargetGlows.TryGetValue(getPatternMatchingResultModelParentPK(result), out var targetGlow) && StandardizerCacheHelper.StandardizedResultIndexedByHashValue.TryGetValue(result.HashedValue, out var standardizedOutput))
						{
							var (part1, part2) = generateCacheSubkey(targetGlow);
							StandardizerCacheHelper.StandardizedResultIndexedByOriginalText[(PatternMatchingNameSchema.Constants.Prefix, part1, part2)] = standardizedOutput;
						}
					}
				}

				var syncObj = new object();

				var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = (System.Environment.ProcessorCount / 2) + 1 };
				var shouldRecordDebugScoringResults = DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName) != null;

				using (var timerOverall = new DeduplicationPerformanceMonitor())
				{
					Parallel.ForEach(materializedTargetGlows.Values, parallelOptions, (targetGlow, loopState) =>
					{
						if (token.IsCancellationRequested || DuplicationFinder.ShouldStopProcessing)
						{
							loopState.Stop();
						}

						var targetGlowPK = getGlowPK(targetGlow);
						if (masterGlowPK != targetGlowPK)
						{
							ScoringResult scoringResult;

							using (var timer = new DeduplicationPerformanceMonitor())
							{
								scoringResult = scoreGlowModel(MasterGlow, targetGlow);
								scoringResult.ExecutionTime = timer.ElapsedDuration;
							}

							lock (syncObj)
							{
								if (shouldRecordDebugScoringResults)
								{
									debuggerScoringResults.Add(scoringResult);
								}

								if (scoringResult.Score > excludeScoreThreshold)
								{
									if (DuplicationFinder.ScoringResults.Count < MaxScoringResult)
									{
										DuplicationFinder.ScoringResults.Add(scoringResult);

										if (DuplicationFinder.ScoringResults.Count == MaxScoringResult)
										{
											loopState.Stop();
										}
									}
								}
							}
						}
					});

					if (shouldRecordDebugScoringResults)
					{
						debuggerTimespan = timerOverall.ElapsedDuration;
					}
				}
			}

			debuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, debuggerScoringResults, nameof(ScoringResults), debuggerTimespan, MasterGlow);

			return DuplicationFinder.ScoringResults;
		}
	}
}
