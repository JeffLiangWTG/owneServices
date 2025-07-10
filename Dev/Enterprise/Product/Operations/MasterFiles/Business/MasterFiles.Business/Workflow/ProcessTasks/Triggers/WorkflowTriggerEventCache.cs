using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This cache should not contain any WTE logs that have been written to the database.
	///
	/// Note the CacheStalenessPolicy in GetCache.
	/// </summary>
	class WorkflowTriggerEventCache
	{
		#region Public API

		public static bool HasWTELog(IWorkflowTrigger trigger, EventSource eventSource)
		{
			return GetCache(trigger.Factory).TryGet(trigger, eventSource, out StmALog _);
		}

		public static (StmALog deletedLog, EventSource nextLog) DeleteWTELog(IWorkflowTrigger trigger, EventSource eventSource)
		{
			return GetCache(trigger.Factory).Delete(trigger, eventSource);
		}

		public static bool TryAddWTELog(IWorkflowTrigger trigger, EventSource eventSource, StmALog log)
		{
			return GetCache(trigger.Factory).Add(trigger, eventSource, log);
		}

		public static int ClearExistingWTELogs(IWorkflowTrigger trigger)
		{
			return GetCache(trigger.Factory).Clear(trigger);
		}

		public static void ClearTopExistingWTELogsByEventTime(IWorkflowTrigger trigger, int numLogs)
		{
			GetCache(trigger.Factory).ClearTopByEventTime(trigger, numLogs);
		}

		public static void UpdateWTEEventTime(IWorkflowTrigger trigger, EventSource eventSource)
		{
			if (GetCache(trigger.Factory).TryGet(trigger, eventSource, out StmALog wteLog))
			{
				using (((IUpdateFieldsLockReachAround)wteLog).LockForUpdatingKeyFields(false))
				{
					wteLog.SL_EventTimeOffset = eventSource.EventTime;
				}
			}
		}

		public static bool WasTriggerFiredByLog(BusinessObjectFactory factory, ZGuid triggerPK, ZGuid logPK)
		{
			var cache = GetCache(factory);
			return cache.logCacheByTrigger.TryGetValue(triggerPK, out var wteLogs)
				&& wteLogs.Keys.Any(eventSource => eventSource?.Log?.Identifier == logPK);
		}

		#endregion

		#region Implementation

		static internal WorkflowTriggerEventCache GetCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(WorkflowTriggerEventCache), () => new WorkflowTriggerEventCache(), CacheStalenessPolicy.StaleOnFactorySave);
		}

		readonly Dictionary<ZGuid, Dictionary<EventSource, StmALog>> logCacheByTrigger = new Dictionary<ZGuid, Dictionary<EventSource, StmALog>>();

		internal bool Add(IWorkflowTrigger trigger, EventSource eventSource, StmALog log)
		{
			if (!logCacheByTrigger.TryGetValue(trigger.Identifier, out var wteByLog))
			{
				logCacheByTrigger[trigger.Identifier] = wteByLog = new Dictionary<EventSource, StmALog>(new EventSourceComparer());
			}

			if (!wteByLog.TryGetValue(eventSource, out var existingWteLog) || existingWteLog == null)
			{
				wteByLog[eventSource] = log;
				return true;
			}

			return false;
		}

		internal bool TryGet(IWorkflowTrigger trigger, EventSource eventSource, out StmALog result)
		{
			result = null;
			return logCacheByTrigger.TryGetValue(trigger.Identifier, out var wteByLog) && wteByLog.TryGetValue(eventSource, out result) && result != null;
		}

		(StmALog deletedLog, EventSource nextLog) Delete(IWorkflowTrigger trigger, EventSource eventSource)
		{
			if (logCacheByTrigger.TryGetValue(trigger.Identifier, out var wteByLog) && wteByLog.TryGetValue(eventSource, out var result))
			{
				wteByLog.Remove(eventSource);
				if (result != null && !result.IsInDatabase)
				{
					result.Delete();
				}

				var nextLog = wteByLog.Keys.MaxBySafe(f => f.EventTime);
				return (result, nextLog);
			}
			return (null, null);
		}

		int Clear(IWorkflowTrigger trigger)
		{
			var clearedLogs = 0;
			if (logCacheByTrigger.TryGetValue(trigger.Identifier, out var wteByLog))
			{
				var logs = wteByLog.ToList();
				foreach (var pair in logs)
				{
					Delete(trigger, pair.Key);
				}
				clearedLogs = logs.Count;
			}
			return clearedLogs;
		}

		void ClearTopByEventTime(IWorkflowTrigger trigger, int numLogs)
		{
			if (logCacheByTrigger.TryGetValue(trigger.Identifier, out var wteByLog))
			{
				var logsToDelete = wteByLog.Keys.OrderByDescending(f => f.EventTime).Take(numLogs).ToList();
				foreach (var key in logsToDelete)
				{
					Delete(trigger, key);
				}
			}
		}

		#endregion

		class EventSourceComparer : IEqualityComparer<IEventSource>
		{
			public bool Equals(IEventSource x, IEventSource y) => x.Identifier.Equals(y.Identifier);
			public int GetHashCode(IEventSource obj) => obj.Identifier.GetHashCode();
		}
	}
}
