using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region Log Finder

	public static class LogFinderUtils
	{
		public static IEnumerable<StmALog> FindOldEstimatedEvents(ProcessTask task, RelatedMilestoneEventDefinition strategy, bool? isInDatabase, bool? isCancelled, bool checkTaskPk, ZString? oldTriggerConditionValue = null, ZQuery additionalFilter = null)
		{
			if (task.Parent is BusinessObject parentBizo)
			{
				var query = additionalFilter ?? new ZQuery();
				query.FetchOnlyFromLocalCache = isInDatabase.HasValue && !isInDatabase.Value;

				var findESTEvents = !strategy.IsEstimate; // Yucky flag should not exist. Inconsistency between strategy pattern and if/else dispatch starts here.

				query.AddToFilter(findESTEvents ? GetEstimateLogsWithReferenceFilter(task) : GetIsEstimateLogsFilter(task, checkTaskPk));
				query.AddToFilter(StmALogSchema.SL_IsEstimate, !findESTEvents);
				query.AddToFilter(StmALogSchema.SL_Parent, task.P9_ParentID);

				if (isCancelled.HasValue)
				{
					query.AddToFilter(StmALogSchema.SL_IsCancelled, isCancelled.Value);
				}
				if (!task.P9_ParentTableCode.IsEmpty)
				{
					query.AddToFilter(StmALogSchema.SL_Table, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(task.P9_ParentTableCode).TableName);
				}

				foreach (var log in task.Factory.Load<StmALog>(query))
				{
					if ((!isInDatabase.HasValue || log.IsInDatabase == isInDatabase.Value) && TriggerConditionEvaluator.AreTriggerConditionsMet(task, log, parentBizo, oldTriggerConditionValue, findExistingLogForRename: findESTEvents))
					{
						yield return log;
					}
				}
			}
		}

		static ZQuery GetIsEstimateLogsFilter(ProcessTask task, bool checkTaskPk)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, task.P9_SE_NKMilestoneEvent);
			if (checkTaskPk)
			{
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, task.PK.ToString());
			}
			return query;
		}

		static ZQuery GetEstimateLogsWithReferenceFilter(ProcessTask task)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.EstimatedDateChangedCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, task.P9_SE_NKMilestoneEvent);
			return query;
		}
	}

	#endregion
}
