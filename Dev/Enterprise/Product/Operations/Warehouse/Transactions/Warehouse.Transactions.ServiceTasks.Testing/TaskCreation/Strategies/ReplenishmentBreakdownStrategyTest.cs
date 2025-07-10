using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class ReplenishmentBreakdownStrategyTest : TransferBreakdownStrategyTest<ReplenishmentBreakdownStrategy>
	{
		public void TestGetTasksToCreate_WithPickBeingReplenished()
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
					allocateLocations: false, finalise: false);
				var receiveLine1 = receive.Lines[0];
				receiveLine1.WE_WL = location1.PK;
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var pick = Helper.CreatePickNew();
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, string.Empty);
				transferLine1.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				transfer.WD_WP_PickBeingReplenished = pick.PK;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				ITaskManagementTransferLineFact transferLine = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseTransferLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						transferLine = facts.OfType<ITaskManagementTransferLineFact>().Single();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());
					AssertEquals("Should have awaiting picks.", true, transferLine.HasAwaitingPicks);
				}
			}
		}

		public void TestGetTasksToCreate_WithFixedPickFaceWaitingReplenishment()
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
				var location2 = data.Whs1.FindLocation("A-2");
				var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);
				var location3 = data.Whs1.FindLocation("A-3");

				var org2 = Helper.CreateClient();
				Helper.CreateProductClientRelationShip(org2, data.Part1);
				var pickFaceClient2 = Helper.CreateProductPickFace(data.Part1, org2, location2);

				var whs2 = Helper.CreateWarehouse("WW2", "B", 2, 1);
				Factory.Save();
				var locationWhs2_1 = whs2.FindLocation("B-1");
				var locationWhs2_2 = whs2.FindLocation("B-2");
				var pickFaceWhs2 = Helper.CreateProductPickFace(data.Part1, data.Org1, locationWhs2_1);
				Factory.Save();

				var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveLine(receive1, data.Part1, 100m, location1);
				Helper.CreateWhsReceiveLine(receive1, data.Part2, 100m, location2);
				Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location3);
				Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, location3);
				receive1.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive1);
				Factory.Save();

				var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
				Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, locationWhs2_1);
				Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, locationWhs2_2);
				receive2.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive2);
				Factory.Save();

				var receive3 = Helper.CreateWhsReceive(org2, data.Whs1, "R3");
				Helper.CreateWhsReceiveLine(receive3, data.Part1, 50m, location2);
				Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, location3);
				receive3.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive3);
				Factory.Save();

				var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, location3, location1);
				var transfer1Line2 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, location3, location2);
				transfer1Line1.RunPreSaveValidation();
				transfer1Line2.RunPreSaveValidation();
				Factory.Save();
				AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer1.IsFinalised);

				SetupTransfer(transfer1);
				transfer1.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var transfer2 = Helper.CreateWhsTransfer(data.Org1, whs2);
				var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, locationWhs2_2, locationWhs2_1);
				transferLine2.RunPreSaveValidation();
				Factory.Save();
				AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer1.IsFinalised);
				AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer2.IsFinalised);

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 200m);
				var pick1 = Helper.CreatePickNew(order1);
				AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);

				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "O2", data.Part1, 100m);
				var pick2 = Helper.CreatePickNew(order2);
				AssertEquals("Precondition: pick2 is awaiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);

				var order3 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O3", data.Part1, 100m);
				var pick3 = Helper.CreatePickNew(order3);
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer1.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				ITaskManagementTransferLineFact[] transferLines = null;

				var resultFact = new TaskResultFact("TEST");

				var token = new CancellationToken();
				var rulesEngine = new Mock<IProductionRulesEnginePushService>();
				rulesEngine.Setup(re => re.RunRulesEngine(
					RulesContextType.ProductWarehouseTaskBreakdown,
					RulesContextSubType.ProductWarehouseTransferLine,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
					It.IsAny<IEnumerable<IInputFact>>(), token))
					.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
					(_, _, _, facts, _) =>
					{
						transferLines = facts.OfType<ITaskManagementTransferLineFact>().ToArray();
						return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
					});

				using (ObjectFactory.Substitute(rulesEngine.Object))
				{
					var strategy = GetStrategy();
					var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
					var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
					AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

					var part1LineFact = transferLines.Single(l => l.Product.Fact.Code == data.Part1.OP_PartNum);
					var part2LineFact = transferLines.Single(l => l.Product.Fact.Code == data.Part2.OP_PartNum);
					AssertEquals("Line 1 should have awaiting picks.", true, part1LineFact.HasAwaitingPicks);
					AssertEquals("Line 2 should have awaiting picks.", false, part2LineFact.HasAwaitingPicks);
				}
			}
		}

		protected override string JobType => NonPersistentTransferType.Codes.AutoCreatedReplenishment;

		protected override string ExpectedFormFlowType => WarehouseTaskFormFlowTypes.ReplenishmentJob;

		protected override string ExpectedTaskAndWorkflowName => "Replenishment";

		protected override ZInt ExpectedNudge => 200;

		protected override BooleanRegistryItem AutomaticallySetToPlanningRegistryItem
			=> WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning;

		protected override void SetupTransfer(WhsTransfer transfer) => transfer.WD_IsPickFaceReplenishment = true;
	}
}
