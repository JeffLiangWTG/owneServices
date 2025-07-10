using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	class SetFieldTriggerResults : ITriggerActionTracker
	{
		readonly List<SetFieldActionResult> TriggerActionResults;

		internal SetFieldTriggerResults()
		{
			TriggerActionResults = new List<SetFieldActionResult>();
		}

		internal static void AddResult(IBusiness trigger, WorkflowSetFieldProcessor workflowSetFieldProcessor, INotifications notifications, BusinessObject parent, IReadOnlyCollection<IWorkflowSetFieldResult> results)
		{
			var setFieldResults = WorkflowTriggerActionTracker.GetOrCreateTracker(trigger.Factory, () => new SetFieldTriggerResults());
			setFieldResults.AddResultCore(workflowSetFieldProcessor, parent, notifications, results);
		}

		void AddResultCore(WorkflowSetFieldProcessor workflowSetFieldProcessor, BusinessObject parent, INotifications notifications, IReadOnlyCollection<IWorkflowSetFieldResult> results)
		{
			TriggerActionResults.Add(new SetFieldActionResult(workflowSetFieldProcessor, parent, notifications, results));
		}

		void ITriggerActionTracker.OnAllActionsRun()
		{
			bool validateAgain;
			var rolledBackResults = new HashSet<object>();
			do
			{
				validateAgain = false;
				foreach (var triggerActionResult in TriggerActionResults)
				{
					foreach (var result in triggerActionResult.Results)
					{
						if (!rolledBackResults.Contains(result) && !result.Validate())
						{
							rolledBackResults.Add(result);
							validateAgain = true;
							result.Rollback();
						}
					}
				}
			}
			while (validateAgain);

			foreach (var result in TriggerActionResults)
			{
				result.LogNotification();
			}
		}

		internal bool IsSettingProperty(ZPropertyInfo info)
		{
			foreach (var triggerActionResult in TriggerActionResults)
			{
				foreach (var result in triggerActionResult.Results)
				{
					if (result.PropertyInfo?.Equals(info) ?? false)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	class SetFieldActionResult
	{
		readonly BusinessObject Parent;
		readonly WorkflowSetFieldProcessor WorkflowSetFieldProcessor;
		readonly INotifications Notifications;

		internal SetFieldActionResult(WorkflowSetFieldProcessor workflowSetFieldProcessor, BusinessObject parent, INotifications notifications, IReadOnlyCollection<IWorkflowSetFieldResult> results)
		{
			WorkflowSetFieldProcessor = workflowSetFieldProcessor;
			Results = results;
			Notifications = notifications;
			Parent = parent;
		}

		internal IReadOnlyCollection<IWorkflowSetFieldResult> Results { get; }

		internal void LogNotification() => WorkflowSetFieldProcessor.LogNotifcations(Notifications, Parent, Results);
	}
}
