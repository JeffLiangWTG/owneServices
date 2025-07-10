using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderLine))]
	class WhsWorkOrderLineTest : WhsComponentOrderLineTest<WhsWorkOrderLine, WhsWorkOrder>
	{
		#region Validation

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsWorkOrderLineValidation);
		}

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsWorkOrderLineLookups);
		}

		#endregion

		#region Related Business Objects

		#region TestParentLine

		public void TestParentLine()
		{
			DocketLine.WE_LineNo = 2;
			WhsWorkOrderLine childLine = GetNewBusinessObject(Docket);
			AssertNull(childLine.ParentLine);

			childLine.WE_WE_ParentDocketLine = DocketLine.PK;
			AssertEquals(DocketLine, childLine.ParentLine);
		}

		#endregion

		#region TestPickLines

		protected override WhsPickableDocket GetPickableDocket_ForPickLinesTest(TestDataForBOM data)
		{
			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			Helper.CreateWhsPickableDocketLine(docket, data.BOM.Bike, 2m);

			return docket;
		}

		#endregion

		#endregion

		#region Properties

		#region TestCanGenerateChildWorkOrder

		protected override void TestCanGenerateChildWorkOrderCore()
		{
			var orderLine = Factory.New<WhsWorkOrderLine>();
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			var part = Factory.New<OrgSupplierPart>();
			orderLine.WE_OP = part.PK;
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			part.BillOfMaterials.AddNew();
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			var parentLine = Factory.New<WhsWorkOrderLine>();
			orderLine.WE_WE_ParentDocketLine = parentLine.PK;
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			var otherPart = Factory.New<OrgSupplierPart>();
			parentLine.WE_OP = otherPart.PK;
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			otherPart.BillOfMaterials.AddNew();
			AssertEquals(true, orderLine.CanGenerateChildWorkOrder);
		}

		#endregion

		#region TestWE_WD

		#region TestWE_WD_UpdatingTotals

		protected override void TestWE_WD_UpdatingTotals(ZString type)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("A");
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			var level3Part1 = Helper.CreateProduct(org, "Level3Part1");
			var level3Part2 = Helper.CreateProduct(org, "Level3Part2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");
			Helper.SetProductWeightAndVolume(level3Part1, 6, "KG", 0.3, "M3");
			Helper.SetProductWeightAndVolume(level3Part2, 5, "KG", 0.2, "M3");
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(subPart1, level3Part1, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(subPart1, level3Part2, 1m, Constants.PkgUnit.Unit);

			var workOrder1 = GetNewWhsDocket(org, whs);
			workOrder1.WD_DocketSubType = type;
			var workOrder2 = GetNewWhsDocket(org, whs);
			workOrder2.WD_DocketSubType = type;
			var workOrderLine1 = workOrder1.Lines.AddNew();
			workOrderLine1.WE_OP = mainPart.PK;
			workOrderLine1.WE_TransactionQuantity = 1m;

			var workOrderLine2 = workOrder2.Lines.AddNew();
			workOrderLine2.WE_OP = mainPart.PK;
			workOrderLine2.WE_TransactionQuantity = 1m;

			var migratingDocketLine = workOrder1.Lines.AddNew();
			migratingDocketLine.WE_OP = mainPart.PK;
			migratingDocketLine.WE_TransactionQuantity = 1m;

			workOrder1.WD_TotalWeight = 70m;
			workOrder1.WD_TotalCubic = 1.5m;
			workOrder2.WD_TotalWeight = 60m;
			workOrder2.WD_TotalCubic = 1.3m;

			AssertEquals("Precondition - ensure Total Weight is correct", 70m, workOrder1.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Cubic is correct", 1.5m, workOrder1.WD_TotalCubic);
			AssertEquals("Precondition - ensure Total Line Units is correct", 2m, workOrder1.WD_TotalUnitsFromLines);

			AssertEquals("Precondition - ensure Total Weight is correct", 60m, workOrder2.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Cubic is correct", 1.3m, workOrder2.WD_TotalCubic);
			AssertEquals("Precondition - ensure Total Line Units is correct", 1m, workOrder2.WD_TotalUnitsFromLines);

			migratingDocketLine.WE_WD = workOrder2.PK;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Total Weight should be updated, but not recalculated.", 50m, workOrder1.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated, but not recalculated.", 0.4m, workOrder1.WD_TotalCubic);
				AssertEquals("Total Line Units should be updated.", 1m, workOrder1.WD_TotalUnitsFromLines);

				AssertEquals("Total Weight should be updated.", 80m, workOrder2.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated.", 2.4m, workOrder2.WD_TotalCubic);
				AssertEquals("Total Line Units should be updated.", 2m, workOrder2.WD_TotalUnitsFromLines);
			}
			else
			{
				AssertEquals("Total Weight should be updated, but not recalculated.", 40m, workOrder1.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated, but not recalculated.", 0.5m, workOrder1.WD_TotalCubic);
				AssertEquals("Total Line Units should be updated.", 1m, workOrder1.WD_TotalUnitsFromLines);

				AssertEquals("Total Weight should be updated.", 90m, workOrder2.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated.", 2.3m, workOrder2.WD_TotalCubic);
				AssertEquals("Total Line Units should be updated.", 2m, workOrder2.WD_TotalUnitsFromLines);
			}
		}

		#endregion

		public void TestWE_WD_VirtualWhs_ChangesBillOfMaterials()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Whs1
			data.Whs1.WW_IsVirtualWarehouse = false;
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C11");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C12");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			bomPart1.OE_ExcludeForVirtualWarehouse = true;

			// Whs2
			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);
			whs2.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1");
			var line = Helper.CreateWhsWorkOrderLine(workOrder1, bomProduct, 1m);
			AssertEquals("Should have correct BOMs count", 2, line.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have all defined BOMs", new[] { bomPart1, bomPart2 }, line.BillOfMaterials.ToArray());

			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1.PK, whs2.PK, "W2");
			line.WE_WD = workOrder2.PK;
			AssertEquals("Should have correct BOMs count", 1, line.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have only virtual BOMs", new[] { bomPart2 }, line.BillOfMaterials.ToArray());
		}

		#endregion

		#region TestWE_AllocationKeyInfo

		protected override void TestWE_AllocationKeyInfoCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.BOM.Bike, 1m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrder.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var workOrder2 = Factory.New<WhsWorkOrder>();
			workOrder2.WD_OH_Client = data.Org1.PK;
			workOrder2.WD_WW_Whs = data.Whs1.PK;
			workOrder2.WD_RequiredDate = DateTime.Now;

			var line = Helper.CreateWhsPickableDocketLine(workOrder2, data.BOM.Bike, 1m);

			workOrder2.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			AssertEquals("Should not be readonly.", false, line.WE_AllocationKeyInfo.ReadOnly);

			var pick = Helper.CreatePickNew(workOrder2);
			AssertEquals("Should be readonly.", true, line.WE_AllocationKeyInfo.ReadOnly);

			pick.RemoveOrders(new[] { workOrder2 });
			AssertEquals("Should be readonly.", false, line.WE_AllocationKeyInfo.ReadOnly);
		}

		#endregion

		#region TestWE_BOMParentLineNo

		public void TestWE_BOMParentLineNo()
		{
			WhsWorkOrderLine parentLine = GetNewBusinessObject(Docket);
			WhsWorkOrderLine childLine = GetNewBusinessObject(Docket);
			parentLine.WE_LineNo = 7;
			AssertEquals((ZShort)0, childLine.WE_BOMParentLineNo);

			childLine.WE_WE_ParentDocketLine = parentLine.PK;
			AssertEquals((ZShort)7, childLine.WE_BOMParentLineNo);
		}

		public void TestWE_BOMParentLineNoInfo_Name()
		{
			AssertEquals(WhsWorkOrderLine.Schema.WE_BOMParentLineNo, DocketLine.WE_BOMParentLineNoInfo.Name);
		}

		public void TestWE_BOMParentLineNoInfo_ReadOnly()
		{
			TestReadOnly(d => d.WE_BOMParentLineNoInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_Level

		public void TestWE_Level()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// order 10 bikes
			DocketLine.WE_OP = data.BOM.Bike.PK;
			DocketLine.WE_TransactionQuantity = 10;

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(Docket);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(Docket);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(Docket);
			var wheelRimLine = data.BOM.Lines.WheelRim(Docket);
			var engineBlockLine = data.BOM.Lines.EngineBlock(Docket);
			var enginePistonLine = data.BOM.Lines.EnginePiston(Docket);
			var pistonHeadLine = data.BOM.Lines.PistonHead(Docket);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(Docket);
			var pistonRingLine = data.BOM.Lines.PistonRing(Docket);

			// ensure the levels are correct
			AssertEquals(0m, DocketLine.WE_Level);
			AssertEquals(1m, bikeWheelLine.WE_Level);
			AssertEquals(1m, bikeEngineLine.WE_Level);
			AssertEquals(2m, wheelRimLine.WE_Level);
			AssertEquals(2m, wheelTyreLine.WE_Level);
			AssertEquals(2m, engineBlockLine.WE_Level);
			AssertEquals(2m, enginePistonLine.WE_Level);
			AssertEquals(3m, pistonHeadLine.WE_Level);
			AssertEquals(3m, pistonCrankLine.WE_Level);
			AssertEquals(3m, pistonRingLine.WE_Level);
		}

		#endregion

		#region TestPickLineQuantity

		protected override void TestPickLineQuantityCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // enough to build 10 bikes
			Factory.Save(); // for shortfall calc which can stop the pick

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7);
			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Picking failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeLine = data.BOM.Lines.Bike(workOrder);
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// ensure the pick qtys are correct
			AssertEquals(0m, bikeLine.PickLineQuantity);
			AssertEquals(14m, bikeWheelLine.PickLineQuantity);
			AssertEquals(7m, bikeEngineLine.PickLineQuantity);
			AssertEquals(0m, wheelRimLine.PickLineQuantity);
			AssertEquals(0m, wheelTyreLine.PickLineQuantity);
			AssertEquals(0m, engineBlockLine.PickLineQuantity);
			AssertEquals(0m, enginePistonLine.PickLineQuantity);
			AssertEquals(0m, pistonHeadLine.PickLineQuantity);
			AssertEquals(0m, pistonCrankLine.PickLineQuantity);
			AssertEquals(0m, pistonRingLine.PickLineQuantity);
		}

		#endregion

		#region TestPickedPickLineQuantity

		protected override void TestPickedPickLineQuantityCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // enough to build 10 bikes
			Factory.Save(); // for shortfall calc which can stop the pick

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7);
			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Picking failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeLine = data.BOM.Lines.Bike(workOrder);
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// ensure the pick qtys are correct
			AssertEquals(0m, bikeLine.PickedPickLineQuantity);
			AssertEquals(0m, bikeWheelLine.PickedPickLineQuantity);
			AssertEquals(0m, bikeEngineLine.PickedPickLineQuantity);
			AssertEquals(0m, wheelRimLine.PickedPickLineQuantity);
			AssertEquals(0m, wheelTyreLine.PickedPickLineQuantity);
			AssertEquals(0m, engineBlockLine.PickedPickLineQuantity);
			AssertEquals(0m, enginePistonLine.PickedPickLineQuantity);
			AssertEquals(0m, pistonHeadLine.PickedPickLineQuantity);
			AssertEquals(0m, pistonCrankLine.PickedPickLineQuantity);
			AssertEquals(0m, pistonRingLine.PickedPickLineQuantity);

			bikeWheelLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			bikeEngineLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertEquals(0m, bikeLine.PickedPickLineQuantity);
			AssertEquals(14m, bikeWheelLine.PickedPickLineQuantity);
			AssertEquals(7m, bikeEngineLine.PickedPickLineQuantity);
			AssertEquals(0m, wheelRimLine.PickedPickLineQuantity);
			AssertEquals(0m, wheelTyreLine.PickedPickLineQuantity);
			AssertEquals(0m, engineBlockLine.PickedPickLineQuantity);
			AssertEquals(0m, enginePistonLine.PickedPickLineQuantity);
			AssertEquals(0m, pistonHeadLine.PickedPickLineQuantity);
			AssertEquals(0m, pistonCrankLine.PickedPickLineQuantity);
			AssertEquals(0m, pistonRingLine.PickedPickLineQuantity);
		}

		#endregion

		#region TestQuantityNotPicked

		protected override void TestQuantityNotPickedCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // enough to build 10 bikes
			Factory.Save(); // for shortfall calc which can stop the pick

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7);
			WhsPick pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Picking failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeLine = data.BOM.Lines.Bike(workOrder);
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// ensure the pick qtys are correct
			AssertEquals(7m, bikeLine.QuantityNotPicked);
			AssertEquals(0m, bikeWheelLine.QuantityNotPicked);
			AssertEquals(0m, bikeEngineLine.QuantityNotPicked);
			AssertEquals(14m, wheelRimLine.QuantityNotPicked);
			AssertEquals(14m, wheelTyreLine.QuantityNotPicked);
			AssertEquals(7m, engineBlockLine.QuantityNotPicked);
			AssertEquals(28m, enginePistonLine.QuantityNotPicked);
			AssertEquals(28m, pistonHeadLine.QuantityNotPicked);
			AssertEquals(28m, pistonCrankLine.QuantityNotPicked);
			AssertEquals(28m, pistonRingLine.QuantityNotPicked);
		}

		#endregion

		#region TestQuantityAssembled

		public void TestQuantityAssembled()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory(); // enough to build 10 bikes
			Factory.Save(); // for shortfall calc which can stop the pick

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7);
			WhsPick pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Picking failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeLine = data.BOM.Lines.Bike(workOrder);
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// ensure the assembled qtys are correct
			AssertEquals(7m, bikeLine.QuantityAssembled);
			AssertEquals(0m, bikeWheelLine.QuantityAssembled);
			AssertEquals(0m, bikeEngineLine.QuantityAssembled);
			AssertEquals(0m, wheelRimLine.QuantityAssembled);
			AssertEquals(0m, wheelTyreLine.QuantityAssembled);
			AssertEquals(0m, engineBlockLine.QuantityAssembled);
			AssertEquals(0m, enginePistonLine.QuantityAssembled);
			AssertEquals(0m, pistonHeadLine.QuantityAssembled);
			AssertEquals(0m, pistonCrankLine.QuantityAssembled);
			AssertEquals(0m, pistonRingLine.QuantityAssembled);
		}

		public void TestQuantityAssembledIsZero_WhenComponentIsShortedToZero()
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
			AssertEquals("Quantity assembled is incorrect.", 0m, bikeWorkOrderLine.QuantityAssembled);
		}

		public void TestQuantityAssembledIsZero_WhenComponentIsShortedBelowWhatIsNeededButNotZero()
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

			// Short the wheels component to 1
			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == data.BOM.BikeWheel.PK).AvailableInventories[0];
			wheelComponentInv.PickLineQuantity = 1m;
			AssertEquals("Quantity assembled is incorrect.", 0m, bikeWorkOrderLine.QuantityAssembled);
		}
		#endregion

		#region TestSumOfUnitsMet

		protected override void TestSumOfUnitsMetCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive enough stock to build 5 bikes.
			data.BOM.CreateProductInInventory("engines", data.BOM.BikeEngine, 4);
			data.BOM.CreateProductInInventory("wheels", data.BOM.BikeWheel, 20);
			data.BOM.CreateProductInInventory("polish", data.BOM.Polish, 10);
			//
			data.BOM.CreateProductInInventory("blocks", data.BOM.EngineBlock, 2);
			data.BOM.CreateProductInInventory("pistons", data.BOM.EnginePiston, 8); // 6 per engine, 8 == enough for only one engine.
			data.BOM.CreateProductInInventory("engine polish", data.BOM.Polish, 10); // Enough for 10 engines
																					 //
			data.BOM.CreateProductInInventory("rims", data.BOM.BikeWheel, 10);
			data.BOM.CreateProductInInventory("tyres", data.BOM.BikeWheel, 10); // these should not be needed nor picked.
			Factory.Save();

			// order 10 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			// pick the WorkOrder
			WhsPick pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);
			var bikePolishLine = data.BOM.Lines.BikePolish(workOrder);
			var enginePolishLine = data.BOM.Lines.EnginePolish(workOrder);

			AssertEquals("Enough components exist to assemble 4/10 bikes.", 4m, bikeLine.SumOfUnitsMet);
			AssertEquals("Enough components exist to supply 4/10 engines. Building the 5th engine will be on another workOrder, thus shouldn't be included here.", 4m, bikeEngineLine.SumOfUnitsMet);
			AssertEquals("Should only show 8/20 wheels because we can only build 4 bikes.", 8m, bikeWheelLine.SumOfUnitsMet);
			AssertEquals("No rims were needed.", 0m, wheelRimLine.SumOfUnitsMet);
			AssertEquals("No tyres were needed.", 0m, wheelTyreLine.SumOfUnitsMet);
		}

		#endregion

		#region TestPickLinesForRelease

		public void TestPickLinesForRelease()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			AssertEquals(typeof(WhsPickLineCollection), line.PickLinesForRelease.GetType());
		}

		#endregion

		#region TestSumOfUnitsMet_WithDisassembly

		public void TestSumOfUnitsMet_WithDisassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // 100 full bikes
			Factory.Save();

			// order disassembly of 10 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			// pick the WorkOrder
			WhsPick pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			AssertEquals("Enough components exist to disassemble all 10 bikes.", 10m, bikeLine.SumOfUnitsMet);
			AssertEquals(0m, bikeWheelLine.SumOfUnitsMet);
			AssertEquals(0m, bikeEngineLine.SumOfUnitsMet);
			AssertEquals(0m, wheelTyreLine.SumOfUnitsMet);
			AssertEquals(0m, wheelRimLine.SumOfUnitsMet);
			AssertEquals(0m, engineBlockLine.SumOfUnitsMet);
			AssertEquals(0m, enginePistonLine.SumOfUnitsMet);
			AssertEquals(0m, pistonHeadLine.SumOfUnitsMet);
			AssertEquals(0m, pistonCrankLine.SumOfUnitsMet);
			AssertEquals(0m, pistonRingLine.SumOfUnitsMet);
		}

		#endregion

		#region TestReduceOverpickedStock

		public void TestReduceOverpickedStockForChangedProductDefinitions()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct = Helper.CreateProduct(data.Org1, "C1");
			var bomPart = Helper.CreateProductBOM(bomProduct, componentProduct, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 1m, true, true);
			Factory.Save();

			// create the work order and change pack type
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", Notify, WorkOrderType.Codes.Assemble);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			Factory.Save();

			Helper.CreatePickNew(workOrder);
			AssertEquals(true, workOrder.IsAttachedToPickButNotFinalised);
			bomPart.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			AssertNoExceptionThrown("No exceptions should be thrown if product components were changed.", () => line.ReduceOverpickedStock());
		}

		#endregion

		#region TestSumOfUnitsMet_WhenProductDefinitionChangeForAllComponentProducts

		public void TestSumOfUnitsMet_WhenProductDefinitionChangeForAllComponentProducts()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct = Helper.CreateProduct(data.Org1, "C1");
			var bomPart = Helper.CreateProductBOM(bomProduct, componentProduct, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 1m, true, true);
			Factory.Save();

			// create the work order and change pack type
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, WorkOrderType.Codes.Assemble);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			bomPart.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			AssertNoExceptionThrown("No exception should be thrown eventhough component deifinitions have been changed.", () => { var units = line.SumOfUnitsMet; });
		}

		#endregion

		#region TestSumOfUnitsMet_WhenProductDefinitionChangeForFewComponentProducts

		public void TestSumOfUnitsMet_WhenProductDefinitionChangeForFewComponentProducts()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create work order and change pack type of a one component product
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, WorkOrderType.Codes.Assemble);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			AssertNoExceptionThrown("No exception should be thrown eventhough few component deifinitions have been changed.", () => { var units = line.SumOfUnitsMet; });
		}

		#endregion

		#region TestSumOfUnitsMet_WhenProductDefinitionChangeAfterGeneratingPick

		public void TestSumOfUnitsMet_WhenProductDefinitionChangeAfterGeneratingPick()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create work order, pick and then change pack type of a component product
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, WorkOrderType.Codes.Assemble);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals(true, workOrder.IsAttachedToPickButNotFinalised);
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			AssertNoExceptionThrown("No exception should be thrown eventhough the product definition has been changed after generating the pick.", () => { var units = line.SumOfUnitsMet; });
		}

		#endregion

		#region TestWE_TransactionQuantityAfterChangingComponentDefinition

		public void TestWE_TransactionQuantityAfterChangingComponentDefinition()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			Factory.Save();

			// create work order, pick and then change pack type of a component product
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, WorkOrderType.Codes.Assemble);
			var line = workOrder.Lines.AddNew();
			line.WE_OP = bomProduct.PK;
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();
			AssertNoExceptionThrown("No exceptions should be thrown when product definition is changed.", () => line.WE_TransactionQuantity = 1);
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume

		protected override void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume(ZString type)
		{
			var org = Helper.CreateClient();
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			var level3Part1 = Helper.CreateProduct(org, "Level3Part1");
			var level3Part2 = Helper.CreateProduct(org, "Level3Part2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");
			Helper.SetProductWeightAndVolume(level3Part1, 6, "KG", 0.3, "M3");
			Helper.SetProductWeightAndVolume(level3Part2, 5, "KG", 0.2, "M3");
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(subPart1, level3Part1, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(subPart1, level3Part2, 1m, Constants.PkgUnit.Unit);

			var workOrder = GetNewWhsDocket();
			workOrder.WD_DocketSubType = type;
			workOrder.WD_TotalCubicUnit = Constants.Volume.Litre;
			workOrder.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition - ensure Total Weight is 0", 0m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is 0", 0m, workOrder.WD_TotalCubic);

			var workOrderLine = GetNewBusinessObject(workOrder);
			workOrderLine.WE_OP = mainPart.PK;
			workOrderLine.WE_TransactionQuantity = 10m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Total Weight should be correct for Assemble", 440.92m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 11000m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 661.39m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 10000m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCC

		protected override void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore()
		{
			TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore(type: WorkOrderType.Codes.Assemble);
		}

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCC_Disassembly()
		{
			TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore(type: WorkOrderType.Codes.Disassemble);
		}

		void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore(ZString type)
		{
			var org = Helper.CreateClient();
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 300, "G", 10, "CC");
			Helper.SetProductWeightAndVolume(subPart1, 50, "G", 5, "CC");
			Helper.SetProductWeightAndVolume(subPart2, 100, "G", 1, "CC");
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);

			var workOrder = GetNewWhsDocket();
			workOrder.WD_DocketSubType = type;
			workOrder.WD_TotalCubicUnit = Constants.Volume.Litre;
			workOrder.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition - ensure Total Weight is 0", 0m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is 0", 0m, workOrder.WD_TotalCubic);

			var workOrderLine = GetNewBusinessObject(workOrder);
			workOrderLine.WE_OP = mainPart.PK;
			workOrderLine.WE_TransactionQuantity = 10m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Total Weight should be correct for Assemble", 4.40m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 0.11m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 6.61m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 0.1m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestProductDefinitionNotMatchesWithDocketLineDefinition

		public void TestProductDefinitionNotMatchesWithDocketLineDefinition()
		{
			// create bom product
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);

			// create the work order
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			var line = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			AssertEquals("Product definition hasn't been changed after creating the line.", false, line.ProductDefinitionDoesNotMatchDocketLineProductDefinition());

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			AssertEquals("Pack type of a component product has been changed after creating the line.", true, line.ProductDefinitionDoesNotMatchDocketLineProductDefinition());

			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Box;
			AssertEquals("Pack type of component products have been changed after creating the line.", true, line.ProductDefinitionDoesNotMatchDocketLineProductDefinition());

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Unit;
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Unit;
			AssertEquals("Product definition hasn't been changed after creating the line.", false, line.ProductDefinitionDoesNotMatchDocketLineProductDefinition());

			bomPart2.OE_ComponentQty = 2m;
			AssertEquals("Component quantity has been changed after creating the line.", true, line.ProductDefinitionDoesNotMatchDocketLineProductDefinition());
		}

		#endregion

		#region TestNoExceptionWhenComponentDoesnotHaveUnitConversion

		public void TestNoExceptionWhenComponentDoesnotHaveUnitConversion()
		{
			// Create items
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.CreateProductInInventory("20 Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("10 Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("10 Polish", data.BOM.Polish, 10m);
			data.BOM.BikeEngine.OP_StockKeepingUnit = Constants.PkgUnit.Sheet;  // invalid - no unit conversion Sheet to UNT 
			Factory.Save();

			// Create a WorkOrder
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1");
			var bikeWorkOrderLine = Helper.CreateWhsWorkOrderLine(bikeWorkOrder, data.BOM.Bike, 10m);
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception even if there is not a unit conversion.", () => Helper.CreatePickNew(bikeWorkOrder));
			AssertEquals("It should not assemble any product when we do not know how many of each component we need.", 0m, bikeWorkOrderLine.QuantityAssembled);
		}

		#endregion

		#region TestBillOfMaterials

		public void TestBillOfMaterials()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			bomPart1.OE_ExcludeForVirtualWarehouse = true;
			data.Whs1.WW_IsVirtualWarehouse = false;

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1");
			var line1 = Helper.CreateWhsWorkOrderLine(workOrder1, bomProduct, 1m);
			AssertEquals("Should have correct BOMs count", 2, line1.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have all defined BOMs", new[] { bomPart1, bomPart2 }, line1.BillOfMaterials.ToArray());

			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W2");
			var line2 = Helper.CreateWhsWorkOrderLine(workOrder2, bomProduct, 1m);
			AssertEquals("Should have correct BOMs count", 1, line2.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have only virtual BOMs", new[] { bomPart2 }, line2.BillOfMaterials.ToArray());
		}

		#endregion

		#endregion

		#region ReadOnly

		#region TestStandardReadOnly

		protected override void TestStandardReadOnly(ZPropertyInfo info)
		{
			base.TestStandardReadOnly(info);

			var line = (WhsDocketLine)info.BizObj;
			var workOrder = line.Docket;
			workOrder.WD_DocketStatus = DocketStatus.Codes.New;
			var parentLine = GetNewBusinessObject(workOrder);
			line.WE_WE_ParentDocketLine = parentLine.PK;
			TestReadOnly(info, true, true, true, true);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			DocketLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			AssertEquals(false, DocketLine.ReadOnly);

			DocketLine.WE_WE_ParentDocketLine = ZGuid.NewZGuid();
			AssertEquals("BOM Component lines are auto-generated and should not be editable.", true, DocketLine.ReadOnly);

			DocketLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			AssertEquals(false, DocketLine.ReadOnly);

			DocketLine.ReadOnly = true;
			AssertEquals("Should still be able to manually set ReadOnly to true.", true, DocketLine.ReadOnly);
		}

		#endregion

		#endregion

		#region Clone

		public void TestClone_ClonesComponentLines()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order to build 1 bike
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine workOrderline = workOrder.Lines.AddNew();
			workOrderline.WE_TransactionQuantity = 1;
			workOrderline.WE_OP = data.BOM.Bike.PK;

			var expandedLines = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.All);
			AssertEquals("Should be 14 work order lines - bike, engine, wheels, tyre, rim, block, piston, oil, head, crank, ring, polish(3)", 14, expandedLines.Count);

			// clone the workOrderLine
			var clonedLine = workOrderline.Clone<WhsWorkOrderLine>();
			workOrder.Lines.Add(clonedLine);
			AssertEquals("Should be 28 work order lines - 2 of each: bike, engine, wheels, tyre, rim, block, piston, oil, head, crank, ring, polish(3)", 28, expandedLines.Count);

			// make sure the cloned lines point to the correct parent and that all qtys are correct
			var bikeWheelLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.BikeWheel.PK && line.WE_WE_ParentDocketLine == clonedLine.PK);
			var bikeEngineLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.BikeEngine.PK && line.WE_WE_ParentDocketLine == clonedLine.PK);
			var wheelTyreLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.WheelTyre.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
			var wheelRimLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.WheelRim.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
			var engineBlockLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineBlock.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var enginePistonLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EnginePiston.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var engineOilLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineOil.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var pistonHeadLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonHead.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			var pistonCrankLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonCrank.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			var pistonRingLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonRing.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			var bikePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == clonedLine.PK);
			var enginePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var wheelPolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);

			AssertBOMComponentLine(bikeEngineLine, workOrder, clonedLine, data.BOM.BikeEngineBOMPart, 1m, 16);
			AssertBOMComponentLine(engineBlockLine, workOrder, bikeEngineLine, data.BOM.EngineBlockBOMPart, 1m, 17);
			AssertBOMComponentLine(enginePistonLine, workOrder, bikeEngineLine, data.BOM.EnginePistonBOMPart, 4m, 18);
			AssertBOMComponentLine(pistonHeadLine, workOrder, enginePistonLine, data.BOM.PistonHeadBOMPart, 4m, 19);
			AssertBOMComponentLine(pistonCrankLine, workOrder, enginePistonLine, data.BOM.PistonCrankBOMPart, 4m, 20);
			AssertBOMComponentLine(pistonRingLine, workOrder, enginePistonLine, data.BOM.PistonRingBOMPart, 4m, 21);
			AssertBOMComponentLine(engineOilLine, workOrder, bikeEngineLine, data.BOM.EngineOilBOMPart, 1m, 22);
			AssertBOMComponentLine(enginePolishLine, workOrder, bikeEngineLine, data.BOM.EnginePolishBOMPart, 1m, 23);
			AssertBOMComponentLine(bikeWheelLine, workOrder, clonedLine, data.BOM.BikeWheelBOMPart, 2m, 24);
			AssertBOMComponentLine(wheelTyreLine, workOrder, bikeWheelLine, data.BOM.WheelTyreBOMPart, 2m, 25);
			AssertBOMComponentLine(wheelRimLine, workOrder, bikeWheelLine, data.BOM.WheelRimBOMPart, 2m, 26);
			AssertBOMComponentLine(wheelPolishLine, workOrder, bikeWheelLine, data.BOM.WheelPolishBOMPart, 2m, 27);
			AssertBOMComponentLine(bikePolishLine, workOrder, clonedLine, data.BOM.BikePolishBOMPart, 1m, 28);
		}

		#region TestClone_UpdateTotals

		protected override void TestClone_UpdateTotalsCore()
		{
			TestClone_UpdateTotalsCore(type: WorkOrderType.Codes.Assemble);
		}

		public void TestClone_UpdateTotals_Disassemble()
		{
			TestClone_UpdateTotalsCore(type: WorkOrderType.Codes.Disassemble);
		}

		void TestClone_UpdateTotalsCore(ZString type)
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

			var workOrder = Helper.CreateWhsWorkOrder(org, whs);
			workOrder.WD_DocketSubType = type;
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Precondition - ensure Total Weight is correct when type is Assemble", 20m, workOrder.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Volume is correct when type is Assemble", 1.1m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Precondition - ensure Total Volume is correct when type is Disassemble", 30m, workOrder.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Volume is correct when type is Disassemble", 1m, workOrder.WD_TotalCubic);
			}

			WhsWorkOrderLine clone = (WhsWorkOrderLine)workOrderLine.Clone();
			workOrder.Lines.Add(clone); // in the system it's happens when new Line added to the Grid, but in test we need to simulate it.

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Total Weight should be correct for Assemble", 40m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 2.2m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 60m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 2m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#endregion

		#region BOM

		#region TestBOM_IsComponent

		public void TestBOM_IsComponent()
		{
			DocketLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			AssertEquals(false, DocketLine.BOM.IsComponent);

			DocketLine.WE_WE_ParentDocketLine = ZGuid.NewZGuid();
			AssertEquals(true, DocketLine.BOM.IsComponent);
		}

		#endregion

		#region TestBOM_NonBaseUnitChildItemsPicksCorrectly()

		/// <summary>
		/// Ensure that bom child products that are not in base units are calculated properly
		/// on work orders
		/// </summary>
		public void TestBOM_NonBaseUnitChildItemsPicksCorrectly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateSimpleInventory();
			var fishingKit = Helper.CreateProduct(data.Org1, "FishingKit");
			var fishingHook = Helper.CreateProduct(data.Org1, "FishingHook");
			data.BOM.CreateProductInInventory("fishinghooks", fishingHook, 100);
			Factory.Save();

			// Construct a Bom Product with a carton of hooks as child
			var hookCtnBomPart = Helper.CreateProductBOM(fishingKit, fishingHook, 1, "CTN");
			var cartonSize = fishingHook.UnitConverter.Convert(1m, "CTN", "UNT");

			// Work order for 2 kits ( 2 cartons as child parts )
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, fishingKit, 2);
			var hookCtnLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(0);

			AssertEquals("Correct pack quantity of bom hooks.", 2m, hookCtnLine.WE_PackQuantity);
			AssertEquals("Correct unit quantity of bom hooks.", 2m * cartonSize, hookCtnLine.WE_TransactionQuantity);
			AssertEquals("Correct pack type for bom hooks.", "CTN", hookCtnLine.WE_F3_NKPackType);

			// Pick the order
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Correct pick quantity for hooks.", 2m * cartonSize, hookCtnLine.PickLineQuantity);
			AssertEquals("Correct shortfall for hooks.", 0m, hookCtnLine.QuantityNotPicked);
		}

		#endregion

		#region TestBOM_DuplicateChildProductPicksCorrectly

		public void TestBOM_DuplicateChildProductPicksCorrectly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateSimpleInventory();
			var fishingKit = Helper.CreateProduct(data.Org1, "FishingKit");
			var fishingHook = Helper.CreateProduct(data.Org1, "FishingHook");
			data.BOM.CreateProductInInventory("fishinghooks", fishingHook, 100);
			Factory.Save();

			// Construct a Bom Product with a carton of hooks as child
			var hookUntBomPart = Helper.CreateProductBOM(fishingKit, fishingHook, 1, "UNT");
			var hookCtnBomPart = Helper.CreateProductBOM(fishingKit, fishingHook, 1, "CTN");
			var cartonSize = fishingHook.UnitConverter.Convert(1m, "CTN", "UNT");

			// Work order for 2 kits ( 2 cartons as child parts )
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, fishingKit, 2);
			var hookUntLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(0);
			var hookCtnLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(1);

			AssertEquals("Correct pack quantity of bom hooks(unt).", 2m, hookUntLine.WE_PackQuantity);
			AssertEquals("Correct unit quantity of bom hooks(unt).", 2m, hookUntLine.WE_TransactionQuantity);
			AssertEquals("Correct pack type for bom hooks(unt).", "UNT", hookUntLine.WE_F3_NKPackType);

			AssertEquals("Correct pack quantity of bom hooks(ctn).", 2m, hookCtnLine.WE_PackQuantity);
			AssertEquals("Correct unit quantity of bom hooks(ctn).", 2m * cartonSize, hookCtnLine.WE_TransactionQuantity);
			AssertEquals("Correct pack type for bom hooks(ctn).", "CTN", hookCtnLine.WE_F3_NKPackType);

			// Pick the order
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Correct pick quantity for hooks(unt).", 2m, hookUntLine.PickLineQuantity);
			AssertEquals("Correct pick quantity for hooks(unt).", 0m, hookUntLine.QuantityNotPicked);

			AssertEquals("Correct pick quantity for hooks(ctn).", 2m * cartonSize, hookCtnLine.PickLineQuantity);
			AssertEquals("Correct shortfall for hooks(ctn).", 0m, hookCtnLine.QuantityNotPicked);
		}

		#endregion

		#region TestBOM_DuplicateChildItemsPickShortfallCorrectly
		/* // NOTE: DISABLED DUE TO BUGS IN PICK ALGORITHM, SEE WI00030468
		public void TestBOM_DuplicateChildItemsPickShortfallCorrectly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateSimpleInventory();
			var fishingKit = Helper.CreateProduct(data.Org1, "FishingKit");
			var fishingHook = Helper.CreateProduct(data.Org1, "FishingHook");
			var cartonSize = fishingHook.UnitConverter.Convert(1m, "CTN", "UNT");
			
			// Only receive enough inventory to make 2 of the required 3
			var receiveAmount = 2m * cartonSize + 3;
			data.BOM.CreateProductInInventory("fishinghooks", fishingHook, receiveAmount);
			Factory.Save();

			// Construct a Bom Product with a carton of hooks as child as well as a single unit
			var hookUntBomPart = Helper.CreateProductBOM(fishingKit, fishingHook, 1, "UNT");
			var hookCtnBomPart = Helper.CreateProductBOM(fishingKit, fishingHook, 1, "CTN");

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, fishingKit, 3);
			var hookUntLine = workOrder.Lines[0].BOM.ChildComponentLines[0];
			var hookCtnLine = workOrder.Lines[0].BOM.ChildComponentLines[1];

			AssertEquals("Correct pack quantity of bom hooks(unt).", 3m, hookUntLine.WE_PackQuantity);
			AssertEquals("Correct unit quantity of bom hooks(unt).", 3m, hookUntLine.WE_TransactionQuantity);
			AssertEquals("Correct pack type for bom hooks(unt).", "UNT", hookUntLine.WE_F3_NKPackType);

			AssertEquals("Correct pack quantity of bom hooks(ctn).", 3m, hookCtnLine.WE_PackQuantity);
			AssertEquals("Correct unit quantity of bom hooks(ctn).", 3m * cartonSize, hookCtnLine.WE_TransactionQuantity);
			AssertEquals("Correct pack type for bom hooks(ctn).", "CTN", hookCtnLine.WE_F3_NKPackType);

			// Pick the order
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Correct pick quantity for hooks(unt).", 2m, hookUntLine.PickLineQuantity);
			AssertEquals("Correct pick quantity for hooks(unt).", 1m, hookUntLine.QuantityNotPicked);

			AssertEquals("Correct pick quantity for hooks(ctn).", 2m * cartonSize, hookCtnLine.PickLineQuantity);
			AssertEquals("Correct shortfall for hooks(ctn).", 1m * cartonSize, hookCtnLine.QuantityNotPicked);			
		}
		*/

		#endregion

		#region TestBOM_IsTopLevelProduct

		public void TestBOM_IsTopLevelProduct()
		{
			Data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// check for top level products
			AssertEquals(true, bikeLine.BOM.IsTopLevelProduct);
			AssertEquals(false, bikeWheelLine.BOM.IsTopLevelProduct);
			AssertEquals(false, bikeEngineLine.BOM.IsTopLevelProduct);
			AssertEquals(false, wheelRimLine.BOM.IsTopLevelProduct);
			AssertEquals(false, wheelTyreLine.BOM.IsTopLevelProduct);
			AssertEquals(false, engineBlockLine.BOM.IsTopLevelProduct);
			AssertEquals(false, enginePistonLine.BOM.IsTopLevelProduct);
			AssertEquals(false, pistonHeadLine.BOM.IsTopLevelProduct);
			AssertEquals(false, pistonCrankLine.BOM.IsTopLevelProduct);
			AssertEquals(false, pistonRingLine.BOM.IsTopLevelProduct);
		}

		#endregion

		#region TestBOM_IsParentTopLevelProduct

		public void TestBOM_IsParentTopLevelProduct()
		{
			Data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// check for top level products
			AssertEquals(false, bikeLine.BOM.IsParentTopLevelProduct);
			AssertEquals(true, bikeWheelLine.BOM.IsParentTopLevelProduct);
			AssertEquals(true, bikeEngineLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, wheelRimLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, wheelTyreLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, engineBlockLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, enginePistonLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, pistonHeadLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, pistonCrankLine.BOM.IsParentTopLevelProduct);
			AssertEquals(false, pistonRingLine.BOM.IsParentTopLevelProduct);
		}

		#endregion

		#region TestChildBomLinesAreGeneratedWhenProductEntered

		public void TestChildBomLinesAreGeneratedWhenProductEntered()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order with an empty line
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLineCollection expandedLines = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.All);
			WhsWorkOrderLine workOrderLine1 = workOrder.Lines.AddNew();
			AssertEquals("Precondition - should be 1 work order line", 1, expandedLines.Count);

			// assigning a BOM product should auto-create component lines
			workOrderLine1.WE_TransactionQuantity = 1;
			workOrderLine1.WE_OP = data.BOM.Bike.PK;

			AssertEquals("Should be 11 work order lines - bike, engine, wheels, tyre, rim, block, piston, oil, head, crank, ring, bikePolish, enginePolish, wheelPolish", 14, expandedLines.Count);

			var bikeWheelLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.BikeWheel.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			var bikeEngineLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.BikeEngine.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			var wheelTyreLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.WheelTyre.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
			var wheelRimLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.WheelRim.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
			var engineBlockLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineBlock.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var enginePistonLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EnginePiston.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var engineOilLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineOil.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var pistonHeadLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonHead.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			var pistonCrankLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonCrank.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			var pistonRingLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonRing.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			var bikePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			var enginePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			var wheelPolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);

			AssertBOMComponentLine(bikeEngineLine, workOrder, workOrderLine1, data.BOM.BikeEngineBOMPart, 1m, 2);
			AssertBOMComponentLine(engineBlockLine, workOrder, bikeEngineLine, data.BOM.EngineBlockBOMPart, 1m, 3);
			AssertBOMComponentLine(enginePistonLine, workOrder, bikeEngineLine, data.BOM.EnginePistonBOMPart, 4m, 4);

			AssertBOMComponentLine(pistonHeadLine, workOrder, enginePistonLine, data.BOM.PistonHeadBOMPart, 4m, 5);
			AssertBOMComponentLine(pistonCrankLine, workOrder, enginePistonLine, data.BOM.PistonCrankBOMPart, 4m, 6);
			AssertBOMComponentLine(pistonRingLine, workOrder, enginePistonLine, data.BOM.PistonRingBOMPart, 4m, 7);

			AssertBOMComponentLine(engineOilLine, workOrder, bikeEngineLine, data.BOM.EngineOilBOMPart, 1m, 8);
			AssertBOMComponentLine(enginePolishLine, workOrder, bikeEngineLine, data.BOM.EnginePolishBOMPart, 1m, 9);

			AssertBOMComponentLine(bikeWheelLine, workOrder, workOrderLine1, data.BOM.BikeWheelBOMPart, 2m, 10);
			AssertBOMComponentLine(wheelTyreLine, workOrder, bikeWheelLine, data.BOM.WheelTyreBOMPart, 2m, 11);
			AssertBOMComponentLine(wheelRimLine, workOrder, bikeWheelLine, data.BOM.WheelRimBOMPart, 2m, 12);
			AssertBOMComponentLine(wheelPolishLine, workOrder, bikeWheelLine, data.BOM.WheelPolishBOMPart, 2m, 13);

			AssertBOMComponentLine(bikePolishLine, workOrder, workOrderLine1, data.BOM.BikePolishBOMPart, 1m, 14);

			// change to a product that has less bom components
			workOrderLine1.WE_OP = data.BOM.BikeEngine.PK;
			AssertEquals("Should be 8 work order lines - engine, block, piston, oil, head, crank, ring, polish", 8, expandedLines.Count);
			AssertEquals(true, bikeWheelLine.IsDeleted);
			AssertEquals(true, bikeEngineLine.IsDeleted);
			AssertEquals(true, wheelTyreLine.IsDeleted);
			AssertEquals(true, wheelRimLine.IsDeleted);
			AssertEquals(true, engineBlockLine.IsDeleted);
			AssertEquals(true, enginePistonLine.IsDeleted);
			AssertEquals(true, engineOilLine.IsDeleted);
			AssertEquals(true, pistonHeadLine.IsDeleted);
			AssertEquals(true, pistonCrankLine.IsDeleted);
			AssertEquals(true, pistonRingLine.IsDeleted);
			AssertEquals(true, bikePolishLine.IsDeleted);
			AssertEquals(true, wheelPolishLine.IsDeleted);

			engineBlockLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineBlock.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			enginePistonLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EnginePiston.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			engineOilLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineOil.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			pistonHeadLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonHead.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			pistonCrankLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonCrank.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			pistonRingLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonRing.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			enginePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);

			AssertNotNull("Could not find child engineBlockLine", engineBlockLine);
			AssertNotNull("Could not find child enginePistonLine", enginePistonLine);
			AssertNotNull("Could not find child engineOilLine", engineOilLine);
			AssertNotNull("Could not find child pistonHeadLine", pistonHeadLine);
			AssertNotNull("Could not find child pistonCrankLine", pistonCrankLine);
			AssertNotNull("Could not find child pistonRingLine", pistonRingLine);
			AssertNotNull("Could not find child enginePolishLine", enginePolishLine);

			// change to no product at all
			workOrderLine1.WE_OP = ZGuid.Empty;
			AssertEquals("Product is cleared, all previous child lines should be removed leaving the 1 original line", 1, expandedLines.Count);
			AssertEquals(true, engineBlockLine.IsDeleted);
			AssertEquals(true, enginePistonLine.IsDeleted);
			AssertEquals(true, engineOilLine.IsDeleted);
			AssertEquals(true, pistonHeadLine.IsDeleted);
			AssertEquals(true, pistonCrankLine.IsDeleted);
			AssertEquals(true, pistonRingLine.IsDeleted);
			AssertEquals(true, enginePolishLine.IsDeleted);
		}

		void AssertBOMComponentLine(
			WhsWorkOrderLine componentLine,
			WhsWorkOrder expectedParentWorkOrder,
			WhsWorkOrderLine expectedParentLine,
			OrgPartBOM expectedBOMPart,
			ZDecimal expectedUnits,
			ZShort expectedLineNo)
		{
			AssertEquals(expectedParentWorkOrder.PK, componentLine.WE_WD);
			AssertEquals(expectedParentLine.PK, componentLine.WE_WE_ParentDocketLine);
			AssertEquals(expectedUnits, componentLine.WE_TransactionQuantity);
			AssertEquals(expectedLineNo, componentLine.WE_LineNo);
			AssertEquals(expectedBOMPart.OE_F3_NKPackType, componentLine.WE_F3_NKPackType);
			AssertEquals(expectedBOMPart.OE_OP_Component, componentLine.WE_OP);
		}

		#endregion

		#region TestChildBomLineQuantitiesAreRecalculatedWhenParentQuantityChanged

		public void TestChildBomLineQuantitiesAreRecalculatedWhenParentQuantityChanged()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLineCollection expandedLines = new WhsWorkOrderLineCollection(workOrder, WhsWorkOrderLineFilterStrategy.All);
			WhsWorkOrderLine workOrderLine1 = workOrder.Lines.AddNew();

			workOrderLine1.WE_OP = data.BOM.Bike.PK;
			AssertEquals("Precondition: Should be 11 work order lines - bike, engine, wheels, tyre, rim, block, piston, oil, head, crank, ring, 3x polish", 14, expandedLines.Count);

			WhsWorkOrderLine bikeWheelLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.BikeWheel.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			WhsWorkOrderLine bikeEngineLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.BikeEngine.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			WhsWorkOrderLine wheelTyreLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.WheelTyre.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
			WhsWorkOrderLine wheelRimLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.WheelRim.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);
			WhsWorkOrderLine engineBlockLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineBlock.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			WhsWorkOrderLine enginePistonLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EnginePiston.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			WhsWorkOrderLine engineOilLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.EngineOil.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			WhsWorkOrderLine pistonHeadLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonHead.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			WhsWorkOrderLine pistonCrankLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonCrank.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			WhsWorkOrderLine pistonRingLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.PistonRing.PK && line.WE_WE_ParentDocketLine == enginePistonLine.PK);
			WhsWorkOrderLine bikePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == workOrderLine1.PK);
			WhsWorkOrderLine enginePolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeEngineLine.PK);
			WhsWorkOrderLine wheelPolishLine = (WhsWorkOrderLine)expandedLines.Single(line => line.WE_OP == data.BOM.Polish.PK && line.WE_WE_ParentDocketLine == bikeWheelLine.PK);

			AssertNotNull("Could not find child bikeWheelLine", bikeWheelLine);
			AssertNotNull("Could not find child bikeEngineLine", bikeEngineLine);
			AssertNotNull("Could not find child wheelTyreLine", wheelTyreLine);
			AssertNotNull("Could not find child wheelRimLine", wheelRimLine);
			AssertNotNull("Could not find child engineBlockLine", engineBlockLine);
			AssertNotNull("Could not find child enginePistonLine", enginePistonLine);
			AssertNotNull("Could not find child enginePistonLine", engineOilLine);
			AssertNotNull("Could not find child pistonHeadLine", pistonHeadLine);
			AssertNotNull("Could not find child pistonCrankLine", pistonCrankLine);
			AssertNotNull("Could not find child pistonRingLine", pistonRingLine);
			AssertNotNull("Could not find child bikePolish", bikePolishLine);
			AssertNotNull("Could not find child enginePolish", enginePolishLine);
			AssertNotNull("Could not find child wheelPolish", wheelPolishLine);

			ZDecimal qty = workOrderLine1.WE_TransactionQuantity = 10m;

			AssertEquals(qty * 2m, bikeWheelLine.WE_TransactionQuantity);
			AssertEquals(qty * 2m, wheelTyreLine.WE_TransactionQuantity);
			AssertEquals(qty * 2m, wheelRimLine.WE_TransactionQuantity);
			AssertEquals(qty, bikeEngineLine.WE_TransactionQuantity);
			AssertEquals(qty, engineBlockLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, enginePistonLine.WE_TransactionQuantity);
			AssertEquals(qty, engineOilLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, pistonHeadLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, pistonCrankLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, pistonRingLine.WE_TransactionQuantity);
			AssertEquals(qty, enginePolishLine.WE_TransactionQuantity);
			AssertEquals(qty * 2m, wheelPolishLine.WE_TransactionQuantity);
			AssertEquals(qty, bikePolishLine.WE_TransactionQuantity);

			qty = workOrderLine1.WE_TransactionQuantity = 4m;

			AssertEquals(qty * 2m, bikeWheelLine.WE_TransactionQuantity);
			AssertEquals(qty * 2m, wheelTyreLine.WE_TransactionQuantity);
			AssertEquals(qty * 2m, wheelRimLine.WE_TransactionQuantity);
			AssertEquals(qty, bikeEngineLine.WE_TransactionQuantity);
			AssertEquals(qty, engineBlockLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, enginePistonLine.WE_TransactionQuantity);
			AssertEquals(qty, engineOilLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, pistonHeadLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, pistonCrankLine.WE_TransactionQuantity);
			AssertEquals(qty * 4, pistonRingLine.WE_TransactionQuantity);
			AssertEquals(qty, enginePolishLine.WE_TransactionQuantity);
			AssertEquals(qty * 2m, wheelPolishLine.WE_TransactionQuantity);
			AssertEquals(qty, bikePolishLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestGetBOMQtyForChildComponentLine

		public void TestGetBOMQtyForChildComponentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Bag, Constants.PkgUnit.Unit, 5);
			var product = Helper.CreateProductBOM(data.Part1, data.Part2, 10, Constants.PkgUnit.Bag);
			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			workOrderLine.WE_OP = data.Part1.PK;
			workOrderLine.WE_TransactionQuantity = 1;
			AssertEquals(2m, workOrderLine.BOM.GetBOMQtyForChildComponentLine(product));

			workOrderLine.WE_TransactionQuantity = 2;
			AssertEquals(4m, workOrderLine.BOM.GetBOMQtyForChildComponentLine(product));
		}

		#endregion

		#region TestBOM_ClearInVirtualWarehouse

		public void TestBOM_ClearInVirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = false;

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

			data.Whs1.WW_IsVirtualWarehouse = true;
			line.ClearInVirtualWarehouse();
			AssertEquals("Should have correct BOMs count", 1, line.BillOfMaterials.Count);
			AssertContainsExactElementsInAnyOrder("Should have only virtual BOMs", new[] { bomPart2 }, line.BillOfMaterials.ToArray());
		}

		#endregion

		#endregion

		#region Delete

		#region TestCanDelete

		public void TestCanDelete()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine emptyLine = workOrder.Lines.AddNew();
			WhsWorkOrderLine bikeLine = workOrder.Lines.AddNew();
			bikeLine.WE_OP = data.BOM.Bike.PK;
			var bikeWheelLine = (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == data.BOM.BikeWheel.PK);
			var wheelTyreLine = (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == data.BOM.WheelTyre.PK);

			AssertEquals("Non-product should allow delete.", true, emptyLine.CanDelete);
			AssertEquals("Top-Level products should allow delete.", true, bikeLine.CanDelete);
			AssertEquals("Non Top-Level products should *not* allow delete.", false, bikeWheelLine.CanDelete);
			AssertEquals("Component lines should *not* allow delete.", false, wheelTyreLine.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("Bill of Materials Component lines cannot be deleted.", DocketLine.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDeleteDeletesChildComponentLine

		public void TestDeleteDeletesChildComponentLine()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create a work order
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine workOrderLine1 = workOrder.Lines.AddNew();

			// assigning a BOM product should auto-create component lines
			workOrderLine1.WE_OP = data.BOM.Bike.PK;
			workOrder.BOM.ExpandAllLines();
			AssertEquals("Should be 11 work order lines - bike, engine, wheels, tyre, rim, block, piston, oil, head, crank, ring, polish(3)", 14, workOrder.Lines.Count);

			// deleting a top level line should delete children
			IEnumerable<WhsDocketLine> linesThatShouldBeDeleted = workOrder.Lines.Skip(1);
			workOrderLine1.Delete();
			AssertEquals("Work Order line is deleted, all child lines should be removed", 0, workOrder.Lines.Count);
			foreach (WhsWorkOrderLine workOrderLine in linesThatShouldBeDeleted)
			{
				AssertEquals("Line should be deleted", true, workOrderLine.IsDeleted);
			}
		}

		#endregion

		#region TestDeleteReducesWeightCorrectly

		public void TestDeleteReducesWeightCorrectly()
		{
			var whs = Helper.CreateWarehouse("WH1");
			var client = Helper.CreateClient("CL1");
			var productParent = Helper.CreateProduct(client, "BASKET");
			productParent.OP_StockKeepingUnit = "UNT";
			productParent.OP_Weight = 10;
			productParent.OP_WeightUQ = "KG";
			var productChild = Helper.CreateProduct(client, "MELON");
			productChild.OP_StockKeepingUnit = "UNT";
			productChild.OP_Weight = 2;
			productChild.OP_WeightUQ = "KG";

			// 3 MELON, total weight is 3*2 = 6 kg. BASKET did not count.
			Helper.CreateProductBOM(productParent, productChild, 3, "UNT");

			var workOrder = Helper.CreateWhsWorkOrder(client, whs);
			workOrder.WD_TotalWeightUnit = "KG";
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, productParent.PK, 4);
			AssertEquals("Precondition: ", 4m * 6m, workOrder.WD_TotalWeight);

			workOrderLine.Delete();
			AssertEquals(0m, workOrder.WD_TotalWeight);
		}

		#endregion

		#region TestDelete_DoesNotRecalculateTotalWeightAndVolume

		protected override void TestDelete_DoesNotRecalculateTotalWeightAndVolume(ZString type)
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

			var workOrder = Helper.CreateWhsWorkOrder(org, whs);
			workOrder.WD_DocketSubType = type;

			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 1m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, mainPart, 2m);

			AssertEquals("WD_TotalWeightUnit", workOrder.WD_TotalWeightUnit, "KG");
			AssertEquals("WD_TotalVolumeUnit", workOrder.WD_TotalCubicUnit, "M3");

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Precondition - ensure Total Weight is correct when type is Assemble", 60m, workOrder.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Volume is correct when type is Assemble", 3.3m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Precondition - ensure Total Volume is correct when type is Disassemble", 90m, workOrder.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Volume is correct when type is Disassemble", 3m, workOrder.WD_TotalCubic);
			}

			// Manually modification of Weight/Volume is allowed.
			workOrder.WD_TotalWeight = 200m;
			workOrder.WD_TotalCubic = 10m;
			AssertEquals("Total Weight should be updated", 200m, workOrder.WD_TotalWeight);
			AssertEquals("Total Volume should be updated", 10m, workOrder.WD_TotalCubic);

			// Deleting lines will reduce Weight/Volume of deleted line from the total amount without recalculating the totals.
			workOrderLine2.Delete();

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Total Weight should be correct for Assemble", 160m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 7.8m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 140m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 8m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#endregion

		#region Shortfalls

		#region TestWE_ShortfallQuantityCached_ForSerialNumber

		public void TestWE_ShortfallQuantityCached_ForSerialNumber()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Bike, 1m, data.Whs1.DefaultLocation);
			receiveLine.WI_SerialNumber = "Serial0001";
			receive.FinaliseDocket();
			Factory.Save();

			// work order to disassemble 1 bikes with serial number
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			bikeLine.WE_SerialNumber = "Serial0002";
			Factory.Save();

			bikeLine.Validation.ValidateWE_ShortfallQuantityCached(); // simulate Grid behaviour
			AssertEquals(1m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_SerialNumber = "Serial0001";
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached(); // simulate Grid behaviour
			AssertEquals(0m, bikeLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_ForDisassembly_WithAttribs

		public void TestWE_ShortfallQuantityCached_ForDisassembly_WithAttribs()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Bike, 1m, data.Whs1.DefaultLocation);
			receiveLine.WI_PartAttrib1 = "PA1";
			receiveLine.WI_PartAttrib2 = "PA2";
			receiveLine.WI_PartAttrib3 = "PA3";
			receiveLine.WI_ExpiryDate = new ZDate(year + 1, 1, 1);
			receiveLine.WI_PackingDate = new ZDate(year + 1, 1, 2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			bikeLine.WE_PartAttrib1 = "PA1";
			bikeLine.WE_PartAttrib2 = "PA2";
			bikeLine.WE_PartAttrib3 = "PA3";
			bikeLine.WE_ExpiryDate = new ZDate(year + 1, 1, 1);
			bikeLine.WE_PackingDate = new ZDate(year + 1, 1, 2);

			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(0m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_PartAttrib1 = "XXX";
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(1m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_PartAttrib1 = "PA1";
			bikeLine.WE_PartAttrib2 = "XXX";
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(1m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_PartAttrib2 = "PA2";
			bikeLine.WE_PartAttrib3 = "XXX";
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(1m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_PartAttrib3 = "PA3";
			bikeLine.WE_ExpiryDate = new ZDate(year + 2, 1, 1);
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(1m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_ExpiryDate = new ZDate(year + 1, 1, 1);
			bikeLine.WE_PackingDate = new ZDate(year + 2, 1, 2);
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(1m, bikeLine.WE_ShortfallQuantityCached);

			bikeLine.WE_PackingDate = new ZDate(year + 1, 1, 2);
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(0m, bikeLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_ForDisassembly_WithExpiredGoods

		public void TestWE_ShortfallQuantityCached_ForDisassembly_WithExpiredGoods()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.BOM.Bike, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Bike, 1m, data.Whs1.DefaultLocation);
			receiveLine.WI_ExpiryDate = new ZDate(year - 1, 1, 1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			bikeLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals("Stock is expired, should appear as shortfall.", 1m, bikeLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePickCore

		protected override void TestWE_ShortfallQuantityCached_BeforePickCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive 1 fully-built bike (this should not be considered for WorkOrder shortfall)
			data.CreateProductInInventory("Bikes #1", data.BOM.Bike, 1m);

			// receive enough top-level components to build 5/10 bikes
			data.CreateProductInInventory("Wheels #1", data.BOM.BikeWheel, 11m);  // enough to build 5 bikes + 1 spare wheel
			data.CreateProductInInventory("Engines #1", data.BOM.BikeEngine, 6m); // enough to build 6 bikes
			data.CreateProductInInventory("Polish #1", data.BOM.Polish, 10m);     // enough to build 10 bikes

			// receive some sub-components to confirm that they are not used
			data.CreateProductInInventory("Rims #1", data.BOM.WheelRim, 20m);
			data.CreateProductInInventory("Tyres #1", data.BOM.WheelTyre, 20m);

			Factory.Save();

			// order 10 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);
			var bikePolishLine = data.BOM.Lines.BikePolish(workOrder);

			AssertEquals("Only enough stock to build 5/10 bikes, shortfall should be 5.", 5m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals("11 in Stock, shortfall should be 9/20.", 9m, bikeWheelLine.WE_ShortfallQuantityCached);
			AssertEquals(" 6 in Stock, shortfall should be 4/10 Engines.", 4m, bikeEngineLine.WE_ShortfallQuantityCached);
			AssertEquals("10 in Stock, shortfall should be 0 Polish.", 0m, bikePolishLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelTyreLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelRimLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, engineBlockLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, enginePistonLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonHeadLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonCrankLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonRingLine.WE_ShortfallQuantityCached);

			// receive enough wheels to fulfill the WO
			data.CreateProductInInventory("Wheels #2", data.BOM.BikeWheel, 9m);
			Factory.Save();

			// ensure shortfalls are cached (for performance)
			if (ShortfallQuantityIsCached)
			{
				AssertEquals("Shortfall was not cached", 5m, bikeLine.WE_ShortfallQuantityCached);
				AssertEquals("Shortfall was not cached", 9m, bikeWheelLine.WE_ShortfallQuantityCached);
				AssertEquals("Shortfall was not cached", 4m, bikeEngineLine.WE_ShortfallQuantityCached);
			}

			Array.ForEach(workOrder.AllLines.ToArray<WhsWorkOrderLine>(), line => line.ClearWE_ShortfallQuantityCached());
			AssertEquals("Only enough stock to build 6/10 bikes, shortfall should be 4.", 4m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals("20 in Stock, shortfall should be 0/20.", 0m, bikeWheelLine.WE_ShortfallQuantityCached);
			AssertEquals(" 6 in Stock, shortfall should be 4/10 Engines.", 4m, bikeEngineLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelTyreLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelRimLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, engineBlockLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, enginePistonLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonHeadLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonCrankLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonRingLine.WE_ShortfallQuantityCached);

			// receive enough stock to build all 10 bikes
			data.BOM.CreateBOMComponentsInInventory();
			Factory.Save();

			Array.ForEach(workOrder.AllLines.ToArray<WhsWorkOrderLine>(), line => line.ClearWE_ShortfallQuantityCached());
			AssertEquals(0m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikeWheelLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikeEngineLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelTyreLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelRimLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, engineBlockLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, enginePistonLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonHeadLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonCrankLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonRingLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikePolishLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_ForDisassembly

		public void TestWE_ShortfallQuantityCached_BeforePick_ForDisassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // receive 100 bikes
			Factory.Save();

			// order disassembly of 120 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 120m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// only the top line should be in shortfall as we never pick components for DIS
			AssertEquals("Only enough stock to disassemble 100/120 bikes, shortfall should be 20.", 20m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikeWheelLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikeEngineLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelTyreLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelRimLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, engineBlockLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, enginePistonLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonHeadLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonCrankLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonRingLine.WE_ShortfallQuantityCached);

			// bring in more fully built bikes to meet the shortfall
			data.CreateProductInInventory("Bikes", data.BOM.Bike, 20m);
			Factory.Save();

			bikeLine.ClearWE_ShortfallQuantityCached();
			AssertEquals(0m, bikeLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_MinIsZero

		public void TestWE_ShortfallQuantityCached_BeforePick_MinIsZero()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive enough components to build 10 bikes
			data.CreateProductInInventory("Engines", data.BOM.BikeEngine, 10m);
			data.CreateProductInInventory("Wheels", data.BOM.BikeWheel, 20m);
			data.CreateProductInInventory("Polish", data.BOM.Polish, 10m);
			Factory.Save();

			// order the building of 7 bikes, ensure shortfall is not -3.
			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.BOM.Bike.PK;
			docketLine.WE_TransactionQuantity = 7;
			AssertEquals(0m, docketLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_ProductWithZeroComponentQty

		public void TestWE_ShortfallQuantityCached_ProductWithZeroComponentQty()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.BOM.CreateProductInInventory("engines", data.BOM.BikeEngine, 5);
			data.BOM.CreateProductInInventory("wheels", data.BOM.BikeWheel, 20);
			data.BOM.CreateProductInInventory("polish", data.BOM.Polish, 5);

			var bikeEngine = data.BOM.BikeEngineBOMPart;
			bikeEngine.OE_ComponentQty = 0m;
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			AssertEquals("Calculates the shortfall of polish.", 5m, bikeLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_DuringDataImport

		public void TestWE_ShortfallQuantityCached_BeforePick_DuringDataImport()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// order 10 bikes
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);
			Factory.Save();

			// simulate data import and ensure we do not calc shortfalls (performance can be terrible when importing thousands of lines)
			var bikeLineBeforeShortfallCalc = new BusinessObjectFactory().Load<WhsWorkOrderLine>(bikeLine.PK);
			bikeLineBeforeShortfallCalc.IsImportingData = true;
			foreach (WhsWorkOrderLine line in bikeLineBeforeShortfallCalc.WorkOrder.AllLines)
			{
				AssertEquals("IsImportingData is true, Shortfall should not calculate.", 0m, line.WE_ShortfallQuantityCached);
			}
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_AfterPick

		protected override void TestWE_ShortfallQuantityCached_AfterPickCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive enough stock to build 5 bikes.
			data.BOM.CreateProductInInventory("engines", data.BOM.BikeEngine, 5);
			data.BOM.CreateProductInInventory("wheels", data.BOM.BikeWheel, 20);
			data.BOM.CreateProductInInventory("polish", data.BOM.Polish, 10);
			Factory.Save();

			// order 10 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// pick the WorkOrder
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			AssertEquals("Need 10 engines but only 5 available.", 5m, bikeEngineLine.WE_ShortfallQuantityCached);
			AssertEquals("Need 20 wheels and all 20 available but as we can only build 5 bikes, we should only have picked 10, thus shortfall of 10.", 10m, bikeWheelLine.WE_ShortfallQuantityCached);
			AssertEquals("Need to build 10 bikes but can only build 5.", 5m, bikeLine.WE_ShortfallQuantityCached);

			// this test ensures the picklines are loaded prior to shortfall calculation
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			WhsWorkOrderLine bikeLineInOtherFactory = otherFactory.Load<WhsWorkOrderLine>(bikeLine.PK);
			AssertEquals("Need to build 10 bikes but can only build 5.", 5m, bikeLineInOtherFactory.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_AfterPick_ForDisassembly

		public void TestWE_ShortfallQuantityCached_AfterPick_ForDisassembly()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // receive 100 bikes
			Factory.Save();

			// order disassembly of 120 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 120m);

			// pick the bikes for disassembly
			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			// ensure we recalc the shortfall on at least the top + some component lines
			bikeLine.ClearWE_ShortfallQuantityCached();
			bikeWheelLine.ClearWE_ShortfallQuantityCached();
			wheelTyreLine.ClearWE_ShortfallQuantityCached();

			// only the top line should be in shortfall as we never pick components for DIS
			AssertEquals("Only enough stock to disassemble 100/120 bikes, shortfall should be 20.", 20m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikeWheelLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, bikeEngineLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelTyreLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, wheelRimLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, engineBlockLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, enginePistonLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonHeadLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonCrankLine.WE_ShortfallQuantityCached);
			AssertEquals(0m, pistonRingLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_AfterPick_ForDisassembly_SameProduct

		public void TestWE_ShortfallQuantityCached_AfterPick_ForDisassembly_SameProduct()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory(); // receive 100 bikes
			Factory.Save();

			// order disassembly of 240 bikes
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var bikeLine1 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 120m);
			var bikeLine2 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 120m);
			// pick the bikes for disassembly
			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			// ensure we recalc the shortfall on at least the top + some component lines
			bikeLine1.ClearWE_ShortfallQuantityCached();
			bikeLine2.ClearWE_ShortfallQuantityCached();

			// only the top line should be in shortfall as we never pick components for DIS
			AssertEquals("Only enough stock to disassemble 100/120 bikes, shortfall should be 20.", 20m, bikeLine1.WE_ShortfallQuantityCached);
			AssertEquals("No Stock left for this line , shortfall should be 120.", 120m, bikeLine2.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestGetShortfallExistsStatus_AfterPick

		protected override void TestGetShortfallExistsStatus_AfterPickCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive 1 fully-built bike (this should not be considered for WorkOrder shortfall)
			data.CreateProductInInventory("Bikes #1", data.BOM.Bike, 1m);

			// receive enough components to build 7/10 bikes
			data.CreateProductInInventory("Wheels #1", data.BOM.BikeWheel, 21m);  // enough to build 10 bikes + 1 spare wheel
			data.CreateProductInInventory("Engines #1", data.BOM.BikeEngine, 7m); // enough to build 7 bikes
			data.CreateProductInInventory("Polish #1", data.BOM.Polish, 10m);     // enough to build 10 bikes
			Factory.Save();

			// order 10 bikes
			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.BOM.Bike.PK;
			docketLine.WE_TransactionQuantity = 10;

			// get the bike lines
			var bikeWheelLine = data.BOM.Lines.BikeWheel(docket);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(docket);
			var bikePolishLine = data.BOM.Lines.BikePolish(docket);

			AssertEquals("Can only build 7/10 bikes", true, docketLine.GetShortfallExistsStatus());
			AssertEquals("Have 20/20 Wheels.", false, bikeWheelLine.GetShortfallExistsStatus());
			AssertEquals("Have 7/10 engines.", true, bikeEngineLine.GetShortfallExistsStatus());
			AssertEquals("Have 10/10 polish.", false, bikePolishLine.GetShortfallExistsStatus());

			// receive into stock enough engines to fulfill the WorkOrder
			data.CreateProductInInventory("Engines #2", data.BOM.BikeEngine, 3m);
			Factory.Save();

			AssertEquals("Can build 10/10 bikes", false, docketLine.GetShortfallExistsStatus());
			AssertEquals("Have 20/20 Wheels.", false, bikeWheelLine.GetShortfallExistsStatus());
			AssertEquals("Have 10/10 engines.", false, bikeEngineLine.GetShortfallExistsStatus());
			AssertEquals("Have 10/10 polish.", false, bikePolishLine.GetShortfallExistsStatus());
		}

		#endregion

		#region TestWE_TransactionQuantityUpdatesShortfall

		public void TestWE_TransactionQuantityUpdatesShortfall()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			DocketLine.WE_OP = data.BOM.Bike.PK;

			AssertUnitsAndShortfall("Precondition", DocketLine, 0m, 0m);
			AssertUnitsAndShortfall("Precondition", data.BOM.Lines.BikeEngine(Docket), 0m, 0m);
			AssertUnitsAndShortfall("Precondition", data.BOM.Lines.EngineBlock(Docket), 0m, 0m);
			AssertUnitsAndShortfall("Precondition", data.BOM.Lines.EnginePiston(Docket), 0m, 0m);
			AssertUnitsAndShortfall("Precondition", data.BOM.Lines.PistonCrank(Docket), 0m, 0m);

			DocketLine.WE_TransactionQuantity = 1;
			AssertUnitsAndShortfall(DocketLine, 1m, 1m);
			AssertUnitsAndShortfall(data.BOM.Lines.BikeEngine(Docket), 1m, 1m);
			AssertUnitsAndShortfall(data.BOM.Lines.EngineBlock(Docket), 1m, 0m);
			AssertUnitsAndShortfall(data.BOM.Lines.EnginePiston(Docket), 4m, 0m);
			AssertUnitsAndShortfall(data.BOM.Lines.PistonCrank(Docket), 4m, 0m);

			DocketLine.WE_TransactionQuantity = 2;
			AssertUnitsAndShortfall(DocketLine, 2m, 2m);
			AssertUnitsAndShortfall(data.BOM.Lines.BikeEngine(Docket), 2m, 2m);
			AssertUnitsAndShortfall(data.BOM.Lines.EngineBlock(Docket), 2m, 0m);
			AssertUnitsAndShortfall(data.BOM.Lines.EnginePiston(Docket), 8m, 0m);
			AssertUnitsAndShortfall(data.BOM.Lines.PistonCrank(Docket), 8m, 0m);
		}

		void AssertUnitsAndShortfall(WhsWorkOrderLine line, ZDecimal units, ZDecimal shortfall)
		{
			AssertUnitsAndShortfall("", line, units, shortfall);
		}

		void AssertUnitsAndShortfall(string msg, WhsWorkOrderLine line, ZDecimal units, ZDecimal shortfall)
		{
			AssertEquals(msg, units, line.WE_TransactionQuantity);
			AssertEquals(msg, shortfall, line.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestSuspendShortfallCalculation

		protected override void TestSuspendShortfallCalculationCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive enough top-level components to build 5/10 bikes
			data.CreateProductInInventory("Wheels #1", data.BOM.BikeWheel, 11m);  // enough to build 5 bikes + 1 spare wheel
			data.CreateProductInInventory("Engines #1", data.BOM.BikeEngine, 6m); // enough to build 6 bikes
			data.CreateProductInInventory("Polish #1", data.BOM.Polish, 10m);     // enough to build 10 bikes

			Factory.Save();

			// order 10 bikes
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			AssertEquals("Pre-condition", false, bikeLine.Shortfall.IsShortfallCalculationSuspended);

			bikeLine.ClearWE_ShortfallQuantityCached();
			bikeLine.Shortfall.SuspendShortfallCalculation();
			AssertEquals("Shortfall calculation is suspended", 0m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals("Cached value should not be updated if calculation is suspended", false, bikeLine.GetShortfallCacheValueForTest().HasValue);
			AssertEquals(true, bikeLine.Shortfall.IsShortfallCalculationSuspended);

			bikeLine.Shortfall.ResumeShortfallCalculation();
			AssertEquals("Only enough stock to build 5/10 bikes, shortfall should be 5.", 5m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals(false, bikeLine.Shortfall.IsShortfallCalculationSuspended);

			bikeLine.Shortfall.SuspendShortfallCalculation();
			AssertEquals("Shortfall calculation is suspended but value has been cached.", 5m, bikeLine.WE_ShortfallQuantityCached);
			AssertEquals(true, bikeLine.Shortfall.IsShortfallCalculationSuspended);

			bikeLine.GetShortfallExistsStatus();
			AssertEquals("Shortfall calculation suspended so cached value should not be cleared.", 5m, bikeLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestCalculateShortfall_WorkOrderLinesWithSameProduct_Assemble

		public void TestCalculateShortfall_WorkOrderLinesWithSameProduct_Assemble()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// receive enough to make a one bike
			data.CreateProductInInventory("Wheels #1", data.BOM.BikeWheel, 2m);
			data.CreateProductInInventory("Engines #1", data.BOM.BikeEngine, 1m);
			data.CreateProductInInventory("Polish #1", data.BOM.Polish, 1m);

			Factory.Save();

			// order 3 bikes
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			var orderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);
			var orderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 1m);

			AssertEquals("No Shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("No Shortfall.", 0m, orderLine1.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK).WE_ShortfallQuantityCached);
			AssertEquals("No Shortfall.", 0m, orderLine1.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK).WE_ShortfallQuantityCached);
			AssertEquals("No Shortfall.", 0m, orderLine1.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.Polish.PK).WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike.", 1m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike. 2 Wheels.", 2m, orderLine2.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK).WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike. 1 Engine.", 1m, orderLine2.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK).WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike. 1 Polish.", 1m, orderLine2.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.Polish.PK).WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike.", 1m, orderLine3.WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike. 2 Wheels.", 2m, orderLine3.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeWheel.PK).WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike. 1 Engine.", 1m, orderLine3.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.BikeEngine.PK).WE_ShortfallQuantityCached);
			AssertEquals("Shortfall 1 bike. 1 Polish.", 1m, orderLine3.BOM.ChildComponentLines.Single(l => l.WE_OP == data.BOM.Polish.PK).WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestCalculateShortfall_WorkOrderLinesWithSameProduct_Disassemble

		public void TestCalculateShortfall_WorkOrderLinesWithSameProduct_Disassemble()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create receive and work order
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.BOM.Bike, 3m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			AssertEquals("Precondition - number of items in stock is incorrect.", 0m, workOrderLine1.WE_ShortfallQuantityCached);
			AssertEquals("Precondition - number of items in stock is incorrect.", 1m, workOrderLine2.WE_ShortfallQuantityCached);
			AssertEquals("Precondition - number of items in stock is incorrect.", 2m, workOrderLine3.WE_ShortfallQuantityCached);
		}

		#endregion

		#endregion

		#region TestGetQuantityFromComponents

		protected override void TestGetQuantityFromComponentsCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7);
			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Picking failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			var bikeLine = data.BOM.Lines.Bike(workOrder);
			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			AssertEquals(0, bikeLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, bikeWheelLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, bikeEngineLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, wheelRimLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, wheelTyreLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, engineBlockLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, enginePistonLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, pistonHeadLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, pistonCrankLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
			AssertEquals(0, pistonRingLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
		}

		#endregion

		#region BOA/BOD Printing Support

		public void TestOrgSupplierPartDocManagerInfo()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = workOrder.Lines.AddNew();
			bikeLine.WE_OP = data.BOM.Bike.PK;
			var bikeWheelLine = (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == data.BOM.BikeWheel.PK);
			var wheelTyreLine = (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == data.BOM.WheelTyre.PK);

			OrgSupplierPart part = bikeLine.SupplierPart;
			AssertNotNull("Line has a product", part);
			AssertEquals("Assembly work order", WorkOrderType.Codes.Assemble, workOrder.WD_DocketSubType);
			AssertNotNull("Therefore line has a product document manager", bikeLine.SupplierPartDocManagerInfo);
			AssertEquals("Bill Of Assembly document type", "BOA", bikeLine.SupplierPartDocManagerInfo.DocManagerCode);
		}

		public void TestResetSupplierPartDocManagerInfo()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = workOrder.Lines.AddNew();
			bikeLine.WE_OP = Factory.NewWithValidTestData<OrgSupplierPart>().PK;

			AssertNotNull("SupplierPartDoceManagerInfo is valid", bikeLine.SupplierPartDocManagerInfo);
			AssertNotNull("SupplierPartDoceManagerInfo private field has value", bikeLine.supplierPartDocManagerInfo);

			bikeLine.ResetSupplierPartDocManagerInfo();
			AssertNull("SupplierPartDoceManagerInfo private field no longer value", bikeLine.supplierPartDocManagerInfo);
		}

		#endregion

		#region ISupportDataImporting Members

		public void TestISupportDataImporting()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// order 10 bikes
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 10m);

			bikeLine.IsImportingData = true;
			foreach (WhsWorkOrderLine line in bikeLine.WorkOrder.AllLines)
			{
				AssertEquals("IsImportingData should be inherited from the top part.", true, line.IsImportingData);
			}

			bikeLine.IsImportingData = false;
			foreach (WhsWorkOrderLine line in bikeLine.WorkOrder.AllLines)
			{
				AssertEquals("IsImportingData should be inherited from the top part.", false, line.IsImportingData);
			}
		}

		#endregion

		#region Implementation

		protected override bool SupportsCustomsSubType => false;

		protected override FinalisableDocketHelper<WhsWorkOrder> GetNewDocketHelper(BusinessObjectFactory factory)
		{
			return new FinalisableWorkOrderHelper(factory);
		}

		protected TestDataForBOM Data
		{
			get { return data ?? (data = new TestDataForBOM(Factory)); }
		}

		TestDataForBOM data;

		#endregion
	}
}
