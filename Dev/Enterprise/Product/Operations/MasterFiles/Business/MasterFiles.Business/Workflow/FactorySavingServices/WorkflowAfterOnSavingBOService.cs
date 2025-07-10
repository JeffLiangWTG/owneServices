using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// The purpose of this class is to control the order of execution of
	/// workflow related Factory Save services.
	///
	/// We need to decide who gets to be last.
	/// </summary>
	public class WorkflowAfterOnSavingBOService : IAfterOnSavingBOProcessingService
	{
		#region Static API

		public static bool TryHookupMilestoneStatusService(BusinessObjectFactory factory, MilestoneCollectionView view)
		{
			var service = HookupFactory(factory);
			return service.TryHookupMilestoneService(view);
		}

		public static bool TryHookupRaceConditionHandlerService(BusinessObjectFactory factory, TemplateApplicationRaceHandlingConfig config)
		{
			var service = HookupFactory(factory);
			return service.TryHookupRaceConditionHandlerServiceCore(factory, config);
		}

		public static WorkflowItemsManualChangeLogService GetWorkflowItemsChangeLogService(BusinessObjectFactory factory)
		{
			var service = HookupFactory(factory);
			return service.WorkflowItemsChangeLogService;
		}

		public static TemplateApplicationRaceConditionHandler GetTemplateApplicationRaceConditionHandler(BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetAfterOnSavingService<WorkflowAfterOnSavingBOService>();
			return service?.raceConditionHandler;
		}

		public static WorkflowAfterOnSavingBOService HookupFactory(BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetAfterOnSavingService<WorkflowAfterOnSavingBOService>();
			if (service == null)
			{
				service = new WorkflowAfterOnSavingBOService(factory);
				factory.ServiceContainer.AddAfterOnSavingService(service);
			}
			return service;
		}

		#endregion

		WorkflowAfterOnSavingBOService(BusinessObjectFactory factory)
		{
			Factory = factory;
			jobProcessHeaderUpdater = new JobProcessHeaderUpdater(Factory);
		}

		BusinessObjectFactory Factory { get; }

		#region Service Initialization

		UpdateMilestoneStatusService updateMilestoneStatus;
		TemplateApplicationRaceConditionHandler raceConditionHandler;
		WorkflowItemsManualChangeLogService workflowItemsManualChangeLog;
		readonly JobProcessHeaderUpdater jobProcessHeaderUpdater;

		bool TryHookupMilestoneService(MilestoneCollectionView view)
		{
			if (updateMilestoneStatus == null)
			{
				updateMilestoneStatus = new UpdateMilestoneStatusService();
			}
			return updateMilestoneStatus.AddView(view);
		}

		bool TryHookupRaceConditionHandlerServiceCore(BusinessObjectFactory factory, TemplateApplicationRaceHandlingConfig config)
		{
			if (raceConditionHandler != null)
			{
				return false;
			}

			var handler = new TemplateApplicationRaceConditionHandler(factory, config);
			if (handler.ProcessTasksRaceConditionHandler.ShouldProcess || handler.WorkflowRaceConditionHandler.ShouldProcess())
			{
				raceConditionHandler = handler;
				return true;
			}

			return false;
		}

		WorkflowItemsManualChangeLogService WorkflowItemsChangeLogService
		{
			get
			{
				if (workflowItemsManualChangeLog == null)
				{
					workflowItemsManualChangeLog = new WorkflowItemsManualChangeLogService();
				}
				return workflowItemsManualChangeLog;
			}
		}

		#endregion

		#region IAfterOnSavingBOProcessingService

		void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			// The order here is important.
			// Since raceConditionHandler can modify milestones which affects updateMilestoneStatus
			workflowItemsManualChangeLog?.ProcessLogs();
			raceConditionHandler?.ProcessBusinesObjects(businessObjectsInOnSavingOrder);
			updateMilestoneStatus?.ProcessBusinesObjects(businessObjectsInOnSavingOrder);
			jobProcessHeaderUpdater?.ProcessChanges(businessObjectsInOnSavingOrder);
			TaskStatusRaceConditionHandler.Process(businessObjectsInOnSavingOrder);
		}

		#endregion
	}
}
