using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	class PickStrategyUSTest : WhsTestCaseWithFactory
	{
		#region TestConstructor_NullThrows

		public void TestConstructor_NullThrows()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickStrategyUS(null));
		}

		#endregion

		#region TestCanInventoryBeAllocated

		#region TestCanInventoryBeAllocated_AvailableInventoryOnlyShowInventoryWithSamePackageGroupId

		public void TestCanInventoryBeAllocated_AvailableInventoryOnlyShowInventoryWithSamePackageGroupId()
		{
			//	Available Inventory should show only inventory that has the same Package Group ID that is ordered
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "ABC", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "XYZ", 5m);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			orderLine.WE_PackageGroupId = "ABC";

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("ABC", orderedInventory.PackageGroupId);
			AssertEquals(50m, orderedInventory.PickLineQuantity);

			pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.PackageGroupId == "ABC" && i.PickLineQuantity == 50m);

			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			var availables = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			availables.Single(i => i.PackageGroupId == "ABC" && i.QuantityAvailableToPick == 200m);
		}

		#endregion

		#region TestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit

		public void TestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit()
		{
			//	All products with the same Package Group ID are in the same Package and cannot be split. 
			//	Therefore if some products in the same package are not ordered, the available inventory should not be displayed.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PickingArea = bondedArea.PK;
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "ABC", 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, locations[0].PK, "123-1", "ABC", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locations[0].PK, "123-1", "XYZ", 10m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m, "", "ABC");

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			var availables = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			availables.Single(i => i.PackageGroupId == "XYZ" && i.PickLineQuantity == 20m && i.QuantityAvailableToPick == 50m);
		}

		#endregion

		#region TestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_SameProductWithDifferentAttributes

		public void TestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_SameProductWithDifferentAttributes()
		{
			//	All products with the same Package Group ID are in the same Package and cannot be split. 
			//	Therefore if some products in the same package are not ordered, the available inventory should not be displayed.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PickingArea = bondedArea.PK;
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "ABC", 10m);
			inventory1.WI_PartAttrib1 = "Red";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locations[0].PK, "123-1", "ABC", 5m);
			inventory2.WI_PartAttrib1 = "Yellow";
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locations[0].PK, "123-1", "XYZ", 10m);
			inventory3.WI_PartAttrib1 = "Red";
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, locations[0].PK, "123-1", "OPQ", 10m);
			inventory4.WI_PartAttrib1 = "Blue";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m, "", "ABC");
			orderLine.WE_PartAttrib1 = "Red";

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(1, orderedInventory.AvailableInventories.Count);
			var availables = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			availables.Single(i => i.PackageGroupId == "XYZ" && i.PickLineQuantity == 20m && i.QuantityAvailableToPick == 50m);
		}

		#endregion

		#region TestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes

		public void TestTestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentPartAttrib2()
		{
			AssertCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes(AttributeNumber.Two, (ZString)"PA2", (ZString)"XPA2");
		}

		public void TestTestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentPartAttrib3()
		{
			AssertCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes(AttributeNumber.Three, (ZString)"PA3", (ZString)"XPA3");
		}

		public void TestTestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_SerialNumber()
		{
			AssertCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes(AttributeNumber.Serial, (ZString)"SN1", (ZString)"SN2");
		}

		public void TestTestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentExpiry()
		{
			var year = ZDate.Today.Year;
			AssertCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes(AttributeNumber.ExpiryDate, new ZDate(year, 1, 1), new ZDate(year, 1, 2));
		}

		public void TestTestCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentPacking()
		{
			var year = ZDate.Today.Year;
			AssertCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes(AttributeNumber.PackingDate, new ZDate(year, 1, 1), new ZDate(year, 1, 2));
		}

		void AssertCanInventoryBeAllocated_AllProductsWithSamePackageGroupIdCannotBeSplit_DifferentAttributes(AttributeNumber attributeNumber, IZType value1, IZType value2)
		{
			//	All products with the same Package Group ID are in the same Package and cannot be split. 
			//	Therefore if some products in the same package are not ordered, the available inventory should not be displayed.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PickingArea = bondedArea.PK;
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0].PK, "123-1", "ABC", 1m);
			SetAttribute(inventory1.InDocketLine, value1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0].PK, "123-1", "ABC", 1m);
			SetAttribute(inventory2.InDocketLine, value2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m, "", "ABC");
			SetAttribute(orderLine, value1);

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(0, orderedInventory.AvailableInventories.Count);

			void SetAttribute(WhsDocketLine docketLine, IZType value)
			{
				switch (attributeNumber)
				{
					case AttributeNumber.One:
						docketLine.WE_PartAttrib1 = (ZString)value;
						break;
					case AttributeNumber.Two:
						docketLine.WE_PartAttrib2 = (ZString)value;
						break;
					case AttributeNumber.Three:
						docketLine.WE_PartAttrib3 = (ZString)value;
						break;
					case AttributeNumber.Serial:
						docketLine.WE_SerialNumber = (ZString)value;
						break;
					case AttributeNumber.ExpiryDate:
						docketLine.WE_ExpiryDate = (ZDate)value;
						break;
					case AttributeNumber.PackingDate:
						docketLine.WE_PackingDate = (ZDate)value;
						break;
				}
			}
		}

		#endregion

		#region TestCanInventoryBeAllocated_AvailableInventoryOnlyShowInventoryWhosePackagesAreAllPickable_Simple

		public void TestCanInventoryBeAllocated_AvailableInventoryOnlyShowInventoryWhosePackagesAreAllPickable_Simple()
		{
			//Furthermore, if at least one Package cannot be picked (based on Per Package Qty of inventory), it should also not be displayed.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "ABC", 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, locations[0].PK, "123-1", "DEF", 3m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, locations[0].PK, "123-1", "XYZ", 5m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(5m, orderedInventory.PickLineQuantity);
			AssertEquals(2, orderedInventory.AvailableInventories.Count);
			var availables = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			availables.Single(i => i.PackageGroupId == "DEF" && i.PerPackageQty == 3m);
			availables.Single(i => i.PackageGroupId == "XYZ" && i.PerPackageQty == 5m);
		}

		#endregion

		#region TestCanInventoryBeAllocated_AvailableInventoryOnlyShowInventoryWhosePackagesAreAllPickable_Complex

		public void TestCanInventoryBeAllocated_AvailableInventoryOnlyShowInventoryWhosePackagesAreAllPickable_Complex()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locations[0].WLV_WA_PutawayArea = bondedArea.PK;
			locations[1].WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, locations[0].PK, "123-1", "ABC", 10m);  // pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, locations[0].PK, "123-1", "ABC", 5m);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, locations[0].PK, "123-1", "DEF", 3m);   // pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locations[0].PK, "123-1", "DEF", 1m);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, locations[0].PK, "123-1", "OPQ", 5m);   // pickable

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 75m, locations[1].PK, "123-1", "RST", 5m);    // not pickable, because Part2 PerPackageQty = 10m 
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 150m, locations[1].PK, "123-1", "RST", 10m);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInventories.Single(oi => oi.SupplierPart.PK == data.Part1.PK && oi.PickLineQuantity == 10m);
			var orderedInventory2 = orderedInventories.Single(oi => oi.SupplierPart.PK == data.Part2.PK && oi.PickLineQuantity == 5m);

			var availables1 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>();
			AssertEquals(3, availables1.Count());
			var groups = availables1.Cast<WhsPickAvailableInventory>().Select(i => i.PackageGroupId);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC", "DEF", "OPQ" }, groups);
			availables1.Single(i => i.PackageGroupId == "ABC" && i.PerPackageQty == 10m);
			availables1.Single(i => i.PackageGroupId == "DEF" && i.PerPackageQty == 3m);
			availables1.Single(i => i.PackageGroupId == "OPQ" && i.PerPackageQty == 5m);

			var availables2 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>();
			AssertEquals(2, availables2.Count());
			var groups2 = availables2.Cast<WhsPickAvailableInventory>().Select(i => i.PackageGroupId);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC", "DEF" }, groups2);
			availables2.Single(i => i.PackageGroupId == "ABC" && i.PerPackageQty == 5m);
			availables2.Single(i => i.PackageGroupId == "DEF" && i.PerPackageQty == 1m);
		}

		#endregion

		#region TestCanInventoryBeAllocated_PackageGroupIdNotEntered

		public void TestCanInventoryBeAllocated_PackageGroupIdNotEntered()
		{
			//	if PkgGroupID is NOT entered on OrderedInventory then allow:
			//	- inventory without PkgGroupID;
			//	- inventory with PkgGroupID that have all other Products from the PkgGroupID ordered + QtyOrdered >= PerPkgQty.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var location = data.Whs1.FindLocation("A-1");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			// No PackageGroupId, PerPackageQty > QuantityOrdered, >> Not Pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, location.PK, "123-1", "", 10m);
			// No PackageGroupId, PerPackageQty < QuantityOrdered, >> Pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 200m, location.PK, "123-1", "", 5m);
			// Has PackageGroupId, but has other product is not ordered >> Not Pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, location.PK, "123-1", "ABC", 3m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 150m, location.PK, "123-1", "ABC", 3m);
			// Has PackageGroupId, and PerPackageQty < QuantityOrdered >> Pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, location.PK, "123-1", "XYZ", 5m);
			// Has PackageGroupId, and PerPackageQty > QuantityOrdered >> Not Pickable
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 150m, location.PK, "123-1", "DEF", 10m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(2, orderedInventory.AvailableInventories.Count);
			var availables = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			availables.Single(i => i.PackageGroupId.IsEmpty && i.PerPackageQty == 5m);
			availables.Single(i => i.PackageGroupId == "XYZ" && i.PerPackageQty == 5m);
		}

		#endregion

		#endregion

		#region TestGetAutoAllocateQuantity

		#region TestGetAutoAllocateQuantity_ItemsWithoutPackageGroup

		public void TestGetAutoAllocateQuantity_ItemsWithoutPackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-1", "", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, "KEY-1", "", 4m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, "KEY-1", "", 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory1 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 3m);
			var availableInventory2 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 4m);
			var availableInventory3 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 5m);

			// if multiple packages can be picked, then the biggest packages should be used.
			var strategy1 = new PickStrategyUS(pick);
			AssertEquals("Only the biggest package should be picked if multiple packages can be used to fulfill order.", 0m, strategy1.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));
			AssertEquals("Only the biggest package should be picked if multiple packages can be used to fulfill order.", 0m, strategy1.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));
			AssertEquals("Only the biggest package should be picked if multiple packages can be used to fulfill order.", 20m, strategy1.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));

			// system should be able to pick different quantities from different inventories to achieve fulfilment
			orderLine.WE_TransactionQuantity = 37m;
			var strategy2 = new PickStrategyUS(pick);
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 3m, strategy2.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1)); // 1x3
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 4m, strategy2.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2)); // 1x4
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 30m, strategy2.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));  // 6x5

			// system should be able to pick different quantities from different inventories to achieve fulfilment
			orderLine.WE_TransactionQuantity = 78m;
			var strategy3 = new PickStrategyUS(pick);
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 0m, strategy3.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1)); // 0x3
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 28m, strategy3.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));  // 7x4
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 50m, strategy3.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));  // 10x5

			// when order cannot be fulfilled it should achieve max possible allocation but not over-allocate
			orderLine.WE_TransactionQuantity = 118m;
			var strategy4 = new PickStrategyUS(pick);
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 27m, strategy4.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));    // 9x3
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 40m, strategy4.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));    // 10x4
			AssertEquals("All three inventories should be picked to fulfil picking requirements.", 50m, strategy4.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));    // 10x5
		}

		#endregion

		#region TestGetAutoAllocateQuantity_StockWithoutPackageGroup_NoSpecificPackageOrdered

		public void TestGetAutoAllocateQuantity_StockWithoutPackageGroup_NoSpecificPackageOrdered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-1", "123", 3m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, "KEY-1", "123", 5m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, "KEY-1", "456", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, "KEY-1", "456", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);
			var availableInventory1 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 3m);
			var availableInventory2 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 5m);
			var availableInventory3 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 5m);
			var availableInventory4 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 3m);

			// system should be able to select correct stock to achieve fulfilment without splitting packages
			var strategy1 = new PickStrategyUS(pick);
			AssertEquals("Correct inventories should be picked.", 6m, strategy1.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory1)); // 2 x 123
			AssertEquals("Correct inventories should be picked.", 10m, strategy1.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory2));  // 2 x 123
			AssertEquals("Correct inventories should be picked.", 0m, strategy1.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory3));
			AssertEquals("Correct inventories should be picked.", 0m, strategy1.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory4));

			// system should be able to select correct stock to achieve fulfilment without splitting packages
			orderLine1.WE_TransactionQuantity = 10m;
			orderLine2.WE_TransactionQuantity = 6m;
			var strategy2 = new PickStrategyUS(pick);
			AssertEquals("Correct inventories should be picked.", 0m, strategy2.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory1));
			AssertEquals("Correct inventories should be picked.", 0m, strategy2.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory2));
			AssertEquals("Correct inventories should be picked.", 10m, strategy2.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory3));  // 2 x 456
			AssertEquals("Correct inventories should be picked.", 6m, strategy2.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory4)); // 2 x 456

			// system should be able to combine multiple packages to achieve fulfilment
			orderLine1.WE_TransactionQuantity = 24m;
			orderLine2.WE_TransactionQuantity = 24m;
			var strategy3 = new PickStrategyUS(pick);
			AssertEquals("Correct inventories should be picked.", 9m, strategy3.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory1)); // 3 x 123
			AssertEquals("Correct inventories should be picked.", 15m, strategy3.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory2));    // 3 x 123
			AssertEquals("Correct inventories should be picked.", 15m, strategy3.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory3));    // 3 x 456
			AssertEquals("Correct inventories should be picked.", 9m, strategy3.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory4)); // 3 x 456
		}

		#endregion

		#region TestGetAutoAllocateQuantity_ItemsWithPackageGroup_WithAPackageGroupOrdered

		public void TestGetAutoAllocateQuantity_ItemsWithPackageGroup_WithAPackageGroupOrdered()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			locationA2.WLV_WA_PutawayArea = bondedArea.PK;
			locationA3.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1_WithoutPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "", 5m);
			var inventory2_WithoutPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1.PK, "KEY-1", "", 5m);
			// 2 packages in A-2
			var inventory1_WithPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA2.PK, "KEY-1", "123", 5m);
			var inventory2_WithPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA2.PK, "KEY-1", "123", 5m);
			var inventory3_WithPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2.PK, "KEY-1", "123", 5m);
			// 2 packages in A-3
			var inventory4_WithPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA3.PK, "KEY-1", "123", 5m);
			var inventory5_WithPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA3.PK, "KEY-1", "123", 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m, "KEY-1", "DummyOutward-1", "123");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m, "KEY-1", "DummyOutward-1", "123");

			// check all stock is picked and only full packages from each location.
			var pick = Helper.CreatePickNew(order);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);
			var availableInventory1_A2 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.Location == locationA2);
			var availableInventory1_A3 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.Location == locationA3);
			var availableInventory2_A2 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.Location == locationA2);
			var availableInventory2_A3 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.Location == locationA3);

			// 2 full packages can be picked from A-2
			var strategy = pick.CurrentPickStrategy;
			AssertEquals(10m, strategy.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory1_A2));
			AssertEquals(10m, strategy.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory2_A2));
			// 2 full packages can be picked from A-3
			AssertEquals(10m, strategy.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory1_A3));
			AssertEquals(10m, strategy.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory2_A3));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_ItemsWithPackageGroup_WithAPackageGroupOrdered_MultiplePackages

		public void TestGetAutoAllocateQuantity_ItemsWithPackageGroup_WithAPackageGroupOrdered_MultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			locationA2.WLV_WA_PutawayArea = bondedArea.PK;
			locationA3.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "123", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1.PK, "KEY-1", "123", 5m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA2.PK, "KEY-1", "456", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2.PK, "KEY-1", "456", 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, "KEY-1", "DummyOutward-1", "123");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m, "KEY-1", "DummyOutward-1", "123");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, "KEY-1", "DummyOutward-1", "456");
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part2, 5m, "KEY-1", "DummyOutward-1", "456");

			// check all stock is picked
			var pick = Helper.CreatePickNew(order);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PackageGroupId == "123" && o.SupplierPart == data.Part1);
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PackageGroupId == "123" && o.SupplierPart == data.Part2);
			var orderedInventory3 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PackageGroupId == "456" && o.SupplierPart == data.Part1);
			var orderedInventory4 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.PackageGroupId == "456" && o.SupplierPart == data.Part2);
			var availableInventory1 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var availableInventory2 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var availableInventory3 = orderedInventory3.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var availableInventory4 = orderedInventory4.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			// 1 full package picked from A-1 and 1 full package from A-2
			var strategy = pick.CurrentPickStrategy;
			AssertEquals(5m, strategy.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, availableInventory1));
			AssertEquals(5m, strategy.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, availableInventory2));
			AssertEquals(5m, strategy.GetAutoAllocateQuantity(orderedInventory3.QuantityShort, availableInventory3));
			AssertEquals(5m, strategy.GetAutoAllocateQuantity(orderedInventory4.QuantityShort, availableInventory4));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_ItemsWithPackageGroup_WithAPackageGroupOrdered_WithReservedStock

		public void TestGetAutoAllocateQuantity_ItemsWithPackageGroup_WithAPackageGroupOrdered_WithReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			// receive 3 packages "123"
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, locationA1.PK, "KEY-1", "123", 4m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 12m, locationA1.PK, "KEY-1", "123", 4m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// order 3 packages and reserves 2 packages worth of Part1
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 12m, "KEY-1", "DummyOutward-1", "123");
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 12m, "KEY-1", "DummyOutward-1", "123");
			Helper.CreateReservePickLine(order1Line1, inventory1, 8m);

			// order 3 packages and reserves 1 package worth of Part2
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var order2Line1 = Helper.CreateWhsOrderLine(order2, data.Part1, 12m, "KEY-1", "DummyOutward-1", "123");
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part2, 12m, "KEY-1", "DummyOutward-1", "123");
			Helper.CreateReservePickLine(order2Line2, inventory2, 4m);

			// order 3 packages
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3", WhsPickOption.Codes.Manual);
			var order3Line1 = Helper.CreateWhsOrderLine(order3, data.Part1, 12m, "KEY-1", "DummyOutward-1", "123");
			var order3Line2 = Helper.CreateWhsOrderLine(order3, data.Part2, 12m, "KEY-1", "DummyOutward-1", "123");

			// only 2 packages should be allocated to order1 since 1 package of Part2 is reserved by another order.
			var pick1 = Helper.CreatePickNew(order1);
			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory2 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);

			var pickStrategy1 = pick1.CurrentPickStrategy;
			AssertEquals(0m, pickStrategy1.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, orderedInventory1.AvailableInventories[0]));
			AssertEquals(8m, pickStrategy1.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, orderedInventory2.AvailableInventories[0]));

			// only 1 package should be allocated to order2 since 2 packages of Part1 are reserved by another order.
			var pick2 = Helper.CreatePickNew(order2);
			var orderedInventory3 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory4 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);
			var pickStrategy2 = pick2.CurrentPickStrategy;
			AssertEquals(4m, pickStrategy2.GetAutoAllocateQuantity(orderedInventory3.QuantityShort, orderedInventory3.AvailableInventories[0]));
			AssertEquals(0m, pickStrategy2.GetAutoAllocateQuantity(orderedInventory4.QuantityShort, orderedInventory4.AvailableInventories[0]));

			// only 1 package should be allocated to order3 since 2 packages of Part1 and 1 package of Part2 are reserved by other orders.
			var pick3 = Helper.CreatePickNew(order3);
			var orderedInventory5 = pick3.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory6 = pick3.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);
			var pickStrategy3 = pick3.CurrentPickStrategy;
			AssertEquals(4m, pickStrategy3.GetAutoAllocateQuantity(orderedInventory5.QuantityShort, orderedInventory5.AvailableInventories[0]));
			AssertEquals(4m, pickStrategy3.GetAutoAllocateQuantity(orderedInventory6.QuantityShort, orderedInventory6.AvailableInventories[0]));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_NoExceptionWithMixedOrders

		[ExpectNoExceptions]
		public void TestGetAutoAllocateQuantity_NoExceptionWithMixedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "123", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, locationA1.PK, "KEY-1", "123", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// order 1 package
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m, "KEY-1", "DummyOutward-1", "123");
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m, "KEY-1", "DummyOutward-1", "");

			var pick1 = Helper.CreatePickNew(order1);
			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory2 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);

			var pickStrategy1 = pick1.CurrentPickStrategy;
			AssertEquals(5m, pickStrategy1.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, orderedInventory1.AvailableInventories[0]));
			AssertEquals(3m, pickStrategy1.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, orderedInventory2.AvailableInventories[0]));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_CachesValuesForOrderedGroups

		public void TestGetAutoAllocateQuantity_CachesValuesForOrderedGroups()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "123", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, locationA1.PK, "KEY-1", "123", 3m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// order 1 package
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m, "KEY-1", "DummyOutward-1", "123");
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 4m, "KEY-1", "DummyOutward-1", "");

			var pick1 = Helper.CreatePickNew(order1);
			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory2 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);

			var pickStrategy1 = pick1.CurrentPickStrategy;
			AssertEquals(5m, pickStrategy1.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, orderedInventory1.AvailableInventories[0]));
			AssertEquals(3m, pickStrategy1.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, orderedInventory2.AvailableInventories[0]));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_ChecksAttribute

		public void TestGetAutoAllocateQuantity_ChecksPartAttrib1()
		{
			AssertGetAutoAllocateQuantity_ChecksAttribute(AttributeNumber.One, (ZString)"PA1", (ZString)"AAA");
		}

		public void TestGetAutoAllocateQuantity_ChecksPartAttrib2()
		{
			AssertGetAutoAllocateQuantity_ChecksAttribute(AttributeNumber.Two, (ZString)"PA2", (ZString)"AAA");
		}

		public void TestGetAutoAllocateQuantity_ChecksPartAttrib3()
		{
			AssertGetAutoAllocateQuantity_ChecksAttribute(AttributeNumber.Three, (ZString)"PA3", (ZString)"AAA");
		}

		public void TestGetAutoAllocateQuantity_ChecksExpiryDate()
		{
			var year = ZDate.Today.Year;
			AssertGetAutoAllocateQuantity_ChecksAttribute(AttributeNumber.ExpiryDate, new ZDate(year + 2, 1, 1), new ZDate(year + 1, 1, 1));
		}

		public void TestGetAutoAllocateQuantity_ChecksPackingDate()
		{
			var year = ZDate.Today.Year;
			AssertGetAutoAllocateQuantity_ChecksAttribute(AttributeNumber.PackingDate, new ZDate(year + 2, 1, 1), new ZDate(year + 1, 1, 1));
		}

		void AssertGetAutoAllocateQuantity_ChecksAttribute(AttributeNumber attributeNumber, IZType value1, IZType value2)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, attributeNumber, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "123", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, locationA1.PK, "KEY-1", "123", 3m);
			SetAttribute(inventory2.InDocketLine, value1);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// order 1 package
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, "KEY-1", "DummyOutward-1", "123");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, "KEY-1", "DummyOutward-1", "");
			SetAttribute(orderLine2, value2);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, "KEY-1", "DummyOutward-1", "");
			SetAttribute(orderLine3, value1);

			var pick = Helper.CreatePickNew(order);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2 && o.AvailableInventories.Count > 0);

			var pickStrategy = pick.CurrentPickStrategy;
			AssertEquals(5m, pickStrategy.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, orderedInventory1.AvailableInventories[0]));
			AssertEquals(3m, pickStrategy.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, orderedInventory2.AvailableInventories[0]));

			void SetAttribute(WhsDocketLine docketLine, IZType value)
			{
				switch (attributeNumber)
				{
					case AttributeNumber.One:
						docketLine.WE_PartAttrib1 = (ZString)value;
						break;
					case AttributeNumber.Two:
						docketLine.WE_PartAttrib2 = (ZString)value;
						break;
					case AttributeNumber.Three:
						docketLine.WE_PartAttrib3 = (ZString)value;
						break;
					case AttributeNumber.Serial:
						docketLine.WE_SerialNumber = (ZString)value;
						break;
					case AttributeNumber.ExpiryDate:
						docketLine.WE_ExpiryDate = (ZDate)value;
						break;
					case AttributeNumber.PackingDate:
						docketLine.WE_PackingDate = (ZDate)value;
						break;
				}
			}
		}

		#endregion

		#region TestGetAutoAllocateQuantity_BondedEntryKey

		public void TestGetAutoAllocateQuantity_BondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bonded = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var location = data.Whs1.FindLocation("A");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, "KEY-1", "PACK-1", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, "KEY-2", "PACK-2", 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m, "KEY-1", "DummyOutward-1", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m, "KEY-2", "DummyOutward-2", "");

			var pick = Helper.CreatePickNew(order);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.BondedEntryKey == "KEY-1");
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.BondedEntryKey == "KEY-2");

			var pickStrategy = pick.CurrentPickStrategy;
			AssertEquals(20m, pickStrategy.GetAutoAllocateQuantity(orderedInventory1.QuantityShort, orderedInventory1.AvailableInventories[0]));
			AssertEquals(20m, pickStrategy.GetAutoAllocateQuantity(orderedInventory2.QuantityShort, orderedInventory2.AvailableInventories[0]));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_Performance_DifferentProductsWithPackageGroups

		public void TestGetAutoAllocateQuantity_Performance_DifferentProductsWithPackageGroups()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var numberOfProducts = 10;
			for (int i = 0; i < numberOfProducts; i++)
			{
				var part = Helper.CreateProduct(data.Org1, $"Part0{i}");
				Factory.Save();
				Helper.CreateWhsReceiveInventoryLine(receive, part, 30m, $"KEY1-{i + 1}", "", 3m);
				Helper.CreateWhsReceiveInventoryLine(receive, part, 40m, $"KEY1-{i + 1}", "", 4m);
				Helper.CreateWhsReceiveInventoryLine(receive, part, 50m, $"KEY1-{i + 1}", "", 5m);

				Helper.CreateWhsOrderLine(order, part, 117m, $"KEY1-{i + 1}", "DummyOutward-1", "");
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pick = Helper.CreatePickNew(order);

			var pickStrategy = pick.CurrentPickStrategy;
			for (int i = 0; i < numberOfProducts; i++)
			{
				var orderedInventory = pick.OrderedInventories[i];
				var availableInventory1 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 3m);
				var availableInventory2 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 4m);
				var availableInventory3 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 5m);

				AssertEquals("Should be able to allocate.", 27m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));  // 9x3
				AssertEquals("Should be able to allocate.", 40m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));  // 10x4
				AssertEquals("Should be able to allocate.", 50m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));  // 10x5
			}
		}

		#endregion

		#region TestGetAutoAllocateQuantity_Performance_ShouldNotTryToFindBestMatchWhenOrderIsMoreThanAvailable

		public void TestGetAutoAllocateQuantity_Performance_ShouldNotTryToFindBestMatchWhenOrderIsMoreThanAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var numberOfProducts = 5;
			var numberOfDifferentPackages = 10;
			for (int i = 0; i < numberOfProducts; i++)
			{
				var part = Helper.CreateProduct(data.Org1, $"Part0{i}");
				Factory.Save();
				for (int j = 1; j <= numberOfDifferentPackages; j++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, part, j * 10m, $"KEY1-{i + 1}", "", j);
				}

				Helper.CreateWhsOrderLine(order, part, 1000m, $"KEY1-{i + 1}", "DummyOutward-1", ""); // just more than available
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pick = Helper.CreatePickNew(order);
			var pickStrategy = pick.CurrentPickStrategy;
			for (int i = 0; i < numberOfProducts; i++)
			{
				var orderedInventory = pick.OrderedInventories[i];
				for (int j = 1; j <= numberOfDifferentPackages; j++)
				{
					var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PerPackageQty == j);
					AssertEquals("Should be able to allocate all available.", j * 10m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory));
				}
			}
		}

		#endregion

		#region TestGetAutoAllocateQuantity_FindTheBestAllocationWhenNotFulfilled

		public void TestGetAutoAllocateQuantity_FindTheBestAllocationWhenNotFulfilled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			var partA = Helper.CreateProduct(data.Org1, $"PartA");
			var partB = Helper.CreateProduct(data.Org1, $"PartB");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, partA, 60m, $"KEY1-1", "", 6m);
			Helper.CreateWhsReceiveInventoryLine(receive, partA, 40m, $"KEY1-1", "", 4m);
			Helper.CreateWhsReceiveInventoryLine(receive, partB, 60m, $"KEY1-2", "", 6m);
			Helper.CreateWhsReceiveInventoryLine(receive, partB, 40m, $"KEY1-2", "", 4m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, partA, 9m, $"KEY1-1", "DummyOutward-1", "");
			Helper.CreateWhsOrderLine(order, partB, 7m, $"KEY1-2", "DummyOutward-1", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pick = Helper.CreatePickNew(order);

			var strategy = pick.CurrentPickStrategy;
			var orderedInventoryA = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == partA);
			var availableInventoryA6 = orderedInventoryA.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 6m);
			var availableInventoryA4 = orderedInventoryA.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 4m);

			var orderedInventoryB = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == partB);
			var availableInventoryB6 = orderedInventoryB.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 6m);
			var availableInventoryB4 = orderedInventoryB.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(j => j.PerPackageQty == 4m);

			AssertEquals("Should be able to allocate. The biggest package is not the best allocate.", 0m, strategy.GetAutoAllocateQuantity(orderedInventoryA.QuantityShort, availableInventoryA6));
			AssertEquals("Should be able to allocate. The biggest package is not the best allocate.", 8m, strategy.GetAutoAllocateQuantity(orderedInventoryA.QuantityShort, availableInventoryA4));

			AssertEquals("Should be able to allocate. The biggest package is the best allocate.", 6m, strategy.GetAutoAllocateQuantity(orderedInventoryB.QuantityShort, availableInventoryB6));
			AssertEquals("Should be able to allocate. The biggest package is the best allocate.", 0m, strategy.GetAutoAllocateQuantity(orderedInventoryB.QuantityShort, availableInventoryB4));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_AddSingleProductPackagesForAllocations_LargeUnits

		public void TestGetAutoAllocateQuantity_AddSingleProductPackagesForAllocations_LargeUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var maxIntFitInBondedWhsQty = 999999999999999; // decimal(18, 3) WB_BondedWhsQty
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, maxIntFitInBondedWhsQty, "KEY-1", "", 3m);
			receive.WD_TotalCubic = 0m;
			receive.WD_TotalWeight = 0m;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m, WhsPickOption.Codes.Manual);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 3m);

			var strategy = new PickStrategyUS(pick);
			AssertEquals("Should be able to allocate quantity.", 18m, strategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory));
		}

		#endregion

		#region TestGetAutoAllocateQuantity_AddSingleProductPackagesForAllocations_AccumulateQty

		public void TestGetAutoAllocateQuantity_AddSingleProductPackagesForAllocations_AccumulateQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var maxIntFitInBondedWhsQty = 999999999999999; // decimal(18, 3) WB_BondedWhsQty
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, maxIntFitInBondedWhsQty, "KEY-1", "", 3m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, maxIntFitInBondedWhsQty, "KEY-1", "", 3m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, maxIntFitInBondedWhsQty, "KEY-1", "", 3m);
			receive.WD_TotalCubic = 0m;
			receive.WD_TotalWeight = 0m;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m, WhsPickOption.Codes.Manual);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PerPackageQty == 3m);

			var strategy = new PickStrategyUS(pick);
			AssertEquals("Should be able to allocate quantity.", 18m, strategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory));
		}

		#endregion

		#endregion

		#region TestCanAllocateInQuantitiesDifferentToAutoAllocateQuantity

		public void TestCanAllocateInQuantitiesDifferentToAutoAllocateQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventoryNoPackageGroupIDOrPackQty = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, "KEY-1", "", 0m);
			var inventoryNoPackageGroupID = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, "KEY-1", "", 4m);
			// Package Group ID + no PerPackQty has validation errors that prevent finalisation
			var inventoryWithPackageGroupIDAndPackQty = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 60m, "KEY-1", "PCK", 5m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 30m, "KEY-1", "DummyOutward-1", "");
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 40m, "KEY-1", "DummyOutward-1", "");
			var order1Line3 = Helper.CreateWhsOrderLine(order1, data.Part1, 50m, "KEY-1", "DummyOutward-1", "");
			var order1Line4 = Helper.CreateWhsOrderLine(order1, data.Part1, 60m, "KEY-1", "DummyOutward-1", "PCK");

			var pick = Helper.CreatePickNew(order1);

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInventories.Single(ordInv => ordInv.PackageGroupId.IsEmpty);
			var orderedInventory2 = orderedInventories.Single(ordInv => ordInv.PackageGroupId == "PCK");

			var orderedInv1AvailInvs = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>();
			var orderedInv1AvailInv1 = orderedInv1AvailInvs.Single(availInv => availInv.PerPackageQty == 0m);
			var orderedInv1AvailInv2 = orderedInv1AvailInvs.Single(availInv => availInv.PerPackageQty == 4m);
			var orderedInv1AvailInv3 = orderedInv1AvailInvs.Single(availInv => availInv.PerPackageQty == 5m);

			var orderedInv2AvailInv = (WhsPickAvailableInventory)orderedInventory2.AvailableInventories.Single();

			var strategy1 = new PickStrategyUS(pick);
			AssertEquals(true, strategy1.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(orderedInv1AvailInv1));
			AssertEquals(false, strategy1.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(orderedInv1AvailInv2));
			AssertEquals(false, strategy1.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(orderedInv1AvailInv3));
			AssertEquals(false, strategy1.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(orderedInv2AvailInv));
		}

		#endregion
	}
}
