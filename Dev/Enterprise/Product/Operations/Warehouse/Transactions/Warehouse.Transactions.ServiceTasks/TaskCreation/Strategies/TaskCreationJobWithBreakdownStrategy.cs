using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	abstract class TaskCreationJobWithBreakdownStrategy : ITaskCreationJobStrategy
	{
		public abstract TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job);

		public TasksToCreateResult GetTasksToCreate(
			WhsReadyForPlanningJobsView job,
			TaskCreationWorkflowInfo workflowInfo,
			CancellationToken token)
		{
			var facts = GetFacts(job, token);

			var rulesEngineService = ObjectFactory.Get<IProductionRulesEnginePushService>(nameof(IProductionRulesEnginePushService), new[] { job.Factory });
			var warehouseFilter = ProductionRuleSetFilter.WithWarehouse(workflowInfo.WarehousePK.ToGuid());
			var rulesEngineResult = rulesEngineService.RunRulesEngine(RulesContextType.ProductWarehouseTaskBreakdown, BreakdownSubType, warehouseFilter, facts, token);

			var taskResultFacts = rulesEngineResult.Facts.OfType<TaskResultFact>().ToArray();
			var tasksToCreate = new List<TaskToCreate>(taskResultFacts.Length);

			foreach (var taskResultFact in taskResultFacts)
			{
				tasksToCreate.Add(
					new TaskToCreate(
						taskResultFact.PK,
						FormflowType,
						TaskAndWorkflowName,
						TaskAndWorkflowName,
						string.Empty,
						RawNudge,
						taskResultFact.Capability,
						workflowInfo.ReleaseGroupPK));
			}

			return new TasksToCreateResult(tasksToCreate, rulesEngineResult.Facts.OfType<ITaskManagementLineFact>().ToArray());
		}

		protected abstract RulesContextSubType BreakdownSubType { get; }
		protected abstract ZString FormflowType { get; }
		protected abstract ZString TaskAndWorkflowName { get; }
		protected abstract short RawNudge { get; }
		protected abstract IEnumerable<IInputFact> GetFacts(WhsReadyForPlanningJobsView job, CancellationToken token);

		public abstract void LinkTasks(WhsReadyForPlanningJobsView job, IReadOnlyDictionary<ZGuid, ProcessTask> tasks, IEnumerable<ITaskManagementLineFact> lines);

		public abstract void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus);
	}
}
