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
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class TransferBreakdownStrategyTest : TransferBreakdownStrategyTest<TransferBreakdownStrategy>
	{
	}

	abstract class TransferBreakdownStrategyTest<T> : BreakdownStrategyTest<T>
		where T : TransferBreakdownStrategy, new()
	{
		public override void TestGetWorkflowInfo()
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, location1, location2);
				transferLine.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				AssertEquals("Should get the correct job name", transfer.HumanReadableName, workflowInfo.NameForLog);
				AssertEquals("Should get the correct branch", data.Whs1.WW_GB_RelatedCompanyBranch, workflowInfo.BranchPK);
				AssertEquals("Should get the correct warehouse", data.Whs1.PK, workflowInfo.WarehousePK);
				AssertEquals("Should get the correct release group", releaseGroup.PK, workflowInfo.ReleaseGroupPK);
				AssertEquals("Should get the correct workflow provider", transfer, workflowInfo.WorkflowProvider);
			}
		}

		protected override void TestSetJobPlanningStatus(string status)
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, location1, "");
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, location1, location2);
				transferLine.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);

				var strategy = GetStrategy();
				strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status);
				AssertEquals("Should have updated the job in the new factory.", status, transferInNewFactory.WD_TaskPlanningStatus);
				AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);
			}
		}

		public override void TestGetTasksToCreate()
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				Helper.SetClientAllAttributeType(data.Org1, false);
				Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
					useSerialNumber: false);
				Factory.Save();

				Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Core.Constants.PkgUnit.Pallet, 5m);
				Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, (ZString)Core.Constants.PkgUnit.Box)
					.F3_UOMType = UOMPackTypesList.Codes.Case;

				var today = ZDate.Today;
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
					allocateLocations: false, finalise: false);
				receive.WD_ArrivalDate = today.ToZDateTime().ToOffset();

				var inventoryLine = receive.Lines[0];
				inventoryLine.WE_WL = location1.PK;
				inventoryLine.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
				inventoryLine.WE_F3_NKPackType = Core.Constants.PkgUnit.Box;
				inventoryLine.WE_ClientOrderedUnits = 3m;
				inventoryLine.WE_TransactionQuantity = 5m;

				inventoryLine.WE_PalletID = "123";
				inventoryLine.WE_PartAttrib1 = "1";
				inventoryLine.WE_PartAttrib2 = "2";
				inventoryLine.WE_PartAttrib3 = "3";
				inventoryLine.WE_ExpiryDate = today.AddDays(20);
				inventoryLine.WE_PackingDate = today.AddDays(-10);

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLineFromInventoryHelper = new TransferLineFromInventoryHelper(transfer.NotificationSubscriber, transfer);
				transferLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(transfer.Lines, [inventoryLine.Inventory[0]]);
				var transferLine = transfer.Lines.Single();
				transferLine.WE_WL = location2.PK;
				transferLine.WE_PalletID = "456";
				transferLine.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				ITaskManagementContextFact context = null;
				ITaskManagementGroupingFact grouping = null;
				ITaskManagementTransferLineFact transferLineFact = null;

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
						context = facts.OfType<ITaskManagementContextFact>().Single();
						transferLineFact = facts.OfType<ITaskManagementTransferLineFact>().Single();
						grouping = transferLineFact.Grouping.Fact;
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
					AssertEquals(nameof(taskToCreate.FormflowType), ExpectedFormFlowType, taskToCreate.FormflowType);
					AssertEquals(nameof(taskToCreate.TaskType), "UDF", taskToCreate.TaskType);
					AssertEquals(nameof(taskToCreate.RawNudge), ExpectedNudge, taskToCreate.RawNudge);
					AssertEquals(nameof(taskToCreate.TaskName), ExpectedTaskAndWorkflowName, taskToCreate.TaskName);
					AssertEquals(nameof(taskToCreate.WorkflowName), ExpectedTaskAndWorkflowName, taskToCreate.WorkflowName);
					AssertEquals(nameof(taskToCreate.StaffCode), string.Empty, taskToCreate.StaffCode);
					AssertEquals(nameof(taskToCreate.CapabilityCode), "TEST", taskToCreate.CapabilityCode);
					AssertEquals(nameof(taskToCreate.ReleaseGroupPk), releaseGroup.PK, taskToCreate.ReleaseGroupPk);

					AssertNotNull("Should have created a context.", context);
					AssertEquals("Should *not* have set a fallback.", 0, context.MaxNumberOfLinesFallBack);

					AssertNotNull("Should have created a grouping.", grouping);
					AssertEquals("Should set number of lines.", 1, grouping.NumberOfLines);
					AssertEquals("Should set number of packs.", 1, grouping.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);

					AssertEquals(nameof(ITaskManagementTransferLineFact.PK), transferLine.PK, transferLineFact.PK);
					AssertEquals(nameof(ITaskManagementTransferLineFact.Client), receive.WD_OH_Client, transferLineFact.Client.Fact.PK);
					AssertEquals(nameof(ITaskManagementTransferLineFact.FromLocation), location1.PK, transferLineFact.FromLocation.Fact.PK);
					AssertEquals(nameof(ITaskManagementTransferLineFact.ToLocation), location2.PK, transferLineFact.ToLocation.Fact.PK);
					AssertEquals(nameof(ITaskManagementTransferLineFact.FromPalletID), "123", transferLineFact.FromPalletID);
					AssertEquals(nameof(ITaskManagementTransferLineFact.ToPalletID), "456", transferLineFact.ToPalletID);
					AssertEquals(nameof(ITaskManagementTransferLineFact.PackUQ), Core.Constants.PkgUnit.Box, transferLineFact.PackUQ);
					AssertEquals(nameof(ITaskManagementTransferLineFact.UOMType), UOMPackTypesList.Codes.Case, transferLineFact.UOMType);
					AssertEquals(nameof(ITaskManagementTransferLineFact.Reference), "T1 W00000002", transferLineFact.Reference);
					AssertEquals(nameof(ITaskManagementTransferLineFact.DocketID), "W00000002", transferLineFact.DocketID);
					AssertEquals(nameof(ITaskManagementTransferLineFact.HoldCode), "HEL", transferLineFact.HoldCode);
					AssertEquals(nameof(ITaskManagementTransferLineFact.PartAttribute1), "1", transferLineFact.PartAttribute1);
					AssertEquals(nameof(ITaskManagementTransferLineFact.PartAttribute2), "2", transferLineFact.PartAttribute2);
					AssertEquals(nameof(ITaskManagementTransferLineFact.PartAttribute3), "3", transferLineFact.PartAttribute3);
					AssertEquals(nameof(ITaskManagementTransferLineFact.ExpiryDate), inventoryLine.WE_ExpiryDate, transferLineFact.ExpiryDate);
					AssertEquals(nameof(ITaskManagementTransferLineFact.PackingDate), inventoryLine.WE_PackingDate, transferLineFact.PackingDate);
					AssertEquals(nameof(ITaskManagementTransferLineFact.ArrivalDate), receive.WD_ArrivalDate.ToDateTime(), transferLineFact.ArrivalDate);
					AssertEquals(nameof(ITaskManagementTransferLineFact.HasAwaitingPicks), false, transferLineFact.HasAwaitingPicks);

					var part1OwnerRelation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
					AssertEquals(nameof(ITaskManagementTransferLineFact.Product), part1OwnerRelation.PK, transferLineFact.Product.Fact.PK);
					AssertEquals(nameof(ITaskManagementTransferLineFact.Product), data.Part1.OP_PartNum, transferLineFact.Product.Fact.Code);
				}
			}
		}

		public void TestGetTasksToCreate_MultipleLines()
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
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 20m, location1);
				var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 20m, location2);

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1, location2);
				var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
				var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, location1, location2);
				var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, location2, location1);
				transferLine1.RunPreSaveValidation();
				transferLine2.RunPreSaveValidation();
				transferLine3.RunPreSaveValidation();
				transferLine4.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				IEnumerable<ITaskManagementTransferLineFact> transferLines = null;

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

					var clients = transferLines.Select(ul => ul.Client.Fact).Distinct().ToArray();
					AssertEquals("Should have created 1x client.", 1, clients.Length);

					var products = transferLines.Select(ul => ul.Product.Fact).Distinct().ToArray();
					AssertEquals("Should have created 2x products.", 2, products.Length);

					var locations = transferLines.
						Select(ul => ul.FromLocation.Fact)
						.Concat(transferLines.Select(ul => ul.ToLocation.Fact))
						.Distinct()
						.ToArray();
					AssertEquals("Should have created 2x locations.", 2, locations.Length);

					var line1Fact = transferLines.Single(l => l.PK == transferLine1.PK);
					var line2Fact = transferLines.Single(l => l.PK == transferLine2.PK);
					var line3Fact = transferLines.Single(l => l.PK == transferLine3.PK);
					var line4Fact = transferLines.Single(l => l.PK == transferLine4.PK);
					AssertEquals("Should share the same product.", line1Fact.Product.Fact, line2Fact.Product.Fact);
					AssertEquals("Should share the same product.", line3Fact.Product.Fact, line4Fact.Product.Fact);
					AssertNotEquals("Should *not* share the same product.", line1Fact.Product.Fact, line4Fact.Product.Fact);

					AssertEquals("Should share the same location.", line1Fact.FromLocation.Fact, line2Fact.ToLocation.Fact);
					AssertEquals("Should share the same location.", line1Fact.ToLocation.Fact, line2Fact.FromLocation.Fact);
					AssertNotEquals("Should *not* share the same location.", line1Fact.ToLocation.Fact, line1Fact.FromLocation.Fact);

					AssertEquals("Should share the same location.", line3Fact.FromLocation.Fact, line4Fact.ToLocation.Fact);
					AssertEquals("Should share the same location.", line3Fact.ToLocation.Fact, line4Fact.FromLocation.Fact);
					AssertNotEquals("Should *not* share the same location.", line3Fact.ToLocation.Fact, line3Fact.FromLocation.Fact);

					AssertEquals("Should share the same location.", line1Fact.FromLocation.Fact, line3Fact.FromLocation.Fact);
					AssertEquals("Should share the same location.", line1Fact.ToLocation.Fact, line3Fact.ToLocation.Fact);
					AssertEquals("Should share the same location.", line2Fact.FromLocation.Fact, line4Fact.FromLocation.Fact);
					AssertEquals("Should share the same location.", line2Fact.ToLocation.Fact, line4Fact.ToLocation.Fact);
				}
			}
		}

		public void TestGetTasksToCreate_MultipleLines_NoPalletID()
			=> TestGetTasksToCreate_MultipleLines_NoPalletID(hasDestinationPalletID: false);

		public void TestGetTasksToCreate_MultipleLines_NoPalletID_WithDestinationPalletID()
			=> TestGetTasksToCreate_MultipleLines_NoPalletID(hasDestinationPalletID: true);

		void TestGetTasksToCreate_MultipleLines_NoPalletID(bool hasDestinationPalletID)
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
					allocateLocations: false, finalise: false);
				var receiveLine1 = receive.Lines[0];
				receiveLine1.WE_WL = location1.PK;

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, string.Empty, location2.WLV_LocationString, hasDestinationPalletID ? "ABC" : string.Empty);
				var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, string.Empty, location2.WLV_LocationString, hasDestinationPalletID ? "ABC" : string.Empty);
				transferLine1.RunPreSaveValidation();
				transferLine2.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
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

					AssertEquals("Should have 2 lines.", 2, transferLines.Length);

					var grouping1 = transferLines[0].Grouping.Fact;
					AssertNotNull("Should have created a grouping.", grouping1);
					AssertEquals("Should set number of lines.", 1, grouping1.NumberOfLines);
					AssertEquals("Should set number of packs.", 0, grouping1.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, grouping1.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, grouping1.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, grouping1.Volume);

					var grouping2 = transferLines[1].Grouping.Fact;
					AssertNotNull("Should have created a grouping.", grouping2);
					AssertEquals("Should set number of lines.", 1, grouping2.NumberOfLines);
					AssertEquals("Should set number of packs.", 0, grouping2.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, grouping2.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, grouping2.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, grouping2.Volume);
				}
			}
		}

		public void TestGetTasksToCreate_MultipleLines_SamePallet()
			=> TestGetTasksToCreate_MultipleLines_SamePallet(emptyDestinationPalletID: false);

		public void TestGetTasksToCreate_MultipleLines_SamePallet_EmptyDestinationPalletID()
			=> TestGetTasksToCreate_MultipleLines_SamePallet(emptyDestinationPalletID: true);

		void TestGetTasksToCreate_MultipleLines_SamePallet(bool emptyDestinationPalletID)
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m,
					allocateLocations: false, finalise: false);
				var receiveLine1 = receive.Lines[0];
				receiveLine1.WE_WL = location1.PK;
				receiveLine1.WE_PalletID = "ABC";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "ABC", location2.WLV_LocationString, !emptyDestinationPalletID ? "ABC" : string.Empty);
				var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "ABC", location2.WLV_LocationString, !emptyDestinationPalletID ? "ABC" : string.Empty);
				var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "aBc", location2.WLV_LocationString, !emptyDestinationPalletID ? "ABC" : string.Empty);
				transferLine1.RunPreSaveValidation();
				transferLine2.RunPreSaveValidation();
				transferLine3.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
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

					AssertEquals("Should have 3 lines.", 3, transferLines.Length);
					var grouping = transferLines.Select(l => l.Grouping.Fact).Distinct().Single();
					AssertNotNull("Should have created a grouping.", grouping);
					AssertEquals("Should set number of lines.", 3, grouping.NumberOfLines);
					AssertEquals("Should set number of packs.", 1, grouping.NumberOfPacks);
					AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
					AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
					AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);
				}
			}
		}

		public void TestGetTasksToCreate_MultipleLines_ConsidersBothFromAndToPalletForGrouping()
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
					allocateLocations: false, finalise: false);
				var receiveLine1 = receive.Lines[0];
				receiveLine1.WE_WL = location1.PK;
				receiveLine1.WE_PalletID = "ABC";

				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
				receiveLine2.WE_PalletID = "DEF";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "ABC", location2.WLV_LocationString, "123");
				var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "ABC", location2.WLV_LocationString, "456");
				var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "DEF", location2.WLV_LocationString, "123");
				var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, "DEF", location2.WLV_LocationString, "456");
				transferLine1.RunPreSaveValidation();
				transferLine2.RunPreSaveValidation();
				transferLine3.RunPreSaveValidation();
				transferLine4.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
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

					AssertEquals("Should have 4 lines.", 4, transferLines.Length);

					var groupings = transferLines.Select(l => l.Grouping.Fact).Distinct().ToArray();
					AssertEquals("Should have 4 groupings.", 4, groupings.Length);

					foreach (var grouping in groupings)
					{
						AssertNotNull("Should have created a grouping.", grouping);
						AssertEquals("Should set number of lines.", 1, grouping.NumberOfLines);
						AssertEquals("Should set number of packs.", 1, grouping.NumberOfPacks);
						AssertEquals("Should *not* set other quantities.", 0, grouping.NumberOfUnits);
						AssertEquals("Should *not* set other quantities.", 0m, grouping.Weight);
						AssertEquals("Should *not* set other quantities.", 0m, grouping.Volume);
					}
				}
			}
		}

		public void TestGetTasksToCreate_EmptyToLocation()
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

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);

				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, string.Empty);
				transferLine1.RunPreSaveValidation();
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
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

					AssertEquals("Should have a valid from location.", location1.PK, transferLine.FromLocation.Fact.PK);
					AssertNull("Should have a null to location.", transferLine.ToLocation.Fact);
				}
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

		public void TestLinkTasks()
		{
			using (AutomaticallySetToPlanningRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m,
					allocateLocations: false, finalise: false);
				var receiveLine1 = receive.Lines[0];
				receiveLine1.WE_WL = location1.PK;

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				SetupTransfer(transfer);
				Factory.Save();

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1, location2);
				var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1, location2);
				var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1, location2);
				var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1, location2);
				transferLine1.RunPreSaveValidation();
				transferLine2.RunPreSaveValidation();
				transferLine3.RunPreSaveValidation();
				transferLine4.RunPreSaveValidation();
				Factory.Save();

				var taskPK1 = ZGuid.NewZGuid();
				var taskPK2 = ZGuid.NewZGuid();

				var task1 = transfer.WorkflowItems.AddNew();
				var task2 = transfer.WorkflowItems.AddNew();

				var taskLink1 = new Mock<ITaskManagementGroupingFact>();
				taskLink1.Setup(tl => tl.PK).Returns(Guid.NewGuid());
				taskLink1.Setup(tl => tl.AssignedTask).Returns(taskPK1.ToGuid());

				var taskLink2 = new Mock<ITaskManagementGroupingFact>();
				taskLink2.Setup(tl => tl.PK).Returns(Guid.NewGuid());
				taskLink2.Setup(tl => tl.AssignedTask).Returns(taskPK1.ToGuid());

				var taskLink3 = new Mock<ITaskManagementGroupingFact>();
				taskLink3.Setup(tl => tl.PK).Returns(Guid.NewGuid());
				taskLink3.Setup(tl => tl.AssignedTask).Returns(taskPK2.ToGuid());

				var taskLink4 = new Mock<ITaskManagementGroupingFact>();
				taskLink4.Setup(tl => tl.PK).Returns(Guid.NewGuid());
				taskLink4.Setup(tl => tl.AssignedTask).Returns(Guid.Empty);

				var lineFact1 = new Mock<ITaskManagementLineFact>();
				lineFact1.Setup(l => l.PK).Returns(transferLine1.PK.ToGuid());
				lineFact1.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink1.Object));

				var lineFact2 = new Mock<ITaskManagementLineFact>();
				lineFact2.Setup(l => l.PK).Returns(transferLine3.PK.ToGuid());
				lineFact2.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink2.Object));

				var lineFact3 = new Mock<ITaskManagementLineFact>();
				lineFact3.Setup(l => l.PK).Returns(transferLine2.PK.ToGuid());
				lineFact3.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink3.Object));

				var lineFact4 = new Mock<ITaskManagementLineFact>();
				lineFact4.Setup(l => l.PK).Returns(transferLine4.PK.ToGuid());
				lineFact4.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(taskLink4.Object));

				var taskDictionary = new Dictionary<ZGuid, ProcessTask>
				{
					{ taskPK1, task1 },
					{ taskPK2, task2 },
				};

				var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(transfer.PK);
				AssertNotNull("Precondition.", jobReadyForPlanning);

				var strategy = GetStrategy();

				strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object, lineFact2.Object, lineFact3.Object, lineFact4.Object]);
				AssertEquals("Should have linked the tasks.", task1.PK, transferLine1.WE_P9_Task);
				AssertEquals("Should have linked the tasks.", task2.PK, transferLine2.WE_P9_Task);
				AssertEquals("Should have linked the tasks.", task1.PK, transferLine3.WE_P9_Task);
				AssertEquals("Should *not* have linked tasks with no result.", ZGuid.Empty, transferLine4.WE_P9_Task);
			}
		}

		protected override string JobType => DocketType.Codes.Transfer;

		protected virtual string ExpectedFormFlowType => WarehouseTaskFormFlowTypes.TransferJob;

		protected virtual string ExpectedTaskAndWorkflowName => "Transfer";

		protected virtual ZInt ExpectedNudge => 100;

		protected virtual BooleanRegistryItem AutomaticallySetToPlanningRegistryItem
			=> WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning;

		protected virtual void SetupTransfer(WhsTransfer transfer)
		{
		}

		protected override T GetStrategy() => new T();
	}
}
