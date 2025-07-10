using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrder))]
	class WhsDynamicWorkOrderTest : WhsComponentOrderTest<WhsDynamicWorkOrder>
	{
		#region TestSetDefaultValues

		protected override void TestSetDefaultValuesCore(WhsDynamicWorkOrder docket)
		{
			base.TestSetDefaultValuesCore(docket);
			AssertEquals(DocketType.Codes.DynamicWorkOrder, docket.WD_DocketType);
			AssertEquals(WorkOrderType.Codes.Assemble, docket.WD_DocketSubType);
			AssertEquals(true, docket.WD_AutoFinaliseBOMIntoInventory);
			AssertEquals(true, docket.WD_IsInwardsProcessingJob);
		}

		#endregion

		#region Consignee

		protected override void TestConsigneeNameOrPKReadOnlyCore()
		{
			// Dynamic Work Order has no Consignee
			Assert(true);
		}

		protected override void TestConsigneeDocAddress_ReadOnlyCore(bool isAllowed)
		{
			var workOrder = GetNewBusinessObject();
			_ = workOrder.ConsigneeDocAddress;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);
		}

		#endregion

		#region Fetch Strategy

		protected override Type FetchStrategyType
			=> typeof(WhsDynamicWorkOrderFetchStrategy);

		#endregion

		#region TransportCoDocAddress

		public void TestTransportCoDocAddress_ReadOnly()
		{
			var workOrder = GetNewBusinessObject();
			_ = workOrder.TransportCoDocAddress;
			AssertEquals(true, workOrder.TransportCoDocAddress.ReadOnly);
		}

		#endregion

		#region TestDescription

		protected override ZString ExpectedDescription => "Dynamic Work Order";

		#endregion

		#region TestIRelatedJob

		protected override ControllerID ExpectedControllerId => ControllerIDs.WhsDynamicWorkOrder;

		#endregion

		#region IDocManagerSupport Members

		public override void TestDocManagerInfo()
		{
			var docket = GetNewBusinessObject();
			var info = docket.DocManagerInfo;
			AssertEquals(docket, info.BusinessEntity);
			AssertEquals("WDO", info.DocManagerCode);
		}

		#endregion

		#region Lines

		public void TestLinesToAssemble()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine1 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part1, 10m);
			var parentLine2 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part1, 8m);
			var childLine1 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part2, 7m);
			var childLine2 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part2, 13m);
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;
			childLine2.WE_WE_ParentDocketLine = parentLine1.PK;

			var childLine3 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part2, 8m);
			childLine3.WE_WE_ParentDocketLine = parentLine2.PK;

			AssertContainsExactElementsInAnyOrder(new[] { parentLine1, parentLine2 }, workOrder.Lines);
		}

		public void TestAllLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine1 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part1, 10m);
			var parentLine2 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part1, 8m);
			var childLine1 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part2, 7m);
			var childLine2 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part2, 13m);
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;
			childLine2.WE_WE_ParentDocketLine = parentLine1.PK;

			var childLine3 = Helper.CreateWhsPickableDocketLine(workOrder, data.Part2, 8m);
			childLine3.WE_WE_ParentDocketLine = parentLine2.PK;

			AssertContainsExactElementsInAnyOrder(new[] { parentLine1, parentLine2, childLine1, childLine2, childLine3 }, workOrder.AllLines);
		}

		#endregion

		#region Shortfall

		protected override bool UsesShortfall => false;

		#endregion

		#region TestPicking_AllocationWithNoMatchingAllocationKey_Disassemble

		public void TestPicking_AllocationWithNoMatchingAllocationKey_Disassemble()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D1");
			dynamicWorkOrder1.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			var component = Helper.CreateProduct("P1", dynamicWorkOrder1.Client);
			var mainProduct = Helper.CreateProduct("P2", dynamicWorkOrder1.Client);
			CreateIPRReceiveWithInventory(dynamicWorkOrder1.Client, dynamicWorkOrder1.Warehouse, "R1", component, 10m, "Ent-1");
			Factory.Save();

			var parentLine1 = dynamicWorkOrder1.Lines.AddNew();
			parentLine1.WE_OP = mainProduct.PK;
			parentLine1.WE_TransactionQuantity = 5m;
			parentLine1.IsMainInwardProcessedItem = true;

			var childLine1 = dynamicWorkOrder1.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 10m;
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder1);
			AssertIsFinalisedPrecondition(dynamicWorkOrder1.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var receiveLines = dynamicWorkOrder1.Receive.Lines;

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder2.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder2.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = mainProduct.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;
			dynamicWorkOrderLine1.WE_AllocationKey = receiveLines[0].WE_AllocationKey;

			var dynamicWorkOrderLine2 = dynamicWorkOrder2.Lines.AddNew();
			dynamicWorkOrderLine2.WE_OP = mainProduct.PK;
			dynamicWorkOrderLine2.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine2.IsMainInwardProcessedItem = true;
			dynamicWorkOrderLine2.WE_AllocationKey = "ABC";

			var pick2 = Helper.CreatePickNew(dynamicWorkOrder2);
			pick2.AutoAllocateItemsWithMock();
			var pickLine1 = dynamicWorkOrderLine1.PickLines.Single();
			AssertEquals("Should attempt to pick matching AllocationKey.", receiveLines[0].PK, pickLine1.WZ_WE_InventoryLine);

			AssertEquals("Should have no pickLines due to no matching inventory.", 0, dynamicWorkOrderLine2.PickLines.Count);
		}

		#endregion

		#region TestFinalisation

		protected override WhsDynamicWorkOrder SetupForTestFinaliseDocket()
		{
			var workOrder = base.SetupForTestFinaliseDocket();
			var whs = workOrder.Warehouse;
			SetupWarehouse(whs);

			var part1 = Helper.CreateProduct("P1", workOrder.Client);
			var part2 = Helper.CreateProduct("P2", workOrder.Client);
			CreateIPRReceiveWithInventory(workOrder.Client, workOrder.Warehouse, "R1", part1, 10m, "Ent-1");
			Factory.Save();

			var parentLine = workOrder.Lines.AddNew();
			parentLine.WE_OP = part2.PK;
			parentLine.WE_TransactionQuantity = 5m;
			parentLine.IsMainInwardProcessedItem = true;

			var childLine = workOrder.Lines.AddNew();
			childLine.WE_OP = part1.PK;
			childLine.WE_TransactionQuantity = 10m;
			childLine.WE_WE_ParentDocketLine = parentLine.PK;

			Helper.CreatePickNew(workOrder);

			return workOrder;
		}

		#region TestFinalisationForDissassembly

		public void TestFinalisationForDissassembly_NoInventory()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct("P1", client);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = product.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 1m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			AssertEquals("Pick failed due to shortfall.", true, dynamicWorkOrder.IsAttachedToPickButNotFinalised);
			Assert("OrderLine should have no PickLines.", !(dynamicWorkOrderLine1.PickLines.Count > 0));

			dynamicWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			Assert(dynamicWorkOrder.RowErrors.Any(e => e.Message == "Dynamic Work Order cannot be finalized until some inventory is allocated."));
		}

		public void TestFinalisationForDissassembly_NoInventoryWithComponentLinks()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct("P1", client);
			CreateIPRReceiveWithInventory(client, warehouse, "R1", product, 10m, "Ent-1");
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = product.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 1m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			AssertEquals("Pick failed due to shortfall.", true, dynamicWorkOrder.IsAttachedToPickButNotFinalised);
			Assert("OrderLine should have no PickLines.", !(dynamicWorkOrderLine1.PickLines.Count > 0));

			dynamicWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			Assert(dynamicWorkOrder.RowErrors.Any(e => e.Message == "Dynamic Work Order cannot be finalized until some inventory is allocated."));
		}

		public void TestFinalisationForDissassembly_NoInventoryWithComponentLinks_InvalidPickLine()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct("P1", client);
			var receive = CreateIPRReceiveWithInventory(client, warehouse, "R1", product, 10m, "Ent-1");
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = product.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 1m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			AssertEquals("Pick failed due to shortfall.", true, dynamicWorkOrder.IsAttachedToPickButNotFinalised);
			Assert("OrderLine should have no PickLines.", !(dynamicWorkOrderLine1.PickLines.Count > 0));

			var invalidPickLine = dynamicWorkOrderLine1.PickLines.AddNew();
			invalidPickLine.WZ_WE_InventoryLine = receive.Lines[0].PK;

			AssertExceptionThrown<InvalidOperationException>(
				"Unable to revert assembly if allocated inventory has no links.",
				"Attempted to Revert Assembly with Dynamic Work Order for inventory with no Child Component Links.",
				() => dynamicWorkOrder.FinaliseDocketAlwaysFinalisingPick());
		}

		public void TestFinalisationForDissassembly_FullyFulfilled()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D1");
			dynamicWorkOrder1.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			var component = Helper.CreateProduct("P1", dynamicWorkOrder1.Client);
			var mainProduct = Helper.CreateProduct("P2", dynamicWorkOrder1.Client);
			CreateIPRReceiveWithInventory(dynamicWorkOrder1.Client, dynamicWorkOrder1.Warehouse, "R1", component, 10m, "Ent-1");
			Factory.Save();

			var parentLine1 = dynamicWorkOrder1.Lines.AddNew();
			parentLine1.WE_OP = mainProduct.PK;
			parentLine1.WE_TransactionQuantity = 5m;
			parentLine1.IsMainInwardProcessedItem = true;

			var childLine1 = dynamicWorkOrder1.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 10m;
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder1);
			AssertIsFinalisedPrecondition(dynamicWorkOrder1.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var receiveLines = dynamicWorkOrder1.Receive.Lines;

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder2.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder2.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = mainProduct.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 5m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;
			dynamicWorkOrderLine1.WE_AllocationKey = receiveLines[0].WE_AllocationKey;

			var pick2 = Helper.CreatePickNew(dynamicWorkOrder2);
			var pickLine = dynamicWorkOrderLine1.PickLines.SingleOrDefault();
			AssertNotNull("OrderLine should have PickLine.", pickLine);
			AssertEquals("OrderLine should pick from correct inventory.", receiveLines[0].PK , pickLine.WZ_WE_InventoryLine);

			dynamicWorkOrder2.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder2);
			AssertIsFinalisedPrecondition(dynamicWorkOrder2.Receive);

			var newReceiveLine = dynamicWorkOrder2.Receive.Lines.SingleOrDefault();
			AssertNotNull("ReceiveLine should have been created.", newReceiveLine);
			AssertEquals(component.PK, newReceiveLine.WE_OP);
			AssertEquals(10m, newReceiveLine.WE_TransactionQuantity);
			AssertEquals("ENT-1", newReceiveLine.WE_BondedEntryKey);
			AssertEquals(warehouse.DefaultLocationInInwardProcessingArea.PK, newReceiveLine.WE_WL);
		}

		public void TestFinalisationForDissassembly_PartiallyFulfilled()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D1");
			dynamicWorkOrder1.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			var component = Helper.CreateProduct("P1", dynamicWorkOrder1.Client);
			var mainProduct = Helper.CreateProduct("P2", dynamicWorkOrder1.Client);
			CreateIPRReceiveWithInventory(dynamicWorkOrder1.Client, dynamicWorkOrder1.Warehouse, "R1", component, 15m, "Ent-1");
			Factory.Save();

			var parentLine1 = dynamicWorkOrder1.Lines.AddNew();
			parentLine1.WE_OP = mainProduct.PK;
			parentLine1.WE_TransactionQuantity = 5m;
			parentLine1.IsMainInwardProcessedItem = true;

			var childLine1 = dynamicWorkOrder1.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 15m;
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder1);
			AssertIsFinalisedPrecondition(dynamicWorkOrder1.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var receiveLines = dynamicWorkOrder1.Receive.Lines;

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder2.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder2.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = mainProduct.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 10m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;
			dynamicWorkOrderLine1.WE_AllocationKey = receiveLines[0].WE_AllocationKey;

			var pick2 = Helper.CreatePickNew(dynamicWorkOrder2);
			var pickLine = dynamicWorkOrderLine1.PickLines.SingleOrDefault();
			AssertNotNull("OrderLine should have PickLine.", pickLine);
			AssertEquals("OrderLine should pick from correct inventory.", receiveLines[0].PK, pickLine.WZ_WE_InventoryLine);

			AssertEquals(5m, dynamicWorkOrderLine1.SumOfUnitsMet);

			dynamicWorkOrder2.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder2);
			AssertIsFinalisedPrecondition(dynamicWorkOrder2.Receive);

			var newReceiveLine = dynamicWorkOrder2.Receive.Lines.SingleOrDefault();
			AssertNotNull("ReceiveLine should have been created.", newReceiveLine);
			AssertEquals(component.PK, newReceiveLine.WE_OP);
			AssertEquals(15m, newReceiveLine.WE_TransactionQuantity);
			AssertEquals("ENT-1", newReceiveLine.WE_BondedEntryKey);
			AssertEquals(warehouse.DefaultLocationInInwardProcessingArea.PK, newReceiveLine.WE_WL);
		}

		public void TestFinalisationForDissassembly_NestedDisassemblyMaintainsLinks()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			SetupWarehouse(warehouse);

			var client = Helper.CreateClient("C1");
			var mainProduct = Helper.CreateProduct("P1", client);
			var component = Helper.CreateProduct("P2", client);
			var subComponent = Helper.CreateProduct("P3", client);
			CreateIPRReceiveWithInventory(client, warehouse, "R1", subComponent, 10m, "Ent-1");
			Factory.Save();

			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D1");
			dynamicWorkOrder1.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine1 = dynamicWorkOrder1.Lines.AddNew();
			parentLine1.WE_OP = component.PK;
			parentLine1.WE_TransactionQuantity = 5m;
			parentLine1.IsMainInwardProcessedItem = true;

			var childLine1 = dynamicWorkOrder1.Lines.AddNew();
			childLine1.WE_OP = subComponent.PK;
			childLine1.WE_TransactionQuantity = 10m;
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder1);
			AssertIsFinalisedPrecondition(dynamicWorkOrder1.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder2.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder2.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine2 = dynamicWorkOrder2.Lines.AddNew();
			parentLine2.WE_OP = mainProduct.PK;
			parentLine2.WE_TransactionQuantity = 2m;
			parentLine2.IsMainInwardProcessedItem = true;

			var childLine2 = dynamicWorkOrder2.Lines.AddNew();
			childLine2.WE_OP = component.PK;
			childLine2.WE_TransactionQuantity = 5m;
			childLine2.WE_WE_ParentDocketLine = parentLine2.PK;

			Helper.CreatePickNew(dynamicWorkOrder2);
			dynamicWorkOrder2.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder2);
			AssertIsFinalisedPrecondition(dynamicWorkOrder2.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var receiveLines = dynamicWorkOrder2.Receive.Lines;

			var dynamicWorkOrder3 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D3");
			dynamicWorkOrder3.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var dynamicWorkOrderLine1 = dynamicWorkOrder3.Lines.AddNew();
			dynamicWorkOrderLine1.WE_OP = mainProduct.PK;
			dynamicWorkOrderLine1.WE_TransactionQuantity = 2m;
			dynamicWorkOrderLine1.IsMainInwardProcessedItem = true;
			dynamicWorkOrderLine1.WE_AllocationKey = receiveLines[0].WE_AllocationKey;

			var pick2 = Helper.CreatePickNew(dynamicWorkOrder3);
			var pickLine = dynamicWorkOrderLine1.PickLines.SingleOrDefault();
			AssertNotNull("OrderLine should have PickLine.", pickLine);
			AssertEquals("OrderLine should pick from correct inventory.", receiveLines[0].PK, pickLine.WZ_WE_InventoryLine);

			dynamicWorkOrder3.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder3);
			AssertIsFinalisedPrecondition(dynamicWorkOrder3.Receive);

			var newReceiveLine = dynamicWorkOrder3.Receive.Lines.SingleOrDefault();
			AssertNotNull("ReceiveLine should have been created.", newReceiveLine);
			AssertEquals(component.PK, newReceiveLine.WE_OP);
			AssertEquals(5m, newReceiveLine.WE_TransactionQuantity);
			AssertEquals(warehouse.DefaultLocationInInwardProcessingArea.PK, newReceiveLine.WE_WL);

			var link = newReceiveLine.BOMComponentLinks.SingleOrDefault();
			AssertNotNull("Maintains pre-existing links.", link);
			AssertEquals(10m, link.WIP_ComponentQuantity);
			AssertEquals(childLine1.PK, link.WIP_WE_ComponentLine);
		}

		#endregion

		#endregion

		#region TestCalculateTotalUnitsForDisassembly

		public void TestCalculateTotalUnitsForDisassembly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Factory.Save();

			var component1Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1", componentProduct1, 10m, $"BEK-1", allocateLocations: false, finalise: false);
			component1Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component1Receive.WD_IsInwardsProcessingJob = true;
			component1Receive.Lines[0].WE_WL = location.PK;
			component1Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component1Receive);

			var component2Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2", componentProduct2, 10m, $"BEK-2", allocateLocations: false, finalise: false);
			component2Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component2Receive.WD_IsInwardsProcessingJob = true;
			component2Receive.Lines[0].WE_WL = location.PK;
			component2Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component2Receive);
			Factory.Save();

			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D1");
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder1.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, mainProduct, 5m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var workOrderLineMainComp1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, componentProduct1, 5m);
			workOrderLineMainComp1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComp2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, componentProduct2, 10m);
			workOrderLineMainComp2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D2");
			dynamicWorkOrder2.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			dynamicWorkOrder2.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder2.WD_IsInwardsProcessingJob = true;

			var disassemblyLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder2, mainProduct, 5m);
			disassemblyLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			AssertEquals("Total Component Count should be 0 before allocation.", 0m, dynamicWorkOrder2.CalculateTotalUnitsForDisassembly());

			Helper.CreatePickNew(dynamicWorkOrder2);

			AssertEquals("Total Component Count for disassembly accurately calculated.", 15m, dynamicWorkOrder2.CalculateTotalUnitsForDisassembly());
		}

		#endregion

		#region TestGetLinesToPick

		public override void TestGetLinesToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var mainProduct = Helper.CreateProduct("JUICE", data.Org1);
			var secondaryProduct = Helper.CreateProduct("PEEL", data.Org1);
			var componentProduct1 = Helper.CreateProduct("ORANGE", data.Org1);
			var componentProduct2 = Helper.CreateProduct("WATER", data.Org1);
			var componentProduct3 = Helper.CreateProduct("POTASSIUMBENZOATE", data.Org1);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "JUICE LOOSENER");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;
			AssertEquals("Should be robust to no main line existing.", 0, workOrder.GetLinesToPick().Count);

			var workOrderLineSecondary = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryProduct, 1m);
			workOrderLineSecondary.IsSecondaryInwardProcessedItem = true;
			AssertEquals("Should be robust to no main line existing.", 0, workOrder.GetLinesToPick().Count);

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 1m);
			workOrderLineMain.IsMainInwardProcessedItem = true;
			AssertEquals("Should have no lines to pick.", 0, workOrder.GetLinesToPick().Count);

			var workOrderLineMainComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 1m);
			workOrderLineMainComponent1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComponent2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct2, 2m);
			workOrderLineMainComponent2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComponent3 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct3, 10m);
			workOrderLineMainComponent3.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineSecondaryComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 0.1m);
			workOrderLineSecondaryComponent1.WE_WE_ParentDocketLine = workOrderLineSecondary.PK;

			var workOrderLineSecondaryComponent3 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct3, 1m);
			workOrderLineSecondaryComponent3.WE_WE_ParentDocketLine = workOrderLineSecondary.PK;

			AssertContainsExactElementsInAnyOrder("Lines to pick should only include main component lines.",
				new[] { workOrderLineMainComponent1, workOrderLineMainComponent2, workOrderLineMainComponent3 },
				workOrder.GetLinesToPick());

			AssertContainsExactElementsInAnyOrder("Lines to pick should only include main component lines.",
				new[] { workOrderLineMainComponent1, workOrderLineMainComponent2, workOrderLineMainComponent3 },
				workOrder.LinesToPickForBinding);
		}

		#endregion

		#region TestLines

		protected override Type ExpectedLineCollectionType => typeof(WhsDynamicWorkOrderLineCollection);

		#endregion

		#region TestSubTypeDesc

		public override void TestSubTypeDesc()
		{
			// this test will be added in future WI
			Assert(true);
		}

		#endregion

		#region TestDelete_WhenJobHeaderIsSaved

		protected override WhsDynamicWorkOrder GetDocketForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_DocketSubType = "ASS";
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_ExternalReference = "Test";

			return workOrder;
		}

		#endregion

		#region TestGetNewLookups

		protected override Type GetExpectedLookupsType() => typeof(WhsDynamicWorkOrderLookups);

		#endregion

		#region TestGetNewValidation

		protected override Type GetExpectedValidationType() => typeof(WhsDynamicWorkOrderValidation);

		#endregion

		#region SupportsRecalculateOrderPricing

		protected override bool SupportsRecalculateOrderPricing => false;

		#endregion

		#region TestUpdateTotalWeightAndVolumeWhenLineRemoved

		protected override void TestUpdateTotalWeightAndVolumeWhenLineRemovedCore(ZString type)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var workOrder = Helper.CreateWhsDynamicWorkOrder(org, whs, "O1");
			workOrder.WD_DocketSubType = type;
			
			var orderLine1 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart, subPart1, subPart2, 1m);
			var orderLine2 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart, subPart1, subPart2, 1m);
			var orderLine3 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart, subPart1, subPart2, 1m);

			orderLine1.WE_TransactionQuantity = 1m;
			orderLine2.WE_TransactionQuantity = 1m;
			orderLine3.WE_TransactionQuantity = 1m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Precondition", 60m, workOrder.WD_TotalWeight);
				AssertEquals("Precondition", 3.3m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Precondition", 90m, workOrder.WD_TotalWeight);
				AssertEquals("Precondition", 3m, workOrder.WD_TotalCubic);
			}

			workOrder.Lines.Delete(orderLine2);
			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals(40m, workOrder.WD_TotalWeight);
				AssertEquals(2.2m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals(60m, workOrder.WD_TotalWeight);
				AssertEquals(2m, workOrder.WD_TotalCubic);
			}

			workOrder.Lines.RemoveFromRelationship(orderLine1);
			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals(20m, workOrder.WD_TotalWeight);
				AssertEquals(1.1m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals(30m, workOrder.WD_TotalWeight);
				AssertEquals(1m, workOrder.WD_TotalCubic);
			}
		}

		WhsDynamicWorkOrderLine CreateLineIncludingChildComponentLinesIfAssembly(
			WhsDynamicWorkOrder docket,
			OrgSupplierPart part,
			OrgSupplierPart subPart1,
			OrgSupplierPart subPart2,
			decimal quantity,
			decimal subPart1Ratio = 2m,
			decimal subPart2Ratio = 1m)
		{
			var workOrderLine = Helper.CreateWhsDynamicWorkOrderLine(docket, part, 0m);
			workOrderLine.IsMainInwardProcessedItem = true;
			workOrderLine.WE_TransactionQuantity = 1m * quantity;

			if (docket.IsAssembly)
			{
				var childComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(docket, subPart1, 0m);
				childComponentLine1.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine1.WE_TransactionQuantity = subPart1Ratio * quantity; // Set after the line becomes a component line

				var childComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(docket, subPart2, 0m);
				childComponentLine2.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine2.WE_TransactionQuantity = subPart2Ratio * quantity; // Set after the line becomes a component line
			}

			return workOrderLine;
		}

		#endregion

		#region TestWD_TotalWeightAndVolumeCore

		protected override void TestWD_TotalWeightAndVolumeCore(ZString type)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var mainPart2 = Helper.CreateProduct(org, "MainPart2");
			var subPart3 = Helper.CreateProduct(org, "SubPart3");
			var subPart4 = Helper.CreateProduct(org, "SubPart4");
			Helper.SetProductWeightAndVolume(mainPart2, 100, "G", 100, "D3");
			Helper.SetProductWeightAndVolume(subPart3, 90, "G", 50, "D3");
			Helper.SetProductWeightAndVolume(subPart4, 80, "G", 20, "D3");

			var workOrder = Helper.CreateWhsDynamicWorkOrder(org, whs, "O1");
			workOrder.WD_DocketSubType = type;

			var orderLine1 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart, subPart1, subPart2, 1m);
			var orderLine2 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart2, subPart3, subPart4, 1m, subPart1Ratio: 1m);

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("WD_TotalWeight should be the sum of components when type is Assemble", 20.17m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components when type is Assemble", 1.17m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of primary products when type is Disassemble", 30.10m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of primary products when type is Disassemble", 1.10m, workOrder.WD_TotalCubic);
			}

			orderLine1.WE_TransactionQuantity = 2m;
			orderLine2.WE_TransactionQuantity = 2m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("WD_TotalWeight should be the sum of components when type is Assemble", 20.17m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components when type is Assemble", 1.17m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of primary products when type is Disassemble", 60.20m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of primary products when type is Disassemble", 2.20m, workOrder.WD_TotalCubic);
			}

			orderLine1.ChildComponentLines.ForEach(l => l.WE_TransactionQuantity *= 2m);
			orderLine2.ChildComponentLines.ForEach(l => l.WE_TransactionQuantity *= 2m);

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("WD_TotalWeight should be the sum of components when type is Assemble", 40.34m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components when type is Assemble", 2.34m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of primary products when type is Disassemble", 60.20m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of primary products when type is Disassemble", 2.20m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestWD_TotalWeightAndVolume_MultipleLinesWithTheSameProductCore

		protected override void TestWD_TotalWeightAndVolume_MultipleLinesWithTheSameProductCore(ZString type)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var workOrder = Helper.CreateWhsDynamicWorkOrder(org, whs, "O1");
			workOrder.WD_DocketSubType = type;

			var orderLine1 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart, subPart1, subPart2, 1m);
			var orderLine2 = CreateLineIncludingChildComponentLinesIfAssembly(workOrder, mainPart, subPart1, subPart2, 1m);

			orderLine1.ChildComponentLines.ForEach(l => l.WE_TransactionQuantity *= 2m);
			orderLine2.ChildComponentLines.ForEach(l => l.WE_TransactionQuantity *= 2m);

			if (workOrder.IsAssembly)
			{
				AssertEquals("WD_TotalWeight should be the sum of components.", 80m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components.", 4.4m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of main products.", 60m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of main products.", 2m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestShouldUpdateWeightAndVolumeOnTheFly

		public override void TestShouldUpdateWeightAndVolumeOnTheFly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "O1");

			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			AssertEquals("Vol/Weight calculation should be enabled for Assembly.", true, workOrder.ShouldUpdateWeightAndVolumeOnTheFly);

			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertEquals("Vol/Weight calculation should be enabled for Disassembly.", true, workOrder.ShouldUpdateWeightAndVolumeOnTheFly);

			base.TestShouldUpdateWeightAndVolumeOnTheFly();
		}

		#endregion

		#region TestIsUSBonded

		protected override bool SupportsCustomsTransactions => true;

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetValidRelatedJobs(WhsDynamicWorkOrder docket)
		{
			docket.FillWithValidTestData();
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WD_ParentDocket = docket.PK;

			Factory.Save();

			return new List<IRelatedJob>() { receive };
		}

		#endregion

		#region TestLoadPickLinesForAllDocketLines

		protected override void TestLoadPickLinesForAllDocketLinesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var iprLocation = data.Whs1.DefaultLocationInInwardProcessingArea;
			var mainPart = Helper.CreateProduct("P431", data.Org1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			for (var i = 0; i < 2; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, iprLocation.PK, palletID: "", entryKey: $"Ent-{i}");
			} // make sure these are on sep inv lines
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, iprLocation.PK, palletID: "", entryKey: "Ent-3");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine = workOrder.Lines.AddNew();
			parentLine.WE_OP = mainPart.PK;
			parentLine.WE_TransactionQuantity = 1m;
			parentLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var childLine1 = workOrder.Lines.AddNew();
			childLine1.WE_OP = data.Part1.PK;
			childLine1.WE_TransactionQuantity = 1m;
			childLine1.WE_WE_ParentDocketLine = parentLine.PK;

			var childLine2 = workOrder.Lines.AddNew();
			childLine2.WE_OP = data.Part2.PK;
			childLine2.WE_TransactionQuantity = 2m;
			childLine2.WE_WE_ParentDocketLine = parentLine.PK;
			Factory.Save();

			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);
			Factory.Save();

			// load everything in a new factory to test load performance
			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<WhsPickableDocket>(workOrder.PK);
			var line1 = FindPickableDocketLine(docketInOtherFactory, data.Part1);
			var line2 = FindPickableDocketLine(docketInOtherFactory, data.Part2);

			var initialDBHits = otherFactory.DatabaseLoadCount;
			docketInOtherFactory.AddFetchHintsForPickLines();
			AssertEquals("Should have used only 1 DB hit to load all PickLines", 1, otherFactory.DatabaseLoadCount - initialDBHits);
			//
			AssertEquals("Should be 1 PickLine (1 Part1).", 1, line1.PickLines.Count);
			AssertEquals("Should be 2 PickLines (1 Part2 per PickLine).", 2, line2.PickLines.Count);
			//
			AssertEquals("Should have used only 1 DB hit to load all PickLines", 1, otherFactory.DatabaseLoadCount - initialDBHits);
		}

		protected override WhsPickableDocket GetPickableDocket_ForLoadPickLinesForAllDocketLinesTest(TestDataForBOM data)
		{
			data.Whs1.WW_IsVirtualWarehouse = true;
			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);

			var receive = Factory.LoadTop1<WhsReceive>(new ZQuery());
			var location = receive.Lines[0].Location;
			location.WLV_WA_PutawayArea = iprArea.PK;
			location.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			var docket = Factory.New<WhsDynamicWorkOrder>();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine = (WhsDynamicWorkOrderLine)Helper.CreateWhsPickableDocketLine(docket, data.BOM.Bike, 2m);
			parentLine.IsMainInwardProcessedItem = true;

			var childLine1 = docket.Lines.AddNew();

			childLine1.WE_OP = data.BOM.BikeEngine.PK;
			childLine1.WE_TransactionQuantity = 2m;
			childLine1.WE_WE_ParentDocketLine = parentLine.PK;

			var childLine2 = docket.Lines.AddNew();
			childLine2.WE_OP = data.BOM.BikeWheel.PK;
			childLine2.WE_TransactionQuantity = 4m;
			childLine2.WE_WE_ParentDocketLine = parentLine.PK;

			return docket;
		}

		#endregion

		#region TestWD_AutoFinaliseBOMIntoInventory_ReadOnly

		public void TestWD_AutoFinaliseBOMIntoInventory_ReadOnly()
		{
			var dynamicWorkOrder = GetNewBusinessObject();
			AssertEquals("Should be readonly.", true, dynamicWorkOrder.WD_AutoFinaliseBOMIntoInventoryInfo.ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDynamicWorkOrder).GetProperty(WhsDocketSchema.WD_AutoFinaliseBOMIntoInventory.Name)).ReadOnly);
		}

		#endregion

		#region TestWD_IsInwardsProcessingJob

		public void TestWD_IsInwardsProcessingJob_ReadOnly()
		{
			var dynamicWorkOrder = GetNewBusinessObject();
			AssertEquals("Should be readonly.", true, dynamicWorkOrder.WD_IsInwardsProcessingJobInfo.ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDynamicWorkOrder).GetProperty(WhsDocketSchema.WD_IsInwardsProcessingJob.Name)).ReadOnly);
		}

		#endregion

		#region TestRunPreSaveValidation_SetsMatchingLine

		public void TestRunPreSaveValidation_SetsMatchingLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);

			// Work order in error
			setup.WorkOrder.WD_WW_Whs = ZGuid.Empty;
			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Matching line should *not* be set.", ZGuid.Empty, setup.SecondaryComponentLine_Orange.WE_WE_MatchingLine);
			AssertEquals("Matching line should *not* be set.", ZGuid.Empty, setup.SecondaryComponentLine_PotassiumBenzoate.WE_WE_MatchingLine);

			setup.WorkOrder.WD_WW_Whs = data.Whs1.PK;

			// Lines in error
			setup.MainProductLine_Juice.WE_OP = ZGuid.Empty;
			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Matching line should *not* be set.", ZGuid.Empty, setup.SecondaryComponentLine_Orange.WE_WE_MatchingLine);
			AssertEquals("Matching line should *not* be set.", ZGuid.Empty, setup.SecondaryComponentLine_PotassiumBenzoate.WE_WE_MatchingLine);

			setup.MainProductLine_Juice.WE_OP = setup.Orange_ComponentProduct.PK;

			// Component Lines in error
			setup.MainComponentLine_Orange.WE_OP = ZGuid.Empty;
			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Matching line should *not* be set.", ZGuid.Empty, setup.SecondaryComponentLine_Orange.WE_WE_MatchingLine);
			AssertEquals("Matching line should *not* be set.", ZGuid.Empty, setup.SecondaryComponentLine_PotassiumBenzoate.WE_WE_MatchingLine);

			setup.MainComponentLine_Orange.WE_OP = setup.Orange_ComponentProduct.PK;

			// Valid
			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Main product should not have a matching line set.", ZGuid.Empty, setup.MainProductLine_Juice.WE_WE_MatchingLine);
			AssertEquals("Main product components should not have a matching line set.", ZGuid.Empty, setup.MainComponentLine_Orange.WE_WE_MatchingLine);
			AssertEquals("Main product components should not have a matching line set.", ZGuid.Empty, setup.MainComponentLine_Water.WE_WE_MatchingLine);
			AssertEquals("Main product components should not have a matching line set.", ZGuid.Empty, setup.MainComponentLine_PotassiumBenzoate.WE_WE_MatchingLine);

			AssertEquals("Secondary product should not have a matching line set.", ZGuid.Empty, setup.SecondaryProductLine_Peel.WE_WE_MatchingLine);
			AssertEquals("Matching line should be set.", setup.MainComponentLine_Orange.PK, setup.SecondaryComponentLine_Orange.WE_WE_MatchingLine);
			AssertEquals("Matching line should be set.", setup.MainComponentLine_PotassiumBenzoate.PK, setup.SecondaryComponentLine_PotassiumBenzoate.WE_WE_MatchingLine);

			using (setup.SecondaryComponentLine_PotassiumBenzoate.SuspendValidationTesting())
			{
				setup.SecondaryComponentLine_PotassiumBenzoate.WE_OP = setup.Frogurt_InvalidProduct.PK;
			}
			AssertExceptionThrown<ArgumentException>(() => setup.WorkOrder.RunPreSaveValidation());
		}

		public void TestRunPreSaveValidation_SetsMatchingLine_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var today = ZDate.Today;
			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);

			var workOrderLineMain = setup.MainProductLine_Juice;
			workOrderLineMain.WE_PartAttrib1 = "OTH-A";
			workOrderLineMain.WE_PartAttrib2 = "OTH-B";
			workOrderLineMain.WE_PartAttrib3 = "OTH-C";
			workOrderLineMain.WE_SerialNumber = "OTH-D";
			workOrderLineMain.WE_BondedEntryKey = "OTH-E";
			workOrderLineMain.WE_AllocationKey = "OTH-F";
			workOrderLineMain.WE_PackingDate = today.AddDays(-7);
			workOrderLineMain.WE_ExpiryDate = today.AddDays(7);
			workOrderLineMain.IsMainInwardProcessedItem = true;

			var workOrderLineMainComponent1 = setup.MainComponentLine_Orange;
			workOrderLineMainComponent1.WE_PartAttrib1 = "A";
			workOrderLineMainComponent1.WE_PartAttrib2 = "B";
			workOrderLineMainComponent1.WE_PartAttrib3 = "C";
			workOrderLineMainComponent1.WE_SerialNumber = "D";
			workOrderLineMainComponent1.WE_BondedEntryKey = "E";
			workOrderLineMainComponent1.WE_AllocationKey = "F";
			workOrderLineMainComponent1.WE_PackingDate = today.AddDays(-14);
			workOrderLineMainComponent1.WE_ExpiryDate = today.AddDays(14);
			workOrderLineMainComponent1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Main product should not have a matching line set.", ZGuid.Empty, workOrderLineMain.WE_WE_MatchingLine);
			AssertEquals("Main product components should not have a matching line set.", ZGuid.Empty, workOrderLineMainComponent1.WE_WE_MatchingLine);

			AssertEquals("Secondary product should not have a matching line set.", ZGuid.Empty, setup.SecondaryProductLine_Peel.WE_WE_MatchingLine);
			AssertEquals("Matching line should be set.", workOrderLineMainComponent1.PK, setup.SecondaryComponentLine_Orange.WE_WE_MatchingLine);

			AssertEquals("Attributes should be synced.", "A", setup.SecondaryComponentLine_Orange.WE_PartAttrib1);
			AssertEquals("Attributes should be synced.", "B", setup.SecondaryComponentLine_Orange.WE_PartAttrib2);
			AssertEquals("Attributes should be synced.", "C", setup.SecondaryComponentLine_Orange.WE_PartAttrib3);
			AssertEquals("Attributes should be synced.", "D", setup.SecondaryComponentLine_Orange.WE_SerialNumber);
			AssertEquals("Attributes should be synced.", "E", setup.SecondaryComponentLine_Orange.WE_BondedEntryKey);
			AssertEquals("Attributes should be synced.", "F", setup.SecondaryComponentLine_Orange.WE_AllocationKey);
			AssertEquals("Attributes should be synced.", today.AddDays(-14), setup.SecondaryComponentLine_Orange.WE_PackingDate);
			AssertEquals("Attributes should be synced.", today.AddDays(14), setup.SecondaryComponentLine_Orange.WE_ExpiryDate);

			workOrderLineMainComponent1.WE_PartAttrib1 = "1";
			workOrderLineMainComponent1.WE_PartAttrib2 = "2";
			workOrderLineMainComponent1.WE_PartAttrib3 = "3";
			workOrderLineMainComponent1.WE_SerialNumber = "4";
			workOrderLineMainComponent1.WE_BondedEntryKey = "5";
			workOrderLineMainComponent1.WE_AllocationKey = "6";
			workOrderLineMainComponent1.WE_PackingDate = today.AddDays(7);
			workOrderLineMainComponent1.WE_ExpiryDate = today.AddDays(8);

			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Attributes should be synced.", "1", setup.SecondaryComponentLine_Orange.WE_PartAttrib1);
			AssertEquals("Attributes should be synced.", "2", setup.SecondaryComponentLine_Orange.WE_PartAttrib2);
			AssertEquals("Attributes should be synced.", "3", setup.SecondaryComponentLine_Orange.WE_PartAttrib3);
			AssertEquals("Attributes should be synced.", "4", setup.SecondaryComponentLine_Orange.WE_SerialNumber);
			AssertEquals("Attributes should be synced.", "5", setup.SecondaryComponentLine_Orange.WE_BondedEntryKey);
			AssertEquals("Attributes should be synced.", "6", setup.SecondaryComponentLine_Orange.WE_AllocationKey);
			AssertEquals("Attributes should be synced.", today.AddDays(7), setup.SecondaryComponentLine_Orange.WE_PackingDate);
			AssertEquals("Attributes should be synced.", today.AddDays(8), setup.SecondaryComponentLine_Orange.WE_ExpiryDate);
		}

		#endregion

		#region TestRunPreSaveValidationCore_SynchronisesOffset

		public override void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);

			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			mainLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			workOrder.RunPreSaveValidation();

			Assert("Docket should not be in error", !workOrder.HasErrors);

			var expectedOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				mainLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		#endregion

		#region TestFinaliseDocket

		[GuiTest]
		public void TestFinaliseDocket_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			// create staging area
			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING", AreaTypes.Codes.InwardProcessing);
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;

			// create a BOM staging location for the Juice
			var productParams = WhsProduct.GetWhsProduct(setup.Juice_MainProduct).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocation.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;
			var workOrderInOtherFactory = otherFactory.Load<WhsDynamicWorkOrder>(setup.WorkOrder.PK);

			using (RowFactory.SetCachedTables())
			{
				workOrderInOtherFactory.RunPreSaveValidation();
				workOrderInOtherFactory.Validation.ValidateAll();
			}

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ RefCountrySchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 7 },
			};

			AssertDbHits(expectedDBHitsForValidation, otherFactory);

			var expectedDBHitsForFinalisation = new Dictionary<string, int>(expectedDBHitsForValidation);
			expectedDBHitsForFinalisation[WhsDocketSchema.Constants.TableName] += 5;
			expectedDBHitsForFinalisation[WhsAreaSchema.Constants.TableName] += 1;
			expectedDBHitsForFinalisation[WhsDocketLineSchema.Constants.TableName] += 2;
			expectedDBHitsForFinalisation[JobDocAddressSchema.Constants.TableName] += 2;
			expectedDBHitsForFinalisation[StmALogSchema.Constants.TableName] += 2;
			expectedDBHitsForFinalisation[GlbBranchSchema.Constants.TableName] += 1;

			expectedDBHitsForFinalisation[OrgCusCodeSchema.Constants.TableName] = 1;
			expectedDBHitsForFinalisation[OrgPartUnitSchema.Constants.TableName] = 1;

			expectedDBHitsForFinalisation.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketPalletSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTasksSchema.Constants.TableName, 3);
			expectedDBHitsForFinalisation.Add(ProcessTaskTemplateSchema.Constants.TableName, 3);
			expectedDBHitsForFinalisation.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobServiceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(StmEventSchema.Constants.TableName, 4);
			expectedDBHitsForFinalisation.Add(StmNoteSchema.Constants.TableName, 2);
			expectedDBHitsForFinalisation.Add(RefPacksSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(RefPackTypeSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(GlbCompanySchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(GlbStaffSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProductionRuleSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsLocationTypeSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsPickFaceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(OrgCustomLabelsSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsClientParameterByWarehouseSchema.Constants.TableName, 1);

			expectedDBHitsForFinalisation.Add(WhsProductParamsByWhsAndClientSchema.Constants.TableName, 2);
			expectedDBHitsForFinalisation.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsLocationViewSchema.Constants.TableName, 4);
			expectedDBHitsForFinalisation.Add(WhsPickLineSchema.Constants.TableName, 7);
			expectedDBHitsForFinalisation.Add(WhsRowSchema.Constants.TableName, 2);

			var factoryForFinalise = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHitsForFinalisation, factoryForFinalise))
			{
				factoryForFinalise.RefreshEnabled = false;
				var workOrderInFinaliseFactory = factoryForFinalise.Load<WhsDynamicWorkOrder>(setup.WorkOrder.PK);

				using (RowFactory.SetCachedTables())
				{
					var pick = factoryForFinalise.New<WhsPick>();
					pick.PickOrders(workOrderInFinaliseFactory);

					workOrderInFinaliseFactory.FinaliseDocketAlwaysFinalisingPick();
					AssertEquals(true, workOrderInFinaliseFactory.IsFinalised);
				}

				var bwaHitCount = factoryForFinalise.GetTableHitCount(WhsBondedWarehouseAttributeSchema.Constants.TableName);
				AssertEquals(true, bwaHitCount == 3 || bwaHitCount == 4);
				expectedDBHitsForFinalisation[WhsBondedWarehouseAttributeSchema.Constants.TableName] = bwaHitCount;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		protected override void TestFinaliseDocket_CreatesReceiveCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);
			Factory.Save();

			var location = data.Whs1.DefaultLocationInInwardProcessingArea;
			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.MainProductLine_Juice.WE_TransactionQuantity = 5m;
			setup.SecondaryProductLine_Peel.WE_TransactionQuantity = 3m;
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();

			WhsReceive receivePassedToEvent = null;
			setup.WorkOrder.AutoCreatedReceiveSaved += (object sender, WhsWorkOrder.AutoCreatedReceiveSavedEventArgs e) => { receivePassedToEvent = e.Receive; };

			setup.WorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Should have finalized the dynamic work order.", true, setup.WorkOrder.IsFinalised);

			var receive = setup.WorkOrder.Receive;
			AssertEquals("New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive.WD_DocketSubType);
			AssertEquals("New Receive should be Inwards Processing Job.", true, receive.WD_IsInwardsProcessingJob);
			AssertEquals("New Receive should be finalized.", true, receive.IsFinalised);
			AssertEquals("Should have created two products.", 2, receive.Lines.Count);

			var inventoryLineMainProduct = receive.Lines.Single(l => l.WE_OP == setup.Juice_MainProduct.PK);
			var inventoryLineSecondaryProduct1 = receive.Lines.Single(l => l.WE_OP == setup.Peel_SecondaryProduct.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_StockOnHand);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_ClientOrderedUnits);
			AssertEquals("Correct Inwards Processing flag is set.", true, inventoryLineMainProduct.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Correct Inwards Processing flag is set.", false, inventoryLineMainProduct.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("Assembled inventory has correct Location.", location.PK, inventoryLineMainProduct.WE_WL);
			AssertEquals("Assembled inventory should have no Inwards Entry Key.", "", inventoryLineMainProduct.WE_BondedEntryKey);
			AssertEquals("Assembled inventory has no Allocation Key yet.", "", inventoryLineMainProduct.WE_AllocationKey);

			AssertEquals("Has correct Secondary Product Quantities.", 3m, inventoryLineSecondaryProduct1.WE_TransactionQuantity);
			AssertEquals("Has correct Secondary Product Quantities.", 3m, inventoryLineSecondaryProduct1.WE_StockOnHand);
			AssertEquals("Has correct Secondary Product Quantities.", 3m, inventoryLineSecondaryProduct1.WE_ClientOrderedUnits);
			AssertEquals("Arrival Date is set.", true, inventoryLineSecondaryProduct1.WE_AdjustmentArrivalDate.IsValid);
			AssertEquals("Correct Inwards Processing flag is set.", false, inventoryLineSecondaryProduct1.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Correct Inwards Processing flag is set.", true, inventoryLineSecondaryProduct1.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("SecondaryProduct has correct Location.", location.PK, inventoryLineSecondaryProduct1.WE_WL);
			AssertEquals("SecondaryProduct should have no Inwards Entry Key.", "", inventoryLineSecondaryProduct1.WE_BondedEntryKey);
			AssertEquals("SecondaryProduct has no Allocation Key yet.", "", inventoryLineSecondaryProduct1.WE_AllocationKey);

			var expectedMessage = string.Format(
	"Receive {0}-{1} has been created to place the assembled Work Order Product into inventory.\r\n" +
	"\r\n" +
	"After Saving the Work Order the Receive Job will be available on the Related Jobs tab.", setup.WorkOrder.WD_ExternalReference, setup.WorkOrder.WD_ExternalReferenceSplit);

			var notify = (NotificationBuffer)setup.WorkOrder.NotificationManager.Peek;
			AssertEquals(expectedMessage, notify.Events[0].Message);

			// test the AutoCreatedReceiveSaved event
			AssertNull("Receive is not yet saved, event should not fire.", receivePassedToEvent);
			Factory.Save();
			AssertEquals(receive, receivePassedToEvent);
			AssertContainsExactElementsInAnyOrder(new[] { "W00000003-0001", "W00000003-0002" },
				new[] { inventoryLineMainProduct, inventoryLineSecondaryProduct1 }.Select(i => i.WE_AllocationKey));

			setup.WorkOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, setup.WorkOrder.HasErrors);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		protected override void TestFinaliseDocket_CreatesReceive_SetBookingDateBasedOnBranchOfWarehouseCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			data.Whs1.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			Factory.Save();

			var location = data.Whs1.DefaultLocationInInwardProcessingArea;
			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.MainProductLine_Juice.WE_TransactionQuantity = 5m;
			setup.SecondaryProductLine_Peel.WE_TransactionQuantity = 3m;
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();

			WhsReceive receivePassedToEvent = null;
			setup.WorkOrder.AutoCreatedReceiveSaved += (object sender, WhsWorkOrder.AutoCreatedReceiveSavedEventArgs e) => { receivePassedToEvent = e.Receive; };

			setup.WorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Should have finalized the dynamic work order.", true, setup.WorkOrder.IsFinalised);

			var receive = setup.WorkOrder.Receive;
			AssertEquals(new ZDateTimeOffset(2009, 5, 6, 8, 0, 0, TimeSpan.FromHours(8)), receive.WD_BookingDate);
		}

		protected override void TestFinaliseDocket_CreatesReceiveAndFinalises_InformsUserIfFinaliseFailsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			componentReceive.WD_DocketSubType = "CUS";
			componentReceive.WD_IsInwardsProcessingJob = true;
			var line1 = Helper.CreateWhsReceiveLine(componentReceive, setup.Orange_ComponentProduct, 100m, data.Whs1.DefaultLocationInInwardProcessingArea);
			var line2 = Helper.CreateWhsReceiveLine(componentReceive, setup.Water_ComponentProduct, 100m, data.Whs1.DefaultLocationInInwardProcessingArea);
			var line3 = Helper.CreateWhsReceiveLine(componentReceive, setup.PotassiumBenzoate_ComponentProduct, 100m, data.Whs1.DefaultLocationInInwardProcessingArea);
			line1.CustomsData.WB_EntryKey = "ABC";
			line2.CustomsData.WB_EntryKey = "ABC";
			line3.CustomsData.WB_EntryKey = "ABC";
			componentReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();

			// setup a mandatory attribute on the product -- thus the receive cannot be auto-finalised because the line does not have a value for the attrib
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, setup.Juice_MainProduct, AttributeNumber.One, true);

			setup.WorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals(false, setup.WorkOrder.Receive.IsInDatabase);
			AssertEquals(true, setup.WorkOrder.IsRegisteredEditableChildObject(setup.WorkOrder.Receive));
			AssertEquals("Receive should not be auto-finalized because the Receive failed finalise validation.", false, setup.WorkOrder.Receive.IsFinalised);

			var notify = (NotificationBuffer)setup.WorkOrder.NotificationManager.Peek;
			var expectedMessage =
				$"Receive {setup.WorkOrder.WD_ExternalReference}-{setup.WorkOrder.WD_ExternalReferenceSplit} has been created to place the assembled Work Order Product into inventory.\r\n" +
				"\r\n" +
				"After Saving the Work Order the Receive Job will be available on the Related Jobs tab.\r\n" +
				"\r\n" +
				"The Receive could not be automatically finalized. You can manually Finalize the Receive.";
			AssertEquals(expectedMessage, notify.Events[0].Message);
		}

		protected override void SetupWarehouse(WhsWarehouse warehouse)
		{
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();
		}

		protected override WhsDynamicWorkOrder CreateComponentOrderForExternalReferenceTests(TestDataForBOM data)
		{
			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();
			return setup.WorkOrder;
		}

		public override void TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);
			Factory.Save();

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = "CUS";
			receive.WD_IsInwardsProcessingJob = true;
			// Orange in total shortfall (that's bad!), water fully available (that's good!), potassium benzoate in partial shortfall (that's bad!)
			var line1 = Helper.CreateWhsReceiveLine(receive, setup.Water_ComponentProduct, 100m, data.Whs1.DefaultLocationInInwardProcessingArea);
			var line2 = Helper.CreateWhsReceiveLine(receive, setup.PotassiumBenzoate_ComponentProduct, 5m, data.Whs1.DefaultLocationInInwardProcessingArea);
			line1.CustomsData.WB_EntryKey = "ABC";
			line2.CustomsData.WB_EntryKey = "ABC";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();

			setup.WorkOrder.FinaliseDocket();
			AssertEquals(false, setup.WorkOrder.IsFinalised);

			var notify = (NotificationBuffer)setup.WorkOrder.NotificationManager.Peek;
			AssertEquals(true, notify.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_AssemblyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			// create staging area
			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING", AreaTypes.Codes.InwardProcessing);
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;

			// create a BOM staging location for the Juice
			var productParams = WhsProduct.GetWhsProduct(setup.Juice_MainProduct).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocation.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(setup.WorkOrder);
			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer is created.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			var transferLine1 = newTransfer.Lines.Single(l => l.WE_OP == setup.Orange_ComponentProduct.PK);
			var transferLine2 = newTransfer.Lines.Single(l => l.WE_OP == setup.Water_ComponentProduct.PK);
			var transferLine3 = newTransfer.Lines.Single(l => l.WE_OP == setup.PotassiumBenzoate_ComponentProduct.PK);
			AssertEquals("Precondition: In-Transit Transfer Line has destination Location.", stagingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition: In-Transit Transfer Line has destination Location.", stagingLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition: In-Transit Transfer Line has destination Location.", stagingLocation.PK, transferLine3.WE_WL);

			productParams.W3_WL_InwardsProcessingStagingLocationBOM = ZGuid.Empty; // clear out staging location on product to test that staging location in transfers get refreshed.

			// finalise the WorkOrder
			setup.WorkOrder.FinaliseDocketAlwaysFinalisingPick();

			// test receive
			AssertEquals(false, setup.WorkOrder.Receive.IsInDatabase);
			AssertEquals("Receive should be auto-finalized because AutoFinaliseBOMIntoInventory was true.", true, setup.WorkOrder.Receive.IsFinalised);

			var juiceInventory = setup.WorkOrder.Receive.Lines.Single(l => l.WE_OP == setup.Juice_MainProduct.PK);
			AssertEquals("Should have received 1x Juice.", 1m, juiceInventory.WE_StockOnHand);
			AssertEquals("The stock should be available.", InventoryStatus.Codes.Available, juiceInventory.WE_CurrentInventoryStatus);
			AssertEquals("The stock should be in the Correct Location.", data.Whs1.DefaultLocationInInwardProcessingArea.PK, juiceInventory.WE_WL);
		}

		protected override void TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);
			Factory.Save();

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			// create a BOM staging location for the Juice
			var productParams = WhsProduct.GetWhsProduct(setup.Juice_MainProduct).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer is NOT created.", 0, pick.Transfers.Count);

			// finalise the WorkOrder
			setup.WorkOrder.FinaliseDocketAlwaysFinalisingPick();

			var juiceInventory = setup.WorkOrder.Receive.Lines.Single(l => l.WE_OP == setup.Juice_MainProduct.PK);
			AssertEquals("Juice must be in Warehouse default locations since Staging location BOM is a dock door.", data.Whs1.DefaultLocationInInwardProcessingArea.PK, juiceInventory.WE_WL);
			AssertEquals("The stock should be Available.", InventoryStatus.Codes.Available, juiceInventory.WE_CurrentInventoryStatus);
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 1 product.", 1, receive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)receive.Lines.Single();
			AssertEquals("Assembled correct Product.", data.Part2.PK, inventoryLineMainProduct.WE_OP);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var links = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links.Count());

			var link = links.Single();
			AssertEquals("Linked correct Component Line.", componentLine.PK, link.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		WhsReceive CreateIPRReceiveWithInventory(
			OrgHeader org,
			WhsWarehouse whs,
			string receiveReference,
			OrgSupplierPart part,
			decimal quantity,
			string entryKey)
		{
			var componentReceive = Helper.CreateWhsReceive(org, whs, receiveReference, Notify);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive, part.PK, quantity, whs.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: entryKey);
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			return componentReceive;
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProductsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var mainComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 1.5m);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine);

			var secondaryProductLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 10m);
			secondaryProductLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 0.5m);
			secondaryProductLine.ChildComponentLinesCollection.Add(secondaryComponentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 10m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 1m, link1.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link2 = secondaryProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.5m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingMultipleSecondaryProductsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var mainComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 1.5m);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine);

			var secondaryProductLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 10m);
			secondaryProductLine1.IsSecondaryInwardProcessedItem = true;

			var secondary1ComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 0.3m);
			secondaryProductLine1.ChildComponentLinesCollection.Add(secondary1ComponentLine);

			var secondaryProductLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part4, 7m);
			secondaryProductLine2.IsSecondaryInwardProcessedItem = true;

			var secondary2ComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 0.2m);
			secondaryProductLine2.ChildComponentLinesCollection.Add(secondary2ComponentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 2 secondary products.", 3, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct1 = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 10m, inventoryLineSecondaryProduct1.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct2 = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 7m, inventoryLineSecondaryProduct2.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 1m, link1.WIP_ComponentQuantity);

			var secondary1ProductLinks = inventoryLineSecondaryProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondary1ProductLinks.Count());

			var link2 = secondary1ProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.3m, link2.WIP_ComponentQuantity);

			var secondary2ProductLinks = inventoryLineSecondaryProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondary2ProductLinks.Count());

			var link3 = secondary2ProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.2m, link3.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive, data.Part1.PK, 2m, data.Whs1.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "Ent-1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, data.Part2.PK, 1m, data.Whs1.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "Ent-1");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", part3, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var mainComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			var mainComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 0.75m);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine1);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine2);

			var secondaryProductLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part4, 10m);
			secondaryProductLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 0.25m);
			secondaryProductLine.ChildComponentLinesCollection.Add(secondaryComponentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 10m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, mainProductLinks.Count());

			var link1 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part1.PK);
			AssertEquals("Linked correct Component Line.", mainComponentLine1.PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link1.WIP_ComponentQuantity);

			var link2 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part2.PK);
			AssertEquals("Linked correct Component Line.", mainComponentLine2.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.5m, link2.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link3 = secondaryProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine2.PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.25m, link3.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents_AllUsedBySecondaryProductCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive, data.Part1.PK, 2m, data.Whs1.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "Ent-1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, data.Part2.PK, 1m, data.Whs1.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "Ent-1");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", part3, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var mainComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 1.5m);
			var mainComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 0.75m);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine1);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine2);

			var secondaryProductLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part4, 10m);
			secondaryProductLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 0.5m);
			var secondaryComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 0.25m);
			secondaryProductLine.ChildComponentLinesCollection.Add(secondaryComponentLine1);
			secondaryProductLine.ChildComponentLinesCollection.Add(secondaryComponentLine2);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 10m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, mainProductLinks.Count());

			var link1 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part1.PK);
			AssertEquals("Linked correct Component Line.", mainComponentLine1.PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 1m, link1.WIP_ComponentQuantity);

			var link2 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part2.PK);
			AssertEquals("Linked correct Component Line.", mainComponentLine2.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.5m, link2.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, secondaryProductLinks.Count());

			var link3 = secondaryProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part1.PK);
			AssertEquals("Linked correct Component Line.", mainComponentLine1.PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.5m, link3.WIP_ComponentQuantity);

			var link4 = secondaryProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part2.PK);
			AssertEquals("Linked correct Component Line.", mainComponentLine2.PK, link4.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 0.25m, link4.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingWasteSecondaryProductsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var mainComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 1.5m);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine);

			var secondaryProductLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 10m);
			secondaryProductLine.IsSecondaryInwardProcessedItem = true;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 10m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 1.5m, link1.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link2 = secondaryProductLinks.Single();
			AssertEquals("Linked Kit Line for Waste Products.", mainLine.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked Full Kit Qty for Waste Products.", 5m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldNotFailIfTotalComponentQtyMatchesStockQuantityCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var mainComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			mainLine.ChildComponentLinesCollection.Add(mainComponentLine);

			var secondaryProductLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 10m);
			secondaryProductLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			secondaryProductLine.ChildComponentLinesCollection.Add(secondaryComponentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 10m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have no Link as total quantity used.", 0, mainProductLinks.Count());

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link = secondaryProductLinks.Single();
			AssertEquals("Linked correct Component Line.", mainComponentLine.PK, link.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 3m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = (WhsDynamicWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var line2 = (WhsDynamicWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			AssertEquals(componentReceive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));
			AssertEquals("All new Lines are MainProducts.", true, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().All(l => l.IsMainInwardProcessedItem && !l.IsSecondaryInwardProcessedItem));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links1.Count());

			var link1 = links1.Single();
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var links2 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links2.Count());

			var link2 = links2.Single();
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 1m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_UpdatesPackQuantityCorrectlyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 3m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);
			workOrder.RunPreSaveValidation();

			AssertEquals("Precondition: Package Qty is correct.", 6m, mainLine.WE_PackQuantity);
			AssertEquals("Precondition: Package Qty is correct.", 3m, componentLine.WE_PackQuantity);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = (WhsDynamicWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var line2 = (WhsDynamicWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			AssertEquals(componentReceive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Package Qty is correct.", 4m, line1.WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 2m, line2.WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 2m, line1.ChildComponentLines.Single().WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 1m, line2.ChildComponentLines.Single().WE_PackQuantity);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));
			AssertEquals("All new Lines are MainProducts.", true, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().All(l => l.IsMainInwardProcessedItem && !l.IsSecondaryInwardProcessedItem));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links1.Count());
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplitCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var part3 = Helper.CreateProduct("P3", data.Org1);
			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, "Ent-2");
			var componentReceive3 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 2m, "Ent-3");
			var componentReceive4 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 2m, "Ent-4");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 2m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 4m);
			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 2m);
			mainLine.ChildComponentLinesCollection.Add(componentLine1);
			mainLine.ChildComponentLinesCollection.Add(componentLine2);
			workOrder.RunPreSaveValidation();

			var pick = Helper.CreatePickNew(workOrder);
			// Pick all lines
			var now = ZDateTimeOffset.Now;
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = now);
			Factory.Save(); // create Outbound Transfers

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventory1 = componentReceive1.Lines[0].PK;
			var inventory2 = componentReceive2.Lines[0].PK;
			var inventory3 = componentReceive3.Lines[0].PK;
			var inventory4 = componentReceive4.Lines[0].PK;
			var line1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count == 2);
			var line2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count == 1);
			AssertEquals(1, line1.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_OriginalPickedInventoryLine == inventory1));
			AssertEquals(1, line1.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_OriginalPickedInventoryLine == inventory2));
			AssertEquals(inventory4, line1.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals(inventory3, line2.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals(inventory4, line2.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsNotInKitAmountsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 7m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 3, workOrder.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var line2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.Single().PickLines.Count == 1);
			var line3 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.Single().PickLines.Count == 2);
			AssertEquals(componentReceive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertContainsExactElementsInAnyOrder(new[] { componentReceive1.Lines[0].PK, componentReceive2.Lines[0].PK }, line3.ChildComponentLines.Single().PickLines.Select(pl => pl.WZ_WE_InventoryLine));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));
			AssertEquals("All new Lines are MainProducts.", true, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().All(l => l.IsMainInwardProcessedItem && !l.IsSecondaryInwardProcessedItem));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 3 Lines.", 3, receive.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = receive.Lines.Cast<WhsReceiveLine>()
				.Single(l => l.WE_TransactionQuantity == 1m && l.BOMComponentLinks.Single().ComponentLine.PickLines.Count == 1);
			var link1 = inventoryLineMainProduct1.BOMComponentLinks.Single();
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = receive.Lines.Cast<WhsReceiveLine>()
				.Single(l => l.WE_TransactionQuantity == 1m && l.BOMComponentLinks.Single().ComponentLine.PickLines.Count == 2);
			var link2 = inventoryLineMainProduct2.BOMComponentLinks.Single();
			AssertEquals("Linked correct Component Line.", line3.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link2.WIP_ComponentQuantity);

			var inventoryLineMainProduct3 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var links3 = inventoryLineMainProduct3.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links3.Count());

			var link3 = links3.Single();
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines.Single().PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 6m, link3.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsLessThanKitAmountCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 9m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);
			workOrder.RunPreSaveValidation();

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var line2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
			AssertEquals(1, line1.ChildComponentLines.Single().PickLines.Count(pl => pl.WZ_WE_InventoryLine == componentReceive1.Lines[0].PK));
			AssertEquals(1, line1.ChildComponentLines.Single().PickLines.Count(pl => pl.WZ_WE_InventoryLine == componentReceive2.Lines[0].PK));
			AssertEquals(componentReceive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));
			AssertEquals("All new Lines are MainProducts.", true, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().All(l => l.IsMainInwardProcessedItem && !l.IsSecondaryInwardProcessedItem));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = receive.Lines.Cast<WhsReceiveLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var link1 = inventoryLineMainProduct1.BOMComponentLinks.Single();
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = receive.Lines.Cast<WhsReceiveLine>().Single(l => l.WE_TransactionQuantity == 4m);
			var link2 = inventoryLineMainProduct2.BOMComponentLinks.Single();
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 8m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_GreedySplitScenarioCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var part3 = Helper.CreateProduct("P3", data.Org1);
			var receive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			var receive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 18m, "Ent-2");
			var receive3 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 3m, "Ent-3");
			var receive4 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 3m, "Ent-4");
			var receive5 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R5", part3, 4m, "Ent-5");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];
			workOrderLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 20m);
			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 10m);
			workOrderLine.ChildComponentLinesCollection.Add(componentLine1);
			workOrderLine.ChildComponentLinesCollection.Add(componentLine2);
			workOrder.RunPreSaveValidation();

			// Should Split the Component Lines as follows:
			// 1 Line with 1 kit from R1 & (R3 or R4)
			// 1 Line with 2 kit from R2 & (R3 or R4)
			// 1 Line with 3 kit from R2 & (R3 or R4)
			// 1 Line with 4 kit from R2 & R5
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 4, workOrder.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 4, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventory1 = receive1.Lines[0].PK;
			var inventory2 = receive2.Lines[0].PK;
			var inventory3 = receive3.Lines[0].PK;
			var inventory4 = receive4.Lines[0].PK;
			var inventory5 = receive5.Lines[0].PK;
			var line1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var line2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m);
			var line3 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var line4 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
			AssertEquals(inventory1, line1.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(1, line1.ChildComponentLines.Single(l => l.WE_OP == part3.PK)
				.PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory3 || pl.WZ_WE_InventoryLine == inventory4));
			AssertEquals(inventory2, line2.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(1, line2.ChildComponentLines.Single(l => l.WE_OP == part3.PK)
				.PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory3 || pl.WZ_WE_InventoryLine == inventory4));
			AssertEquals(inventory2, line3.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(1, line3.ChildComponentLines.Single(l => l.WE_OP == part3.PK)
				.PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory3 || pl.WZ_WE_InventoryLine == inventory4));
			AssertEquals(inventory2, line4.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(inventory5, line4.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 4 Lines.", 4, receive.Lines.Count);
			AssertEquals("Should have created 4 Lines for the same Product.", 4, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = receive.Lines.Cast<WhsReceiveLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var inventory1Link1 = inventoryLineMainProduct1.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 2m);
			var inventory1Link2 = inventoryLineMainProduct1.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 1m);
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines
				.Single(l => l.WE_OP == data.Part1.PK).PK, inventory1Link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines
				.Single(l => l.WE_OP == part3.PK).PK, inventory1Link2.WIP_WE_ComponentLine);

			var inventoryLineMainProduct2 = receive.Lines.Cast<WhsReceiveLine>().Single(l => l.WE_TransactionQuantity == 2m);
			var inventory2Link1 = inventoryLineMainProduct2.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 4m);
			var inventory2Link2 = inventoryLineMainProduct2.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 2m);
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines
				.Single(l => l.WE_OP == data.Part1.PK).PK, inventory2Link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines
				.Single(l => l.WE_OP == part3.PK).PK, inventory2Link2.WIP_WE_ComponentLine);

			var inventoryLineMainProduct3 = receive.Lines.Cast<WhsReceiveLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var inventory3Link1 = inventoryLineMainProduct3.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 6m);
			var inventory3Link2 = inventoryLineMainProduct3.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 3m);
			AssertEquals("Linked correct Component Line.", line3.ChildComponentLines
				.Single(l => l.WE_OP == data.Part1.PK).PK, inventory3Link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Line.", line3.ChildComponentLines
				.Single(l => l.WE_OP == part3.PK).PK, inventory3Link2.WIP_WE_ComponentLine);

			var inventoryLineMainProduct4 = receive.Lines.Cast<WhsReceiveLine>().Single(l => l.WE_TransactionQuantity == 4m);
			var inventory4Link1 = inventoryLineMainProduct4.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 8m);
			var inventory4Link2 = inventoryLineMainProduct4.BOMComponentLinks.Single(c => c.WIP_ComponentQuantity == 4m);
			AssertEquals("Linked correct Component Line.", line4.ChildComponentLines
				.Single(l => l.WE_OP == data.Part1.PK).PK, inventory4Link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Line.", line4.ChildComponentLines
				.Single(l => l.WE_OP == part3.PK).PK, inventory4Link2.WIP_WE_ComponentLine);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var part3 = Helper.CreateProduct("P3", data.Org1);
			var receive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, "Ent-1");
			var receive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, "Ent-2");
			var receive3 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 8m, "Ent-3");
			var receive4 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 12m, "Ent-4");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];
			workOrderLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 20m);
			workOrderLine.ChildComponentLinesCollection.Add(componentLine1);
			workOrderLine.ChildComponentLinesCollection.Add(componentLine2);
			workOrder.RunPreSaveValidation();

			// Should Split the Component Lines as follows:
			// 1 Line with 1 kit from R1 & R3
			// 1 Line with 1 kit from R2 & R3
			// 1 Line with 3 kit from R2 & R4
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 3, workOrder.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventory1 = receive1.Lines[0].PK;
			var inventory2 = receive2.Lines[0].PK;
			var inventory3 = receive3.Lines[0].PK;
			var inventory4 = receive4.Lines[0].PK;
			AssertEquals("Should have Two 1x Kit Lines.", 2, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Count(l => l.WE_TransactionQuantity == 1m));
			AssertEquals("Should have 1x Kit for R1 & R3.", 1, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory1) == 1
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory3) == 1));
			AssertEquals("Should have 1x Kit for R2 & R3.", 1, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory2) == 1
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory3) == 1));

			var lastLine = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
			AssertEquals(inventory2, lastLine.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(inventory4, lastLine.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 3 Lines.", 3, receive.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1Count = receive.Lines.Cast<WhsReceiveLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 2m)
					.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory1
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 4m)
					.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory3);
			AssertEquals("Linked correct Component & Qty.", 1, inventoryLineMainProduct1Count);

			var inventoryLineMainProduct2Count = receive.Lines.Cast<WhsReceiveLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 2m)
					.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory2
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 4m)
					.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory3);
			AssertEquals("Linked correct Component & Qty.", 1, inventoryLineMainProduct2Count);

			var inventoryLineMainProduct3 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var links = inventoryLineMainProduct3.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links.Count());

			var link1 = links.Single(l => l.WIP_ComponentQuantity == 6m);
			var link2 = links.Single(l => l.WIP_ComponentQuantity == 12m);
			AssertEquals("Linked correct Component Line & Qty.", lastLine.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Line & Qty.", lastLine.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PK, link2.WIP_WE_ComponentLine);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsOfDifferingInventoryAmountsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var part3 = Helper.CreateProduct("P3", data.Org1);
			var receive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m, "Ent-1");
			var receive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 7m, "Ent-2");
			var receive3 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 4m, "Ent-3");
			var receive4 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 4m, "Ent-4");
			var receive5 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R5", part3, 12m, "Ent-5");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];
			workOrderLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 20m);
			workOrderLine.ChildComponentLinesCollection.Add(componentLine1);
			workOrderLine.ChildComponentLinesCollection.Add(componentLine2);
			workOrder.RunPreSaveValidation();

			// Should Split the Component Lines as follows:
			// 1 Line with 1 kit from R1 & (R3 or R4)
			// 1 Line with 1 kit from R1 & R2 & (R3 or R4)
			// 1 Line with 3 kit from R2 & R5
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 3, workOrder.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventory1 = receive1.Lines[0].PK;
			var inventory2 = receive2.Lines[0].PK;
			var inventory3 = receive3.Lines[0].PK;
			var inventory4 = receive4.Lines[0].PK;
			var inventory5 = receive5.Lines[0].PK;
			var line1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.All(cl => cl.PickLines.Count == 1));
			var line2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.Any(cl => cl.PickLines.Count > 1));
			AssertEquals("Should have 1x Kit for R1 & (R3 or R4).", line1, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>()
				.Single(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Count == 1 && cl.PickLines.Single().WZ_WE_InventoryLine == inventory1) == 1
				&&
				(
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory3
					||
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory4)
				));
			AssertEquals("Should have 1x Kit for R1 & R2 & (R3 or R4).", line2, workOrder.Lines.Cast<WhsDynamicWorkOrderLine>()
				.Single(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory1) == 1
				&& l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory2) == 1
				&&
				(
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory3
					||
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory4)
				));

			var line3 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
			AssertEquals(inventory2, line3.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(inventory5, line3.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 3 Lines.", 3, receive.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1Count = receive.Lines.Cast<WhsReceiveLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 2m)
					.ComponentLine.PickLines.Count == 1
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 2m)
					.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory1
					&&
					(
						l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 4m)
						.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory3
						||
						l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 4m)
						.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory4
					));
			AssertEquals("Linked correct Component & Qty.", 1, inventoryLineMainProduct1Count);

			var inventoryLineMainProduct2Count = receive.Lines.Cast<WhsReceiveLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 2m)
					.ComponentLine.PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory1) == 1
					&& l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 2m)
					.ComponentLine.PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory2) == 1
					&&
					(
						l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 4m)
						.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory3
						||
						l.BOMComponentLinks.Single(b => b.WIP_ComponentQuantity == 4m)
						.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == inventory4
					));
			AssertEquals("Linked correct Component & Qty.", 1, inventoryLineMainProduct2Count);

			var inventoryLineMainProduct3 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var links = inventoryLineMainProduct3.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links.Count());

			var link1 = links.Single(l => l.WIP_ComponentQuantity == 6m);
			var link2 = links.Single(l => l.WIP_ComponentQuantity == 12m);
			AssertEquals("Linked correct Component Line & Qty.", line3.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Line & Qty.", line3.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PK, link2.WIP_WE_ComponentLine);
			AssertNoExceptionThrown(() => Factory.Save());

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts(matchingLineSetBeforeFinalization: true);

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_MatchingLinesSetOnFinalize()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts(matchingLineSetBeforeFinalization: false);

		void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts(bool matchingLineSetBeforeFinalization)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 8m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 12m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 3m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 6m);
			secondaryLine.ChildComponentLinesCollection.Add(secondaryComponentLine);

			AssertEquals("Precondition: Package Qty is correct.", 6m, mainLine.WE_PackQuantity);
			AssertEquals("Precondition: Package Qty is correct.", 12m, componentLine.WE_PackQuantity);
			AssertEquals("Precondition: Package Qty is correct.", 3m, secondaryLine.WE_PackQuantity);
			AssertEquals("Precondition: Package Qty is correct.", 6m, secondaryComponentLine.WE_PackQuantity);

			Helper.CreatePickNew(workOrder);

			if (matchingLineSetBeforeFinalization)
			{
				workOrder.RunPreSaveValidation();
			}
			else
			{
				AssertEquals("Matching line should *not* be set.", ZGuid.Empty, secondaryComponentLine.WE_WE_MatchingLine);
			}

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 4, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for the secondary Product.", 2, workOrder.Lines.Count(l => l.WE_OP == part3.PK));

			var mainLine1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
			var mainLine2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m && l.IsMainInwardProcessedItem);
			var mainLine1Component = mainLine1.ChildComponentLines.Single();
			var mainLine2Component = mainLine2.ChildComponentLines.Single();
			AssertEquals(componentReceive1.Lines[0].PK, mainLine1Component.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive2.Lines[0].PK, mainLine2Component.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Package Qty is correct.", 4m, mainLine1.WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 2m, mainLine2.WE_PackQuantity);
			AssertEquals("Should be a main product.", true, mainLine1.IsMainInwardProcessedItem);
			AssertEquals("Should be a main product.", false, mainLine1.IsSecondaryInwardProcessedItem);
			AssertEquals("Should be a main product.", true, mainLine2.IsMainInwardProcessedItem);
			AssertEquals("Should be a main product.", false, mainLine2.IsSecondaryInwardProcessedItem);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));

			var secondaryLine1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m && !l.IsMainInwardProcessedItem);
			var secondaryLine2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m);
			AssertEquals("Package Qty is correct.", 2m, secondaryLine1.WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 1m, secondaryLine2.WE_PackQuantity);
			AssertEquals("Should be a secondary product.", false, secondaryLine1.IsMainInwardProcessedItem);
			AssertEquals("Should be a secondary product.", true, secondaryLine1.IsSecondaryInwardProcessedItem);
			AssertEquals("Should be a secondary product.", false, secondaryLine2.IsMainInwardProcessedItem);
			AssertEquals("Should be a secondary product.", true, secondaryLine2.IsSecondaryInwardProcessedItem);

			var secondaryLine1Component = (WhsDynamicWorkOrderLine)secondaryLine1.ChildComponentLinesCollection.Single();
			var secondaryLine2Component = (WhsDynamicWorkOrderLine)secondaryLine2.ChildComponentLinesCollection.Single();
			AssertEquals("Secondary transaction quantity.", 4m, secondaryLine1Component.WE_TransactionQuantity);
			AssertEquals("Secondary transaction quantity.", 2m, secondaryLine2Component.WE_TransactionQuantity);
			AssertEquals("Secondary package quantity.", 4m, secondaryLine1Component.WE_PackQuantity);
			AssertEquals("Secondary package quantity.", 2m, secondaryLine2Component.WE_PackQuantity);
			AssertEquals("Should have no pick lines.", 0, secondaryLine1Component.PickLines.Count);
			AssertEquals("Should have no pick lines.", 0, secondaryLine2Component.PickLines.Count);
			AssertEquals("Matching lines should be correctly set.", mainLine1Component.PK, secondaryLine1Component.WE_WE_MatchingLine);
			AssertEquals("Matching lines should be correctly set.", mainLine2Component.PK, secondaryLine2Component.WE_WE_MatchingLine);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 4 Lines.", 4, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for the secondary Product.", 2, receive.Lines.Count(l => l.WE_OP == part3.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links1.Count());

			var link1 = links1.Single();
			AssertEquals("Linked correct Component Line.", mainLine1Component.PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 4m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_OP == data.Part2.PK);
			var links2 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links2.Count());

			var link2 = links2.Single();
			AssertEquals("Linked correct Component Line.", mainLine2Component.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link2.WIP_ComponentQuantity);

			var inventoryLineSecondaryProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_OP == part3.PK);
			var links3 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links3.Count());

			var link3 = links3.Single();
			AssertEquals("Linked correct Component Line.", mainLine1Component.PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 4m, link3.WIP_ComponentQuantity);

			var inventoryLineSecondaryProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			var links4 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links4.Count());

			var link4 = links4.Single();
			AssertEquals("Linked correct Component Line.", mainLine2Component.PK, link4.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link4.WIP_ComponentQuantity);

			workOrder.RunPreSaveValidation();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Matching lines should remain correctly set.", mainLine1Component.PK, secondaryLine1Component.WE_WE_MatchingLine);
			AssertEquals("Matching lines should remain correctly set.", mainLine2Component.PK, secondaryLine2Component.WE_WE_MatchingLine);

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);

			AssertContainsExactElementsInAnyOrder(new[] { mainLine1Component, mainLine2Component }, workOrder.GetLinesToPick());
			AssertContainsExactElementsInAnyOrder(new[] { mainLine1Component, mainLine2Component }, workOrder.LinesToPickForBinding);
		}

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities_1()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities(decimalPlacesToTest: 1);

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities_2()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities(decimalPlacesToTest: 2);

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities_3()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities(decimalPlacesToTest: 3);

		void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_DecimalQuantities(byte decimalPlacesToTest)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_CountDecimalPlaces = decimalPlacesToTest;
			var part3 = Helper.CreateProduct("P3", data.Org1);
			part3.OP_CountDecimalPlaces = decimalPlacesToTest;
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 8m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 12m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 2m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 7m);
			secondaryLine.ChildComponentLinesCollection.Add(secondaryComponentLine);

			workOrder.RunPreSaveValidation();
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 4, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for the secondary Product.", 2, workOrder.Lines.Count(l => l.WE_OP == part3.PK));

			var mainLine1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
			var mainLine2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m && l.IsMainInwardProcessedItem);
			AssertEquals(componentReceive1.Lines[0].PK, mainLine1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive2.Lines[0].PK, mainLine2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Should be a main product.", true, mainLine1.IsMainInwardProcessedItem);
			AssertEquals("Should be a main product.", false, mainLine1.IsSecondaryInwardProcessedItem);
			AssertEquals("Should be a main product.", true, mainLine2.IsMainInwardProcessedItem);
			AssertEquals("Should be a main product.", false, mainLine2.IsSecondaryInwardProcessedItem);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));

			var secondaryLine1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == Utilities.Round(1.333m, decimalPlacesToTest) && !l.IsMainInwardProcessedItem);
			var secondaryLine2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == Utilities.Round(0.667m, decimalPlacesToTest));
			AssertEquals("Should be a secondary product.", false, secondaryLine1.IsMainInwardProcessedItem);
			AssertEquals("Should be a secondary product.", true, secondaryLine1.IsSecondaryInwardProcessedItem);
			AssertEquals("Should be a secondary product.", false, secondaryLine2.IsMainInwardProcessedItem);
			AssertEquals("Should be a secondary product.", true, secondaryLine2.IsSecondaryInwardProcessedItem);

			var secondaryLine1Component = (WhsDynamicWorkOrderLine)secondaryLine1.ChildComponentLinesCollection.Single();
			var secondaryLine2Component = (WhsDynamicWorkOrderLine)secondaryLine2.ChildComponentLinesCollection.Single();
			AssertEquals("Secondary transaction quantity.", Utilities.Round(4.667m, decimalPlacesToTest), secondaryLine1Component.WE_TransactionQuantity);
			AssertEquals("Secondary transaction quantity.", Utilities.Round(2.333m, decimalPlacesToTest), secondaryLine2Component.WE_TransactionQuantity);
			AssertEquals("Should have no pick lines.", 0, secondaryLine1Component.PickLines.Count);
			AssertEquals("Should have no pick lines.", 0, secondaryLine2Component.PickLines.Count);
			AssertEquals("Matching lines should be correctly set.", mainLine1.ChildComponentLines.Single().PK, secondaryLine1Component.WE_WE_MatchingLine);
			AssertEquals("Matching lines should be correctly set.", mainLine2.ChildComponentLines.Single().PK, secondaryLine2Component.WE_WE_MatchingLine);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 4 Lines.", 4, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for the secondary Product.", 2, receive.Lines.Count(l => l.WE_OP == part3.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links1.Count());

			var link1 = links1.Single();
			AssertEquals("Linked correct Component Line.", mainLine1.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", Utilities.Round(3.333m, decimalPlacesToTest), link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_OP == data.Part2.PK);
			var links2 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links2.Count());

			var link2 = links2.Single();
			AssertEquals("Linked correct Component Line.", mainLine2.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", Utilities.Round(1.667m, decimalPlacesToTest), link2.WIP_ComponentQuantity);

			var inventoryLineSecondaryProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == Utilities.Round(1.333m, decimalPlacesToTest) && l.WE_OP == part3.PK);
			var links3 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links3.Count());

			var link3 = links3.Single();
			AssertEquals("Linked correct Component Line.", mainLine1.ChildComponentLines.Single().PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", Utilities.Round(3.333m, decimalPlacesToTest), link3.WIP_ComponentQuantity);

			var inventoryLineSecondaryProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == Utilities.Round(0.667m, decimalPlacesToTest));
			var links4 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links4.Count());

			var link4 = links4.Single();
			AssertEquals("Linked correct Component Line.", mainLine2.ChildComponentLines.Single().PK, link4.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", Utilities.Round(1.667m, decimalPlacesToTest), link4.WIP_ComponentQuantity);

			workOrder.RunPreSaveValidation();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Matching lines should remain correctly set.", mainLine1.ChildComponentLines.Single().PK, secondaryLine1Component.WE_WE_MatchingLine);
			AssertEquals("Matching lines should remain correctly set.", mainLine2.ChildComponentLines.Single().PK, secondaryLine2Component.WE_WE_MatchingLine);

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_BadQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m, "Ent-2");
			var componentReceive3 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 4m, "Ent-3");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 12m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 1m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 6m);
			secondaryLine.ChildComponentLinesCollection.Add(secondaryComponentLine);

			workOrder.RunPreSaveValidation();
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var notify = (NotificationBuffer)workOrder.NotificationSubscriber;
			AssertEquals(true, notify.HasErrors);
			AssertEquals(@"Attempted to split lines into a fractional quantity beyond what is allowed with the Product's configured decimal places.

To finalize this Dynamic Work Order, allocate uniform components, create separate Dynamic Work Orders for each unit of the Main Product or alter the configured decimal places on the Product master file.

Affected Product(s): P3", notify.AsString.Trim());
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as an error occurred during Finalization. Please reload the Dynamic Work Order.", Factory.Save);
		}

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithSecondaryProducts_MultipleComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 8m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m, "Ent-2");
			var componentReceive3 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R3", part4, 24m, "Ent-3");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 12m);
			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part4, 24m);
			mainLine.ChildComponentLinesCollection.Add(componentLine1);
			mainLine.ChildComponentLinesCollection.Add(componentLine2);

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 3m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 6m);
			var secondaryComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part4, 6m);
			secondaryLine.ChildComponentLinesCollection.Add(secondaryComponentLine1);
			secondaryLine.ChildComponentLinesCollection.Add(secondaryComponentLine2);

			workOrder.RunPreSaveValidation();
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 4, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for the first secondary Product.", 2, workOrder.Lines.Count(l => l.WE_OP == part3.PK));

			var mainLine1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
			var mainLine1Component1 = mainLine1.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK);
			var mainLine1Component2 = mainLine1.ChildComponentLines.Single(l => l.WE_OP == part4.PK);

			var mainLine2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m && l.IsMainInwardProcessedItem);
			var mainLine2Component1 = mainLine2.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK);
			var mainLine2Component2 = mainLine2.ChildComponentLines.Single(l => l.WE_OP == part4.PK);

			AssertEquals(componentReceive1.Lines[0].PK, mainLine1Component1.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive2.Lines[0].PK, mainLine2Component1.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive3.Lines[0].PK, mainLine1Component2.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(componentReceive3.Lines[0].PK, mainLine2Component2.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Should be a main product.", true, mainLine1.IsMainInwardProcessedItem);
			AssertEquals("Should be a main product.", false, mainLine1.IsSecondaryInwardProcessedItem);
			AssertEquals("Should be a main product.", true, mainLine2.IsMainInwardProcessedItem);
			AssertEquals("Should be a main product.", false, mainLine2.IsSecondaryInwardProcessedItem);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == mainLine.WE_FinalisedDate));

			var secondaryLine1 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m && !l.IsMainInwardProcessedItem);
			var secondaryLine2 = workOrder.Lines.Cast<WhsDynamicWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m);
			AssertEquals("Should be a secondary product.", false, secondaryLine1.IsMainInwardProcessedItem);
			AssertEquals("Should be a secondary product.", true, secondaryLine1.IsSecondaryInwardProcessedItem);
			AssertEquals("Should be a secondary product.", false, secondaryLine2.IsMainInwardProcessedItem);
			AssertEquals("Should be a secondary product.", true, secondaryLine2.IsSecondaryInwardProcessedItem);

			var secondaryLine1Component1 = (WhsDynamicWorkOrderLine)secondaryLine1.ChildComponentLinesCollection.Single(l => l.WE_OP == data.Part1.PK);
			var secondaryLine1Component2 = (WhsDynamicWorkOrderLine)secondaryLine1.ChildComponentLinesCollection.Single(l => l.WE_OP == part4.PK);
			var secondaryLine2Component1 = (WhsDynamicWorkOrderLine)secondaryLine2.ChildComponentLinesCollection.Single(l => l.WE_OP == data.Part1.PK);
			var secondaryLine2Component2 = (WhsDynamicWorkOrderLine)secondaryLine2.ChildComponentLinesCollection.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Secondary transaction quantity.", 4m, secondaryLine1Component1.WE_TransactionQuantity);
			AssertEquals("Secondary transaction quantity.", 2m, secondaryLine2Component1.WE_TransactionQuantity);
			AssertEquals("Should have no pick lines.", 0, secondaryLine1Component1.PickLines.Count);
			AssertEquals("Should have no pick lines.", 0, secondaryLine1Component2.PickLines.Count);
			AssertEquals("Should have no pick lines.", 0, secondaryLine2Component1.PickLines.Count);
			AssertEquals("Should have no pick lines.", 0, secondaryLine2Component2.PickLines.Count);
			AssertEquals("Matching lines should be correctly set.", mainLine1Component1.PK, secondaryLine1Component1.WE_WE_MatchingLine);
			AssertEquals("Matching lines should be correctly set.", mainLine1Component2.PK, secondaryLine1Component2.WE_WE_MatchingLine);
			AssertEquals("Matching lines should be correctly set.", mainLine2Component1.PK, secondaryLine2Component1.WE_WE_MatchingLine);
			AssertEquals("Matching lines should be correctly set.", mainLine2Component2.PK, secondaryLine2Component2.WE_WE_MatchingLine);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 4 Lines.", 4, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for the secondary Product.", 2, receive.Lines.Count(l => l.WE_OP == part3.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links1.Count());

			var link1Component1 = links1.Single(l => l.WIP_WE_ComponentLine == mainLine1Component1.PK);
			AssertEquals("Linked correct Component Qty.", 4m, link1Component1.WIP_ComponentQuantity);

			var link1Component2 = links1.Single(l => l.WIP_WE_ComponentLine == mainLine1Component2.PK);
			AssertEquals("Linked correct Component Qty.", 12m, link1Component2.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_OP == data.Part2.PK);
			var links2 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links2.Count());

			var link2Component1 = links2.Single(l => l.WIP_WE_ComponentLine == mainLine2Component1.PK);
			AssertEquals("Linked correct Component Qty.", 2m, link2Component1.WIP_ComponentQuantity);

			var link2Component2 = links2.Single(l => l.WIP_WE_ComponentLine == mainLine2Component2.PK);
			AssertEquals("Linked correct Component Qty.", 6m, link2Component2.WIP_ComponentQuantity);

			var inventoryLineSecondaryProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_OP == part3.PK);
			var links3 = inventoryLineSecondaryProduct1.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links3.Count());

			var link3Component1 = links3.Single(l => l.WIP_WE_ComponentLine == mainLine1Component1.PK);
			AssertEquals("Linked correct Component Qty.", 4m, link3Component1.WIP_ComponentQuantity);

			var link3Component2 = links3.Single(l => l.WIP_WE_ComponentLine == mainLine1Component2.PK);
			AssertEquals("Linked correct Component Qty.", 4m, link3Component2.WIP_ComponentQuantity);

			var inventoryLineSecondaryProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			var links4 = inventoryLineSecondaryProduct2.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links4.Count());

			var link4Component1 = links4.Single(l => l.WIP_WE_ComponentLine == mainLine2Component1.PK);
			AssertEquals("Linked correct Component Qty.", 2m, link4Component1.WIP_ComponentQuantity);

			var link4Component2 = links4.Single(l => l.WIP_WE_ComponentLine == mainLine2Component2.PK);
			AssertEquals("Linked correct Component Qty.", 2m, link4Component2.WIP_ComponentQuantity);

			workOrder.RunPreSaveValidation();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Matching lines should remain correctly set.", mainLine1Component1.PK, secondaryLine1Component1.WE_WE_MatchingLine);
			AssertEquals("Matching lines should remain correctly set.", mainLine1Component2.PK, secondaryLine1Component2.WE_WE_MatchingLine);
			AssertEquals("Matching lines should remain correctly set.", mainLine2Component1.PK, secondaryLine2Component1.WE_WE_MatchingLine);
			AssertEquals("Matching lines should remain correctly set.", mainLine2Component2.PK, secondaryLine2Component2.WE_WE_MatchingLine);

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_WithMultipleSecondaryProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			SetupWarehouse(data.Whs1);

			var componentReceive1 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 8m, "Ent-1");
			var componentReceive2 = CreateIPRReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m, "Ent-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 12m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);

			var secondaryLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 3m);
			secondaryLine1.IsSecondaryInwardProcessedItem = true;

			var secondary1ComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 3m);
			secondaryLine1.ChildComponentLinesCollection.Add(secondary1ComponentLine);

			var secondaryLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part4, 6m);
			secondaryLine2.IsSecondaryInwardProcessedItem = true;

			var secondary2ComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 3m);
			secondaryLine2.ChildComponentLinesCollection.Add(secondary2ComponentLine);

			workOrder.RunPreSaveValidation();
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, mainLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, mainLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 6, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for secondary Product 1.", 2, workOrder.Lines.Count(l => l.WE_OP == part3.PK));
			AssertEquals("Should have created 2 Lines for secondary Product 2.", 2, workOrder.Lines.Count(l => l.WE_OP == part4.PK));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 6 Lines.", 6, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the main Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for secondary Product 1.", 2, receive.Lines.Count(l => l.WE_OP == part3.PK));
			AssertEquals("Should have created 2 Lines for secondary Product 2.", 2, receive.Lines.Count(l => l.WE_OP == part4.PK));

			var bomLinks = receive.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.BOMComponentLinks).ToArray();
			AssertEquals("Should have 6 links.", 6, bomLinks.Length);
			AssertEquals("Should have fully linked receive1.", 8m, bomLinks.Where(l => l.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == componentReceive1.Lines[0].PK).Sum(l => l.WIP_ComponentQuantity));
			AssertEquals("Should have fully linked receive2.", 4m, bomLinks.Where(l => l.ComponentLine.PickLines.Single().WZ_WE_InventoryLine == componentReceive2.Lines[0].PK).Sum(l => l.WIP_ComponentQuantity));

			workOrder.RunPreSaveValidation();
			AssertEquals("Should have no errors.", false, workOrder.HasErrors);
		}

		#endregion

		#region CustomFieldsSupported

		protected override bool CustomFieldsSupported => false;

		protected override bool ExpectedIsBondedEntryKeyVisibleForCustomsTransactions => true;

		#endregion

		#region	IWhsJobTemplateCopyable Members

		protected override void AssertTemplateCopy(WhsDynamicWorkOrder source, WhsDynamicWorkOrder copy, bool shouldCopyLines)
		{
			base.AssertTemplateCopy(source, copy, shouldCopyLines);

			AssertEquals("Total cubic on source should not be changed by copy", 10.9m, source.WD_TotalCubic);
			AssertEquals("Total weight on source should not be changed by copy", 147m, source.WD_TotalWeight);
			AssertEquals("Total cubic should be copied.", 10.9m, copy.WD_TotalCubic);
			AssertEquals("Total weight should be copied.", 147m, copy.WD_TotalWeight);

			if (shouldCopyLines)
			{
				AssertEquals(2, copy.Lines.Count);
				AssertEquals(7, copy.AllLines.Count);

				var linesArray = copy.AllLines.Cast<WhsDynamicWorkOrderLine>().ToArray();
				var mainLine = linesArray.Single(l => l.IsMainInwardProcessedItem);
				AssertEquals("Product should be correct.", "JUICE", mainLine.ProductCode);
				AssertEquals("Transaction Quantity should be correct.", 1m, mainLine.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, mainLine.WE_WE_ParentDocketLine);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, mainLine.WE_WE_MatchingLine);

				var mainComponent1Line = linesArray.Single(l => l.WE_WE_ParentDocketLine == mainLine.PK && l.ProductCode == "ORANGE");
				AssertEquals("Transaction Quantity should be correct.", 1m, mainComponent1Line.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, mainComponent1Line.WE_WE_MatchingLine);

				var mainComponent2Line = linesArray.Single(l => l.WE_WE_ParentDocketLine == mainLine.PK && l.ProductCode == "WATER");
				AssertEquals("Transaction Quantity should be correct.", 2m, mainComponent2Line.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, mainComponent2Line.WE_WE_MatchingLine);

				var mainComponent3Line = linesArray.Single(l => l.WE_WE_ParentDocketLine == mainLine.PK && l.ProductCode == "POTASSIUMBENZOATE");
				AssertEquals("Transaction Quantity should be correct.", 10m, mainComponent3Line.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, mainComponent3Line.WE_WE_MatchingLine);

				var secondaryLine = linesArray.Single(l => l.IsSecondaryInwardProcessedItem);
				AssertEquals("Product should be correct.", "PEEL", secondaryLine.ProductCode);
				AssertEquals("Transaction Quantity should be correct.", 1m, secondaryLine.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, secondaryLine.WE_WE_ParentDocketLine);
				AssertEquals("Line should be unlinked.", ZGuid.Empty, secondaryLine.WE_WE_MatchingLine);

				var secondaryComponent1Line = linesArray.Single(l => l.WE_WE_ParentDocketLine == secondaryLine.PK && l.ProductCode == "ORANGE");
				AssertEquals("Transaction Quantity should be correct.", 0.1m, secondaryComponent1Line.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", mainComponent1Line.PK, secondaryComponent1Line.WE_WE_MatchingLine);

				var secondaryComponent3Line = linesArray.Single(l => l.WE_WE_ParentDocketLine == secondaryLine.PK && l.ProductCode == "POTASSIUMBENZOATE");
				AssertEquals("Transaction Quantity should be correct.", 1m, secondaryComponent3Line.WE_TransactionQuantity);
				AssertEquals("Line should be unlinked.", mainComponent3Line.PK, secondaryComponent3Line.WE_WE_MatchingLine);

				AssertContainsExactElementsInAnyOrder(new[] { mainLine, secondaryLine }, copy.Lines);
			}
			else
			{
				AssertEquals(0, copy.Lines.Count);
				AssertEquals(0, copy.AllLines.Count);
			}
		}

		protected override WhsDynamicWorkOrder GetNewDocketForTemplateCopy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.MainProductLine_Juice.WE_TransactionQuantity = 0m;
			setup.MainComponentLine_Orange.WE_TransactionQuantity = 0m;
			setup.MainComponentLine_Water.WE_TransactionQuantity = 0m;
			setup.MainComponentLine_PotassiumBenzoate.WE_TransactionQuantity = 0m;
			setup.SecondaryProductLine_Peel.WE_TransactionQuantity = 0m;
			setup.SecondaryComponentLine_Orange.WE_TransactionQuantity = 0m;
			setup.SecondaryComponentLine_PotassiumBenzoate.WE_TransactionQuantity = 0m;

			setup.MainProductLine_Juice.WE_TransactionQuantity = 1m;
			setup.MainComponentLine_Orange.WE_TransactionQuantity = 1m;
			setup.MainComponentLine_Water.WE_TransactionQuantity = 2m;
			setup.MainComponentLine_PotassiumBenzoate.WE_TransactionQuantity = 10m;
			setup.SecondaryProductLine_Peel.WE_TransactionQuantity = 1m;
			setup.SecondaryComponentLine_Orange.WE_TransactionQuantity = 0.1m;
			setup.SecondaryComponentLine_PotassiumBenzoate.WE_TransactionQuantity = 1m;

			return setup.WorkOrder;
		}

		#endregion

		#region TestDefaultDocketSubTypeForCustomsTransaction

		public override void TestDefaultDocketSubTypeForCustomsTransaction()
		{
			AssertEquals(WorkOrderType.Codes.Assemble, GetNewBusinessObject().DefaultDocketSubTypeForCustomsTransaction);
		}

		#endregion

		#region IWhsLogEventParent

		protected override string ExpectedEventReferenceParameterType => Constants.EventReferenceParameterTypes.DynamicWorkOrder;

		#endregion

		#region IWorkflowProvider

		public void TestIWorkflowProvider()
		{
			var workOrder = (IWorkflowProvider)Factory.New<WhsDynamicWorkOrder>();
			AssertEquals(WorkflowDescriptors.WhsDynamicWorkOrderWorkflowDescriptorCode, workOrder.WorkflowType);
			AssertType<WhsDynamicWorkOrderProcessTasksCollection>(workOrder.WorkflowItems);
		}

		#endregion

		#region TestUniversalDataContext

		protected override DataContextType? ExpectedDataContextType => DataContextType.WarehouseDynamicWorkOrder;

		#endregion

		#region TestUpdateWP_CriticalChangesVersionID

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_IsInwardsProcessingJob()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_IsInwardsProcessingJob = false, true);
		}

		#endregion

		#region TestDocketUpdatedByDataRefresh

		protected override void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			// create staging area
			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING", AreaTypes.Codes.InwardProcessing);
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;

			// create a BOM staging location for the Juice
			var productParams = WhsProduct.GetWhsProduct(setup.Juice_MainProduct).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocation.PK;
			Factory.Save();

			var dynamicWorkOrder = setup.WorkOrder;
			var newFactory = new BusinessObjectFactory();
			var dynamicWorkOrderInNewFactory = newFactory.Load<WhsDynamicWorkOrder>(dynamicWorkOrder.PK);

			Helper.CreatePickNew(dynamicWorkOrder);

			dynamicWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals(true, dynamicWorkOrder.IsFinalised);

			dynamicWorkOrderInNewFactory.WD_ExternalReference = "NEWDW1";
			AssertEquals(false, dynamicWorkOrderInNewFactory.IsFinalised);
			newFactory.Save();

			AssertEquals("Dynamic Work Order is still finalised after data refresh.", true, dynamicWorkOrder.IsFinalised);
			AssertEquals("External reference is updated after data refresh.", "NEWDW1", dynamicWorkOrder.WD_ExternalReference);
			Helper.AssertZCannotSaveExceptionThrown("The Dynamic Work Order has been updated by another job. Please reload the Dynamic Work Order.", Factory.Save);
		}

		protected override void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupWarehouse(data.Whs1);

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			// create staging area
			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING", AreaTypes.Codes.InwardProcessing);
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;

			// create a BOM staging location for the Juice
			var productParams = WhsProduct.GetWhsProduct(setup.Juice_MainProduct).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocation.PK;
			Factory.Save();

			var dynamicWorkOrder = setup.WorkOrder;
			var newFactory = new BusinessObjectFactory();
			var dynamicWorkOrderInNewFactory = newFactory.Load<WhsDynamicWorkOrder>(dynamicWorkOrder.PK);

			var newCriticalChangesVersionID = ZGuid.NewZGuid();
			AssertNotEquals(dynamicWorkOrder.WD_CriticalChangesVersionID, newCriticalChangesVersionID);

			dynamicWorkOrderInNewFactory.WD_CriticalChangesVersionID = newCriticalChangesVersionID;
			newFactory.Save();

			AssertEquals("WD_CriticalChangesVersionID is updated after data refresh.", newCriticalChangesVersionID, dynamicWorkOrder.WD_CriticalChangesVersionID);
			Helper.CreatePickNew(dynamicWorkOrder);

			var notify = (NotificationBuffer)setup.WorkOrder.NotificationManager.Peek;
			dynamicWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Dynamic Work Order should have the 'need to reload' notification", true, notify.ContainsNotificationType(WhsErrorTypes.CannotFinaliseWithoutReload));
		}

		#endregion
	}
}
