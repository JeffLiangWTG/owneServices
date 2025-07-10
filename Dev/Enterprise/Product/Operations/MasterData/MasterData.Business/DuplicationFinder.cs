using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "Requires more than two generic parameters")]
	public abstract class DuplicationFinder<TMasterBizO, TTargetBizO, TGlow> : DuplicationFinderBase<TMasterBizO, TTargetBizO, TGlow>, IDuplicationFinder<TMasterBizO, TTargetBizO>, ISupportDuplicationFinder
		where TMasterBizO : BusinessObject, IDeduplicatable
		where TTargetBizO : BusinessObject, IDeduplicatable
	{
		const string ScoreDuplicates = "ScoreDuplicates";

		protected readonly TMasterBizO bizo;
		protected DeduplicationExclusionManager<TTargetBizO> exclusionManager;
		protected readonly bool shouldUseCache;
		IDeduplicationDebuggerParticipant debuggerParticipant;

		public bool MeetMinimumRequirements => results != null;

		protected override IDeduplicationDebuggerParticipant DebuggerParticipant
		{
			get
			{
				if (debuggerParticipant == null)
				{
					debuggerParticipant = new DeduplicationDebuggerParticipant();

					if (ShouldFindDuplications)
					{
						DeduplicationUtils.DebuggerHubInstance.Register(debuggerParticipant);
					}
				}

				return debuggerParticipant;
			}
#if DEBUG
			set { debuggerParticipant = value; }
#endif
		}

		DuplicationStatus ISupportDuplicationFinder.LastRunStatus { set; get; }

		protected DuplicationFinder(TMasterBizO bizo, bool shouldUseCache)
		{
			this.bizo = bizo;
			this.shouldUseCache = shouldUseCache;

			ScoringResults = new List<ScoringResult>();
			exclusionManager = new DeduplicationExclusionManager<TTargetBizO>();
		}

		void ShowDuplications(IEnumerable<ScoringResult> duplications)
		{
			var methodName = nameof(ShowDuplications);

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					using (var timer = new DeduplicationPerformanceMonitor())
					{
						DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, duplications, methodName, timer.ElapsedDuration, MasterGlow);
					}

					if (duplications.Any())
					{
						bizo.ValidateDuplicationResult(true);
						bizo.PropagateDeduplication(duplications, results, TargetGlows);
					}
					else
					{
						bizo.ValidateDuplicationResult(false);
					}
				}
				finally
				{
					bizo.PropagateDeduplicationEnded(duplications, results, TargetGlows, ((ISupportDuplicationFinder)this).LastRunStatus, exclusionManager);
				}
			}
		}

		protected DeduplicationResponseStatus GetResponseWithTimeout(TimeSpan timeOut, Func<DeduplicationResponseStatus> action)
		{
			var result = new DeduplicationResponseStatus();
			var timeoutExecution = new DeduplicationTimeout<DeduplicationResponseStatus>(timeOut);

			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					result = timeoutExecution.DoWork(() =>
					{
						return action();
					});
				}
			}
			catch (TimeoutException)
			{
				result = new DeduplicationResponseStatus
				{
					Message = DuplicationResponseMessages.Timeout
				};
			}

			return result;
		}

		protected string ComputeResponseMessage(ZString staffCode, ZGuid targetPk)
		{
			if (((ISupportDuplicationFinder)this).LastRunStatus is DuplicationStatus.ErrorOccurred)
			{
				return DuplicationResponseMessages.Exception;
			}

			if (((ISupportDuplicationFinder)this).LastRunStatus is DuplicationStatus.Timeout)
			{
				return results != null && results.Any() ? DuplicationResponseMessages.PartialTimeout : DuplicationResponseMessages.Timeout;
			}

			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var queryPmtsWithMaster = new ZDBOnlyQuery(typeof(PatternMatchingResult));
				queryPmtsWithMaster.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix);
				queryPmtsWithMaster.AddToFilter(PatternMatchingResultSchema.PMT_MasterPK, bizo.PK);
				var patternMatchingResult = factory.Load<PatternMatchingResult>(queryPmtsWithMaster).Where(pmt => pmt.PMT_Status != "PDU" || pmt.PMT_Status != "NDU");

				if (patternMatchingResult.Any(pmt => pmt.PMT_Status == "EXC"))
				{
					return DuplicationResponseMessages.Exclusion;
				}

				var pmtWithSetTarget = patternMatchingResult.Where(pmt => pmt.PMT_TargetPK == targetPk && pmt.PMT_TargetTableCode == bizo.TablePrefix);

				if (pmtWithSetTarget.Any(pmt => pmt.PMT_Status == "PIG"))
				{
					return DuplicationResponseMessages.PIGExisted;
				}
				else if (pmtWithSetTarget.Any(pmt => pmt.PMT_Status == "TIG" && pmt.PMT_GS_NKExcludeBy == staffCode))
				{
					return DuplicationResponseMessages.TIGExisted;
				}
			}
			return DuplicationResponseMessages.Success;
		}

		protected UserIgnoreStatus ComputeIgnoreStatus(string message)
		{
			return DuplicationResponseMessages.PIGExisted.Equals(message) || DuplicationResponseMessages.PIGExisted.Equals(message) ? UserIgnoreStatus.PermanentIgnore :
				DuplicationResponseMessages.TIGExisted.Equals(message) || DuplicationResponseMessages.TIGExisted.Equals(message) ? UserIgnoreStatus.TemporaryIgnore :
				DuplicationResponseMessages.Exclusion.Equals(message) ? UserIgnoreStatus.ExcludedIgnore : null;
		}

		IEnumerable<ScoringResult> FindPotentialDuplicatesCore()
		{
			try
			{
				StandardizerCacheHelper.StandardizedResultIndexedByHashValue.Clear();
				StandardizerCacheHelper.StandardizedResultIndexedByOriginalText.Clear();

				if (EqualityComparer<TGlow>.Default.Equals(MasterGlow, default))
				{
					ZString message = (NoResString)"You have setup duplication finder with No MasterGlow record. Please check and confirm your setup conditions";
					ExceptionReporter.Instance.ReportDeveloperException("efa7bf6c-ca3f-41b1-b378-1b19ca7a86b0", message, new ArgumentNullException(message));
				}

				StandardizeMaster();

				if (!TokenSource.Token.IsCancellationRequested)
				{
					ProcessDuplications();
				}
			}
			catch (InvalidOperationException ioException)
			{
				var ignoreSystemDataExceptions = new[]
				{
					(NoResString)"invalid attempt to read when no data is present." ,    // client may have closed MDM form while thread is running to finding duplicates
					(NoResString)"reader is closed",
					(NoResString)"connection",
				};
				if (!ignoreSystemDataExceptions.Any(o => ioException.Message.ToLowerInvariant().Contains(o)))
				{
					HandleException(ioException, nameof(FindPotentialDuplicatesCore));
				}
			}
			catch (SqlException sqlException)
			{
				HandleException(sqlException, nameof(FindPotentialDuplicatesCore));
			}

			return ScoringResults;
		}

		protected void HandleException(Exception exception, string name)
		{
			ZString message = $"An error occurred in {name}.";

			DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName,
			new List<DeduplicationDebuggerObjects>
			{
				new DeduplicationDebuggerObjects
				{
					Message = $"{message} {exception.Message}",
					HumanReadableBizName = bizo?.HumanReadableName
				}
			}, nameof(FindPotentialDuplicates), TimeSpan.FromSeconds(1), MasterGlow);

			((ISupportDuplicationFinder)this).LastRunStatus = DuplicationStatus.ErrorOccurred;
			ExceptionReporter.Instance.ReportDeveloperException("F7ABEE63-0D58-4E07-9A8F-EF3258421B23", message, exception);
		}

		protected virtual double GetScoreThresholdFromConfidenceRating()
		{
			return ScoringResult.ConfidenceRatingToScoreThreshold(DeduplicationUtils.GetExcludeConfidenceRatingResult());
		}

		void ProcessDuplications()
		{
			var matchingResultModels = GetPatternMatchingResultModels();

			if (matchingResultModels != null)
			{
				var candidatePKsAndResultModels = FindTargetPKsAndResultModels(matchingResultModels.ToArray()).ToArray();

				if (candidatePKsAndResultModels.Any())
				{
					var resultScorer = new DuplicationFinderResultsScorer<TGlow>(shouldUseCache, MasterGlow, MaxScoringResult, DebuggerParticipant, this);
					resultScorer.SetExcludeScoreThresholdReader(GetScoreThresholdFromConfidenceRating);

					var targetGlows = new List<TGlow>();
					TargetGlows = targetGlows;

					using (Db.DisposableActionForDbConnection())
					using (var timer = new DeduplicationPerformanceMonitor())
					{
						Db.Connection.EnsureIsOpen();
						var batchSize = GetBatchSize(candidatePKsAndResultModels.Length);
						IEnumerable<IGrouping<Guid, PatternMatchingResultModel>> candidatesForEnum = candidatePKsAndResultModels;

						while (ScoringResults.Count < MaxScoringResult && candidatesForEnum.Any() && !TokenSource.Token.IsCancellationRequested)
						{
							var batchCandidates = candidatesForEnum.Take(batchSize).ToArray();
							var targetBizOs = LoadTargetBizOs(new ReadOnlyBusinessObjectFactory { NameForDebugging = string.Format(CultureInfo.InvariantCulture, "Potential Targets Loader") }, new HashSet<Guid>(batchCandidates.Select(u => u.Key))).ToArray();

							if (targetBizOs.Any())
							{
								var targetGlowBizOs = GetTargetGlowBizos(targetBizOs);
								targetGlows.AddRange(targetGlowBizOs);
								DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, targetGlowBizOs, string.Join("_", nameof(ProcessDuplications), nameof(TargetGlows)), timer.ElapsedDuration, MasterGlow);

								resultScorer.ScoringResults(targetGlowBizOs, batchCandidates.SelectMany(u => u), GetGlowPK, GetPatternMatchingResultModelParentPK, GenerateCacheSubkey, ScoreGlowModel, TokenSource.Token);
							}

							candidatesForEnum = candidatesForEnum.Skip(batchSize);
						}

						DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, ScoringResults, string.Join("_", nameof(ProcessDuplications), ScoreDuplicates), timer.ElapsedDuration, MasterGlow);
					}
				}
			}
		}

		protected async Task<IEnumerable<ScoringResult>> GetDuplicationsAsync()
		{
			TokenSource.Token.ThrowIfCancellationRequested();

			var task = Task.Factory.StartNew(() => FindPotentialDuplicates(true), TokenSource.Token, TaskCreationOptions.AttachedToParent, Scheduler);

			return await task.WithExceptionHandler(defaultResult: Enumerable.Empty<ScoringResult>().ToList().AsReadOnly(), exception =>
			{
				HandleException(exception, nameof(GetDuplicationsAsync));
			});
		}

		protected static ZDBOnlyQuery GetExclusionQueryBase(Type type, HashSet<Guid> candidatePKs, TMasterBizO bizo)
		{
			var query = new ZDBOnlyQuery(type);
			var queryForCandidatePKs = new ZQuery(bizo.PKSchemaColumn, candidatePKs);

			query.AddToFilter(queryForCandidatePKs);

			return query;
		}

		protected static ZDBOnlyQuery GetExclusionQuery(Type type, HashSet<Guid> candidatePKs, TMasterBizO bizo, DeduplicationExclusionManager<TTargetBizO> exclusionManager)
		{
			var query = GetExclusionQueryBase(type, candidatePKs, bizo);

			var queryForPig = new ZQuery(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);
			var queryForTig = new ZQuery(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.TemporaryIgnore);
			queryForTig.AddToFilter(PatternMatchingResultSchema.PMT_GS_NKExcludeBy, GlbStaff.CurrentUser.GS_Code);
			var queryForPigAndTig = new ZQuery(queryForPig, JoinCondition.Or, queryForTig);

			var conditionQuery = new ZQuery(PatternMatchingResultSchema.PMT_TargetTableCode, bizo.TablePrefix);
			conditionQuery.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix);

			var subQueryForPigAndTigAsTarget = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_TargetPK, true);
			subQueryForPigAndTigAsTarget.AddToFilter(PatternMatchingResultSchema.PMT_MasterPK, bizo.PK);
			subQueryForPigAndTigAsTarget.AddToFilter(conditionQuery);
			subQueryForPigAndTigAsTarget.AddToFilter(queryForPigAndTig);

			var subQueryForPigAndTigAsMaster = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_MasterPK, true);
			subQueryForPigAndTigAsMaster.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, bizo.PK);
			subQueryForPigAndTigAsMaster.AddToFilter(conditionQuery);
			subQueryForPigAndTigAsMaster.AddToFilter(queryForPigAndTig);

			var subQueryForExc = new ZDBOnlySubQuery(typeof(PatternMatchingResult), PatternMatchingResultSchema.PMT_MasterPK, true);
			subQueryForExc.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix);
			subQueryForExc.AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.Excluded);

			query.AddSubQuery(subQueryForExc, JoinCondition.And);
			query.AddSubQuery(subQueryForPigAndTigAsTarget, JoinCondition.And);
			query.AddSubQuery(subQueryForPigAndTigAsMaster, JoinCondition.And);

			if (exclusionManager != null)
			{
				exclusionManager.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetPermanentIgnoresAsMaster(GetExclusionQueryBase(type, candidatePKs, bizo), bizo),
					ResString.GetMultilingualString("6c108974-8b2f-40e1-b427-21cc9c6662b9", "This record has been ignored for everyone"));

				exclusionManager.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetTemporaryIgnoresAsMaster(GetExclusionQueryBase(type, candidatePKs, bizo), bizo),
					ResString.GetMultilingualString("feea9c13-7365-4d14-a0fb-7cc9dbbffcd0", "This record has been ignored by you"));

				exclusionManager.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetPermanentIgnoresAsTarget(GetExclusionQueryBase(type, candidatePKs, bizo), bizo),
					ResString.GetMultilingualString("05b9793c-4208-4729-8ab8-538dbfb719ad", "This record has been ignored for everyone"));

				exclusionManager.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetTemporaryIgnoresAsTarget(GetExclusionQueryBase(type, candidatePKs, bizo), bizo),
					ResString.GetMultilingualString("ea0737a5-4d4d-4f40-8fd8-681bcb4ea23b", "This record has been ignored by you"));

				exclusionManager.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetExlusionStatus(GetExclusionQueryBase(type, candidatePKs, bizo), bizo),
					ResString.GetMultilingualString("d89135d9-5b25-4a7a-91cf-32cec80f625e", "Excluded from De-duplication"));
			}

			return query;
		}

		protected IEnumerable<TGlow> ExcludeGlowTargetsFromScoredResults(IEnumerable<ScoringResult> previousScores)
		{
			var excludedPks = previousScores.Select(p => p.TargetPK);

			return TargetGlows.Where(t => !excludedPks.Contains(GetGlowPK(t)));
		}

		protected abstract int RegistryTimeout { get; }

		protected IEnumerable<TGlow> TargetGlows { get; set; } = Enumerable.Empty<TGlow>();

		protected abstract ScoringResult ScoreGlowModel(TGlow master, TGlow target);

		protected abstract (string Part1, string Part2) GenerateCacheSubkey(TGlow targetGlow);

		protected abstract Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		public abstract Task<IEnumerable<TMasterBizO>> GetPotentialTargetsByEmailDomainAsync(string emailAddress);

		int duplicateDetectionTimeout;
		protected int DuplicateDetectionTimeout
		{
			get => duplicateDetectionTimeout == 0 ? RegistryTimeout : duplicateDetectionTimeout;
			set { duplicateDetectionTimeout = value; }
		}
		protected virtual bool ShouldFindDuplications => bizo != null && MasterGlow != null && EndableDeduplicationFinderCheckpoint && bizo.ShouldRunDeduplication;

		protected abstract bool EndableDeduplicationFinderCheckpoint { get; }

		protected virtual bool IsBizoDirty(TMasterBizO obj)
		{
			return false;
		}

		public IReadOnlyList<ScoringResult> FindPotentialDuplicates(bool isTimeoutRequired)
		{
			var result = Enumerable.Empty<ScoringResult>();
			if (ShouldFindDuplications)
			{
				if (isTimeoutRequired)
				{
					var timeoutExecution = new DeduplicationTimeout<IEnumerable<ScoringResult>>(TimeSpan.FromSeconds(DuplicateDetectionTimeout));
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							result = DuplicationScoringResults = timeoutExecution.DoWork(FindPotentialDuplicatesCore);
						}
					}
					catch (TimeoutException)
					{
						ShouldStopProcessing = true;
						result = ScoringResults;
						((ISupportDuplicationFinder)this).LastRunStatus = DuplicationStatus.Timeout;

						TokenSource.Cancel();

						DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName,
						new List<DeduplicationDebuggerObjects>
						{
							new DeduplicationDebuggerObjects
							{
								Message = (NoResString)"The operation timeout during execution",
								HumanReadableBizName = bizo.HumanReadableName
							}
						}, nameof(FindPotentialDuplicates), TimeSpan.FromSeconds(DuplicateDetectionTimeout), MasterGlow);
					}
				}
				else
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = FindPotentialDuplicatesCore();
					}
				}
			}
			return result.ToList().AsReadOnly();
		}

		public IEnumerable<ScoringResult> DuplicationScoringResults { get; private set; }

		async Task FindPotentialDuplicatesAsync(bool shouldShowDuplication = true)
		{
			var duplications = DuplicationScoringResults = await GetDuplicationsAsync();

			if (shouldShowDuplication)
			{
				ShowDuplications(duplications);
			}
		}

		#region IDuplicationFinder Members

		protected DeduplicationResponseStatus AddIgnoreAction(TTargetBizO target, ZString staffCode, UserIgnoreStatus ignoreStatus, ScoringResult score)
		{
			var factory = new BusinessObjectFactory();
			var queryIgnore1 = GetBasicZQueryForIgnores(bizo, target)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new[] { UserIgnoreStatus.TemporaryIgnore.Name, UserIgnoreStatus.PermanentIgnore.Name });
			var queryIgnore2 = GetBasicZQueryForIgnores(target, bizo)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, new[] { UserIgnoreStatus.TemporaryIgnore.Name, UserIgnoreStatus.PermanentIgnore.Name });

			var patternMatchingResults = factory.Load<PatternMatchingResult>(queryIgnore1.AddToFilter(queryIgnore2, JoinCondition.Or));

			var message = DuplicationResponseMessages.Success;
			if (patternMatchingResults == null)
			{
				message = CreateNewPatternMatchingResult(target.PK, staffCode, ignoreStatus, score.Score, factory);
			}
			else
			{
				var patternMatchingResult = patternMatchingResults.Where(pmt => pmt.PMT_Status == UserIgnoreStatus.TemporaryIgnore.Name && pmt.PMT_GS_NKExcludeBy == staffCode || pmt.PMT_Status == UserIgnoreStatus.PermanentIgnore.Name).FirstOrDefault();
				if (patternMatchingResult == null)
				{
					message = CreateNewPatternMatchingResult(target.PK, staffCode, ignoreStatus, score.Score, factory);
				}
				else if (patternMatchingResult.PMT_Status == UserIgnoreStatus.TemporaryIgnore.Name && patternMatchingResult.PMT_GS_NKExcludeBy == staffCode && UserIgnoreStatus.PermanentIgnore.Name != ignoreStatus.Name)
				{
					message = DuplicationResponseMessages.TIGExisted;
				}
				else if (patternMatchingResult.PMT_Status == UserIgnoreStatus.PermanentIgnore.Name)
				{
					message = DuplicationResponseMessages.PIGExisted;
				}
				else
				{
					patternMatchingResult.PMT_Status = ignoreStatus.Name;
					factory.Save();
				}
			}

			return GetDeduplicationResponseStatus(score, message);
		}

		protected DeduplicationResponseStatus GetDeduplicationResponseStatus(ScoringResult score, ZString message)
		{
			return new DeduplicationResponseStatus
			{
				ScoringResult = score,
				Message = message,
				IgnoreStatus = ComputeIgnoreStatus(message)
			};
		}

		MultilingualString CreateNewPatternMatchingResult(ZGuid targetPK, ZString staffCode, UserIgnoreStatus ignoreStatus, double score, BusinessObjectFactory factory)
		{
			var message = DuplicationResponseMessages.Success;
			var newResult = factory.New<PatternMatchingResult>();
			newResult.PMT_MasterPK = bizo.PK;
			newResult.PMT_MasterTableCode = bizo.TablePrefix;
			newResult.PMT_TargetPK = targetPK;
			newResult.PMT_TargetTableCode = targetPK == ZGuid.Empty ? "" : bizo.TablePrefix;
			newResult.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			newResult.PMT_Status = ignoreStatus.Name;
			newResult.PMT_GS_NKExcludeBy = staffCode;
			newResult.PMT_ScorePercent = (ZByte)(score * 100);

			ZExceptionReporting.ProcessWithSaveExceptionHandling(
				() => { factory.Save(); },
				() => { message = DuplicationResponseMessages.Exception; });

			return message;
		}

		protected DeduplicationResponseStatus RemoveTemporaryIgnoresAction(TTargetBizO target)
		{
			var delIgnoresQuery1 = GetBasicZQueryForIgnores(bizo, target)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, UserIgnoreStatus.TemporaryIgnore.Name)
				.AddToFilter(PatternMatchingResultSchema.PMT_GS_NKExcludeBy, GlbStaff.CurrentUser.GS_Code);

			var delIgnoresQuery2 = GetBasicZQueryForIgnores(target, bizo)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, UserIgnoreStatus.TemporaryIgnore.Name)
				.AddToFilter(PatternMatchingResultSchema.PMT_GS_NKExcludeBy, GlbStaff.CurrentUser.GS_Code);

			return RemoveIgnoreAction(delIgnoresQuery1, delIgnoresQuery2);
		}

		protected DeduplicationResponseStatus RemovePermanentIgnoreAction(TTargetBizO targetBizO)
		{
			var permanentsQuery = GetBasicZQueryForIgnores(bizo, targetBizO)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);
			var permanentsQueryViceVersa = GetBasicZQueryForIgnores(targetBizO, bizo)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PermanentIgnore);

			return RemoveIgnoreAction(permanentsQuery, permanentsQueryViceVersa);
		}

		ZQuery GetBasicZQueryForIgnores(BusinessObject master, BusinessObject target)
		{
			return new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, master.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, target.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_TargetTableCode, bizo.TablePrefix);
		}

		DeduplicationResponseStatus RemoveIgnoreAction(ZQuery query, ZQuery queryViceVersa)
		{
			var factory = bizo.Factory;

			var patternMatchingResult = factory.Load<PatternMatchingResult>(query.AddToFilter(queryViceVersa, JoinCondition.Or));

			if (patternMatchingResult == null || !patternMatchingResult.Any())
			{
				return GetDeduplicationResponseStatus(null, DuplicationResponseMessages.NoIgnoranceExisted);
			}
			else
			{
				return RemoveIgnores(factory, patternMatchingResult);
			}
		}

		DeduplicationResponseStatus RemoveIgnores(BusinessObjectFactory factory, PatternMatchingResult[] patternMatchingResult)
		{
			foreach (var result in patternMatchingResult)
			{
				result.Delete();
			}

			factory.Save();
			return GetDeduplicationResponseStatus(null, DuplicationResponseMessages.Success);
		}

		protected DeduplicationResponseStatus AddExclude(ZString staffCode)
		{
			var factory = bizo.Factory;
			var patternMatchingResult = GetPatternMatchingExclusion(factory);
			var messgae = DuplicationResponseMessages.Exclusion;

			if (patternMatchingResult == null)
			{
				var existingResults = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, bizo.PK));
				foreach (var result in existingResults)
				{
					result.Delete();
				}

				messgae = CreateNewPatternMatchingResult(ZGuid.Empty, staffCode, UserIgnoreStatus.ExcludedIgnore, 0, factory);
			}

			return GetDeduplicationResponseStatus(null, messgae);
		}

		protected DeduplicationResponseStatus RemoveExclude()
		{
			var factory = bizo.Factory;
			var patternMatchingResult = GetPatternMatchingExclusion(factory);

			if (patternMatchingResult == null)
			{
				return GetDeduplicationResponseStatus(null, DuplicationResponseMessages.NoExclusionExisted);
			}
			else
			{
				patternMatchingResult.Delete();
				factory.Save();

				return GetDeduplicationResponseStatus(null, DuplicationResponseMessages.Success);
			}
		}

		PatternMatchingResult GetPatternMatchingExclusion(BusinessObjectFactory factory)
		{
			var query = new ZQuery()
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterPK, bizo.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, bizo.TablePrefix)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, UserIgnoreStatus.ExcludedIgnore.Name);

			return factory.LoadTop1<PatternMatchingResult>(query);
		}

		DeduplicationResponseStatus IDuplicationFinder<TMasterBizO, TTargetBizO>.CompareBizOs(TTargetBizO targetBizO, ZString staffCode)
		{
			var target = ConvertTargetToGlowModel(targetBizO);
			var message = ComputeResponseMessage(staffCode, targetBizO.PK);

			return GetResponseWithTimeout(TimeSpan.FromSeconds(DuplicateDetectionTimeout), () =>
			{
				var result = ScoreGlowModel(MasterGlow, target);
				return GetDeduplicationResponseStatus(result, message);
			});
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		async Task<IEnumerable<DeduplicationResponseStatus>> GetPotentialDuplicatesAsync(bool shouldInvokeDeduplicationEvents, ZString staffCode)
		{
			var resultTargets = new List<DeduplicationResponseStatus>();

			if (IsBizoDirty(bizo))
			{
				resultTargets.Add(GetDeduplicationResponseStatus(null, DuplicationResponseMessages.MasterIsDirty));
			}
			else if (ShouldFindDuplications)
			{
				await FindPotentialDuplicatesAsync(shouldInvokeDeduplicationEvents);

				foreach (var result in DuplicationScoringResults)
				{
					var message = ComputeResponseMessage(staffCode, result.TargetPK);
					if (message == DuplicationResponseMessages.Success)
					{
						resultTargets.Add(GetDeduplicationResponseStatus(result, message));
					}
				}
			}

			return resultTargets;
		}

		IEnumerable<DeduplicationResponseStatus> IDuplicationFinder<TMasterBizO, TTargetBizO>.GetPotentialTargets(ZString staffCode)
		{
			var matchingResponses = new List<DeduplicationResponseStatus>();

			if (IsBizoDirty(bizo))
			{
				return new List<DeduplicationResponseStatus>
				{
					GetDeduplicationResponseStatus(null, DuplicationResponseMessages.MasterIsDirty)
				};
			}
			else if (ShouldFindDuplications)
			{
				var potentialDuplicates = FindPotentialDuplicates(true).ToList();

				foreach (var result in potentialDuplicates)
				{
					var message = ComputeResponseMessage(staffCode, result.TargetPK);
					if (message == DuplicationResponseMessages.Success)
					{
						matchingResponses.Add(GetDeduplicationResponseStatus(result, message));
					}
				}
			}

			return matchingResponses;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		async Task<IEnumerable<DeduplicationResponseStatus>> IDuplicationFinder<TMasterBizO, TTargetBizO>.GetPotentialDuplicatesAsync(ZString staffCode)
		{
			return await GetPotentialDuplicatesAsync(true, staffCode);
		}

		DeduplicationResponseStatus IDuplicationFinder<TMasterBizO, TTargetBizO>.AddExclusion(ZString staffCode)
		{
			return AddExclude(staffCode);
		}

		DeduplicationResponseStatus IDuplicationFinder<TMasterBizO, TTargetBizO>.AddIgnore(TTargetBizO targetBizO, UserIgnoreStatus ignoreStatus, ZString staffCode)
		{
			var target = ConvertTargetToGlowModel(targetBizO);
			var score = ScoreGlowModel(MasterGlow, target);
			return AddIgnoreAction(targetBizO, staffCode, ignoreStatus, score);
		}

		DeduplicationResponseStatus IDuplicationFinder<TMasterBizO, TTargetBizO>.RemoveExclusion()
		{
			return RemoveExclude();
		}

		DeduplicationResponseStatus IDuplicationFinder<TMasterBizO, TTargetBizO>.RemoveTemporaryIgnore(TTargetBizO targetBizO)
		{
			return RemoveTemporaryIgnoresAction(targetBizO);
		}

		DeduplicationResponseStatus IDuplicationFinder<TMasterBizO, TTargetBizO>.RemovePermanentIgnore(TTargetBizO targetBizO)
		{
			return RemovePermanentIgnoreAction(targetBizO);
		}

		#endregion

		public Type TargetType { get { return typeof(TTargetBizO); } }

		public List<ScoringResult> ScoringResults { get; }

		Task ISupportDuplicationFinder.FindDuplicates()
		{
			var task = GetPotentialDuplicatesAsync(true, GlbStaff.CurrentUser.GS_Code);
			task.ConfigureAwait(false);
			return task;
		}

		void ISupportDuplicationFinder.RequestToCancel()
		{
			TokenSource.Cancel();
		}
	}
}
