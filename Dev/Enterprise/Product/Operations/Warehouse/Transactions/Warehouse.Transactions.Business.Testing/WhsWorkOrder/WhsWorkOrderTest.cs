using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrder))]
	class WhsWorkOrderTest : WhsComponentOrderTest<WhsWorkOrder>
	{
		#region Business Object Overrides

		protected override void TestCloneCore(WhsWorkOrder originalDocket, WhsWorkOrder clonedDocket)
		{
			var workOrder = clonedDocket;
			AssertEquals("ConsigneePK", ZGuid.Empty, workOrder.ConsigneePK);
			AssertEquals("TransportCoPK", ZGuid.Empty, workOrder.TransportCoPK);
			AssertEquals("WD_WLO_PlannedLoad not cloned", ZGuid.Empty, workOrder.WD_WLO_PlannedLoad);
		}

		#endregion

		#region Fetch Strategy

		protected override Type FetchStrategyType
			=> typeof(WhsWorkOrderFetchStrategy);

		#endregion

		#region Related Business Objects

		#region TestConsignee

		#region TestConsigneeNameOrPK

		protected override void TestConsigneeNameOrPKReadOnlyCore()
		{
			TestReadOnly(d => d.ConsigneeNameOrPKInfo, true, true, true, true);
		}

		#endregion

		#region TestConsigneeDocAddress_ReadOnlyIsLazyTriggered

		public void TestConsigneeDocAddress_ReadOnlyIsLazyTriggered()
		{
			var workOrder = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", workOrder.ConsigneeDocAddress);

			JobDocAddress consigneeDocAddress = null;
			AssertPersistentPropertiesHitCount("Getting ConsigneeDocAddress should only trigger Parent Docket hit.", 1, () => consigneeDocAddress = workOrder.ConsigneeDocAddress);

			AssertPersistentPropertiesHitCount("Poking at ReadOnly property of ConsigneeDocAddress should not cause any complex behaviour as it is always readonly.", 0, () => _ = consigneeDocAddress.ReadOnly);
		}

		#endregion

		#endregion

		#region TestLines

		protected override Type ExpectedLineCollectionType => typeof(WhsWorkOrderLineCollection);

		#endregion

		#region TestDisassemblyLinesForPick

		public void TestDisassemblyLinesForPick()
		{
			AssertEquals(WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPick, ((WhsWorkOrderLineCollection)Docket.DisassemblyLinesForPick).FilterStrategy);
		}

		#endregion

		#region TestDisassemblyLinesForPutaway

		public void TestDisassemblyLinesForPutaway()
		{
			AssertEquals(WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPutaway, ((WhsWorkOrderLineCollection)Docket.DisassemblyLinesForPutaway).FilterStrategy);
		}

		#endregion

		#region TestGetLinesToPick

		// Kapitan wrote this test.
		public override void TestGetLinesToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			OrgSupplierPart part11 = Helper.CreateProduct(data.Org1, "P11");
			OrgSupplierPart part12 = Helper.CreateProduct(data.Org1, "P12");
			OrgSupplierPart part111 = Helper.CreateProduct(data.Org1, "P111");

			Helper.CreateProductBOM(data.Part1, part11, 2m, "UNT");
			Helper.CreateProductBOM(data.Part1, part12, 3m, "BAG");
			Helper.CreateProductBOM(part11, part111, 5m, "PLT");

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine line1 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 1m);
			WhsWorkOrderLine line2 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 2m);

			WhsWorkOrderLine line11 = null;
			WhsWorkOrderLine line12 = null;

			WhsWorkOrderLineCollection linesToPick = (WhsWorkOrderLineCollection)workOrder.GetLinesToPick();
			foreach (WhsWorkOrderLine line in linesToPick)
			{
				switch (line.SupplierPart.OP_PartNum)
				{
					case "P1":
					case "P2":
						Assert("Main product should not be picked", false);
						break;
					case "P11":
						line11 = line;
						break;
					case "P12":
						line12 = line;
						break;
					case "P111":
						Assert("Sub component should not be directly picked", false);
						break;
					default:
						Assert("Invalid Product: " + line.SupplierPart.OP_PartNum, false);
						break;
				}
			}

			AssertNotNull(line11);
			AssertNotNull(line12);
		}

		#endregion

		#region TestGetLinesToPickWhenDisassemble

		/// <summary>
		/// Picking rules for disassembly:
		///    1. Top level products / lines only (no parent).
		///    2. Product has OP_CanDisassembleKit = true.
		/// </summary>
		public void TestGetLinesToPickWhenDisassemble()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = CodeLists.WorkOrderType.Codes.Disassemble;
			WhsWorkOrderLine workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			WhsPickableDocketLineCollection linesPicked = workOrder.GetLinesToPick();
			AssertEquals(1, linesPicked.Count);
			AssertCollectionContains("Bike should have been Picked for Disassemble.", data.BOM.Lines.Bike(workOrder), linesPicked);

			data.BOM.Bike.OP_CanDisassembleKit = false;
			WhsPickableDocketLineCollection linesPicked2 = workOrder.GetLinesToPick();
			AssertEquals("No top level product can be disassembled -- GetLinesToPick() should return nothing.", 0, linesPicked2.Count);
		}

		#endregion

		#region TestLoadPickLinesForAllDocketLines

		protected override WhsPickableDocket GetPickableDocket_ForLoadPickLinesForAllDocketLinesTest(TestDataForBOM data)
		{
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			Helper.CreateWhsPickableDocketLine(docket, data.BOM.Bike, 2m);

			return docket;
		}

		#endregion

		#region TestTransportCoAndConsigneePullThroughFromParentOrder

		public void TestTransportCoAndConsigneePullThroughFromParentOrder()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var consignee = Helper.CreateClient("Consignee");
			var transportCo = Helper.CreateClient("TransportCo");

			// setup an order with a BOM shortfall (such that a work order is required to fulfill the shortfall)
			WhsOrder order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.BOM.Bike, 5m);
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = consignee.PK;
			order.TransportCoPK = transportCo.PK;

			// auto-create the work order
			order.BOM.AutoCreateWorkOrders(Notify);
			var workOrder = order.CurrentWorkOrders.ElementAt(0);

			// test that addresses are pulled directly from the order
			AssertEquals(order.ConsigneeDocAddress, workOrder.ConsigneeDocAddress);
			AssertEquals(order.TransportCoDocAddress, workOrder.TransportCoDocAddress);

			// test that addresses reflect changes to the order's addresses
			order.ConsigneeDocAddress.Address.OA_Address1 = "modified consignee address1";
			order.TransportCoDocAddress.Address.OA_Address1 = "modified transport address1";
			AssertEquals("modified consignee address1", workOrder.ConsigneeDocAddress.Address.OA_Address1);
			AssertEquals("modified transport address1", workOrder.TransportCoDocAddress.Address.OA_Address1);

			// test that the work order's addresses are always readonly

			// the docaddress on the order is shared by the workorder so we need to reset the readonly
			// to be able to test that the work Order always has a readonly Consignee
			order.ConsigneeDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy();

			AssertEquals("Precondition", false, order.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Precondition", false, order.TransportCoDocAddress.ReadOnly);
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);
			AssertEquals(true, workOrder.TransportCoDocAddress.ReadOnly);
		}

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetValidRelatedJobs(WhsWorkOrder docket)
		{
			docket.FillWithValidTestData();

			var parentOrder = Factory.NewWithValidTestData<WhsOrder>();
			docket.WD_WD_ParentDocket = parentOrder.PK;

			var childWorkOrder1 = Factory.NewWithValidTestData<WhsWorkOrder>();
			var childWorkOrder2a = Factory.NewWithValidTestData<WhsWorkOrder>();
			var childWorkOrder2b = Factory.NewWithValidTestData<WhsWorkOrder>();
			childWorkOrder1.WD_WD_ParentDocket = docket.PK;
			childWorkOrder2a.WD_WD_ParentDocket = childWorkOrder1.PK;
			childWorkOrder2b.WD_WD_ParentDocket = childWorkOrder1.PK;

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WD_ParentDocket = docket.PK;

			Factory.Save();

			return new List<IRelatedJob>() { parentOrder, childWorkOrder1, childWorkOrder2a, childWorkOrder2b, receive };
		}

		public void TestRelatedJobs_ParentAsWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsWorkOrder parentWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Docket.WD_WD_ParentDocket = parentWorkOrder.PK;

			AssertCollectionContains(parentWorkOrder, Docket.RelatedJobs);
		}

		#endregion

		#endregion

		#region Properties

		#region TestSubTypeDesc

		public override void TestSubTypeDesc()
		{
			Docket.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertEquals(WorkOrderType.Descriptions.Disassemble, Docket.SubTypeDesc);
		}

		#endregion

		#region TestWD_DocketSubType_RunsValidationOnProducts

		public void TestWD_DocketSubType_RunsValidationOnProducts()
		{
			bool productValidationFired = false;

			WhsWorkOrderLine line = Docket.Lines.AddNew();
			line.WE_OPInfo.AdditionalValidation += delegate
			{ productValidationFired = true; };

			AssertEquals("Precondition", false, productValidationFired);
			Docket.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertEquals(true, productValidationFired);
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

			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart2, subPart3, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart2, subPart4, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", subPart1, 10m);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R2", subPart2, 10m);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R3", subPart3, 10m);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R4", subPart4, 10m);

			var workOrder = Helper.CreateWhsWorkOrder(org, whs, "O1", type);
			var orderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);
			var orderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart2, 1m);

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("WD_TotalWeight should be the sum of components when type is Assemble", 20.17m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components when type is Assemble", 1.17m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of kits when type is Disassemble", 30.1m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of kits when type is Disassemble", 1.1m, workOrder.WD_TotalCubic);
			}

			orderLine1.WE_TransactionQuantity = 2m;
			orderLine2.WE_TransactionQuantity = 2m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("WD_TotalWeight should be the sum of components when type is Assemble", 40.34m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components when type is Assemble", 2.34m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of kits when type is Disassemble", 60.2m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of kits when type is Disassemble", 2.2m, workOrder.WD_TotalCubic);
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
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", subPart1, 10m);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R2", subPart2, 10m);

			var workOrder = Helper.CreateWhsWorkOrder(org, whs, "O1", type);
			var orderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);
			var orderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);

			orderLine1.WE_TransactionQuantity = 2m;
			orderLine2.WE_TransactionQuantity = 2m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("WD_TotalWeight should be the sum of components when type is Assemble", 80m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of components when type is Assemble", 4.4m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("WD_TotalWeight should be the sum of kits when type is Disassemble", 120m, workOrder.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should be the sum of kits when type is Disassemble", 4m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestWD_AutoFinaliseBOMIntoInventory

		public void TestWD_AutoFinaliseBOMIntoInventory_SetToTrue_ForVirtualWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = false;

			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);
			whs2.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1");
			AssertEquals("Precondition.", false, workOrder.WD_AutoFinaliseBOMIntoInventory);

			workOrder.WD_WW_Whs = whs2.PK;
			AssertEquals("WD_AutoFinaliseBOMIntoInventory is now true", true, workOrder.WD_AutoFinaliseBOMIntoInventory);
		}

		#endregion

		#region TestAllLines_RegisterEditableChildObject

		public void TestAllLines_RegisterEditableChildObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, wheel.OP_StockKeepingUnit);
			Helper.CreateProductBOM(bike, frame, 1m, frame.OP_StockKeepingUnit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, bike, 10m);
			workOrder.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			var changed = false;
			workOrder.HasChangesChanged += (s, e) => changed = true;
			AssertEquals("Precondition", false, changed);

			pick.AutoAllocateItemsWithMock();
			AssertEquals("Should fire change event.", true, changed);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsPartiallyOrFullyPickedFromPutawayLocation

		protected override void TestIsPartiallyOrFullyPickedFromPutawayLocationCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.CreateProductInInventory("Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("Polish", data.BOM.Polish, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition", 40m, Helper.GetTotalPickLineQuantity(pick));
			AssertEquals("When nothing was allocated yet WorkOrder must not be marked as picked.", false, workOrder.IsPartiallyOrFullyPickedFromPutawayLocation);

			var pickLine = pick.GetAllPickLines().First();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("WorkOrder with any picked pick line should be marked as picked.", true, workOrder.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", false, workOrder.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine.WZ_WE_OriginalPickedInventoryLine = pickLine.WZ_WE_InventoryLine;
			AssertEquals("When pickableDocket have In-Transit pick line attached to its lines, then it is considered picked from Putaway Location.", true, workOrder.IsPartiallyOrFullyPickedFromPutawayLocation);
		}

		#endregion

		#region TestIsCurrentlyBeingPickedFromPutawayLocation

		protected override void TestIsCurrentlyBeingPickedFromPutawayLocationCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.CreateProductInInventory("Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("Polish", data.BOM.Polish, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition", 40m, Helper.GetTotalPickLineQuantity(pick));
			AssertEquals("When nothing was allocated yet WorkOrder must not be marked as currently being picked.", false, workOrder.IsCurrentlyBeingPickedFromPutawayLocation);

			var pickLine = pick.GetAllPickLines().First();
			AssertEquals("When workOrder's pick lines are not currently being picked, then it is still not considered as currently being picked.", false, workOrder.IsCurrentlyBeingPickedFromPutawayLocation);

			pickLine.WZ_GS_NKAssignedTo = "AAA";
			pickLine.WZ_IsPicking = true;
			AssertEquals("When any of workOrder's pick lines are marked IsPicking, it is considered as currently being picked.", true, workOrder.IsCurrentlyBeingPickedFromPutawayLocation);
		}

		#endregion

		#region TestWD_WW_Whs

		public void TestWD_WW_Whs_ClearInVirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = false;

			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);
			whs2.WW_IsVirtualWarehouse = true;

			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			bomPart1.OE_ExcludeForVirtualWarehouse = true;
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1");
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			AssertEquals("Should have correct BOMs count", 2, line.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have all defined BOMs", new[] { bomPart1, bomPart2 }, line.BillOfMaterials.ToArray());

			workOrder.WD_WW_Whs = whs2.PK;
			AssertEquals("Should have correct BOMs count", 1, line.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have only virtual BOMs", new[] { bomPart2 }, line.BillOfMaterials.ToArray());
		}

		#endregion

		#endregion

		#region Properties_ReadOnly

		public void TestTransportBillToDocAddress_ReadOnly()
		{
			AssertEquals(true, Docket.TransportBillToDocAddress.ReadOnly);
		}

		public void TestGoodsBillToDocAddress_ReadOnly()
		{
			AssertEquals(true, Docket.GoodsBillToDocAddress.ReadOnly);
		}

		protected override void TestConsigneeDocAddress_ReadOnlyCore(bool isAllowed)
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.CreateProductInInventory("Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("Polish", data.BOM.Polish, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Factory.Save();

			_ = workOrder.ConsigneeDocAddress; // Testing that changing docket status refreshes the read only
			workOrder.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);

			workOrder.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);

			workOrder.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);

			workOrder.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);

			workOrder.WD_DocketStatus = DocketStatus.Codes.Entered;
			var pick = Helper.CreatePickNew(workOrder);
			workOrder.WD_WP = pick.PK;
			workOrder.FinaliseDocket();
			AssertIsFinalisedPrecondition(workOrder);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(workOrder.Pick);

			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = false;
			workOrder.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);

			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = isAllowed;
			workOrder.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, workOrder.ConsigneeDocAddress.ReadOnly);
		}

		public void TestConsigneeDocAddress_AlwaysReadOnly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.CreateProductInInventory("Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("Polish", data.BOM.Polish, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			bool readOnly = false;
			workOrder.ConsigneeDocAddress.HasChangesChanged += (sender, e) =>
			{
				readOnly = ((JobDocAddress)sender).ReadOnly;
			};

			AssertEquals("Precondition: Docket Status is New.", DocketStatus.Codes.New, workOrder.WD_DocketStatus);
			AssertEquals("Work order's consigneed doc address should always be true.", true, workOrder.ConsigneeDocAddress.ReadOnly);
			Factory.Save();

			AssertEquals("Precondition: Docket Status is updated to Entered.", DocketStatus.Codes.Entered, workOrder.WD_DocketStatus);
			AssertEquals("Work order's consigneed doc address should always be true.", true, readOnly);

			var pick = Helper.CreatePickNew(workOrder);
			workOrder.WD_WP = pick.PK;
			workOrder.FinaliseDocket();

			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Work order's consigneed doc address should always be true.", true, readOnly);
		}

		#endregion

		#region Picking

		#region TestGetPickability

		protected override void TestGetPickabilityCore(WhsPick pick, WhsPickableDocket order, WhsPickableDocketLine line, WhsPick.DocketPickabilityEventArgs args)
		{
			base.TestGetPickabilityCore(pick, order, line, args);

			ZString initialDocketStatus = order.WD_DocketStatus;

			// no product on the order can be fullfilled
			line.SetShortfallForTest(5m);

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("No Stock can be allocated to this {0} due to shortfalls.", order.Description), NotificationTypes.Error, false, false, args);
			line.SetShortfallForTest(0m); // clean up

			// if the order is already picked, the shortfall check should not occur
			line.SetShortfallForTest(5m);
			order.WD_WP = pick.PK; // dodgy but to do this properly is a pain
			order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args); // pickExists is false because pick.IsInDataBase is false, this is a hack for speed.
			line.SetShortfallForTest(0m); // clean up
			order.WD_WP = ZGuid.Empty; // clean up
			order.WD_DocketStatus = initialDocketStatus;
		}

		#endregion

		#region TestProductDefinitionNotMatchesWithDocketLineDefinition

		public void TestGetPickabilityCoreForComponentDefinitionChange()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create the work order
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);

			var pick = Factory.New<WhsPick>();
			pick.WP_PickNo = "Pick1";
			pick.WP_PickStatus = PickStatus.Codes.Created;
			pick.WP_WW_Whs = workOrder.WD_WW_Whs;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			var pickableEventArgs = new WhsPick.DocketPickabilityEventArgs();
			var pickAbilityWithMatchingProductDefinitions = workOrder.GetPickability(pick);
			AssertEquals("Product definition hasn't been changed after creating the work order.", "", pickAbilityWithMatchingProductDefinitions.Message);

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			var pickAbilityWithOneUnMatchingProductDefinition = workOrder.GetPickability(pick);
			AssertEquals("Pack type of a component product has been changed after creating the work order.",
				string.Format("One of the component products has been changed. Please cancel the work order {0} and recreate it.", workOrder.WD_ExternalReference), pickAbilityWithOneUnMatchingProductDefinition.Message);

			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Box;
			var pickAbilityWithAllUnMatchingProductDefinition = workOrder.GetPickability(pick);
			AssertEquals("Pack types of all compnent products have been changed after creating the work order.",
				string.Format("One of the component products has been changed. Please cancel the work order {0} and recreate it.", workOrder.WD_ExternalReference), pickAbilityWithOneUnMatchingProductDefinition.Message);
		}

		#endregion

		#region TestPicking_AllocationWithNoMatchingAllocationKey_Disassemble

		public void TestPicking_AllocationWithNoMatchingAllocationKey_Disassemble()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(workOrder);

			var receiveLines = workOrder.Receive.Lines;
			receiveLines[0].WE_AllocationKey = "ABC";
			receiveLines[1].WE_AllocationKey = "DEF";
			receiveLines[2].WE_AllocationKey = "GHE";

			AssertIsFinalisedPrecondition(workOrder.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.Bike, 2m);
			workOrderLine1.WE_AllocationKey = "ABC";
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.Bike, 2m);
			workOrderLine2.WE_AllocationKey = "GHE";
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.Bike, 2m);
			workOrderLine3.WE_AllocationKey = "123";

			var pick2 = Helper.CreatePickNew(workOrder2);
			var pickLine1 = workOrderLine1.PickLines.Single();
			AssertEquals("Should attempt to pick matching AllocationKey.", receiveLines[0].PK, pickLine1.WZ_WE_InventoryLine);

			var pickLine2 = workOrderLine2.PickLines.Single();
			AssertEquals("Should attempt to pick matching AllocationKey.", receiveLines[2].PK, pickLine2.WZ_WE_InventoryLine);

			AssertEquals("Should have no pickLines due to no matching inventory.", 0, workOrderLine3.PickLines.Count);
		}

		#endregion

		#endregion

		#region Finalisation

		protected override void AssertNoUserConfirmation(WhsWorkOrder docket, TestNotificationBuffer notify)
		{
			var queryList = new List<QueryUserEventArgs>();
			notify.PreQueryUser += (sender, e) => queryList.Add(e);
			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, docket.IsFinalised);
			AssertEquals(1, queryList.Count);

			// should only have the Assembly Confirmation - not the Finalisation confirmation
			AssertType<AssemblyConfirmationQueryUserEventArgs>(queryList.Single());
		}

		public void TestDBHitsForOrderedInventory_WorkOrder()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
												   // create a BOM staging location for the Bike
			var productParams = WhsProduct.GetWhsProduct(data.BOM.Bike).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			var stagingLocationBOM = Helper.CreateRowAndGenerateLocations(data.Whs1, "R").Locations[0];
			productParams.W3_WL_StagingLocationBOM = stagingLocationBOM.PK;
			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			for (int i = 0; i < 10; i++)
			{
				Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			}
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_TotalUnits = 10m;

			AssertEquals("Precondition: ", 10, workOrder.Lines.Count);
			AssertEquals("Precondition: ", 30, workOrder.GetLinesToPick().Count);

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition: ", 1, pick.Orders.Count);
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInAnotherFactory = anotherFactory.Load<WhsPick>(pick.PK);
			var poke = pickInAnotherFactory.OrderedInventories;

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 }, // this is what we care about the most
			};

			AssertDbHits(expectedDBHits, anotherFactory);
		}

		#region TestFinaliseDocket_DBHits

		[GuiTest]
		public void TestFinaliseDocket_DBHits() => TestFinaliseDocket_DBHits_Core();

		[GuiTest]
		public void TestFinaliseDocket_DBHits_HeldGoodsForOrdersDisabled() => TestFinaliseDocket_DBHits_Core(enableHeldGoodsForOrders: false);

		void TestFinaliseDocket_DBHits_Core(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataForBOM(Factory);
				data.CreateBOMProductsInInventory(); // 100 bikes
				data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
				Factory.Save();

				// setup a WorkOrder for Finalise
				var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);
				workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
				workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
				workOrder.WD_TotalUnits = 10m;

				// create a BOM staging location for the Bike
				var productParams = data.BOM.Lines.Bike(workOrder).Product.ParamsByWhsAndClient.AddNew();
				productParams.W3_WW = workOrder.Warehouse.PK;
				var stagingLocationBOM = Helper.CreateRowAndGenerateLocations(workOrder.Warehouse, "R").Locations[0];
				productParams.W3_WL_StagingLocationBOM = stagingLocationBOM.PK;
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				otherFactory.RefreshEnabled = false;
				var workOrderInOtherFactory = otherFactory.Load<WhsWorkOrder>(workOrder.PK);

				using (RowFactory.SetCachedTables())
				{
					workOrderInOtherFactory.RunPreSaveValidation();
					workOrderInOtherFactory.Validation.ValidateAll();
				}

				var expectedDBHitsForValidation = new Dictionary<string, int>()
				{
					{ OrgPartBOMSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },		// ?
					{ WhsInventoryViewSchema.Constants.TableName, 3 },	//WhsWorkOrderLine.GetProductAvailablity
					{ OrgSupplierPartSchema.Constants.TableName, 2 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 3 },
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
				};

				AssertDbHits(expectedDBHitsForValidation, otherFactory);

				var expectedDBHitsForFinalisation = new Dictionary<string, int>(expectedDBHitsForValidation);
				expectedDBHitsForFinalisation[WhsDocketSchema.Constants.TableName] += 5;
				expectedDBHitsForFinalisation[WhsDocketLineSchema.Constants.TableName] += 1;
				expectedDBHitsForFinalisation[OrgPartUnitSchema.Constants.TableName] = 1;
				expectedDBHitsForFinalisation[WhsInventoryViewSchema.Constants.TableName] += 1;
				expectedDBHitsForFinalisation[WhsLocationViewSchema.Constants.TableName] += 2;
				expectedDBHitsForFinalisation[WhsPickLineSchema.Constants.TableName] += 7;
				expectedDBHitsForFinalisation[WhsProductParamsByWhsAndClientSchema.Constants.TableName] = 2;
				expectedDBHitsForFinalisation[WhsRowSchema.Constants.TableName] = 2;
				expectedDBHitsForFinalisation[OrgSupplierPartSchema.Constants.TableName] -= 1;
				expectedDBHitsForFinalisation[GlbBranchSchema.Constants.TableName] += 1;

				expectedDBHitsForFinalisation.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(WhsDocketPalletSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(ProcessTasksSchema.Constants.TableName, 2);
				expectedDBHitsForFinalisation.Add(ProcessTaskTemplateSchema.Constants.TableName, 2);
				expectedDBHitsForFinalisation.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(StmALogSchema.Constants.TableName, 3);
				expectedDBHitsForFinalisation.Add(JobServiceSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(WhsLocationTypeSchema.Constants.TableName, 2);
				expectedDBHitsForFinalisation.Add(WhsDocketContainerSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(StmEventSchema.Constants.TableName, 4);
				expectedDBHitsForFinalisation.Add(StmNoteSchema.Constants.TableName, 2);
				expectedDBHitsForFinalisation.Add(RefPacksSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(RefPackTypeSchema.Constants.TableName, 1);

				expectedDBHitsForFinalisation.Add(GlbCompanySchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(GlbStaffSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(JobDocAddressSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(OrgAddressSchema.Constants.TableName, 3);
				expectedDBHitsForFinalisation.Add(OrgAddressCapabilitySchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(OrgContactSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(OrgCusCodeSchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation.Add(RefCountrySchema.Constants.TableName, 1);
				expectedDBHitsForFinalisation[ProductionRuleSchema.Constants.TableName] = 1;
				expectedDBHitsForFinalisation[WhsPickFaceSchema.Constants.TableName] = 1;
				expectedDBHitsForFinalisation[WhsAreaSchema.Constants.TableName] += 1;

				var factoryForFinalise = new BusinessObjectFactory();
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHitsForFinalisation, factoryForFinalise))
				{
					factoryForFinalise.RefreshEnabled = false;
					var workOrderInFinaliseFactory = factoryForFinalise.Load<WhsWorkOrder>(workOrder.PK);

					using (RowFactory.SetCachedTables())
					{
						var pick = factoryForFinalise.New<WhsPick>();
						pick.PickOrders(workOrderInFinaliseFactory);

						workOrderInFinaliseFactory.FinaliseDocketAlwaysFinalisingPick();
						AssertEquals(true, workOrderInFinaliseFactory.IsFinalised);
					}
				}
			}
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive

		protected override void TestFinaliseDocket_CreatesReceiveCore()
		{
			TestFinaliseDocket_CreatesReceiveCore(true);
		}

		void TestFinaliseDocket_CreatesReceiveCore(bool isAssembly)
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 bikes
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.BOM.EngineBlock, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.BOM.EnginePiston, 24m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.BOM.EngineOil, 6m);
			var extraPolish = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.BOM.Polish, 6m);
			// force this inventory to be allocated second.
			extraPolish.Lines[0].WE_AdjustmentArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);
			workOrder.WD_DocketSubType = isAssembly ? WorkOrderType.Codes.Assemble : WorkOrderType.Codes.Disassemble;
			workOrder.WD_ExternalReferenceSplit = 3;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_TotalUnits = 20m;
			workOrder.WD_PackagesSent = 10;
			workOrder.WD_TotalPallets = new ZShort(4);

			// setup orderline that cannot be disassembled
			data.BOM.BikeEngine.OP_CanDisassembleKit = false;
			var orderline = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeEngine, 10m);

			// create a BOM staging location for the Bike
			var productParams = data.BOM.Lines.Bike(workOrder).Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = workOrder.Warehouse.PK;
			var stagingLocationBOM = Helper.CreateRowAndGenerateLocations(workOrder.Warehouse, "R").Locations[0];
			productParams.W3_WL_StagingLocationBOM = stagingLocationBOM.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// finalise the WorkOrder (this is what we are testing..)
			WhsReceive receivePassedToEvent = null;
			workOrder.AutoCreatedReceiveSaved += (object sender, WhsComponentOrder.AutoCreatedReceiveSavedEventArgs e) => { receivePassedToEvent = e.Receive; };
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			// test receive
			var receive = workOrder.Receive;
			AssertEquals(false, receive.IsInDatabase);
			AssertEquals(true, workOrder.IsRegisteredEditableChildObject(receive));
			AssertEquals("Receive should not be auto-finalized because AutoFinaliseBOMIntoInventory was false.", false, receive.IsFinalised);

			AssertNotEquals("Precondition", "", workOrder.WD_ExternalReference);
			AssertNotEquals("Precondition", new ZByte(0), workOrder.WD_ExternalReferenceSplit);

			AssertEquals(false, receive.WD_IsInwardsProcessingJob);
			AssertEquals(workOrder.WD_WW_Whs, receive.WD_WW_Whs);
			AssertEquals(workOrder.WD_OH_Client, receive.WD_OH_Client);
			AssertEquals(workOrder.PK, receive.WD_WD_ParentDocket);
			AssertEquals(workOrder.WD_ExternalReference, receive.WD_ExternalReference);
			AssertEquals(workOrder.WD_ExternalReferenceSplit, receive.WD_ExternalReferenceSplit);
			AssertEquals(new ZDateTimeOffset(2009, 5, 6, 10, 0, 0, TimeSpan.FromHours(10)), receive.WD_ArrivalDate);
			AssertEquals(new ZDateTimeOffset(2009, 5, 6, 10, 0, 0, TimeSpan.FromHours(10)), receive.WD_BookingDate);
			AssertEquals(workOrder.WD_TotalUnits, receive.WD_TotalUnits);
			AssertEquals(workOrder.WD_PackagesSent, receive.WD_PackagesSent);
			AssertEquals(workOrder.WD_TotalPallets, receive.WD_TotalPallets);

			// test inventory (detailed check for other fields is on the WhsInventoryViewCollection test)
			string expectedMessage;

			if (isAssembly)
			{
				AssertEquals(2, receive.Inventory.Count);

				var bikeInventory = (WhsInventoryView)receive.Inventory.Find(new ZQuery(WhsInventoryViewSchema.WI_OP, data.BOM.Bike.PK))[0];
				AssertEquals("Should have received 10 x Bikes.", 10m, bikeInventory.WI_TotalUnits);
				AssertEquals(InventoryStatus.Codes.Putaway, bikeInventory.WI_InventoryStatus);
				AssertEquals("Allocation Key should only be set for Inwards Processing.", "", bikeInventory.InDocketLine.WE_AllocationKey);

				var bikeEngineInventory = (WhsInventoryView)workOrder.Receive.Inventory.Find(new ZQuery(WhsInventoryViewSchema.WI_OP, data.BOM.BikeEngine.PK))[0];
				AssertEquals("Should have received 6 x Bike Engines.", 6m, bikeEngineInventory.WI_TotalUnits);
				AssertEquals(InventoryStatus.Codes.Putaway, bikeEngineInventory.WI_InventoryStatus);
				AssertEquals("Allocation Key should only be set for Inwards Processing.", "", bikeEngineInventory.InDocketLine.WE_AllocationKey);

				expectedMessage = string.Format(
					"Receive {0}-{1} has been created to place the assembled Work Order Product into inventory.\r\n" +
					"\r\n" +
					"After Saving the Work Order the Receive Job will be available on the Related Jobs tab.", workOrder.WD_ExternalReference, workOrder.WD_ExternalReferenceSplit);
			}
			else
			{
				AssertEquals(2, receive.Inventory.Count);

				var engineInventory = (WhsInventoryView)receive.Inventory.Find(new ZQuery(WhsInventoryViewSchema.WI_OP, data.BOM.BikeEngine.PK))[0];
				var wheelInventory = (WhsInventoryView)receive.Inventory.Find(new ZQuery(WhsInventoryViewSchema.WI_OP, data.BOM.BikeWheel.PK))[0];
				AssertEquals("Should have received 10 x Engines.", 10m, engineInventory.WI_TotalUnits);
				AssertEquals("Should have received 20 x Wheels.", 20m, wheelInventory.WI_TotalUnits);

				expectedMessage = string.Format(
					"Receive {0}-{1} has been created to place the disassembled components back into inventory.\r\n" +
					"\r\n" +
					"After Saving the Work Order the Receive Job will be available on the Related Jobs tab.", workOrder.WD_ExternalReference, workOrder.WD_ExternalReferenceSplit);
			}

			// test the message shown to the user
			var notify = (NotificationBuffer)workOrder.NotificationManager.Peek;
			AssertEquals(expectedMessage, notify.Events[0].Message);

			// test the AutoCreatedReceiveSaved event
			AssertNull("Receive is not yet saved, event should not fire.", receivePassedToEvent);
			Factory.Save();
			AssertEquals(receive, receivePassedToEvent);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_ForDisassembly

		[TestDate(2009, 5, 6)]
		public void TestFinaliseDocket_CreatesReceive_ForDisassembly()
		{
			TestFinaliseDocket_CreatesReceiveCore(false);
		}

		#endregion

		protected override void TestFinaliseDocket_CreatesReceive_SetBookingDateBasedOnBranchOfWarehouseCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 bikes
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.BOM.EngineBlock, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.BOM.EnginePiston, 24m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.BOM.EngineOil, 6m);
			var extraPolish = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.BOM.Polish, 6m);
			// force this inventory to be allocated second.
			extraPolish.Lines[0].WE_AdjustmentArrivalDate = ZDateTimeOffset.Today.AddDays(1);

			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			data.Whs1.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_ExternalReferenceSplit = 3;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_TotalUnits = 20m;
			workOrder.WD_PackagesSent = 10;
			workOrder.WD_TotalPallets = new ZShort(4);

			var orderline = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeEngine, 10m);

			// create a BOM staging location for the Bike
			var productParams = data.BOM.Lines.Bike(workOrder).Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = workOrder.Warehouse.PK;
			var stagingLocationBOM = Helper.CreateRowAndGenerateLocations(workOrder.Warehouse, "R").Locations[0];
			productParams.W3_WL_StagingLocationBOM = stagingLocationBOM.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// finalise the WorkOrder to create a receive
			WhsReceive receivePassedToEvent = null;
			workOrder.AutoCreatedReceiveSaved += (object sender, WhsComponentOrder.AutoCreatedReceiveSavedEventArgs e) => { receivePassedToEvent = e.Receive; };
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			// test receive's WD_BookingDate
			var receive = workOrder.Receive;
			AssertEquals(new ZDateTimeOffset(2009, 5, 6, 8, 0, 0, TimeSpan.FromHours(8)), receive.WD_BookingDate);
		}

		#region TestFinaliseDocket_CreatesReceive_ManualPickingForBOMLines

		[TestDate(2009, 5, 6)]
		public void TestFinaliseDocket_CreatesReceive_ManualPickingForBOMLines()
		{
			Data.CreateBOMProducts();
			var palletID = 0;
			var receive = Helper.CreateWhsReceive(Data.Org1, Data.Whs1, "1");

			var engineInventory1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.BikeEngine, 10, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var engineInventory2 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.BikeEngine, 10, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var engineInventory3 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.BikeEngine, 10, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var engineInventories = new[] { engineInventory1, engineInventory2, engineInventory3 };

			var wheelInventory1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.BikeWheel, 20, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var wheelInventory2 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.BikeWheel, 20, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var wheelInventory3 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.BikeWheel, 20, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var wheelInventories = new[] { wheelInventory1, wheelInventory1, wheelInventory1 };

			var polishInventory1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.Polish, 10, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var polishInventory2 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.Polish, 10, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var polishInventory3 = Helper.CreateWhsReceiveInventoryLine(receive, Data.BOM.Polish, 10, Data.Whs1.DefaultLocation, "Pallet: " + palletID++);
			var polishInventories = new[] { polishInventory1, polishInventory1, polishInventory1 };

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			AssertEquals("Precondition", 30m, Data.BOM.InventoryQueries.BikeEngine.UnitsAvailable);
			AssertEquals("Precondition", 60m, Data.BOM.InventoryQueries.BikeWheel.UnitsAvailable);
			AssertEquals("Precondition", 30m, Data.BOM.InventoryQueries.Polish.UnitsAvailable);

			var order = Helper.CreateWhsWorkOrder(Data.Org1, Data.Whs1, "01");
			order.WD_PickOption = "MAN";

			var line = Helper.CreateWhsWorkOrderLine(order, Data.BOM.Bike, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			var availableInventoryBikeEngine = pickInOtherFactory.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availableInventoryBikeWheel = pickInOtherFactory.OrderedInventories[1].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availableInventoryPolish = pickInOtherFactory.OrderedInventories[2].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();

			AllocatePicks(availableInventoryBikeEngine);
			AllocatePicks(availableInventoryBikeWheel);
			AllocatePicks(availableInventoryPolish);

			otherFactory.Save();

			var testingThisPick = otherFactory.Load<WhsPick>(pickInOtherFactory.PK);

			order.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals(true, pick.IsFinalised);
			AssertCommittedInventoryIsZero(engineInventories);
			AssertCommittedInventoryIsZero(wheelInventories);
			AssertCommittedInventoryIsZero(polishInventories);
		}

		static void AssertCommittedInventoryIsZero(WhsInventoryView[] inventory)
		{
			foreach (var inv in inventory)
			{
				AssertEquals(0m, inv.CommittedQuantityIncludingUnfinalisedReceipt);
			}
		}

		static void AllocatePicks(WhsPickAvailableInventory[] availableInventory)
		{
			foreach (WhsPickAvailableInventory inv in availableInventory)
			{
				inv.Allocate = true;
			}
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_OnlyCreatesInventoryForBOMLines

		public void TestFinaliseDocket_CreatesReceive_OnlyCreatesInventoryForBOMLines()
		{
			var data = new TestDataForInventory(Factory);
			var docket = CreateAndSetupWorkOrderForFinalise(data);

			docket.BOM.ExpandAllLines(); // expanding lines will add the component lines to the Order.Lines collection, this is here to make sure the code isn't use Lines!
			docket.FinaliseDocketAlwaysFinalisingPick();

			AssertEquals("More than 1 line was received, make sure we are not receiving Component (child) lines.", 1, docket.Receive.Inventory.Count);
			AssertEquals("The Parent Line was not received into Inventory. Make sure the Child line (Part2) was not received.", data.Part1.PK, docket.Receive.Inventory[0].WI_OP);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_DisassambleDoesNotCreateInventoryForNonReusableBOM

		public void TestFinaliseDocket_CreatesReceive_DisassambleDoesNotCreateInventoryForNonReusableBOM()
		{
			Data.CreateBOMProducts();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeEngine, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeEngine, 1m);

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(new WhsWorkOrder[] { workOrder });
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			var inventory = workOrder.Receive.Inventory.ToArray<WhsInventoryView>();
			AssertEquals("2 inventory lines should be created - EngineBlock + EnginePistons.", 2, inventory.Length); // EngineOil is not in the list because it can not be reused.
			inventory.Single(i => i.WI_OP == Data.BOM.EngineBlock.PK);
			inventory.Single(i => i.WI_OP == Data.BOM.EnginePiston.PK);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_AndFinalises

		[TestDate(2009, 5, 6)]
		public void TestFinaliseDocket_CreatesReceive_AndFinalises()
		{
			var data = new TestDataForInventory(Factory);
			var docket = CreateAndSetupWorkOrderForFinalise(data);

			docket.WD_AutoFinaliseBOMIntoInventory = true;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				docket.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertEquals(false, docket.Receive.IsInDatabase);
			AssertEquals(true, docket.IsRegisteredEditableChildObject(docket.Receive));
			AssertEquals("Receive should be auto-finalized because AutoFinaliseBOMIntoInventory was true.", true, docket.Receive.IsFinalised);

			var notify = (NotificationBuffer)docket.NotificationManager.Peek;
			string expectedMessage =
				"Receive ExtRef-3 has been created to place the assembled Work Order Product into inventory.\r\n" +
				"\r\n" +
				"After Saving the Work Order the Receive Job will be available on the Related Jobs tab.";
			AssertEquals(expectedMessage, notify.Events[0].Message);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SameExternalNumberAsExistingReceive

		protected override WhsWorkOrder CreateComponentOrderForExternalReferenceTests(TestDataForBOM data)
		{
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 3m);
			return workOrder;
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_AndFinalises_InformsUserIfFinaliseFails

		protected override void TestFinaliseDocket_CreatesReceiveAndFinalises_InformsUserIfFinaliseFailsCore()
		{
			var data = new TestDataForInventory(Factory);
			var docket = CreateAndSetupWorkOrderForFinalise(data);

			// setup a mandatory attribute on the product -- thus the receive cannot be auto-finalised because the line does not have a value for the attrib
			Helper.SetClientAttributeType(docket.Client, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(docket.Client, docket.Lines[0].SupplierPart, AttributeNumber.One, true);

			docket.WD_AutoFinaliseBOMIntoInventory = true;
			docket.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals(false, docket.Receive.IsInDatabase);
			AssertEquals(true, docket.IsRegisteredEditableChildObject(docket.Receive));
			AssertEquals("Receive should not be auto-finalized because the Receive failed finalise validation.", false, docket.Receive.IsFinalised);

			var notify = (NotificationBuffer)docket.NotificationManager.Peek;
			string expectedMessage =
				"Receive ExtRef-3 has been created to place the assembled Work Order Product into inventory.\r\n" +
				"\r\n" +
				"After Saving the Work Order the Receive Job will be available on the Related Jobs tab.\r\n" +
				"\r\n" +
				"The Receive could not be automatically finalized. You can manually Finalize the Receive.";
			AssertEquals(expectedMessage, notify.Events[0].Message);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_Assembly

		protected override void TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_AssemblyCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 bikes
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);

			// create staging area
			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING");
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;

			// create a BOM staging location for the Bike
			var productParams = WhsProduct.GetWhsProduct(data.BOM.Bike).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_StagingLocationBOM = stagingLocation.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(workOrder);
			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer is created.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			var transferLine1 = newTransfer.Lines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK);
			var transferLine2 = newTransfer.Lines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK);
			AssertEquals("Precondition: In-Transit Transfer Line has destination Location.", stagingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition: In-Transit Transfer Line has destination Location.", stagingLocation.PK, transferLine2.WE_WL);

			productParams.W3_WL_StagingLocationBOM = ZGuid.Empty; // clear out staging location on product to test that staging location in transfers get refreshed.

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			// test receive
			AssertEquals(false, workOrder.Receive.IsInDatabase);
			AssertEquals("Receive should not be auto-finalized because AutoFinaliseBOMIntoInventory was false.", false, workOrder.Receive.IsFinalised);

			var bikeInventory = workOrder.Receive.Lines.Single();
			AssertEquals("Should have received 10 x Bikes.", 10m, bikeInventory.WE_StockOnHand);
			AssertEquals("The stock should be Putaway.", InventoryStatus.Codes.Putaway, bikeInventory.WE_CurrentInventoryStatus);
			AssertEquals("The stock should be in the Correct Location.", data.Whs1.DefaultLocation.PK, bikeInventory.WE_WL);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrder

		protected override void TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrderCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 bikes
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);

			// create a BOM staging location for the Bike
			var productParams = WhsProduct.GetWhsProduct(data.BOM.Bike).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_StagingLocationBOM = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer is NOT created.", 0, pick.Transfers.Count);

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var bikeInventory = workOrder.Receive.Lines.Single();
			AssertEquals("Bikes must be in Warehouse default locations since Staging location BOM is a dock door.", data.Whs1.DefaultLocation.PK, bikeInventory.WE_WL);
			AssertEquals("The stock should be Putaway.", InventoryStatus.Codes.Putaway, bikeInventory.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_PopulateStagingLocationInNonBondedArea_WorkOrder

		public void TestFinaliseDocket_CreatesReceive_PopulateStagingLocationInNonBondedArea_WorkOrder()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 bikes
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			Factory.Save();

			var bondedLocation = data.Whs1.FindLocation("A-2");
			bondedLocation.WLV_WA_PickingArea = bondedArea.PK;
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);

			// create a BOM staging location for the Bike
			var productParams = WhsProduct.GetWhsProduct(data.BOM.Bike).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_StagingLocationBOM = bondedLocation.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer is NOT created.", 0, pick.Transfers.Count);

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var bikeInventory = workOrder.Receive.Lines.Single();
			AssertEquals("Bikes must be in Warehouse default locations since Staging location BOM is in a bonded area.", data.Whs1.DefaultLocation.PK, bikeInventory.WE_WL);
			AssertEquals("The stock should be Putaway.", InventoryStatus.Codes.Putaway, bikeInventory.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_Disassembly

		public void TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_Disassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 bikes
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			Factory.Save();

			// setup a WorkOrder for Finalise
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			// create staging area
			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING");
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;

			// create a BOM staging area for the Bike
			var productParams = WhsProduct.GetWhsProduct(data.BOM.Bike).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WL_StagingLocationBOM = stagingLocation.PK;
			Factory.Save();

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(workOrder);
			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			Factory.Save();
			AssertEquals("Precondition: In-Transit Transfer is created.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			var transferLine = newTransfer.Lines.Single();
			AssertEquals("Precondition: In-Transit Transfer Line has destination Location.", stagingLocation.PK, transferLine.WE_WL);

			productParams.W3_WL_StagingLocationBOM = ZGuid.Empty; // clear out staging location on product to test that staging location in transfers get refreshed.

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			// test receive
			AssertEquals(false, workOrder.Receive.IsInDatabase);
			AssertEquals("Receive should not be auto-finalized because AutoFinaliseBOMIntoInventory was false.", false, workOrder.Receive.IsFinalised);

			var engineInventory = workOrder.Receive.Lines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK);
			var wheelInventory = workOrder.Receive.Lines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK);
			AssertEquals("Should have received 10 x Engines.", 10m, engineInventory.WE_StockOnHand);
			AssertEquals("Should have received 20 x Wheels.", 20m, wheelInventory.WE_StockOnHand);
			AssertEquals("The stock should be Putaway.", InventoryStatus.Codes.Putaway, engineInventory.WE_CurrentInventoryStatus);
			AssertEquals("The stock should be Putaway.", InventoryStatus.Codes.Putaway, wheelInventory.WE_CurrentInventoryStatus);
			AssertEquals("The stock should be in the Correct Location.", data.Whs1.DefaultLocation.PK, engineInventory.WE_WL);
			AssertEquals("The stock should be in the Correct Location.", data.Whs1.DefaultLocation.PK, wheelInventory.WE_WL);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_IncompleteKitComponents

		public void TestFinaliseDocket_CreatesReceive_ForIncompleteKitsComponents()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// Create inventory enough to assemble 2 bikes
			data.CreateProductInInventory("4 Wheels", data.BOM.BikeWheel, 4m);
			data.CreateProductInInventory("2 Engines", data.BOM.BikeEngine, 2m);
			data.CreateProductInInventory("2 Polish", data.BOM.Polish, 2m);
			Factory.Save();

			// Create a WorkOrder with two bikes ordered
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 2m);

			// Shortfall wheels quantity
			var pick = Helper.CreatePickNew(bikeWorkOrder);
			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.BikeWheel.PK).AvailableInventories[0];
			wheelComponentInv.PickLineQuantity = 2m;

			// Finalise the WorkOrder
			bikeWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(bikeWorkOrder);
			Factory.Save();

			// Expect items in stock
			var stock = bikeWorkOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 3, stock.Count);

			var bike = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Bike);
			AssertEquals("Number of bikes in stock is incorrect.", 1m, bike.WI_TotalUnits);

			var newWorkOrderLine = (WhsWorkOrderLine)bikeWorkOrder.Lines.Single(l => l != bikeWorkOrderLine);
			AssertEquals("The 1 kit that could be assembled should be split off.", 1m, newWorkOrderLine.WE_TransactionQuantity);
			AssertEquals("The previous Kit Line should have its quantity reduced by 1.", 1m, bikeWorkOrderLine.WE_TransactionQuantity);

			var link1 = ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == data.BOM.BikeWheel.PK);
			var link2 = ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == data.BOM.BikeEngine.PK);
			var link3 = ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == data.BOM.Polish.PK);
			AssertEquals(newWorkOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals(newWorkOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK).PK, link2.WIP_WE_ComponentLine);
			AssertEquals(newWorkOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.BOM.Polish.PK).PK, link3.WIP_WE_ComponentLine);
			AssertEquals(2m, link1.WIP_ComponentQuantity);
			AssertEquals(1m, link2.WIP_ComponentQuantity);
			AssertEquals(1m, link3.WIP_ComponentQuantity);

			var polish = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Polish);
			AssertEquals("Number of polishes in stock is incorrect.", 1m, polish.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)polish.InDocketLine).BOMComponentLinks.Count());

			var engine = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeEngine);
			AssertEquals("Number of engines in stock is incorrect.", 1m, engine.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)engine.InDocketLine).BOMComponentLinks.Count());
		}

		public void TestFinaliseDocket_CreatesReceive_CorrectlyWhenAllComponentsAreShorted()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// Create inventory for 3 bikes
			data.CreateProductInInventory("6 Wheels", data.BOM.BikeWheel, 6m);
			data.CreateProductInInventory("3 Engines", data.BOM.BikeEngine, 3m);
			data.CreateProductInInventory("3 Polish", data.BOM.Polish, 3m);
			Factory.Save();

			// Create WorkOrder
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 3m);

			// Shortfall each component
			var pick = Helper.CreatePickNew(bikeWorkOrder);
			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.BikeWheel.PK).AvailableInventories[0];
			wheelComponentInv.PickLineQuantity = 5m;

			var engineComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.BikeEngine.PK).AvailableInventories[0];
			engineComponentInv.PickLineQuantity = 2m;

			var polishComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.Polish.PK).AvailableInventories[0];
			polishComponentInv.PickLineQuantity = 1m;

			// Finalise the WorkOrder
			bikeWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(bikeWorkOrder);
			Factory.Save();

			// Expect items in stock
			var stock = bikeWorkOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 3, stock.Count);

			var bike = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Bike);
			AssertEquals("Number of bikes in stock is incorrect.", 1m, bike.WI_TotalUnits);

			var newWorkOrderLine = (WhsWorkOrderLine)bikeWorkOrder.Lines.Single(l => l != bikeWorkOrderLine);
			AssertEquals("The 1 kit that could be assembled should be split off.", 1m, newWorkOrderLine.WE_TransactionQuantity);
			AssertEquals("The previous Kit Line should have its quantity reduced by 1.", 2m, bikeWorkOrderLine.WE_TransactionQuantity);

			var link1 = ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == data.BOM.BikeWheel.PK);
			var link2 = ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == data.BOM.BikeEngine.PK);
			var link3 = ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == data.BOM.Polish.PK);
			AssertEquals(newWorkOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals(newWorkOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK).PK, link2.WIP_WE_ComponentLine);
			AssertEquals(newWorkOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.BOM.Polish.PK).PK, link3.WIP_WE_ComponentLine);
			AssertEquals(2m, link1.WIP_ComponentQuantity);
			AssertEquals(1m, link2.WIP_ComponentQuantity);
			AssertEquals(1m, link3.WIP_ComponentQuantity);

			var wheel = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeWheel);
			AssertEquals("Number of wheels in stock is incorrect.", 3m, wheel.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)wheel.InDocketLine).BOMComponentLinks.Count());

			var engine = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeEngine);
			AssertEquals("Number of engines in stock is incorrect.", 1m, engine.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)engine.InDocketLine).BOMComponentLinks.Count());
		}

		public void TestFinaliseDocket_CreatesReceive_CorrectlyWhenComponentAddedWithDifferentPackTypesAndShorted()
		{
			// create BOM product
			//		- 6 x bomComponent1 (1 unit + 1 bag)
			//		- 1 x bomComponent2
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");

			var bomComponent1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductUnit(bomComponent1, Constants.PkgUnit.Bag, 5m);
			Helper.CreateProductBOM(mainProduct, bomComponent1, 1m, Constants.PkgUnit.Bag);
			Helper.CreateProductBOM(mainProduct, bomComponent1, 1m, Constants.PkgUnit.Unit);

			var bomComponent2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponent2, 1m, Constants.PkgUnit.Unit);

			// Create inventory for 2 main products
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponent1, 12m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponent2, 2m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// Create WorkOrder
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 2m);

			// Shortfall bomComponentInv1 quantity
			var pick = Helper.CreatePickNew(workOrder);
			//var bomComponentInv1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == bomComponent1.PK).AvailableInventories[0];
			var bomComponentInv1 = pick.GetAllPickLines().Single(l => l.WZ_Units == 10m);
			bomComponentInv1.WZ_Units = 9m;

			// Finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			Factory.Save();

			// Expect items in stock
			var stock = workOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 4, stock.Count);

			var mainProductStock = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == mainProduct);
			AssertEquals("Number of mainProducts in stock is incorrect.", 1m, mainProductStock.WI_TotalUnits);

			var bomComponentBagStock1 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == bomComponent1 && i.WI_F3_NKPackType == Constants.PkgUnit.Bag);
			AssertEquals("Number of bomComponent1 bags in stock is incorrect.", 4m, bomComponentBagStock1.WI_TotalUnits);

			var bomComponentStock1 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == bomComponent1 && i.WI_F3_NKPackType == Constants.PkgUnit.Unit);
			AssertEquals("Number of bomComponent1 units in stock is incorrect.", 1m, bomComponentStock1.WI_TotalUnits);

			var bomComponentStock2 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == bomComponent2);
			AssertEquals("Number of bomComponent2 in stock is incorrect.", 1m, bomComponentStock2.WI_TotalUnits);
		}

		public void TestFinaliseDocket_CreatesReceive_CorrectlyWhenComponentAddedWithDifferentPackTypesAndShortedAnotherComponent()
		{
			// create BOM product
			//		- 6 x bomComponent1 (1 unit + 1 bag)
			//		- 1 x bomComponent2
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");

			var bomComponent1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductUnit(bomComponent1, Constants.PkgUnit.Bag, 5m);
			Helper.CreateProductBOM(mainProduct, bomComponent1, 1m, Constants.PkgUnit.Bag);
			Helper.CreateProductBOM(mainProduct, bomComponent1, 1m, Constants.PkgUnit.Unit);

			var bomComponent2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponent2, 1m, Constants.PkgUnit.Unit);

			// Create inventory for 2 main products
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponent1, 12m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponent2, 2m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// Create a WorkOrder
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 2m);

			// Shortfall bomComponentInv2 quantity
			var pick = Helper.CreatePickNew(workOrder);
			var bomComponentInv2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == bomComponent2.PK).AvailableInventories[0];
			bomComponentInv2.PickLineQuantity = 1m;

			// Finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			Factory.Save();

			// Expect items in stock
			var stock = workOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 3, stock.Count);

			var mainProductStock = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == mainProduct);
			AssertEquals("Number of mainProducts in stock is incorrect", 1m, mainProductStock.WI_TotalUnits);

			var bomComponentStock1 = stock.Cast<WhsInventoryView>().Single(i => (i.SupplierPart == bomComponent1) && (i.WI_F3_NKPackType == Constants.PkgUnit.Unit));
			AssertEquals("Number of bomComponent1 units in stock is incorrect.", 1m, bomComponentStock1.WI_TotalUnits);

			var bomComponentBagStock1 = stock.Cast<WhsInventoryView>().Single(i => (i.SupplierPart == bomComponent1) && (i.WI_F3_NKPackType == Constants.PkgUnit.Bag));
			AssertEquals("Number of bomComponent1 bags in stock is incorrect.", 5m, bomComponentBagStock1.WI_TotalUnits);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_ChangingComponentDefinitionBeforeFinalisingWorkOrder

		public void TestFinaliseDocket_CreatesReceive_ChangingComponentDefinitionBeforeFinalisingWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var component1 = Helper.CreateProduct(data.Org1, "C1");
			var component2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, component1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, component2, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, component1, 20m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, component2, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, WorkOrderType.Codes.Assemble);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition.", true, workOrder.IsAttachedToPickButNotFinalised);
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Basket;

			AssertNoExceptionThrown("Should not throw exception", () => workOrder.FinaliseDocketAlwaysFinalisingPick());
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_UnitConversion

		public void TestFinaliseDocket_CreatesReceive_UnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 2m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 0.7m, Constants.PkgUnit.Sheet);
			Helper.CreateProductUnit(componentProduct2, Constants.PkgUnit.Sheet, 0.3m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 20m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create the work order
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition.", true, workOrder.IsAttachedToPickButNotFinalised);
			var bomComponentInv1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == componentProduct1.PK).AvailableInventories[0];
			bomComponentInv1.PickLineQuantity = 8m;
			var bomComponentInv2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == componentProduct2.PK).AvailableInventories[0];
			bomComponentInv2.PickLineQuantity = 5 * 0.21m - 0.1m; // Just to make less than requirement
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var stock = workOrder.Receive.Inventory.Cast<WhsInventoryView>();
			AssertEquals("bomProduct and componentProduct2.", 2, stock.Count());

			var bomInventory = stock.Single(i => i.SupplierPart == bomProduct);
			AssertEquals("Number of bomProduct in stock should be 4.", 4m, bomInventory.WI_TotalUnits);
			AssertNull("For Component1 do not need to create inventory.", stock.SingleOrDefault(i => i.SupplierPart == componentProduct1));

			var link1 = ((WhsReceiveLine)bomInventory.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == componentProduct1.PK);
			var link2 = ((WhsReceiveLine)bomInventory.InDocketLine).BOMComponentLinks.Single(l => l.ComponentLine.WE_OP == componentProduct2.PK);
			AssertEquals(workOrderLine.ChildComponentLines.Single(l => l.WE_OP == componentProduct1.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals(workOrderLine.ChildComponentLines.Single(l => l.WE_OP == componentProduct2.PK).PK, link2.WIP_WE_ComponentLine);
			AssertEquals(8m, link1.WIP_ComponentQuantity);
			AssertEquals(0.84m, link2.WIP_ComponentQuantity);

			var overPickedInventory = stock.Single(i => i.SupplierPart == componentProduct2);
			AssertEquals("Still we create inventory even value is too small (to avoid unbalanced inventory).", 0.21m - 0.1m, overPickedInventory.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)overPickedInventory.InDocketLine).BOMComponentLinks.Count());
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_InwardsProcessing

		public void TestFinaliseDocket_CreatesReceive_InwardsProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var secondaryPart1 = Helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateSecondaryProduct(data.Part2, secondaryPart1, 3m);
			Helper.CreateSecondaryProduct(data.Part2, secondaryPart2, 4m);

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive.WD_DocketSubType);
			AssertEquals("New Receive should be Inwards Processing Job.", true, receive.WD_IsInwardsProcessingJob);
			AssertEquals("Should have created three products.", 3, receive.Lines.Count);

			var inventoryLineMainProduct = receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			var inventoryLineSecondaryProduct1 = receive.Lines.Single(l => l.WE_OP == secondaryPart1.PK);
			var inventoryLineSecondaryProduct2 = receive.Lines.Single(l => l.WE_OP == secondaryPart2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);
			AssertEquals("Correct Inwards Processing flag is set.", true, inventoryLineMainProduct.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Correct Inwards Processing flag is set.", false, inventoryLineMainProduct.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("Assembled inventory has correct Location.", location.PK, inventoryLineMainProduct.WE_WL);
			AssertEquals("Assembled inventory should have no Inwards Entry Key.", "", inventoryLineMainProduct.WE_BondedEntryKey);
			AssertEquals("Assembled inventory has no Allocation Key yet.", "", inventoryLineMainProduct.WE_AllocationKey);

			AssertEquals("Has correct Secondary Product Quantities.", 15m, inventoryLineSecondaryProduct1.WE_TransactionQuantity);
			AssertEquals("Has correct Secondary Product Quantities.", 15m, inventoryLineSecondaryProduct1.WE_StockOnHand);
			AssertEquals("Has correct Secondary Product Quantities.", 15m, inventoryLineSecondaryProduct1.WE_ClientOrderedUnits);
			AssertEquals("Arrival Date is set.", true, inventoryLineSecondaryProduct1.WE_AdjustmentArrivalDate.IsValid);
			AssertEquals("Correct Inwards Processing flag is set.", false, inventoryLineSecondaryProduct1.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Correct Inwards Processing flag is set.", true, inventoryLineSecondaryProduct1.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("SecondaryProduct has correct Location.", location.PK, inventoryLineSecondaryProduct1.WE_WL);
			AssertEquals("SecondaryProduct should have no Inwards Entry Key.", "", inventoryLineSecondaryProduct1.WE_BondedEntryKey);
			AssertEquals("SecondaryProduct has no Allocation Key yet.", "", inventoryLineSecondaryProduct1.WE_AllocationKey);

			AssertEquals("Has correct Secondary Product Quantities.", 20m, inventoryLineSecondaryProduct2.WE_TransactionQuantity);
			AssertEquals("Has correct Secondary Product Quantities.", 20m, inventoryLineSecondaryProduct2.WE_StockOnHand);
			AssertEquals("Has correct Secondary Product Quantities.", 20m, inventoryLineSecondaryProduct2.WE_ClientOrderedUnits);
			AssertEquals("Arrival Date is set.", true, inventoryLineSecondaryProduct2.WE_AdjustmentArrivalDate.IsValid);
			AssertEquals("Correct Inwards Processing flag is set.", false, inventoryLineSecondaryProduct2.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Correct Inwards Processing flag is set.", true, inventoryLineSecondaryProduct2.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("SecondaryProduct has correct Location.", location.PK, inventoryLineSecondaryProduct2.WE_WL);
			AssertEquals("SecondaryProduct should have no Inwards Entry Key.", "", inventoryLineSecondaryProduct2.WE_BondedEntryKey);
			AssertEquals("SecondaryProduct has no Allocation Key yet.", "", inventoryLineSecondaryProduct2.WE_AllocationKey);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { "W00000003-0001", "W00000003-0002", "W00000003-0003" },
				new[] { inventoryLineMainProduct, inventoryLineSecondaryProduct1, inventoryLineSecondaryProduct2 }.Select(i => i.WE_AllocationKey));
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_Disassemble

		public void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasNoLink()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Factory.Save();

			var kitReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, "BEK-1", allocateLocations: false, finalise: false);
			kitReceive.Lines[0].WE_WL = data.Whs1.DefaultLocation.PK;
			kitReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(kitReceive);
			Factory.Save();

			AssertEquals("Precondition: Kit doesn't have links.", 0, ((WhsReceiveLine)kitReceive.Lines.Single()).BOMComponentLinks.Count());

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				workOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("New Receive should be a Receipt Receive.", ReceiveType.Codes.Receipt, receive.WD_DocketSubType);
			AssertEquals("Should have created 1 product.", 1, receive.Lines.Count);

			var inventoryLineProduct = (WhsReceiveLine)receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Disassembled correct amounts.", 20m, inventoryLineProduct.WE_TransactionQuantity);
			AssertNoExceptionThrown("Should be able to save.", Factory.Save);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinks()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinksCore(inventoryTransfered: false, inventoryPickedAndReturned: false);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinks_InventoryTransfered()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinksCore(inventoryTransfered: true, inventoryPickedAndReturned: false);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinks_InventoryPickedAndReturned()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinksCore(inventoryTransfered: false, inventoryPickedAndReturned: true);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinks_InventoryTransferedPickedAndReturned()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinksCore(inventoryTransfered: true, inventoryPickedAndReturned: true);
		}

		void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinksCore(bool inventoryTransfered, bool inventoryPickedAndReturned)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.Lines[0].WE_WL = data.Whs1.DefaultLocation.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has one link.", 1, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());

			// Change the BOM, and assert that Disassembly Work Order uses the link and not the BOM.
			Helper.CreateProductBOM(data.Part2, part3, 1m, "UNT");
			Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");
			Factory.Save();

			if (inventoryTransfered)
			{
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, "A-1", "A-2");
				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transfer);
				Factory.Save();
			}

			if (inventoryPickedAndReturned)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 5m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();

				var orderLine = (WhsOrderLine)order.Lines.Single();
				var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
				var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 5m, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
				AssertEquals("Return stock should success.", true, result.IsSuccess);

				var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK));
				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transfer);
				Factory.Save();
			}

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", data.Part2, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var receive2 = disassembleWorkOrder.Receive;
			AssertEquals("Should have created one products.", 1, receive2.Lines.Count);

			var inventory2 = receive2.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Disassembled correct part1 amounts.", 10m, inventory2.WE_TransactionQuantity);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_KitHasLinks_ComponentNotReusable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var bom2 = Helper.CreateProductBOM(data.Part2, part3, 1m, "UNT");
			bom2.OE_CanReuse = false;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.Lines[0].WE_WL = data.Whs1.DefaultLocation.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);

			var component2Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 10m, "BEK-2", allocateLocations: false, finalise: false);
			component2Receive.Lines[0].WE_WL = data.Whs1.DefaultLocation.PK;
			component2Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component2Receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has two links.", 2, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());

			// Change the BOM, and assert that Disassembly Work Order uses the link and not the BOM.
			Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", data.Part2, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var receive2 = disassembleWorkOrder.Receive;
			AssertEquals("Should have created one products and ignored part3 as it's not reusable.", 1, receive2.Lines.Count);

			var inventory2 = receive2.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Disassembled correct part1 amounts.", 10m, inventory2.WE_TransactionQuantity);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_2KitsWithDifferentLinksAnd2KitsWithoutLinks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 1m);

			var bom1 = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Factory.Save();

			// create workOrder1 with BOM
			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef1", data.Part2, 1m);
			workOrder1.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				workOrder1.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(workOrder1);

			var receive1 = workOrder1.Receive;
			receive1.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);
			var link1 = ((WhsReceiveLine)receive1.Lines.Single()).BOMComponentLinks;
			AssertEquals("Should have created 1 product.", 1, receive1.Lines.Count);
			AssertEquals("Should have created 1 part2.", 1m, receive1.Lines.Single(l => l.WE_OP == data.Part2.PK).WE_TransactionQuantity);
			AssertEquals("link1.", 1, link1.Count());
			Factory.Save();

			// create workOrder2 with changed BOM
			bom1.Delete();
			var bom2 = Helper.CreateProductBOM(data.Part2, part3, 3m, "UNT");
			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", data.Part2, 1m);
			workOrder2.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder2.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				workOrder2.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(workOrder2);

			var receive2 = workOrder2.Receive;
			receive2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive2);
			var link2 = ((WhsReceiveLine)receive2.Lines.Single()).BOMComponentLinks;
			AssertEquals("Should have created 1 product.", 1, receive2.Lines.Count);
			AssertEquals("Should have created 1 part2.", 1m, receive2.Lines.Single(l => l.WE_OP == data.Part2.PK).WE_TransactionQuantity);
			AssertEquals("link2.", 1, link2.Count());
			Factory.Save();

			// changed BOM again
			bom2.Delete();
			var bom3 = Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");

			var disassemblyWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef3", data.Part2, 4m);
			disassemblyWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassemblyWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(disassemblyWorkOrder);

			var receive3 = disassemblyWorkOrder.Receive;
			AssertEquals("Should have created 3 receive lines.", 3, receive3.Lines.Count);

			var part1Inventory = (WhsReceiveLine)receive3.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("part1 from link1.", 2m, part1Inventory.WE_TransactionQuantity);

			var part3Inventory = (WhsReceiveLine)receive3.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("part3 from link2.", 3m, part3Inventory.WE_TransactionQuantity);

			var part4Inventory = (WhsReceiveLine)receive3.Lines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("part4 without links.", 2m, part4Inventory.WE_TransactionQuantity);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestFinaliseDocket_CreatesReceive_InwardsProcessing_Disassemble_KitHasLinks_AttributesCopied()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, data.Part1, 1m, "UNT");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var packingDate = ZDate.Today.AddDays(7);
			var expiryDate = ZDate.Today.AddMonths(1);
			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(componentReceive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "D");
			receiveLine1.WI_AllocationKey = "AKEY";
			componentReceive.FinaliseDocket();
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 1m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			AssertEquals("Precondition: New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive.WD_DocketSubType);
			AssertEquals("Precondition: New Receive should be Inwards Processing Job.", true, receive.WD_IsInwardsProcessingJob);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has one links.", 1, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", data.Part2, 1m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			disassembleWorkOrder.WD_IsInwardsProcessingJob = true;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var receive2 = disassembleWorkOrder.Receive;
			AssertEquals("New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive2.WD_DocketSubType);
			AssertEquals("New Receive should be Inwards Processing Job.", true, receive2.WD_IsInwardsProcessingJob);
			AssertEquals("Should have created one products.", 1, receive2.Lines.Count);

			var inventory2 = receive2.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Disassembled correct part1 amounts.", 1m, inventory2.WE_TransactionQuantity);
			AssertEquals("Disassembled inventory has correct Location.", location.PK, inventory2.WE_WL);
			AssertEquals("Attributes are copied.", "A", inventory2.WE_PartAttrib1);
			AssertEquals("Attributes are copied.", "B", inventory2.WE_PartAttrib2);
			AssertEquals("Attributes are copied.", "C", inventory2.WE_PartAttrib3);
			AssertEquals("Attributes are copied.", "S1", inventory2.WE_SerialNumber);
			AssertEquals("Attributes are copied.", expiryDate, inventory2.WE_ExpiryDate);
			AssertEquals("Attributes are copied.", packingDate, inventory2.WE_PackingDate);
			AssertEquals("Attributes are copied.", "D", inventory2.WE_BondedEntryKey);
			AssertEquals("Attributes are copied.", "AKEY", inventory2.WE_AllocationKey);
		}

		public void TestFinaliseDocket_CreatesReceive_InwardsProcessing_Disassemble_KitHasLinks_CustomsDataCopied()
		{
			using (WarehouseDataRegistry.Instance.EnableImprovedStorageOfCustomsData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.EnableWarehouseForBond(data.Whs1, true);
				data.Whs1.WW_IsVirtualWarehouse = true;
				var part3 = Helper.CreateProduct("P3", data.Org1);
				var part4 = Helper.CreateProduct("P4", data.Org1);
				Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

				var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
				var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
				var location = row.Locations.Single();
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				var bwa1 = Factory.NewWithValidTestData<WhsBondedWarehouseAttribute>();
				bwa1.WB_ParentID = ZGuid.NewZGuid();
				bwa1.WB_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
				Factory.Save();

				var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
				componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
				componentReceive.WD_IsInwardsProcessingJob = true;
				componentReceive.Lines[0].WE_WL = location.PK;
				var invCustomsData = componentReceive.Lines[0].CustomsData;
				invCustomsData.WB_WB_InwardsEntry = bwa1.PK;
				invCustomsData.WB_InwardStyle = "is";
				invCustomsData.WB_EntryLineNo = 1;
				invCustomsData.WB_EntryKey = "DEF";
				invCustomsData.WB_AddInfo = "addinfo";
				invCustomsData.WB_BondedWhsQty = 19m;
				invCustomsData.WB_BondedWhsUnitOfQty = "KG";
				invCustomsData.WB_CustomsQty = 3m;
				invCustomsData.WB_CustomsUnitOfQty = "LB";
				invCustomsData.WB_ValueForDuty = 0.99m;
				componentReceive.FinaliseDocket();
				AssertIsFinalisedPrecondition(componentReceive);
				Factory.Save();

				var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
				workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
				workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
				workOrder.WD_IsInwardsProcessingJob = true;

				var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
				productParam.W3_OH = data.Org1.PK;
				productParam.W3_WW = data.Whs1.PK;
				productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

				Helper.CreatePickNew(workOrder);
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertIsFinalisedPrecondition(workOrder);

				var receive = workOrder.Receive;
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				AssertEquals("Precondition: New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive.WD_DocketSubType);
				AssertEquals("Precondition: New Receive should be Inwards Processing Job.", true, receive.WD_IsInwardsProcessingJob);
				AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
				AssertEquals("Precondition: Kit has one links.", 1, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());
				Factory.Save();

				var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", data.Part2, 5m);
				disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
				disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
				disassembleWorkOrder.WD_IsInwardsProcessingJob = true;

				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
				}
				AssertIsFinalisedPrecondition(disassembleWorkOrder);

				var receive2 = disassembleWorkOrder.Receive;
				AssertEquals("New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive2.WD_DocketSubType);
				AssertEquals("New Receive should be Inwards Processing Job.", true, receive2.WD_IsInwardsProcessingJob);
				AssertEquals("Should have created one products.", 1, receive2.Lines.Count);

				var inventory2 = receive2.Lines.Single(l => l.WE_OP == data.Part1.PK);
				AssertEquals("Disassembled correct part1 amounts.", 10m, inventory2.WE_TransactionQuantity);
				AssertEquals("Disassembled inventory has correct Location.", location.PK, inventory2.WE_WL);

				var customsData = inventory2.CustomsData;
				AssertEquals("CustomsData.WB_IsMainInwardsProcessedItem.", false, customsData.WB_IsMainInwardsProcessedItem);
				AssertEquals("CustomsData.WB_IsSecondaryInwardsProcessedItem.", false, customsData.WB_IsSecondaryInwardsProcessedItem);
				AssertEquals("CustomsData.WB_WB_InwardsEntry.", bwa1.PK, customsData.WB_WB_InwardsEntry);
				AssertEquals("CustomsData.WB_InwardStyle.", "is", customsData.WB_InwardStyle);
				AssertEquals("CustomsData.WB_EntryLineNo.", (short)1, customsData.WB_EntryLineNo);
				AssertEquals("CustomsData.WB_EntryKey.", "DEF", customsData.WB_EntryKey);
				AssertEquals("CustomsData.WB_AddInfo", "addinfo", customsData.WB_AddInfo);
				AssertEquals("CustomsData.WB_BondedWhsQty", 10m, customsData.WB_BondedWhsQty);
				AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", "KG", customsData.WB_BondedWhsUnitOfQty);
				AssertEquals("CustomsData.WB_CustomsQty", 3m, customsData.WB_CustomsQty);
				AssertEquals("CustomsData.WB_CustomsUnitOfQty", "LB", customsData.WB_CustomsUnitOfQty);
				AssertEquals("CustomsData.WB_ValueForDuty", 0.99m, customsData.WB_ValueForDuty);
			}
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinks()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinksCore(inventoryTransfered: false, inventoryPickedAndReturned: false);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinks_InventoryTransfered()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinksCore(inventoryTransfered: true, inventoryPickedAndReturned: false);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinks_InventoryPickedAndReturned()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinksCore(inventoryTransfered: false, inventoryPickedAndReturned: true);
		}

		public void TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinks_InventoryTransferedPickedAndReturned()
		{
			TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinksCore(inventoryTransfered: true, inventoryPickedAndReturned: true);
		}

		void TestFinaliseDocket_CreatesReceive_Disassemble_ComponentHasLinksCore(bool inventoryTransfered, bool inventoryPickedAndReturned)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(part3, data.Part2, 2m, "UNT");
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.Lines[0].WE_WL = data.Whs1.DefaultLocation.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 10m);
			workOrder1.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder1);
			workOrder1.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder1);

			var receive1 = workOrder1.Receive;
			receive1.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			AssertEquals("Precondition: Should have created 1 products.", 1, receive1.Lines.Count);
			AssertEquals("Precondition: Kit has one link.", 1, ((WhsReceiveLine)receive1.Lines.Single()).BOMComponentLinks.Count());

			if (inventoryTransfered)
			{
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-2");
				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transfer);
				Factory.Save();
			}

			if (inventoryPickedAndReturned)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 10m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();

				var orderLine = (WhsOrderLine)order.Lines.Single();
				var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
				var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10m, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
				AssertEquals("Return stock should success.", true, result.IsSuccess);

				var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK));
				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transfer);
				Factory.Save();
			}

			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", part3, 5m);
			workOrder2.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder2.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				workOrder2.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(workOrder2);
			var receive2 = workOrder2.Receive;
			receive2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			AssertEquals("Precondition: Should have created 1 products.", 1, receive2.Lines.Count);
			AssertEquals("Precondition: Kit2 has one link.", 1, ((WhsReceiveLine)receive2.Lines.Single()).BOMComponentLinks.Count());
			AssertEquals("Precondition: 5 units of part3.", 5m, receive2.Lines[0].WE_TransactionQuantity);

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef3", part3, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassembleWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(disassembleWorkOrder);

			var receive3 = disassembleWorkOrder.Receive;
			var inventory2 = receive3.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Should have created one products.", 1, receive3.Lines.Count);
			AssertEquals("After disassembl we have part2.", 10m, inventory2.WE_TransactionQuantity);

			var inventoryLine = (WhsReceiveLine)receive3.Lines.Single();
			var kitLine = (WhsWorkOrderLine)disassembleWorkOrder.Lines.Single();
			var componentLine = (WhsWorkOrderLine)kitLine.ChildComponentLines.Single();
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_ExcludeForVirtualWhs

		public void TestFinaliseDocket_CreatesReceive_ExcludeForVirtualWhs_Assemble()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 3m, "UNT");
			var bom2 = Helper.CreateProductBOM(data.Part2, part3, 2m, "UNT");
			bom2.OE_ExcludeForVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(componentReceive, data.Part1, 15m, location, "", "");
			var receiveLine2 = Helper.CreateWhsReceiveLine(componentReceive, part3, 10m, location, "", "");
			var receiveLine3 = Helper.CreateWhsReceiveLine(componentReceive, part4, 5m, location, "", "");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			AssertEquals("Precondition: Part1 SOH correct.", 15m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Part3 SOH correct.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Part4 SOH correct.", 5m, receiveLine3.WE_StockOnHand);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created one product.", 1, receive.Lines.Count);

			var inventoryLineMainProduct = receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			AssertEquals("Part1 SOH correct.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Part3 SOH correct.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Part4 SOH correct.", 0m, receiveLine3.WE_StockOnHand);

			var workOrderAllLines = workOrder.AllLines;
			AssertEquals("Work order all lines correct.", 3, workOrderAllLines.Count);

			var part2WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Part2 work order line correct.", 5m, part2WorkOrderLine.WE_TransactionQuantity);

			var part1WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Part1 work order line correct.", 15m, part1WorkOrderLine.WE_TransactionQuantity);

			var part4WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Part4 work order line correct.", 5m, part4WorkOrderLine.WE_TransactionQuantity);
		}

		public void TestFinaliseDocket_CreatesReceive_ExcludeForVirtualWhs_Assemble_NotVirtualWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 3m, "UNT");
			var bom2 = Helper.CreateProductBOM(data.Part2, part3, 2m, "UNT");
			bom2.OE_ExcludeForVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.FreeStore);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(componentReceive, data.Part1, 15m, location, "", "");
			var receiveLine2 = Helper.CreateWhsReceiveLine(componentReceive, part3, 10m, location, "", "");
			var receiveLine3 = Helper.CreateWhsReceiveLine(componentReceive, part4, 5m, location, "", "");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			AssertEquals("Precondition: Part1 SOH correct.", 15m, receiveLine1.WE_StockOnHand);
			AssertEquals("Precondition: Part3 SOH correct.", 10m, receiveLine2.WE_StockOnHand);
			AssertEquals("Precondition: Part4 SOH correct.", 5m, receiveLine3.WE_StockOnHand);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created one product.", 1, receive.Lines.Count);

			var inventoryLineMainProduct = receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);
			AssertEquals("Part1 SOH correct.", 0m, receiveLine1.WE_StockOnHand);
			AssertEquals("Part3 SOH correct.", 0m, receiveLine2.WE_StockOnHand);
			AssertEquals("Part4 SOH correct.", 0m, receiveLine3.WE_StockOnHand);

			var workOrderAllLines = workOrder.AllLines;
			AssertEquals("Work order all lines correct.", 4, workOrderAllLines.Count);

			var part1WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Part1 work order line correct.", 15m, part1WorkOrderLine.WE_TransactionQuantity);

			var part2WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Part2 work order line correct.", 5m, part2WorkOrderLine.WE_TransactionQuantity);

			var part3WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Part3 work order line correct.", 10m, part3WorkOrderLine.WE_TransactionQuantity);

			var part4WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Part4 work order line correct.", 5m, part4WorkOrderLine.WE_TransactionQuantity);
		}

		public void TestFinaliseDocket_CreatesReceive_ExcludeForVirtualWhs_Disassemble()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 3m, "UNT");
			var bom2 = Helper.CreateProductBOM(data.Part2, part3, 2m, "UNT");
			bom2.OE_ExcludeForVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.FreeStore);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(componentReceive, data.Part2, 10m, location, "", "");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			AssertEquals("Precondition: Part2 SOH correct.", 10m, receiveLine.WE_StockOnHand);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 5m);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 products.", 2, receive.Lines.Count);

			var inventoryLinePart1 = receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Disassembled Part1 correct kit amounts.", 15m, inventoryLinePart1.WE_TransactionQuantity);

			var inventoryLinePart4 = receive.Lines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Disassembled Part4 correct kit amounts.", 5m, inventoryLinePart4.WE_TransactionQuantity);

			AssertEquals("Part2 SOH correct.", 5m, receiveLine.WE_StockOnHand);

			var workOrderAllLines = workOrder.AllLines;
			AssertEquals("Work order all lines correct.", 3, workOrderAllLines.Count);

			var part1WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Part1 work order line correct.", 15m, part1WorkOrderLine.WE_TransactionQuantity);

			var part2WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Part2 work order line correct.", 5m, part2WorkOrderLine.WE_TransactionQuantity);

			var part4WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Part4 work order line correct.", 5m, part4WorkOrderLine.WE_TransactionQuantity);
		}

		public void TestFinaliseDocket_CreatesReceive_ExcludeForVirtualWhs_Disassemble_NotVirtualWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 3m, "UNT");
			var bom2 = Helper.CreateProductBOM(data.Part2, part3, 2m, "UNT");
			bom2.OE_ExcludeForVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.FreeStore);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(componentReceive, data.Part2, 10m, location, "", "");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			AssertEquals("Precondition: Part2 SOH correct.", 10m, receiveLine.WE_StockOnHand);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 5m);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 products.", 3, receive.Lines.Count);

			var inventoryLinePart1 = receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Disassembled Part1 correct kit amounts.", 15m, inventoryLinePart1.WE_TransactionQuantity);

			var inventoryLinePart3 = receive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Disassembled Part3 correct kit amounts.", 10m, inventoryLinePart3.WE_TransactionQuantity);

			var inventoryLinePart4 = receive.Lines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Disassembled Part4 correct kit amounts.", 5m, inventoryLinePart4.WE_TransactionQuantity);

			AssertEquals("Part2 SOH correct.", 5m, receiveLine.WE_StockOnHand);

			var workOrderAllLines = workOrder.AllLines;
			AssertEquals("Work order all lines correct.", 4, workOrderAllLines.Count);

			var part1WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("Part1 work order line correct.", 15m, part1WorkOrderLine.WE_TransactionQuantity);

			var part2WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Part2 work order line correct.", 5m, part2WorkOrderLine.WE_TransactionQuantity);

			var part3WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Part3 work order line correct.", 10m, part3WorkOrderLine.WE_TransactionQuantity);

			var part4WorkOrderLine = workOrderAllLines.Single(l => l.WE_OP == part4.PK);
			AssertEquals("Part4 work order line correct.", 5m, part4WorkOrderLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_ConfirmAssembly

		public void TestFinaliseDocket_CreatesReceive_ConfirmAssembly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);

			workOrder.NotificationManager.Push(Notify);
			var queryUserEventArgs = new List<QueryUserEventArgs>();
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is AssemblyConfirmationQueryUserEventArgs args)
				{
					args.Response = true;
				}
				else if (e is DefaultableQueryUserEventArgs args2)
				{
					args2.Response = true;
				}
				queryUserEventArgs.Add(e);
			};

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNoExceptionThrown(() => Factory.Save());

			var receive = workOrder.Receive;
			AssertEquals("Should have created 1 product.", 1, receive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)receive.Lines.Single();
			AssertEquals("Assembled correct Product.", data.Part2.PK, inventoryLineMainProduct.WE_OP);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var queryArgs = (AssemblyConfirmationQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("", queryArgs.Caption);
			AssertEquals("", queryArgs.Message);
			AssertEquals(true, queryArgs.Response);
		}

		public void TestFinaliseDocket_CreatesReceive_CancelAssembly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);

			workOrder.NotificationManager.Push(Notify);
			var queryUserEventArgs = new List<QueryUserEventArgs>();
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is AssemblyConfirmationQueryUserEventArgs args)
				{
					args.Response = false;
				}
				else if (e is DefaultableQueryUserEventArgs args2)
				{
					args2.Response = true;
				}
				queryUserEventArgs.Add(e);
			};

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as an error occurred during Finalization. Please reload the Work Order.", Factory.Save);

			var queryArgs = (AssemblyConfirmationQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("", queryArgs.Caption);
			AssertEquals("", queryArgs.Message);
			AssertEquals(false, queryArgs.Response);

			var lastMessage = Notify.LastEvent;
			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, lastMessage.Type);
			AssertEquals("Work Order Finalization was canceled.", lastMessage.Message);
		}

		#endregion

		#region TestFinalizeDocket_FullyPicked_NoPromptFinalizesDocketAndPick

		public void TestFinalizeDocket_FullyPicked_NoPrompt_FinalizesDocketAndPick()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();
			Notify.Clear();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			var pick = Helper.CreatePickNew(workOrder);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			workOrder.NotificationManager.Push(Notify);
			Notify.PreQueryUser += (sender, e) => ((AssemblyConfirmationQueryUserEventArgs)e).Response = true;
			AssertEquals("Precondition: work order attached to pick but unfinalized", true, workOrder.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition: pick unfinalized", false, pick.IsFinalised);

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			AssertEquals("Postcondition: Last query not finalize unfinalized prompt", false, Notify.LastQueryUserEventArgs is DefaultableQueryUserEventArgs);
			AssertEquals("Postcondition: pick finalized", true, pick.IsFinalised);
			AssertEquals("Postcondition: work order finalized", true, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinalizeDocket_PartiallyPicked_TriggersPrompt_PressYesFinalizesDocketAndPick

		public void TestFinalizeDocket_PartiallyPicked_TriggersPrompt_PressYesFinalizesDocketAndPick()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();
			Notify.Clear();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			var line2 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 5m);
			var pick = Helper.CreatePickNew(workOrder);

			var childLine = line.ChildComponentLines.First();
			childLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertEquals("Precondition: work order attached to pick but unfinalized", true, workOrder.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition: pick unfinalized", false, pick.IsFinalised);

			workOrder.NotificationManager.Push(Notify);
			var queryUserEventArgs = new List<QueryUserEventArgs>();
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is AssemblyConfirmationQueryUserEventArgs args)
				{
					args.Response = true;
				}
				else if (e is DefaultableQueryUserEventArgs args2)
				{
					args2.Response = true;
				}
				queryUserEventArgs.Add(e);
			};

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			// EventArgs tested here should be second last, if finalization occurs then an assembly confirmation notification should be shown (not tested in this test)
			var secondLastQueryEventArgs = (DefaultableQueryUserEventArgs)queryUserEventArgs[queryUserEventArgs.Count - 2];
			AssertEquals("Postcondition: Second last query should be YesNoDefaultConfirmationQueryUserEventArgs", true, secondLastQueryEventArgs is DefaultableQueryUserEventArgs);
			AssertEquals("Postcondition: Caption on query matches", "Continue?", secondLastQueryEventArgs.Caption);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNo, secondLastQueryEventArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No }, secondLastQueryEventArgs.Context.DialogResultsToNotSave);
			AssertEquals("Postcondition: finalise query has correct message", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.", secondLastQueryEventArgs.Message);
			AssertEquals("Postcondition: Response is as set", true, secondLastQueryEventArgs.Response);
			AssertEquals("Postcondition: pick finalized", true, pick.IsFinalised);
			AssertEquals("Postcondition: work order finalized", true, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinalizeDocket_PartiallyPicked_TriggersPrompt_PressNoDoesNotFinalizePickOrDocket

		public void TestFinalizeDocket_PartiallyPicked_TriggersPrompt_PressNoDoesNotFinalizePickOrDocket()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();
			Notify.Clear();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			var line2 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 5m);
			var pick = Helper.CreatePickNew(workOrder);

			var childLine = line.ChildComponentLines.First();
			childLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertEquals("Precondition: work order attached to pick but unfinalized", true, workOrder.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition: pick unfinalized", false, pick.IsFinalised);

			workOrder.NotificationManager.Push(Notify);
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is AssemblyConfirmationQueryUserEventArgs args)
				{
					args.Response = true;
				}
				else if (e is DefaultableQueryUserEventArgs args2)
				{
					args2.Response = false;
				}
			};

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Postcondition: Caption on query matches", "Continue?", lastQueryEventArgs.Caption);
			AssertEquals("Postcondition: Message on query matches", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.", lastQueryEventArgs.Message);
			AssertEquals("Postcondition: Response is as set", false, lastQueryEventArgs.Response);
			AssertEquals("Postcondition: pick unfinalized", false, pick.IsFinalised);
			AssertEquals("Postcondition: work order unfinalized", false, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinalizeDocket_Unpicked_TriggersPrompt_PressYesFinalizesPickAndDocket

		public void TestFinalizeDocket_Unpicked_TriggersPrompt_PressYesFinalizesPickAndDocket()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();
			Notify.Clear();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition: work order attached to pick but unfinalized", true, workOrder.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition: pick unfinalized", false, pick.IsFinalised);

			workOrder.NotificationManager.Push(Notify);
			var queryUserEventArgs = new List<QueryUserEventArgs>();
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is AssemblyConfirmationQueryUserEventArgs args)
				{
					args.Response = true;
				}
				else if (e is DefaultableQueryUserEventArgs args2)
				{
					args2.Response = true;
				}
				queryUserEventArgs.Add(e);
			};

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			// EventArgs tested here should be second last, if finalization occurs then an assembly confirmation notification should be shown (not tested in this test)
			var secondLastQueryEventArgs = (DefaultableQueryUserEventArgs)queryUserEventArgs[queryUserEventArgs.Count - 2];
			AssertEquals("Postcondition: Second last query should be YesNoDefaultConfirmationQueryUserEventArgs", true, secondLastQueryEventArgs is DefaultableQueryUserEventArgs);
			AssertEquals("Postcondition: Caption on query matches", "Continue?", secondLastQueryEventArgs.Caption);
			AssertEquals("Postcondition: Message on query matches", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.", secondLastQueryEventArgs.Message);
			AssertEquals("Postcondition: Response is as set", true, secondLastQueryEventArgs.Response);
			AssertEquals("Postcondition: pick finalized", true, pick.IsFinalised);
			AssertEquals("Postcondition: work order finalized", true, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinalizeDocket_Unpicked_TriggersPrompt_PressNoDoesNotFinalizePickOrDocket

		public void TestFinalizeDocket_Unpicked_TriggersPrompt_PressNoDoesNotFinalizePickOrDocket()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();
			Notify.Clear();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition: work order attached to pick but unfinalized", true, workOrder.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition: pick unfinalized", false, pick.IsFinalised);

			workOrder.NotificationManager.Push(Notify);
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is AssemblyConfirmationQueryUserEventArgs args)
				{
					args.Response = true;
				}
				else if (e is DefaultableQueryUserEventArgs args2)
				{
					args2.Response = false;
				}
			};

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Postcondition: Caption on query matches", "Continue?", lastQueryEventArgs.Caption);
			AssertEquals("Postcondition: Message on query matches", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.", lastQueryEventArgs.Message);
			AssertEquals("Postcondition: Response is as set", false, lastQueryEventArgs.Response);
			AssertEquals("Postcondition: pick unfinalized", false, pick.IsFinalised);
			AssertEquals("Postcondition: work order unfinalized", false, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinalizeDocket_Unpicked_TriggerOnePrompt_FinalizesPickAndWorkOrder

		public void TestFinalizeDocket_Unpicked_TriggerOnePrompt_FinalizesPickAndWorkOrder()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();
			Notify.Clear();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			var pick = Helper.CreatePickNew(workOrder);

			pick.NotificationManager.Push(Notify);
			workOrder.NotificationManager.Push(Notify);
			var queryUserEventArgs = new List<QueryUserEventArgs>();
			Notify.PreQueryUser += (sender, e) => queryUserEventArgs.Add(e);

			AssertEquals("Precondition: pick unfinalized", false, pick.IsFinalised);
			AssertEquals("Precondition: work order unfinalized", false, workOrder.IsFinalised);

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var yesNoDefaultArgs = queryUserEventArgs.OfType<DefaultableQueryUserEventArgs>().Take(2).ToArray();
			AssertEquals("Postcondition: should only be one prompt", 1, yesNoDefaultArgs.Length);
			AssertEquals("Postcondition: finalise query has correct message", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.", yesNoDefaultArgs[0].Message);
			AssertEquals("Postcondition: pick unfinalized", true, pick.IsFinalised);
			AssertEquals("Postcondition: work order unfinalized", true, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

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
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 10m, link.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProductsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			var component = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var secondaryProduct = Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 5m);
			var componentUsage = secondaryProduct.ComponentUsages.AddNew();
			componentUsage.OPP_OE_Component = component.PK;
			componentUsage.OPP_ComponentQuantity = 0.5m;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 25m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 7.5m, link1.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link2 = secondaryProductLinks.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2.5m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProductsWithPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			Helper.CreateProductUnit(data.Part1, "UNT", "BOX", 3m);
			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			// 6x UNTs per Kit
			var component = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "BOX");
			var secondaryProduct = Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 5m);
			var componentUsage = secondaryProduct.ComponentUsages.AddNew();
			componentUsage.OPP_OE_Component = component.PK;
			componentUsage.OPP_ComponentQuantity = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 25m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 20m, link1.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link2 = secondaryProductLinks.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 10m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingMultipleSecondaryProductsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart1 = Helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = Helper.CreateProduct("P4", data.Org1);
			var component = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var secondaryProduct1 = Helper.CreateSecondaryProduct(data.Part2, secondaryPart1, 5m);
			var secondaryProduct2 = Helper.CreateSecondaryProduct(data.Part2, secondaryPart2, 3m);
			var componentUsage1 = secondaryProduct1.ComponentUsages.AddNew();
			componentUsage1.OPP_OE_Component = component.PK;
			componentUsage1.OPP_ComponentQuantity = 0.5m;
			var componentUsage2 = secondaryProduct2.ComponentUsages.AddNew();
			componentUsage2.OPP_OE_Component = component.PK;
			componentUsage2.OPP_ComponentQuantity = 1.1m;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 2 secondary products.", 3, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct1 = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart1.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 25m, inventoryLineSecondaryProduct1.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct2 = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart2.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 15m, inventoryLineSecondaryProduct2.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link1.WIP_ComponentQuantity);

			var secondaryProductLinks1 = inventoryLineSecondaryProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks1.Count());

			var link2 = secondaryProductLinks1.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2.5m, link2.WIP_ComponentQuantity);

			var secondaryProductLinks2 = inventoryLineSecondaryProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks2.Count());

			var link3 = secondaryProductLinks2.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 5.5m, link3.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			var secondComponentPart = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var component = Helper.CreateProductBOM(data.Part2, secondComponentPart, 3m, "UNT");
			var secondaryProduct = Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 1m);
			var componentUsage = secondaryProduct.ComponentUsages.AddNew();
			componentUsage.OPP_OE_Component = component.PK;
			componentUsage.OPP_ComponentQuantity = 1m;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM1";
			receive1.Lines[0].WE_WL = row.Locations[0].PK;
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", secondComponentPart, 15m, allocateLocations: false, finalise: false);
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM2";
			receive2.Lines[0].WE_WL = row.Locations[0].PK;
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 5m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 2, mainProductLinks.Count());

			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			var link1 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part1.PK);
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 10m, link1.WIP_ComponentQuantity);

			var link2 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == secondComponentPart.PK);
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == secondComponentPart.PK).PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 10m, link2.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link3 = secondaryProductLinks.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == secondComponentPart.PK).PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 5m, link3.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents_AllUsedBySecondaryProductCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			var secondComponentPart = Helper.CreateProduct("P4", data.Org1);
			var component1 = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var component2 = Helper.CreateProductBOM(data.Part2, secondComponentPart, 3m, "UNT");
			var secondaryProduct = Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 1m);
			var componentUsage1 = secondaryProduct.ComponentUsages.AddNew();
			componentUsage1.OPP_OE_Component = component1.PK;
			componentUsage1.OPP_ComponentQuantity = 0.5m;
			var componentUsage2 = secondaryProduct.ComponentUsages.AddNew();
			componentUsage2.OPP_OE_Component = component2.PK;
			componentUsage2.OPP_ComponentQuantity = 1m;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM1";
			receive1.Lines[0].WE_WL = row.Locations[0].PK;
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", secondComponentPart, 15m, allocateLocations: false, finalise: false);
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM2";
			receive2.Lines[0].WE_WL = row.Locations[0].PK;
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 5m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 2, mainProductLinks.Count());

			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			var link1 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part1.PK);
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 7.5m, link1.WIP_ComponentQuantity);

			var link2 = mainProductLinks.Single(l => l.ComponentLine.WE_OP == secondComponentPart.PK);
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == secondComponentPart.PK).PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 10m, link2.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 2, secondaryProductLinks.Count());

			var link3 = secondaryProductLinks.Single(l => l.ComponentLine.WE_OP == data.Part1.PK);
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2.5m, link3.WIP_ComponentQuantity);

			var link4 = secondaryProductLinks.Single(l => l.ComponentLine.WE_OP == secondComponentPart.PK);
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single(l => l.WE_OP == secondComponentPart.PK).PK, link4.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 5m, link4.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingWasteSecondaryProductsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			var component = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 5m);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 25m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, mainProductLinks.Count());

			var link1 = mainProductLinks.Single();
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 10m, link1.WIP_ComponentQuantity);

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link2 = secondaryProductLinks.Single();
			AssertEquals("Linked Kit Line for Waste Products.", workOrderLine.PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked Full Kit Qty for Waste Products.", 5m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldNotFailIfTotalComponentQtyMatchesStockQuantityCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			var component = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var secondaryProduct = Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 5m);
			var componentUsage = secondaryProduct.ComponentUsages.AddNew();
			componentUsage.OPP_OE_Component = component.PK;
			componentUsage.OPP_ComponentQuantity = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var notify = (NotificationBuffer)workOrder.NotificationSubscriber;
			AssertEquals(false, notify.HasErrors);
			AssertNoExceptionThrown(() => Factory.Save());

			var newReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product & 1 secondary product.", 2, newReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Assembled correct kit amounts.", 5m, inventoryLineMainProduct.WE_TransactionQuantity);

			var inventoryLineSecondaryProduct = (WhsReceiveLine)newReceive.Lines.Single(l => l.WE_OP == secondaryPart.PK);
			AssertEquals("Assembled correct Secondary Product amounts.", 25m, inventoryLineSecondaryProduct.WE_TransactionQuantity);

			var mainProductLinks = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have no Link as total quantity used.", 0, mainProductLinks.Count());

			var secondaryProductLinks = inventoryLineSecondaryProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, secondaryProductLinks.Count());

			var link = secondaryProductLinks.Single();
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			var childWorkOrderLine = (WhsWorkOrderLine)workOrderLine.ChildComponentLines.Single();
			AssertEquals("Linked Kit Line for Waste Products.", childWorkOrderLine.PK, link.WIP_WE_ComponentLine);
			AssertEquals("Linked Full Kit Qty for Waste Products.", 10m, link.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_GroupsByInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, "UNT", "BOX", 4m);
			Helper.CreateProductUnit(data.Part1, "BOX", "PLT", 2m);
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "PLT").F3_UOMType = UOMPackTypesList.Codes.Pallet;

			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 14m);
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 7m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var pick = Helper.CreatePickNew(workOrder);
			var pickLines = pick.GetAllPickLines().ToList();
			AssertEquals("Precondition: UOM Types are split correctly.", 3, pickLines.Count);

			var pickLine1 = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "PLT");
			var pickLine2 = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "BOX");
			var pickLine3 = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			AssertEquals("Precondition: UOM Types are split correctly.", 8m, pickLine1.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 4m, pickLine2.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 2m, pickLine3.WZ_Units);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine1.WZ_WE_InventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine2.WZ_WE_InventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine3.WZ_WE_InventoryLine);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: UOM Types are split correctly.", 8m, pickLine1.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 4m, pickLine2.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 2m, pickLine3.WZ_Units);
			AssertEquals("Precondition: Original Picked Inventory is correct.", inventoryLine.PK, pickLine1.WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine2.WZ_WE_InventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine3.WZ_WE_InventoryLine);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var workOrderReceive = workOrder.Receive;
			AssertEquals("Should have created 1 product.", 1, workOrderReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)workOrderReceive.Lines.Single();
			AssertEquals("Assembled correct Product.", data.Part2.PK, inventoryLineMainProduct.WE_OP);
			AssertEquals("Assembled correct kit amounts.", 7m, inventoryLineMainProduct.WE_TransactionQuantity);

			var links = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links.Count());

			var componentInventory = inventoryLineMainProduct.ComponentInventoryLines;
			AssertEquals("Should have only one Inventory.", 1, componentInventory.Count);
			AssertEquals("Should have only one Inventory.", inventoryLine, componentInventory.Single());

			var componentPickLinesForAssembly = inventoryLineMainProduct.BOMComponentLinksForBinding;
			AssertEquals("Should have only one Pick Line for Binding.", 1, componentPickLinesForAssembly.Count);
			AssertEquals("Pick Line should have correct Inventory to display.", inventoryLine, componentPickLinesForAssembly[0].InventoryLine);
			AssertEquals("Pick Line should have correct Quantity to display.", 14m, componentPickLinesForAssembly[0].UnitsToPick);

			var link = links.Single();
			var workOrderLine = (WhsWorkOrderLine)workOrder.Lines.Single();
			AssertEquals("Linked correct Component Line.", workOrderLine.ChildComponentLines.Single().PK, link.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 14m, link.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestFinaliseDocket_Disassembly_LinkedInventory_GroupsByInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, "UNT", "BOX", 4m);
			Helper.CreateProductUnit(data.Part1, "BOX", "PLT", 2m);
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "PLT").F3_UOMType = UOMPackTypesList.Codes.Pallet;

			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 14m);
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 7m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var pick = Helper.CreatePickNew(workOrder);
			var pickLines = pick.GetAllPickLines().ToList();
			AssertEquals("Precondition: UOM Types are split correctly.", 3, pickLines.Count);

			var pickLine1 = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "PLT");
			var pickLine2 = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "BOX");
			var pickLine3 = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			AssertEquals("Precondition: UOM Types are split correctly.", 8m, pickLine1.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 4m, pickLine2.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 2m, pickLine3.WZ_Units);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine1.WZ_WE_InventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine2.WZ_WE_InventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine3.WZ_WE_InventoryLine);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: UOM Types are split correctly.", 8m, pickLine1.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 4m, pickLine2.WZ_Units);
			AssertEquals("Precondition: UOM Types are split correctly.", 2m, pickLine3.WZ_Units);
			AssertEquals("Precondition: Original Picked Inventory is correct.", inventoryLine.PK, pickLine1.WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine2.WZ_WE_InventoryLine);
			AssertEquals("Precondition: Allocated Inventory is correct.", inventoryLine.PK, pickLine3.WZ_WE_InventoryLine);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var workOrderReceive = workOrder.Receive;
			AssertEquals("Precondition: Should have created 1 product.", 1, workOrderReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)workOrderReceive.Lines.Single();
			AssertEquals("Precondition: Assembled correct Product.", data.Part2.PK, inventoryLineMainProduct.WE_OP);
			AssertEquals("Precondition: Assembled correct kit amounts.", 7m, inventoryLineMainProduct.WE_TransactionQuantity);

			workOrderReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(workOrderReceive);

			Factory.Save();

			var disassemblyWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", WorkOrderType.Codes.Disassemble, data.Part2, 7m);
			Helper.CreatePickNew(disassemblyWorkOrder);

			disassemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(disassemblyWorkOrder);

			var disassemblyReceive = disassemblyWorkOrder.Receive;
			AssertEquals("Should have created 1 product.", 1, disassemblyReceive.Lines.Count);

			var inventoryLineComponentProduct = (WhsReceiveLine)disassemblyReceive.Lines.Single();
			AssertEquals("Disassembled Component is correct Product.", data.Part1.PK, inventoryLineComponentProduct.WE_OP);
			AssertEquals("Disassembled correct component amounts.", 14m, inventoryLineComponentProduct.WE_TransactionQuantity);
		}

		public void TestFinaliseDocket_Disassembly_LinkedInventory_MultipleComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, part3, 6m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 3m, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 3m, null, "PLT-2");
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 1m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var workOrderReceive = workOrder.Receive;
			AssertEquals("Precondition: Should have created 1 product.", 1, workOrderReceive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)workOrderReceive.Lines.Single();
			AssertEquals("Precondition: Assembled correct Product.", data.Part2.PK, inventoryLineMainProduct.WE_OP);
			AssertEquals("Precondition: Assembled correct kit amounts.", 1m, inventoryLineMainProduct.WE_TransactionQuantity);

			workOrderReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(workOrderReceive);

			Factory.Save();

			var disassemblyWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", WorkOrderType.Codes.Disassemble, data.Part2, 1m);
			Helper.CreatePickNew(disassemblyWorkOrder);

			disassemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(disassemblyWorkOrder);

			var disassemblyReceive = disassemblyWorkOrder.Receive;
			AssertEquals("Should have created 3 Inventory Records.", 3, disassemblyReceive.Lines.Count);

			var inventoryLineComponentProduct1 = (WhsReceiveLine)disassemblyReceive.Lines.Single(dl => dl.WE_OP == data.Part1.PK);
			var inventoryLinesComponentProduct2 = disassemblyReceive.Lines.Where(dl => dl.WE_OP == part3.PK);
			AssertEquals("Disassembled correct component amounts.", 2m, inventoryLineComponentProduct1.WE_TransactionQuantity);
			AssertEquals("Disassembled 2 unique components for Second Component.", 2, inventoryLinesComponentProduct2.Count());
			AssertEquals("Disassembled correct component amounts for Second Component.", true, inventoryLinesComponentProduct2.All(l => l.WE_TransactionQuantity == 3m));
		}

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldFailIfTotalComponentQtyExceedsStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			var secondaryPart = Helper.CreateProduct("P3", data.Org1);
			var component = Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var secondaryProduct = Helper.CreateSecondaryProduct(data.Part2, secondaryPart, 5m);
			var componentUsage = secondaryProduct.ComponentUsages.AddNew();
			componentUsage.OPP_OE_Component = component.PK;
			componentUsage.OPP_ComponentQuantity = 3m; // Validation prevents saving this normally
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			receive.Lines[0].CustomsData.WB_EntryKey = "ENTRYNUM";
			receive.Lines[0].WE_WL = row.Locations[0].PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var notify = (NotificationBuffer)workOrder.NotificationSubscriber;
			AssertEquals(true, notify.HasErrors);
			AssertEquals("Secondary Product Setup is not correct for Component Product: P1. Total component quantity used exceeds quantity available for Main Product P2.", notify.AsString.Trim());
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as an error occurred during Finalization. Please reload the Work Order.", Factory.Save);
		}

		public void TestFinaliseDocket_CreatesReceive_DoesNotLinkZeroUnitAssemblyLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 0m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 1 product.", 1, receive.Lines.Count);

			var inventoryLineMainProduct = (WhsReceiveLine)receive.Lines.Single();
			AssertEquals("Assembled correct Product.", data.Part2.PK, inventoryLineMainProduct.WE_OP);
			AssertEquals("Assembled correct kit amounts.", 0m, inventoryLineMainProduct.WE_TransactionQuantity);

			var links = inventoryLineMainProduct.BOMComponentLinks;
			AssertEquals("Should have no Links.", 0, links.Count());
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var line2 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertEquals(receive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(receive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links1.Count());

			var link1 = links1.Single();
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 4m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var links2 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links2.Count());

			var link2 = links2.Single();
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 6m, link2.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_UpdatesPackQuantityCorrectlyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];
			AssertEquals("Precondition: Package Qty is correct.", 5m, workOrderLine.WE_PackQuantity);
			AssertEquals("Precondition: Package Qty is correct.", 10m, workOrderLine.ChildComponentLines.Single().WE_PackQuantity);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var line2 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertEquals(receive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(receive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Package Qty is correct.", 2m, line1.WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 3m, line2.WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 4m, line1.ChildComponentLines.Single().WE_PackQuantity);
			AssertEquals("Package Qty is correct.", 6m, line2.ChildComponentLines.Single().WE_PackQuantity);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplitCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, part3, 1m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 2m);

			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 2m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 2m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

			var pick = Helper.CreatePickNew(workOrder);
			// Pick all lines
			var now = ZDateTimeOffset.Now;
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = now);
			Factory.Save(); // create Outbound Transfers

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventory1 = receive1.Lines[0].PK;
			var inventory2 = receive2.Lines[0].PK;
			var inventory3 = receive3.Lines[0].PK;
			var inventory4 = receive4.Lines[0].PK;
			var line1 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count == 2);
			var line2 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count == 1);
			AssertEquals(1, line1.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_OriginalPickedInventoryLine == inventory1));
			AssertEquals(1, line1.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_OriginalPickedInventoryLine == inventory2));
			AssertEquals(inventory4, line1.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals(inventory3, line2.ChildComponentLines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals(inventory4, line2.ChildComponentLines.Single(l => l.WE_OP == part3.PK).PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 Lines.", 2, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleKits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(part4, part3, 3m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 3m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 9m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine1 = workOrder.Lines[0];
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, part4, 4m);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine1.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine1.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 4, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for Product 2.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines for Product 4.", 2, workOrder.Lines.Count(l => l.WE_OP == part4.PK));

			var line1 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 2m);
			var line2 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 3m);
			var line3 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_OP == part4.PK && l.WE_TransactionQuantity == 1m);
			var line4 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_OP == part4.PK && l.WE_TransactionQuantity == 3m);
			AssertEquals(receive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(receive2.Lines[0].PK, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(receive3.Lines[0].PK, line3.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(receive4.Lines[0].PK, line4.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine1.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 4 Lines.", 4, receive.Lines.Count);
			AssertEquals("Should have created 2 Lines Product 2.", 2, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));
			AssertEquals("Should have created 2 Lines Product 4.", 2, receive.Lines.Count(l => l.WE_OP == part4.PK));

			var inventoryLineMainProduct1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 2m);
			var links1 = inventoryLineMainProduct1.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links1.Count());

			var link1 = links1.Single();
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 4m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 3m);
			var links2 = inventoryLineMainProduct2.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links2.Count());

			var link2 = links2.Single();
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 6m, link2.WIP_ComponentQuantity);

			var inventoryLineMainProduct3 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_OP == part4.PK && l.WE_TransactionQuantity == 1m);
			var links3 = inventoryLineMainProduct3.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links3.Count());

			var link3 = links3.Single();
			AssertEquals("Linked correct Component Line.", line3.ChildComponentLines.Single().PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 3m, link3.WIP_ComponentQuantity);

			var inventoryLineMainProduct4 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_OP == part4.PK && l.WE_TransactionQuantity == 3m);
			var links4 = inventoryLineMainProduct4.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links4.Count());

			var link4 = links4.Single();
			AssertEquals("Linked correct Component Line.", line4.ChildComponentLines.Single().PK, link4.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 9m, link4.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsNotInKitAmountsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 7m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 3, workOrder.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var line1 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.Single().PickLines.Count == 1);
			var line2 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.Single().PickLines.Count == 2);
			var line3 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
			AssertEquals(receive1.Lines[0].PK, line1.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals(1, line2.ChildComponentLines.Single().PickLines.Count(pl => pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals(1, line2.ChildComponentLines.Single().PickLines.Count(pl => pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
			AssertEquals(receive2.Lines[0].PK, line3.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

			var receive = workOrder.Receive;
			AssertEquals("Should have created 3 Lines.", 3, receive.Lines.Count);
			AssertEquals("Should have created 3 Lines for the same Product.", 3, receive.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventoryLineMainProduct1 = receive.Lines.Cast<WhsReceiveLine>()
				.Single(l => l.WE_TransactionQuantity == 1m && l.BOMComponentLinks.Single().ComponentLine.PickLines.Count == 1);
			var link1 = inventoryLineMainProduct1.BOMComponentLinks.Single();
			AssertEquals("Linked correct Component Line.", line1.ChildComponentLines.Single().PK, link1.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link1.WIP_ComponentQuantity);

			var inventoryLineMainProduct2 = receive.Lines.Cast<WhsReceiveLine>()
				.Single(l => l.WE_TransactionQuantity == 1m && l.BOMComponentLinks.Single().ComponentLine.PickLines.Count == 2);
			var link2 = inventoryLineMainProduct2.BOMComponentLinks.Single();
			AssertEquals("Linked correct Component Line.", line2.ChildComponentLines.Single().PK, link2.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 2m, link2.WIP_ComponentQuantity);

			var inventoryLineMainProduct3 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var links3 = inventoryLineMainProduct3.BOMComponentLinks;
			AssertEquals("Should have one Link.", 1, links3.Count());

			var link3 = links3.Single();
			AssertEquals("Linked correct Component Line.", line3.ChildComponentLines.Single().PK, link3.WIP_WE_ComponentLine);
			AssertEquals("Linked correct Component Qty.", 6m, link3.WIP_ComponentQuantity);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsLessThanKitAmountCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 9m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertNotEquals(ZDateTime.Empty, workOrderLine.WE_FinalisedDate);
			AssertEquals(DocketLineStatus.Codes.Finalised, workOrderLine.WE_DocketLineStatus);
			AssertEquals("Should have split the Kit Lines based on Component Inventory.", 2, workOrder.Lines.Count);
			AssertEquals("Should have created 2 Lines for the same Product.", 2, workOrder.Lines.Count(l => l.WE_OP == data.Part2.PK));

			var inventory1 = receive1.Lines[0].PK;
			var inventory2 = receive2.Lines[0].PK;
			var line1 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var line2 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
			AssertEquals(1, line1.ChildComponentLines.Single().PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory1));
			AssertEquals(1, line1.ChildComponentLines.Single().PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory2));
			AssertEquals(inventory2, line2.ChildComponentLines.Single().PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
			AssertEquals("All new Lines are Finalised.", true, workOrder.AllLines.All(l => l.WE_FinalisedDate == workOrderLine.WE_FinalisedDate));

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
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_GreedySplitScenarioCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, part3, 1m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 18m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 3m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 3m);
			var receive5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", part3, 4m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

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
			var line1 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m);
			var line2 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 2m);
			var line3 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
			var line4 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 4m);
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
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, part3, 4m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 8m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 12m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

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
			AssertEquals("Should have Two 1x Kit Lines.", 2, workOrder.Lines.Cast<WhsWorkOrderLine>().Count(l => l.WE_TransactionQuantity == 1m));
			AssertEquals("Should have 1x Kit for R1 & R3.", 1, workOrder.Lines.Cast<WhsWorkOrderLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory1) == 1
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory3) == 1));
			AssertEquals("Should have 1x Kit for R2 & R3.", 1, workOrder.Lines.Cast<WhsWorkOrderLine>()
				.Count(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory2) == 1
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Single().WZ_WE_InventoryLine == inventory3) == 1));

			var lastLine = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
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
		}

		protected override void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsOfDifferingInventoryAmountsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, part3, 4m, "UNT");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 7m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 4m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 4m);
			var receive5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", part3, 12m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = workOrder.Lines[0];

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
			var line1 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.All(cl => cl.PickLines.Count == 1));
			var line2 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 1m && l.ChildComponentLines.Any(cl => cl.PickLines.Count > 1));
			AssertEquals("Should have 1x Kit for R1 & (R3 or R4).", line1, workOrder.Lines.Cast<WhsWorkOrderLine>()
				.Single(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Count(cl => cl.PickLines.Count == 1 && cl.PickLines.Single().WZ_WE_InventoryLine == inventory1) == 1
				&&
				(
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory3
					||
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory4)
				));
			AssertEquals("Should have 1x Kit for R1 & R2 & (R3 or R4).", line2, workOrder.Lines.Cast<WhsWorkOrderLine>()
				.Single(l => l.WE_TransactionQuantity == 1m
				&& l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory1) == 1
				&& l.ChildComponentLines.Single(cl => cl.WE_OP == data.Part1.PK).PickLines.Count(pl => pl.WZ_WE_InventoryLine == inventory2) == 1
				&&
				(
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory3
					||
					l.ChildComponentLines.Single(cl => cl.WE_OP == part3.PK).PickLines.Single().WZ_WE_InventoryLine == inventory4)
				));

			var line3 = workOrder.Lines.Cast<WhsWorkOrderLine>().Single(l => l.WE_TransactionQuantity == 3m);
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
		}

		#endregion

		#region TestFinaliseDocketAbortsAndNotifiesUserIfRunRreFinaliseValidationFails

		public override void TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails()
		{
			Docket.Lines.AddNew(); // need a line to pick
			WhsPick pick = Factory.New<WhsPick>();
			pick.PickOrders(new WhsWorkOrder[] { Docket });

			base.TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails();
		}

		#endregion

		#region TestCannotFinaliseWorkOrderWithUnfinalisedChildren

		public void TestCannotFinaliseWorkOrderWithUnfinalisedChildren()
		{
			Data.CreateBOMComponentsInInventory(); // enough to build 10 bikes
			Factory.Save(); // SupplierParts lookups will fail finalise validation if parts are not in the db

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(Data.Org1, data.Whs1, "123");
			WhsWorkOrderLine line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 15m); // shortfall of 5

			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed", true, workOrder.IsAttachedToPickButNotFinalised);

			// auto-create work orders to fulfill shortfalls
			workOrder.BOM.AutoCreateWorkOrders(Notify);
			Factory.Save(); // make related jobs available

			workOrder.FinaliseDocket();

			// level 1
			WhsWorkOrder enginesWO = workOrder.CurrentWorkOrders.Single(wo => wo.Lines.Single().SupplierPart == data.BOM.BikeEngine);
			WhsWorkOrder wheelsWO = workOrder.CurrentWorkOrders.Single(wo => wo.Lines.Single().SupplierPart == data.BOM.BikeWheel);
			// level 2
			WhsWorkOrder pistonsWO = enginesWO.CurrentWorkOrders.Single();

			string expectedMsg = string.Format(
				"Cannot finalize this Work Order because the following related Work Orders exist:" + "\r\n" +
				"\r\n" +
				"	{0}" + "\r\n" +
				"	{1}" + "\r\n" +
				"	{2}" + "\r\n" +
				"\r\n" +
				"You must first finalize the related Work Orders.",
				enginesWO.WD_DocketID,
				pistonsWO.WD_DocketID,
				wheelsWO.WD_DocketID);

			var notify = (NotificationBuffer)workOrder.NotificationManager.Peek;
			AssertContains(expectedMsg, notify.Events[0].Message);
			AssertEquals(false, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits

		#region TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_AssemblyWorkOrder

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_AssemblyWorkOrder()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductUnit(bomProduct, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 20m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 20m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create the work order
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 20m, Constants.PkgUnit.Pallet); // 1 PLT
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(workOrder);
			AssertEquals("Precondition.", true, workOrder.IsAttachedToPickButNotFinalised);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertNotNull("Precondition", workOrder.Receive);
			var inventoryLine = (WhsInventoryView)workOrder.Receive.Inventory.Single();
			AssertEquals(1m, inventoryLine.InDocketLine.WE_PackQuantity);
			AssertEquals(Constants.PkgUnit.Pallet, inventoryLine.WI_F3_NKPackType);
			AssertEquals(20m, inventoryLine.WI_InDocketLineUnits);
			AssertEquals(Constants.PkgUnit.Unit, inventoryLine.WI_UnitsUQ);
		}

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_WhenComponentIsShortedToZero()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// Create inventory
			data.CreateProductInInventory("20 Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("10 Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("10 Polish", data.BOM.Polish, 10m);
			Factory.Save();

			// Create a WorkOrder
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 10m);

			// Sort picked inventories
			var pick = Helper.CreatePickNew(bikeWorkOrder);

			// Short the engine component to 0
			var engineComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.BikeEngine.PK).AvailableInventories[0];
			engineComponentInv.PickLineQuantity = 0m;

			// Finalise the WorkOrder
			bikeWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(bikeWorkOrder);
			Factory.Save();

			// Expect items in stock
			var stock = bikeWorkOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 3, stock.Count);

			var bike = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Bike);
			AssertEquals("Number of bikes in stock is incorrect.", 0m, bike.WI_TotalUnits);
			AssertEquals("Nothing should be linked.", 0, ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Count());

			var wheel = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeWheel);
			AssertEquals("Number of wheels in a stock is incorrect.", 20m, wheel.WI_TotalUnits);

			var polish = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Polish);
			AssertEquals("Number of polishes in a stock is incorrect.", 10m, polish.WI_TotalUnits);
		}

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_WhenComponentIsShortedBelowWhatIsNeededButNotZero()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// Create inventory
			data.CreateProductInInventory("20 Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("10 Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("10 Polish", data.BOM.Polish, 10m);
			Factory.Save();

			// Create a WorkOrder
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 10m);

			// Sort picked inventory
			var pick = Helper.CreatePickNew(bikeWorkOrder);

			// Short the wheels component to 1
			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.BikeWheel.PK).AvailableInventories[0];
			wheelComponentInv.PickLineQuantity = 1m;

			// Finalise the WorkOrder
			bikeWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(bikeWorkOrder);
			Factory.Save();
			AssertEquals("Precondition - Receive was not written to database.", true, bikeWorkOrder.Receive.IsInDatabase);

			// Expect items in a stock
			var bike = bikeWorkOrder.Receive.Inventory.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Bike);
			AssertEquals("Number of bikes in stock is incorrect.", 0m, bike.WI_TotalUnits);
			AssertEquals("Nothing should be linked.", 0, ((WhsReceiveLine)bike.InDocketLine).BOMComponentLinks.Count());
		}

		#endregion

		#region TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_DisassemblyWorkOrder

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_DisassemblyWorkOrder()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			Helper.CreateProductUnit(componentProduct1, Constants.PkgUnit.Unit, Constants.PkgUnit.Roll, 1m);
			Helper.CreateProductUnit(componentProduct2, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 1m);

			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Roll);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Box);
			Helper.CreateProductUnit(bomProduct, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 20m);

			// receive 20 units of bomProduct
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bomProduct, 20m);
			Factory.Save();

			// create a disassemble work order
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Disassemble);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m, Constants.PkgUnit.Pallet);
			Factory.Save();

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				workOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertNotNull("Precondition", workOrder.Receive);
			var inventoryCollection = workOrder.Receive.Inventory;
			AssertEquals(2, inventoryCollection.Count);
			var inventoryForBomPart1 = inventoryCollection.Cast<WhsInventoryView>().Single(i => i.WI_OP == componentProduct1.PK);
			AssertEquals(20m, inventoryForBomPart1.InDocketLine.WE_PackQuantity);
			AssertEquals(Constants.PkgUnit.Roll, inventoryForBomPart1.WI_F3_NKPackType);
			AssertEquals(20m, inventoryForBomPart1.WI_InDocketLineUnits);
			AssertEquals(Constants.PkgUnit.Unit, inventoryForBomPart1.WI_UnitsUQ);

			var inventoryForBomPart2 = inventoryCollection.Cast<WhsInventoryView>().Single(i => i.WI_OP == componentProduct2.PK);
			AssertEquals(20m, inventoryForBomPart2.InDocketLine.WE_PackQuantity);
			AssertEquals(Constants.PkgUnit.Box, inventoryForBomPart2.WI_F3_NKPackType);
			AssertEquals(20m, inventoryForBomPart2.WI_InDocketLineUnits);
			AssertEquals(Constants.PkgUnit.Unit, inventoryForBomPart2.WI_UnitsUQ);
		}

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_DisassemblyWorkOrderWhenKitsQuantityIsShorted()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create receive and workOrder
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.BOM.Bike, 10m);
			Factory.Save();
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble, data.BOM.Bike, 10m);

			// pick the WorkOrder and short bikes quantity to 9
			var pick = Helper.CreatePickNew(workOrder);
			var bikesInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.Bike.PK).AvailableInventories[0];
			bikesInventory.PickLineQuantity = 9m;
			Factory.Save();

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Precondition", DocketStatus.Codes.Finalised, workOrder.WD_DocketStatus);
			Factory.Save();

			// check stock
			var stock = workOrder.Receive.Lines;
			AssertEquals("Number of items in a stock is incorrect.", 2, stock.Count);

			var engineInventory = stock.Single(l => l.WE_OP == data.BOM.BikeEngine.PK);
			AssertEquals("Should have received 9 x Engines.", 9m, engineInventory.WE_StockOnHand);

			var wheelInventory = stock.Single(l => l.WE_OP == data.BOM.BikeWheel.PK);
			AssertEquals("Should have received 18 x Wheels.", 18m, wheelInventory.WE_StockOnHand);
		}

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_DisassemblyWorkOrderWhenKitsQuantityIsShortedToZero()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create receive and work order
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.BOM.Bike, 10m);
			Factory.Save();
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble, data.BOM.Bike, 10m);

			// pick the WorkOrder and short bikes quantity to 0
			var pick = Helper.CreatePickNew(workOrder);
			var bikesInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.Bike.PK).AvailableInventories[0];
			bikesInventory.PickLineQuantity = 0m;
			Factory.Save();

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			Factory.Save();

			// check stock
			var stock = workOrder.Receive.Lines;
			AssertEquals("Precondition - number of items in stock is incorrect.", 2, stock.Count);

			var engineInventory = stock.Single(l => l.WE_OP == data.BOM.BikeEngine.PK);
			AssertEquals("Should have received 0 x Engines.", 0m, engineInventory.WE_StockOnHand);

			var wheelInventory = stock.Single(l => l.WE_OP == data.BOM.BikeWheel.PK);
			AssertEquals("Should have received 0 x Wheels.", 0m, wheelInventory.WE_StockOnHand);
		}

		public void TestFinaliseDocket_CreateReceiveWithCorrectTotalUnits_DisassemblyWorkOrderForDifferentLinesWithSameProduct()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create receive and work order
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.BOM.Bike, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 5m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			// pick the WorkOrder and short bikes quantity for one line
			var pick = Helper.CreatePickNew(workOrder);
			workOrderLine1.PickLines[0].WZ_Units = 3m;
			Factory.Save();

			// finalise the WorkOrder
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			// check stock
			var stock = workOrder.Receive.Lines;
			AssertEquals("Precondition - number of items in stock is incorrect.", 4, stock.Count);

			var engineInventory1 = stock.Single(l => (l.WE_OP == data.BOM.BikeEngine.PK) && (l.WE_StockOnHand == 3m));
			AssertEquals("Incorrect number of engines received in stock for workOrderLine1.", 3m, engineInventory1.WE_TransactionQuantity);

			var wheelInventory1 = stock.Single(l => (l.WE_OP == data.BOM.BikeWheel.PK) && (l.WE_StockOnHand == 6m));
			AssertEquals("Incorrect number of wheels received in stock for workOrderLine1.", 6m, wheelInventory1.WE_TransactionQuantity);

			var engineInventory2 = stock.Single(l => (l.WE_OP == data.BOM.BikeEngine.PK) && (l.WE_StockOnHand == 2m));
			AssertEquals("Incorrect number of engines received in stock for workOrderLine2.", 2m, engineInventory2.WE_TransactionQuantity);

			var wheelInventory2 = stock.Single(l => (l.WE_OP == data.BOM.BikeWheel.PK) && (l.WE_StockOnHand == 4m));
			AssertEquals("Incorrect number of wheels received in stock for workOrderLine2.", 4m, wheelInventory2.WE_TransactionQuantity);
		}

		#endregion

		#endregion

		#region TestFinaliseDocket_Assembly_FinalisingWorkOrderWithZeroQuantityLines

		public void TestFinaliseDocket_Assembly_FinalisingWorkOrderWithZeroQuantityLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var productComponent1 = Helper.CreateProduct(data.Org1, "C1");
			var productComponent2 = Helper.CreateProduct(data.Org1, "C2");
			Helper.CreateProductBOM(bomProduct, productComponent1, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bomProduct, productComponent2, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, productComponent1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, productComponent2, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create the work order
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 2m);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 0m);
			Factory.Save();
			AssertEquals("Precondition", 2, workOrder.Lines.Count);

			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition.", true, workOrder.IsAttachedToPickButNotFinalised);

			AssertNoExceptionThrown("No exception thrown during finalisation.", () => workOrder.FinaliseDocketAlwaysFinalisingPick());
			AssertEquals("Work Order is finalised.", true, workOrder.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_CreateReceiveLines_OverPicked

		public void TestFinaliseDocket_CreateReceiveLines_OverPicked_AttributesRetained()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeEngine, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeEngine, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeEngine, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeEngine, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeEngine, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeEngine, AttributeNumber.ExpiryDate, true);

			// Create inventory enough to assemble 1 bikes
			var packingDate = ZDate.Today.AddDays(7);
			var expiryDate = ZDate.Today.AddMonths(1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeEngine.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "D");
			receiveLine1.WI_AllocationKey = "AKEY";
			receive.FinaliseDocket();
			data.CreateProductInInventory("2 Wheels", data.BOM.BikeWheel, 2m);
			data.CreateProductInInventory("1 Polish", data.BOM.Polish, 1m);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// Create a WorkOrder with one bikes ordered
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 1m);

			// Shortfall Polish quantity
			var pick = Helper.CreatePickNew(bikeWorkOrder);
			var polishComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.Polish.PK).AvailableInventories[0];
			polishComponentInv.PickLineQuantity = 0m;

			// Finalise the WorkOrder
			bikeWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(bikeWorkOrder);
			Factory.Save();

			// Expect items in stock
			var stock = bikeWorkOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 3, stock.Count);

			var bike = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Bike);
			AssertEquals("Number of bikes in stock is incorrect.", 0m, bike.WI_TotalUnits);

			var polish = stock.Cast<WhsInventoryView>().SingleOrDefault(i => i.SupplierPart == data.BOM.Polish);
			AssertNull("Polish is not picked thus not returned.", polish);

			var wheels = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeWheel);
			AssertEquals("Number of wheels in stock is incorrect.", 2m, wheels.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)wheels.InDocketLine).BOMComponentLinks.Count());

			var engine = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeEngine);
			AssertEquals("Number of engines in stock is incorrect.", 1m, engine.WI_TotalUnits);
			AssertEquals(0, ((WhsReceiveLine)engine.InDocketLine).BOMComponentLinks.Count());

			var inventory = (WhsReceiveLine)engine.InDocketLine;
			AssertEquals("Attributes of engine are retained.", "A", inventory.WE_PartAttrib1);
			AssertEquals("Attributes of engine are retained.", "B", inventory.WE_PartAttrib2);
			AssertEquals("Attributes of engine are retained.", "C", inventory.WE_PartAttrib3);
			AssertEquals("Attributes of engine are retained.", "S1", inventory.WE_SerialNumber);
			AssertEquals("Attributes of engine are retained.", expiryDate, inventory.WE_ExpiryDate);
			AssertEquals("Attributes of engine are retained.", packingDate, inventory.WE_PackingDate);
			AssertEquals("Attributes of engine are retained.", "D", inventory.WE_BondedEntryKey);
			AssertEquals("Attributes of engine are retained.", "AKEY", inventory.WE_AllocationKey);
			AssertEquals("Original DocketLine retained.", receiveLine1.PK, inventory.WE_WE_OriginalDocketLineForRating);
		}

		public void TestFinaliseDocket_CreateReceiveLines_OverPicked_CustomsDataRetained()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", Helper.CreateProduct("P3", data.Org1), 10m, "BONDED-1", allocateLocations: false, finalise: false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var bwa1 = receive.Lines[0].CustomsData;

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			var invCustomsData = componentReceive.Lines[0].CustomsData;
			invCustomsData.WB_WB_InwardsEntry = bwa1.PK;
			invCustomsData.WB_InwardStyle = "is";
			invCustomsData.WB_EntryLineNo = 1;
			invCustomsData.WB_EntryKey = "DEF";
			invCustomsData.WB_AddInfo = "addinfo";
			invCustomsData.WB_BondedWhsQty = 19m;
			invCustomsData.WB_BondedWhsUnitOfQty = "KG";
			invCustomsData.WB_CustomsQty = 3m;
			invCustomsData.WB_CustomsUnitOfQty = "LB";
			invCustomsData.WB_ValueForDuty = 0.99m;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			AssertEquals("Precondition: WB_BondedWhsQty is set to WE_TransactionQuantity", 10m, invCustomsData.WB_BondedWhsQty);
			AssertEquals("Precondition: WB_WB_InwardsEntry is not empty", false, invCustomsData.WB_WB_InwardsEntry.IsEmpty);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			var pick = Helper.CreatePickNew(workOrder);
			var componentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.Part1.PK).AvailableInventories[0];
			componentInv.PickLineQuantity = 3m;
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var stock = workOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 2, stock.Count);

			var part2 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.Part2);
			AssertEquals("Number of part2 in stock is incorrect.", 1m, part2.WI_TotalUnits);
			var part1 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.Part1);
			AssertEquals("Number of part1 in stock is incorrect.", 1m, part1.WI_TotalUnits);

			var inventory = (WhsReceiveLine)part1.InDocketLine;
			var customsData = inventory.CustomsData;
			AssertEquals("CustomsData.WB_WB_InwardsEntry.", bwa1.PK, customsData.WB_WB_InwardsEntry);
			AssertEquals("CustomsData.WB_InwardStyle.", "is", customsData.WB_InwardStyle);
			AssertEquals("CustomsData.WB_EntryLineNo.", (short)1, customsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey.", "DEF", customsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", "addinfo", customsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 10m, customsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", "KG", customsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 3m, customsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", "LB", customsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_ValueForDuty", 0.99m, customsData.WB_ValueForDuty);
		}

		public void TestFinaliseDocket_CreateReceiveLines_OverPicked_CustomsDataRetained_WithOutboundTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			Helper.EnableWarehouseForExcise(data.Whs1, true);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var area = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Excise);
			var whsRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "EXC");
			whsRow.Locations[0].WLV_WA_PickingArea = area.PK;
			whsRow.Locations[0].WLV_WA_PutawayArea = area.PK;

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.Lines[0].WE_WL = whsRow.Locations[0].PK;

			var invCustomsData = componentReceive.Lines[0].CustomsData;
			invCustomsData.WB_InwardStyle = "is";
			invCustomsData.WB_EntryLineNo = 1;
			invCustomsData.WB_EntryKey = "DEF";
			invCustomsData.WB_AddInfo = "addinfo";
			invCustomsData.WB_BondedWhsQty = 19m;
			invCustomsData.WB_BondedWhsUnitOfQty = "KG";
			invCustomsData.WB_CustomsQty = 3m;
			invCustomsData.WB_CustomsUnitOfQty = "LB";
			invCustomsData.WB_ValueForDuty = 0.99m;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			AssertEquals("Precondition: WB_BondedWhsQty is set to WE_TransactionQuantity", 10m, invCustomsData.WB_BondedWhsQty);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var pick = Helper.CreatePickNew(workOrder);
			var componentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.Part1.PK).AvailableInventories[0];
			componentInv.PickLineQuantity = 3m;
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			Factory.Save();

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var stock = workOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 2, stock.Count);

			var part2 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.Part2);
			AssertEquals("Number of part2 in stock is incorrect.", 1m, part2.WI_TotalUnits);
			var part1 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.Part1);
			AssertEquals("Number of part1 in stock is incorrect.", 1m, part1.WI_TotalUnits);

			var inventory = (WhsReceiveLine)part1.InDocketLine;
			var customsData = inventory.CustomsData;
			AssertEquals("CustomsData.WB_InwardStyle.", "is", customsData.WB_InwardStyle);
			AssertEquals("CustomsData.WB_EntryLineNo.", (short)1, customsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey.", "DEF", customsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", "addinfo", customsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 10m, customsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", "KG", customsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 3m, customsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", "LB", customsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_ValueForDuty", 0.99m, customsData.WB_ValueForDuty);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var customsData2 = newFactory.Load<WhsBondedWarehouseAttribute>(inventory.CustomsData.PK);
			AssertEquals("CustomsData.WB_InwardStyle.", "is", customsData2.WB_InwardStyle);
			AssertEquals("CustomsData.WB_EntryLineNo.", (short)1, customsData2.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey.", "DEF", customsData2.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", "addinfo", customsData2.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 10m, customsData2.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", "KG", customsData2.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 3m, customsData2.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", "LB", customsData2.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_ValueForDuty", 0.99m, customsData2.WB_ValueForDuty);
		}

		public void TestFinaliseDocket_CreateReceiveLines_OverPicked_UnitsOfAttributesDoesnotExceedPicked()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.BikeWheel, AttributeNumber.One, true);

			// Create inventory enough to assemble 1 bikes
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, new TestNotificationBuffer());
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeWheel, 1m);
			receiveLine1.WI_PartAttrib1 = "A";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeWheel, 1m);
			receiveLine2.WI_PartAttrib1 = "B";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			data.CreateProductInInventory("1 Engine", data.BOM.BikeEngine, 1m);
			data.CreateProductInInventory("1 Polish", data.BOM.Polish, 1m);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// Create a WorkOrder with one bikes ordered
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 1m);

			// Shortfall Polish quantity
			var pick = Helper.CreatePickNew(bikeWorkOrder);
			var polishComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.Polish.PK).AvailableInventories[0];
			polishComponentInv.PickLineQuantity = 0m;

			// Finalise the WorkOrder
			bikeWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(bikeWorkOrder);
			Factory.Save();

			// Expect items in stock
			var stock = bikeWorkOrder.Receive.Inventory;
			AssertEquals("Number of items in stock is incorrect.", 4, stock.Count);

			var bike = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.Bike);
			AssertEquals("Number of bikes in stock is incorrect.", 0m, bike.WI_TotalUnits);

			var engine = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeEngine);
			AssertEquals("Number of engines in stock is incorrect.", 1m, engine.WI_TotalUnits);

			var wheel1 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeWheel && i.InDocketLine.WE_PartAttrib1 == "A");
			AssertEquals("Shoud have one wheel with attribute A.", 1m, wheel1.WI_TotalUnits);

			var wheel2 = stock.Cast<WhsInventoryView>().Single(i => i.SupplierPart == data.BOM.BikeWheel && i.InDocketLine.WE_PartAttrib1 == "B");
			AssertEquals("Shoud have one wheel with attribute B.", 1m, wheel2.WI_TotalUnits);
		}

		#endregion

		#region Setup

		WhsWorkOrder CreateAndSetupWorkOrderForFinalise(TestDataForInventory data)
		{
			// create the work order
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK);

			// setup the WorkOrder
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_ExternalReference = "ExtRef";
			workOrder.WD_ExternalReferenceSplit = 3;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreateProductBOM(data.Part1, data.Part2, 2, Core.Constants.PkgUnit.Unit); // each Part1 consists of 2xPart2
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 3m);

			// create a BOM staging area for Part1
			var productParams = line.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = workOrder.Warehouse.PK;
			productParams.W3_WL_StagingLocationBOM = workOrder.Warehouse.Rows.AddNew().Locations.AddNew().PK;

			// pick the WorkOrder
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(new WhsWorkOrder[] { workOrder });
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			return workOrder;
		}

		protected override WhsWorkOrder SetupForTestFinaliseDocket()
		{
			var workOrder = base.SetupForTestFinaliseDocket("FD1");

			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();

			// 1. shortfall check during pick will hit the DB during finalise.
			// 2. the supplier part validation does a DBOnly query to ensure the product is a BOM item.
			Factory.Save();

			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_ExternalReference = "WORKORD1";
			workOrder.ConsigneePK = workOrder.Client.PK;
			workOrder.ConsigneeAddressPK = workOrder.Client.MainAddress.PK;

			// pick the WorkOrder
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition", true, workOrder.IsAttachedToPickButNotFinalised);

			return workOrder;
		}

		#endregion

		#endregion

		#region BOA/BOD eDoc Printing Support

		public void TestOnDocketSubTypeChanged()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = CodeLists.WorkOrderType.Codes.Assemble;
			WhsWorkOrderLine workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			AssertEquals("Work Order has 1 line", 1, workOrder.Lines.Count);
			AssertEquals("SupplierPartDocManager DocManagerCode", "BOA", workOrder.Lines[0].SupplierPartDocManagerInfo.DocManagerCode);

			workOrder.WD_DocketSubType = CodeLists.WorkOrderType.Codes.Disassemble;
			AssertEquals("SupplierPartDocManager DocManagerCode", "BOD", workOrder.Lines[0].SupplierPartDocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Delete

		#region TestDeleteAlsoDeletesChildWorkOrders

		public void TestDeleteAlsoDeletesChildWorkOrders()
		{
			var child1 = GetNewBusinessObject();
			var child2 = GetNewBusinessObject();
			var finalizedChild = GetNewBusinessObject();
			var cancelledChild = GetNewBusinessObject();
			var docket = GetNewBusinessObject();
			child1.WD_WD_ParentDocket = docket.PK;
			child2.WD_WD_ParentDocket = docket.PK;
			finalizedChild.WD_WD_ParentDocket = docket.PK;
			cancelledChild.WD_WD_ParentDocket = docket.PK;

			var subChild1 = GetNewBusinessObject();
			var subChild2 = GetNewBusinessObject();
			subChild1.WD_WD_ParentDocket = child1.PK;
			subChild2.WD_WD_ParentDocket = child2.PK;

			finalizedChild.WD_FinalisedDate = ZDateTimeOffset.Today;
			cancelledChild.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			docket.Delete();

			AssertEquals(true, docket.IsDeleted);
			AssertEquals(true, child1.IsDeleted);
			AssertEquals(true, child2.IsDeleted);
			AssertEquals(true, subChild1.IsDeleted);
			AssertEquals(true, subChild2.IsDeleted);

			AssertEquals(false, finalizedChild.IsDeleted);
			AssertEquals(false, cancelledChild.IsDeleted);
		}

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
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", subPart1, 10m);
			Helper.CreateWhsReceiveWithInventory(org, whs, "R2", subPart2, 10m);

			var workOrder = Helper.CreateWhsWorkOrder(org, whs, "O1", type);
			var orderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);
			var orderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);
			var orderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);

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

		#endregion

		#endregion

		#region TestCannotCancelWorkOrderWithChildren

		public void TestCannotCancelWorkOrderWithChildren()
		{
			var child1 = GetNewBusinessObject();
			child1.WD_WD_ParentDocket = Docket.PK;
			child1.WD_DocketID = "Child 1";

			var child2 = GetNewBusinessObject();
			child2.WD_DocketID = "Child 2";
			child2.WD_WD_ParentDocket = Docket.PK;

			var finalizedChild = GetNewBusinessObject();
			finalizedChild.WD_WD_ParentDocket = Docket.PK;
			finalizedChild.WD_DocketID = "Finalised Child";
			finalizedChild.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var subChild1 = GetNewBusinessObject();
			subChild1.WD_WD_ParentDocket = child1.PK;
			subChild1.WD_DocketID = "SubChild 1";

			var subChild2 = GetNewBusinessObject();
			subChild2.WD_WD_ParentDocket = child2.PK;
			subChild2.WD_DocketID = "SubChild 2";

			Docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			string expectedMsg =
				"Cannot cancel this Work Order because the following related Work Orders exist:" + "\r\n" +
				"\r\n" +
				"	Child 1" + "\r\n" +
				"	Child 2" + "\r\n" +
				"	SubChild 1" + "\r\n" +
				"	SubChild 2" + "\r\n" +
				"\r\n" +
				"You must first cancel or delete the related Work Orders.";

			NotificationBuffer notify = (NotificationBuffer)Docket.NotificationManager.Peek;
			AssertEquals(expectedMsg, notify.Events[0].Message);
			AssertEquals(false, Docket.IsCancelled);
		}

		#endregion

		#region TestCancelReactivateDocket_WorkOrder

		public void TestCancelReactivateDocket_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, data.Part1.OP_StockKeepingUnit);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Assemble);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder1, data.Part1, 10m);
			var reservedPickLine1 = Helper.CreateReservePickLine(workOrderLine1.ChildComponentLines.Single(), receive2.Inventory[0], 4m);

			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO2", WorkOrderType.Codes.Disassemble);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, data.Part1, 10m);
			var reservedPickLine2 = Helper.CreateReservePickLine(workOrderLine2, receive1.Inventory[0], 6m);
			Factory.Save();
			AssertEquals("Precondition", false, workOrder1.IsCancelled);
			AssertEquals("Precondition", false, workOrder2.IsCancelled);

			workOrder1.CancelReactivateDocket();
			workOrder2.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Work Order should be cancelled.", true, workOrder1.IsCancelled);
			AssertEquals("Work Order should be cancelled.", true, workOrder2.IsCancelled);
			AssertEquals("Reserved pick lines should be deleted when work order is cancelled.", true, reservedPickLine1.IsDeleted);
			AssertEquals("Reserved pick lines should be deleted when work order is cancelled.", true, reservedPickLine2.IsDeleted);

			workOrder1.CancelReactivateDocket();
			workOrder2.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Work Order should be re-activated.", false, workOrder1.IsCancelled);
			AssertEquals("Work Order should be re-activated.", false, workOrder2.IsCancelled);
		}

		#endregion

		#region TestDocManagerInfo

		public override void TestDocManagerInfo()
		{
			DocManagerInfo docManagerInfo = Docket.DocManagerInfo;
			AssertType("Work Order DocManagerInfo", typeof(WhsWorkOrderDocManagerInfo), docManagerInfo);
			AssertEquals(Docket, docManagerInfo.BusinessEntity);
			AssertEquals("WWO", docManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestBOMExpandAndCollapseLines

		public void TestBOMExpandAndCollapseLines()
		{
			if (SupportsBomExpandAndCollapse)
			{
				TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory);

				var part11 = Helper.CreateProduct(data.Org1, "P11");
				var part12 = Helper.CreateProduct(data.Org1, "P12");
				Helper.CreateProductUnit(part12, Core.Constants.PkgUnit.Bag, 25);

				var part111 = Helper.CreateProduct(data.Org1, "P111");
				Helper.CreateProductUnit(part111, Core.Constants.PkgUnit.Pallet, 50);

				Helper.CreateProductBOM(data.Part1, part11, 2m, "UNT");
				Helper.CreateProductBOM(data.Part1, part12, 3m, "BAG");
				Helper.CreateProductBOM(part11, part111, 5m, "PLT");

				Docket.WD_OH_Client = data.Org1.PK;
				Docket.WD_WW_Whs = data.Whs1.PK;

				var line1 = Helper.CreateWhsWorkOrderLine(Docket, data.Part1, 10m);
				var line2 = Helper.CreateWhsWorkOrderLine(Docket, data.Part2, 10m);

				AssertEquals("Precondition", 2, Docket.Lines.Count);

				Docket.BOM.ExpandAllLines();
				AssertEquals(5, Docket.Lines.Count);

				Docket.BOM.ExpandAllLines();
				AssertEquals("The second expand failed", 5, Docket.Lines.Count);

				WhsWorkOrderLine line11 = null;
				WhsWorkOrderLine line12 = null;
				WhsWorkOrderLine line111 = null;

				foreach (WhsWorkOrderLine line in Docket.Lines)
				{
					switch (line.SupplierPart.OP_PartNum)
					{
						case "P1":
							Assert(ReferenceEquals(line, line1));
							break;
						case "P2":
							Assert(ReferenceEquals(line, line2));
							break;
						case "P11":
							line11 = line;
							break;
						case "P12":
							line12 = line;
							break;
						case "P111":
							line111 = line;
							break;
						default:
							Fail("Invalid Product: " + line.SupplierPart.OP_PartNum);
							break;
					}
				}

				AssertNotNull(line11);
				AssertEquals((ZShort)1, line1.WE_LineNo);
				AssertEquals(line1.WE_LineNo, line11.WE_BOMParentLineNo);
				AssertEquals("UNT", line11.WE_F3_NKPackType);
				AssertEquals("Expected 20x line11 after expansion of lines.", 20m, line11.WE_PackQuantity);

				AssertNotNull(line12);
				AssertEquals((ZShort)4, line12.WE_LineNo);
				AssertEquals(line1.WE_LineNo, line12.WE_BOMParentLineNo);
				AssertEquals("BAG", line12.WE_F3_NKPackType);
				AssertEquals("Expected 30x line12 after expansion of lines.", 30m, line12.WE_PackQuantity);

				AssertNotNull(line111);
				AssertEquals((ZShort)3, line111.WE_LineNo);
				AssertEquals(line11.WE_LineNo, line111.WE_BOMParentLineNo);
				AssertEquals("PLT", line111.WE_F3_NKPackType);
				AssertEquals("Expected 100x line111 after expansion of lines.", 100m, line111.WE_PackQuantity);

				Docket.BOM.CollapseAllLines();
				AssertEquals("Expected 2 Docket Lines after collapse.", 2, Docket.Lines.Count);

				Docket.BOM.ExpandLines(line2);
				AssertEquals("Docket Lines should stay unchanged after expanding Part2.", 2, Docket.Lines.Count);

				Docket.BOM.ExpandLines(line1, line2);
				AssertEquals("Docket Lines should expand to 5 when expanding Part1.", 5, Docket.Lines.Count);

				Docket.BOM.CollapseLines(line11);
				AssertEquals("Docket Lines should collapse to 4 when collapsing Part11.", 4, Docket.Lines.Count);

				Docket.BOM.ExpandLines(line1);
				AssertEquals("Docket Lines should re-expand to 5 when expanding Part1.", 5, Docket.Lines.Count);

				Docket.BOM.CollapseLines(line11);
				AssertEquals("Docket Lines should re-collapse to 4 when collapsing Part11.", 4, Docket.Lines.Count);

				Docket.BOM.CollapseLines(line2);
				AssertEquals("Docket Lines should stay unchanged after collapsing Part2.", 4, Docket.Lines.Count);

				Docket.BOM.CollapseLines(line1, line2);
				AssertEquals("Docket Lines should collapse to 2 when collapsing Part1.", 2, Docket.Lines.Count);
			}
			else
			{
				Assert("BOM expand/collapse is not supported.", true);
			}
		}

		protected virtual bool SupportsBomExpandAndCollapse
		{
			get { return true; }
		}

		#endregion

		#region TestBOMLevel

		public void TestBOMLevel()
		{
			AssertEquals("Precondition", 1, Docket.BOM.Level);
			WhsWorkOrder childDocket = GetNewBusinessObject();
			childDocket.WD_WD_ParentDocket = Docket.PK;
			AssertEquals("Level 2", 2, childDocket.BOM.Level);
		}

		#endregion

		#region TestAutoCreateWorkOrders_CreatesChildItems

		public void TestAutoCreateWorkOrders_CreatesChildItems()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// Polish for one bike, 2 engines, 4 wheels
			data.CreateProductInInventory("7 Polish", data.BOM.Polish, 7m);

			// stock for one bike
			data.CreateProductInInventory("1 Engine", data.BOM.BikeEngine, 1m);
			data.CreateProductInInventory("2 Wheels", data.BOM.BikeWheel, 2m);

			// stock for 2 engines, includes building of 8 pistons
			data.CreateProductInInventory("2 Engine Blocks", data.BOM.EngineBlock, 2m);
			data.CreateProductInInventory("1 Engine Piston", data.BOM.EnginePiston, 1m);
			data.CreateProductInInventory("8 Piston Cranks", data.BOM.PistonCrank, 8m);
			data.CreateProductInInventory("8 Piston Heads", data.BOM.PistonHead, 8m);
			data.CreateProductInInventory("8 Piston Rings", data.BOM.PistonRing, 8m);

			// stock for 4 wheels
			data.CreateProductInInventory("4 Rims", data.BOM.WheelRim, 4m);
			data.CreateProductInInventory("4 Tyres", data.BOM.WheelTyre, 4m);

			Factory.Save();

			// create a work order for 3 bikes
			WhsWorkOrder bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			bikeWorkOrder.WD_ExternalReference = "abc";
			Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 3m); // shortfall of 2.

			// pick the order, should have 2 bikes missing.
			WhsPick pick = Helper.CreatePickNew(bikeWorkOrder);
			AssertEquals("Precondition - Pick failed.", true, bikeWorkOrder.IsAttachedToPickButNotFinalised);

			bikeWorkOrder.BOM.AutoCreateWorkOrders(Notify);

			// level 1 work orders (engine WO & wheels WO)
			WhsWorkOrder[] engineAndWheelsworkOrders = Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, bikeWorkOrder.PK));
			AssertEquals("Should have one work Order for the Engine, and one for the Wheels.", 2, engineAndWheelsworkOrders.Length);

			WhsWorkOrder engineWorkOrder = FindWorkOrder(data.BOM.BikeEngine, engineAndWheelsworkOrders);
			AssertEquals("Should have created one line for the engine.", 1, engineWorkOrder.Lines.Count);
			AssertEquals("Should be building 2 engines (1 per bike).", 2m, engineWorkOrder.Lines[0].WE_TransactionQuantity);
			AssertEquals("Should have copied WD_ExternalReference.", "abc", engineWorkOrder.WD_ExternalReference);
			AssertEquals("Should have incremented WD_ExternalReferenceSplit.", new ZByte(1), engineWorkOrder.WD_ExternalReferenceSplit);

			WhsWorkOrder wheelsWorkOrder = FindWorkOrder(data.BOM.BikeWheel, engineAndWheelsworkOrders);
			AssertEquals("Should have created one line for the wheels.", 1, wheelsWorkOrder.Lines.Count);
			AssertEquals("Should be building 4 wheels (2 per bike).", 4m, wheelsWorkOrder.Lines[0].WE_TransactionQuantity);
			AssertEquals("Should have copied WD_ExternalReference.", "abc", wheelsWorkOrder.WD_ExternalReference);
			AssertEquals("Should have incremented WD_ExternalReferenceSplit.", new ZByte(3), wheelsWorkOrder.WD_ExternalReferenceSplit);

			// level 2 work orders (piston WO)
			WhsWorkOrder[] pistonsWorkOrders = Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, engineWorkOrder.PK));
			AssertEquals("Should have one work Order for the pistons.", 1, pistonsWorkOrders.Length);

			WhsWorkOrder pistonsWorkOrder = pistonsWorkOrders.Single();
			AssertEquals("Should have created one line for the pistons.", 1, pistonsWorkOrder.Lines.Count);
			AssertEquals("Should be building 7 pistons (1 in stock, 4 per engine).", 7m, pistonsWorkOrder.Lines[0].WE_TransactionQuantity);
			AssertEquals("Should have copied WD_ExternalReference.", "abc", pistonsWorkOrder.WD_ExternalReference);
			AssertEquals("Should have incremented WD_ExternalReferenceSplit.", new ZByte(2), pistonsWorkOrder.WD_ExternalReferenceSplit);

			// test that saving does not fail, and confirm that the results are available in the RelatedJobs
			Factory.Save();
			AssertEquals(3, bikeWorkOrder.RelatedJobs.Count);
			AssertCollectionContains(engineWorkOrder, bikeWorkOrder.RelatedJobs);
			AssertCollectionContains(pistonsWorkOrder, bikeWorkOrder.RelatedJobs);
			AssertCollectionContains(wheelsWorkOrder, bikeWorkOrder.RelatedJobs);
		}

		#endregion

		#region TestTemplateCopy

		protected override void AssertTemplateCopy(WhsWorkOrder source, WhsWorkOrder copy, bool shouldCopyLines)
		{
			base.AssertTemplateCopy(source, copy, shouldCopyLines);

			AssertEquals("Total cubic on source should not be changed by copy", 0.3m, source.WD_TotalCubic);
			AssertEquals("Total weight on source should not be changed by copy", 30m, source.WD_TotalWeight);
			AssertEquals(0.3m, copy.WD_TotalCubic);
			AssertEquals(30m, copy.WD_TotalWeight);

			if (shouldCopyLines)
			{
				AssertEquals(2, copy.Lines.Count);

				// .Single() throws an exception if more/less than one element is found thus no Assertions needed.
				var bikeLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.SupplierPart.OP_Desc == "Motorbike"); // the original line
				var bikeWheelLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.BikeWheel.PK && line.WE_WE_ParentDocketLine == bikeLine.PK);
				var bikeEngineLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.BikeEngine.PK && line.WE_WE_ParentDocketLine == bikeLine.PK);
				var bikePolishLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeLine.PK);
				var wheelTyreLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.WheelTyre.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
				var wheelRimLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.WheelRim.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
				var wheelPolishLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
				var engineBlockLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.EngineBlock.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
				var enginePistonLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.EnginePiston.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
				var engineOilLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.EngineOil.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
				var enginePolishLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
				var pistonHeadLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.PistonHead.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
				var pistonCrankLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.PistonCrank.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
				var pistonRingLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.PistonRing.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);

				// same as above, butsearch for the wheels...
				var wheelLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.SupplierPart.OP_Desc == "Wheel" && ((WhsWorkOrderLine)line).BOM.IsTopLevelProduct); // the original line
				var tyreLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.WheelTyre.PK && line.WE_WE_ParentDocketLine == wheelLine.PK);
				var rimLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.WheelRim.PK && line.WE_WE_ParentDocketLine == wheelLine.PK);
				var polishLine = (WhsWorkOrderLine)copy.AllLines.Single(line => line.WE_OP == Data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == wheelLine.PK);
			}
			else
			{
				AssertEquals(0, copy.Lines.Count);
				AssertEquals(0, copy.AllLines.Count);
			}
		}

		protected override WhsWorkOrder GetNewDocketForTemplateCopy()
		{
			Data.CreateBOMProducts();

			var workOrder = Helper.CreateWhsWorkOrder(Data.Org1, Data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, Data.BOM.Bike, 3);
			Helper.CreateWhsWorkOrderLine(workOrder, Data.BOM.BikeWheel, 1);
			Helper.CreateWhsWorkOrderLine(workOrder, Data.Part1, 1); // this should not be cloned as it is an invalid (non-BOM) product

			return workOrder;
		}

		#endregion

		#region TestAddNewFromDocketLine

		[TestDate(2009, 5, 6)]
		public void TestAddNewFromWorkOrderLine()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			data.CreateProductInInventory("Rims", data.BOM.WheelRim, 3);
			data.CreateProductInInventory("Tyres", data.BOM.WheelTyre, 3);
			Factory.Save();

			// create a work order with 2 lines
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 3m);
			var lineWithArea = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 5m);
			line.WE_PartAttrib1 = "Attrib1";
			line.WE_PartAttrib2 = "Attrib2";
			line.WE_PartAttrib3 = "Attrib3";
			line.WE_PackingDate = ZDate.Today;
			line.WE_ExpiryDate = ZDate.Today.AddDays(7);

			// setup a mandatory attribute on one of the products
			Helper.SetClientAttributeType(workOrder.Client, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(workOrder.Client, line.SupplierPart, AttributeNumber.One, true);

			// setup a BOM staging area for one of the products
			var area = data.Whs1.Areas[0];
			var productParams = lineWithArea.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			var bomStagingLocation = data.Whs1.Rows.AddNew().Locations.AddNew();
			productParams.W3_WL_StagingLocationBOM = bomStagingLocation.PK;

			// pick the job
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// test line whose product has no BOM Staging Area
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var inventory = workOrder.Receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_OP == line.WE_OP);
			AssertEquals(line.WE_OP, inventory.WI_OP);
			AssertEquals(3m, inventory.WI_TotalUnits);
			AssertEquals(3m, inventory.WI_InDocketLineUnits);
			AssertEquals(new ZDateTimeOffset(2009, 05, 06, 10, 0, 0, TimeSpan.FromHours(10)), inventory.WI_ArrivalDate);
			AssertEquals(CodeLists.InventoryStatus.Codes.Putaway, inventory.WI_InventoryStatus);
			AssertEquals(workOrder.WD_OH_Client, inventory.WI_OH_Client);
			AssertEquals(workOrder.Warehouse.DefaultLocation.PK, inventory.WI_WL);
			AssertEquals(line.WE_PartAttrib1, inventory.WI_PartAttrib1);
			AssertEquals(line.WE_PartAttrib2, inventory.WI_PartAttrib2);
			AssertEquals(line.WE_PartAttrib3, inventory.WI_PartAttrib3);
			AssertEquals(line.WE_PackingDate, inventory.WI_PackingDate);
			AssertEquals(line.WE_ExpiryDate, inventory.WI_ExpiryDate);

			// test line whose product has a BOM Staging Location
			var destInventoryWithBOMStagingLocation = workOrder.Receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_OP == lineWithArea.WE_OP);
			AssertEquals(bomStagingLocation.PK, destInventoryWithBOMStagingLocation.WI_WL);
		}

		public void TestAddNewFromWorkOrderLine_SerialNumber()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // 10 engines + 20 wheels
			data.CreateProductInInventory("Rims", data.BOM.WheelRim, 3);
			data.CreateProductInInventory("Tyres", data.BOM.WheelTyre, 3);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 1m);
			line.WE_SerialNumber = "SN1";

			Helper.SetClientAttributeType(workOrder.Client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(workOrder.Client, line.SupplierPart, AttributeNumber.Serial, true);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			var inventory = workOrder.Receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_OP == line.WE_OP);
			AssertEquals(line.WE_OP, inventory.WI_OP);
			AssertEquals(1m, inventory.WI_TotalUnits);
			AssertEquals(1m, inventory.WI_InDocketLineUnits);
			AssertEquals(CodeLists.InventoryStatus.Codes.Putaway, inventory.WI_InventoryStatus);
			AssertEquals(workOrder.WD_OH_Client, inventory.WI_OH_Client);
			AssertEquals(workOrder.Warehouse.DefaultLocation.PK, inventory.WI_WL);
			AssertEquals("SN1", inventory.WI_SerialNumber);
		}

		#endregion

		#region TestRunPreSaveValidationCore_SynchronisesOffset

		public override void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			Factory.Save();

			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			workOrderLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			workOrder.RunPreSaveValidation();

			Assert("Docket should not be in error", !workOrder.HasErrors);

			var expectedOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				workOrderLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		#endregion

		#region TestCheckThatPickIsFinalisedIfWorkOrderIsFinalisedDuringSave

		public void TestCheckThatPickIsFinalisedIfWorkOrderIsFinalisedDuringSave()
		{
			var workOrder = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			workOrder.WD_WP = pick.PK;
			Env.Security.WhsReleaseFinalise.IsAllowed = false;
			workOrder.WD_FinalisedDate = ZDateTimeOffset.Today;
			workOrder.WD_DocketStatus = DocketStatus.Codes.Finalised;

			AssertEquals("Precondition", false, workOrder.HasErrors);
			workOrder.RunPreSaveValidation();

			AssertEquals(true, workOrder.IsFinalised);
			AssertEquals(false, workOrder.Pick.IsFinalised);
			AssertHasRowError(workOrder, @"Job was finalized but Pick was not finalized.");
		}

		#endregion

		#region IWhsLogEventParent

		protected override string ExpectedEventReferenceParameterType => Constants.EventReferenceParameterTypes.WorkOrder;

		#endregion

		#region TestGetDisassemblyQuantity

		public void TestGetDisassemblyQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", mainProduct, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			AssertEquals(15m, workOrder.GetDisassemblyQuantity());
		}

		public void TestGetDisassemblyQuantity_AssembleWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>("GetDisassemblyQuantity cannot be invoked for assembly work orders.", () => workOrder.GetDisassemblyQuantity());
		}

		public void TestGetDisassemblyQuantity_WorksWithOrWithoutLink()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct("bike", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var bom2 = Helper.CreateProductBOM(data.Part2, bike, 1m, "UNT");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bike, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "Wo01", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has two links.", 2, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "Wo02", data.Part2, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertIsFinalisedPrecondition(disassembleWorkOrder);

			AssertEquals("Should get values from link.", 15m, disassembleWorkOrder.GetDisassemblyQuantity());

			((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.ForEach(l => l.Delete());
			Factory.Save();
			AssertEquals(0, Factory.Load<WhsBOMInventoryPivot>(new ZQuery()).Length);
			AssertEquals("Should be able to caculate without link.", 15m, disassembleWorkOrder.GetDisassemblyQuantity());
		}

		#endregion

		#region TestGetPlannedDisassemblyQuantity

		public void TestGetPlannedDisassemblyQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", mainProduct, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			Factory.Save();

			AssertEquals("Precondition", 0m, workOrder.GetDisassemblyQuantity());
			AssertEquals(15m, workOrder.GetPlannedDisassemblyQuantity());

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition", 15m, workOrder.GetDisassemblyQuantity());
			AssertEquals(15m, workOrder.GetPlannedDisassemblyQuantity());

			pick.GetAllPickLines().First().WZ_Units = 3m;
			AssertEquals("Precondition", 9m, workOrder.GetDisassemblyQuantity());
			AssertEquals(15m, workOrder.GetPlannedDisassemblyQuantity());
		}

		public void TestGetPlannedDisassemblyQuantity_AssembleWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>("GetPlannedDisassemblyQuantity cannot be invoked for assembly work orders.", () => workOrder.GetDisassemblyQuantity());
		}

		#endregion

		#region TestUpdateWP_CriticalChangesVersionID

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_IsInwardsProcessingJob()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_IsInwardsProcessingJob = true, true);
		}

		#endregion

		#region TestDocketUpdatedByDataRefresh

		protected override void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct("bike", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, bike, 1m, "UNT");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bike, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "Wo01", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsWorkOrder>(workOrder.PK);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			workOrderInNewFactory.WD_ExternalReference = "NEWWO1";
			AssertEquals(false, workOrderInNewFactory.IsFinalised);
			newFactory.Save();

			AssertEquals("Work Order is still finalised after data refresh.", true, workOrder.IsFinalised);
			AssertEquals("External reference is updated after data refresh.", "NEWWO1", workOrder.WD_ExternalReference);
			Helper.AssertZCannotSaveExceptionThrown("The Work Order has been updated by another job. Please reload the Work Order.", Factory.Save);
		}

		protected override void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct("bike", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, bike, 1m, "UNT");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bike, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "Wo01", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsWorkOrder>(workOrder.PK);

			var newCriticalChangesVersionID = ZGuid.NewZGuid();
			AssertNotEquals(workOrder.WD_CriticalChangesVersionID, newCriticalChangesVersionID);

			workOrderInNewFactory.WD_CriticalChangesVersionID = newCriticalChangesVersionID;
			newFactory.Save();

			AssertEquals("WD_CriticalChangesVersionID is updated after data refresh.", newCriticalChangesVersionID, workOrder.WD_CriticalChangesVersionID);

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var notify = (NotificationBuffer)workOrder.NotificationManager.Peek;
			AssertEquals("Work Order should have the 'need to reload' notification", true, notify.ContainsNotificationType(WhsErrorTypes.CannotFinaliseWithoutReload));
		}

		#endregion

		#region Implementation

		protected TestDataForBOM Data
		{
			get { return data ?? (data = new TestDataForBOM(Factory)); }
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.WhsWorkOrder);

		protected override DataContextType? ExpectedDataContextType => DataContextType.WarehouseWorkOrder;

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsWorkOrderValidation);
		}

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsWorkOrderLookups);
		}
		protected override ControllerID ExpectedControllerId
		{
			get { return ControllerIDs.WhsWorkOrder; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Work Order"; }
		}

		protected override WhsWorkOrder GetDocketForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
		}

		TestDataForBOM data;

		#endregion
	}
}
