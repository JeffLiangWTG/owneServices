using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class LoadBreakdownStrategyTest : BreakdownStrategyTest<LoadBreakdownStrategy>
	{
		public override void TestGetWorkflowInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
			AssertEquals("Should get the correct job name", load.HumanReadableName, workflowInfo.NameForLog);
			AssertEquals("Should get the correct branch", data.Whs1.WW_GB_RelatedCompanyBranch, workflowInfo.BranchPK);
			AssertEquals("Should get the correct warehouse", data.Whs1.PK, workflowInfo.WarehousePK);
			AssertEquals("Should get the correct release group", releaseGroup.PK, workflowInfo.ReleaseGroupPK);
			AssertEquals("Should get the correct workflow provider", load, workflowInfo.WorkflowProvider);
		}

		protected override void TestSetJobPlanningStatus(string status)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadInNewFactory = newFactory.Load<WhsLoad>(load.PK);

			strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status);
			AssertEquals("Should have updated the job in the new factory.", status, loadInNewFactory.WLO_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, load.WLO_TaskPlanningStatus);
		}

		public override void TestGetTasksToCreate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = data.Org1.PK;
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			ITaskManagementGroupingFact grouping = null;
			ITaskManagementLoadPackageFact packageFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseLoadPackage,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					packageFact = facts.OfType<ITaskManagementLoadPackageFact>().Single();
					grouping = packageFact.Grouping.Fact;
					grouping.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.LoadJob, taskToCreate.FormflowType);
				AssertEquals(nameof(taskToCreate.TaskType), "UDF", taskToCreate.TaskType);
				AssertEquals(nameof(taskToCreate.RawNudge), new ZShort(200), taskToCreate.RawNudge);
				AssertEquals(nameof(taskToCreate.TaskName), "Load Packages", taskToCreate.TaskName);
				AssertEquals(nameof(taskToCreate.WorkflowName), "Load Packages", taskToCreate.WorkflowName);
				AssertEquals(nameof(taskToCreate.StaffCode), string.Empty, taskToCreate.StaffCode);
				AssertEquals(nameof(taskToCreate.CapabilityCode), "TEST", taskToCreate.CapabilityCode);
				AssertEquals(nameof(taskToCreate.ReleaseGroupPk), releaseGroup.PK, taskToCreate.ReleaseGroupPk);

				AssertNotNull("Should have created a context.", context);
				AssertEquals("Should *not* have set a fallback.", 0, context.MaxNumberOfLinesFallBack);

				AssertNotNull("Should have created a grouping.", grouping);
				AssertEquals("Should set number of packs.", 1, grouping.NumberOfPacks);
				AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfLines);
				AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
				AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
				AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package.PK, packageFact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.Client), data.Org1.PK, packageFact.Client.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.DockDoorLocation), data.Whs1.DefaultOutboundDockDoorLocation.PK, packageFact.DockDoorLocation.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.TransportCompany), data.Org1.PK, packageFact.TransportCompany.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.ConsigneeAddress), order.ConsigneeAddressPK, packageFact.ConsigneeAddress.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.CarrierServiceLevel), "RD", packageFact.CarrierServiceLevel);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.HasDangerousGoods), false, packageFact.HasDangerousGoods);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.IsOnAHandlingUnit), false, packageFact.IsOnAHandlingUnit);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterPackageType), PkgUnit.Box, packageFact.OuterPackageType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterUOMType), UOMPackTypesList.Codes.Case, packageFact.OuterUOMType);
			}
		}

		public void TestGetTasksToCreate_LoosePackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = data.Org1.PK;
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Bag);
			package1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package2.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var undg = Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");
			undg.DI_ParentID = package2.PK;
			undg.DI_ParentTableCode = package2.TablePrefix;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementLoadPackageFact packageFact1 = null;
			ITaskManagementLoadPackageFact packageFact2 = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseLoadPackage,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					packageFact1 = facts.OfType<ITaskManagementLoadPackageFact>().Single(p => p.PK == package1.PK);
					packageFact2 = facts.OfType<ITaskManagementLoadPackageFact>().Single(p => p.PK == package2.PK);

					var grouping1 = packageFact1.Grouping.Fact;
					grouping1.AssignedTask = resultFact.PK;

					var grouping2 = packageFact2.Grouping.Fact;
					grouping2.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.LoadJob, taskToCreate.FormflowType);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package1.PK, packageFact1.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.HasDangerousGoods), false, packageFact1.HasDangerousGoods);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.IsOnAHandlingUnit), false, packageFact1.IsOnAHandlingUnit);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterPackageType), PkgUnit.Box, packageFact1.OuterPackageType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterUOMType), UOMPackTypesList.Codes.Case, packageFact1.OuterUOMType);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package2.PK, packageFact2.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.HasDangerousGoods), true, packageFact2.HasDangerousGoods);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.IsOnAHandlingUnit), false, packageFact2.IsOnAHandlingUnit);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterPackageType), PkgUnit.Bag, packageFact2.OuterPackageType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterUOMType), string.Empty, packageFact2.OuterUOMType);

				AssertNotEquals(nameof(ITaskManagementLoadPackageFact.Grouping), packageFact1.Grouping.Fact.PK, packageFact2.Grouping.Fact.PK);
			}
		}

		public void TestGetTasksToCreate_HandlingUnits() => TestGetTasksToCreate_HandlingUnits(hasLoadPivots: false);
		public void TestGetTasksToCreate_HandlingUnits_OnLoad() => TestGetTasksToCreate_HandlingUnits(hasLoadPivots: true);

		void TestGetTasksToCreate_HandlingUnits(bool hasLoadPivots)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, PkgUnit.Package)).F3_UOMType = UOMPackTypesList.Codes.Case;

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = data.Org1.PK;
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package2.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var undg = Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");
			undg.DI_ParentID = package2.PK;
			undg.DI_ParentTableCode = package2.TablePrefix;
			Factory.Save();

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			PackingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);
			Factory.Save();

			if (hasLoadPivots)
			{
				load.WLO_TransportationUnitNumber = "TEST";
				load.WLO_StartTime = ZDateTimeOffset.Now;
				var pivot1PKG = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
				var pivot2PKG = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
				var pivotHU = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage.PK, load);
				Factory.Save();
			}

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementGroupingFact grouping = null;
			ITaskManagementLoadPackageFact[] packageFacts = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseLoadPackage,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					packageFacts = facts.OfType<ITaskManagementLoadPackageFact>().ToArray();

					grouping = facts.OfType<ITaskManagementLoadPackageFact>().First().Grouping.Fact;
					grouping.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				AssertEquals("Should have 2 packages passed in.", 2, packageFacts.Length);
				var packageFact1 = packageFacts.Single(p => p.PK == package1.PK);
				var packageFact2 = packageFacts.Single(p => p.PK == package2.PK);

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.LoadJob, taskToCreate.FormflowType);

				AssertNotNull("Should have created a grouping.", grouping);
				AssertEquals("Should set number of packs.", 1, grouping.NumberOfPacks);
				AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfLines);
				AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
				AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
				AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package1.PK, packageFact1.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.HasDangerousGoods), false, packageFact1.HasDangerousGoods);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.IsOnAHandlingUnit), true, packageFact1.IsOnAHandlingUnit);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterPackageType), PkgUnit.Package, packageFact1.OuterPackageType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterUOMType), UOMPackTypesList.Codes.Case, packageFact1.OuterUOMType);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package2.PK, packageFact2.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.HasDangerousGoods), true, packageFact2.HasDangerousGoods);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.IsOnAHandlingUnit), true, packageFact2.IsOnAHandlingUnit);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterPackageType), PkgUnit.Package, packageFact2.OuterPackageType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.OuterUOMType), UOMPackTypesList.Codes.Case, packageFact2.OuterUOMType);

				AssertEquals("Should have the same grouping.", packageFact1.Grouping.Fact.PK, packageFact2.Grouping.Fact.PK);
			}
		}

		public void TestGetTasksToCreate_Loaded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			load.WLO_TransportationUnitNumber = "TEST";
			load.WLO_StartTime = ZDateTimeOffset.Now;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = data.Org1.PK;
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package2.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pivotPKG = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivotPKG.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG.WLP_GS_NKLoadingUser = "LTS";
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementLoadPackageFact packageFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseLoadPackage,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					packageFact = facts.OfType<ITaskManagementLoadPackageFact>().Single();

					var grouping1 = packageFact.Grouping.Fact;
					grouping1.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.LoadJob, taskToCreate.FormflowType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package2.PK, packageFact.PK);
			}
		}

		public void TestGetTasksToCreate_OveriddenConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WLO_PlannedLoad = load.PK;
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_City = "Sydney";
			order.ConsigneeDocAddress.E2_Postcode = "2000";
			order.ConsigneeDocAddress.E2_State = "NSW";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementLoadPackageFact packageFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseLoadPackage,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					packageFact = facts.OfType<ITaskManagementLoadPackageFact>().Single();

					var grouping1 = packageFact.Grouping.Fact;
					grouping1.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.LoadJob, taskToCreate.FormflowType);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package1.PK, packageFact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.ConsigneeAddress), order.ConsigneeDocAddress.PK, packageFact.ConsigneeAddress.Fact.PK);
				AssertNull("Organisation should be null.", packageFact.ConsigneeAddress.Fact.Organisation.Fact);
			}
		}

		public void TestGetTasksToCreate_Organisations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var org2 = Helper.CreateClient("C2");
			var org3 = Helper.CreateClient("C3");
			var org4 = Helper.CreateClient("C4");
			var org5 = Helper.CreateClient("C5");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var carrierServicelevel = org3.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(org3, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order1.ConsigneePK = org4.PK;
			order1.WD_WLO_PlannedLoad = load.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, data.Part1, 10m);
			order2.ConsigneePK = org5.PK;
			order2.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;

			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);

			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(load.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementLoadPackageFact packageFact1 = null;
			ITaskManagementLoadPackageFact packageFact2 = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehouseLoadPackage,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					packageFact1 = facts.OfType<ITaskManagementLoadPackageFact>().Single(p => p.PK == package1.PK);
					packageFact2 = facts.OfType<ITaskManagementLoadPackageFact>().Single(p => p.PK == package2.PK);

					var grouping1 = packageFact1.Grouping.Fact;
					grouping1.AssignedTask = resultFact.PK;

					var grouping2 = packageFact2.Grouping.Fact;
					grouping2.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var taskToCreate = tasksToCreate.Tasks.Single();
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.LoadJob, taskToCreate.FormflowType);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package1.PK, packageFact1.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.Client), data.Org1.PK, packageFact1.Client.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.TransportCompany), org3.PK, packageFact1.TransportCompany.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.ConsigneeAddress), org4.PK, packageFact1.ConsigneeAddress.Fact.Organisation.Fact.PK);

				AssertEquals(nameof(ITaskManagementLoadPackageFact.PK), package2.PK, packageFact2.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.Client), org2.PK, packageFact2.Client.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.TransportCompany), org3.PK, packageFact2.TransportCompany.Fact.PK);
				AssertEquals(nameof(ITaskManagementLoadPackageFact.ConsigneeAddress), org5.PK, packageFact2.ConsigneeAddress.Fact.Organisation.Fact.PK);

				AssertNotEquals(nameof(ITaskManagementLoadPackageFact.Grouping), packageFact1.Grouping.Fact.PK, packageFact2.Grouping.Fact.PK);
			}
		}

		PackingTestHelper PackingHelper => packingHelper ??= new PackingTestHelper(Factory);
		PackingTestHelper packingHelper;

		protected override string JobType => "LOA";
		protected override LoadBreakdownStrategy GetStrategy() => new LoadBreakdownStrategy();
	}
}
