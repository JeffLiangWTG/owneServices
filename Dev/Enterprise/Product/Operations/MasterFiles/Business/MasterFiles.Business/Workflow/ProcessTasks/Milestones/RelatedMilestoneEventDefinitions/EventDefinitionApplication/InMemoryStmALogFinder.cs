using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	static class InMemoryStmALogFinder
	{
		public static StmALog Find(ProcessTask task, RelatedMilestoneEventDefinition definition, string oldTriggerConditionValue)
		{
			switch (definition)
			{
				case RelatedMilestoneActualEventDefinition _:
					var @event = Events.All[definition.EventType];
					var log = task.ParentBusinessObject.GetLogs().MostRecentLogByEventTime(@event, WorkflowDefaultDateProvider.GetQueryForDefaults(task, false, oldTriggerConditionValue));
					return log != null && log.IsInDatabase ? null : log;

				case RelatedMilestoneIsEstimateEvent c2:
					return LogFinderUtils.FindOldEstimatedEvents(task, c2, isInDatabase: false, isCancelled: false, checkTaskPk: false, oldTriggerConditionValue: oldTriggerConditionValue, additionalFilter: new ZQuery() { OrderBy = StmALogSchema.SL_EventTime.Name + " DESC", MaximumRows = 1 }).SingleOrDefault();

				case RelatedMilestoneESTEventDefinition c3:
					var additionalFilter = new ZQuery() { OrderBy = StmALogSchema.SL_EventTime.Name + " DESC", MaximumRows = 1 };
					return LogFinderUtils.FindOldEstimatedEvents(task, c3, isInDatabase: false, isCancelled: false, checkTaskPk: false, oldTriggerConditionValue: oldTriggerConditionValue, additionalFilter: additionalFilter).SingleOrDefault();

				default:
					throw new ArgumentException("Unhandled", nameof(definition));
			}
		}
	}
}
