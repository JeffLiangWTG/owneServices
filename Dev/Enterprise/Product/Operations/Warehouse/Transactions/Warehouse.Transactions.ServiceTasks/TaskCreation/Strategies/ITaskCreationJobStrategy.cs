using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	interface ITaskCreationJobStrategy
	{
		TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job);
		TasksToCreateResult GetTasksToCreate(WhsReadyForPlanningJobsView job, TaskCreationWorkflowInfo workflowInfo, CancellationToken token);
		void LinkTasks(WhsReadyForPlanningJobsView job, IReadOnlyDictionary<ZGuid, ProcessTask> tasks, IEnumerable<ITaskManagementLineFact> lines);
		void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus);
	}
}
