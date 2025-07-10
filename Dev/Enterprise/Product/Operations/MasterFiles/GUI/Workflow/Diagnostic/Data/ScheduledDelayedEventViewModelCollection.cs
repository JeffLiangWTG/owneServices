using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class ScheduledDelayedEventViewModelCollection : NonPersistentBusinessObjectCollection<ScheduledDelayedEventViewModel>
	{
		public ScheduledDelayedEventViewModelCollection(ProcessTask task)
		{
			this.task = task;
		}

		#region NonPersistentBusinessObjectCollection Overrides

		public override void Load()
		{
			foreach (var schedule in GetSchedules())
			{
				Add(new ScheduledDelayedEventViewModel(schedule));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		#endregion

		#region Implementation

		readonly ProcessTask task;

		BusinessObject Job => task?.ParentBusinessObject;

		IEnumerable<IActionSchedule> GetSchedules()
		{
			if (task == null)
			{
				return Enumerable.Empty<IActionSchedule>();
			}

			var query = new ZQuery(TimeActionScheduleSchema.TAS_TargetPK, Job.PK)
				.AddToFilter(TimeActionScheduleSchema.TAS_TargetTableCode, Job.TablePrefix)
				.AddToFilter(TimeActionScheduleSchema.TAS_ActionCode, WorkflowDelayedEventProcessor.Code);

			if (!task.IsNonPersistedRepresentationOfTemplateTrigger)
			{
				var taskQuery = new ZQuery(TimeActionScheduleSchema.TAS_TargetPK, task.PK)
					.AddToFilter(TimeActionScheduleSchema.TAS_TargetTableCode, task.TablePrefix)
					.AddToFilter(TimeActionScheduleSchema.TAS_ActionCode, WorkflowDelayedTriggerProcessor.Code);
				query.AddToFilter(taskQuery, JoinCondition.Or);
			}
			else
			{
				var triggerLinkQuery = new ZQuery(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, task.P9_ParentTemplateID);
				triggerLinkQuery.AddToFilter(ProcessJobTriggerLinkSchema.P9L_ParentId, task.P9_ParentID);
				var triggerLinks = task.Factory.Load<IProcessJobTriggerLink>(triggerLinkQuery);

				if (triggerLinks.Length > 0)
				{
					var triggerLinkTaskQuery = new ZQuery(TimeActionScheduleSchema.TAS_TargetTableCode, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(triggerLinks[0].TableName))
						.AddToFilter(TimeActionScheduleSchema.TAS_ActionCode, WorkflowDelayedTriggerProcessor.Code)
						.AddToFilter(TimeActionScheduleSchema.TAS_TargetPK, triggerLinks.Select(l => l.Identifier));
					query.AddToFilter(triggerLinkTaskQuery, JoinCondition.Or);
				}
			}

			query.OrderBy = TimeActionScheduleSchema.Constants.TAS_ExecutionDateTimeUtc + OrderByClause.Descending;

			var schedules = task.Factory.Load<IActionSchedule>(query);
			return schedules.Cast<IActionSchedule>();
		}

		#endregion
	}
}
