using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This class is used to identify race conditions in template applications.
	/// It is considered a race condition if the same template creates the same Task, Milestone, Trigger or Workflow on the same job.
	/// </summary>
	public class TemplateApplicationRaceConditionHandler
	{
		public TemplateApplicationProcessTasksRaceConditionHandler ProcessTasksRaceConditionHandler { get; }
		public ITemplateApplicationRaceConditionHandler WorkflowRaceConditionHandler { get; }

		public TemplateApplicationRaceConditionHandler(BusinessObjectFactory factory, TemplateApplicationRaceHandlingConfig config)
		{
			ProcessTasksRaceConditionHandler = new TemplateApplicationProcessTasksRaceConditionHandler(factory, config.TryHandleProcessTaskConflictsAutomatically, config.TryHandleProcessTaskConflictsWhenServiceTasksOnlySelected);
			WorkflowRaceConditionHandler = ObjectFactory.Get<ITemplateApplicationRaceConditionHandler>("WorkflowTemplateApplicationRaceConditionHandler", factory, config.TryHandleProcessHeaderConflictsAutomatically);
		}

		public bool IsSuspectedDuplicate(ProcessTask task)
		{
			return ProcessTasksRaceConditionHandler.IsSuspectedDuplicate(task);
		}

		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			WorkflowRaceConditionHandler.Process(businessObjectsInOnSavingOrder);
			ProcessTasksRaceConditionHandler.Process(businessObjectsInOnSavingOrder);
		}

#if DEBUG

		public static bool TryHookupFactory(BusinessObjectFactory factory, bool tryHandleConflictsAutomatically, bool tryHandleProcessTaskConflictsWhenServiceTasksOnlySelected = false)
		{
			var config = new TemplateApplicationRaceHandlingConfig(tryHandleConflictsAutomatically, tryHandleConflictsAutomatically, tryHandleProcessTaskConflictsWhenServiceTasksOnlySelected);
			return WorkflowAfterOnSavingBOService.TryHookupRaceConditionHandlerService(factory, config);
		}

#endif
	}
}
