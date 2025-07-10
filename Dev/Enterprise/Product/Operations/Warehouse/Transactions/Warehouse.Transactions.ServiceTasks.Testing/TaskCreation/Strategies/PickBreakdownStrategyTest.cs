using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class PickBreakdownStrategyTest : BreakdownStrategyTest<PickBreakdownStrategy>
	{
		public override void TestGetWorkflowInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
			AssertEquals("Should get the correct job name", pick.HumanReadableName, workflowInfo.NameForLog);
			AssertEquals("Should get the correct branch", data.Whs1.WW_GB_RelatedCompanyBranch, workflowInfo.BranchPK);
			AssertEquals("Should get the correct warehouse", data.Whs1.PK, workflowInfo.WarehousePK);
			AssertEquals("Should get the correct release group", releaseGroup.PK, workflowInfo.ReleaseGroupPK);
			AssertEquals("Should get the correct workflow provider", pick, workflowInfo.WorkflowProvider);
		}

		protected override void TestSetJobPlanningStatus(string status)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			var strategy = GetStrategy();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			strategy.SetJobPlanningStatus(newFactory, jobReadyForPlanning, status);
			AssertEquals("Should have updated the job in the new factory.", status, pickInNewFactory.WP_TaskPlanningStatus);
			AssertEquals("Should *not* have updated the job in the original factory.", TaskPlanningStatus.Codes.Ready, pick.WP_TaskPlanningStatus);
		}

		public override void TestGetTasksToCreate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: false);
			Factory.Save();

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var palletId = "PAL123";
			var attrib1 = "ATTR1";
			var attrib2 = "ATTR2";
			var attrib3 = "ATTR3";
			var arrivalDate = new ZDateTimeOffset(2025, 5, 15);
			var expiryDate = new ZDate(2026, 6, 14);
			var packingDate = new ZDate(2025, 5, 10);
			var bondedEntryDate = new ZDate(2025, 5, 5);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", arrivalDate, data.Part1, 10m, finalise: false);
			var invLine = receive.Lines[0];
			invLine.WE_PalletID = palletId;
			invLine.WE_PartAttrib1 = attrib1;
			invLine.WE_PartAttrib2 = attrib2;
			invLine.WE_PartAttrib3 = attrib3;
			invLine.WE_ExpiryDate = expiryDate;
			invLine.WE_PackingDate = packingDate;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var split = pick.OrderedInventories[0].AvailableInventories[0].AvailableInventoriesSplitByPickedDetails[0];
			var splitPk = split.PK;

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			ITaskManagementPickLineFact pickLineFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFact = facts.OfType<ITaskManagementPickLineFact>().Single();

					var grouping = pickLineFact.Grouping.Fact;
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
				AssertEquals(nameof(taskToCreate.FormflowType), WarehouseTaskFormFlowTypes.PickJob, taskToCreate.FormflowType);
				AssertEquals(nameof(taskToCreate.TaskType), "UDF", taskToCreate.TaskType);
				AssertEquals(nameof(taskToCreate.RawNudge), new ZShort(200), taskToCreate.RawNudge);
				AssertEquals(nameof(taskToCreate.TaskName), "Pick Lines", taskToCreate.TaskName);
				AssertEquals(nameof(taskToCreate.WorkflowName), "Pick Lines", taskToCreate.WorkflowName);
				AssertEquals(nameof(taskToCreate.StaffCode), string.Empty, taskToCreate.StaffCode);
				AssertEquals(nameof(taskToCreate.CapabilityCode), "TEST", taskToCreate.CapabilityCode);
				AssertEquals(nameof(taskToCreate.ReleaseGroupPk), releaseGroup.PK, taskToCreate.ReleaseGroupPk);

				AssertNotNull("Should have created a context.", context);
				AssertEquals("Should *not* have set a fallback.", 0, context.MaxNumberOfLinesFallBack);

				AssertNotNull("Should have created pick line fact.", pickLineFact);
				AssertEquals("Should set client correctly", data.Org1.PK, pickLineFact.Client.Fact.PK);
				AssertEquals("Should set product correctly", data.Part1.OP_PartNum, pickLineFact.Product.Fact.Code);

				AssertEquals("Should set is work order flag correctly", false, pickLineFact.IsWorkOrder);
				AssertEquals("Should set is customs transaction flag correctly", order.IsCustomsTransaction, pickLineFact.IsCustomsTransaction);

				AssertEquals("Should set pack UQ correctly", "UNT", pickLineFact.PackUQ);
				AssertEquals("Should set pallet ID correctly", palletId, pickLineFact.PalletID);
				AssertEquals("Should set part attribute 1 correctly", attrib1, pickLineFact.PartAttribute1);
				AssertEquals("Should set part attribute 2 correctly", attrib2, pickLineFact.PartAttribute2);
				AssertEquals("Should set part attribute 3 correctly", attrib3, pickLineFact.PartAttribute3);
				AssertEquals("Should set serial number correctly", string.Empty, pickLineFact.SerialNumber);
				AssertEquals("Should set arrival date correctly", arrivalDate.ToDateTime(), pickLineFact.ArrivalDate);
				AssertEquals("Should set expiry date correctly", expiryDate.ToDateTime(), pickLineFact.ExpiryDate);
				AssertEquals("Should set packing date correctly", packingDate.ToDateTime(), pickLineFact.PackingDate);

				AssertEquals("Should set number of units correctly", 5, pickLineFact.NumberOfUnits);
				AssertEquals("Should set number of packs correctly", 5, pickLineFact.NumberOfPacks);
				AssertEquals("Should set number of lines correctly", 1, pickLineFact.NumberOfLines);
				AssertEquals("Should set weight correctly", 10m, pickLineFact.Weight);
				AssertEquals("Should set weight UQ correctly", "KG", pickLineFact.WeightUQ);
				AssertEquals("Should set volume correctly", 0.10m, pickLineFact.Volume);
				AssertEquals("Should set volume UQ correctly", "M3", pickLineFact.VolumeUQ);

				AssertEquals("Should have set the PK.", splitPk, ((ITaskManagementLineFact)pickLineFact).PK);
				AssertEquals("Should have set the PK.", splitPk, ((ITaskManagemementTaskLink)pickLineFact).PK);
				AssertEquals("Should have set the PK.", splitPk, pickLineFact.AvailableInventorySplitPK);

				var hasCachedSplits = jobReadyForPlanning.Factory.TryGetValueFromCacheOnly<IDictionary<Guid, WhsPickAvailableInventorySplitBase>>($"{nameof(PickBreakdownStrategy)}_AvailableInventorySplits", out var splitMap);
				AssertEquals("Should have cached the splits.", true, hasCachedSplits);
				AssertEquals("Should have cached the correct number of splits.", 1, splitMap.Count);
				AssertEquals("Should have cached the split with the correct PK.", split, splitMap[splitPk.ToGuid()]);
			}
		}

		public void TestGetTasksToCreate_PickByUOMEnabled() => TestGetTasksToCreate_PickByUOMEnabled(cartoniseSplitCase: false, pickPalletsByLabel: false, pickCasesByLabel: false);
		public void TestGetTasksToCreate_PickByUOMEnabled_UsingCartonization() => TestGetTasksToCreate_PickByUOMEnabled(cartoniseSplitCase: true, pickPalletsByLabel: false, pickCasesByLabel: false);
		public void TestGetTasksToCreate_PickByUOMEnabled_UsingPickPalletsByLabel() => TestGetTasksToCreate_PickByUOMEnabled(cartoniseSplitCase: false, pickPalletsByLabel: true, pickCasesByLabel: false);
		public void TestGetTasksToCreate_PickByUOMEnabled_UsingPickCasesByLabel() => TestGetTasksToCreate_PickByUOMEnabled(cartoniseSplitCase: false, pickPalletsByLabel: false, pickCasesByLabel: true);
		public void TestGetTasksToCreate_PickByUOMEnabled_UsingCartonizationAndPickByLabel() => TestGetTasksToCreate_PickByUOMEnabled(cartoniseSplitCase: true, pickPalletsByLabel: true, pickCasesByLabel: true);

		void TestGetTasksToCreate_PickByUOMEnabled(bool cartoniseSplitCase, bool pickPalletsByLabel, bool pickCasesByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Pallet)).F3_UOMType = UOMPackTypesList.Codes.Pallet;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 10m);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 3m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 37m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = cartoniseSplitCase;
			pick.WP_PickPalletsByLabel = pickPalletsByLabel;
			pick.WP_PickCasesByLabel = pickCasesByLabel;
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var availableInventorySplitByUOM = pick.OrderedInventories[0].AvailableInventories[0].AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().ToArray();
			AssertEquals("Should have created 3 splits.", 3, availableInventorySplitByUOM.Length);

			var pltSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.Pallet);
			var casSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.Case);
			var spcSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.SplitCase);
			AssertEquals("Pallet split should have 3 packs.", 3m, pltSplit.PackQuantity);
			AssertEquals("Case split should have 3 packs.", 2m, casSplit.PackQuantity);
			AssertEquals("Pallet split should have 3 packs.", 1m, spcSplit.PackQuantity);

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			IEnumerable<ITaskManagementPickLineFact> pickLineFacts = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFacts = facts.OfType<ITaskManagementPickLineFact>().ToArray();

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var expectedLines = 0;
				if (!cartoniseSplitCase)
				{
					var splitCasePickLineFact = pickLineFacts.Single(pl => pl.UOMType == UOMPackTypesList.Codes.SplitCase);
					AssertEquals("Should have set the PK.", spcSplit.PK, ((ITaskManagementLineFact)splitCasePickLineFact).PK);
					AssertEquals("Should have set the PK.", spcSplit.PK, ((ITaskManagemementTaskLink)splitCasePickLineFact).PK);
					AssertEquals("Should have set the PK.", spcSplit.PK, splitCasePickLineFact.AvailableInventorySplitPK);
					AssertEquals("Should have the correct pack type.", Constants.PkgUnit.Unit, splitCasePickLineFact.PackUQ);
					AssertEquals("Should set the ceiling of number of units correctly", 1, splitCasePickLineFact.NumberOfUnits);
					AssertEquals("Should set the ceiling of number of packs correctly", 1, splitCasePickLineFact.NumberOfPacks);
					AssertEquals("Should set number of lines correctly", 1, splitCasePickLineFact.NumberOfLines);
					AssertEquals("Should set weight correctly", 2m, splitCasePickLineFact.Weight);
					AssertEquals("Should set weight UQ correctly", "KG", splitCasePickLineFact.WeightUQ);
					AssertEquals("Should set volume correctly", 0.02m, splitCasePickLineFact.Volume);
					AssertEquals("Should set volume UQ correctly", "M3", splitCasePickLineFact.VolumeUQ);

					expectedLines++;
				}

				if (!pickCasesByLabel)
				{
					var casePickLineFact = pickLineFacts.Single(pl => pl.UOMType == UOMPackTypesList.Codes.Case);
					AssertEquals("Should have set the PK.", casSplit.PK, ((ITaskManagementLineFact)casePickLineFact).PK);
					AssertEquals("Should have set the PK.", casSplit.PK, ((ITaskManagemementTaskLink)casePickLineFact).PK);
					AssertEquals("Should have set the PK.", casSplit.PK, casePickLineFact.AvailableInventorySplitPK);
					AssertEquals("Should have the correct pack type.", Constants.PkgUnit.Box, casePickLineFact.PackUQ);
					AssertEquals("Should set the ceiling of number of units correctly", 6, casePickLineFact.NumberOfUnits);
					AssertEquals("Should set the ceiling of number of packs correctly", 2, casePickLineFact.NumberOfPacks);
					AssertEquals("Should set number of lines correctly", 1, casePickLineFact.NumberOfLines);
					AssertEquals("Should set weight correctly", 6 * 2m, casePickLineFact.Weight);
					AssertEquals("Should set weight UQ correctly", "KG", casePickLineFact.WeightUQ);
					AssertEquals("Should set volume correctly", 6 * 0.02m, casePickLineFact.Volume);
					AssertEquals("Should set volume UQ correctly", "M3", casePickLineFact.VolumeUQ);

					expectedLines++;
				}

				if (!pickPalletsByLabel)
				{
					var palletPickLineFact = pickLineFacts.Single(pl => pl.UOMType == UOMPackTypesList.Codes.Pallet);
					AssertEquals("Should have set the PK.", pltSplit.PK, ((ITaskManagementLineFact)palletPickLineFact).PK);
					AssertEquals("Should have set the PK.", pltSplit.PK, ((ITaskManagemementTaskLink)palletPickLineFact).PK);
					AssertEquals("Should have set the PK.", pltSplit.PK, palletPickLineFact.AvailableInventorySplitPK);
					AssertEquals("Should have the correct pack type.", Constants.PkgUnit.Pallet, palletPickLineFact.PackUQ);
					AssertEquals("Should set the ceiling of number of units correctly", 30, palletPickLineFact.NumberOfUnits);
					AssertEquals("Should set the ceiling of number of packs correctly", 3, palletPickLineFact.NumberOfPacks);
					AssertEquals("Should set number of lines correctly", 1, palletPickLineFact.NumberOfLines);
					AssertEquals("Should set weight correctly", 30 * 2m, palletPickLineFact.Weight);
					AssertEquals("Should set weight UQ correctly", "KG", palletPickLineFact.WeightUQ);
					AssertEquals("Should set volume correctly", 30 * 0.02m, palletPickLineFact.Volume);
					AssertEquals("Should set volume UQ correctly", "M3", palletPickLineFact.VolumeUQ);

					expectedLines++;
				}

				AssertEquals("Should have returned the correct number of lines.", expectedLines, pickLineFacts.Count());
			}
		}

		public void TestGetTasksToCreate_PickByUOMEnabled_WeightAndVolumeOnConversion() => TestGetTasksToCreate_PickByUOMEnabled_WeightOrVolumeOnConversion(weight: 5m, volume: 3m);
		public void TestGetTasksToCreate_PickByUOMEnabled_WeightOnlyOnConversion() => TestGetTasksToCreate_PickByUOMEnabled_WeightOrVolumeOnConversion(weight: 5m, volume: 0m);
		public void TestGetTasksToCreate_PickByUOMEnabled_VolumeOnlyOnConversion() => TestGetTasksToCreate_PickByUOMEnabled_WeightOrVolumeOnConversion(weight: 0m, volume: 3m);

		void TestGetTasksToCreate_PickByUOMEnabled_WeightOrVolumeOnConversion(decimal weight, decimal volume)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Pallet)).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var conversion = Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 10m);
			conversion.OF_Weight = weight;
			conversion.OF_Cubic = volume;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var availableInventorySplitByUOM = pick.OrderedInventories[0].AvailableInventories[0].AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().ToArray();
			AssertEquals("Should have created 1 split.", 1, availableInventorySplitByUOM.Length);

			var pltSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.Pallet);
			AssertEquals("Pallet split should have 2 packs.", 2m, pltSplit.PackQuantity);

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			ITaskManagementPickLineFact pickLineFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFact = facts.OfType<ITaskManagementPickLineFact>().Single();

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var usingConversion = weight > 0 || volume > 0; // Assume conversion entry is all or nothing, as with existing places
				var weightToAssert = weight > 0 || volume > 0 ? weight : (decimal)data.Part1.OP_Weight;
				var volumeToAssert = weight > 0 || volume > 0 ? volume : (decimal)data.Part1.OP_Cubic;
				AssertEquals("Should have the correct pack type.", Constants.PkgUnit.Pallet, pickLineFact.PackUQ);
				AssertEquals("Should set the ceiling of number of units correctly", 20, pickLineFact.NumberOfUnits);
				AssertEquals("Should set the ceiling of number of packs correctly", 2, pickLineFact.NumberOfPacks);
				AssertEquals("Should set number of lines correctly", 1, pickLineFact.NumberOfLines);
				AssertEquals("Should set weight correctly", 2 * weightToAssert, pickLineFact.Weight);
				AssertEquals("Should set weight UQ correctly", "KG", pickLineFact.WeightUQ);
				AssertEquals("Should set volume correctly", 2 * volumeToAssert, pickLineFact.Volume);
				AssertEquals("Should set volume UQ correctly", "M3", pickLineFact.VolumeUQ);
			}
		}

		public void TestGetTasksToCreate_Picked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementPickLineFact pickLineFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					pickLineFact = facts.OfType<ITaskManagementPickLineFact>().SingleOrDefault();

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertNull("Should have filtered out picked available inventory splits.", pickLineFact);
			}
		}

		public void TestGetTasksToCreate_Split()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementPickLineFact pickLineFact = null;
			ITaskManagementPickLineFact splitFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					pickLineFact = facts.OfType<ITaskManagementPickLineFact>().SingleOrDefault();
					splitFact = (ITaskManagementPickLineFact)pickLineFact.Split(5);

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Append(splitFact).Append(resultFact));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should include the original fact in the result object.", true, tasksToCreate.Lines.Contains(pickLineFact));
				AssertEquals("Should include the split fact in the result object.", true, tasksToCreate.Lines.Contains(splitFact));
			}
		}

		public void TestGetTasksToCreate_FractionalQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1.4m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			ITaskManagementPickLineFact pickLineFact = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFact = facts.OfType<ITaskManagementPickLineFact>().Single();

					var grouping = pickLineFact.Grouping.Fact;
					grouping.AssignedTask = resultFact.PK;

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				AssertEquals("Should take the ceiling of number of units", 2, pickLineFact.NumberOfUnits);
				AssertEquals("Should take the ceiling of number of packs", 2, pickLineFact.NumberOfPacks);
				AssertEquals("Should set number of lines correctly", 1, pickLineFact.NumberOfLines);
				AssertEquals("Should set weight correctly", 1.4m * 2m, pickLineFact.Weight);
				AssertEquals("Should set weight UQ correctly", "KG", pickLineFact.WeightUQ);
				AssertEquals("Should set volume correctly", 1.4m * 0.02m, pickLineFact.Volume);
				AssertEquals("Should set volume UQ correctly", "M3", pickLineFact.VolumeUQ);
			}
		}

		public void TestGetTasksToCreate_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			IEnumerable<ITaskManagementPickLineFact> pickLineFacts = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFacts = facts.OfType<ITaskManagementPickLineFact>().ToArray();

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var pickLineProduct1Fact = pickLineFacts.SingleOrDefault(f => f.Product.Fact.Code == data.Part1.OP_PartNum);
				var pickLineProduct2Fact = pickLineFacts.SingleOrDefault(f => f.Product.Fact.Code == data.Part2.OP_PartNum);
				AssertNotNull("Should have created a pick line fact for product 1.", pickLineProduct1Fact);
				AssertNotNull("Should have created a pick line fact for product 2.", pickLineProduct2Fact);
			}
		}

		public void TestGetTasksToCreate_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			IEnumerable<ITaskManagementPickLineFact> pickLineFacts = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFacts = facts.OfType<ITaskManagementPickLineFact>().ToArray();

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var pickLineClient1Fact = pickLineFacts.SingleOrDefault(f => f.Client.Fact.PK == data.Org1.PK);
				var pickLineClient2Fact = pickLineFacts.SingleOrDefault(f => f.Client.Fact.PK == client2.PK);
				AssertNotNull("Should have created a pick line fact for client 1.", pickLineClient1Fact);
				AssertNotNull("Should have created a pick line fact for client 2.", pickLineClient2Fact);
				AssertNotEquals("Should *not* have the same product in both facts as we base product on the product relation.", pickLineClient1Fact.Product.Fact, pickLineClient2Fact.Product.Fact);
				AssertEquals("Precondition: Same product code.", pickLineClient1Fact.Product.Fact.Code, pickLineClient2Fact.Product.Fact.Code);
			}
		}

		public void TestGetTasksToCreate_MultipleLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, location1, string.Empty);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, location2, string.Empty);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);

			ITaskManagementContextFact context = null;
			IEnumerable<ITaskManagementPickLineFact> pickLineFacts = null;

			var resultFact = new TaskResultFact("TEST");

			var token = new CancellationToken();
			var rulesEngine = new Mock<IProductionRulesEnginePushService>();
			rulesEngine.Setup(re => re.RunRulesEngine(
				RulesContextType.ProductWarehouseTaskBreakdown,
				RulesContextSubType.ProductWarehousePickLine,
				It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == data.Whs1.PK),
				It.IsAny<IEnumerable<IInputFact>>(),
				token))
				.Returns<RulesContextType, RulesContextSubType, ProductionRuleSetFilter, IEnumerable<IInputFact>, CancellationToken>(
				(_, _, _, facts, _) =>
				{
					context = facts.OfType<ITaskManagementContextFact>().Single();
					pickLineFacts = facts.OfType<ITaskManagementPickLineFact>().ToArray();

					return new ProductionRulesEngineResult(facts.Cast<IFact>().Concat([resultFact]));
				});

			using (ObjectFactory.Substitute(rulesEngine.Object))
			{
				var strategy = GetStrategy();
				var workflowInfo = strategy.GetWorkflowInfo(jobReadyForPlanning);
				var tasksToCreate = strategy.GetTasksToCreate(jobReadyForPlanning, workflowInfo, token);
				AssertEquals("Should try to create 1 task.", 1, tasksToCreate.Tasks.Count());

				var pickLineLocation1Fact = pickLineFacts.SingleOrDefault(f => f.Location.Fact.PK == location1.PK);
				var pickLineLocation2Fact = pickLineFacts.SingleOrDefault(f => f.Location.Fact.PK == location2.PK);
				AssertNotNull("Should have created a pick line fact for client 1.", pickLineLocation1Fact);
				AssertNotNull("Should have created a pick line fact for client 2.", pickLineLocation2Fact);
				AssertEquals("Should have the same product in both facts.", pickLineLocation1Fact.Product.Fact, pickLineLocation2Fact.Product.Fact);
				AssertEquals("Should have the same client in both facts.", pickLineLocation1Fact.Client.Fact, pickLineLocation2Fact.Client.Fact);
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

		public void TestLinkTasks() => TestLinkTasks(isPickByUOM: false);
		public void TestLinkTasks_IsPickByUOM() => TestLinkTasks(isPickByUOM: true);

		void TestLinkTasks(bool isPickByUOM)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_IsPickByUOMEnabled = isPickByUOM;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, string.Empty);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location2, string.Empty);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location3, string.Empty);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m, location4, string.Empty);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 40m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			PopulateCacheForLinkTasks(pick);
			Factory.Save();

			var orderedInv = pick.OrderedInventories[0];

			var availableInventory1 = orderedInv.AvailableInventories[0];
			var availableInventory2 = orderedInv.AvailableInventories[1];
			var availableInventory3 = orderedInv.AvailableInventories[2];
			var availableInventory4 = orderedInv.AvailableInventories[3];

			var availableInventorySplit1 = availableInventory1.AvailableInventoriesSplit.Single();
			var availableInventorySplit2 = availableInventory2.AvailableInventoriesSplit.Single();
			var availableInventorySplit3 = availableInventory3.AvailableInventoriesSplit.Single();
			var availableInventorySplit4 = availableInventory4.AvailableInventoriesSplit.Single();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var taskPK1 = ZGuid.NewZGuid();
			var taskPK2 = ZGuid.NewZGuid();

			var task1 = pick.WorkflowItems.AddNew();
			var task2 = pick.WorkflowItems.AddNew();

			var lineFact1 = new Mock<ITaskManagementPickLineFact>();
			lineFact1.Setup(l => l.NumberOfUnits).Returns(10);
			lineFact1.Setup(l => l.Quantity).Returns(10);
			lineFact1.Setup(l => l.AssignedTask).Returns(taskPK1.ToGuid());
			lineFact1.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit1.PK.ToGuid());

			var lineFact2 = new Mock<ITaskManagementPickLineFact>();
			lineFact2.Setup(l => l.NumberOfUnits).Returns(10);
			lineFact2.Setup(l => l.Quantity).Returns(10);
			lineFact2.Setup(l => l.AssignedTask).Returns(taskPK1.ToGuid());
			lineFact2.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit3.PK.ToGuid());

			var lineFact3 = new Mock<ITaskManagementPickLineFact>();
			lineFact3.Setup(l => l.NumberOfUnits).Returns(10);
			lineFact3.Setup(l => l.Quantity).Returns(10);
			lineFact3.Setup(l => l.AssignedTask).Returns(taskPK2.ToGuid());
			lineFact3.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit2.PK.ToGuid());

			var lineFact4 = new Mock<ITaskManagementPickLineFact>();
			lineFact4.Setup(l => l.NumberOfUnits).Returns(10);
			lineFact4.Setup(l => l.Quantity).Returns(10);
			lineFact4.Setup(l => l.AssignedTask).Returns(Guid.Empty);
			lineFact4.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit4.PK.ToGuid());

			var taskDictionary = new Dictionary<ZGuid, ProcessTask>
				{
					{ taskPK1, task1 },
					{ taskPK2, task2 },
				};

			var strategy = GetStrategy();

			strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object, lineFact2.Object, lineFact3.Object, lineFact4.Object]);
			AssertEquals("Should have linked the tasks.", task1.PK, availableInventory1.PickLines.Single().WZ_P9_Task);
			AssertEquals("Should have linked the tasks.", task2.PK, availableInventory2.PickLines.Single().WZ_P9_Task);
			AssertEquals("Should have linked the tasks.", task1.PK, availableInventory3.PickLines.Single().WZ_P9_Task);
			AssertEquals("Should *not* have linked tasks with no result.", ZGuid.Empty, availableInventory4.PickLines.Single().WZ_P9_Task);
		}

		public void TestLinkTasks_NoCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var orderedInv = pick.OrderedInventories[0];
			var availableInventory1 = orderedInv.AvailableInventories[0];
			var availableInventorySplit1 = availableInventory1.AvailableInventoriesSplit.Single();
			AssertEquals("Precondition: Single pick line.", 1, availableInventory1.PickLines.Count());

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var taskPK1 = ZGuid.NewZGuid();
			var taskPK2 = ZGuid.NewZGuid();

			var task1 = pick.WorkflowItems.AddNew();
			var task2 = pick.WorkflowItems.AddNew();

			var lineFact1 = new Mock<ITaskManagementPickLineFact>();
			lineFact1.Setup(l => l.NumberOfUnits).Returns(10);
			lineFact1.Setup(l => l.Quantity).Returns(10);
			lineFact1.Setup(l => l.AssignedTask).Returns(taskPK1.ToGuid());
			lineFact1.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit1.PK.ToGuid());

			var taskDictionary = new Dictionary<ZGuid, ProcessTask>
				{
					{ taskPK1, task1 },
					{ taskPK2, task2 },
				};

			var strategy = GetStrategy();

			strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object]);
			AssertEquals("Should *not* have linked the tasks.", ZGuid.Empty, availableInventory1.PickLines.Single().WZ_P9_Task);
		}

		public void TestLinkTasks_Split()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			PopulateCacheForLinkTasks(pick);
			Factory.Save();

			var orderedInv = pick.OrderedInventories[0];
			var availableInventory1 = orderedInv.AvailableInventories[0];
			var availableInventorySplit1 = availableInventory1.AvailableInventoriesSplit.Single();
			AssertEquals("Precondition: Single pick line.", 1, availableInventory1.PickLines.Count());

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var taskPK1 = ZGuid.NewZGuid();
			var taskPK2 = ZGuid.NewZGuid();

			var task1 = pick.WorkflowItems.AddNew();
			var task2 = pick.WorkflowItems.AddNew();

			var lineFact1 = new Mock<ITaskManagementPickLineFact>();
			lineFact1.Setup(l => l.NumberOfUnits).Returns(4);
			lineFact1.Setup(l => l.Quantity).Returns(4);
			lineFact1.Setup(l => l.AssignedTask).Returns(taskPK1.ToGuid());
			lineFact1.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit1.PK.ToGuid());

			var lineFact2 = new Mock<ITaskManagementPickLineFact>();
			lineFact2.Setup(l => l.NumberOfUnits).Returns(6);
			lineFact2.Setup(l => l.Quantity).Returns(6);
			lineFact2.Setup(l => l.AssignedTask).Returns(taskPK2.ToGuid());
			lineFact2.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit1.PK.ToGuid());

			var taskDictionary = new Dictionary<ZGuid, ProcessTask>
				{
					{ taskPK1, task1 },
					{ taskPK2, task2 },
				};

			var strategy = GetStrategy();

			strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object, lineFact2.Object]);
			AssertEquals("Should have split the tasks.", 2, availableInventory1.PickLines.Count());
			AssertContainsExactElementsInAnyOrder("Should have linked the tasks.", [task1.PK, task2.PK], availableInventory1.PickLines.Select(pl => pl.WZ_P9_Task));
		}

		public void TestTestLinkTasks_FractionalQuantity() => TestLinkTasks_FractionalQuantity(greaterThanOne: false, twoTasks: false);
		public void TestTestLinkTasks_FractionalQuantity_GreaterThanOne() => TestLinkTasks_FractionalQuantity(greaterThanOne: true, twoTasks: false);
		public void TestTestLinkTasks_FractionalQuantity_GreaterThanOne_WithTwoTasks() => TestLinkTasks_FractionalQuantity(greaterThanOne: true, twoTasks: true);

		void TestLinkTasks_FractionalQuantity(bool greaterThanOne, bool twoTasks)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var quantity = greaterThanOne ? 1.23m : 0.42m;
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, quantity);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			PopulateCacheForLinkTasks(pick);
			Factory.Save();

			var orderedInv = pick.OrderedInventories[0];
			var availableInventory1 = orderedInv.AvailableInventories[0];
			var availableInventorySplit1 = availableInventory1.AvailableInventoriesSplit.Single();
			var pickLine = availableInventory1.PickLines.Single();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var taskPK1 = ZGuid.NewZGuid();

			var task1 = pick.WorkflowItems.AddNew();
			var lineFact1 = new Mock<ITaskManagementPickLineFact>();
			lineFact1.Setup(l => l.Quantity).Returns(quantity);
			lineFact1.Setup(l => l.NumberOfUnits).Returns((int)Math.Ceiling(quantity));
			lineFact1.Setup(l => l.AssignedTask).Returns(taskPK1.ToGuid());
			lineFact1.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit1.PK.ToGuid());

			var taskDictionary = new Dictionary<ZGuid, ProcessTask>
				{
					{ taskPK1, task1 },
				};

			ProcessTask task2 = null;
			if (twoTasks)
			{
				var taskPK2 = ZGuid.NewZGuid();
				task2 = pick.WorkflowItems.AddNew();
				var lineFact2 = new Mock<ITaskManagementPickLineFact>();
				lineFact2.Setup(l => l.NumberOfUnits).Returns(1);
				lineFact2.Setup(l => l.AssignedTask).Returns(taskPK2.ToGuid());
				lineFact2.Setup(l => l.AvailableInventorySplitPK).Returns(availableInventorySplit1.PK.ToGuid());
				taskDictionary.Add(taskPK2, task2);
			}

			var strategy = GetStrategy();

			strategy.LinkTasks(jobReadyForPlanning, taskDictionary, [lineFact1.Object]);
			AssertEquals("Should have linked the task.", task1.PK, pickLine.WZ_P9_Task);
			AssertEquals("Should have retained the fractional quantity.", quantity, pickLine.WZ_Units);
		}

		static void PopulateCacheForLinkTasks(WhsPick pick)
		{
			var availableInventorySplitMap =
				pick.OrderedInventories
				.Cast<WhsPickOrderedInventory>()
				.SelectMany(ordInv => ordInv.AvailableInventories)
				.Cast<WhsPickAvailableInventory>()
				.SelectMany(availInv => availInv.AvailableInventoriesSplit)
				.ToDictionary(l => l.PK.ToGuid());

			pick.Factory.GetCachedValue<IDictionary<Guid, WhsPickAvailableInventorySplitBase>>($"{nameof(PickBreakdownStrategy)}_AvailableInventorySplits", () => availableInventorySplitMap);
		}

		protected override string JobType => "PIC";

		protected override PickBreakdownStrategy GetStrategy() => new PickBreakdownStrategy();
	}
}
