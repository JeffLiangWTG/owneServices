using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PickingSlipBuilderTest : WhsTestCaseWithFactory
	{
		#region TestUpdatePickLineQuantity

		public void TestUpdatePickLineQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(50m, null); // 0 + 50
			AssertEquals("We should be able to pick less that ordered.", 50m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less that ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);

			var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(70m, null); // 50 + 70
			AssertEquals("PickLineQuantity could not be greater that Quantity Ordered.", 80m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("When user try to pick more that Quantity Ordered, then we should get quantity that was not picked.", 40m, quantityThatCouldNotBeAllocatedOrDeallocated2);

			orderLine1.WE_TransactionQuantity = 100m;
			var quantityThatCouldNotBeAllocatedOrDeallocated3 = pickSlipBuilder.UpdatePickLineQuantity(-10m, null); // 80 - 10
			AssertEquals("We should be able to pick less that ordered.", 70m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less that ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated3);

			var quantityThatCouldNotBeAllocatedOrDeallocated4 = pickSlipBuilder.UpdatePickLineQuantity(50m, null); // 70 + 50
			AssertEquals("PickLineQuantity could not be greater that what is available in stock.", 100m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("When user try to pick more that Quantity Available, then we should get quantity that was not picked.", 20m, quantityThatCouldNotBeAllocatedOrDeallocated4);
		}

		#endregion

		#region TestUpdatePickLineQuantity_Bom

		public void TestUpdatePickLineQuantity_BomAutoAllocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 5m);
			var kitOrderLine1 = order.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(order, bike, 5m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.ChildComponentLines.Count > 0);

			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var wheelOrdered = orderedInvs.Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);
			var wheelComponentInv = wheelOrdered.AvailableInventories[0];

			using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
			{
				var pickSlipBuilder = new PickingSlipBuilder(wheelComponentInv);
				var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(-6m, null);
				AssertEquals("Semaphore suspended Qty selected should be picked.", 14m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less that ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
				Factory.Save();

				quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(2m, null);
				AssertEquals("Semaphore suspended so updating pickline can occur.", 16m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("Update allowed so picking should occur.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
			}
		}

		public void TestUpdatePickLineQuantity_BomManualAllocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 5m);
			var kitOrderLine1 = order.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(order, bike, 5m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.ChildComponentLines.Count > 0);

			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var wheelOrdered = orderedInvs.Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder);
			var wheelComponentInv = wheelOrdered.AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(wheelComponentInv);
			var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(-6m, null);
			AssertEquals("Semaphore NOT suspended Qty picked should not change.", 20m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No changing pickline if Semaphore NOT suspended", -6m, quantityThatCouldNotBeAllocatedOrDeallocated1);
			Factory.Save();

			quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(-2m, null);
			AssertEquals("Semaphore NOT suspended Qty picked should not change.", 20m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No changing pickline if Semaphore NOT suspended", -2m, quantityThatCouldNotBeAllocatedOrDeallocated1);
		}

		#endregion

		#region TestUpdatePickLineQuantity_WithReservedStock

		public void TestUpdatePickLineQuantity_WithReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held);
			receive2.FinaliseDocketWithoutUserConfirmation();
			Assert(receive2.IsFinalised);
			Factory.Save();

			var reservedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(reservedOrder, data.Part1, 5m);
			AssertNotNull("Precondition - Stock is reserved.", orderLine.ReserveStockIfAbleTo(receive1.Inventory[0]));

			var pickedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(pickedOrder);
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(o => o.AvailableInventories).Single();
			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			AssertEquals("No picklines should be created.", 0, availableInventory.PickLines.Count());

			// ensure new pickline is created for 5 units (does not take reserved stock)
			AssertEquals("Since 5 units are reserved, should not be able to allocate 10 units.", 5m, pickSlipBuilder.UpdatePickLineQuantity(10m, null));
			var pickLine = availableInventory.PickLines.Single();
			AssertEquals("Should only have 5 units picked, as 5 units are reserved.", 5m, pickLine.WZ_Units);

			// ensure existing pickline is set to 5 units / i.e. does not change (does not take reserved stock)
			AssertEquals("Since 5 units are reserved, should not be able to allocate 5 more units.", 5m, pickSlipBuilder.UpdatePickLineQuantity(5m, null));
			AssertEquals("Should only have 5 units picked, as 5 units are reserved.", 5m, pickLine.WZ_Units);
		}

		#endregion

		#region TestUpdatePickLineQuantity_WithDocketLinePkSpecified_InvalidDocketLine_Throws

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_InvalidDocketLine_Throws()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.
			var part2 = Helper.CreateProduct(data.Org1, "P2");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part2, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPartPK == data.Part1.PK).AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			AssertExceptionThrown<ArgumentException>(() => pickSlipBuilder.UpdatePickLineQuantity(1m, orderLine3.PK));
			AssertExceptionThrown<ArgumentException>(() => pickSlipBuilder.UpdatePickLineQuantity(-1m, orderLine3.PK));
		}

		#endregion

		#region TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_NewPickLines

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_NewPickLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(50m, orderLine1.PK);
			AssertEquals("We should be able to pick less that ordered.", 50m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
			AssertEquals("PickLine on line 1 created.", 50m, orderLine1.PickLineQuantity);
			AssertEquals("No PickLines on line 2 created.", 0m, orderLine2.PickLineQuantity);

			pickSlipBuilder.UpdatePickLineQuantity(-50m, null);
			AssertEquals("Precondition: un-allocated.", 0m, pickSlipBuilder.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(60m, orderLine1.PK);
			AssertEquals("We should be able to pick what was ordered.", 60m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
			AssertEquals("PickLine on line 1 created.", 60m, orderLine1.PickLineQuantity);
			AssertEquals("No PickLines on line 2 created.", 0m, orderLine2.PickLineQuantity);

			pickSlipBuilder.UpdatePickLineQuantity(-60m, null);
			AssertEquals("Precondition: un-allocated.", 0m, pickSlipBuilder.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated3 = pickSlipBuilder.UpdatePickLineQuantity(70m, orderLine1.PK);
			AssertEquals("We should only be able to pick order line 1's stock.", 60m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("Should not be able to allocate 10 units.", 10m, quantityThatCouldNotBeAllocatedOrDeallocated3);
			AssertEquals("PickLine on line 1 created.", 60m, orderLine1.PickLineQuantity);
			AssertEquals("No PickLines on line 2 created.", 0m, orderLine2.PickLineQuantity);
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_NewPickLines_LongDecimal()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			pick.RunPreSaveValidation();

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			CombineAssertions(() =>
			{
				var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(40.56489418m, orderLine1.PK);
				AssertEquals("We should be able to pick less that ordered.", 40.565m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
				AssertEquals("PickLine on line 1 created.", 40.565m, orderLine1.PickLineQuantity);
				AssertEquals("No PickLines on line 2 created.", 0m, orderLine2.PickLineQuantity);
				AssertEquals("ReleaseLine Qty was updated correctly.", 40.565m, orderLine1.ReleaseLines[0].Quantity);

				pickSlipBuilder.UpdatePickLineQuantity(-40.56489418m, null);
				AssertEquals("Precondition: un-allocated.", 0m, pickSlipBuilder.PickLineQuantity);

				var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(55.4191918m, orderLine1.PK);
				AssertEquals("We should be able to pick what was ordered.", 55.419m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
				AssertEquals("PickLine on line 1 created.", 55.419m, orderLine1.PickLineQuantity);
				AssertEquals("No PickLines on line 2 created.", 0m, orderLine2.PickLineQuantity);
				AssertEquals("ReleaseLine Qty was updated correctly.", 55.419m, orderLine1.ReleaseLines[0].Quantity);

				pickSlipBuilder.UpdatePickLineQuantity(-55.4191918m, null);
				AssertEquals("Un-allocated.", 0m, pickSlipBuilder.PickLineQuantity);
			});
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_OtherDocketLineHasPickLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			pickSlipBuilder.UpdatePickLineQuantity(10m, orderLine1.PK);
			AssertEquals("Precondition: We should be able to pick less than ordered.", 10m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("Precondition: PickLine on line 1 created.", 10m, orderLine1.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated = pickSlipBuilder.UpdatePickLineQuantity(20m, orderLine2.PK);
			AssertEquals("We should be able to pick less than ordered.", 30m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertEquals("PickLine on line 1 unchanged.", 10m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 created.", 20m, orderLine2.PickLineQuantity);
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_OtherDocketLineHasPickLines_LongDecimal()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			pick.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
				pickSlipBuilder.UpdatePickLineQuantity(10.196418418m, orderLine1.PK);
				AssertEquals("Precondition: We should be able to pick less than ordered.", 10.196m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("Precondition: PickLine on line 1 created.", 10.196m, orderLine1.PickLineQuantity);
				AssertEquals("ReleaseLine Qty was updated correctly.", 10.196m, orderLine1.ReleaseLines[0].Quantity);

				var quantityThatCouldNotBeAllocatedOrDeallocated = pickSlipBuilder.UpdatePickLineQuantity(19.3784183m, orderLine2.PK);
				AssertEquals("We should be able to pick less than ordered.", 29.574m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
				AssertEquals("PickLine on line 1 unchanged.", 10.196m, orderLine1.PickLineQuantity);
				AssertEquals("PickLine on line 2 created.", 19.378m, orderLine2.PickLineQuantity);
				AssertEquals("ReleaseLine Qty was updated correctly.", 19.378m, orderLine2.ReleaseLines[0].Quantity);
			});
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_MultipleAvailableInventory()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.AddDays(-1), data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);

			var availInvs = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availInv1 = availInvs.Single(inv => inv.ArrivalDate == today.AddDays(-1));
			var availInv2 = availInvs.Single(inv => inv.ArrivalDate == today);

			var pickSlipBuilder1 = new PickingSlipBuilder(availInv1);
			pickSlipBuilder1.UpdatePickLineQuantity(3m, order.Lines[0].PK);
			AssertEquals("Precondition: We should be able to pick less than ordered.", 3m, pickSlipBuilder1.PickLineQuantity);
			AssertEquals("Precondition: PickLine on line 1 created.", 3m, pickSlipBuilder1.PickLineQuantity);

			var pickSlipBuilder2 = new PickingSlipBuilder(availInv2);
			var quantityThatCouldNotBeAllocatedOrDeallocated = pickSlipBuilder2.UpdatePickLineQuantity(5m, order.Lines[0].PK);
			AssertEquals("We should be able to pick less than ordered.", 5m, pickSlipBuilder2.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertEquals("PickLine on availInv1 unchanged.", 3m, availInv1.PickLineQuantity);
			AssertEquals("PickLine on availInv2 created.", 5m, availInv2.PickLineQuantity);
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_MultipleAvailableInventory_LongDecimal()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.AddDays(-1), data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			pick.RunPreSaveValidation();

			var availInvs = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availInv1 = availInvs.Single(inv => inv.ArrivalDate == today.AddDays(-1));
			var availInv2 = availInvs.Single(inv => inv.ArrivalDate == today);

			CombineAssertions(() =>
			{
				var pickSlipBuilder1 = new PickingSlipBuilder(availInv1);
				pickSlipBuilder1.UpdatePickLineQuantity(2.16516m, order.Lines[0].PK);
				AssertEquals("Precondition: We should be able to pick less than ordered.", 2.165m, pickSlipBuilder1.PickLineQuantity);
				AssertEquals("Precondition: PickLine on line 1 created.", 2.165m, pickSlipBuilder1.PickLineQuantity);
				AssertEquals("ReleaseLine Qty was updated down correctly.", 2.165m, order.Lines[0].ReleaseLines[0].Quantity);

				var pickSlipBuilder2 = new PickingSlipBuilder(availInv2);
				var quantityThatCouldNotBeAllocatedOrDeallocated = pickSlipBuilder2.UpdatePickLineQuantity(5.5198781m, order.Lines[0].PK);
				AssertEquals("We should be able to pick less than ordered.", 5m, pickSlipBuilder2.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less than ordered.", 0.5198781m, quantityThatCouldNotBeAllocatedOrDeallocated);
				AssertEquals("ReleaseLine Qty was updated correctly.", 7.165m, order.Lines[0].ReleaseLines[0].Quantity);
				AssertEquals("PickLine on availInv1 unchanged.", 2.165m, availInv1.PickLineQuantity);
				AssertEquals("PickLine on availInv2 created.", 5m, availInv2.PickLineQuantity);
			});
		}

		#endregion

		#region TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_ExistingPickLines

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_ExistingPickLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(10m, null);
			AssertEquals("We should be able to pick 10m.", 10m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
			AssertEquals("PickLine on line 1 created.", 5m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 created", 5m, orderLine2.PickLineQuantity);

			orderLine1.WE_TransactionQuantity = 20m;
			orderLine2.WE_TransactionQuantity = 20m;
			var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(10m, orderLine1.PK);
			AssertEquals("We should be able to pick 10m.", 20m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
			AssertEquals("PickLine on line 1 was updated.", 15m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated3 = pickSlipBuilder.UpdatePickLineQuantity(10m, orderLine1.PK);
			AssertEquals("We should be able to pick 5m.", 25m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("5m shortfall expected as we can only allocate from line 1.", 5m, quantityThatCouldNotBeAllocatedOrDeallocated3);
			AssertEquals("PickLine on line 1 was updated.", 20m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_IncrementPickingSlip_ExistingPickLines_HeldInventoryOrder()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				receive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
				var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
				orderLine1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
				orderLine2.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				Factory.Save();

				var pick = Helper.CreatePickNew(order);
				var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

				var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
				var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(10m, null);
				AssertEquals("We should be able to pick 10m.", 10m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("0 not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
				AssertEquals("PickLine on line 1 created.", 5m, orderLine1.PickLines[0].WZ_Units);
				AssertEquals("PickLine on line 2 created", 5m, orderLine2.PickLines[0].WZ_Units);

				orderLine1.WE_TransactionQuantity = 20m;
				orderLine2.WE_TransactionQuantity = 20m;
				var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(10m, orderLine1.PK);
				AssertEquals("We should be able to pick 10m.", 20m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("0 not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
				AssertEquals("PickLine on line 1 was updated.", 15m, orderLine1.PickLines[0].WZ_Units);
				AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLines[0].WZ_Units);

				var quantityThatCouldNotBeAllocatedOrDeallocated3 = pickSlipBuilder.UpdatePickLineQuantity(10m, orderLine1.PK);
				AssertEquals("We should be able to pick 5m.", 25m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("5m shortfall expected as we can only allocate from line 1.", 5m, quantityThatCouldNotBeAllocatedOrDeallocated3);
				AssertEquals("PickLine on line 1 was updated.", 20m, orderLine1.PickLines[0].WZ_Units);
				AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLines[0].WZ_Units);
			}
		}

		#endregion

		#region TestUpdatePickLineQuantity_WithDocketLinePkSpecified_DecrementPickingSlip

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_DecrementPickingSlip()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(10m, null);
			AssertEquals("We should be able to pick 10m.", 10m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
			AssertEquals("PickLine on line 1 created.", 5m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 created", 5m, orderLine2.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(-1m, orderLine1.PK);
			AssertEquals("We should be able to unpick 1m.", 9m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
			AssertEquals("PickLine on line 1 was updated.", 4m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated3 = pickSlipBuilder.UpdatePickLineQuantity(-4m, orderLine1.PK);
			AssertEquals("We should be able to unpick 4m.", 5m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated3);
			AssertEquals("PickLine on line 1 was updated.", 0m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);

			var quantityThatCouldNotBeAllocatedOrDeallocated4 = pickSlipBuilder.UpdatePickLineQuantity(-1m, orderLine1.PK);
			AssertEquals("Cannot unpick from other order lines.", 5m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("Cannot unpick from other order lines.", -1m, quantityThatCouldNotBeAllocatedOrDeallocated4);
			AssertEquals("Pick quantity on line 1 is unchanged.", 0m, orderLine1.PickLineQuantity);
			AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_DecrementPickingSlip_LongDecimal()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			pick.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
				var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(10m, null);
				AssertEquals("We should be able to pick 10m.", 10m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
				AssertEquals("PickLine on line 1 created.", 5m, orderLine1.PickLineQuantity);
				AssertEquals("PickLine on line 2 created", 5m, orderLine2.PickLineQuantity);

				var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(-1.54198181m, orderLine1.PK);
				AssertEquals("We should be able to unpick.", 8.458m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
				AssertEquals("ReleaseLine Qty was updated down correctly.", 3.458m, orderLine1.ReleaseLines[0].Quantity);
				AssertEquals("PickLine on line 1 was updated.", 3.458m, orderLine1.PickLineQuantity);
				AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);

				var quantityThatCouldNotBeAllocatedOrDeallocated3 = pickSlipBuilder.UpdatePickLineQuantity(-3.458m, orderLine1.PK);
				AssertEquals("We should be able to unpick.", 5m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated3);
				AssertEquals("No ReleaseLines should be found.", 0, orderLine1.ReleaseLines.Count);
				AssertEquals("PickLine on line 1 was updated.", 0m, orderLine1.PickLineQuantity);
				AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);

				var quantityThatCouldNotBeAllocatedOrDeallocated4 = pickSlipBuilder.UpdatePickLineQuantity(-1.98871m, orderLine1.PK);
				AssertEquals("Cannot unpick from other order lines.", 5m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("Cannot unpick from other order lines.", -1.98871m, quantityThatCouldNotBeAllocatedOrDeallocated4);
				AssertEquals("No ReleaseLines should be found.", 0, orderLine1.ReleaseLines.Count);
				AssertEquals("Pick quantity on line 1 is unchanged.", 0m, orderLine1.PickLineQuantity);
				AssertEquals("PickLine on line 2 is unchanged", 5m, orderLine2.PickLineQuantity);
			});
		}

		public void TestUpdatePickLineQuantity_WithDocketLinePkSpecified_DecrementPickingSlip_WithMultipleAvailableInventories()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.AddDays(-1), data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availInvs = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availInv1 = availInvs.Single(inv => inv.ArrivalDate == today.AddDays(-1));
			var availInv2 = availInvs.Single(inv => inv.ArrivalDate == today);

			var pickLine1 = receive1.Lines.Single().Inventory[0].CommittedPickLines.Single();
			var pickLine2 = receive2.Lines.Single().Inventory[0].CommittedPickLines.Single();

			var pickSlipBuilder = new PickingSlipBuilder(availInv2);
			var quantityThatCouldNotBeAllocatedOrDeallocated = pickSlipBuilder.UpdatePickLineQuantity(-1m, order.Lines[0].PK);
			AssertEquals("We should be able to unpick 1m.", 4m, pickSlipBuilder.PickLineQuantity);
			AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertEquals("PickLine1 is unchanged", 5m, pickLine1.WZ_Units);
			AssertEquals("PickLine2 was updated.", 4m, pickLine2.WZ_Units);
		}

		#endregion

		#region TestUpdatePickLineQuantity_DecrementPickingSlip_PackLongDecimal

		public void TestUpdatePickLineQuantity_DecrementPickingSlip_PackLongDecimal()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			pick.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
				var quantityThatCouldNotBeAllocatedOrDeallocated1 = pickSlipBuilder.UpdatePickLineQuantity(10m, null);
				AssertEquals("We should be able to pick 10m.", 10m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected if we pick less than ordered.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated1);
				AssertEquals("PickLine on line 1 created.", 10m, orderLine1.PickLineQuantity);

				var quantityThatCouldNotBeAllocatedOrDeallocated2 = pickSlipBuilder.UpdatePickLineQuantity(-9.5883322m, orderLine1.PK);
				AssertEquals("We should be able to unpick.", 0.412m, pickSlipBuilder.PickLineQuantity);
				AssertEquals("No not picked quantity expected.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated2);
				AssertEquals("ReleaseLine Qty was updated down correctly.", 0.412m, orderLine1.ReleaseLines[0].Quantity);
				AssertEquals("PickLine on line 1 was updated.", 0.412m, orderLine1.PickLineQuantity);

				AssertNoExceptionThrown(() => order.PackageJob.AutoPack(new NotificationsForTesting()));
			});
		}

		class NotificationsForTesting : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		#endregion

		#region TestUpdatePickLineQuantity_NewPickLineWhenExistingAreAssigned

		public void TestUpdatePickLineQuantity_NewPickLineWhenExistingAreAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(o => o.AvailableInventories).Single();
			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);
			AssertEquals("1 Pick Line should be created.", 1, availableInventory.PickLines.Count());

			orderLine1.WE_TransactionQuantity = 10m;
			Factory.Save();

			AssertEquals(0m, pickSlipBuilder.UpdatePickLineQuantity(5m, orderLine1.PK));
			AssertEquals("No New Pick Line should be created.", 1, availableInventory.PickLines.Count());
			var pickLine = availableInventory.PickLines.Single();
			AssertEquals("Pick Line has 10 units.", 10m, pickLine.WZ_Units);

			orderLine1.WE_TransactionQuantity = 20m;
			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			pickLine.WZ_GS_NKAssignedTo = activeStaff1.GS_Code;
			Factory.Save();

			AssertEquals(0m, pickSlipBuilder.UpdatePickLineQuantity(10m, orderLine1.PK));
			AssertEquals("1 New Pick Line should be created.", 2, availableInventory.PickLines.Count());
			var pickLines = availableInventory.PickLines.ToArray();
			AssertEquals("first pick line has 10 units.", 10m, pickLines[0].WZ_Units);
			AssertEquals("second pick line has 10 units.", 10m, pickLines[1].WZ_Units);
		}

		#endregion

		#region TestUpdatePickLineQuantity_WhenPickingCommenced

		public void TestUpdatePickLineQuantity_WhenPickingCommenced()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var pickSlipBuilder = new PickingSlipBuilder(availableInventory);

			pickSlipBuilder.UpdatePickLineQuantity(2m, null);
			var pickLine = pick.GetAllPickLines().Single();
			AssertResult(1, 2m, 2m);

			pickLine.WZ_IsPicking = true;
			pickLine.WZ_GS_NKAssignedTo = "A";
			Factory.Save();

			pickSlipBuilder.UpdatePickLineQuantity(4m, null);
			AssertResult(2, 6m, 2m);

			pickSlipBuilder.UpdatePickLineQuantity(-4m, null); // 80 - 10
			AssertResult(1, 2, 2m);

			pickSlipBuilder.UpdatePickLineQuantity(-1m, null); // 80 - 10
			AssertResult(1, 2m, 2m, true);

			void AssertResult(int expectedPickLineCount, decimal expectedPickLineQuantity, decimal expectedUnit, bool hasWarning = false)
			{
				var pickLines = pick.GetAllPickLines().ToList();
				var pickLinesInPicking = pickLines.Where(line => line.PK == pickLine.PK).ToList();
				AssertEquals($"Should be {expectedPickLineCount} pick lines", expectedPickLineCount, pickLines.Count);
				AssertEquals($"PickLineQuantity should be {expectedPickLineQuantity}", expectedPickLineQuantity, availableInventory.PickLineQuantity);
				AssertEquals( 1, pickLinesInPicking.Count);
				AssertEquals($"The quantity of pick line that started Picking should be {expectedUnit}", expectedUnit, pickLinesInPicking[0].WZ_Units);
			}
		}

		#endregion

		#region TestOrderLineAllocation

		[GuiTest]
		public void TestOrderLineAllocation()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();
			data.Part2 = Helper.CreateProduct(data.Org1, "P2");
			data.CreateSimpleInventoryManyLines(data.Whs1, data.Org1, data.Part2, new ZDecimal[] { 10m, 20m, 30m, 40m });
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			WhsOrderLine orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			WhsOrderLine orderLine4 = Helper.CreateWhsOrderLine(order, data.Part2, 30m);

			WhsPick pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals(20m, orderLine1.PickLineQuantity);
			AssertEquals(30m, orderLine2.PickLineQuantity);
			AssertEquals(20m, orderLine3.PickLineQuantity);
			AssertEquals(30m, orderLine4.PickLineQuantity);
		}

		#endregion

		#region TestSplitByPackType

		public void TestSplitByPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 16, data.Whs1.DefaultLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, data.Whs1.DefaultLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var pick = Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			AssertEquals(5, orderLine.PickLines.Count);
			AssertEquals(5, pick.GetAllPickLines().Count());

			AssertEquals(3, inventory1.CommittedPickLines.Count());
			AssertEquals(1, inventory2.CommittedPickLines.Count());
			AssertEquals(1, inventory3.CommittedPickLines.Count());

			var pickLineCAS = orderLine.PickLines.SingleOrDefault(pl => pl.WZ_F3_NKAllocatedPackType == "CAS");
			AssertNotNull(pickLineCAS);
			AssertEquals(10m, pickLineCAS.WZ_Units);
			AssertEquals(inventory1.InDocketLine.PK, pickLineCAS.WZ_WE_InventoryLine);

			var pickLineBOX = orderLine.PickLines.SingleOrDefault(pl => pl.WZ_F3_NKAllocatedPackType == "BOX");
			AssertNotNull(pickLineBOX);
			AssertEquals(5m, pickLineBOX.WZ_Units);
			AssertEquals(inventory1.InDocketLine.PK, pickLineBOX.WZ_WE_InventoryLine);

			var pickLineUNT1 = orderLine.PickLines.SingleOrDefault(pl => pl.WZ_F3_NKAllocatedPackType == "UNT" && pl.WZ_WE_InventoryLine == inventory1.InDocketLine.PK);
			AssertNotNull(pickLineUNT1);
			AssertEquals(1m, pickLineUNT1.WZ_Units);

			var pickLineUNT2 = orderLine.PickLines.SingleOrDefault(pl => pl.WZ_F3_NKAllocatedPackType == "UNT" && pl.WZ_WE_InventoryLine == inventory2.InDocketLine.PK);
			AssertNotNull(pickLineUNT2);
			AssertEquals(1m, pickLineUNT2.WZ_Units);

			var pickLineUNT3 = orderLine.PickLines.SingleOrDefault(pl => pl.WZ_F3_NKAllocatedPackType == "UNT" && pl.WZ_WE_InventoryLine == inventory3.InDocketLine.PK);
			AssertNotNull(pickLineUNT3);
			AssertEquals(1m, pickLineUNT3.WZ_Units);
		}

		public void TestSplitByPackType_ClearsExistingPickLineData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var orderLine = order.Lines.Single();
			var pick = Helper.CreatePickNew(order);

			var boxPickLine = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "BOX");
			boxPickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			boxPickLine.WZ_GS_NKAssignedTo = "ZZZ";

			foreach (var pickLine in orderLine.PickLines.Where(pl => pl != boxPickLine))
			{
				// let's change packtype from all the rest picklines
				pickLine.WZ_F3_NKAllocatedPackType = "AAA";
			}

			pick.OrderedInventories[0].AvailableInventories[0].SplitByPackType();

			// picked information should be retained
			AssertEquals(ZDateTimeOffset.Today, boxPickLine.WZ_PickedDateTime);
			AssertEquals("ZZZ", boxPickLine.WZ_GS_NKAssignedTo);

			// packtypes should be reassigned
			AssertEquals(10m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == "CAS").Sum(pl => pl.WZ_Units));
			AssertEquals(5m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == "BOX").Sum(pl => pl.WZ_Units));
			AssertEquals(3m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == "UNT").Sum(pl => pl.WZ_Units));
		}

		public void TestSplitByPackType_WithInTransitPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var orderLine = order.Lines.Single();
			var pick = Helper.CreatePickNew(order);

			var boxPickLine = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "BOX");
			var transferLine = Helper.PickAndMakeInTransitTransfer(boxPickLine, ZDateTimeOffset.Now);
			var inTransitBoxPickLine = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "BOX");
			inTransitBoxPickLine.WZ_GS_NKAssignedTo = "ZZZ";

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Available Inventory has In-Transit PickLine.", true, availableInventory.PickLines.Contains(inTransitBoxPickLine));

			foreach (var pickLine in orderLine.PickLines.Where(pl => pl != inTransitBoxPickLine))
			{
				// change packtype on the rest of the picklines
				pickLine.WZ_F3_NKAllocatedPackType = "AAA";
			}

			availableInventory.SplitByPackType();

			// picked information should be retained
			AssertNotEquals(ZGuid.Empty, inTransitBoxPickLine.WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("ZZZ", inTransitBoxPickLine.WZ_GS_NKAssignedTo);

			// packtypes should be reassigned
			AssertEquals(10m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == "CAS").Sum(pl => pl.WZ_Units));
			AssertEquals(5m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == "BOX").Sum(pl => pl.WZ_Units));
			AssertEquals(3m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == "UNT").Sum(pl => pl.WZ_Units));
		}

		public void TestSplitByPackType_OrderedInventoryNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 2);
			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			pick.WP_ForcePickByCaseUOMTypeAllocation = true;
			var orderedInventory = new WhsPickOrderedInventory(Factory);
			orderedInventory.Owners.Add(order.Lines[0]);
			var availableInventory = new WhsPickAvailableInventory(Factory);
			((IWhsPickAvailableInventoryInternals)availableInventory).SetAllProperties(orderedInventory, receive.Lines[0].Inventory[0]);

			AssertNull("Precondition - Pick on ordered inventory should be null.", orderedInventory.Pick);
			AssertNoExceptionThrown(() => availableInventory.SplitByPackType());

			var orderLine = order.Lines[0];
			AssertEquals(1, orderLine.PickLines.Count);
			AssertEquals(1, pick.GetAllPickLines().Count());

			var pickLinePLT = orderLine.PickLines.SingleOrDefault(pl => pl.WZ_F3_NKAllocatedPackType == "PLT");
			AssertNotNull(pickLinePLT);
			AssertEquals(10m, pickLinePLT.WZ_Units);
		}

		#endregion

		#region TestSetPivotSelectorPickingPickLine

		public void TestSetPivotSelectorPickingPickLine()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = "SN1";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1);
				var pick = Helper.CreatePickNew(order);

				var pickLine = pick.GetAllPickLines().Single();
				var selector = pick.OrderedInventories[0].AvailableInventories[0].SerialNumbers[0];

				AssertEquals("Precondition", true, selector.Selected);

				selector.SelectSerialNumber(ZGuid.Empty);
				AssertEquals("Should be able to clear.", false, selector.Selected);
				AssertEquals("Should be able to clear.", ZGuid.Empty, pivot.WSV_WZ_PickingLine);

				selector.SelectSerialNumber(pickLine.PK);
				AssertEquals("Should be able to set.", true, selector.Selected);
				AssertEquals("Should be able to set.", pickLine.PK, pivot.WSV_WZ_PickingLine);
			}
		}

		#endregion
	}
}
