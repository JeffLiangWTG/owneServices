using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class UnloadBreakdownStrategyTest : BreakdownStrategyTest<UnloadBreakdownStrategy>
	{
		public override void TestGetWorkflowInfo()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				AssertEquals("Should get the correct job name", receive.HumanReadableName, workflowInfo.NameForLog);
				AssertEquals("Should get the correct branch", data.Whs1.WW_GB_RelatedCompanyBranch, workflowInfo.BranchPK);
				AssertEquals("Should get the correct warehouse", data.Whs1.PK, workflowInfo.WarehousePK);
				AssertEquals("Should get the correct release group", releaseGroup.PK, workflowInfo.ReleaseGroupPK);
				AssertEquals("Should get the correct workflow provider", receive, workflowInfo.WorkflowProvider);
			}
		}

		protected override void TestSetJobPlanningStatus(string status)
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				var strategy = GetStrategy();
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

				strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status);
				AssertEquals("Should have updated the job in the new factory.", status, receiveInNewFactory.WD_TaskPlanningStatus);
				AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);
			}
		}

		public override void TestGetTasksToCreate()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
				data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

				Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Core.Constants.PkgUnit.Pallet, 5m);
				Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, (ZString)Core.Constants.PkgUnit.Pallet)
					.F3_UOMType = UOMPackTypesList.Codes.Pallet;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
					allocateLocations: false, finalise: false);
				receive.WD_ExternalReference = "RR";
				receive.WD_CustomerReference = "CR";
				receive.WD_RS_NKServiceLevel = "TST";
				receive.WD_ArrivalDate = ZDateTime.BrettsBirthday.ToOffset();
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

				var inventoryLine = receive.Lines[0];
				inventoryLine.WE_WHC_NKOriginalInventoryHeldCode = "123";
				inventoryLine.WE_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				inventoryLine.WE_ClientOrderedUnits = 3m;
				inventoryLine.WE_TransactionQuantity = 5m;

				inventoryLine.WE_PartAttrib1 = "1";
				inventoryLine.WE_PartAttrib2 = "2";
				inventoryLine.WE_PartAttrib3 = "3";
				inventoryLine.WE_ExpiryDate = ZDate.BrettsBirthday.AddDays(20);
				inventoryLine.WE_PackingDate = ZDate.BrettsBirthday.AddDays(-10);
				inventoryLine.WE_RequiredByDate = ZDateTime.BrettsBirthday.AddDays(10).ToOffset();
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				ITaskManagementContextFact context = null;
				ITaskManagementGroupingFact grouping = null;
				ITaskManagementUnloadLineFact unloadLine = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseUnloadLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						context = facts.OfType<ITaskManagementContextFact>().Single();
						unloadLine = facts.OfType<ITaskManagementUnloadLineFact>().Single();
						grouping = unloadLine.Grouping.Fact;
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
					AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.UnloadJob, taskToCreate.FormflowType);
					AssertEquals(nameof(taskToCreate.TaskType), "UDF", taskToCreate.TaskType);
					AssertEquals(nameof(taskToCreate.RawNudge), new ZShort(100), taskToCreate.RawNudge);
					AssertEquals(nameof(taskToCreate.TaskName), "Unload", taskToCreate.TaskName);
					AssertEquals(nameof(taskToCreate.WorkflowName), "Unload", taskToCreate.WorkflowName);
					AssertEquals(nameof(taskToCreate.StaffCode), string.Empty, taskToCreate.StaffCode);
					AssertEquals(nameof(taskToCreate.CapabilityCode), "TEST", taskToCreate.CapabilityCode);
					AssertEquals(nameof(taskToCreate.ReleaseGroupPk), releaseGroup.PK, taskToCreate.ReleaseGroupPk);

					AssertNotNull("Should have created a context.", context);
					AssertEquals("Should *not* have set a fallback.", 0, context.MaxNumberOfLinesFallBack);

					AssertNotNull("Should have created a grouping.", grouping);
					AssertEquals("Should set number of lines.", 1, grouping.NumberOfLines);
					AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);

					AssertEquals(nameof(ITaskManagementUnloadLineFact.Client), receive.WD_OH_Client, unloadLine.Client.Fact.PK);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.PackUQ), Core.Constants.PkgUnit.Pallet, unloadLine.PackUQ);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.UOMType), UOMPackTypesList.Codes.Pallet, unloadLine.UOMType);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.ServiceLevel), "TST", unloadLine.ServiceLevel);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.ReceiveReference), "RR", unloadLine.ReceiveReference);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.CustomerReference), "CR", unloadLine.CustomerReference);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.HoldCode), "123", unloadLine.HoldCode);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.ArrivalDate), ZDateTime.BrettsBirthday, unloadLine.ArrivalDate);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.RequiredDate), ZDateTime.BrettsBirthday.AddDays(10), unloadLine.RequiredDate);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.PartAttribute1), "1", unloadLine.PartAttribute1);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.PartAttribute2), "2", unloadLine.PartAttribute2);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.PartAttribute3), "3", unloadLine.PartAttribute3);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.ExpiryDate), ZDate.BrettsBirthday.AddDays(20), unloadLine.ExpiryDate);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.PackingDate), ZDateTime.BrettsBirthday.AddDays(-10), unloadLine.PackingDate);

					var part1OwnerRelation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.Product), part1OwnerRelation.PK, unloadLine.Product.Fact.PK);
					AssertEquals(nameof(ITaskManagementUnloadLineFact.Product), data.Part1.OP_PartNum, unloadLine.Product.Fact.Code);
				}
			}
		}

		public void TestGetTasksToCreate_Product()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
					allocateLocations: false, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				var receiveLine1 = receive.Lines[0];
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 20m);
				var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 20m);
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				IEnumerable<ITaskManagementUnloadLineFact> unloadLines = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseUnloadLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						unloadLines = facts.OfType<ITaskManagementUnloadLineFact>().ToArray();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

					var products = unloadLines.Select(ul => ul.Product.Fact).Distinct().ToArray();
					AssertEquals("Should have created 2x products.", 2, products.Length);

					var line1Fact = unloadLines.Single(l => l.PK == receiveLine1.PK);
					var line2Fact = unloadLines.Single(l => l.PK == receiveLine2.PK);
					var line3Fact = unloadLines.Single(l => l.PK == receiveLine3.PK);
					var line4Fact = unloadLines.Single(l => l.PK == receiveLine4.PK);
					AssertEquals("Should share the same product.", line1Fact.Product.Fact, line2Fact.Product.Fact);
					AssertEquals("Should share the same product.", line3Fact.Product.Fact, line4Fact.Product.Fact);
					AssertNotEquals("Should *not* share the same product.", line1Fact.Product.Fact, line4Fact.Product.Fact);
				}
			}
		}

		public void TestGetTasksToCreate_Organisations()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, allocateLocations: false, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				receive.SupplierDocAddress.OrganisationPK = data.Org1.PK;

				var receiveLine1 = receive.Lines[0];
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receiveLine1.ConsigneeDocAddress.OrganisationPK = data.Org1.PK;
				receiveLine2.ConsigneeDocAddress.OrganisationPK = data.Org1.PK;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				IEnumerable<ITaskManagementUnloadLineFact> unloadLines = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseUnloadLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						unloadLines = facts.OfType<ITaskManagementUnloadLineFact>().ToArray();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

					var line1Fact = unloadLines.Single(l => l.PK == receiveLine1.PK);
					var line2Fact = unloadLines.Single(l => l.PK == receiveLine2.PK);
					AssertEquals("Should share the same organisation.", line1Fact.Client.Fact, line1Fact.Supplier.Fact);
					AssertEquals("Should share the same organisation.", line1Fact.Client.Fact, line1Fact.Consignee.Fact);
					AssertEquals("Should share the same organisation.", line1Fact.Consignee.Fact, line2Fact.Consignee.Fact);
				}
			}
		}

		public void TestGetTasksToCreate_Organisations_Different()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				var org2 = Helper.CreateClient("O2");
				var org3 = Helper.CreateClient("O3");
				var org4 = Helper.CreateClient("O4");

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, allocateLocations: false, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				receive.SupplierDocAddress.OrganisationPK = org2.PK;

				var receiveLine1 = receive.Lines[0];
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receiveLine1.ConsigneeDocAddress.OrganisationPK = org3.PK;
				receiveLine2.ConsigneeDocAddress.OrganisationPK = org4.PK;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				IEnumerable<ITaskManagementUnloadLineFact> unloadLines = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseUnloadLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						unloadLines = facts.OfType<ITaskManagementUnloadLineFact>().ToArray();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

					var line1Fact = unloadLines.Single(l => l.PK == receiveLine1.PK);
					var line2Fact = unloadLines.Single(l => l.PK == receiveLine2.PK);
					AssertEquals("Should share the client.", line1Fact.Client.Fact, line2Fact.Client.Fact);
					AssertEquals("Should share the supplier.", line1Fact.Supplier.Fact, line2Fact.Supplier.Fact);
					AssertEquals("Should use the correct client.", data.Org1.PK, line1Fact.Client.Fact.PK);
					AssertEquals("Should use the correct supplier.", org2.PK, line1Fact.Supplier.Fact.PK);
					AssertEquals("Should use the correct consignee.", org3.PK, line1Fact.Consignee.Fact.PK);
					AssertEquals("Should use the correct consignee.", org4.PK, line2Fact.Consignee.Fact.PK);
				}
			}
		}

		public void TestGetTasksToCreate_LooseInventory()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
					allocateLocations: false, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				var receiveLine1 = receive.Lines[0];
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				IEnumerable<ITaskManagementUnloadLineFact> unloadLines = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseUnloadLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						unloadLines = facts.OfType<ITaskManagementUnloadLineFact>().ToArray();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

					var groupings = unloadLines.Select(ul => ul.Grouping.Fact).ToArray();
					AssertEquals("Should have created 3x single line groupings.", 3, groupings.Length);

					foreach (var grouping in groupings)
					{
						AssertEquals("Should set number of lines.", 1, grouping.NumberOfLines);
						AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfPacks);
						AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
						AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
						AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);
					}
				}
			}
		}

		public void TestGetTasksToCreate_Pallets() => TestGetTasksToCreate_Pallets(testCaseInsensitivity: false);
		public void TestGetTasksToCreate_Pallets_CaseInsensitive() => TestGetTasksToCreate_Pallets(testCaseInsensitivity: true);

		void TestGetTasksToCreate_Pallets(bool testCaseInsensitivity)
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
					allocateLocations: false, finalise: false);
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				var receiveLine1 = receive.Lines[0];
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
				var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
				receiveLine1.WE_PalletID = "ABC";
				receiveLine2.WE_PalletID = testCaseInsensitivity ? "aBc" : "ABC";
				receiveLine3.WE_PalletID = "DEF";
				receiveLine4.WE_PalletID = testCaseInsensitivity ? "dEf" : "DEF";
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(receive.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				IEnumerable<ITaskManagementUnloadLineFact> unloadLines = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseUnloadLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						unloadLines = facts.OfType<ITaskManagementUnloadLineFact>().ToArray();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

					var groupings = unloadLines.Select(ul => ul.Grouping.Fact).Distinct().ToArray();
					AssertEquals("Should have created 2x groupings.", 2, groupings.Length);
					AssertEquals("Should set number of lines.", 2, groupings[0].NumberOfLines);
					AssertEquals("Should set number of lines.", 2, groupings[1].NumberOfLines);
					AssertEquals("Should set number of packs.", 1, groupings[0].NumberOfPacks);
					AssertEquals("Should set number of packs.", 1, groupings[1].NumberOfPacks);

					var line1Fact = unloadLines.Single(l => l.PK == receiveLine1.PK);
					var line2Fact = unloadLines.Single(l => l.PK == receiveLine2.PK);
					var line3Fact = unloadLines.Single(l => l.PK == receiveLine3.PK);
					var line4Fact = unloadLines.Single(l => l.PK == receiveLine4.PK);
					AssertEquals("Lines on the same pallet should be grouped.", line1Fact.Grouping.Fact, line2Fact.Grouping.Fact);
					AssertEquals("Lines on the same pallet should be grouped.", line3Fact.Grouping.Fact, line4Fact.Grouping.Fact);
					AssertNotEquals("Lines on different pallets should *not* be grouped.", line1Fact.Grouping.Fact, line4Fact.Grouping.Fact);
				}
			}
		}

		protected override string JobType => DocketType.Codes.Receive;
		protected override UnloadBreakdownStrategy GetStrategy() => new UnloadBreakdownStrategy();
	}
}
