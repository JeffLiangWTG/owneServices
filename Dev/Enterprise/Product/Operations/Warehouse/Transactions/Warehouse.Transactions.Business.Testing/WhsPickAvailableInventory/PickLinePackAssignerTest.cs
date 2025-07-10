using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PickLinePackAssignerTest : WhsTestCaseWithFactory
	{
		#region TestAssignPackTypes

		public void TestAssignPackTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 236m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			// as PickByUOM is off - only 1 pickline will be created
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.GetAllPickLines().Count());
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals(236m, pickline.WZ_Units);
			AssertEquals(236m, pickline.WZ_OriginalReservedQty);
			AssertEquals("", pickline.WZ_F3_NKAllocatedPackType);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(236m, availableInventory.PickLineQuantity);

			// now let's assign pack types
			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 200 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 30 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 6 && pl.WZ_F3_NKAllocatedPackType == "UNT"));
			AssertEquals(3, pick.GetAllPickLines().Count());
			AssertEquals(236m, availableInventory.PickLineQuantity);

			// now let's imagine that pick line for 200 units was short picked to 145.
			var pickLinePLT = order.Lines[0].PickLines.Single(pl => pl.WZ_Units == 200 && pl.WZ_F3_NKAllocatedPackType == "PLT");
			pickLinePLT.WZ_Units = 145;
			PickLinePackAssigner.AssignPackTypes(new[] { pickLinePLT }, pick.ForceWhsPickUOMTypeAllocation);

			// 30 for BOX and 6 for UNT remains untouched
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 30 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 6 && pl.WZ_F3_NKAllocatedPackType == "UNT"));

			// but PLT pickline is splitted:
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 100 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 40 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 5 && pl.WZ_F3_NKAllocatedPackType == "UNT"));

			AssertEquals(5, pick.GetAllPickLines().Count());
			AssertEquals(181m, availableInventory.PickLineQuantity);
		}

		#endregion

		#region TestAssignPackTypesShouldReturnAllPickLinesIncludeSplitLines

		public void TestAssignPackTypesShouldReturnAllPickLinesIncludeSplitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 254m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			// as PickByUOM is off - only 1 pickline will be created
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.GetAllPickLines().Count());
			AssertEquals(254m, pick.GetAllPickLines().Single().WZ_Units);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(254m, availableInventory.PickLineQuantity);

			// now let's assign pack types
			var pickLinesIncludeSplitLines = PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertEquals(3, pickLinesIncludeSplitLines.Count);
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 200 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 50 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 4 && pl.WZ_F3_NKAllocatedPackType == "UNT"));
			AssertEquals(3, pick.GetAllPickLines().Count());
		}

		#endregion

		#region TestAssignPackTypesShouldReturnAllPickLinesIncludeSplitLines_MoreThanOneLine

		public void TestAssignPackTypesShouldReturnAllPickLinesIncludeSplitLines_MoreThanOnePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 113);
			order1.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 11);
			order2.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			// as PickByUOM is off - only 1 pickline will be created
			var pick = Helper.CreatePickNew(order1, order2);

			AssertEquals(2, pick.GetAllPickLines().Count());
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(124m, availableInventory.PickLineQuantity);

			// now let's assign pack types
			var pickLinesIncludeSplitLines = PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 100 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 13 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 7 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 4 && pl.WZ_F3_NKAllocatedPackType == "UNT"));
		}

		#endregion

		#region TestAssignPackTypesShouldReturnAllPickLinesIncludeSplitLines_IfSplitRequired

		public void TestAssignPackTypesShouldReturnAllPickLinesIncludeSplitLines_IfSplitRequired()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 98);
			order1.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			order2.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			order3.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			// as PickByUOM is off - only 1 pickline will be created
			var pick = Helper.CreatePickNew(order1, order2, order3);

			AssertEquals(3, pick.GetAllPickLines().Count());
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(110m, availableInventory.PickLineQuantity);

			// now let's assign pack types
			var pickLinesIncludeSplitLines = PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 98 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 10 && pl.WZ_F3_NKAllocatedPackType == "BOX"));
			AssertNotNull(pickLinesIncludeSplitLines.SingleOrDefault(pl => pl.WZ_Units == 2 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertEquals(3, pickLinesIncludeSplitLines.Count);
		}

		#endregion

		#region TestAssignPackTypes_EmptyArray

		public void TestAssignPackTypes_EmptyArray()
		{
			AssertNoExceptionThrown(() => PickLinePackAssigner.AssignPackTypes(Array.Empty<WhsPickLine>(), ForceWhsPickUOMTypeAllocation.None));
		}

		#endregion

		#region TestAssignPackTypes_DifferentProducts

		public void TestAssignPackTypes_DifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			AssertExceptionThrown<InvalidOperationException>("Exception should be thrown as Pack Assigner uses product unit conversions to deal with pick lines, so there must be only 1 product for the pickLines provided",
				() => PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation));
		}

		#endregion

		#region TestAssignPackTypes_WithReleaseCapturedAttribs

		public void TestAssignPackTypes_WithReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			// PickByUOM is disabled, therefore only 1 pickline will be created
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 236m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition: Order Line is Fully Picked.", 236m, pick.GetAllPickLines().Single().WZ_Units);
			AssertEquals("Precondition: UOM Pack Type is empty on PickLine.", "", pick.GetAllPickLines().Single().WZ_F3_NKAllocatedPackType);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Picked Stock is all on the same Inventory.", 236m, availableInventory.PickLineQuantity);

			var orderLine = order.Lines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("PickLine should be fully release Captured.", 0m, pick.GetAllPickLines().Single().UnreleaseCapturedQty);

			Factory.Save();

			// now let's assign pack types
			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);
			AssertEquals("Should have split Pick Line into three; PLT, BOX & UNT.", 3, pick.GetAllPickLines().Count());
			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 200 && pl.WZ_F3_NKAllocatedPackType == "PLT"
				&& pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 30 && pl.WZ_F3_NKAllocatedPackType == "BOX"
				&& pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("Release Captured Attributes should be split correctly amongst new PickLines.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 6 && pl.WZ_F3_NKAllocatedPackType == "UNT"
				&& pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertEquals("Order is still Fully Picked.", 236m, availableInventory.PickLineQuantity);

			// make sure Data is correct and validates and saves without issue
			releaseLine.Validation.ValidateAll();
			AssertNoErrors(releaseLine);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestAssignPackTypes_WithReleaseCapturedAttribs_NotFullyReleased

		public void TestAssignPackTypes_WithReleaseCapturedAttribs_NotFullyReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 236m);
			// as PickByUOM is off - only 1 pickline will be created
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition: Order Line is Fully Picked.", 236m, pick.GetAllPickLines().Single().WZ_Units);
			AssertEquals("Precondition: UOM Pack Type is empty on PickLine.", "", pick.GetAllPickLines().Single().WZ_F3_NKAllocatedPackType);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Picked Stock is all on the same Inventory.", 236m, availableInventory.PickLineQuantity);

			var orderLine = order.Lines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			releaseLine.Quantity = 210m;
			AssertEquals("PickLine should not be fully release Captured.", 26m, pick.GetAllPickLines().Sum(l => l.UnreleaseCapturedQty));

			Factory.Save();

			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);
			AssertEquals("Should have split Pick Line into four: PLT, BOX, UNT & by RCA.", 4, pick.GetAllPickLines().Count());
			AssertNotNull("200 RED PLT.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 200m && pl.WZ_F3_NKAllocatedPackType == "PLT"
				&& pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("26 BOX", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 26m && pl.WZ_F3_NKAllocatedPackType == "BOX" && pl.WZ_ReleaseCapturedPartAttrib1 == ""));
			AssertNotNull("4 RED BOX, and make a total of 30 BOX.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 4m && pl.WZ_F3_NKAllocatedPackType == "BOX"
				&& pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("6 RED UNT.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 6m && pl.WZ_F3_NKAllocatedPackType == "UNT" && pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertEquals("Order is still Fully Picked.", 236m, availableInventory.PickLineQuantity);

			// make sure Data is correct and validates and saves without issue
			releaseLine.Validation.ValidateAll();
			AssertNoErrors(releaseLine);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestAssignPackTypes_WithMultipleReleaseCapturedAttribs

		public void TestAssignPackTypes_WithMultipleReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 236m);
			// as PickByUOM is off - only 1 pickline will be created
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition: Order Line is Fully Picked.", 236m, pick.GetAllPickLines().Single().WZ_Units);
			AssertEquals("Precondition: UOM Pack Type is empty on PickLine.", "", pick.GetAllPickLines().Single().WZ_F3_NKAllocatedPackType);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Picked Stock is all on the same Inventory.", 236m, availableInventory.PickLineQuantity);

			var orderLine = order.Lines[0];
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.Quantity = 20m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "GREEN";
			releaseLine2.Quantity = 15m;
			AssertEquals("PickLine should not be fully release Captured.", 201m, pick.GetAllPickLines().Sum(pl => pl.UnreleaseCapturedQty));

			Factory.Save();

			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);
			AssertEquals("Should have split Pick Line into five: PLT, BOX, UNT and by RCA.", 5, pick.GetAllPickLines().Count());
			AssertNotNull("One PLT PickLine without Attribs.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 200m && pl.WZ_F3_NKAllocatedPackType == "PLT" && !pl.HasReleaseCapturedAttribs));
			AssertNotNull("One UNT PickLine without Attribs.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 1m && pl.WZ_F3_NKAllocatedPackType == "UNT" && !pl.HasReleaseCapturedAttribs));
			AssertNotNull("One RED BOX PickLine.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 20m && pl.WZ_F3_NKAllocatedPackType == "BOX" && pl.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertNotNull("One GREEN BOX PickLine.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 10m && pl.WZ_F3_NKAllocatedPackType == "BOX" && pl.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));
			AssertNotNull("One GREEN UNT PickLine.", orderLine.PickLines.SingleOrDefault(pl => pl.WZ_Units == 5m && pl.WZ_F3_NKAllocatedPackType == "UNT" && pl.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));
			AssertEquals("Order is still Fully Picked.", 236m, availableInventory.PickLineQuantity);

			// make sure Data is correct and validates and saves without issue
			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestAssignPackTypes_ForceCASAsLargestUOMType

		public void TestAssignPackTypes_ForceCASAsLargestUOMType_SingleOrderLineWithCASConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Part1.Lookups.PackTypes.Find(p => p.F3_Code == "CAS").Single().F3_UOMType = "CAS";
			Helper.CreateProductUnit(data.Part1, "PLT", 50);
			data.Part1.Lookups.PackTypes.Find(p => p.F3_Code == "PLT").Single().F3_UOMType = "PLT";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			var pick = Helper.CreatePickNew();
			pick.WP_ForcePickByCaseUOMTypeAllocation = true;
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			pick.AddOrders(new WhsPickableDocket[] { order });
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			Factory.Save();

			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull("WP_ForcePickByCaseUOMTypeAllocation is true and the product has conversions defined for CAS and PLT; CAS should be used as the UOM type for the pick line.",
				order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 50 && pl.WZ_F3_NKAllocatedPackType == "CAS"));
			AssertEquals(1, pick.GetAllPickLines().Count());
			AssertEquals(50m, availableInventory.PickLineQuantity);
		}

		public void TestAssignPackTypes_ForceCASAsLargestUOMType_SingleOrderLineWithNoCASConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 50);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			var pick = Helper.CreatePickNew();
			pick.WP_ForcePickByCaseUOMTypeAllocation = true;
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			pick.AddOrders(new WhsPickableDocket[] { order });
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			Factory.Save();

			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull("The product has no CAS conversion defined; PLT should be assigned to the pick line, even though WP_ForcePickByCaseUOMTypeAllocation is true.",
				order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 50 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertEquals(1, pick.GetAllPickLines().Count());
			AssertEquals(50m, availableInventory.PickLineQuantity);
		}

		public void TestAssignPackTypes_ForceCASAsLargestUOMType_TwoOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "PLT", 50);
			data.Part1.Lookups.PackTypes.Find(p => p.F3_Code == "CAS").Single().F3_UOMType = "CAS";
			data.Part1.Lookups.PackTypes.Find(p => p.F3_Code == "PLT").Single().F3_UOMType = "PLT";
			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part2, "PLT", 100);
			data.Part2.Lookups.PackTypes.Find(p => p.F3_Code == "PLT").Single().F3_UOMType = "PLT";
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1000m);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 100m);
			orderLine1.ReserveStockIfAbleTo(receive1.Inventory[0]);
			orderLine2.ReserveStockIfAbleTo(receive2.Inventory[0]);
			var pick = Helper.CreatePickNew();
			pick.WP_ForcePickByCaseUOMTypeAllocation = true;
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			pick.AddOrders(new WhsPickableDocket[] { order });
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			Factory.Save();

			PickLinePackAssigner.AssignPackTypes(order.Lines[0].PickLines.ToArray(), pick.ForceWhsPickUOMTypeAllocation);
			PickLinePackAssigner.AssignPackTypes(order.Lines[1].PickLines.ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull("The first order line's pick lines should have CAS UOM pack type assigned to them, as the product has a CAS conversion defined.",
				orderLine1.PickLines.SingleOrDefault(pl => pl.WZ_Units == 50 && pl.WZ_F3_NKAllocatedPackType == "CAS"));
			AssertNotNull("The second order line's pick lines should have PLT UOM pack type assigned to them, as the product has no CAS conversion defined.",
				orderLine2.PickLines.SingleOrDefault(pl => pl.WZ_Units == 100 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertEquals("The pick should have one pick line for each order line; 2 pick lines in total.",
				2, pick.GetAllPickLines().Count());
			AssertEquals(50m, availableInventory1.PickLineQuantity);
			AssertEquals(100m, availableInventory2.PickLineQuantity);
		}

		public void TestAssignPackTypes_ForceCASAsLargestUOMType_OptionDisabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "PLT", 50);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			var pick = Helper.CreatePickNew();
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			pick.AddOrders(new WhsPickableDocket[] { order });
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			Factory.Save();

			PickLinePackAssigner.AssignPackTypes(pick.GetAllPickLines().ToArray(), pick.ForceWhsPickUOMTypeAllocation);

			AssertNotNull("WP_ForcePickByCaseUOMTypeAllocation is false and the product has conversions defined for CAS and PLT; PLT should be used as the UOM type for the pick line.",
				order.Lines[0].PickLines.SingleOrDefault(pl => pl.WZ_Units == 50 && pl.WZ_F3_NKAllocatedPackType == "PLT"));
			AssertEquals(1, pick.GetAllPickLines().Count());
			AssertEquals(50m, availableInventory.PickLineQuantity);
		}

		#endregion
	}
}
