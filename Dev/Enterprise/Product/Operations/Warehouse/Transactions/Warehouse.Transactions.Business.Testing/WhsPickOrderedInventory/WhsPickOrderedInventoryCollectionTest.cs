using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickOrderedInventoryCollection))]
	class WhsPickOrderedInventoryCollectionTest : WhsNonPersistentBusinessObjectCollectionTestCase<WhsPickOrderedInventoryCollection>
	{
		#region Adding / Removing From Order

		#region TestCannotAddDirectlyToCollection

		public void TestCannotAddDirectlyToCollection()
		{
			var pick = Factory.New<WhsPick>();
			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not add Ordered Inventory to the collection manually, use 'AddNewFromOrder()' instead.",
				() => collection.AddNew());
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not add Ordered Inventory to the collection manually, use 'AddNewFromOrder()' instead.",
				() => collection.Add(new WhsPickOrderedInventory(Factory)));
		}

		#endregion

		#region AddNewFromOrder

		public void TestAddNewFromOrder()
		{
			using (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertAddNewFromOrder();
			}
		}

		#region TestAddNewFromOrder_SeparateSameProductAndDifferentClient

		public void TestAddNewFromOrder_SeparateSameProductAndDifferentClient()
		{
			OrgHeader client1 = Helper.CreateClient("CL1");
			OrgHeader client2 = Helper.CreateClient("CL2");
			WhsWarehouse whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			OrgSupplierPart part = Helper.CreateProduct(client1, "P1");
			Helper.CreateProductClientRelationShip(client2, part);

			WhsOrder order1 = Helper.CreateWhsOrderWithOrderLine(client1, whs, part, 10m);
			WhsOrder order2 = Helper.CreateWhsOrderWithOrderLine(client2, whs, part, 10m);

			WhsPick pick = Helper.CreatePickNew(order1, order2);

			AssertEquals("Should be created 2 different OrderedInventories for same product, but different clients", 2, pick.OrderedInventories.Count);
		}

		#endregion

		#region TestAddNewFromOrder_SeparateByMinimumShelfLife

		public void TestAddNewFromOrder_SeparateByMinimumShelfLife()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true);

			var consignee1 = Helper.CreateClient("CONSIGNEE1");
			var consignee2 = Helper.CreateClient("CONSIGNEE2");
			consignee1.MiscServ.OM_MinimumShelfLifeAccepted = 0;
			consignee2.MiscServ.OM_MinimumShelfLifeAccepted = 10;

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order1.ConsigneePK = consignee1.PK;
			var order1Line_WithoutJulianBatchNumber = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order1Line_WithJulianBatchNumber = Helper.CreateWhsOrderLine(order1, data.Part2, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order2.ConsigneePK = consignee2.PK;
			var order2Line_WithoutJulianBatchNumber = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var order2Line_WithJulianBatchNumber = Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("3 Ordered Inventories should be created, 1 for Part1, and 2 for Part2.", 3, pick.OrderedInventories.Count);
			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2 && l.MinimumShelfLife == 0);
			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2 && l.MinimumShelfLife == 10);
		}

		#endregion

		#region TestAddNewFromOrder_SeparateByPackageGroupID

		public void TestAddNewFromOrder_SeparateByPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "KEY-1", "DummyOutward-1", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m, "KEY-1", "DummyOutward-1", "123");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 3m, "KEY-1", "DummyOutward-1", "123");
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, "KEY-1", "DummyOutward-1", "456");
			var pick = Helper.CreatePickNew(order);
			AssertEquals("An ordered inventory line should be created for each package group id.", 3, pick.OrderedInventories.Count);

			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.PackageGroupId == "");
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.PackageGroupId == "123");
			var orderedInventory3 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.PackageGroupId == "456");
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1 }, orderedInventory1.Owners);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine2, orderLine3 }, orderedInventory2.Owners);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine4 }, orderedInventory3.Owners);
		}

		#endregion

		#region TestAddNewFromOrder

		public void TestAddNewFromOrder_UpdatesAvailableInventoryPickLinesRelationship()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year - 1, 12, 1), data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year - 1, 12, 2), data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order1);
			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order1);

			var orderedInventory = collection[0];
			AssertEquals("Precondition: Has both available Inventories.", 2, orderedInventory.AvailableInventories.Count);

			var availableInventory1 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.ArrivalDate == new ZDateTimeOffset(year - 1, 12, 1));
			var availableInventory2 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.ArrivalDate == new ZDateTimeOffset(year - 1, 12, 2));
			AssertEquals("Available Inventory PickLines should be Empty.", 0, availableInventory1.PickLines.Count());
			AssertEquals("Available Inventory PickLines should be Empty.", 0, availableInventory2.PickLines.Count());

			var pickLine1 = orderLine1.PickLines.AddNew();
			pickLine1.WZ_Units = 10m;
			pickLine1.WZ_WE_InventoryLine = availableInventory1.Inventory[0].WI_WE_InDocketLine;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.PickLines);
			AssertEquals("Other Available Inventory PickLines should be Empty.", 0, availableInventory2.PickLines.Count());

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 6m);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			pick.Orders.Add(order2);

			collection.AddNewFromOrder(order2);
			var pickLine2 = orderLine2.PickLines.AddNew();
			pickLine2.WZ_Units = 6m;
			pickLine2.WZ_WE_InventoryLine = availableInventory2.Inventory[0].WI_WE_InDocketLine;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.PickLines);

			var pickLine3 = orderLine2.PickLines.AddNew();
			pickLine3.WZ_Units = 4m;
			pickLine3.WZ_WE_InventoryLine = availableInventory1.Inventory[0].WI_WE_InDocketLine;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine3 }, availableInventory1.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.PickLines);
		}

		#endregion

		#region TestAddNewFromOrder

		public void TestAddNewFromOrder_GroupsByAllocationKey()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			orderLine1.WE_AllocationKey = "123";
			orderLine2.WE_AllocationKey = "456";
			orderLine3.WE_AllocationKey = "123";

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order1);
			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order1);

			AssertEquals("Precondition: Has three Ordered Inventories.", 3, collection.Count);

			var orderedInventory1 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.AllocationKey == "123");
			var orderedInventory2 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.AllocationKey == "456");
			var orderedInventory3 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.AllocationKey == string.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine3 }, orderedInventory1.Owners);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine2 }, orderedInventory2.Owners);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine4 }, orderedInventory3.Owners);
		}

		#endregion

		#region TestAddNewFromOrder_WithPalletIDEntered

		public void TestAddNewFromOrder_WithPalletIDEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			orderLine3.WE_PalletID = "ABC1";

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order1);

			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order1);

			AssertEquals("Precondition: Has two Ordered Inventories.", 2, collection.Count);

			var orderedInventory1 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.PalletIDOrdered == "ABC1");
			var orderedInventory2 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.PalletIDOrdered == string.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine3 }, orderedInventory1.Owners);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine2, orderLine4 }, orderedInventory2.Owners);
		}

		#endregion

		#endregion

		#region RemoveFromOrder

		public void TestRemoveFromOrder()
		{
			using (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertAddNewFromOrder();

				var collection = Order12.Pick.OrderedInventories;
				var orderedInventory = collection[0];
				var availableInventory = orderedInventory.AvailableInventories[0];
				availableInventory.PickLineQuantity = 20m;
				AssertNoErrors(orderedInventory.PickLineQuantityInfo);

				collection.RemoveFromOrder(null);
				collection.RemoveFromOrder(Order22);
				collection.RemoveFromOrder(Order11);
				AssertEquals(6, collection.Count);

				collection.RemoveFromOrder(Order12);
				AssertEquals(0, collection.Count);
			}
		}

		#endregion

		#region TestRemoveFromOrder_UpdatesAvailableInventoryPickLinesRelationship

		public void TestRemoveFromOrder_UpdatesAvailableInventoryPickLinesRelationship()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year - 1, 12, 1), data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year - 1, 12, 2), data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 6m);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			pick.Orders.Add(order1);
			pick.Orders.Add(order2);

			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order1);
			collection.AddNewFromOrder(order2);

			var orderedInventory = collection[0];
			AssertEquals("Precondition: Has both available Inventories.", 2, orderedInventory.AvailableInventories.Count);

			var availableInventory1 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.ArrivalDate == new ZDateTimeOffset(year - 1, 12, 1));
			var availableInventory2 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.ArrivalDate == new ZDateTimeOffset(year - 1, 12, 2));
			AssertEquals("Available Inventory PickLines should be Empty.", 0, availableInventory1.PickLines.Count());
			AssertEquals("Available Inventory PickLines should be Empty.", 0, availableInventory2.PickLines.Count());

			var pickLine1 = orderLine1.PickLines.AddNew();
			var pickLine2 = orderLine2.PickLines.AddNew();
			var pickLine3 = orderLine2.PickLines.AddNew();
			pickLine1.WZ_Units = 10m;
			pickLine2.WZ_Units = 6m;
			pickLine3.WZ_Units = 4m;
			pickLine1.WZ_WE_InventoryLine = availableInventory1.Inventory[0].WI_WE_InDocketLine;
			pickLine2.WZ_WE_InventoryLine = availableInventory2.Inventory[0].WI_WE_InDocketLine;
			pickLine3.WZ_WE_InventoryLine = availableInventory1.Inventory[0].WI_WE_InDocketLine;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine3 }, availableInventory1.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.PickLines);

			collection.RemoveFromOrder(order2);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.PickLines);
			AssertEquals("Available Inventory should no longer have PickLines from removed Orders.", 0, availableInventory2.PickLines.Count());

			collection.RemoveFromOrder(order1);
			AssertEquals("PickLines Collection on Available Inventory should be deactivated.", 0, availableInventory1.PickLines.Count());
			AssertEquals("PickLines Collection on Available Inventory should be deactivated.", 0, availableInventory2.PickLines.Count());

			orderedInventory.Owners.Add(orderLine1);
			orderedInventory.Owners.Add(orderLine2);
			orderedInventory.Owners.Add(orderLine3);
			AssertEquals("PickLines Collection on Available Inventory should be deactivated.", 0, availableInventory1.PickLines.Count());
			AssertEquals("PickLines Collection on Available Inventory should be deactivated.", 0, availableInventory2.PickLines.Count());
		}

		#endregion

		#endregion

		#region TestDeferValidationItemsOnAddOrRemove

		public void TestDeferValidationItemsOnAddOrRemove()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);

			var pickLineQuantityValidationCounter = 0;
			var quantityShortValidationCounter = 0;

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCounter++;
			orderedInventory.QuantityShortInfo.AdditionalValidation += () => quantityShortValidationCounter++;

			pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2"));
			AssertEquals("Precondition: Should have run validation once.", 1, pickLineQuantityValidationCounter);
			AssertEquals("Precondition: Should have run validation once.", 1, quantityShortValidationCounter);

			using (pick.OrderedInventories.DeferValidationItemsOnAddOrRemove())
			{
				pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3"));
				pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4"));
				AssertEquals("Should have *not* have run validation.", 1, pickLineQuantityValidationCounter);
				AssertEquals("Should have *not* have run validation.", 1, quantityShortValidationCounter);
			}

			AssertEquals("Should have run validation after deferring is complete.", 2, pickLineQuantityValidationCounter);
			AssertEquals("Should have run validation after deferring is complete.", 2, quantityShortValidationCounter);
		}

		public void TestDeferValidationItemsOnAddOrRemove_Nested()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);

			var pickLineQuantityValidationCounter = 0;
			var quantityShortValidationCounter = 0;

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCounter++;
			orderedInventory.QuantityShortInfo.AdditionalValidation += () => quantityShortValidationCounter++;

			pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2"));
			AssertEquals("Precondition: Should have run validation once.", 1, pickLineQuantityValidationCounter);
			AssertEquals("Precondition: Should have run validation once.", 1, quantityShortValidationCounter);

			using (pick.OrderedInventories.DeferValidationItemsOnAddOrRemove())
			{
				using (pick.OrderedInventories.DeferValidationItemsOnAddOrRemove())
				{
					pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3"));
				}

				AssertEquals("Should have *not* have run validation.", 1, pickLineQuantityValidationCounter);
				AssertEquals("Should have *not* have run validation.", 1, quantityShortValidationCounter);

				pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4"));
				AssertEquals("Should have *not* have run validation.", 1, pickLineQuantityValidationCounter);
				AssertEquals("Should have *not* have run validation.", 1, quantityShortValidationCounter);
			}

			AssertEquals("Should have run validation after deferring is complete.", 2, pickLineQuantityValidationCounter);
			AssertEquals("Should have run validation after deferring is complete.", 2, quantityShortValidationCounter);
		}

		public void TestDeferValidationItemsOnAddOrRemove_DoesNotRunValidationIfNotNecessary()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);

			var pickLineQuantityValidationCounter = 0;
			var quantityShortValidationCounter = 0;

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCounter++;
			orderedInventory.QuantityShortInfo.AdditionalValidation += () => quantityShortValidationCounter++;

			pick.Orders.Add(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2"));
			AssertEquals("Precondition: Should have run validation once.", 1, pickLineQuantityValidationCounter);
			AssertEquals("Precondition: Should have run validation once.", 1, quantityShortValidationCounter);

			using (pick.OrderedInventories.DeferValidationItemsOnAddOrRemove())
			{
			}

			AssertEquals("Should *not* have run validation after deferring is complete if validation was not attempted.", 1, pickLineQuantityValidationCounter);
			AssertEquals("Should *not* have run validation after deferring is complete if validation was not attempted.", 1, quantityShortValidationCounter);
		}

		#endregion

		#region TestGetOrderedInventoryForLine

		public void TestGetOrderedInventoryForLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);
			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order);

			var orderedInventoryWithPart1 = collection.GetOrderedInventoryForLine(orderLine1);
			AssertEquals("Correct Ordered Inventory was returned.", data.Part1, orderedInventoryWithPart1.SupplierPart);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1 }, orderedInventoryWithPart1.Owners);

			var orderedInventoryWithPart2 = collection.GetOrderedInventoryForLine(orderLine2);
			AssertEquals("Correct Ordered Inventory was returned.", data.Part2, orderedInventoryWithPart2.SupplierPart);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine2 }, orderedInventoryWithPart2.Owners);
		}

		#endregion

		#region Merging

		#region TestOrderedInventoryMergeChildLinesConsideringParent

		public void TestOrderedInventoryMergeChildLinesConsideringParent()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct1 = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct1.OP_IsComponentPickedOnSalesOrder = true;
			var mainProduct2 = Helper.CreateProduct(data.Org1, "MP2");
			mainProduct2.OP_IsComponentPickedOnSalesOrder = true;

			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct11 = Helper.CreateProductBOM(mainProduct1, bomComponentProduct1, 3m, Constants.PkgUnit.Unit); //So 3 bomComponentProduct can create on mainProduct
			var bomPartForMainProduct12 = Helper.CreateProductBOM(mainProduct2, bomComponentProduct1, 4m, Constants.PkgUnit.Unit); //So 4 bomComponentProduct can create on mainProduct

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct21 = Helper.CreateProductBOM(mainProduct1, bomComponentProduct2, 1m, Constants.PkgUnit.Unit); //So 1 bomComponentProduct can create on mainProduct
			var bomPartForMainProduct22 = Helper.CreateProductBOM(mainProduct2, bomComponentProduct2, 1m, Constants.PkgUnit.Unit); //So 1 bomComponentProduct can create on mainProduct

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct1, 10m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct2, 10m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 40m, inventoryLocation);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 40m, inventoryLocation);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, mainProduct1, 15m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, mainProduct2, 15m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			AssertEquals("1 order should be added to Pick", 1, pick.Orders.Count);
			AssertEquals("0 lines should be added to order; component lines added after allocation", 2, pick.Orders[0].Lines.Count);

			AssertOrderedInventory(pick, mainProduct1, 15m, 10m);
			AssertOrderedInventory(pick, mainProduct2, 15m, 10m);
		}

		void AssertOrderedInventory(WhsPick pick, OrgSupplierPart product, ZDecimal quantityOrdered, ZDecimal pickLineQuantity)
		{
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(i => i.SupplierPart.PK == product.PK);
			orderedInventory.AvailableInventories[0].Allocate = true;
			AssertEquals("Precondition", quantityOrdered, orderedInventory.QuantityOrdered);
			AssertEquals("Precondition", pickLineQuantity, orderedInventory.PickLineQuantity);
		}

		#endregion

		void AssertAddNewFromOrder()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org1 = Helper.CreateClient("CL1");
			var org2 = Helper.CreateClient("CL2");
			var part11 = Helper.CreateProduct(org1, "P11");
			var part12 = Helper.CreateProduct(org1, "P12");
			var part21 = Helper.CreateProduct(org1, "P21");

			var receive = Helper.CreateWhsReceive(org1, whs, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part11, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			Order11 = Helper.CreateWhsOrder(org1, whs, "11");
			Helper.CreateWhsOrderLine(Order11, part11, 10m);
			pick.Orders.Add(Order11);

			var collection = pick.OrderedInventories;
			AssertEquals(1, collection.Count);
			AssertOrderedInventory(collection[0], "CL1", part11, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "", "", "");

			Order12 = Helper.CreateWhsOrder(org1, whs, "12");
			Helper.CreateWhsOrderLine(Order12, part11, 15m);
			Helper.CreateWhsOrderLine(Order12, part12, 20m);
			Helper.CreateWhsOrderLine(Order12, part12, 25m);
			var orderLine4 = Helper.CreateWhsOrderLine(Order12, part12, 15m);
			var orderLine5 = Helper.CreateWhsOrderLine(Order12, part12, 20m);
			var orderLine6 = Helper.CreateWhsOrderLine(Order12, part12, 30m);
			var orderLine7 = Helper.CreateWhsOrderLine(Order12, part12, 40m);
			var orderLine8 = Helper.CreateWhsOrderLine(Order12, part12, 50m);
			var orderLine9 = Helper.CreateWhsOrderLine(Order12, part12, 60m);

			var today = ZDate.Today;
			Helper.SetDocketLineAttributes(orderLine4, today.AddDays(1), today.AddDays(-1), "PA1", "PA2", "PA3", "SN3", "BEK-1");
			Helper.SetDocketLineAttributes(orderLine5, today.AddDays(1), today.AddDays(-1), "PA1", "PA2", "PA3", "SN3", "BEK-1");
			Helper.SetDocketLineAttributes(orderLine6, today.AddDays(1), today.AddDays(-1), "PA1", "PA21", "PA3", "SN3", "BEK-1");
			Helper.SetDocketLineCustomAttributes(orderLine7, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
			Helper.SetDocketLineCustomAttributes(orderLine8, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
			Helper.SetDocketLineCustomAttributes(orderLine9, "CA1", "CA2", "CA31", "CA4", "CA5", "CA6", 0m, 0m, 5m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");

			pick.Orders.Add(Order12);

			AssertEquals(6, collection.Count);
			collection.Sort(WhsPickOrderedInventory.Schema.QuantityOrdered);
			AssertOrderedInventory(collection[0], "CL1", part11, 25m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "", "", "");
			AssertOrderedInventory(collection[1], "CL1", part12, 30m, today.AddDays(1), today.AddDays(-1), "BEK-1", "PA1", "PA21", "PA3", "SN3", "", "", "");
			AssertEquals("Serial Number Correct Line 2", "SN3", collection[1].SerialNumber);
			AssertOrderedInventory(collection[2], "CL1", part12, 35m, today.AddDays(1), today.AddDays(-1), "BEK-1", "PA1", "PA2", "PA3", "SN3", "", "", "");
			AssertEquals("Serial Number Correct Line 3", "SN3", collection[2].SerialNumber);
			AssertOrderedInventory(collection[3], "CL1", part12, 45m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "", "", "");
			AssertOrderedInventory(collection[4], "CL1", part12, 60m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA31");
			AssertOrderedInventory(collection[5], "CL1", part12, 90m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA3");

			Order13 = Helper.CreateWhsOrder(org1, whs, "13");
			Order14 = Helper.CreateWhsOrder(org1, whs, "14");
			Order21 = Helper.CreateWhsOrder(org2, whs, "21");
			Order22 = Helper.CreateWhsOrder(org2, whs, "22");
		}

		public void TestAddNewFromOrder_GroupsByCustomColumns_RegistryOn()
		{
			TestAddNewFromOrder_GroupsByCustomColumnsCore(registryOn: true);
		}

		public void TestAddNewFromOrder_GroupsByCustomColumns_RegistryOff()
		{
			TestAddNewFromOrder_GroupsByCustomColumnsCore(registryOn: false);
		}

		void TestAddNewFromOrder_GroupsByCustomColumnsCore(bool registryOn)
		{
			using (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryOn))
			{
				var today = ZDate.Today;
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var pick = Factory.New<WhsPick>();
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
				Helper.SetDocketLineCustomAttributes(orderLine1, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				Helper.SetDocketLineCustomAttributes(orderLine2, "CA1", "CA2", "CA88", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
				pick.Orders.Add(order);

				var collection = pick.OrderedInventories;
				collection.Sort(WhsPickOrderedInventory.Schema.QuantityOrdered);

				if (registryOn)
				{
					AssertEquals(2, collection.Count);
					AssertOrderedInventory(collection[0], "111", data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA3");
					AssertOrderedInventory(collection[1], "111", data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA88");
				}
				else
				{
					AssertEquals(1, collection.Count);
					AssertOrderedInventory(collection[0], "111", data.Part1, 15m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA3");
				}
			}
		}

		public void TestAddNewFromOrder_GroupsByCustomColumns_ByBranch()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var branch2 = Helper.CreateGlbBranch("BR2");
			var whs2 = Helper.CreateWarehouse("222", Factory.NewWithValidTestData<OrgAddress>(), branch2);
			Helper.CreateRowAndGenerateLocations(whs2, "B");
			using (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.SetTemporaryValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, true))
			{
				var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m);
				receive1.AllocateLocationsWithMock();
				receive1.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive1);

				var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 100m);
				receive2.AllocateLocationsWithMock();
				receive2.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive2);

				var pick1 = Factory.New<WhsPick>();
				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
				Helper.SetDocketLineCustomAttributes(orderLine1, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
				var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
				Helper.SetDocketLineCustomAttributes(orderLine2, "CA1", "CA2", "CA88", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
				pick1.Orders.Add(order1);

				var collection1 = pick1.OrderedInventories;
				AssertEquals(1, collection1.Count);
				AssertOrderedInventory(collection1[0], "111", data.Part1, 20m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA3");

				var pick2 = Factory.New<WhsPick>();
				var order2 = Helper.CreateWhsOrder(data.Org1, whs2, "O2");
				var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
				Helper.SetDocketLineCustomAttributes(orderLine3, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
				var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
				Helper.SetDocketLineCustomAttributes(orderLine4, "CA1", "CA2", "CA88", "CA4", "CA5", "CA6", 0m, 0m, 0m, 0m, 0m, today, today, today, today, today, true, true, true, true, true, "BLOB");
				pick2.Orders.Add(order2);

				var collection2 = pick2.OrderedInventories;
				AssertEquals(2, collection2.Count);
				collection2.Sort(WhsPickOrderedInventory.Schema.QuantityOrdered);
				AssertOrderedInventory(collection2[0], "111", data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA3");
				AssertOrderedInventory(collection2[1], "111", data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "", "", "CA1", "CA2", "CA88");
			}
		}

		public void TestAddNewFromOrder_GroupsByCustomsOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var customsOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			customsOrder.WD_DocketSubType = OrderType.Codes.Customs;

			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order);
			AssertEquals("Should contain 1 ordered Inventory for the Order.", 1, collection.Count);

			AssertEquals("Should be for the Correct Product.", data.Part1.PK, collection[0].Product.Parent.PK);

			collection.AddNewFromOrder(customsOrder);
			AssertEquals("Should contain 2 ordered Inventories, one for each Order.", 2, collection.Count);
			AssertEquals("Should be for the Correct Product.", data.Part1.PK, collection[0].Product.Parent.PK);
			AssertEquals("Should be for the Correct Product.", data.Part1.PK, collection[1].Product.Parent.PK);
		}

		public void TestAddNewFromOrder_GroupsByInwardProcessingOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);

			var customsOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			customsOrder.WD_DocketSubType = OrderType.Codes.Customs;

			var inwardProcessingOrder1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			inwardProcessingOrder1.WD_IsInwardsProcessingJob = true;
			inwardProcessingOrder1.WD_DocketSubType = OrderType.Codes.Customs;

			var inwardProcessingOrder2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			inwardProcessingOrder2.WD_IsInwardsProcessingJob = true;
			inwardProcessingOrder2.WD_DocketSubType = OrderType.Codes.Customs;

			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order);
			AssertEquals("Should contain 1 ordered Inventory for the Order.", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { order.Lines[0] }, collection[0].Owners);

			collection.AddNewFromOrder(customsOrder);
			AssertEquals("Should contain 2 ordered Inventories, one for each Order.", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { customsOrder.Lines[0] }, collection[1].Owners);

			collection.AddNewFromOrder(inwardProcessingOrder1);
			AssertEquals("Should contain 3 ordered Inventories, one for each Order.", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inwardProcessingOrder1.Lines[0] }, collection[2].Owners);

			collection.AddNewFromOrder(inwardProcessingOrder2);
			AssertEquals("Should contain 3 ordered Inventories", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inwardProcessingOrder1.Lines[0], inwardProcessingOrder2.Lines[0] }, collection[2].Owners);
		}

		public void TestAddNewFromOrder_2ResetEvents()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org1 = Helper.CreateClient("CL1");
			var part11 = Helper.CreateProduct(org1, "P11");
			var receive = Helper.CreateWhsReceive(org1, whs, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part11, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var order11 = Helper.CreateWhsOrder(org1, whs, "11");
			var orderLine11 = Helper.CreateWhsOrderLine(order11, part11, 10m);

			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order11);
			var orderedInventory = (WhsPickOrderedInventory)collection.Single();

			List<ListChangedEventArgs> listChangedEvents = new List<ListChangedEventArgs>();
			((IBindingList)orderedInventory.Owners).ListChanged += delegate(object sender, ListChangedEventArgs e)
			{
				listChangedEvents.Add(e);
			};

			AssertEquals("0 ListChanged events fired", 0, listChangedEvents.Count);

			var order12 = Helper.CreateWhsOrder(org1, whs, "12");
			Helper.CreateWhsOrderLine(order12, part11, 10m);
			Helper.CreateWhsOrderLine(order12, part11, 10m);
			Helper.CreateWhsOrderLine(order12, part11, 10m);
			Helper.CreateWhsOrderLine(order12, part11, 10m);
			Helper.CreateWhsOrderLine(order12, part11, 10m);
			collection.AddNewFromOrder(order12);

			AssertEquals("2 more ListChanged events fired", 2, listChangedEvents.Count);
			AssertEquals(ListChangedType.Reset, listChangedEvents[0].ListChangedType);
			AssertEquals(ListChangedType.Reset, listChangedEvents[1].ListChangedType);
		}

		public void TestAddNewFromOrderLines()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org1 = Helper.CreateClient("CL1");
			var part1 = Helper.CreateProduct(org1, "P1");
			var part2 = Helper.CreateProduct(org1, "P2");
			var receive = Helper.CreateWhsReceive(org1, whs, "1", Notify);
			Helper.CreateWhsReceiveLine(receive, part1, 10m);
			Helper.CreateWhsReceiveLine(receive, part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsOrder(org1, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, part1, 10m);

			var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			collection.AddNewFromOrder(order);
			var orderedInventory = (WhsPickOrderedInventory)collection.Single();

			AssertEquals(part1.OP_PartNum, orderedInventory.ProductCode);
			AssertEquals(10m, orderedInventory.QuantityOrdered);

			var orderLine2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			collection.AddNewFromOrderLines(new[] { orderLine2 });

			AssertEquals(2, collection.Count);
			AssertEquals(10m, collection[0].QuantityOrdered);
			AssertEquals(10m, collection[1].QuantityOrdered);
			AssertEquals(part1.OP_PartNum, collection[0].ProductCode);
			AssertEquals(part2.OP_PartNum, collection[1].ProductCode);
		}

		#region TestAddNewFromOrder_WithSerialNumber

		public void TestAddNewFromOrder_WithSerialNumber_EnableSchemaRedesignChanges()
		{
			TestAddNewFromOrder_WithSerialNumber_Core(schemaRedesign: true);
		}

		public void TestAddNewFromOrder_WithSerialNumber_DisableSchemaRedesignChanges()
		{
			TestAddNewFromOrder_WithSerialNumber_Core(schemaRedesign: false);
		}

		void TestAddNewFromOrder_WithSerialNumber_Core(bool schemaRedesign)
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, schemaRedesign))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
				var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
				orderLine1.WE_SerialNumber = "SN1";

				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(order1);

				var collection = new WhsPickOrderedInventoryCollection(Factory, pick);
				collection.AddNewFromOrder(order1);

				AssertEquals("Should have two ordered inventories because a serial number is specified in first order line.", 2, collection.Count);

				var orderedInventory1 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.SerialNumber.Equals("SN1"));
				var orderedInventory2 = collection.Cast<WhsPickOrderedInventory>().Single(o => o.SerialNumber.Equals(string.Empty));
				AssertContainsExactElementsInAnyOrder(new[] { orderLine1 }, orderedInventory1.Owners);
				AssertContainsExactElementsInAnyOrder(new[] { orderLine2 }, orderedInventory2.Owners);
			}
		}

		#endregion

		#endregion

		#region TestElementGetRemoved_WhenItsOwnersCountChangedToZero

		public void TestElementGetRemoved_WhenItsOwnersCountChangedToZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			AssertEquals("Precondition: OrderedInventory's Count should be 2.", 2, pick.OrderedInventories.Count);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Precondition: OrderedInventory's Count should be 2.", 2, pickInOtherFactory.OrderedInventories.Count);

			orderLine1.Delete();
			Factory.Save();

			AssertEquals("OrderedInventory's Count should be 1.", 1, pick.OrderedInventories.Count);
			AssertEquals("OrderedInventory's Count should be 1.", 1, pickInOtherFactory.OrderedInventories.Count);
		}

		#endregion

		#region NonPersistentBusinessObjectCollection Overrides

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#endregion

		#region TestClearCollectionAndRelatedCache

		public void TestClearCollectionAndRelatedCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Ordered inventory count should be 2.", 2, pick.OrderedInventories.Count);
			AssertNotNull("Precondition", pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
			AssertNotNull("Precondition", pick.OrderedInventories.Cast<WhsPickOrderedInventory>().FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 5));

			pick.OrderedInventories.ClearCollectionAndRelatedCache();
			AssertEquals("Ordered inventory count is 0.", 0, pick.OrderedInventories.Count);
		}

		#endregion

		#region IEnumerable Members

		public void TestIEnumerable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product3 = Helper.CreateProduct("Part3", data.Org1);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);

			var pick = Helper.CreatePickNew(order);
			var orderedInventories = pick.OrderedInventories;
			AssertEquals("Enumeration should find 2 PickOrderedInventory Lines.", 2, orderedInventories.Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find 2 PickOrderedInventory Lines.", 2, ((IEnumerable<BusinessObject>)orderedInventories).Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find 2 PickOrderedInventory Lines.", 2, ((BusinessObjectCollection)orderedInventories).Where(r => r != null).ToArray().Length);

			var orderLine3 = Helper.CreateWhsOrderLine(order, product3, 15m);
			AssertEquals("Enumeration should find 3 PickOrderedInventory Lines.", 3, orderedInventories.Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find 3 PickOrderedInventory Lines.", 3, ((IEnumerable<BusinessObject>)orderedInventories).Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find 3 PickOrderedInventory Lines.", 3, ((BusinessObjectCollection)orderedInventories).Where(r => r != null).ToArray().Length);
		}

		#endregion

		#region Implementation

		void AssertOrderedInventory(
			WhsPickOrderedInventory orderedInventory,
			ZString clientCode,
			OrgSupplierPart part,
			ZDecimal unitsOrdered,
			ZDateTime expiry,
			ZDateTime packing,
			ZString bondedEntryKey,
			ZString partAttrib1,
			ZString partAttrib2,
			ZString partAttrib3,
			ZString serialNumber,
			ZString customAttrib1,
			ZString customAttrib2,
			ZString customAttrib3)
		{
			AssertEquals(clientCode, orderedInventory.ClientCode);
			AssertEquals(part, orderedInventory.SupplierPart);
			AssertEquals(unitsOrdered, orderedInventory.QuantityOrdered);
			AssertEquals(expiry, orderedInventory.ExpiryDate);
			AssertEquals(packing, orderedInventory.PackingDate);
			AssertEquals(bondedEntryKey, orderedInventory.BondedEntryKey);
			AssertEquals(partAttrib1, orderedInventory.PartAttrib1);
			AssertEquals(partAttrib2, orderedInventory.PartAttrib2);
			AssertEquals(partAttrib3, orderedInventory.PartAttrib3);
			AssertEquals(serialNumber, orderedInventory.SerialNumber);
			AssertEquals(customAttrib1, orderedInventory.CustomAttrib1);
			AssertEquals(customAttrib2, orderedInventory.CustomAttrib2);
			AssertEquals(customAttrib3, orderedInventory.CustomAttrib3);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsPickOrderedInventory(Factory);
		}

		protected override WhsPickOrderedInventoryCollection GetCollectionToTest()
		{
			return new WhsPickOrderedInventoryCollection(Factory, Factory.New<WhsPick>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			AllowAddDirectlyToCollectionDisposable = WhsPickOrderedInventoryCollection.SuspendAddingOrderedInventorySempahoreForTesting(Collection);
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (AllowAddDirectlyToCollectionDisposable != null)
			{
				AllowAddDirectlyToCollectionDisposable.Dispose();
			}

			AllowAddDirectlyToCollectionDisposable = null;
		}

		IDisposable AllowAddDirectlyToCollectionDisposable;

		WhsOrder Order11;
		WhsOrder Order12;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in AssertAddNewFromOrder")]
		WhsOrder Order13;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in AssertAddNewFromOrder")]
		WhsOrder Order14;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in AssertAddNewFromOrder")]
		WhsOrder Order21;
		WhsOrder Order22;

		#endregion
	}
}
