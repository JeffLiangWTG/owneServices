using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class CycleCountBreakdownStrategy : TaskCreationJobWithBreakdownStrategy
	{
		public CycleCountBreakdownStrategy(ICycleCountLocationTaskBreakdownFactLoader factLoader)
		{
			FactLoader = Argument.NotNull(factLoader, nameof(factLoader));
		}

		ICycleCountLocationTaskBreakdownFactLoader FactLoader { get; }

		public override TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job)
		{
			var warehouse = LoadWarehouse(job);
			var name = Res.GetString("9c0e3e7a-6c70-42a1-a838-f55f1facbb27", "Cycle Count Tasks For Warehouse {0}", warehouse.WW_WarehouseCode);

			var cycleCountWave = job.Factory.New<WhsCycleCountWave>();
			return new TaskCreationWorkflowInfo(name, warehouse.WW_GB_RelatedCompanyBranch, warehouse.PK, warehouse.WW_GG_ReleaseGroup, cycleCountWave);
		}

		public override void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(planningStatus, nameof(planningStatus));

			IEnumerable<WhsCycleCountLocation> cycleCountLocations = null;
			if (job.Factory.TryGetValueFromCacheOnly<IEnumerable<Guid>>(CacheKey, out var cycleCountPKs))
			{
				cycleCountLocations = LoadCycleCountLocations(factory, cycleCountPKs);
			}
			else if (string.IsNullOrEmpty(planningStatus) ||
				string.Equals(planningStatus, TaskPlanningStatus.Codes.Error, StringComparison.OrdinalIgnoreCase))
			{
				var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsCycleCountLocationSchema.WCL_WL_Location);
				warehouseSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, job.PK);

				var query = new ZDBOnlyQuery(typeof(WhsCycleCountLocation));
				query.AddToFilter(WhsCycleCountLocationSchema.WCL_TaskPlanningStatus, TaskPlanningStatus.Codes.Ready);
				query.AddSubQuery(warehouseSubQuery, JoinCondition.And);

				cycleCountLocations = factory.Load<WhsCycleCountLocation>(query);
			}
			else
			{
				throw new ArgumentException($"Attempted to set status '{planningStatus}' with no {nameof(WhsCycleCountLocation)} cached.");
			}

			foreach (var cycleCountLocation in cycleCountLocations)
			{
				cycleCountLocation.WCL_TaskPlanningStatus = planningStatus;
			}
		}

		protected override RulesContextSubType BreakdownSubType => RulesContextSubType.ProductWarehouseCycleCountLocation;

		protected override ZString FormflowType => WarehouseTaskFormFlowTypes.CycleCountJob;

		protected override ZString TaskAndWorkflowName => Res.GetString("4da37e01-06ac-4b47-a975-3df3dab52bb5", "Count Locations");

		protected override short RawNudge => 0;

		protected override IEnumerable<IInputFact> GetFacts(WhsReadyForPlanningJobsView job, CancellationToken token)
		{
			var warehouse = LoadWarehouse(job);
			var loadedFacts = FactLoader.LoadInputFacts(job.Factory.GetCachedReadOnlyFactory(), warehouse.PK, token).ToArray();

			var cycleCountPKs = loadedFacts.OfType<ITaskManagementCycleCountLocationFact>().Select(ccl => ((CycleCountLocationCoreFact)ccl.Grouping.Fact).EntityPK).ToArray();
			job.Factory.GetCachedValue<IEnumerable<Guid>>(CacheKey, () => cycleCountPKs);

			return [..loadedFacts, new TaskManagementContextFact(warehouse.WW_NumberOfCycleCountLocationsToAutoAssign)];
		}

		public override void LinkTasks(
			WhsReadyForPlanningJobsView job,
			IReadOnlyDictionary<ZGuid, ProcessTask> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
		{
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(tasks, nameof(tasks));
			Argument.NotNull(lines, nameof(lines));

			if (job.Factory.TryGetValueFromCacheOnly<IEnumerable<Guid>>(CacheKey, out var cycleCountPKs))
			{
				var cycleCountLocations = LoadCycleCountLocations(job.Factory, cycleCountPKs).ToDictionary(ccl => ccl.PK);

				foreach (var taskLink in lines.Select(l => l.Grouping.Fact).Distinct().Where(tl => tl.AssignedTask != Guid.Empty))
				{
					var assignedTask = tasks[taskLink.AssignedTask];
					var cycleCountLocation = cycleCountLocations[taskLink.PK];

					cycleCountLocation.WCL_P9_Task = assignedTask.PK;
				}
			}
		}

		IEnumerable<WhsCycleCountLocation> LoadCycleCountLocations(BusinessObjectFactory factory, IEnumerable<Guid> pks)
		{
			var query = new ZQuery { AllowTableValuedParameters = true };
			query.AddToFilter(WhsCycleCountLocationSchema.PK, pks);
			return factory.Load<WhsCycleCountLocation>(query);
		}

		WhsWarehouse LoadWarehouse(WhsReadyForPlanningJobsView job, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(job, nameof(job));
			return Argument.NotNull((factory ?? job.Factory).Load<WhsWarehouse>(job.PK), "loaded job");
		}

		const string CacheKey = $"{nameof(CycleCountBreakdownStrategy)}_PKs";
	}
}
