using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Here is where we differentiate the different ways that a RelatedMiletoneEventDefinition can be used to
	/// Created, Update or Delete events related to ProcessTasks
	/// </summary>
	abstract class RelatedMilestoneEventDefinitionApplication
	{
		protected RelatedMilestoneEventDefinitionApplication(RelatedMilestoneEventDefinition eventDefinition)
		{
			EventDefinition = eventDefinition;
		}

		internal abstract void Apply(EventValue value);
		internal RelatedMilestoneEventDefinition EventDefinition { get; }
	}

	class UpdateReferenceOfInMemoryLog : RelatedMilestoneEventDefinitionApplication
	{
		public UpdateReferenceOfInMemoryLog(ProcessTask task, RelatedMilestoneEventDefinition eventDefinition, string oldReference)
			: base(eventDefinition)
		{
			this.oldReference = oldReference;
			this.task = task;
		}
		readonly string oldReference;
		readonly ProcessTask task;

		internal override void Apply(EventValue value)
		{
			if (new ZDateTimeOffset(EventDefinition.NewDate).IsValid && EventDefinition.RelatedPropertyHasChanges)
			{
				var logToUpdate = InMemoryStmALogFinder.Find(task, EventDefinition, oldReference);
				if (logToUpdate != null)
				{
					var newReference = EventDefinition.MergeIntoExistingReference(logToUpdate.SL_Reference, value.ReferenceAndParameters);
					if (logToUpdate.SL_Reference != newReference)
					{
						using (((IUpdateFieldsLock)logToUpdate).LockForUpdatingKeyFields())
						{
							logToUpdate.SL_Reference = newReference;
						}
					}
				}
			}
		}
	}

	class UpdateOrCreateOrDeleteEstimatedEvent : RelatedMilestoneEventDefinitionApplication
	{
		public UpdateOrCreateOrDeleteEstimatedEvent(ProcessTask task, RelatedMilestoneEventDefinition eventDefinition)
			: base(eventDefinition)
		{
			this.task = task;
		}
		readonly ProcessTask task;

		internal override void Apply(EventValue value)
		{
			var originalDate = EventDefinition.OriginalDate;
			var newDate = EventDefinition.NewDate;
			if (!new ZDateTimeOffset(originalDate).IsValid && !new ZDateTimeOffset(newDate).IsValid)
			{
				LogFinderUtils.FindOldEstimatedEvents(task, EventDefinition, isInDatabase: false, isCancelled: false, checkTaskPk: false, task.P9_TriggerConditionValue, new ZQuery() { OrderBy = StmALogSchema.SL_EventTime.Name + " DESC", MaximumRows = 1 }).SingleOrDefault()?.Delete();
			}
			else
			{
				foreach (var oldLog in LogFinderUtils.FindOldEstimatedEvents(task, EventDefinition, isInDatabase: true, isCancelled: false, checkTaskPk: false))
				{
					((IStmALogInternals)oldLog).CancelWithoutNotifications(); // No need to update dates on process tasks - they will be updated later when new log is created
				}

				if (task.Parent is BusinessObject bizoParent)
				{
					var existingLog = LogFinderUtils.FindOldEstimatedEvents(task, EventDefinition, isInDatabase: false, isCancelled: false, checkTaskPk: false, task.P9_TriggerConditionValue, new ZQuery() { OrderBy = StmALogSchema.SL_EventTime.Name + " DESC", MaximumRows = 1 }).SingleOrDefault();
					bizoParent.GetLogs().CreateRecreateOrUpdateEventLog(value, existingLog);
				}
			}
		}
	}
}
