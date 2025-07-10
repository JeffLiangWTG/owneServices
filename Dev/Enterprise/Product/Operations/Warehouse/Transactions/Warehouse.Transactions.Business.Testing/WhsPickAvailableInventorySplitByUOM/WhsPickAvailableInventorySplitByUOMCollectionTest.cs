using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickAvailableInventorySplitByUOMCollection))]
	class WhsPickAvailableInventorySplitByUOMCollectionTest : WhsPickAvailableInventorySplitBaseCollectionTest<WhsPickAvailableInventorySplitByUOMCollection, WhsPickAvailableInventorySplitByUOM>
	{
		#region TestRebuildCollection

		protected override void TestRebuildCollectionCore()
		{
			Assert("All tests of RebuildCollection are in the following region TestRebuildCollection_Detailed.", true);
		}

		protected override void AdditionalSetupForLoadAvailableInventoryTest(TestDataSimpleEnvironment data)
		{
			base.AdditionalSetupForLoadAvailableInventoryTest(data);
			data.Whs1.WW_IsPickByUOMEnabled = true;
		}

		#endregion

		#region TestRebuildCollection_Detailed

		#region TestUOMInventoryCreatedFromPickLinesAndUnitConversion

		public void TestUOMInventoryCreatedFromPickLinesAndUnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "PLT", 20m);
			var refPackTypes = new RefPackTypeCollection(Factory);
			refPackTypes.Single(r => r.F3_Code == "PLT").F3_UOMType = UOMPackTypesList.Codes.Pallet;
			refPackTypes.Single(r => r.F3_Code == "UNT").F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			// receive 22 unit in three lines (15+5+2)
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 22m);
			var pick = Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: 3 picklines created", 3, orderLine.PickLines.Count);
			// now let's emulate pick allocation, so 2 picklines (15+5) will get assigned a PLT pack type and the third pickline will get UNT
			orderLine.PickLines.Single(pl => pl.WZ_Units == 15m).WZ_F3_NKAllocatedPackType = "PLT";
			orderLine.PickLines.Single(pl => pl.WZ_Units == 5m).WZ_F3_NKAllocatedPackType = "PLT";
			orderLine.PickLines.Single(pl => pl.WZ_Units == 2m).WZ_F3_NKAllocatedPackType = "UNT";

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories.Count);

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.NeedsRefresh = true;
			uomAvalilableInventoryCollection.RebuildCollection();
			AssertEquals("Collection has been rebuilt, NeedsRefresh should be false.", false, uomAvalilableInventoryCollection.NeedsRefresh);

			AssertEquals(2, uomAvalilableInventoryCollection.Count);
			var uomAvailableInventoryPLT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "PLT");
			AssertNotNull(uomAvailableInventoryPLT);
			AssertEquals("Should be 1 PLT", 1m, uomAvailableInventoryPLT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryPLT.StockKeepingUnit);
			AssertEquals(20m, uomAvailableInventoryPLT.StockUnitQuantity);
			AssertEquals(UOMPackTypesList.Codes.Pallet, uomAvailableInventoryPLT.UOMType);

			var uomAvailableInventoryUNT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().Single(i => i.PackQuantityUQ == "UNT");
			AssertNotNull(uomAvailableInventoryUNT);
			AssertEquals("Should be 2 UNT", 2m, uomAvailableInventoryUNT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryUNT.StockKeepingUnit);
			AssertEquals(2m, uomAvailableInventoryUNT.StockUnitQuantity);
			AssertEquals(UOMPackTypesList.Codes.SplitCase, uomAvailableInventoryUNT.UOMType);
		}

		#endregion

		#region TestUOMInventoryCreatedFromPickLinesAndUnitConversion_SplitAfterPicking

		public void TestUOMInventoryCreatedFromPickLinesAndUnitConversion_SplitAfterPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var refPackTypes = new RefPackTypeCollection(Factory);
			refPackTypes.Single(r => r.F3_Code == "UNT").F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: 1 pickline created", 1, orderLine.PickLines.Count);

			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_F3_NKAllocatedPackType = "UNT";

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories.Count);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var splitPickLine = pickLine.Split(2m);

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.NeedsRefresh = true;
			uomAvalilableInventoryCollection.RebuildCollection();
			AssertEquals("Collection has been rebuilt, NeedsRefresh should be false.", false, uomAvalilableInventoryCollection.NeedsRefresh);

			AssertEquals(1, uomAvalilableInventoryCollection.Count);

			var uomAvailableInventoryUNT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().Single(i => i.PackQuantityUQ == "UNT");
			AssertNotNull(uomAvailableInventoryUNT);
			AssertEquals("Should be 5 UNT", 5m, uomAvailableInventoryUNT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryUNT.StockKeepingUnit);
			AssertEquals(5m, uomAvailableInventoryUNT.StockUnitQuantity);
			AssertEquals(UOMPackTypesList.Codes.SplitCase, uomAvailableInventoryUNT.UOMType);
		}

		#endregion

		#region TestUOMInventoryNotCreatedWhenPickLineHasNoPackTypeAssigned

		public void TestUOMInventoryNotCreatedWhenPickLineHasNoPackTypeAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories.Count);
			AssertEquals(1, order.Lines[0].PickLines.Count);
			AssertEquals("", order.Lines[0].PickLines[0].WZ_F3_NKAllocatedPackType);

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.RebuildCollection();

			AssertEquals("As pickLine had no pack type assigned to it - UOM inventory should not be created", 0, uomAvalilableInventoryCollection.Count);
		}

		#endregion

		#region TestPackQtyIsZeroWhenUnitConversionsInvalid

		public void TestPackQtyIsZeroWhenUnitConversionsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 20m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: 2 picklines created", 2, orderLine.PickLines.Count);
			AssertEquals("PLT", orderLine.PickLines.Single(pl => pl.WZ_Units == 20m).WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", orderLine.PickLines.Single(pl => pl.WZ_Units == 5m).WZ_F3_NKAllocatedPackType);

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.RebuildCollection();

			AssertEquals(2, uomAvalilableInventoryCollection.Count);

			var uomAvailableInventoryPLT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "PLT");
			AssertNotNull(uomAvailableInventoryPLT);
			AssertEquals("Should be 1 PLT", 1m, uomAvailableInventoryPLT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryPLT.StockKeepingUnit);
			AssertEquals(20m, uomAvailableInventoryPLT.StockUnitQuantity);

			var uomAvailableInventoryUNT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().Single(i => i.PackQuantityUQ == "UNT");
			AssertNotNull(uomAvailableInventoryUNT);
			AssertEquals("Should be 5 UNT", 5m, uomAvailableInventoryUNT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryUNT.StockKeepingUnit);
			AssertEquals(5m, uomAvailableInventoryUNT.StockUnitQuantity);

			// now lets mess with unit conversions
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "BOX", 9);
			Helper.CreateProductUnit(data.Part1, "PCK", 3);

			// according to this new unit conversions there should be 2 BOX (18 UNT) + 2 PCK (6 UNT) + 1 UNT
			var newUOMCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			newUOMCollection.RebuildCollection();
			AssertEquals("Still 2 items should be created, as there are 2 different pack types assigned to pickLines.", 2, newUOMCollection.Count);

			var newUOMItemForPLT = newUOMCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "PLT");
			AssertNotNull("Event though there is no unit conversion to PLT - pickLine still holds this pack type, so UOMInventory should be created for PLT", newUOMItemForPLT);
			AssertEquals("Pack qty should be reset to 0 as we don't know how to convert 20 UNT to PLT", 0m, newUOMItemForPLT.PackQuantity);
			AssertEquals("UNT", newUOMItemForPLT.StockKeepingUnit);
			AssertEquals(20m, newUOMItemForPLT.StockUnitQuantity);

			var newUOMItemForUNT = newUOMCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "UNT");
			AssertNotNull(newUOMItemForUNT);
			AssertEquals("For stock keeping unit nothing has changed", 5m, newUOMItemForUNT.PackQuantity);
			AssertEquals("UNT", newUOMItemForUNT.StockKeepingUnit);
			AssertEquals(5m, newUOMItemForUNT.StockUnitQuantity);
		}

		#endregion

		#region TestPackQtyRemainsSameEvenWhenNewConversionsAdded

		public void TestPackQtyRemainsSameEvenWhenNewConversionsAdded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "BOX", 10m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: 2 picklines created", 2, orderLine.PickLines.Count);
			AssertEquals("BOX", orderLine.PickLines.Single(pl => pl.WZ_Units == 20m).WZ_F3_NKAllocatedPackType);
			AssertEquals("UNT", orderLine.PickLines.Single(pl => pl.WZ_Units == 5m).WZ_F3_NKAllocatedPackType);

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.RebuildCollection();

			AssertEquals(2, uomAvalilableInventoryCollection.Count);

			var uomAvailableInventoryPLT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "BOX");
			AssertNotNull(uomAvailableInventoryPLT);
			AssertEquals("Should be 2 BOX", 2m, uomAvailableInventoryPLT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryPLT.StockKeepingUnit);
			AssertEquals(20m, uomAvailableInventoryPLT.StockUnitQuantity);

			var uomAvailableInventoryUNT = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().Single(i => i.PackQuantityUQ == "UNT");
			AssertNotNull(uomAvailableInventoryUNT);
			AssertEquals("Should be 5 UNT", 5m, uomAvailableInventoryUNT.PackQuantity);
			AssertEquals("UNT", uomAvailableInventoryUNT.StockKeepingUnit);
			AssertEquals(5m, uomAvailableInventoryUNT.StockUnitQuantity);

			// now lets add new unit conversions
			Helper.CreateProductUnit(data.Part1, "PLT", 20);
			Helper.CreateProductUnit(data.Part1, "CAS", 3);

			// new split might look like 1 PLT (20 UNT) + 1 CAS (3 UNT) + 2 UNT
			// but pack types were already assigned to picklines, so we will see old split, and the old unit conversion is still valid (2 BOX + 5 UNT)
			var newUOMCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			newUOMCollection.RebuildCollection();
			AssertEquals("Still 2 items should be created, as there are 2 different pack types assigned to pickLines", 2, newUOMCollection.Count);

			var newUOMItemForPLT = newUOMCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "BOX");
			AssertNotNull(newUOMItemForPLT);
			AssertEquals("Pack qty is still 2 as we can convert 20 UNT to 2 BOX", 2m, newUOMItemForPLT.PackQuantity);
			AssertEquals("UNT", newUOMItemForPLT.StockKeepingUnit);
			AssertEquals(20m, newUOMItemForPLT.StockUnitQuantity);

			var newUOMItemForUNT = newUOMCollection.Cast<WhsPickAvailableInventorySplitByUOM>().SingleOrDefault(i => i.PackQuantityUQ == "UNT");
			AssertNotNull(newUOMItemForUNT);
			AssertEquals("For stock keeping unit nothing has changed", 5m, newUOMItemForUNT.PackQuantity);
			AssertEquals("UNT", newUOMItemForUNT.StockKeepingUnit);
			AssertEquals(5m, newUOMItemForUNT.StockUnitQuantity);
		}

		#endregion

		#region TestPackQtyGroupedByPackTypeAndPickerAndPickTime

		public void TestPackQtyGroupedByPackTypeAndPickerAndPickTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "BOX", 10m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 35m);
			var pick = Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: 7 picklines created (because of 7 inventories)", 7, orderLine.PickLines.Count);
			var dateOne = ZDateTimeOffset.Today.AddDays(-2);
			var dateTwo = ZDateTimeOffset.Today.AddDays(-1);

			var pickerAAA = Helper.CreateGlbStaff("AAA", "Picker AAA");
			var pickerBBB = Helper.CreateGlbStaff("BBB", "Picker BBB");

			// Picker AAA, PickDate=dateOne,  1 BOX (10 UNT) + 5 UNT
			orderLine.PickLines[0].WZ_F3_NKAllocatedPackType = "BOX";
			orderLine.PickLines[0].WZ_PickedDateTime = dateOne;
			orderLine.PickLines[0].WZ_GS_NKAssignedTo = "AAA";

			orderLine.PickLines[1].WZ_F3_NKAllocatedPackType = "BOX";
			orderLine.PickLines[1].WZ_PickedDateTime = dateOne;
			orderLine.PickLines[1].WZ_GS_NKAssignedTo = "AAA";

			orderLine.PickLines[2].WZ_F3_NKAllocatedPackType = "UNT";
			orderLine.PickLines[2].WZ_PickedDateTime = dateOne;
			orderLine.PickLines[2].WZ_GS_NKAssignedTo = "AAA";

			// Picker BBB, PickDate=dateOne,  1 BOX (10 UNT) + 5 UNT
			orderLine.PickLines[3].WZ_F3_NKAllocatedPackType = "BOX";
			orderLine.PickLines[3].WZ_PickedDateTime = dateOne;
			orderLine.PickLines[3].WZ_GS_NKAssignedTo = "BBB";

			orderLine.PickLines[4].WZ_F3_NKAllocatedPackType = "BOX";
			orderLine.PickLines[4].WZ_PickedDateTime = dateOne;
			orderLine.PickLines[4].WZ_GS_NKAssignedTo = "BBB";

			orderLine.PickLines[5].WZ_F3_NKAllocatedPackType = "UNT";
			orderLine.PickLines[5].WZ_PickedDateTime = dateOne;
			orderLine.PickLines[5].WZ_GS_NKAssignedTo = "BBB";

			// Picker BBB, PickDate=dateTwo,  5 UNT
			orderLine.PickLines[6].WZ_F3_NKAllocatedPackType = "UNT";
			orderLine.PickLines[6].WZ_PickedDateTime = dateTwo;
			orderLine.PickLines[6].WZ_GS_NKAssignedTo = "BBB";

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.RebuildCollection();

			AssertUOMCollectionContainsElement("Should be line for 1 BOX (10 UNT) picked by AAA at " + dateOne.ToShortDateString(), uomAvalilableInventoryCollection,
				1m, "BOX", 10m, "UNT", pickerAAA, dateOne);

			AssertUOMCollectionContainsElement("Should be line for 5 UNT (5 UNT) picked by AAA at " + dateOne.ToShortDateString(), uomAvalilableInventoryCollection,
				5m, "UNT", 5m, "UNT", pickerAAA, dateOne);

			AssertUOMCollectionContainsElement("Should be line for 1 BOX (10 UNT) picked by BBB at " + dateOne.ToShortDateString(), uomAvalilableInventoryCollection,
				1m, "BOX", 10m, "UNT", pickerBBB, dateOne);

			AssertUOMCollectionContainsElement("Should be line for 5 UNT (5 UNT) picked by BBB at " + dateOne.ToShortDateString(), uomAvalilableInventoryCollection,
				5m, "UNT", 5m, "UNT", pickerBBB, dateOne);

			AssertUOMCollectionContainsElement("Should be line for 5 UNT (5 UNT) picked by BBB at " + dateTwo.ToShortDateString(), uomAvalilableInventoryCollection,
				5m, "UNT", 5m, "UNT", pickerBBB, dateTwo);

			AssertEquals(5, uomAvalilableInventoryCollection.Count);
		}

		void AssertUOMCollectionContainsElement(ZString message, WhsPickAvailableInventorySplitByUOMCollection collection, ZDecimal packQty, ZString packUQ, ZDecimal stockUnitQty, ZString stockUQ, GlbStaff picker, ZDateTimeOffset pickedDate)
		{
			AssertNotNull(message, collection.Cast<WhsPickAvailableInventorySplitByUOM>().FirstOrDefault(
				x => x.PackQuantity == packQty
					 && x.PackQuantityUQ == packUQ
					 && x.StockUnitQuantity == stockUnitQty
					 && x.StockKeepingUnit == stockUQ
					 && x.AssignedToPK == picker.PK
					 && x.PickedDate == pickedDate));
		}

		#endregion

		#region TestWorkOrderPickCanLoadWorkflowItems

		public void TestWorkOrderPickCanLoadWorkflowItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 50m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1");
			Helper.CreateWhsWorkOrderLine(workOrder1, bomProduct, 5m);
			Factory.Save();

			Helper.CreatePickNew(workOrder1);
			Factory.Save();

			AssertNoExceptionThrown(() => { var count = workOrder1.WorkflowItems.Count; }); //Picking Slip print dialog box access WorkflowItems.
		}

		#endregion

		#region TestRebuildCollection_ShouldGroupByOrder

		public void TestRebuildCollection_ShouldGroupByOrder()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 24m);
			var orderLine12 = Helper.CreateWhsOrderLine(order1, data.Part1, 8m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, data.Part1, 12m);
			var orderLine22 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);

			var pick = Helper.CreatePickNew(order1, order2);

			AssertEquals("Precondition: Expecting 4 pick lines created", 4, pick.GetAllPickLines().Count());

			orderLine11.PickLines[0].WZ_F3_NKAllocatedPackType = "CTN";
			orderLine12.PickLines[0].WZ_F3_NKAllocatedPackType = "UNT";
			orderLine21.PickLines[0].WZ_F3_NKAllocatedPackType = "CTN";
			orderLine22.PickLines[0].WZ_F3_NKAllocatedPackType = "UNT";

			// Act

			var uomAvalilableInventoryCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			uomAvalilableInventoryCollection.RebuildCollection();

			// Assert

			var uomAvailableInventories = uomAvalilableInventoryCollection.Cast<WhsPickAvailableInventorySplitByUOM>().ToArray();
			AssertEquals("Expecting 4 available inventories for both orders", 4, uomAvailableInventories.Length);
			AssertEquals("Expecting 2 available inventories for Order 1", 2, uomAvailableInventories.Count(inventory => inventory.OrderReference == order1.WD_ExternalReference));
			AssertEquals("Expecting 2 available inventories for Order 2", 2, uomAvailableInventories.Count(inventory => inventory.OrderReference == order2.WD_ExternalReference));
		}

		#endregion

		#endregion

		#region Implementation

		protected override WhsPickAvailableInventorySplitByUOMCollection GetNewCollection(WhsPickAvailableInventory availableInventory)
		{
			return new WhsPickAvailableInventorySplitByUOMCollection(Factory, availableInventory);
		}

		protected override WhsPickAvailableInventorySplitByUOMCollection GetCollectionToTest()
		{
			var availableInventory = new WhsPickAvailableInventory(Factory);
			return new WhsPickAvailableInventorySplitByUOMCollection(Factory, availableInventory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsPickAvailableInventorySplitByUOM(Factory);
		}

		#endregion
	}
}
