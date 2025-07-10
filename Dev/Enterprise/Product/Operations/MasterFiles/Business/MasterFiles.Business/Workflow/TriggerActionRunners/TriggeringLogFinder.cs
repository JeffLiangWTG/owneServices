using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class TriggeringLogFinder
	{
		public static bool TryFindLog(WorkflowTriggerEventData wteData, IBaseTrigger trigger, BusinessObject workflowParent, out StmALog result, bool findCancelled = false, IQueuedLog wteFallbackLog = null)
		{
			result = null;

			if (trigger != null
				&& workflowParent != null
				&& wteData != null)
			{
				if (wteData.TriggeringLogPK == ZGuid.Empty && wteFallbackLog != null)
				{
					result = RecreateAuditLogFromWTEEvent(wteFallbackLog, trigger, workflowParent);
				}
				else if (wteData.TriggeringLogPK.IsValid)
				{
					var triggeringLogParentPK = wteData.TriggeringLogParentPK.IsValid ? wteData.TriggeringLogParentPK : trigger.ParentID;
					result = GetLogFromBusinessObject(trigger, workflowParent, wteData.TriggeringLogPK, triggeringLogParentPK, findCancelled);
				}
			}
			return result != null;
		}

		public static bool IsLogVisibleToCurrentUser(IStmALogParent workflowParent, ZGuid logParentPK)
		{
			return workflowParent.LogsParentPK == logParentPK
				|| workflowParent.BusinessObjectsWithRelatedEvents?.FirstOrDefault(bo => bo?.PK == logParentPK) != null;
		}

		static StmALog GetLogFromBusinessObject(IBaseTrigger trigger, BusinessObject workflowParent, ZGuid triggeringLogPK, ZGuid logParentPK, bool findCancelled)
		{
			/*	Cases:
				1. find the log on the workflow parent or its BusinessObjectsWithRelatedEvents
				2. trigger is IExternalReferencingTrigger -> use ReferencedID to find the log
				3. logParentPK is valid and we still have not found the log,
					this means the log belongs to a business object the current user cannot access e.g. a JobHeader from another company
					-> use the logParentPk to find the log, this causes a DB hit
				5. if we still haven't found the log, look on all BusinessObjectsWithRelatedEvents, this causes 1 or more DB hits
			*/

			ZQuery GetBaseQuery()
			{
				var baseQuery = new ZQuery();
				baseQuery.AddToFilter(StmALogSchema.PK, triggeringLogPK);
				return baseQuery;
			}

			var query = GetBaseQuery();
			StmALog result = null;
			var parent = (IStmALogParent)workflowParent;

			if (logParentPK == workflowParent.PK)
			{
				result = workflowParent.GetFirstMatchingLog(query);
			}
			else
			{
				var logParent = parent.BusinessObjectsWithRelatedEvents?.FirstOrDefault(bo => bo?.PK == logParentPK);
				result = logParent?.GetFirstMatchingLog(query);
			}

			if (result == null && trigger is IExternalReferencingTrigger referencingTrigger && referencingTrigger.ReferencedID.IsValid)
			{
				var logParent = parent.BusinessObjectsWithRelatedEvents?.FirstOrDefault(bo => bo?.PK == referencingTrigger.ReferencedID);
				result = logParent?.GetFirstMatchingLog(query);
			}

			if (result == null && WorkflowDataRegistry.Instance.ShouldFindLogsOnRelatedJobsDuringLWKProcessing.Value)
			{
				var logQuery = GetBaseQuery();
				logQuery.AddToFilter(StmALogSchema.SL_Parent, logParentPK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, trigger.TriggerEventCode);
				result = parent.Factory.LoadTop1<StmALog>(logQuery);
			}

			if (result != null && !findCancelled && result.IsCancelled)
			{
				return null;
			}

			return result;
		}

		static StmALogAddedToQueueOnly RecreateAuditLogFromWTEEvent(IQueuedLog log, IBaseTrigger trigger, BusinessObject workflowParent)
		{
			var nonPersistedLog = log.Factory.New<StmALogAddedToQueueOnly>();
			using (((IUpdateFieldsLockReachAround)nonPersistedLog).LockForUpdatingKeyFields(false))
			{
				nonPersistedLog.SL_EventTime = log.SJ_EventTime;
				nonPersistedLog.SL_EventTimeUtc = log.SJ_EventTimeUtc;
				nonPersistedLog.SL_EventTimeOffset = log.EventTimeOffset;
				nonPersistedLog.SL_Parent = workflowParent.PK;
				nonPersistedLog.SL_GS_NKUser = log.SJ_GS_NKUser;
				nonPersistedLog.SL_SE_NKEvent = trigger.TriggerEventCode;
				nonPersistedLog.SL_Table = workflowParent.TableName;
				nonPersistedLog.SL_IsEstimate = log.SJ_IsEstimate;
				nonPersistedLog.SL_GB_NKBranch = log.SJ_GB_NKBranch;
				nonPersistedLog.SL_GE_NKDepartment = log.SJ_GE_NKDepartment;
			}

			return nonPersistedLog;
		}
	}
}
