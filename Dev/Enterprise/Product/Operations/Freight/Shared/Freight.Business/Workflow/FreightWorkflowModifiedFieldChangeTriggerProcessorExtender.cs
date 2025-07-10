using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	abstract class FreightWorkflowModifiedFieldChangeTriggerProcessorExtender : IWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		IWorkflowTrigger[] IWorkflowModifiedFieldChangeTriggerProcessorExtender.GetTriggersToRun(StmChangeLog changeLog, string[] changedPropertyNames)
		{
			return GetTriggersToRunForSailingChangeLog(changeLog, changedPropertyNames);
		}

		protected abstract List<string> GetSqls();

		ProcessTask[] GetTriggersToRunForSailingChangeLog(StmChangeLog changeLog, string[] changedPropertyNames)
		{
			changedPropertyNames = ExcludeEmptyProperties(changeLog, changedPropertyNames);
			var sqls = GetSqls().Select(subsql => string.Format(CultureInfo.InvariantCulture, ProcessTask.Schema.P9_TriggerField + @" IN (SELECT Value FROM @ChangedPropertyNames) AND P9_Type IN('TRG', 'MIL') AND P9_ParentId IN ({0})", subsql));
			var parameters = new ZSqlParameterCollection()
			{
				ZSqlParameter.New("@ParentID", changeLog.SY_ParentID.ToGuid(), StmChangeLogSchema.SY_ParentID),
				ZSqlParameter.New("@ChangedPropertyNames", changedPropertyNames, ProcessTasksSchema.P9_TriggerField, isTableValued: true)
			};

			var result = new HashSet<ProcessTask>(BusinessObjectEqualityComparer<ProcessTask>.PKOnlyComparer);
			foreach (var sql in sqls)
			{
				var query = new ZDBOnlyQuery(typeof(ProcessTask));
				query.AddFilterAndZSQLParameterCollection(sql, parameters);

				foreach (var trigger in changeLog.Factory.Load<ProcessTask>(query))
				{
					if (trigger.P9_ReferencedID.IsEmpty || MatchesSailingOrTransportChangeLog(changeLog, trigger))
					{
						result.Add(trigger);
					}
				}
			}
			return result.ToArray();
		}

#if DEBUG
		internal
#endif
		string[] ExcludeEmptyProperties(StmChangeLog changeLog, string[] changedPropertyNames)
		{
			var fieldChanges = changeLog.FieldChanges.Cast<StmFieldChangeLog>();
			return changedPropertyNames.Where(x => fieldChanges.All(y => y.PropertyName != x)
					|| !ShouldBeExcludedForEmptyProperty(x)
					|| fieldChanges.Any(y => y.PropertyName == x && (!y.OldValue?.IsEmpty ?? false))).ToArray();
		}

		bool ShouldBeExcludedForEmptyProperty(string propertyName)
		{
			return propertyName == Transport.Schema.JW_ETD
				|| propertyName == Transport.Schema.JW_ETA
				|| propertyName == Transport.Schema.JW_VoyageFlight
				|| propertyName == Transport.Schema.JW_Vessel;
		}

		bool MatchesSailingOrTransportChangeLog(StmChangeLog changeLog, ProcessTask trigger)
		{
			var routingWorkflowItem = trigger as RoutingSupportProcessTask;

			return routingWorkflowItem != null &&
				routingWorkflowItem.Transport != null &&
				MatchesSailingOrTransportChangeLog(changeLog, routingWorkflowItem.Transport);
		}

		bool MatchesSailingOrTransportChangeLog(StmChangeLog changeLog, Transport transport)
		{
			bool result = changeLog.SY_ParentID == transport.PK;
			if (!result && transport.Sailing != null)
			{
				result =
					changeLog.SY_ParentID == transport.Sailing.PK ||
					changeLog.SY_ParentID == transport.Sailing.JX_JA ||
					changeLog.SY_ParentID == transport.Sailing.JX_JB;
			}

			return result;
		}
	}
}
