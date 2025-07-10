using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class CycleCountBreakdownStrategyTest : BreakdownStrategyTest<CycleCountBreakdownStrategy>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountBreakdownStrategy(null));
		}

		public override void TestGetWorkflowInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var location1 = data.Whs1.FindLocation("A-1");
			var oldCycleCountWave = Factory.New<WhsCycleCountWave>();
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(data.Whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
			AssertEquals("Should get the correct job name", "Cycle Count Tasks For Warehouse 1", workflowInfo.NameForLog);
			AssertEquals("Should get the correct warehouse", data.Whs1.PK, workflowInfo.WarehousePK);
			AssertEquals("Should get the correct branch", data.Whs1.WW_GB_RelatedCompanyBranch, workflowInfo.BranchPK);
			AssertEquals("Should get the correct release group", releaseGroup.PK, workflowInfo.ReleaseGroupPK);

			var newCycleCountWaves = Factory.Load<WhsCycleCountWave>(new ZQuery(WhsCycleCountWaveSchema.PK, SQLComparisonOperator.NotEqual, oldCycleCountWave.PK));
			var newCycleCountWave = newCycleCountWaves.SingleOrDefault();
			AssertNotNull("Should have created a single new cycle count wave.", newCycleCountWave);
			AssertEquals("Should get the correct workflow provider", newCycleCountWave, workflowInfo.WorkflowProvider);
		}

		protected override void TestSetJobPlanningStatus(string status)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var location5 = data.Whs1.FindLocation("A-5");
			var location6 = data.Whs1.FindLocation("A-6");

			var otherWarehouse = Helper.CreateWarehouse("TEST");
			otherWarehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			var otherRow = Helper.CreateRowAndGenerateLocations(otherWarehouse, "A");

			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount4 = Helper.CreateWhsCycleCountLocation(location4, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount5 = Helper.CreateWhsCycleCountLocation(location5, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount6 = Helper.CreateWhsCycleCountLocation(otherRow.Locations[0], CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount3.WCL_TaskPlanningStatus = string.Empty;
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			cycleCount5.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			cycleCount6.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(data.Whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
			strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, new CancellationToken());

			var cycleCount7 = Helper.CreateWhsCycleCountLocation(location6, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount7.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status);

			var cycleCount1InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount1.PK);
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			var cycleCount3InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount3.PK);
			var cycleCount4InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount4.PK);
			var cycleCount5InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount5.PK);
			var cycleCount6InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount6.PK);
			var cycleCount7InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount7.PK);
			AssertEquals("Should have updated the job in the new factory.", status, cycleCount1InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should have updated the job in the new factory.", status, cycleCount2InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, cycleCount1.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, cycleCount2.WCL_TaskPlanningStatus);

			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount3.WCL_TaskPlanningStatus, cycleCount3InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount4.WCL_TaskPlanningStatus, cycleCount4InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount5.WCL_TaskPlanningStatus, cycleCount5InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount6.WCL_TaskPlanningStatus, cycleCount6InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount7.WCL_TaskPlanningStatus, cycleCount7InNewFactory.WCL_TaskPlanningStatus);
		}

		public void TestSetJobPlanningStatus_LoadFailed_Error() => TestSetJobPlanningStatus_LoadFailed(status: TaskPlanningStatus.Codes.Error);
		public void TestSetJobPlanningStatus_LoadFailed_Empty() => TestSetJobPlanningStatus_LoadFailed(status: string.Empty);

		void TestSetJobPlanningStatus_LoadFailed(string status)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var location5 = data.Whs1.FindLocation("A-5");
			var location6 = data.Whs1.FindLocation("A-6");

			var otherWarehouse = Helper.CreateWarehouse("TEST");
			otherWarehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			var otherRow = Helper.CreateRowAndGenerateLocations(otherWarehouse, "A");

			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount4 = Helper.CreateWhsCycleCountLocation(location4, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount5 = Helper.CreateWhsCycleCountLocation(location5, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount6 = Helper.CreateWhsCycleCountLocation(otherRow.Locations[0], CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount3.WCL_TaskPlanningStatus = string.Empty;
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			cycleCount5.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			cycleCount6.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(data.Whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
			// Avoid calling GetTasksToCreate, to simulate this call failing

			var cycleCount7 = Helper.CreateWhsCycleCountLocation(location6, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount7.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status);

			var cycleCount1InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount1.PK);
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			var cycleCount3InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount3.PK);
			var cycleCount4InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount4.PK);
			var cycleCount5InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount5.PK);
			var cycleCount6InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount6.PK);
			var cycleCount7InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount7.PK);
			AssertEquals("Should have updated the job in the new factory.", status, cycleCount1InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should have updated the job in the new factory.", status, cycleCount2InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should have updated the job in the new factory.", status, cycleCount7InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, cycleCount1.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, cycleCount2.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, cycleCount7.WCL_TaskPlanningStatus);

			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount3.WCL_TaskPlanningStatus, cycleCount3InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount4.WCL_TaskPlanningStatus, cycleCount4InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount5.WCL_TaskPlanningStatus, cycleCount5InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* have updated jobs that were not loaded.", cycleCount6.WCL_TaskPlanningStatus, cycleCount6InNewFactory.WCL_TaskPlanningStatus);
		}

		public void TestSetJobPlanningStatus_LoadFailed_Planned()
		{
			const string status = TaskPlanningStatus.Codes.Planned;
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var data = new TestDataSimpleEnvironment(Factory, 6, 1);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var location5 = data.Whs1.FindLocation("A-5");
			var location6 = data.Whs1.FindLocation("A-6");

			var otherWarehouse = Helper.CreateWarehouse("TEST");
			otherWarehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			var otherRow = Helper.CreateRowAndGenerateLocations(otherWarehouse, "A");

			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount4 = Helper.CreateWhsCycleCountLocation(location4, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount5 = Helper.CreateWhsCycleCountLocation(location5, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount6 = Helper.CreateWhsCycleCountLocation(otherRow.Locations[0], CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount3.WCL_TaskPlanningStatus = string.Empty;
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			cycleCount5.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			cycleCount6.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(data.Whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
			// Avoid calling GetTasksToCreate, to simulate this call failing

			var cycleCount7 = Helper.CreateWhsCycleCountLocation(location6, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount7.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			AssertExceptionThrown<ArgumentException>(() => strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status));
		}

		protected override void TestSetJobPlanningStatus_JobDoesNotExistCore()
		{
			Assert(true); // Irrelevant as we don't require the warehouse
		}

		public override void TestGetTasksToCreate()
		{
			var token = new CancellationToken();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var location1 = data.Whs1.FindLocation("A-1");
			var oldCycleCountWave = Factory.New<WhsCycleCountWave>();
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(data.Whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var fact1 = Mock.Of<IInputFact>();
			var fact2 = Mock.Of<IInputFact>();
			var facts = new[] { fact1, fact2 };
			var breakdownFactLoader = new Mock<ICycleCountLocationTaskBreakdownFactLoader>();
			breakdownFactLoader.Setup(fl => fl.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), data.Whs1.PK, token)).Returns(facts);

			var resultFact = new TaskResultFact("TEST");

			IEnumerable<IInputFact> factsPassedIn = null;
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseCycleCountLocation,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					factsPassedIn = facts;
					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = new CycleCountBreakdownStrategy(breakdownFactLoader.Object);
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				AssertEquals("Should have passed in 3 facts.", 3, factsPassedIn.Count());
				AssertEquals("Should return the facts from the fact loader.", true, factsPassedIn.Contains(fact1));
				AssertEquals("Should return the facts from the fact loader.", true, factsPassedIn.Contains(fact2));

				var contextFact = factsPassedIn.OfType<ITaskManagementContextFact>().Single();
				AssertEquals("Should default the fallback from the warehouse.", 5, contextFact.MaxNumberOfLinesFallBack);

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.CycleCountJob, taskToCreate.FormflowType);
				AssertEquals(nameof(taskToCreate.TaskType), "UDF", taskToCreate.TaskType);
				AssertEquals(nameof(taskToCreate.RawNudge), new ZShort(0), taskToCreate.RawNudge);
				AssertEquals(nameof(taskToCreate.TaskName), "Count Locations", taskToCreate.TaskName);
				AssertEquals(nameof(taskToCreate.WorkflowName), "Count Locations", taskToCreate.WorkflowName);
				AssertEquals(nameof(taskToCreate.StaffCode), string.Empty, taskToCreate.StaffCode);
				AssertEquals(nameof(taskToCreate.CapabilityCode), "TEST", taskToCreate.CapabilityCode);
				AssertEquals(nameof(taskToCreate.ReleaseGroupPk), releaseGroup.PK, taskToCreate.ReleaseGroupPk);
			}
		}

		public void TestGetTasksToCreate_IntegrationTest()
		{
			var token = new CancellationToken();
			var whs1 = Helper.CreateWarehouse("whs0");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");

			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_Priority = 4;
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.PalletIDOnly);
			cycleCount2.WCL_Priority = 7;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var resultFact = new TaskResultFact("TEST");

			IEnumerable<IInputFact> factsPassedIn = null;
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseCycleCountLocation,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					factsPassedIn = facts;
					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			var foundPks1 = Factory.TryGetValueFromCacheOnly<IEnumerable<Guid>>($"{nameof(CycleCountBreakdownStrategy)}_PKs", out var pks1);
			AssertEquals("Precondition: Should *not* have cached pks yet.", false, foundPks1);

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				AssertEquals("Should have passed in 3 facts.", 3, factsPassedIn.Count());
				var locationFact1 = factsPassedIn.OfType<ITaskManagementCycleCountLocationFact>().Single(l => l.LocationPK == location1.PK);
				var locationFact2 = factsPassedIn.OfType<ITaskManagementCycleCountLocationFact>().Single(l => l.LocationPK == location2.PK);

				var contextFact = factsPassedIn.OfType<ITaskManagementContextFact>().Single();
				AssertEquals("Should default the fallback from the warehouse.", 5, contextFact.MaxNumberOfLinesFallBack);

				CombineAssertions(() =>
				{
					AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Granularity), CycleCountGranularity.Codes.ProductWithAttributes, locationFact1.Granularity);
					AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Priority), 4, locationFact1.Priority);

					AssertEquals("Should set number of lines.", 1, locationFact1.Grouping.Fact.NumberOfLines);
					AssertEquals("Should *not* set other quantities.", 0, locationFact1.Grouping.Fact.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, locationFact1.Grouping.Fact.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, locationFact1.Grouping.Fact.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, locationFact1.Grouping.Fact.Volume);
					AssertEquals(nameof(CycleCountLocationCoreFact.EntityPK), cycleCount1.PK, ((CycleCountLocationCoreFact)locationFact1.Grouping.Fact).EntityPK);

					AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Granularity), CycleCountGranularity.Codes.PalletIDOnly, locationFact2.Granularity);
					AssertEquals(nameof(ITaskManagementCycleCountLocationFact.Priority), 7, locationFact2.Priority);
					AssertEquals("Should set number of lines.", 1, locationFact2.Grouping.Fact.NumberOfLines);
					AssertEquals("Should *not* set other quantities.", 0, locationFact2.Grouping.Fact.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, locationFact2.Grouping.Fact.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, locationFact2.Grouping.Fact.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, locationFact2.Grouping.Fact.Volume);
					AssertEquals(nameof(CycleCountLocationCoreFact.EntityPK), cycleCount2.PK, ((CycleCountLocationCoreFact)locationFact2.Grouping.Fact).EntityPK);
				});

				var foundPks2 = Factory.TryGetValueFromCacheOnly<IEnumerable<Guid>>($"{nameof(CycleCountBreakdownStrategy)}_PKs", out var pks2);
				AssertEquals("Should have cached pks.", true, foundPks2);
				AssertContainsExactElementsInAnyOrder("Should have cached pks.", [cycleCount1.PK.ToGuid(), cycleCount2.PK.ToGuid()], pks2);

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.CycleCountJob, taskToCreate.FormflowType);
				AssertEquals(nameof(taskToCreate.TaskType), "UDF", taskToCreate.TaskType);
				AssertEquals(nameof(taskToCreate.RawNudge), new ZShort(0), taskToCreate.RawNudge);
				AssertEquals(nameof(taskToCreate.TaskName), "Count Locations", taskToCreate.TaskName);
				AssertEquals(nameof(taskToCreate.WorkflowName), "Count Locations", taskToCreate.WorkflowName);
				AssertEquals(nameof(taskToCreate.StaffCode), string.Empty, taskToCreate.StaffCode);
				AssertEquals(nameof(taskToCreate.CapabilityCode), "TEST", taskToCreate.CapabilityCode);
				AssertEquals(nameof(taskToCreate.ReleaseGroupPk), releaseGroup.PK, taskToCreate.ReleaseGroupPk);
			}
		}

		public void TestLinkTasks_NullArguments()
		{
			var readyForPlanningJob = Factory.New<WhsReadyForPlanningJobsView>();
			var tasks = new Dictionary<ZGuid, ProcessTask>();
			var lines = Enumerable.Empty<ITaskManagementLineFact>();

			var strategy = GetStrategy();
			AssertExceptionThrown<ArgumentNullException>(() => strategy.LinkTasks(null, tasks, lines));
			AssertExceptionThrown<ArgumentNullException>(() => strategy.LinkTasks(readyForPlanningJob, null, lines));
			AssertExceptionThrown<ArgumentNullException>(() => strategy.LinkTasks(readyForPlanningJob, tasks, null));
		}

		public void TestLinkTasks() => TestLinkTasks(pksCached: true);

		public void TestLinkTasks_NoCycleCountPKsCached() => TestLinkTasks(pksCached: false);

		void TestLinkTasks(bool pksCached)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount4 = Helper.CreateWhsCycleCountLocation(location4, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount3.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var taskPK1 = ZGuid.NewZGuid();
			var taskPK2 = ZGuid.NewZGuid();

			var wave = Factory.New<WhsCycleCountWave>();
			var task1 = wave.WorkflowItems.AddNew();
			var task2 = wave.WorkflowItems.AddNew();

			var taskLink1 = new Mock<ITaskManagementGroupingFact>();
			taskLink1.Setup(tl => tl.PK).Returns(cycleCount1.PK.ToGuid());
			taskLink1.Setup(tl => tl.AssignedTask).Returns(taskPK1.ToGuid());

			var taskLink2 = new Mock<ITaskManagementGroupingFact>();
			taskLink2.Setup(tl => tl.PK).Returns(cycleCount3.PK.ToGuid());
			taskLink2.Setup(tl => tl.AssignedTask).Returns(taskPK1.ToGuid());

			var taskLink3 = new Mock<ITaskManagementGroupingFact>();
			taskLink3.Setup(tl => tl.PK).Returns(cycleCount2.PK.ToGuid());
			taskLink3.Setup(tl => tl.AssignedTask).Returns(taskPK2.ToGuid());

			var taskLink4 = new Mock<ITaskManagementGroupingFact>();
			taskLink4.Setup(tl => tl.PK).Returns(cycleCount4.PK.ToGuid());
			taskLink4.Setup(tl => tl.AssignedTask).Returns(Guid.Empty);

			var lineFact1 = new Mock<ITaskManagementLineFact>();
			lineFact1.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact1.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink1.Object));

			var lineFact2 = new Mock<ITaskManagementLineFact>();
			lineFact2.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact2.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink2.Object));

			var lineFact3 = new Mock<ITaskManagementLineFact>();
			lineFact3.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact3.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink3.Object));

			var lineFact4 = new Mock<ITaskManagementLineFact>();
			lineFact4.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact4.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink4.Object));

			var taskDictionary = new Dictionary<ZGuid, ProcessTask>
			{
				{ taskPK1, task1 },
				{ taskPK2, task2 },
			};

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(data.Whs1.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();

			if (pksCached)
			{
				Factory.GetCachedValue<IEnumerable<Guid>>($"{nameof(CycleCountBreakdownStrategy)}_PKs", () => [cycleCount1.PK.ToGuid(), cycleCount2.PK.ToGuid(), cycleCount3.PK.ToGuid()]);

				strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object, lineFact2.Object, lineFact3.Object, lineFact4.Object]);
				AssertEquals("Should have linked the tasks.", task1.PK, cycleCount1.WCL_P9_Task);
				AssertEquals("Should have linked the tasks.", task2.PK, cycleCount2.WCL_P9_Task);
				AssertEquals("Should have linked the tasks.", task1.PK, cycleCount3.WCL_P9_Task);
				AssertEquals("Should *not* have linked tasks with no result.", ZGuid.Empty, cycleCount4.WCL_P9_Task);
			}
			else
			{
				strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object, lineFact2.Object, lineFact3.Object]);
				AssertEquals("Should *not* have linked the tasks.", ZGuid.Empty, cycleCount1.WCL_P9_Task);
				AssertEquals("Should *not* have linked the tasks.", ZGuid.Empty, cycleCount2.WCL_P9_Task);
				AssertEquals("Should *not* have linked the tasks.", ZGuid.Empty, cycleCount3.WCL_P9_Task);
				AssertEquals("Should *not* have linked the tasks.", ZGuid.Empty, cycleCount4.WCL_P9_Task);
			}
		}

		protected override string JobType => "CCL";
		protected override CycleCountBreakdownStrategy GetStrategy() => new CycleCountBreakdownStrategy(new CycleCountLocationTaskBreakdownFactLoader());
	}
}
