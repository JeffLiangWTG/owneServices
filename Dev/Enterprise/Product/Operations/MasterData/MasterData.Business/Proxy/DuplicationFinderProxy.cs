using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public abstract class DuplicationFinderProxy<TMaster, TTarget> : IDuplicationFinder<TMaster, TTarget>
		where TMaster : BusinessObject, IDeduplicatable
		where TTarget : BusinessObject, IDeduplicatable
	{
		readonly IDuplicationFinder<TMaster, TTarget> duplicationFinder;

		public Type TargetType => typeof(TTarget);

		public bool IsProxied { get; set; }

		public bool ShouldStopProcessing
		{
			get
			{
				return duplicationFinder.ShouldStopProcessing;
			}
			set
			{
				duplicationFinder.ShouldStopProcessing = value;
			}
		}

		DuplicationFinderProxy()
		{
		}

		protected DuplicationFinderProxy(TMaster masterBizO)
			: this(masterBizO, new DeduplicationProxyConfig())
		{
		}

		protected DuplicationFinderProxy(TMaster masterBizO, DeduplicationProxyConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			if (masterBizO == null)
			{
				throw new ArgumentNullException(nameof(masterBizO));
			}

			duplicationFinder = (IDuplicationFinder<TMaster, TTarget>)masterBizO.GetSupportedDuplicationFinders(typeof(TTarget), config).FirstOrDefault(t => t.TargetType == typeof(TTarget));
		}

		public DeduplicationResponseStatus CompareBizOs(TTarget targetBizO, ZString staffCode)
		{
			return duplicationFinder?.CompareBizOs(targetBizO, staffCode);
		}

		public IEnumerable<DeduplicationResponseStatus> GetPotentialTargets(ZString staffCode)
		{
			return duplicationFinder?.GetPotentialTargets(staffCode);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		public async Task<IEnumerable<DeduplicationResponseStatus>> GetPotentialDuplicatesAsync(ZString staffCode)
		{
			return await duplicationFinder?.GetPotentialDuplicatesAsync(staffCode);
		}

		public DeduplicationResponseStatus AddIgnore(TTarget targetBizO, UserIgnoreStatus ignoreStatus, ZString staffCode)
		{
			return duplicationFinder?.AddIgnore(targetBizO, ignoreStatus, staffCode);
		}

		public DeduplicationResponseStatus AddExclusion(ZString staffCode)
		{
			return duplicationFinder?.AddExclusion(staffCode);
		}

		public DeduplicationResponseStatus RemoveExclusion()
		{
			return duplicationFinder?.RemoveExclusion();
		}

		public DeduplicationResponseStatus RemoveTemporaryIgnore(TTarget targetBizO)
		{
			return duplicationFinder?.RemoveTemporaryIgnore(targetBizO);
		}

		public DeduplicationResponseStatus RemovePermanentIgnore(TTarget targetBizO)
		{
			return duplicationFinder?.RemovePermanentIgnore(targetBizO);
		}

		public Task FindDuplicates()
		{
			return duplicationFinder.FindDuplicates();
		}

		public void RequestToCancel()
		{
			duplicationFinder.RequestToCancel();
		}

		public List<ScoringResult> ScoringResults => duplicationFinder?.ScoringResults;

		public CancellationTokenSource TokenSource => duplicationFinder?.TokenSource;

		DuplicationStatus ISupportDuplicationFinder.LastRunStatus { get; set; }
	}
}
