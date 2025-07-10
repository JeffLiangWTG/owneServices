using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickAvailableInventoryCollection))]
	class WhsPickAvailableInventoryCollectionTest : WhsNonPersistentBusinessObjectCollectionTestCase<WhsPickAvailableInventoryCollection>
	{
		#region LoadFromOrderedInventory

		#region TestLoadForOrderedInventory

		[TestDate(2004, 10, 30)]
		public void TestLoadForOrderedInventory()
		{
			TestLoadForOrderedInventoryCore(orderedInv => orderedInv.AvailableInventories);
		}

		[TestDate(2004, 10, 30)]
		public void TestLoadForOrderedInventory_PalletIDOverload()
		{
			TestLoadForOrderedInventoryCore(
				orderedInv =>
				{
					var collection = new WhsPickAvailableInventoryCollection(Factory);
					collection.LoadSpecificPalletIDsForOrderedInventory(orderedInv, new[] { string.Empty, "PALID1", "PALID2", "PALID3" }); // Empty pallet id, for compatibility with old test
					return collection;
				});
		}

		void TestLoadForOrderedInventoryCore(Func<WhsPickOrderedInventory, WhsPickAvailableInventoryCollection> getAvailableInventory)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetClientAttributeType(data.Org1, attribNo, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, attribNo, true);
			}

			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory = new WhsInventoryView[32];

			// this test creates many lines and varies one attribute per line to assert each 
			// attribute is correctly evaluated for merging

			// there are 4 pairs of data sets
			// pair 1 uses a product with no attributes
			// pair 2 uses a product with std attributes and matching Serial Numbers
			// pair 3 uses a product with no attributes and fields that effect the merge, but are not attributes.
			// pair 4 uses a product with std attributes and unique Serial Numbers and should not merge

			// Pair 1, Set 1 and 2 should be merged
			// Set 1
			inventory[0] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			// Set 2
			inventory[10] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m);

			// Pair 2, Set 1 and 2 should be merged
			// Set 1
			inventory[1] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m, today.AddDays(1), today.AddDays(-1), "PA11", "PA12", "PA13", "BEK11");
			inventory[2] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, today.AddDays(1), today.AddDays(-1), "PA21", "PA22", "PA23", "BEK21");
			inventory[3] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 4m, today.AddDays(1), today.AddDays(-1), "PA31", "PA32", "PA33", "BEK31");
			inventory[4] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, today.AddDays(1), today.AddDays(-1), "PA11", "PA12", "PA43", "BEK11");
			inventory[5] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, today.AddDays(1), today.AddDays(-1), "PA51", "PA12", "PA13", "BEK11");
			inventory[6] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, today.AddDays(1), today.AddDays(-1), "PA11", "PA62", "PA13", "BEK11");
			inventory[7] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 8m, today.AddDays(1), today.AddDays(1), "PA11", "PA12", "PA13", "BEK11");
			inventory[8] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 9m, today.AddDays(-1), today.AddDays(-1), "PA11", "PA12", "PA13", "BEK11");
			inventory[9] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, today.AddDays(-1), today.AddDays(-1), "PA11", "PA12", "PA13", "BEK21");
			// Set 2
			inventory[11] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 12m, today.AddDays(1), today.AddDays(-1), "PA11", "PA12", "PA13", "BEK11");
			inventory[12] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 13m, today.AddDays(1), today.AddDays(-1), "PA21", "PA22", "PA23", "BEK21");
			inventory[13] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 14m, today.AddDays(1), today.AddDays(-1), "PA31", "PA32", "PA33", "BEK31");
			inventory[14] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, today.AddDays(1), today.AddDays(-1), "PA11", "PA12", "PA43", "BEK11");
			inventory[15] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 16m, today.AddDays(1), today.AddDays(-1), "PA51", "PA12", "PA13", "BEK11");
			inventory[16] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 17m, today.AddDays(1), today.AddDays(-1), "PA11", "PA62", "PA13", "BEK11");
			inventory[17] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 18m, today.AddDays(1), today.AddDays(1), "PA11", "PA12", "PA13", "BEK11");
			inventory[18] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 19m, today.AddDays(-1), today.AddDays(-1), "PA11", "PA12", "PA13", "BEK11");
			inventory[19] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, today.AddDays(-1), today.AddDays(-1), "PA11", "PA12", "PA13", "BEK21");

			for (int i = 0; i < 20; i++)
			{
				inventory[i].WI_WL = data.Whs1.DefaultLocation.PK;
			}

			// Pair 3, Set 1 and 2 should be merged
			// Set 1
			inventory[20] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m), locations[0].PK, "PALID1");
			inventory[21] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m), locations[0].PK, "PALID2");
			inventory[22] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m), locations[0].PK, "PALID1", InventoryHoldCodes.Codes.Damaged);
			inventory[23] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m), locations[0].PK, "PALID1");
			inventory[24] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m), locations[1].PK, "PALID3");
			// Set 2
			inventory[25] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m), locations[0].PK, "PALID1");
			inventory[26] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m), locations[0].PK, "PALID2");
			inventory[27] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m), locations[0].PK, "PALID1", InventoryHoldCodes.Codes.Damaged);
			inventory[28] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9m), locations[0].PK, "PALID1");
			inventory[29] = CreateInventoryLineWithExtraParams(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m), locations[1].PK, "PALID3");

			// Pair 4, Set 1 and 2 should not be merged
			// Set 1
			inventory[30] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, today.AddDays(1), today.AddDays(-1), "PA31", "PA32", "PA33", "BEK301");
			inventory[30].WI_WL = data.Whs1.DefaultLocation.PK;
			// Set 2
			inventory[31] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, today.AddDays(1), today.AddDays(-1), "PA31", "PA32", "PA33", "BEK301");
			inventory[31].WI_WL = data.Whs1.DefaultLocation.PK;

			receive.FinaliseDocket();
			inventory[30].WI_SerialNumber = "Serial30";
			inventory[31].WI_SerialNumber = "Serial31";
			AssertIsFinalisedPrecondition(receive);

			// set varied arrival date here so it doesn't get overrwritten by Receive.FinaliseDocket();
			for (int a = 20; a < 30; a++)
			{
				inventory[a].WI_ArrivalDate = (a == 23 || a == 28) ? today.AddDays(3).ToZDateTime().ToOffset() : today.AddDays(2).ToZDateTime().ToOffset();
				inventory[a].InDocketLine.WE_AdjustmentArrivalDate = (a == 23 || a == 28) ? today.AddDays(3).ToZDateTime().ToOffset() : today.AddDays(2).ToZDateTime().ToOffset();
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Helper.CreateWhsOrderLine(order, data.Part2, 100m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AssertEquals("Should be 2 WhsPickOrderedInventory objects, 1 for each Part", 2, pick.OrderedInventories.Count);
			var orderedInventory1 = FindOrderedInventory(pick, data.Part1);
			var orderedInventory2 = FindOrderedInventory(pick, data.Part2);
			AssertEquals(5, orderedInventory1.AvailableInventories.Count);
			AssertEquals(11, orderedInventory2.AvailableInventories.Count);

			var orderedInventory1_AvailableInventory = getAvailableInventory(orderedInventory1);
			var orderedInventory2_AvailableInventory = getAvailableInventory(orderedInventory2);

			// Pair 1 merged
			AssertEquals(12m, FindPickInventory(orderedInventory1_AvailableInventory, inventory[0]).QuantityAvailableToPick);

			// Pair 2 merged
			AssertEquals(14m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[1]).QuantityAvailableToPick);
			AssertEquals(16m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[2]).QuantityAvailableToPick);
			AssertEquals(18m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[3]).QuantityAvailableToPick);
			AssertEquals(20m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[4]).QuantityAvailableToPick);
			AssertEquals(22m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[5]).QuantityAvailableToPick);
			AssertEquals(24m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[6]).QuantityAvailableToPick);
			AssertEquals(26m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[7]).QuantityAvailableToPick);
			AssertEquals(28m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[8]).QuantityAvailableToPick);
			AssertEquals(30m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[9]).QuantityAvailableToPick);

			// Pair 3 merged
			AssertEquals(7m, FindPickInventory(orderedInventory1_AvailableInventory, inventory[20]).QuantityAvailableToPick);
			AssertEquals(9m, FindPickInventory(orderedInventory1_AvailableInventory, inventory[21]).QuantityAvailableToPick);

			AssertEquals(13m, FindPickInventory(orderedInventory1_AvailableInventory, inventory[23]).QuantityAvailableToPick);
			AssertEquals(15m, FindPickInventory(orderedInventory1_AvailableInventory, inventory[24]).QuantityAvailableToPick);

			// Pair 4 not merged
			AssertEquals(1m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[30]).QuantityAvailableToPick);
			AssertEquals(1m, FindPickInventory(orderedInventory2_AvailableInventory, inventory[31]).QuantityAvailableToPick);
		}

		#endregion

		#region TestLoadForOrderedInventory_WithPartAttributeCaseMismatch

		public void TestLoadForOrderedInventory_WithPartAttributeCaseMismatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetClientAllAttributeType(data.Org1, false);

			// receive 10 units of stock with attributes in Title Case

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var rcvLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			rcvLine.WI_PartAttrib1 = "Black";
			rcvLine.WI_PartAttrib2 = "Medium";
			rcvLine.WI_PartAttrib3 = "Leather";
			rcvLine.WI_BondedEntryKey = "Abc123";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// order 5 units of stock with all attributes specifed, but in UPPER CASE

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5);
			orderLine.WE_PartAttrib1 = "BLACK";
			orderLine.WE_PartAttrib2 = "MEDIUM";
			orderLine.WE_PartAttrib3 = "LEATHER";
			orderLine.WE_BondedEntryKey = "ABC123";

			var pick = Helper.CreatePickNew(order);
			pick.PickOrdersWithAllocationMock();

			var orderedInventory = FindOrderedInventory(pick, data.Part1);
			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			AssertEquals(10m, orderedInventory.AvailableInventories[0].QuantityAvailableToPick);
		}

		#endregion

		#region TestLoadForOrderedInventory_WithHeldInventory

		public void TestLoadForOrderedInventory_WithHeldInventory_HLD() => TestLoadForOrderedInventory_WithHeldInventory_Core(InventoryHoldCodes.Codes.Held);

		public void TestLoadForOrderedInventory_WithHeldInventory_DMG() => TestLoadForOrderedInventory_WithHeldInventory_Core(InventoryHoldCodes.Codes.Damaged);

		void TestLoadForOrderedInventory_WithHeldInventory_Core(string heldCode)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
				var rcvLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				rcvLine.OriginalInventoryStatus = InventoryHoldCodes.Codes.Held;
				rcvLine.OriginalInventoryHeldCode = heldCode;

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5);
				orderLine.WE_WHC_NKOrderedHeldCode = heldCode;

				var pick = Helper.CreatePickNew(order);
				pick.PickOrdersWithAllocationMock();

				var orderedInventory = FindOrderedInventory(pick, data.Part1);

				AssertEquals(1, orderedInventory.AvailableInventories.Count);
				AssertEquals(10m, orderedInventory.AvailableInventories[0].QuantityAvailableToPick);
			}
		}

		#endregion

		#region TestIsValidStatusForOrderedInventory

		public void TestIsValidStatusForOrderedInventory_OrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];

			AssertEquals("Precondition", PickType.Codes.Order, pick.WP_PickType);
			AssertEquals("Precondition", InventoryStatus.Codes.Available, receiveLine.WI_InventoryStatus);

			orderedInventory.AvailableInventories.LoadForOrderedInventory(orderedInventory);
			AssertEquals("Available Inventory is valid on this Pick", 1, orderedInventory.AvailableInventories.Count);

			orderedInventory.AvailableInventories[0].Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Held;
			orderedInventory.AvailableInventories.LoadForOrderedInventory(orderedInventory);
			AssertEquals("Held Inventory is not valid on this Pick", 0, orderedInventory.AvailableInventories.Count);
		}

		public void TestIsValidStatusForOrderedInventory_HeldInventoryOrderPick()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				var pick = Helper.CreatePickNew(order);
				var orderedInventory = pick.OrderedInventories[0];

				AssertEquals("Precondition", PickType.Codes.HeldInventoryOrder, pick.WP_PickType);
				AssertEquals("Precondition", InventoryStatus.Codes.Held, receiveLine.WE_CurrentInventoryStatus);

				orderedInventory.AvailableInventories.LoadForOrderedInventory(orderedInventory);
				AssertEquals("Held Inventory is valid on this Pick", 1, orderedInventory.AvailableInventories.Count);

				orderedInventory.AvailableInventories[0].Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Available;
				orderedInventory.AvailableInventories.LoadForOrderedInventory(orderedInventory);
				AssertEquals("Available Inventory is not valid on this Pick", 0, orderedInventory.AvailableInventories.Count);
			}
		}

		#endregion

		#region TestLoadForOrderedInventory_WithMatchingPartAttribsAndSerialIncluded

		public void TestLoadForOrderedInventory_WithMatchingPartAttribsAndSerialIncluded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetClientAllAttributeType(data.Org1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var rcvLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine1.WI_PartAttrib1 = "BLACK";
			rcvLine1.WI_PartAttrib2 = "MEDIUM";
			rcvLine1.WI_PartAttrib3 = "LEATHER";
			rcvLine1.WI_SerialNumber = "SERIAL1";
			rcvLine1.WI_BondedEntryKey = "ABC1";

			var rcvLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine2.WI_PartAttrib1 = "BLACK";
			rcvLine2.WI_PartAttrib2 = "MEDIUM";
			rcvLine2.WI_PartAttrib3 = "LEATHER";
			rcvLine2.WI_SerialNumber = "SERIAL2";
			rcvLine2.WI_BondedEntryKey = "ABC2";

			var rcvLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine3.WI_PartAttrib1 = "BLACK";
			rcvLine3.WI_PartAttrib2 = "MEDIUM";
			rcvLine3.WI_PartAttrib3 = "LEATHER";
			rcvLine3.WI_SerialNumber = "SERIAL3";
			rcvLine3.WI_BondedEntryKey = "ABC3";

			var rcvLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine4.WI_PartAttrib1 = "BLACK";
			rcvLine4.WI_PartAttrib2 = "MEDIUM";
			rcvLine4.WI_PartAttrib3 = "LEATHER";
			rcvLine4.WI_SerialNumber = "SERIAL4";
			rcvLine4.WI_BondedEntryKey = "ABC4";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PartAttrib1 = "BLACK";
			orderLine.WE_PartAttrib2 = "MEDIUM";
			orderLine.WE_PartAttrib3 = "LEATHER";
			orderLine.WE_SerialNumber = "SERIAL4";

			var pick = Helper.CreatePickNew(order);
			pick.PickOrdersWithAllocationMock();

			var orderedInventory = FindOrderedInventory(pick, data.Part1);
			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			Assert("Successfully match with Receive Line.", orderedInventory.AvailableInventories[0].QuantityAvailableToPick == 1m);
			Assert("Match with ReceiveLine4.", orderedInventory.AvailableInventories[0].BondedEntryKey == "ABC4");
		}

		#endregion

		#region TestLoadForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded

		public void TestLoadForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded() => TestLoadForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded(attributeNeutral: false);

		public void TestLoadForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded_AttributeNeutral() => TestLoadForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded(attributeNeutral: true);

		void TestLoadForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded(bool attributeNeutral)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);

			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = attributeNeutral ? WhsPickMode.Codes.AttributeNeutral : WhsPickMode.Codes.AttributeSpecified;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var rcvLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine1.WI_PartAttrib1 = "BLACK";
			rcvLine1.WI_PartAttrib2 = "MEDIUM";
			rcvLine1.WI_PartAttrib3 = "LEATHER";

			var rcvLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine2.WI_PartAttrib1 = "BLACK";
			rcvLine2.WI_PartAttrib2 = "MEDIUM";
			rcvLine2.WI_PartAttrib3 = "LEATHER";

			var rcvLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			rcvLine3.WI_PartAttrib1 = "BLACK";
			rcvLine3.WI_PartAttrib2 = "MEDIUM";
			rcvLine3.WI_PartAttrib3 = "LEATHER";
			rcvLine3.WI_SerialNumber = "";

			var rcvLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			rcvLine4.WI_PartAttrib1 = "BLACK";
			rcvLine4.WI_PartAttrib2 = "MEDIUM";
			rcvLine4.WI_PartAttrib3 = "LEATHER";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			rcvLine1.WI_SerialNumber = "SERIAL1";
			rcvLine2.WI_SerialNumber = "SERIAL2";
			rcvLine4.WI_SerialNumber = "SERIAL4";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PartAttrib1 = "BLACK";
			orderLine.WE_PartAttrib2 = "MEDIUM";
			orderLine.WE_PartAttrib3 = "LEATHER";

			var pick = Helper.CreatePickNew(order);
			pick.PickOrdersWithAllocationMock();

			var orderedInventory = FindOrderedInventory(pick, data.Part1);

			if (attributeNeutral)
			{
				AssertEquals(1, orderedInventory.AvailableInventories.Count);
				AssertEquals(string.Empty, orderedInventory.AvailableInventories[0].SerialNumber);
			}
			else
			{
				AssertEquals(4, orderedInventory.AvailableInventories.Count);
				AssertContainsExactElementsInAnyOrder(new[] { string.Empty, "SERIAL1", "SERIAL2", "SERIAL4" }, orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Select(i => i.SerialNumber));
			}
		}

		#endregion

		#region TestLoadSpecificPalletIDsForOrderedInventory

		public void TestTestLoadSpecificPalletIDsForOrderedInventory_NullThrows()
		{
			var orderedInv = new WhsPickOrderedInventory(Factory);
			var availInvCollection = new WhsPickAvailableInventoryCollection(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => availInvCollection.LoadSpecificPalletIDsForOrderedInventory(orderedInv, null));
		}

		public void TestLoadSpecificPalletIDsForOrderedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive_inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			var receive_inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			var receive_inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			var unfilteredAvailableInventories = orderedInventory.AvailableInventories;
			AssertEquals(3, unfilteredAvailableInventories.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z1", "Z2", "Z3" }, unfilteredAvailableInventories.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv1 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv1.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z1" });
			AssertEquals(1, filteredAvailInv1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z1" }, filteredAvailInv1.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv2 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv2.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z2", "Z3" });
			AssertEquals(2, filteredAvailInv2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2", "Z3" }, filteredAvailInv2.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));
		}

		#region TestLoadSpecificPalletIDsForOrderedInventory_WithMatchingPartAttribsAndSerialIncluded

		public void TestLoadSpecificPalletIDsForOrderedInventory_WithMatchingPartAttribsAndSerialIncluded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive_inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "Z1");
			receive_inv1.WI_PartAttrib1 = "BLACK";
			receive_inv1.WI_PartAttrib2 = "MEDIUM";
			receive_inv1.WI_PartAttrib3 = "LEATHER";
			receive_inv1.WI_SerialNumber = "SERIAL1";

			var receive_inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "Z2");
			receive_inv2.WI_PartAttrib1 = "BLACK";
			receive_inv2.WI_PartAttrib2 = "MEDIUM";
			receive_inv2.WI_PartAttrib3 = "LEATHER";
			receive_inv2.WI_SerialNumber = "SERIAL2";

			var receive_inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "Z3");
			receive_inv3.WI_PartAttrib1 = "BLACK";
			receive_inv3.WI_PartAttrib2 = "MEDIUM";
			receive_inv3.WI_PartAttrib3 = "LEATHER";
			receive_inv3.WI_SerialNumber = "SERIAL3";

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			orderLine.WE_PartAttrib1 = "BLACK";
			orderLine.WE_PartAttrib2 = "MEDIUM";
			orderLine.WE_PartAttrib3 = "LEATHER";
			orderLine.WE_SerialNumber = "Serial2";

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			var unfilteredAvailableInventories = orderedInventory.AvailableInventories;
			AssertEquals(1, unfilteredAvailableInventories.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2" }, unfilteredAvailableInventories.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv1 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv1.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z1" });
			AssertEquals(0, filteredAvailInv1.Count);

			var filteredAvailInv2 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv2.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z2", });
			AssertEquals(1, filteredAvailInv2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2", }, filteredAvailInv2.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv3 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv3.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z3" });
			AssertEquals(0, filteredAvailInv3.Count);

			var filteredAvailInv4 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv4.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z1", "Z2", "Z3" });
			AssertEquals(1, filteredAvailInv4.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2", }, filteredAvailInv4.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));
		}

		#endregion

		#region TestLoadSpecificPalletIDsForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded

		public void TestLoadSpecificPalletIDsForOrderedInventory_WithMatchingPartAttribsAndNoSerialIncluded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetClientAllAttributeType(data.Org1, false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive_inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "Z1");
			receive_inv1.WI_PartAttrib1 = "BLACK";
			receive_inv1.WI_PartAttrib2 = "MEDIUM";
			receive_inv1.WI_PartAttrib3 = "LEATHER";

			var receive_inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "Z2");
			receive_inv2.WI_PartAttrib1 = "BLACK";
			receive_inv2.WI_PartAttrib2 = "MEDIUM";
			receive_inv2.WI_PartAttrib3 = "LEATHER";

			var receive_inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "Z3");
			receive_inv3.WI_PartAttrib1 = "BLACK";
			receive_inv3.WI_PartAttrib2 = "MEDIUM";
			receive_inv3.WI_PartAttrib3 = "LEATHER";

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// hack to set serial number
			receive_inv1.WI_SerialNumber = "SERIAL1";
			receive_inv2.WI_SerialNumber = "SERIAL2";
			receive_inv3.WI_SerialNumber = "SERIAL3";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			orderLine.WE_PartAttrib1 = "BLACK";
			orderLine.WE_PartAttrib2 = "MEDIUM";
			orderLine.WE_PartAttrib3 = "LEATHER";

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			var unfilteredAvailableInventories = orderedInventory.AvailableInventories;
			AssertEquals(3, unfilteredAvailableInventories.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z1", "Z2", "Z3" }, unfilteredAvailableInventories.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv1 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv1.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z1" });
			AssertEquals(1, filteredAvailInv1.Count);

			var filteredAvailInv2 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv2.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z2", });
			AssertEquals(1, filteredAvailInv2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2", }, filteredAvailInv2.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv3 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv3.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z3" });
			AssertEquals(1, filteredAvailInv3.Count);

			var filteredAvailInv4 = new WhsPickAvailableInventoryCollection(Factory);
			filteredAvailInv4.LoadSpecificPalletIDsForOrderedInventory(orderedInventory, new[] { "Z2", "Z3" });
			AssertEquals(2, filteredAvailInv4.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2", "Z3" }, filteredAvailInv4.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));
		}

		#endregion

		#endregion

		#region TestLoadForOrderedInventory_WithFinalisedPick

		public void TestLoadForOrderedInventory_WithFinalisedPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);
			data.Line111.WI_ArrivalDate = ZDateTimeOffset.Today;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var reLoadedPick = otherFactory.Load<WhsPick>(pick.PK);
			var orderedInventory = FindOrderedInventory(reLoadedPick, data.Part1);
			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			AssertEquals(100m, FindPickInventory(orderedInventory.AvailableInventories, data.Line111).PickLineQuantity);
		}

		#endregion

		#region TestLoadForOrderedInventory_WithTransfers

		public void TestLoadForOrderedInventory_WithTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			pick.ClearAllInventoriesCache();
			var orderedInventory = pick.OrderedInventories[0];
			var collection1 = new WhsPickAvailableInventoryCollection(Factory);
			collection1.LoadForOrderedInventory(orderedInventory);
			AssertEquals("Should only have the Receive Inventory.", 1, collection1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inventory }, collection1[0].Inventory);

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now; // pick the transfer Line.

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			// finalise Pick to prevent creation of second In-Transit Transfer Line
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			pick.ClearAllInventoriesCache();
			var collection2 = new WhsPickAvailableInventoryCollection(Factory);
			collection2.LoadForOrderedInventory(orderedInventory);
			AssertEquals("Should only have the Receive Inventory.", 1, collection2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inventory }, collection2[0].Inventory);
		}

		#endregion

		#region TestLoadForOrderedInventory_ForDisassemblyWorkOrders

		public void TestLoadForOrderedInventory_ForDisassemblyDynamicWorkOrders()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var client = Helper.CreateClient("C1");
			var component = Helper.CreateProduct("P1", client);
			var mainProduct = Helper.CreateProduct("P2", client);

			var mainProductReceive = Helper.CreateWhsReceive(client, warehouse, "R1");
			mainProductReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			mainProductReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(mainProductReceive, mainProduct.PK, 3m, warehouse.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-1");
			mainProductReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(mainProductReceive);

			var componentReceive = Helper.CreateWhsReceive(client, warehouse, "R2");
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive, component.PK, 10m, warehouse.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-1");
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);

			Factory.Save();

			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D1");
			dynamicWorkOrder1.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine1 = dynamicWorkOrder1.Lines.AddNew();
			parentLine1.WE_OP = mainProduct.PK;
			parentLine1.WE_TransactionQuantity = 3m;
			parentLine1.IsMainInwardProcessedItem = true;

			var childLine1 = dynamicWorkOrder1.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 5m;
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder1);
			AssertIsFinalisedPrecondition(dynamicWorkOrder1.Receive);
			Factory.Save();

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D2");
			dynamicWorkOrder2.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrder2.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine2 = dynamicWorkOrder2.Lines.AddNew();
			parentLine2.WE_OP = mainProduct.PK;
			parentLine2.WE_TransactionQuantity = 3m;
			parentLine2.IsMainInwardProcessedItem = true;

			var componentLine = dynamicWorkOrder2.Lines.AddNew();
			componentLine.WE_OP = component.PK;
			componentLine.WE_TransactionQuantity = 5m;
			componentLine.WE_WE_ParentDocketLine = parentLine2.PK;

			Helper.CreatePickNew(dynamicWorkOrder2);
			dynamicWorkOrder2.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrder2);
			AssertIsFinalisedPrecondition(dynamicWorkOrder2.Receive);
			Factory.Save();

			var dynamicWorkOrderReceive1 = dynamicWorkOrder1.Receive;
			var inventory1 = dynamicWorkOrderReceive1.Lines[0].Inventory.Single();
			var dynamicWorkOrderReceive2 = dynamicWorkOrder2.Receive;
			var inventory2 = dynamicWorkOrderReceive2.Lines[0].Inventory.Single();

			var dynamicWorkOrder3 = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D3");
			dynamicWorkOrder3.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var disassemblyLine = dynamicWorkOrder3.Lines.AddNew();
			disassemblyLine.WE_OP = mainProduct.PK;
			disassemblyLine.WE_TransactionQuantity = 3m;
			disassemblyLine.IsMainInwardProcessedItem = true;

			var pick = Helper.CreatePickNew(dynamicWorkOrder3);

			var collection1 = new WhsPickAvailableInventoryCollection(Factory);
			var orderedInventory1 = pick.OrderedInventories[0];
			collection1.LoadForOrderedInventory(orderedInventory1);
			AssertEquals("Should only have the assembled kits Inventories.", 2, collection1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory2 }, collection1.SelectMany(a => ((WhsPickAvailableInventory)a).Inventory));

			disassemblyLine.WE_AllocationKey = dynamicWorkOrderReceive1.Lines[0].WE_AllocationKey;
			pick.ClearAllInventoriesCache();

			var collection2 = new WhsPickAvailableInventoryCollection(Factory);
			var orderedInventory2 = pick.OrderedInventories[0];
			collection2.LoadForOrderedInventory(orderedInventory2);
			AssertEquals("Should only have the assembled kits Inventories with matching Allocation Key.", 1, collection2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inventory1 }, collection2.SelectMany(a => ((WhsPickAvailableInventory)a).Inventory));
		}

		public void TestLoadForOrderedInventory_ForDisassemblyDynamicWorkOrders_IgnoresKitsWithSecondaryProducts()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var client = Helper.CreateClient("C1");
			var component = Helper.CreateProduct("Component", client);
			var mainProduct = Helper.CreateProduct("Main Product", client);
			var secondaryProduct = Helper.CreateProduct("Secondary Product", client);

			var componentReceive = Helper.CreateWhsReceive(client, warehouse, "R1");
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive, component.PK, 10m, warehouse.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-1");
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);

			Factory.Save();

			var assemblyWorkOrder = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D1");
			assemblyWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			assemblyWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainProductAssemblyLine = assemblyWorkOrder.Lines.AddNew();
			mainProductAssemblyLine.WE_OP = mainProduct.PK;
			mainProductAssemblyLine.WE_TransactionQuantity = 4m;
			mainProductAssemblyLine.IsMainInwardProcessedItem = true;

			var componentAssemblyLine = assemblyWorkOrder.Lines.AddNew();
			componentAssemblyLine.WE_OP = component.PK;
			componentAssemblyLine.WE_TransactionQuantity = 10m;
			componentAssemblyLine.WE_WE_ParentDocketLine = mainProductAssemblyLine.PK;

			var secondaryProductAssemblyLine = assemblyWorkOrder.Lines.AddNew();
			secondaryProductAssemblyLine.WE_OP = secondaryProduct.PK;
			secondaryProductAssemblyLine.WE_TransactionQuantity = 1m;
			secondaryProductAssemblyLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentAssemblyLine = assemblyWorkOrder.Lines.AddNew();
			secondaryComponentAssemblyLine.WE_OP = component.PK;
			secondaryComponentAssemblyLine.WE_TransactionQuantity = 2m;
			secondaryComponentAssemblyLine.WE_WE_ParentDocketLine = secondaryProductAssemblyLine.PK;

			Helper.CreatePickNew(assemblyWorkOrder);
			assemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(assemblyWorkOrder);
			AssertIsFinalisedPrecondition(assemblyWorkOrder.Receive);
			Factory.Save();

			AssertFindsNoResultsForKit(mainProduct);
			AssertFindsNoResultsForKit(secondaryProduct);
			AssertFindsNoResultsForKit(component);

			void AssertFindsNoResultsForKit(OrgSupplierPart orderedKit)
			{
				var disassemblyDynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(client, warehouse);
				disassemblyDynamicWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

				var disassemblyLine = disassemblyDynamicWorkOrder.Lines.AddNew();
				disassemblyLine.WE_OP = orderedKit.PK;
				disassemblyLine.WE_TransactionQuantity = 1m;
				disassemblyLine.IsMainInwardProcessedItem = true;

				var pick = Helper.CreatePickNew(disassemblyDynamicWorkOrder);

				var collection = new WhsPickAvailableInventoryCollection(Factory);
				var orderedInventory = pick.OrderedInventories[0];
				collection.LoadForOrderedInventory(orderedInventory);
				AssertEquals($"Should not return any matching inventory for {orderedKit.OP_PartNum}.", 0, collection.Count);
			}
		}

		public void TestLoadForOrderedInventory_ForDisassemblyWorkOrders_IgnoresKitsWithSecondaryProducts()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var client = Helper.CreateClient("C1");
			var component = Helper.CreateProduct("Component", client);
			var mainProduct = Helper.CreateProduct("Main Product", client);
			var secondaryProduct = Helper.CreateProduct("Secondary Product", client);

			var componentReceive = Helper.CreateWhsReceive(client, warehouse, "R1");
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;

			Helper.CreateWhsReceiveInventoryLine(componentReceive, component.PK, 10m, warehouse.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-1");
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);

			Factory.Save();

			var componentBOM = Helper.CreateProductBOM(mainProduct, component, 2m, "UNT");
			var secondaryProductBOM = Helper.CreateSecondaryProduct(mainProduct, secondaryProduct, 5m);
			var componentUsage = secondaryProductBOM.ComponentUsages.AddNew();
			componentUsage.OPP_OE_Component = componentBOM.PK;
			componentUsage.OPP_ComponentQuantity = 0.5m;

			var randomProduct1 = Helper.CreateProduct("RP1", client);
			Helper.CreateProductBOM(secondaryProduct, randomProduct1, 2m, "UNT");
			var randomProduct2 = Helper.CreateProduct("RP2", client);
			Helper.CreateProductBOM(component, randomProduct2, 2m, "UNT");

			Factory.Save();

			var assemblyWorkOrder = Helper.CreateWhsWorkOrderWithLine(client, warehouse, "ExtRef", mainProduct, 5m);
			assemblyWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			assemblyWorkOrder.WD_IsInwardsProcessingJob = true;
			assemblyWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var productParam = assemblyWorkOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = client.PK;
			productParam.W3_WW = warehouse.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(assemblyWorkOrder);
			assemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(assemblyWorkOrder);
			AssertIsFinalisedPrecondition(assemblyWorkOrder.Receive);

			Factory.Save();

			AssertFindsNoResultsForKit(mainProduct);
			AssertFindsNoResultsForKit(secondaryProduct);
			AssertFindsNoResultsForKit(component);

			void AssertFindsNoResultsForKit(OrgSupplierPart orderedKit)
			{
				var disassemblyDynamicWorkOrder = Helper.CreateWhsWorkOrder(client, warehouse);
				disassemblyDynamicWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
				disassemblyDynamicWorkOrder.WD_IsInwardsProcessingJob = true;

				Helper.CreateWhsWorkOrderLine(disassemblyDynamicWorkOrder, orderedKit, 1m);

				var pick = Factory.New<WhsPick>();
				pick.AddOrders(new[] { disassemblyDynamicWorkOrder });

				var collection = new WhsPickAvailableInventoryCollection(Factory);
				var orderedInventory = pick.OrderedInventories[0];
				collection.LoadForOrderedInventory(orderedInventory);
				AssertEquals($"Should not return any matching inventory for {orderedKit.OP_PartNum}.", 0, collection.Count);
			}
		}

		public void TestLoadForOrderedInventory_ForDisassemblyDynamicWorkOrders_DBHits()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var client = Helper.CreateClient("C1");
			var component = Helper.CreateProduct("P1", client);
			var mainProduct = Helper.CreateProduct("P2", client);
			var secondaryPart = Helper.CreateProduct("P3", client);

			Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				PrepareNewAssembledKitInventories(client, warehouse, mainProduct, component, secondaryPart, i);
			}

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "DWO");
			dynamicWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			var disassemblyLine = dynamicWorkOrder.Lines.AddNew();
			disassemblyLine.WE_OP = mainProduct.PK;
			disassemblyLine.WE_TransactionQuantity = 3m;
			disassemblyLine.IsMainInwardProcessedItem = true;

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 }, // Need to check for Component Links for each inventory, only perform single hit
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var orderedInventory = pickInNewFactory.OrderedInventories[0];

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var collection = new WhsPickAvailableInventoryCollection(newFactory);
				collection.LoadForOrderedInventory(orderedInventory);
			}
		}

		void PrepareNewAssembledKitInventories(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart mainProduct, OrgSupplierPart component, OrgSupplierPart secondaryProduct, int count)
		{
			var componentReceive = Helper.CreateWhsReceive(client, warehouse, "R" + count);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive, component.PK, 10m, warehouse.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-" + count);
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var dynamicWorkOrderWithoutSecondary = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "DM" + count);
			dynamicWorkOrderWithoutSecondary.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrderWithoutSecondary.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine1 = dynamicWorkOrderWithoutSecondary.Lines.AddNew();
			parentLine1.WE_OP = mainProduct.PK;
			parentLine1.WE_TransactionQuantity = 3m;
			parentLine1.IsMainInwardProcessedItem = true;

			var childLine1 = dynamicWorkOrderWithoutSecondary.Lines.AddNew();
			childLine1.WE_OP = component.PK;
			childLine1.WE_TransactionQuantity = 5m;
			childLine1.WE_WE_ParentDocketLine = parentLine1.PK;

			Helper.CreatePickNew(dynamicWorkOrderWithoutSecondary);
			dynamicWorkOrderWithoutSecondary.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrderWithoutSecondary);
			AssertIsFinalisedPrecondition(dynamicWorkOrderWithoutSecondary.Receive);
			Factory.Save();

			var dynamicWorkOrderWithSecondary = Helper.CreateWhsDynamicWorkOrder(client, warehouse, "D" + count);
			dynamicWorkOrderWithSecondary.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			dynamicWorkOrderWithSecondary.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine2 = dynamicWorkOrderWithSecondary.Lines.AddNew();
			parentLine2.WE_OP = mainProduct.PK;
			parentLine2.WE_TransactionQuantity = 3m;
			parentLine2.IsMainInwardProcessedItem = true;

			var childLine2 = dynamicWorkOrderWithSecondary.Lines.AddNew();
			childLine2.WE_OP = component.PK;
			childLine2.WE_TransactionQuantity = 5m;
			childLine2.WE_WE_ParentDocketLine = parentLine2.PK;

			var secondaryProductAssemblyLine = dynamicWorkOrderWithSecondary.Lines.AddNew();
			secondaryProductAssemblyLine.WE_OP = secondaryProduct.PK;
			secondaryProductAssemblyLine.WE_TransactionQuantity = 1m;
			secondaryProductAssemblyLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentAssemblyLine = dynamicWorkOrderWithSecondary.Lines.AddNew();
			secondaryComponentAssemblyLine.WE_OP = component.PK;
			secondaryComponentAssemblyLine.WE_TransactionQuantity = 2m;
			secondaryComponentAssemblyLine.WE_WE_ParentDocketLine = secondaryProductAssemblyLine.PK;

			Helper.CreatePickNew(dynamicWorkOrderWithSecondary);
			dynamicWorkOrderWithSecondary.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(dynamicWorkOrderWithSecondary);
			AssertIsFinalisedPrecondition(dynamicWorkOrderWithSecondary.Receive);
			Factory.Save();
		}

		#endregion

		#region TestLoadForOrderedInventory_ShowsOnlyAvailableInventory

		public void TestLoadForOrderedInventory_ShowsOnlyAvailableInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, loc1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, loc2);
			var inventory3_Held = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, loc1);
			var inventory4_Damaged = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, loc1);
			inventory3_Held.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			inventory4_Damaged.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			receive.FinaliseDocket();

			AssertIsFinalisedPrecondition(receive);
			AssertEquals(InventoryStatus.Codes.Held, inventory3_Held.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Held, inventory4_Damaged.WI_InventoryStatus);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var inventory_InTransit = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, "A-1", "A-2");
			inventory_InTransit.RunPreSaveValidation();
			inventory_InTransit.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition.", InventoryStatus.Codes.InTransit, inventory_InTransit.WE_CurrentInventoryStatus);
			Factory.Save();

			AssertLoadForOrderedInventory_ShowsOnlyAvailableInventory(data, inventory1, inventory2, 5m);
		}

		public void TestLoadForOrderedInventory_ShowsOnlyAvailableInventoryWithSerialNumbersAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory3_Held = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory4_Damaged = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			inventory3_Held.WI_SerialNumber = "SN:003";
			inventory4_Damaged.WI_SerialNumber = "SN:004";
			inventory3_Held.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			inventory4_Damaged.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			AssertLoadForOrderedInventory_ShowsOnlyAvailableInventory(data, inventory1, inventory2, 1m);
		}

		void AssertLoadForOrderedInventory_ShowsOnlyAvailableInventory(TestDataSimpleEnvironment data, WhsInventoryView inventory1, WhsInventoryView inventory2, ZDecimal unitsToOrder)
		{
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, unitsToOrder);

			WhsPick pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);

			// when finalising Pick
			WhsPickOrderedInventory orderedInventory = FindOrderedInventory(pick, data.Part1);
			AssertEquals("Incorrect QuantityOrdered on OrderedInventory", unitsToOrder, orderedInventory.QuantityOrdered);
			AssertEquals("Incorrect PickLineQuantity on OrderedInventory", unitsToOrder, orderedInventory.PickLineQuantity);
			AssertEquals("Incorrect QuantityShort on OrderedInventory", 0m, orderedInventory.QuantityShort);

			//pick.FinaliseAllOrders();
			//pick.FinalisePick();
			order.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Order should be Finalised", true, order.IsFinalised);
			AssertEquals("Pick should be Finalised", true, pick.IsFinalised);
			AssertEquals(2, orderedInventory.AvailableInventories.Count);
			AssertEquals(1, orderedInventory.AvailableInventories[0].Inventory.Count);
			AssertEquals(1, orderedInventory.AvailableInventories[1].Inventory.Count);
			AssertCollectionContains("Inventory1 should be available to pick", inventory1, orderedInventory.AvailableInventories[0].Inventory);
			AssertCollectionContains("Inventory2 should be available to pick", inventory2, orderedInventory.AvailableInventories[1].Inventory);

			// when Opening / Editing a Finalised Pick
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			WhsPick pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			WhsPickOrderedInventory orderedInventoryInOtherFactory = FindOrderedInventory(pickInOtherFactory, data.Part1);
			AssertEquals("Incorrect QuantityOrdered on OrderedInventory", unitsToOrder, orderedInventoryInOtherFactory.QuantityOrdered);
			AssertEquals("Incorrect PickLineQuantity on OrderedInventory", unitsToOrder, orderedInventoryInOtherFactory.PickLineQuantity);
			AssertEquals("Incorrect QuantityShort on OrderedInventory", 0m, orderedInventoryInOtherFactory.QuantityShort);

			// Only inventories from which items were allocated should be visible
			AssertEquals(1, orderedInventoryInOtherFactory.AvailableInventories.Count);
			AssertEquals(1, orderedInventoryInOtherFactory.AvailableInventories[0].Inventory.Count);
			AssertEquals("Inventory1 should be in available to pick list", inventory1.PK, orderedInventoryInOtherFactory.AvailableInventories[0].Inventory[0].PK);
		}

		#endregion

		#region TestLoadForOrderedInventory_ExcludesBondedAreasForNonCustomsOrders

		public void TestLoadForOrderedInventory_ExcludesBondedAreasForNonCustomsOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			location1.WLV_WA_PickingArea = bondedArea.PK;
			var location2 = data.Whs1.FindLocation("A-2");

			var customsReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			customsReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(customsReceive, data.Part1.PK, 10m, location1.PK, "", "BEK-1");
			customsReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(customsReceive);

			var nonCustomsReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(nonCustomsReceive, data.Part1, 10m, location2);
			nonCustomsReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(nonCustomsReceive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var customsOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			customsOrder.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(customsOrder.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order, customsOrder);

			var collection1 = new WhsPickAvailableInventoryCollection(Factory);
			collection1.LoadForOrderedInventory(pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => !o.Owners.Single().IsCustomsTransaction));
			AssertEquals("Should only have one Available Inventory", 1, collection1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inventory2 }, collection1[0].Inventory);

			var collection2 = new WhsPickAvailableInventoryCollection(Factory);
			collection2.LoadForOrderedInventory(pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.Owners.Single().IsCustomsTransaction));
			AssertEquals("Should have all Available Inventory", 2, collection2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { inventory1, inventory2 }, collection2.SelectMany(a => ((WhsPickAvailableInventory)a).Inventory));
		}

		#endregion

		#region TestLoadForOrderedInventory_InwardProcessingAreas

		public void TestLoadForOrderedInventory_InwardProcessingAreas()
			=> TestLoadForOrderedInventory_InwardProcessingAreas(otherJobIsCustoms: false);

		public void TestLoadForOrderedInventory_InwardProcessingAreas_OtherJobIsCustoms()
			=> TestLoadForOrderedInventory_InwardProcessingAreas(otherJobIsCustoms: true);

		void TestLoadForOrderedInventory_InwardProcessingAreas(bool otherJobIsCustoms)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var location2 = data.Whs1.FindLocation("A-2");
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = iprArea.PK;
			location1.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			var iprReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			iprReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			iprReceive.WD_IsInwardsProcessingJob = true;

			var inventory1 = Helper.CreateWhsReceiveLine(iprReceive, data.Part1.PK, 10m, location1.PK, "", "BEK-1").Inventory[0];
			iprReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(iprReceive);

			var nonCustomsReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(nonCustomsReceive, data.Part1, 10m, location2);
			nonCustomsReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(nonCustomsReceive);
			Factory.Save();

			var iprOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			iprOrder.WD_DocketSubType = OrderType.Codes.Customs;
			iprOrder.WD_IsInwardsProcessingJob = true;
			Helper.SetOutwardsEntryKeyForOrderLine(iprOrder.Lines[0], "ABC");
			var iprPick = Helper.CreatePickNew(iprOrder);

			var otherOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			otherOrder.WD_DocketSubType = otherJobIsCustoms ? OrderType.Codes.Customs : OrderType.Codes.Order;
			if (otherJobIsCustoms)
			{
				Helper.SetOutwardsEntryKeyForOrderLine(otherOrder.Lines[0], "ABC");
			}
			var otherPick = Helper.CreatePickNew(otherOrder);

			var collection1 = iprPick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInAnyOrder(new[] { inventory1 }, collection1.Inventory);

			var collection2 = otherPick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInAnyOrder(new[] { inventory2 }, collection2.Inventory);
		}

		#endregion

		#region TestLoadForOrderedInventory_ExcludesInTransitInventory

		public void TestLoadForOrderedInventory_ExcludesInTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var inventory_InTransit = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "A-2");
			inventory_InTransit.RunPreSaveValidation();
			inventory_InTransit.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition.", InventoryStatus.Codes.InTransit, inventory_InTransit.WE_CurrentInventoryStatus);
			AssertEquals("Precondition.", 0m, receive.Inventory[0].WI_TotalUnits);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - 1 Ordered Inventory.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition - 0 Available Inventory (In-Transit is not available to pick).", 0, pick.OrderedInventories[0].AvailableInventories.Count);
		}

		#endregion

		#region TestLoadForOrderedInventory_ExcludesInTransitInventory_DoesNotMergeWithAvailableStock

		[TestDate(2017, 01, 01)] // Available inventory is grouped by arrival date mock the date so both receives have same arrival date 
		public void TestLoadForOrderedInventory_ExcludesInTransitInventory_DoesNotMergeWithAvailableStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-2"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var inventory_InTransit = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-2", "A-1");
			inventory_InTransit.RunPreSaveValidation();
			inventory_InTransit.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition.", 10m, receive.Inventory[0].WI_TotalUnits);
			AssertEquals("Precondition.", 0m, receive.Inventory[1].WI_TotalUnits);
			AssertEquals("Precondition.", InventoryStatus.Codes.InTransit, inventory_InTransit.WE_CurrentInventoryStatus);
			AssertEquals("Precondition.", 10m, inventory_InTransit.WE_StockOnHand);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Single().WZ_Units -= 1m;

			// Commit the in transit line, just for asserting "QuantityCommitted" doesnt include these lines
			// Remove if we have triggers to stop this
			var dodgyPickLine = Factory.New<WhsPickLine>();
			dodgyPickLine.WZ_WE_InventoryLine = inventory_InTransit.PK;
			dodgyPickLine.WZ_WE_TransactionLine = order.Lines[0].PK;
			dodgyPickLine.WZ_Units = 1m;
			Factory.Save();

			pick.ClearOrderedInventoriesCache();
			AssertEquals("Precondition - 1 Ordered Inventory.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition - 1 Available Inventory.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("QuantityTotal should only include the 'AVL' line.", 10m, availInv.QuantityTotal);
			AssertEquals("QuantityAvailableToPick should only include the 'AVL' line.", 10m, availInv.QuantityAvailableToPick);
			AssertEquals("QuantityCommitted should only include the 'AVL' line.", 4m, availInv.QuantityCommitted);
			AssertEquals("QuantityCrossDocked should be 0.", 0m, availInv.QuantityCrossDocked);

			transfer.FinaliseDocket();
			AssertEquals("Precondition.", InventoryStatus.Codes.Available, inventory_InTransit.WE_CurrentInventoryStatus);
			Factory.Save();

			pick.ClearOrderedInventoriesCache();
			AssertEquals("Precondition - 1 Ordered Inventory.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition - 1 Available Inventory.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("QuantityTotal.", 20m, availInv.QuantityTotal);
			AssertEquals("QuantityAvailableToPick.", 20m, availInv.QuantityAvailableToPick);
			AssertEquals("QuantityCommitted.", 5m, availInv.QuantityCommitted);
			AssertEquals("QuantityCrossDocked should be 0.", 0m, availInv.QuantityCrossDocked);
		}

		#endregion

		#region TestLoadForOrderedInventory_ShowsInventoryWithTotalUnitsZeroIfPickLineExists

		/// <summary>
		/// This test ensures 'orphan' picklines are made visible so that the user can unallocate them.
		/// (Previous bugs have resulted in picklines remaining attached to Orders after the Orders were removed from their Pick).
		/// </summary>
		public void TestLoadForOrderedInventory_ShowsInventoryWithTotalUnitsZeroIfPickLineExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locations[0]);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[1]);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[2]);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, locations[1]);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, locations[2]);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			AssertEquals("Precondition - SOH must be removed by the adjustment.", 0m, inventory2.WI_TotalUnits);
			AssertEquals("Precondition - SOH must be removed by the adjustment.", 0m, inventory3.WI_TotalUnits);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			pick.ClearOrderedInventoriesCache();
			var pickLine1 = Helper.CreateWhsPickLine(orderLine, inventory1, 1m);
			var pickLine2 = Helper.CreateWhsPickLine(orderLine, inventory2, 1m);
			var pickLine3 = Helper.CreateWhsPickLine(orderLine, inventory3, 1m);

			// Check that all 3 pick lines are available so the user can un-allocate them and avoid having to call us for datafix.
			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(3, pick.OrderedInventories[0].AvailableInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[1].PickLines.Count());
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[2].PickLines.Count());
		}

		#endregion

		#region TestLoadForOrderedInventory_MergeWithPackageGroupIdAndPerPackageQty

		public void TestLoadForOrderedInventory_MergeWithPackageGroupIdAndPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			locations[1].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locations[0].PK, "123-1", "ABC", 5m);   // mergable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locations[0].PK, "123-1", "ABC", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locations[1].PK, "123-1", "OPQ", 5m);   // not mergable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locations[1].PK, "123-1", "RST", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locations[0].PK, "123-1", "", 5m);      // not mergable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "", 10m);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(50m, orderedInventory.PickLineQuantity);

			AssertEquals(5, orderedInventory.AvailableInventories.Count);
			var availables = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			availables.Single(i => i.PackageGroupId == "ABC" && i.QuantityAvailableToPick == 200m);
			availables.Single(i => i.PackageGroupId == "OPQ" && i.QuantityAvailableToPick == 100m && i.PerPackageQty == 5m);
			availables.Single(i => i.PackageGroupId == "RST" && i.QuantityAvailableToPick == 50m && i.PerPackageQty == 5m);
			availables.Single(i => i.PackageGroupId == "" && i.QuantityAvailableToPick == 100m && i.PerPackageQty == 5m);
			availables.Single(i => i.PackageGroupId == "" && i.QuantityAvailableToPick == 200m && i.PerPackageQty == 10m);
		}

		#endregion

		#region TestLoadPickLinesForItemsOfThisCollection

		public void TestLoadPickLinesForItemsOfThisCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1_1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1_2", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1_3", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1_3", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1_3", "", "", "");

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 50m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 50m);

			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition - Pick Line Required Count", 1, pick1.OrderedInventories.Count);

			var orderedInventory = FindOrderedInventory(pick1, data.Part1);
			AssertEquals("Precondition - Owners Count", 2, orderedInventory.Owners.Count);
			AssertEquals("Precondition - Available Lines Count", 3, orderedInventory.AvailableInventories.Count);

			var availableInventory1 = orderedInventory.AvailableInventories[0];
			var availableInventory2 = orderedInventory.AvailableInventories[1];
			var availableInventory3 = orderedInventory.AvailableInventories[2];
			AssertEquals("Precondition - Inventory Lines Count", 1, availableInventory1.Inventory.Count);
			AssertEquals("Precondition - Inventory Lines Count", 1, availableInventory2.Inventory.Count);
			AssertEquals("Precondition - Inventory Lines Count", 3, availableInventory3.Inventory.Count);

			// clear automaticaly allocated PickLines to set them manually.
			orderedInventory.PickLines.DeleteAll();

			var pickLine21 = CreatePickLine(availableInventory2, availableInventory2.Inventory[0], orderLine1);
			var pickLine32 = CreatePickLine(availableInventory3, availableInventory3.Inventory[1], orderLine1);
			var pickLine331 = CreatePickLine(availableInventory3, availableInventory3.Inventory[2], orderLine1);
			var pickLine332 = CreatePickLine(availableInventory3, availableInventory3.Inventory[2], orderLine1);
			var pickLine333 = CreatePickLine(availableInventory3, availableInventory3.Inventory[2], orderLine2);

			Factory.Save();

			// actual test
			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick1.PK);
			var orderedInventory2 = FindOrderedInventory(pickInOtherFactory, data.Part1);

			// we interested in PickLinesOnly
			AssertEquals(3, orderedInventory2.AvailableInventories.Count);
			AssertEquals(0, orderedInventory2.AvailableInventories[0].PickLines.Count());
			AssertEquals(1, orderedInventory2.AvailableInventories[1].PickLines.Count());
			AssertEquals(pickLine21.PK, orderedInventory2.AvailableInventories[1].PickLines.ElementAt(0).PK);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine32.PK, pickLine331.PK, pickLine332.PK, pickLine333.PK },
				orderedInventory2.AvailableInventories[2].PickLines.Select(pl => pl.PK));
		}

		#endregion

		#region TestLoadForOrderedInventory_BondedEntryKey

		public void TestLoadForOrderedInventory_BondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "ABC-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "ABC-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "ABC-3");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "ABC-2", "DummyOutward-1", "");
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Only fully matching inventory should be available to pick.", 1, pick.OrderedInventories[0].AvailableInventories.Count);
			AssertEquals("Only fully matching inventory should be available to pick.", 1, pick.OrderedInventories[0].AvailableInventories[0].Inventory.Count);
		}

		#endregion

		#region TestLoadForOrderedInventory_AllocationKey

		public void TestLoadForOrderedInventory_AllocationKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			inventory2.WI_AllocationKey = "ABC";
			inventory3.WI_AllocationKey = "ABC";
			inventory4.WI_AllocationKey = "DEF";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_AllocationKey = "ABC";

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Only fully matching available inventory should be available to pick.", 1, pick.OrderedInventories[0].AvailableInventories.Count);
			AssertContainsExactElementsInAnyOrder(
				new[] { inventory2, inventory3 },
				 pick.OrderedInventories[0].AvailableInventories[0].Inventory);
		}

		#endregion

		#region TestLoadForOrderedInventory_MergeNonCustomsReceiveWithDifferentpackageGroupIDAndPerPackageQty

		public void TestLoadForOrderedInventory_MergeNonCustomsReceiveWithDifferentpackageGroupIDAndPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateArea(data.Whs1, "A", AreaTypes.Codes.FreeStore);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "", "A", 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "", "B", 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "", "C", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertNotEquals("Receive is not Customs subtype.", receive.WD_DocketSubType, ReceiveType.Codes.Customs);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, "", "", "");
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Inventory should be merged.", 1, pick.OrderedInventories[0].AvailableInventories.Count);
		}

		#endregion

		#region TestValidationPickableDocketLineShouldNotLoadAllInventories

		[TestDate(2016, 04, 19)] // Available inventory is grouped by arrival date mock the date so both receives have same arrival date 
		public void TestValidationPickableDocketLineShouldNotLoadAllInventories()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = FindOrderedInventory(pick, data.Part1);
			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			Factory.Save();
			var inventories = orderedInventory.AvailableInventories[0].Inventory.ToArray();
			var pickedinventory = orderedInventory.AvailableInventories[0].PickLines.ElementAt(0).Inventory;
			var unrelatedinventory = (WhsInventoryView)inventories.Single(i => i.PK != pickedinventory.PK);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderinNewFactory = newFactory.Load<WhsOrder>(order.PK);
			orderinNewFactory.Pick.IsAlterPick = true;
			orderinNewFactory.FinaliseDocket();

			// finalise docket should not load all available inventories
			AssertWhsInventoryExistsInFactory(newFactory, pickedinventory.PK, false);
			AssertWhsInventoryExistsInFactory(newFactory, unrelatedinventory.PK, false);

			AssertEquals(1, FindOrderedInventory(orderinNewFactory.Pick, data.Part1).AvailableInventories.Count);

			// after call AvailableInventories both inventories should be loaded
			AssertWhsInventoryExistsInFactory(newFactory, pickedinventory.PK, true);
			AssertWhsInventoryExistsInFactory(newFactory, unrelatedinventory.PK, true);
		}

		void AssertWhsInventoryExistsInFactory(BusinessObjectFactory newFactory, ZGuid objPK, bool shouldExists)
		{
			var allBusinessObjectsInFactory = ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects;
			AssertEquals(shouldExists, allBusinessObjectsInFactory.Any(bizObj => bizObj.PK == objPK && bizObj is WhsInventoryView));
		}

		#endregion

		WhsPickLine CreatePickLine(WhsPickAvailableInventory availableInventory, WhsInventoryView inventory, WhsPickableDocketLine orderLine)
		{
			var pickLine = availableInventory.OrderedInventory.PickLines.AddNew();
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			pickLine.WZ_Units = 1m;
			return pickLine;
		}

		WhsInventoryView CreateInventoryLineWithExtraParams(WhsInventoryView inventory, ZGuid wi_wl, ZString palletID, string heldCode = "")
		{
			inventory.WI_WL = wi_wl;
			inventory.OriginalInventoryHeldCode = heldCode;
			inventory.WI_PalletID = palletID;
			return inventory;
		}

		WhsPickAvailableInventory FindPickInventory(WhsPickAvailableInventoryCollection availableInventories, WhsInventoryView inventory)
		{
			foreach (WhsPickAvailableInventory availableInventory in availableInventories)
			{
				if (availableInventory.SupplierPart.PK == inventory.SupplierPart.PK && AttributeComparer.CompareWithIsEmptyCheck(availableInventory, inventory) &&
					availableInventory.LocationPK == inventory.WI_WL && availableInventory.ArrivalDate == inventory.WI_ArrivalDate &&
					availableInventory.InventoryStatus == inventory.WI_InventoryStatus && availableInventory.PalletID == inventory.WI_PalletID)
				{
					return availableInventory;
				}
			}
			return null;
		}

		WhsPickOrderedInventory FindOrderedInventory(WhsPick pick, OrgSupplierPart part)
		{
			foreach (WhsPickOrderedInventory orderedInventory in pick.OrderedInventories)
			{
				if (orderedInventory.SupplierPart.PK == part.PK)
				{
					return orderedInventory;
				}
			}
			Fail("Could not find OrderedInventory");
			return null;
		}

		#region TestAvailableInventoryForSerialNumberAttributeNeutral

		public void TestAvailableInventoryForSerialNumberAttributeNeutral_1_2()
		{
			TestAvailableInventoryForSerialNumberAttributeNeutralCore(AttributeNumber.One, AttributeNumber.Two);
		}

		public void TestAvailableInventoryForSerialNumberAttributeNeutral_2_3()
		{
			TestAvailableInventoryForSerialNumberAttributeNeutralCore(AttributeNumber.Two, AttributeNumber.Three);
		}

		public void TestAvailableInventoryForSerialNumberAttributeNeutral_3_1()
		{
			TestAvailableInventoryForSerialNumberAttributeNeutralCore(AttributeNumber.Three, AttributeNumber.One);
		}

		void TestAvailableInventoryForSerialNumberAttributeNeutralCore(AttributeNumber nonSerialAttribute, AttributeNumber serialAttribute)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (int i = 0; i < 10; i++)
			{
				var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, data.Whs1.DefaultLocation);
				inventoryLine.WI_PartAttrib1 = "LIBERAL";
				inventoryLine.WI_SerialNumber = "SN" + i;
			}

			for (int i = 10; i < 20; i++)
			{
				var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, data.Whs1.DefaultLocation);
				inventoryLine.WI_PartAttrib1 = "LABOR";
				inventoryLine.WI_SerialNumber = "SN" + i;
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Should ignore SN AN for grouping available inventory", 2, orderedInventory.AvailableInventories.Count);

			var liberal = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().SingleOrDefault(a => a.PartAttrib1 == "LIBERAL" && a.SerialNumber == "");
			var labor = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().SingleOrDefault(a => a.PartAttrib1 == "LABOR" && a.SerialNumber == "");
			AssertNotNull(liberal);
			AssertNotNull(labor);
			AssertEquals(10m, liberal.PickLineQuantity);
			AssertEquals(10m, labor.PickLineQuantity);

			Assert(liberal.PickLines.All(pl => pl.InventoryLine.WE_PartAttrib1 == "LIBERAL"));
			Assert(labor.PickLines.All(pl => pl.InventoryLine.WE_PartAttrib1 == "LABOR"));

			AssertEquals(10, liberal.PickLines.Select(pl => pl.InventoryLine.WE_SerialNumber).Distinct().Count());
			AssertEquals(10, labor.PickLines.Select(pl => pl.InventoryLine.WE_SerialNumber).Distinct().Count());
		}

		public void TestIsAttributeNeutralUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);

			owner.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("Should be attribute neutral.", true, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, data.Part1.PK, data.Org1.PK, null));
			AssertEquals("Should *not* be attribute neutral.", false, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, ZGuid.NewZGuid(), data.Org1.PK, null));
			AssertEquals("Should *not* be attribute neutral.", false, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, data.Part1.PK, ZGuid.NewZGuid(), null));

			owner.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("Should be cached.", true, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, data.Part1.PK, data.Org1.PK, null));

			Factory.Save();
			AssertEquals("Cache should cleared.", false, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, data.Part1.PK, data.Org1.PK, null));
		}

		public void TestIsAttributeNeutralUsed_OrderedSerial()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			line.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "SN1";
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Attribute Neutral should not be used when the Serial is Ordered.", false, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, data.Part1.PK, data.Org1.PK, orderedInventory));
		}

		public void TestIsAttributeNeutralUsed_NonOrderedSerial()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			line.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Attribute Neutral should be used when the Serial is not Ordered.", true, WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, data.Part1.PK, data.Org1.PK, orderedInventory));
		}

		#endregion

		#endregion

		#region TestMergeInventoryIntoThisCollection

		public void TestMergeInventoryIntoThisCollection_NullArgument()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventories = orderedInventory.AvailableInventories;

			AssertExceptionThrown<ArgumentNullException>(() => availableInventories.MergeInventoryIntoThisCollection(null));
		}

		public void TestMergeInventoryIntoThisCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventories = orderedInventory.AvailableInventories;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventoryLines = receive.Lines[0].Inventory.Cast<WhsInventoryView>();

			availableInventories.MergeInventoryIntoThisCollection(inventoryLines);

			AssertEquals(1, availableInventories.Count);
			AssertEquals(1, availableInventories[0].Inventory.Count);
			AssertEquals(inventoryLines.Single(), availableInventories[0].Inventory[0]);
		}

		public void TestMergeInventoryIntoThisCollection_InventoryIsNotAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventories = orderedInventory.AvailableInventories;

			var inventoryLine = Factory.New<WhsInventoryView>();
			inventoryLine.WI_InventoryStatus = InventoryStatus.Codes.Received;

			availableInventories.MergeInventoryIntoThisCollection(new WhsInventoryView[] { inventoryLine });

			AssertEquals(0, availableInventories.Count);
		}

		#endregion

		#region Sorting

		public void TestSort()
		{
			var today = ZDate.Today;
			var todayOffset = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true); //ExpiryDate
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory = new WhsInventoryView[5];
			inventory[0] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, today.AddDays(2), ZDate.Empty, "SN:0", "", "", "");
			inventory[1] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, today.AddDays(4), ZDate.Empty, "SN:1", "", "", "");
			inventory[2] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, today.AddDays(3), ZDate.Empty, "SN:2", "", "", "");
			inventory[3] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, today.AddDays(5), ZDate.Empty, "SN:3", "", "", "");
			inventory[4] = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, today.AddDays(1), ZDate.Empty, "SN:4", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			inventory[0].WI_ArrivalDate = todayOffset.AddDays(-3);
			inventory[0].InDocketLine.WE_AdjustmentArrivalDate = todayOffset.AddDays(-3);

			inventory[1].WI_ArrivalDate = todayOffset.AddDays(-1);
			inventory[1].InDocketLine.WE_AdjustmentArrivalDate = todayOffset.AddDays(-1);

			inventory[2].WI_ArrivalDate = todayOffset.AddDays(-4);
			inventory[2].InDocketLine.WE_AdjustmentArrivalDate = todayOffset.AddDays(-4);

			inventory[3].WI_ArrivalDate = todayOffset.AddDays(-5);
			inventory[3].InDocketLine.WE_AdjustmentArrivalDate = todayOffset.AddDays(-5);

			inventory[4].WI_ArrivalDate = todayOffset.AddDays(-2);
			inventory[4].InDocketLine.WE_AdjustmentArrivalDate = todayOffset.AddDays(-2);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Should be 1 WhsPickOrderedInventory objects", 1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Should contain 5 WhsPickAvailableInventory objects", 5, orderedInventory.AvailableInventories.Count);

			//Mess order up and check that messing had worked
			orderedInventory.AvailableInventories.Sort(WhsPickAvailableInventory.Schema.QuantityAvailableToPick);
			AssertCorrectOrderByQuantityAvailable(orderedInventory, 1m, 2m, 3m, 4m, 5m);    // sorted by QuantityAvailableToPick

			orderedInventory.AvailableInventories.Sort(new SortPickInventoryForPicking());
			AssertCorrectOrderByQuantityAvailable(orderedInventory, 5m, 1m, 3m, 2m, 4m);    // sorted by ExpiryDate

			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			orderedInventory.AvailableInventories.Sort(new SortPickInventoryForPicking());
			AssertCorrectOrderByQuantityAvailable(orderedInventory, 5m, 1m, 3m, 2m, 4m);    // sorted by ExpiryDate
		}

		public void TestSortingByLocation_LocationString()
		{
			TestSortingByLocationCore(nameof(WhsPickAvailableInventory.LocationString));
		}

		public void TestSortingByLocation_LocationPK()
		{
			TestSortingByLocationCore(nameof(WhsPickAvailableInventory.LocationPK));
		}

		void TestSortingByLocationCore(string locationPropertyToSort)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-10"));
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 30);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Should contain 3 WhsPickAvailableInventory objects", 3, orderedInventory.AvailableInventories.Count);

			orderedInventory.AvailableInventories.Sort(locationPropertyToSort, ListSortDirection.Ascending);
			AssertEquals("A-1", orderedInventory.AvailableInventories[0].LocationString);
			AssertEquals("A-2", orderedInventory.AvailableInventories[1].LocationString);
			AssertEquals("A-10", orderedInventory.AvailableInventories[2].LocationString);

			orderedInventory.AvailableInventories.Sort(locationPropertyToSort, ListSortDirection.Descending);
			AssertEquals("A-10", orderedInventory.AvailableInventories[0].LocationString);
			AssertEquals("A-2", orderedInventory.AvailableInventories[1].LocationString);
			AssertEquals("A-1", orderedInventory.AvailableInventories[2].LocationString);
		}

		public void TestSortingByLocation_LocationString_FixedWidthLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "AA", 4, 3, 2);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, warehouse.FindLocation("AA010102"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, warehouse.FindLocation("AA020101"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, warehouse.FindLocation("AA040302"));
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, data.Part1, 30);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Should contain 3 WhsPickAvailableInventory objects", 3, orderedInventory.AvailableInventories.Count);

			orderedInventory.AvailableInventories.Sort(nameof(WhsPickAvailableInventory.LocationString), ListSortDirection.Ascending);
			AssertEquals("AA-01-01-02", orderedInventory.AvailableInventories[0].LocationString);
			AssertEquals("AA-02-01-01", orderedInventory.AvailableInventories[1].LocationString);
			AssertEquals("AA-04-03-02", orderedInventory.AvailableInventories[2].LocationString);

			orderedInventory.AvailableInventories.Sort(nameof(WhsPickAvailableInventory.LocationString), ListSortDirection.Descending);
			AssertEquals("AA-04-03-02", orderedInventory.AvailableInventories[0].LocationString);
			AssertEquals("AA-02-01-01", orderedInventory.AvailableInventories[1].LocationString);
			AssertEquals("AA-01-01-02", orderedInventory.AvailableInventories[2].LocationString);
		}

		#endregion

		#region NonPersistentBusinessObjectCollection Overrides

		public void TestAllowNew()
		{
			Collection = GetCollectionToTest();
			AssertEquals(false, Collection.AllowNew);
		}

		#endregion

		#region TestGetTotalAvailableToPickUnits

		public void TestGetTotalAvailableToPickUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 1m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today.AddDays(-4), data.Part1, 2m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 3m, location3, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location2, location1);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);

			var pick = Helper.CreatePickNew(order);
			var availableInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories;
			AssertEquals(6m, availableInventories.GetTotalAvailableToPickUnits(new HashSet<WhsLocation>()));
			AssertEquals(4m, availableInventories.GetTotalAvailableToPickUnits(new HashSet<WhsLocation>() { location2 }));
			AssertEquals(1m, availableInventories.GetTotalAvailableToPickUnits(new HashSet<WhsLocation>() { location2, location3 }));
			AssertEquals(0m, availableInventories.GetTotalAvailableToPickUnits(new HashSet<WhsLocation>() { location1, location2, location3 }));

			var availableInventoryForLocation1 = availableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var availableInventoryForLocation2 = availableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location2);
			availableInventoryForLocation1.Allocate = true;
			availableInventoryForLocation2.Allocate = true;
			AssertEquals(4m, availableInventories.GetTotalAvailableToPickUnits(new HashSet<WhsLocation>()));
			AssertEquals(3m, availableInventories.GetTotalAvailableToPickUnits(new HashSet<WhsLocation>() { location2 }));
		}

		#endregion

		#region TestGetAvailableInventoryFromInventoryPK

		public void TestGetAvailableInventoryFromInventoryPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P123", ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P456", ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P789", ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P789", ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Should have 3 Available Inventories.", 3, orderedInventory.AvailableInventories.Count);

			var firstRedInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib1 == "Red" && a.PalletID == "P123");
			var secondRedInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib1 == "Red" && a != firstRedInventory);
			var onlyBlueInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib1 == "Blue");
			AssertEquals("Should match Correct Available Inventory.", firstRedInventory, orderedInventory.AvailableInventories.GetAvailableInventoryFromInventoryPK(inventory1.WI_WE_InDocketLine));
			AssertEquals("Should match Correct Available Inventory.", secondRedInventory, orderedInventory.AvailableInventories.GetAvailableInventoryFromInventoryPK(inventory2.WI_WE_InDocketLine));
			AssertEquals("Should match Correct Available Inventory.", onlyBlueInventory, orderedInventory.AvailableInventories.GetAvailableInventoryFromInventoryPK(inventory3.WI_WE_InDocketLine));
			AssertEquals("Should match Correct Available Inventory.", onlyBlueInventory, orderedInventory.AvailableInventories.GetAvailableInventoryFromInventoryPK(inventory4.WI_WE_InDocketLine));
		}

		#endregion

		#region TestGetInventoryHashCode

		public void TestGetInventoryHashCode_Client() => TestGetInventoryHashCode((i, v) => i.WI_OH_Client = v, ZGuid.NewZGuid(), ZGuid.NewZGuid());
		public void TestGetInventoryHashCode_Product() => TestGetInventoryHashCode((i, v) => i.WI_OP = v, ZGuid.NewZGuid(), ZGuid.NewZGuid());
		public void TestGetInventoryHashCode_Location() => TestGetInventoryHashCode((i, v) => i.WI_WL = v, ZGuid.NewZGuid(), ZGuid.NewZGuid());
		public void TestGetInventoryHashCode_ArrivalDate() => TestGetInventoryHashCode((i, v) => i.WI_ArrivalDate = v, ZDateTimeOffset.Today, ZDateTimeOffset.Today.AddDays(-1));
		public void TestGetInventoryHashCode_InventoryStatus() => TestGetInventoryHashCode((i, v) => i.WI_InventoryStatus = v, InventoryStatus.Codes.Pending, InventoryStatus.Codes.Held);
		public void TestGetInventoryHashCode_PalletID() => TestGetInventoryHashCode((i, v) => i.WI_PalletID = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_BondedEntryKey() => TestGetInventoryHashCode((i, v) => i.WI_BondedEntryKey = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_AllocationKey() => TestGetInventoryHashCode((i, v) => i.WI_AllocationKey = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_PartAttrib1() => TestGetInventoryHashCode((i, v) => i.WI_PartAttrib1 = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_PartAttrib2() => TestGetInventoryHashCode((i, v) => i.WI_PartAttrib2 = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_PartAttrib3() => TestGetInventoryHashCode((i, v) => i.WI_PartAttrib3 = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_SerialNumber() => TestGetInventoryHashCode((i, v) => i.WI_SerialNumber = v, "ABC", "DEF");
		public void TestGetInventoryHashCode_PackingDate() => TestGetInventoryHashCode((i, v) => i.WI_PackingDate = v, ZDate.Today, ZDate.Today.AddDays(-1));
		public void TestGetInventoryHashCode_ExpiryDate() => TestGetInventoryHashCode((i, v) => i.WI_ExpiryDate = v, ZDate.Today, ZDate.Today.AddDays(-1));
		public void TestGetInventoryHashCode_ValueForDuty() => TestGetInventoryHashCode((i, v) => i.CustomsData.WB_ValueForDuty = v, 8.74m, 19.33m);
		public void TestGetInventoryHashCode_BondedWhsQty() => TestGetInventoryHashCode((i, v) => i.CustomsData.WB_BondedWhsQty = v, 3.14m, 9.81m);

		void TestGetInventoryHashCode<T>(Action<WhsInventoryView, T> setValue, T value1, T value2)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetClientAllAttributeType(data.Org1, true);

			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m).Inventory[0];

			setValue(inventory, value1);
			var originalHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", originalHashCode);

			setValue(inventory, value2);
			var newHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", newHashCode);
			AssertNotEquals("HashCodes should be different.", newHashCode, originalHashCode);
		}

		public void TestGetInventoryHashCode_PackageGroupId()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetClientAllAttributeType(data.Org1, true);

			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m).Inventory[0];

			inventory.InDocketLine.WE_PackageGroupId = "ABC";
			var originalHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", originalHashCode);

			inventory.InDocketLine.WE_PackageGroupId = "DEF";
			var newHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", newHashCode);
			AssertNotEquals("HashCodes should be different.", newHashCode, originalHashCode);
		}

		public void TestGetInventoryHashCode_Excluded_InDocketLineUnits() => TestGetInventoryHashCode_Excluded((i, v) => i.WI_InDocketLineUnits = v, 3.14m, 9.81m);
		public void TestGetInventoryHashCode_Excluded_TotalUnits() => TestGetInventoryHashCode_Excluded((i, v) => i.WI_TotalUnits = v, 3.14m, 9.81m);
		public void TestGetInventoryHashCode_Excluded_LineNo() => TestGetInventoryHashCode_Excluded<ZShort>((i, v) => i.WI_LineNo = v, 1, 2);
		public void TestGetInventoryHashCode_Excluded_IsOriginalReceiptLine() => TestGetInventoryHashCode_Excluded((i, v) => i.WI_IsOriginalReceiptLine = v, false, true);

		void TestGetInventoryHashCode_Excluded<T>(Action<WhsInventoryView, T> setValue, T value1, T value2)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m).Inventory[0];

			setValue(inventory, value1);
			var originalHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", originalHashCode);

			setValue(inventory, value2);
			var newHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", newHashCode);
			AssertEquals("HashCodes should be the same.", newHashCode, originalHashCode);
		}

		public void TestGetInventoryHashCode_Excluded_PK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m).Inventory[0];

			var hashCode1 = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory1, null);
			var hashCode2 = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory2, null);
			AssertNotNull("Precondition: Has hash code.", hashCode1);
			AssertNotNull("Precondition: Has hash code.", hashCode2);
			AssertEquals("HashCodes should be the same.", hashCode1, hashCode2);
		}

		public void TestGetInventoryHashCode_Excluded_AttributeNeutral_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m).Inventory[0];

			inventory.WI_SerialNumber = "1";
			var originalHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", originalHashCode);

			inventory.WI_SerialNumber = "2";
			var newHashCode = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null);
			AssertNotNull("Precondition: Has hash code.", newHashCode);
			AssertEquals("HashCodes should be the same.", newHashCode, originalHashCode);
		}

		public void TestGetInventoryHashCodeIncludingOrderedSerial_AttributeNeutral_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1").Inventory[0];
			inventory.WI_SerialNumber = "1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var lineWithNoSerial = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var lineWithSerial = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			lineWithSerial.WE_SerialNumber = "1";
			var pick = Helper.CreatePickNew(order);

			var orderedSerialInv = pick.OrderedInventories.Where(o => o.SerialNumber == "1").Single();
			var nonOrderedSerialInv = pick.OrderedInventories.Where(o => o.SerialNumber.IsEmpty).Single();
			AssertEquals("When no serial is ordered, Hash Codes should be the same with attribute neutral.",
				WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null),
				WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, nonOrderedSerialInv));

			AssertNotEquals("When serial is ordered, Hash Codes should not be the same even with attribute neutral.",
				WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, null),
				WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, orderedSerialInv));
			AssertNotEquals("When serial is ordered, Hash Codes should not be the same even with attribute neutral.",
				WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, nonOrderedSerialInv),
				WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(inventory, orderedSerialInv));
		}

		#endregion

		#region Properties

		#region TestIsProductUsingSerialNumberAttribute

		public void TestIsProductUsingSerialNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = FindOrderedInventory(pick, data.Part1);
			AssertEquals(false, orderedInventory.AvailableInventories.IsProductUsingSerialNumberAttribute);

			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			orderedInventory = FindOrderedInventory(pick, data.Part1);
			AssertEquals(true, orderedInventory.AvailableInventories.IsProductUsingSerialNumberAttribute);

			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(false, orderedInventory.AvailableInventories.IsProductUsingSerialNumberAttribute);
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(false, orderedInventory.AvailableInventories.IsProductUsingSerialNumberAttribute);
		}

		#endregion

		#endregion

		#region Implementation

		protected void AssertCorrectOrderByQuantityAvailable(WhsPickOrderedInventory orderedInventory, ZDecimal qty1, ZDecimal qty2, ZDecimal qty3, ZDecimal qty4, ZDecimal qty5)
		{
			AssertEquals(qty1, orderedInventory.AvailableInventories[0].QuantityAvailableToPick);
			AssertEquals(qty2, orderedInventory.AvailableInventories[1].QuantityAvailableToPick);
			AssertEquals(qty3, orderedInventory.AvailableInventories[2].QuantityAvailableToPick);
			AssertEquals(qty4, orderedInventory.AvailableInventories[3].QuantityAvailableToPick);
			AssertEquals(qty5, orderedInventory.AvailableInventories[4].QuantityAvailableToPick);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsPickAvailableInventory(Factory);
		}

		protected override WhsPickAvailableInventoryCollection GetCollectionToTest()
		{
			return new WhsPickAvailableInventoryCollection(Factory);
		}

		new WhsPickAvailableInventoryCollection Collection;

		#endregion
	}
}
