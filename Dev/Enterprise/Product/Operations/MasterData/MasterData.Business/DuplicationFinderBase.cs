using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "Requires more than two generic parameters")]
	public abstract class DuplicationFinderBase<TMasterBizO, TTargetBizO, TGlow> : IDisposable
		where TMasterBizO : BusinessObject, IDeduplicatable
		where TTargetBizO : BusinessObject, IDeduplicatable
	{
		protected DuplicationFinderBase()
		{
			TokenSource = new CancellationTokenSource();
		}

		public CancellationTokenSource TokenSource { get; }

		public bool ShouldStopProcessing { get; set; }

		protected IEnumerable<PatternMatchingResultModel> results;

		protected virtual int MaxScoringResult => 4;

		protected abstract TGlow MasterGlow { get; }

		protected TaskScheduler Scheduler => ObjectFactory.Get<TaskScheduler>();

		protected abstract ITargetFinderController CreateTargetFinderController(TGlow glowModel);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected abstract IEnumerable<IGrouping<Guid, PatternMatchingResultModel>> FindTargetPKsAndResultModels(PatternMatchingResultModel[] patternMatchingResults);

		protected abstract Guid GetGlowPK(TGlow glowModel);

		protected abstract IEnumerable<TMasterBizO> LoadTargetBizOs(IFactory factory, HashSet<Guid> candidatePKs);

		protected abstract void StandardizeMaster();

		protected abstract TGlow ConvertMasterToGlowModel(TMasterBizO targetBizo);

		protected abstract TGlow ConvertTargetToGlowModel(TTargetBizO targetBizo);

		protected virtual IEnumerable<PatternMatchingResultModel> GetPatternMatchingResultModels()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Pattern Matching Result" };
				return FindPotentialTargets(CreateTargetFinderController(MasterGlow), factory);
			}
		}

		IEnumerable<PatternMatchingResultModel> FindPotentialTargets(ITargetFinderController finder, BusinessObjectFactory factory)
		{
			var methodName = nameof(FindPotentialTargets);

			IDbConnected connected = factory;
			IDbConnectionInternals connection = connected.Connection;
			var transaction = connection.InternalDbTransaction;

			using (var timer = new DeduplicationPerformanceMonitor())
			{
				results = finder.GetPotentialTargetInfo(connection.InternalDbConnection, DebuggerParticipant?.DebuggerHub, false, transaction, RepeatedValueLimit);
				if (results != null)
				{
					DebuggerParticipant?.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, results, methodName, timer.ElapsedDuration, MasterGlow);
				}
			}

			return results;
		}

		protected virtual IDeduplicationDebuggerParticipant DebuggerParticipant { get; set; }

		int RepeatedValueLimit => OrganisationsDataRegistry.Instance.RepeatedValueLimit.Value;

		protected IEnumerable<TMasterBizO> GetOrderedList(TMasterBizO[] targetBizos, HashSet<Guid> candidatePKs)
		{
			if (targetBizos == null || targetBizos.Length == 0 || candidatePKs == null)
			{
				return targetBizos;
			}

			var orderedList = new List<TMasterBizO>();
			foreach (var candidatePK in candidatePKs)
			{
				var result = targetBizos.FirstOrDefault(u => u.PK == candidatePK);

				if (result != null)
				{
					orderedList.Add(result);
				}
			}

			return orderedList;
		}

		protected int GetBatchSize(int candidateLength)
		{
			int batchSize;

			if (candidateLength <= 20)
			{
				batchSize = 4;
			}
			else if (candidateLength >= 75)
			{
				batchSize = 15;
			}
			else
			{
				batchSize = (int)Utilities.Round((decimal)candidateLength / 5, 0);
			}

			return batchSize;
		}

		protected List<TGlow> GetTargetGlowBizos(IEnumerable<TMasterBizO> targetBizOs)
		{
			return targetBizOs.Select(ConvertMasterToGlowModel).ToList();
		}

		#region Disposable
		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				TokenSource.Dispose();
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
