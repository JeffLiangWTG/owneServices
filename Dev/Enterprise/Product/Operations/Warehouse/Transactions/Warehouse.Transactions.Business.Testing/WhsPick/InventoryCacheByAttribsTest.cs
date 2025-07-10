using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class InventoryCacheByAttribsTest : WhsTestCaseWithFactory
	{
		#region TestAddInventory

		public void TestAddInventory_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InventoryCacheByAttribs().AddInventory(null));
		}

		public void TestAddInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty,
				"PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty,
				"PA1", "PA2", "PA3", "");
			var cache = new InventoryCacheByAttribs();
			cache.AddInventory(inventory1);
			cache.AddInventory(inventory2);

			var orderLine = Factory.New<WhsOrderLine>();
			orderLine.WE_PartAttrib1 = "PA1";
			AssertEquals("Since the inventory was added without checking attribs, cannot find matching inventory.", 0,
				cache.FindMatchingInventoryByAttribs(orderLine).Count());

			// with no ordered attribs, all inventory should be returned.
			orderLine.WE_PartAttrib1 = "";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory2 },
				cache.FindMatchingInventoryByAttribs(orderLine));
		}

		public void TestAddInventory_OrderedPalletIDOnOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty,
				"PA1", "PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty,
				"PA1", "PA2", "PA3", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty,
				"PA1", "PA2", "PA3", "");
			inventory1.WI_PalletID = "ABC1";
			inventory2.WI_PalletID = "ABC2";
			var cache = new InventoryCacheByAttribs();
			cache.AddInventory(inventory1);
			cache.AddInventory(inventory2);
			cache.AddInventory(inventory3);

			var orderLine = Factory.New<WhsOrderLine>();
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory2, inventory3 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PalletID = "ABC2";
			AssertEquals("Since the inventory was added without checking attribs, cannot find matching inventory.", 0,
				cache.FindMatchingInventoryByAttribs(orderLine).Count());
		}

		#endregion

		#region TestCacheByAttributes

		public void TestCacheByAttributes_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InventoryCacheByAttribs().CacheByAttributes(null));
		}

		public void TestCacheByAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var year = ZDateTime.Today.Year;
			var expiry1 = new ZDate(year, 1, 1);
			var expiry2 = new ZDate(year, 1, 2);
			var packing1 = new ZDate(year, 2, 1);
			var packing2 = new ZDate(year, 2, 2);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry2, packing1, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing2, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PAX",
				"PA2", "PA3", "BEK-1");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PA1",
				"PAX", "PA3", "BEK-1");
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PA1",
				"PA2", "PAX", "BEK-1");
			var inventory7 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PA1",
				"PA2", "PA3", "BEK-2");
			var inventory8 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory9 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "Pa1",
				"pA2", "pa3", "BeK-1");
			inventory1.WI_SerialNumber = "SN1";
			inventory2.WI_SerialNumber = "SN1";
			inventory3.WI_SerialNumber = "SN1";
			inventory4.WI_SerialNumber = "SN1";
			inventory5.WI_SerialNumber = "SN1";
			inventory6.WI_SerialNumber = "SN1";
			inventory7.WI_SerialNumber = "SN1";
			inventory8.WI_SerialNumber = "SN2";
			inventory9.WI_SerialNumber = "sn1";

			var cache = new InventoryCacheByAttribs();
			cache.CacheByAttributes(inventory1);
			cache.CacheByAttributes(inventory2);
			cache.CacheByAttributes(inventory3);
			cache.CacheByAttributes(inventory4);
			cache.CacheByAttributes(inventory5);
			cache.CacheByAttributes(inventory6);
			cache.CacheByAttributes(inventory7);
			cache.CacheByAttributes(inventory8);
			cache.CacheByAttributes(inventory9);

			var orderLine = Factory.New<WhsOrderLine>();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					inventory1, inventory2, inventory3, inventory4, inventory5, inventory6, inventory7, inventory8,
					inventory9
				}, cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_ExpiryDate = expiry1;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					inventory1, inventory3, inventory4, inventory5, inventory6, inventory7, inventory8, inventory9
				}, cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PackingDate = packing1;
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory1, inventory4, inventory5, inventory6, inventory7, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PartAttrib1 = "PA1";
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory1, inventory5, inventory6, inventory7, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PartAttrib2 = "PA2";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory6, inventory7, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PartAttrib3 = "PA3";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory7, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_BondedEntryKey = "BEK-1";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_SerialNumber = "SN1";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));
		}

		public void TestCacheByAttributes_InventoryWithMissingAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var year = ZDateTime.Today.Year;
			var expiry = new ZDate(year, 1, 1);
			var packing = new ZDate(year, 2, 1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry, packing, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, packing, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry, ZDate.Empty, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry, packing, "", "PA2",
				"PA3", "BEK-1");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry, packing, "PA1", "",
				"PA3", "BEK-1");
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry, packing, "PA1",
				"PA2", "", "BEK-1");
			var inventory7 =
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry, packing, "PA1", "PA2", "PA3", "");
			inventory2.WI_SerialNumber = "SN1";
			inventory3.WI_SerialNumber = "SN1";
			inventory4.WI_SerialNumber = "SN1";
			inventory5.WI_SerialNumber = "SN1";
			inventory6.WI_SerialNumber = "SN1";
			inventory7.WI_SerialNumber = "SN1";

			var cache = new InventoryCacheByAttribs();
			cache.CacheByAttributes(inventory1);
			cache.CacheByAttributes(inventory2);
			cache.CacheByAttributes(inventory3);
			cache.CacheByAttributes(inventory4);
			cache.CacheByAttributes(inventory5);
			cache.CacheByAttributes(inventory6);
			cache.CacheByAttributes(inventory7);

			var orderLine = Factory.New<WhsOrderLine>();
			orderLine.WE_ExpiryDate = expiry;
			orderLine.WE_PackingDate = packing;
			orderLine.WE_PartAttrib1 = "PA1";
			orderLine.WE_PartAttrib2 = "PA2";
			orderLine.WE_PartAttrib3 = "PA3";
			orderLine.WE_BondedEntryKey = "BEK-1";
			orderLine.WE_SerialNumber = "SN1";
			AssertEquals("If Inventory Attribute is empty, then it can't match to a non empty ordered attrib.", 0,
				cache.FindMatchingInventoryByAttribs(orderLine).Count());
		}

		public void TestCacheByAttributes_AllocationKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory2.WI_AllocationKey = "ABC";
			inventory3.WI_AllocationKey = "ABC";
			inventory4.WI_AllocationKey = "abc";
			inventory5.WI_AllocationKey = "DEF";

			var cache = new InventoryCacheByAttribs();
			cache.CacheByAttributes(inventory1);
			cache.CacheByAttributes(inventory2);
			cache.CacheByAttributes(inventory3);
			cache.CacheByAttributes(inventory4);
			cache.CacheByAttributes(inventory5);

			var orderLine = Factory.New<WhsOrderLine>();
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory1, inventory2, inventory3, inventory4, inventory5 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_AllocationKey = "ABC";
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory2, inventory3, inventory4 },
				cache.FindMatchingInventoryByAttribs(orderLine));
		}

		public void TestCacheByAttributes_NoMatchingInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var year = ZDateTime.Today.Year;
			var expiry1 = new ZDate(year, 1, 1);
			var expiry2 = new ZDate(year, 1, 2);
			var packing1 = new ZDate(year, 2, 1);
			var packing2 = new ZDate(year, 2, 2);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry1, packing1, "PA1",
				"PA2", "PA3", "BEK-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, expiry2, packing2, "PA1",
				"PA2", "PA3", "BEK-1");
			inventory1.WI_SerialNumber = "SN1";
			inventory2.WI_SerialNumber = "SN1";

			var cache = new InventoryCacheByAttribs();
			cache.CacheByAttributes(inventory1);
			cache.CacheByAttributes(inventory2);

			var orderLine = Factory.New<WhsOrderLine>();
			orderLine.WE_ExpiryDate = expiry1;
			orderLine.WE_PackingDate = packing2;
			orderLine.WE_PartAttrib1 = "PA1";
			orderLine.WE_PartAttrib2 = "PA2";
			orderLine.WE_PartAttrib3 = "PA3";
			orderLine.WE_BondedEntryKey = "BEK-1";
			orderLine.WE_SerialNumber = "SN1";
			AssertEquals(
				"Even though there is Inventory matching Expiry1 and Packing2, there is no inventory with both.", 0,
				cache.FindMatchingInventoryByAttribs(orderLine).Count());
		}

		public void TestCacheByAttributes_OrderedPalletIDOnOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PA2", "PA3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PA2", "PA3", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PA2", "PA3", "");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PAX",
				"PA2", "PA3", "");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PAX", "PA3", "");
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PA2", "PAX", "");
			var inventory7 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PA2", "PA3", "");
			var inventory8 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1",
				"PA2", "PA3", "");
			var inventory9 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "Pa1",
				"pA2", "pa3", "");
			inventory1.WI_SerialNumber = "SN1";
			inventory1.WI_PalletID = "123";
			inventory2.WI_SerialNumber = "SN2";
			inventory2.WI_PalletID = "123";
			inventory3.WI_SerialNumber = "SN3";
			inventory3.WI_PalletID = "123";
			inventory4.WI_SerialNumber = "SN1";
			inventory4.WI_PalletID = "456";
			inventory5.WI_SerialNumber = "SN2";
			inventory5.WI_PalletID = "456";
			inventory6.WI_SerialNumber = "SN3";
			inventory6.WI_PalletID = "456";
			inventory7.WI_SerialNumber = "SN1";
			inventory7.WI_PalletID = "789";
			inventory8.WI_SerialNumber = "SN2";
			inventory8.WI_PalletID = "789";
			inventory9.WI_SerialNumber = "sn3";
			inventory9.WI_PalletID = "789";

			var cache = new InventoryCacheByAttribs();
			cache.CacheByAttributes(inventory1);
			cache.CacheByAttributes(inventory2);
			cache.CacheByAttributes(inventory3);
			cache.CacheByAttributes(inventory4);
			cache.CacheByAttributes(inventory5);
			cache.CacheByAttributes(inventory6);
			cache.CacheByAttributes(inventory7);
			cache.CacheByAttributes(inventory8);
			cache.CacheByAttributes(inventory9);

			var orderLine = Factory.New<WhsOrderLine>();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
				inventory1, inventory2, inventory3, inventory4, inventory5, inventory6, inventory7, inventory8,
				inventory9
				}, cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PartAttrib1 = "PA1";
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory1, inventory2, inventory3, inventory5, inventory6, inventory7, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PalletID = "456";
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory5, inventory6 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PartAttrib2 = "PA2";
			orderLine.WE_PalletID = "";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory2, inventory3, inventory6, inventory7, inventory8, inventory9 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PalletID = "456";
			AssertContainsExactElementsInAnyOrder(new[] { inventory6 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_SerialNumber = "SN1";
			orderLine.WE_PalletID = "";
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory7 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			orderLine.WE_PalletID = "789";
			AssertContainsExactElementsInAnyOrder(new[] { inventory7 },
				cache.FindMatchingInventoryByAttribs(orderLine));
		}

		#endregion

		#region TestCacheByAttributes_HoldCodes

		public void TestCacheByAttributes_HoldCodes()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders (Must order held inventory before roll up)
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");

				var pallet1 = "PLT01";
				var pallet2 = "PLT02";

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, pallet1, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, pallet1, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				var receiveLine7 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				var receiveLine8 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				var receiveLine9 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, ZString.Empty, InventoryStatus.Codes.Available);
				var receiveLine10 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, pallet2, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				var receiveLine11 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, pallet2, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);

				var heldInventory1 = receiveLine1.Inventory[0];
				var heldInventory2 = receiveLine2.Inventory[0];
				var heldInventory3 = receiveLine3.Inventory[0];
				var heldInventory4 = receiveLine4.Inventory[0];
				var heldInventory5 = receiveLine5.Inventory[0];
				var heldInventory6 = receiveLine6.Inventory[0];
				var heldInventory7 = receiveLine7.Inventory[0];
				var heldInventory8 = receiveLine8.Inventory[0];
				var availableInventory9 = receiveLine9.Inventory[0];
				var heldInventory10 = receiveLine10.Inventory[0];
				var heldInventory11 = receiveLine11.Inventory[0];

				heldInventory2.WI_PartAttrib1 = "Red";
				heldInventory6.WI_PartAttrib1 = "Red";

				heldInventory3.WI_PartAttrib2 = "Large";
				heldInventory7.WI_PartAttrib2 = "Large";

				heldInventory4.WI_PartAttrib3 = "Hot";
				heldInventory8.WI_PartAttrib3 = "Hot";

				heldInventory10.WI_PartAttrib1 = "Blue";
				heldInventory11.WI_PartAttrib1 = "Blue";
				heldInventory10.WI_PartAttrib2 = "Small";
				heldInventory11.WI_PartAttrib2 = "Small";
				heldInventory10.WI_PartAttrib3 = "Cold";
				heldInventory11.WI_PartAttrib3 = "Cold";

				var cache = new InventoryCacheByAttribs();
				cache.CacheByAttributes(heldInventory1);
				cache.CacheByAttributes(heldInventory2);
				cache.CacheByAttributes(heldInventory3);
				cache.CacheByAttributes(heldInventory4);
				cache.CacheByAttributes(heldInventory5);
				cache.CacheByAttributes(heldInventory6);
				cache.CacheByAttributes(heldInventory7);
				cache.CacheByAttributes(heldInventory8);
				cache.CacheByAttributes(availableInventory9);
				cache.CacheByAttributes(heldInventory10);
				cache.CacheByAttributes(heldInventory11);

				var orderLine = Factory.New<WhsOrderLine>();
				orderLine.WE_WHC_NKOrderedHeldCode = ZString.Empty; // Baseline, Picks will never exist for mixed inventory types
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory1, heldInventory2, heldInventory3, heldInventory4, heldInventory5, heldInventory6, heldInventory7, heldInventory8, availableInventory9, heldInventory10, heldInventory11 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory1, heldInventory2, heldInventory3, heldInventory4, heldInventory10, heldInventory11 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory5, heldInventory6, heldInventory7, heldInventory8 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_PalletID = pallet1;
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory1 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory5 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_PalletID = ZString.Empty; // Clear
				orderLine.WE_PartAttrib1 = "Red";
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory2 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory6 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_PartAttrib1 = ZString.Empty; // Clear
				orderLine.WE_PartAttrib2 = "Large";
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory3 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory7 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_PartAttrib2 = ZString.Empty; // Clear
				orderLine.WE_PartAttrib3 = "Hot";
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory4 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory8 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				orderLine.WE_PalletID = pallet2;
				orderLine.WE_PartAttrib1 = "Blue";
				orderLine.WE_PartAttrib2 = "Small";
				orderLine.WE_PartAttrib3 = "Cold";
				AssertContainsExactElementsInAnyOrder(new[] { heldInventory10, heldInventory11 }, cache.FindMatchingInventoryByAttribs(orderLine));

				orderLine.WE_PartAttrib3 = "NotCold";
				AssertEquals(0, cache.FindMatchingInventoryByAttribs(orderLine).Count());
			}
		}

		#endregion

		#region TestFindMatchingInventoryByAttribs_DoesNotAcceptNull

		public void TestFindMatchingInventoryByAttribs_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new InventoryCacheByAttribs().FindMatchingInventoryByAttribs(null));
		}

		#endregion

		#region TestRemoveInventoryFromAll

		public void TestRemoveInventoryFromAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			var cache = new InventoryCacheByAttribs();
			cache.CacheByAttributes(inventory1);
			cache.CacheByAttributes(inventory2);

			var orderLine = Factory.New<WhsOrderLine>();
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory1, inventory2 },
				cache.FindMatchingInventoryByAttribs(orderLine));

			inventory1.Delete();
			cache.RemoveInventoryFromAll(inventory1.PK);
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory2 },
				cache.FindMatchingInventoryByAttribs(orderLine));
		}

		#endregion
	}
}
