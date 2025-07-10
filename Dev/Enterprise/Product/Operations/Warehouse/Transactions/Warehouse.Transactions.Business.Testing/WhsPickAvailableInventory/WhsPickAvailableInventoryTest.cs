using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Business;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickAvailableInventory))]
	class WhsPickAvailableInventoryTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region TestPerformance

		#region TestPerformance_PickLines_DoesNotCreateNewActiveCollections

		public void TestPerformance_PickLines_DoesNotCreateNewActiveCollections()
		{
			// for performance reasons
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			AssertEquals("Precondition.", 10, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", 10, orderedInventory.PickLines.Count);

			var numberOfPickLineCollections = GenericTestHelper.GetNumberOfActiveBusinessObjectCollections<WhsPickLine>(Factory);
			AssertEquals("Should have 1 collection for each inventory, 1 for order line and 1 for ordered inventory (10 + 1 + 1).", 12, numberOfPickLineCollections);
		}

		#endregion

		#region TestPerformance_VerifiedNonEmpty

		public void TestPerformance_VerifiedNonEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			data.Whs1.WW_VerifyEmptyLocations = true;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < 10; i++)
			{
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory.WI_SerialNumber = "SN" + i;
			}
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			AssertEquals("Precondition.", 5, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", 5, orderedInventory.PickLines.Count);

			AssertPersistentPropertiesHitCount("VerifyNonEmtpy should not access pick line properties extensivelly.", typeof(WhsPickLine), 1050, () => // needs futher reduction
			{
				foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
				{
					_ = availableInventory.VerifiedNonEmpty;
				}
			});
		}

		#endregion

		#region TestPerformance_VerifiedNonEmptyUseCacheInFactorySave

		public void TestPerformance_VerifiedNonEmptyUseCacheInFactorySave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventories = pick.OrderedInventories;
			var availableInventories = orderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Only one OrderedInventories.", 1, orderedInventories.Count);
			AssertEquals("Precondition: Only one AvailableInventories.", 1, orderedInventories[0].AvailableInventories.Count);

			var assertionHit = false;
			var receiveLine = receive.Lines[0];
			receiveLine.Location.WLV_MaximumPickCountBeforeAutomatedStocktake = 1;
			receiveLine.Location.WLV_FinalisedPickCountInfo.ValueChanged += (s, e) =>
			{
				// poke to have cache lazy calculated
				_ = availableInventories.VerifiedNonEmpty;

				assertionHit = true;
				availableInventories.VerifiedNonEmpty = true;
				AssertEquals("The cache value should be used during the save process.", false, availableInventories.VerifiedNonEmpty);
			};

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();
			AssertEquals(true, assertionHit);

			AssertEquals("The cache value should not be used during the save process.", true, availableInventories.VerifiedNonEmpty);

			availableInventories.VerifiedNonEmpty = false;
			AssertEquals("The cache value should not be used during the save process.", false, availableInventories.VerifiedNonEmpty);
		}

		#endregion

		#endregion

		#region Business Object Overrides

		#region TestDelete

		public void TestDelete()
		{
			SetupWhsPickOrderedInventory();

			var availableInventory = AvailableInventory;
			var owner = availableInventory.OrderedInventory.Owners[0];
			var inventory = availableInventory.Inventory[0];
			var child1 = owner.PickLines.AddNew();
			var child2 = owner.PickLines.AddNew();
			child1.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			child2.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			var reservedPickLine = Helper.CreateReservePickLine(availableInventory.OrderedInventory.Owners[0], availableInventory.Inventory[0], 1m);
			((IBusinessObjectInternals)reservedPickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m; // since pick line is attached to pick, need to set this field via row.

			AssertContainsExactElementsInAnyOrder(new[] { child1, child2, reservedPickLine }, availableInventory.PickLines);

			availableInventory.Delete();
			AssertEquals(0, availableInventory.PickLines.Count());
			AssertEquals(true, child1.IsDeleted);
			AssertEquals(true, child2.IsDeleted);
			AssertEquals(false, reservedPickLine.IsDeleted);
		}

		#endregion

		#region TestRunPreSaveValidation

		public void TestRunPreSaveValidation_RebuildsAvailableInventoriesSplitByPickedDetailsIfNecessary()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var inventorySplit = availableInventory.AvailableInventoriesSplitByPickedDetails.Single();
			orderLine.PickLines[0].Delete();
			availableInventory.ClearPickLinesCache();
			AssertEquals("Inventory Split should not be rebuilt yet.", false, inventorySplit.IsDeleted);

			availableInventory.RunPreSaveValidation();
			AssertEquals("Inventory Split should have been rebuilt on RunPreSaveValidation().", true, inventorySplit.IsDeleted);
		}

		public void TestRunPreSaveValidation_RebuildsAvailableInventoriesSplitByUOMIfNecessary()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var inventorySplit = availableInventory.AvailableInventoriesSplitByUOM.Single();
			orderLine.PickLines[0].Delete();
			availableInventory.ClearPickLinesCache();
			AssertEquals("Inventory Split should not be rebuilt yet.", false, inventorySplit.IsDeleted);

			availableInventory.RunPreSaveValidation();
			AssertEquals("Inventory Split should have been rebuilt on RunPreSaveValidation().", true, inventorySplit.IsDeleted);
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestInventory

		public void TestInventory()
		{
			AssertNotNull(AvailableInventory.Inventory);
			AssertEquals("Should not be registered as child", false, AvailableInventory.IsRegisteredEditableChildObject(AvailableInventory.Inventory));
		}

		#endregion

		#region TestAvailableInventoriesSplitByUOM

		public void TestAvailableInventoriesSplitByUOM()
		{
			AssertNotNull(AvailableInventory.AvailableInventoriesSplitByUOM);
			AssertEquals(typeof(WhsPickAvailableInventorySplitByUOMCollection), AvailableInventory.AvailableInventoriesSplitByUOM.GetType());
			Assert("Should be registered as child", AvailableInventory.IsRegisteredEditableChildObject(AvailableInventory.AvailableInventoriesSplitByUOM));
		}

		#endregion

		#region TestAvailableInventriesSplitByPickedDetails

		public void TestAvailableInventriesSplitByPickedDetails()
		{
			AssertNotNull(AvailableInventory.AvailableInventoriesSplitByPickedDetails);
			AssertType<WhsPickAvailableInventorySplitByPickedDetailsCollection>(AvailableInventory.AvailableInventoriesSplitByPickedDetails);
			Assert("Should be registered as child", AvailableInventory.IsRegisteredEditableChildObject(AvailableInventory.AvailableInventoriesSplitByPickedDetails));
		}

		#endregion

		#region TestArea

		public void TestArea()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			WhsPick pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AvailableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull(AvailableInventory.Location.PickingArea);
			AssertEquals(AvailableInventory.Location.PickingArea, AvailableInventory.Area);

			AvailableInventory.Location.PickingArea.WA_Name = "A1";
			AvailableInventory.Location.PickingArea.WA_AreaType = "AT1";
			Factory.Save();
			AssertEquals("A1", AvailableInventory.AreaName);
			AssertEquals("AT1", AvailableInventory.AreaType);
		}

		#endregion

		#region TestClient

		public void TestClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Client on Available Inventory should match the Docket's Client.", data.Org1, availableInventory.Client);
		}

		#endregion

		#region TestPick

		#region TestUpdatePickTotals_Suspend

		[GuiTest]
		public void TestUpdatePickTotals_Suspend()
		{
			PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KG");
			PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "M3");
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("Part3", data.Org1);
			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 1, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(part3, 1, "KG", 1, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.DefaultLocation, "111");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m, data.Whs1.DefaultLocation, "111");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 1m, data.Whs1.DefaultLocation, "111");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m, WhsPickOption.Codes.Manual);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m, WhsPickOption.Codes.Manual);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", part3, 1m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			_ = pick.TotalWeight;
			AssertEquals("Precondition: TotalWeight should be 0 KG.", 0m, pick.TotalWeight);
			AssertEquals("Precondition: TotalVolume should be 0 M3.", 0m, pick.TotalVolume);
			AssertEquals("Precondition: TotalLineQty should be 0.", 0m, pick.TotalLineQty);

			int assertionHitCount = 0;
			pick.TotalWeightInfo.ValueChanged += (sender, e) => assertionHitCount++;

			pick.AutoAllocateItems();
			AssertEquals("Pick Totals Update should be updated after AutoAllocate.", 3m, pick.TotalWeight);
			AssertEquals("Pick Totals Update should be updated after AutoAllocate.", 3m, pick.TotalVolume);
			AssertEquals("Pick Totals Update should be updated after AutoAllocate.", 3m, pick.TotalLineQty);
			AssertEquals("Totals should only change once during AutoAllocate.", 1, assertionHitCount);

			assertionHitCount = 0;
			pick.ClearAllocatedItems();
			AssertEquals("Pick Totals Update should be updated after ClearAllocated.", 0m, pick.TotalWeight);
			AssertEquals("Pick Totals Update should be updated after ClearAllocated.", 0m, pick.TotalVolume);
			AssertEquals("Pick Totals Update should be updated after ClearAllocated.", 0m, pick.TotalLineQty);
			AssertEquals("Totals should only change once during ClearAllocated.", 1, assertionHitCount);
		}

		#endregion

		#endregion

		#region TestPickLines

		#region TestPickLines

		public void TestPickLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org2, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			orderLine1.WE_BondedEntryKey = "BEK21-3";
			orderLine1.WE_ExpiryDate = ZDate.Today.AddMonths(1);
			orderLine1.WE_PackingDate = ZDate.Today.AddMonths(-1);
			orderLine1.WE_PartAttrib1 = "PA1";
			orderLine1.WE_PartAttrib2 = "PA22";
			orderLine1.WE_PartAttrib3 = "PA3";
			orderLine1.CustomsData.WB_EntryKey = "ABC";

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should be 1 Pickline", 1, availableInventory.PickLines.Count());

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			var pickLinePK = availableInventory.PickLines.Single().PK;

			// PickLine from DB should get loaded when Pick.OrderedInventories.AvailableInventories.PickLines is touched
			var availableInventoryInOtherFactory = pickInOtherFactory.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(1, availableInventoryInOtherFactory.PickLines.Count());
			AssertEquals(pickLinePK, availableInventoryInOtherFactory.PickLines.Single().PK);
		}

		#endregion

		#region TestPickLines_IsUpdatedCorrectly

		public void TestPickLines_IsUpdatedCorrectly()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			availableInventory.PickLineQuantity = 10m;
			AssertContainsExactElementsInAnyOrder(new[] { orderLine.PickLines.Single() }, availableInventory.PickLines);

			availableInventory.PickLineQuantity = 15m;
			AssertContainsExactElementsInAnyOrder(new[] { receive1.Lines.Single().PK, receive2.Lines.Single().PK }, orderLine.PickLines.Select(pl => pl.WZ_WE_InventoryLine));
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, availableInventory.PickLines);

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());
		}

		#endregion

		#region TestPickLines_OnSplitDetails_IsUpdatedCorrectly

		public void TestPickLines_OnSplitDetails_IsUpdatedCorrectly()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			availableInventory.PickLineQuantity = 10m;
			var pickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);
			AssertEquals("Should have 1 Inventory Split.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);

			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			var newPickLine = inTransitLine.PickLines.Single();

			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { newPickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
		}

		#endregion

		#region TestPickLines_CommittedFromAllPicks

		public void TestPickLines_CommittedFromAllPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 15);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 25);

			var pick1 = Helper.CreatePickNew(false, false, order1);
			Helper.CreatePickNew(false, false, order2);
			Helper.CreatePickNew(true, true, order3);

			var availableInventory = pick1.OrderedInventories[0].AvailableInventories[0];

			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(2, availableInventory.PickLinesCommittedFromAllPicks.Count);
			AssertEquals(10m, availableInventory.PickLinesCommittedFromAllPicks[0].WZ_Units);
			AssertEquals(15m, availableInventory.PickLinesCommittedFromAllPicks[1].WZ_Units);
		}

		#endregion

		#region TestPickLines_DBHits

		public void TestPickLines_DBHits() => TestPickLines_DBHitsCore();

		public void TestPickLines_DBHits_HeldGoodsForOrdersDisabled() => TestPickLines_DBHitsCore(enableHeldGoodsForOrders: false);

		void TestPickLines_DBHitsCore(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				for (int i = 0; i < 5; i++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA2", "", "", "");
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA3", "", "", "");
				}
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
				CreateWhsOrderLine(order1, data.Part1, 1m, "PA1");
				CreateWhsOrderLine(order1, data.Part1, 1m, "PA1");
				CreateWhsOrderLine(order1, data.Part1, 2m, "PA2");

				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR2", Notify);
				CreateWhsOrderLine(order2, data.Part1, 2m, "PA3");

				var pick = Helper.CreatePickByAttachingOrders(order1, order2);
				pick.AutoAllocateItemsWithMock();

				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
				var availableInventory = pickInOtherFactory.OrderedInventories[0].AvailableInventories[0]; // use to load all other data except PickLines, that way we can see how much DB hits required to load PickLines
				int initialDBHits = otherFactory.DatabaseLoadCount;
				AssertEquals(2, pickInOtherFactory.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
				AssertEquals(2, pickInOtherFactory.OrderedInventories[1].AvailableInventories[0].PickLines.Count());
				AssertEquals(2, pickInOtherFactory.OrderedInventories[2].AvailableInventories[0].PickLines.Count());

				var expectedHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 }
				};

				AssertDbHits(expectedHits, otherFactory);
			}
		}

		WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZString partAttribute1)
		{
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.WE_PartAttrib1 = partAttribute1;
			return orderLine;
		}

		#endregion

		#endregion

		#region TestIsPickLineForInventory

		public void TestIsPickLineForInventory()
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

			AssertEquals("Inventory on receive1 should be for available inventory 1.", true, availInv1.IsPickLineForInventory(pickLine1));
			AssertEquals("Inventory on receive2 should *not* be for available inventory 1.", false, availInv1.IsPickLineForInventory(pickLine2));

			AssertEquals("Inventory on receive1 should *not* be for available inventory 2.", false, availInv2.IsPickLineForInventory(pickLine1));
			AssertEquals("Inventory on receive2 should be for available inventory 2.", true, availInv2.IsPickLineForInventory(pickLine2));
		}

		#endregion

		#endregion

		#region Validation

		public void TestValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(typeof(WhsPickAvailableInventoryValidation), availableInventory.Validation.GetType());
		}

		public void TestValidationUS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA1 = data.Whs1.DefaultLocation;
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(typeof(WhsPickAvailableInventoryValidationUS), availableInventory.Validation.GetType());
		}

		#endregion

		#region Properties

		#region ProductCode

		public void TestProductCodeInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.ProductCode, AvailableInventory.ProductCodeInfo.Name);
		}

		#endregion

		#region LocationString

		public void TestLocationStringInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.LocationString, AvailableInventory.LocationStringInfo.Name);
		}

		public void TestLocationString()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var location = data.Whs1.FindLocation("A-2-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("A-2-1", availableInventory.LocationString);
			AssertEquals(location.PK, availableInventory.LocationPK);
		}

		public void TestLocationString_UserFriendly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z040302");
			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 10m, location, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Z-04-03-02", availableInventory.LocationString);
			AssertEquals(location.PK, availableInventory.LocationPK);
		}

		#endregion

		#region LocationStatus

		public void TestLocationStatusAndDescription()
		{
			SetupWhsPickOrderedInventory();
			WhsInventoryView inventory = AvailableInventory.Inventory[0];
			inventory.LocationString = "A-2-1";
			((IWhsPickAvailableInventoryInternals)AvailableInventory).SetAllProperties(AvailableInventory.OrderedInventory, inventory);
			AssertEquals("Location status = Normal", LocationStatus.Codes.Normal, AvailableInventory.LocationStatus);
			AssertEquals("Location status Desc should be empty if Location Status = Normal", "", AvailableInventory.LocationStatusDesc);

			inventory.Location.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			AssertEquals(LocationStatus.Codes.Damaged, AvailableInventory.LocationStatus);
			AssertEquals(LocationStatus.Descriptions.Damaged, AvailableInventory.LocationStatusDesc);

			var face = AvailableInventory.WhsProduct.PickFaces.AddNew();
			face.WF_WL = inventory.WI_WL;
			AssertEquals(LocationStatus.Codes.Damaged, AvailableInventory.LocationStatus);
			AssertEquals("Face, " + LocationStatus.Descriptions.Damaged, AvailableInventory.LocationStatusDesc);
			AssertEquals("A-2-1", AvailableInventory.LocationString);
		}

		public void TestLocationStatusDescInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.LocationStatusDesc, AvailableInventory.LocationStatusDescInfo.Name);
		}

		#endregion

		#region InventoryStatus

		public void TestInventoryStatusDescInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.InventoryStatusDesc, AvailableInventory.InventoryStatusDescInfo.Name);
		}

		#endregion

		#region ArrivalDate

		public void TestArrivalDateInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.ArrivalDate, AvailableInventory.ArrivalDateInfo.Name);
		}

		#endregion

		#region PalletID

		public void TestPalletIDInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PalletID, AvailableInventory.PalletIDInfo.Name);
		}

		#endregion

		#region HoldCode

		public void TestHoldCode_HEL() => TestHoldCodeCore(InventoryHoldCodes.Codes.Held);

		public void TestHoldCode_DAM() => TestHoldCodeCore(InventoryHoldCodes.Codes.Damaged);

		public void TestHoldCode_HEL_HeldGoodsForOrdersDisabled() => TestHoldCodeCore(InventoryHoldCodes.Codes.Held, enableHeldGoodsForOrders: false);

		public void TestHoldCode_DAM_HeldGoodsForOrdersDisabled() => TestHoldCodeCore(InventoryHoldCodes.Codes.Damaged, enableHeldGoodsForOrders: false);

		void TestHoldCodeCore(string holdCode, bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, location, "", InventoryStatus.Codes.Held, holdCode);
				receive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				orderLine.WE_WHC_NKOrderedHeldCode = holdCode;

				var pick = Helper.CreatePickNew(order);

				if (enableHeldGoodsForOrders)
				{
					var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
					AssertEquals(holdCode, availableInventory.HoldCode);
				}
				else
				{
					AssertEquals("No held inventory", 0, pick.OrderedInventories[0].AvailableInventories.Count);
				}
			}
		}

		public void TestHoldCodeInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.HoldCode, AvailableInventory.HoldCodeInfo.Name);
		}

		#endregion

		#region PackageGroupId

		public void TestPackageGroupId()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PackageGroupId, AvailableInventory.PackageGroupIdInfo.Name);
		}

		#endregion

		#region PerPackageQty

		public void TestPerPackageQty()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PerPackageQty, AvailableInventory.PerPackageQtyInfo.Name);
		}

		#endregion

		#region QuantityUQ

		public void TestQuantityUQ()
		{
			AssertEquals("UNT", AvailableInventory.QuantityUQ);
		}

		public void TestQuantityUQInfo()
		{
			AssertEquals("QuantityUQ", AvailableInventory.QuantityUQInfo.Name);
		}

		#endregion

		#region Allocate

		#region TestAllocate

		public void TestAllocate()
		{
			SetupWhsPickOrderedInventory();

			AvailableInventory.Allocate = true;
			AssertEquals(true, AvailableInventory.Allocate);
			AvailableInventory.Allocate = false;
			AssertEquals(false, AvailableInventory.Allocate);
		}

		#endregion

		#region TestAllocate_ActuallyAllocatesStock

		public void TestAllocate_ActuallyAllocatesStock()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 80m);

			WhsOrder order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 30m);
			Helper.CreateReservePickLine(orderLine2, data.Line111, 30m);

			WhsPick pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			WhsPickOrderedInventory orderedInventory = pick.OrderedInventories[0];
			AvailableInventory = orderedInventory.AvailableInventories[0];

			AvailableInventory.Allocate = true;
			AssertAllocatedItem(orderedInventory, AvailableInventory, 70m, 10m, true);

			AvailableInventory.PickLineQuantity = 50m;
			AssertAllocatedItem(orderedInventory, AvailableInventory, 50m, 30m, true);

			AvailableInventory.Allocate = true;
			AssertAllocatedItem(orderedInventory, AvailableInventory, 70m, 10m, true);

			AvailableInventory.Allocate = false;
			AssertAllocatedItem(orderedInventory, AvailableInventory, 0m, 80m, false);
		}

		#endregion

		//#region TestUSBonded_ChangingOrderWE_TransactionQuantity_ActuallyDeAllocateAvailableInventories

		//public void TestUSBonded_ChangingOrderWE_TransactionQuantity_ActuallyDeAllocateAvailableInventories()
		//{
		//	var data = new TestDataSimpleEnvironment(Factory, 3, 1);
		//	data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
		//	Helper.EnableWarehouseForBond(data.Whs1, true);

		//	var locations = data.Whs1.Rows[0].Locations;
		//	var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
		//	receive.WD_DocketSubType = ReceiveType.Codes.Customs;
		//	Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, locations[0].PK, "123-1", "ABC", 2m);

		//	receive.FinaliseDocket();
		//	AssertIsFinalisedPrecondition(receive);

		//	Factory.Save();

		//	var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
		//	var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
		//	orderLine.WE_PackageGroupId = "ABC";

		//	var pick = Helper.CreatePickNew(order);

		//	var orderedInventory = pick.OrderedInventories[0];
		//	var availableInventory = orderedInventory.AvailableInventories[0];
		//	availableInventory.Allocate = true;
		//	AssertAllocatedItem(orderedInventory, availableInventory, 6m, 0m, true);

		//	orderLine.WE_TransactionQuantity = 4m;
		//	order.IsSavedFromOrderForm = true;
		//	order.RunPreSaveValidation();

		//	AssertAllocatedItem(orderedInventory, availableInventory, 4m, 0m, true);
		//}

		//#endregion

		#region TestAllocate_UpdatePickPercentCompleted

		public void TestAllocate_UpdatePickPercentCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, locations[1]);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrders(order);

			AssertEquals("Precondition:", (ZByte)0, pick.WP_PercentageComplete);

			pick.OrderedInventories[0].AvailableInventories[0].Allocate = true;
			AssertEquals("Item only allocated for pick, should change Pick Percent Completed value.", (ZByte)0, pick.WP_PercentageComplete);

			pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Item was picked, should change Pick Percent Completed value.", (ZByte)100, pick.WP_PercentageComplete);

			pick.OrderedInventories[1].AvailableInventories[0].Allocate = true;
			AssertEquals("More Items were allocated to Pick, should change Pick Percent Completed value.", (ZByte)50, pick.WP_PercentageComplete);

			pick.OrderedInventories[0].AvailableInventories[0].Allocate = false;
			AssertEquals("Picked item was unallocated, should change Pick Percent Completed value.", (ZByte)0, pick.WP_PercentageComplete);
		}

		#endregion

		#region TestAllocate_WhenCartonising

		public void TestAllocate_WhenCartonising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 10m, availInv.PickLineQuantity);

			availInv.Allocate = false;
			AssertEquals("Precondition", 0m, availInv.PickLineQuantity);

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			cartonisationMutex.Lock();

			using (cartonisationMutex)
			{
				AssertEquals("Precondition", true, pick.IsCartonising);
				availInv.Allocate = true;
				AssertEquals("Should not have changed as allocations changing during cartonisation will cause concurrency issues.", 0m, availInv.PickLineQuantity);
			}

			pick.WP_IsCartonised = true;
			availInv.Allocate = true;
			AssertEquals("Should have changed, this allows the user to short the pick or reallocate shorted lines.", 10m, availInv.PickLineQuantity);
		}

		#endregion

		#region TestAllocate_WhenCartonising_DuringAutoAllocateItems

		public void TestAllocate_WhenCartonising_DuringAutoAllocateItems()
		{
			// We block allocation during cartonisation to show a nice message to a user manually tweaking allocations.
			// During auto allocate (especially with the rules engine), there is no message and race conditions involved.
			// This means it is best to allow the allocation to proceed and have the higher level concurrency checks OnFactorySaving block the save.
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 10m, availInv.PickLineQuantity);

			availInv.Allocate = false;
			AssertEquals("Precondition", 0m, availInv.PickLineQuantity);

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			cartonisationMutex.Lock();

			using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
			using (cartonisationMutex)
			{
				AssertEquals("Precondition", true, pick.IsCartonising);
				availInv.Allocate = true;
				AssertEquals("Should have allocated as the user did not manually allocate.", 10m, availInv.PickLineQuantity);
			}
		}

		#endregion

		#endregion

		#region AllocationLog

		public void TestAllocationLog()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			bool isHasChangesSuspendedWhenSettingAllocationLog = false;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.AllocationLogInfo.ValueChanged += (sender, e) =>
			{
				isHasChangesSuspendedWhenSettingAllocationLog = availableInventory.IsSettingHasChangesSuspended;
			};

			availableInventory.AllocationLog = "Allocation Desc1";
			AssertEquals("Allocation Desc1", availableInventory.AllocationLog);
			AssertEquals("Setting HasChanges should be suspended when setting AllocationLog.", true, isHasChangesSuspendedWhenSettingAllocationLog);

			isHasChangesSuspendedWhenSettingAllocationLog = false;
			availableInventory.AllocationLog += "\nAllocation Desc2";
			AssertEquals("Allocation Desc1\nAllocation Desc2", availableInventory.AllocationLog);
			AssertEquals("Setting HasChanges should be suspended when setting AllocationLog.", true, isHasChangesSuspendedWhenSettingAllocationLog);
		}

		public void TestAllocationLogInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.AllocationLog, AvailableInventory.AllocationLogInfo.Name);
			AssertEquals(true, AvailableInventory.AllocationLogInfo.ReadOnly);
		}

		public void TestAllocationLogEntryStack()
		{
			AssertEquals(1, AvailableInventory.AllocationLogEntryStack.Count);
			AssertEquals("user", AvailableInventory.AllocationLogEntryStack.Peek());
		}

		public void TestAllocationLogMaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			AssertEquals("Shouldn't need max length - NonPersistentBizO and property.", -1, availableInventory.FindPropertyInfo("AllocationLog").MaxLength);
		}

		public void TestAllocationLogMaxLength_BashPast5000()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var stringBuilder = new ZStringBuilder();
			for (int i = 0; i < 5000; i++)
			{
				stringBuilder.Append("A");
			}

			availableInventory.AllocationLog = stringBuilder.ToStringWithNewLineBetweenAppends(); // Go well past 5k with new line chars
			AssertEquals("Should not blow up.", (4999 * 3 + 1), availableInventory.AllocationLog.Length);
		}

		#endregion

		#region Area

		// See TestArea() for asserting AreaName and AreaType

		public void TestAreaNameAndTypeInfos()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.AreaName, AvailableInventory.AreaNameInfo.Name);
			AssertEquals(WhsPickAvailableInventory.Schema.AreaType, AvailableInventory.AreaTypeInfo.Name);
		}

		public void TestAreaName_Translatable()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AvailableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull(AvailableInventory.Location.PickingArea);
			AssertEquals(AvailableInventory.Location.PickingArea, AvailableInventory.Area);

			var area = AvailableInventory.Location.PickingArea;
			area.WA_Name = "Test Area";
			AssertEquals("Test Area", AvailableInventory.AreaName);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "Test Area").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("AreaName in Chinese.", "测试区", AvailableInventory.AreaName);
			}
		}

		#endregion

		#region Flags

		#region TestIsAllocatedToPickFace

		public void TestIsAllocatedToPickFace()
		{
			SetupWhsPickOrderedInventory();
			WhsRow row = Helper.CreateRowAndGenerateLocations(AvailableInventory.Inventory[0].Warehouse, "B", 2, 2, 2);
			AvailableInventory.Inventory[0].WI_WL = row.Locations[0].PK;
			WhsPickFace pickFace = Helper.CreateProductPickFace(AvailableInventory.Inventory[0].Product, AvailableInventory.Inventory[0].Client, row.Locations[4]);
			AvailableInventory.Inventory[0].Product.PickFaces.Add(pickFace);
			((IWhsPickAvailableInventoryInternals)AvailableInventory).SetAllProperties(AvailableInventory.OrderedInventory, AvailableInventory.Inventory[0]);
			AssertEquals("This location should not be a PickFace", false, AvailableInventory.IsAllocatedToPickFace());
			AvailableInventory.Inventory[0].WI_WL = row.Locations[4].PK;
			((IWhsPickAvailableInventoryInternals)AvailableInventory).SetAllProperties(AvailableInventory.OrderedInventory, AvailableInventory.Inventory[0]);
			AssertEquals("This location should be a PickFace", true, AvailableInventory.IsAllocatedToPickFace());
		}

		#endregion

		#region TestIsInventoryPickable

		public void TestIsInventoryPickable()
		{
			SetupWhsPickOrderedInventory();

			AvailableInventory.Inventory[0].Location.WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Normal;
			AvailableInventory.Inventory[0].WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Available;
			AssertEquals("Precondition", true, AvailableInventory.IsInventoryPickable);

			AvailableInventory.Inventory[0].WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Held;
			AvailableInventory.Inventory[0].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = ZString.Empty;
			AvailableInventory.Inventory[0].WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Arrived;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Pending;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Putaway;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Available;
			AssertEquals(true, AvailableInventory.IsInventoryPickable);

			AvailableInventory.Inventory[0].Location.WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Damaged;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].Location.WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Held;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].Location.WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Void;
			AssertEquals(false, AvailableInventory.IsInventoryPickable);
			AvailableInventory.Inventory[0].Location.WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Normal;
			AssertEquals(true, AvailableInventory.IsInventoryPickable);
		}

		#endregion

		#region TestIsInventoryPickable_HeldInventory

		public void TestIsInventoryPickable_HeldInventory_GivenAvailableInventoryOrderPick() => TestIsInventoryPickable_HeldInventoryCore(PickType.Codes.Order, false);

		public void TestIsInventoryPickable_HeldInventory_GivenHeldInventoryOrderPick() => TestIsInventoryPickable_HeldInventoryCore(PickType.Codes.HeldInventoryOrder, true);

		public void TestIsInventoryPickable_HeldInventory_GivenWorkOrderPick() => TestIsInventoryPickable_HeldInventoryCore(PickType.Codes.WorkOrder, false);

		public void TestIsInventoryPickable_HeldInventory_GivenDynamicWorkOrderPick() => TestIsInventoryPickable_HeldInventoryCore(PickType.Codes.DynamicWorkOrder, false);

		void TestIsInventoryPickable_HeldInventoryCore(string pickType, bool expectCanPickHeldInventory)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, location, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				receive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

				var pick = Helper.CreatePickNew(order);
				pick.WP_PickType = pickType;

				var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

				availableInventory.Inventory[0].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(expectCanPickHeldInventory, availableInventory.IsInventoryPickable);
				availableInventory.Inventory[0].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
				AssertEquals(expectCanPickHeldInventory, availableInventory.IsInventoryPickable);

				availableInventory.Inventory[0].Location.WLV_LocationStatus = LocationStatus.Codes.Damaged;
				AssertEquals(expectCanPickHeldInventory, availableInventory.IsInventoryPickable);
				availableInventory.Inventory[0].Location.WLV_LocationStatus = LocationStatus.Codes.Held;
				AssertEquals(expectCanPickHeldInventory, availableInventory.IsInventoryPickable);
				availableInventory.Inventory[0].Location.WLV_LocationStatus = LocationStatus.Codes.Void;
				AssertEquals(expectCanPickHeldInventory, availableInventory.IsInventoryPickable);
				availableInventory.Inventory[0].Location.WLV_LocationStatus = LocationStatus.Codes.Normal;
				AssertEquals(expectCanPickHeldInventory, availableInventory.IsInventoryPickable);
			}
		}

		#endregion

		#region TestIsPickByBOMKitInventory

		public void TestIsPickByBOMKitInventory()
		{
			SetupWhsPickOrderedInventory();

			var inventory = AvailableInventory.Inventory[0];
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals("Precondition", false, AvailableInventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = ZString.Empty;
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Arrived;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);

			var pick = Factory.New<WhsPick>();
			inventory.Docket.WD_WP_ParentPickForReceive = pick.PK;
			AssertEquals(true, AvailableInventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals(true, AvailableInventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Arrived;
			AssertEquals(false, AvailableInventory.IsPickByBOMKitInventory);

			inventory.WI_InventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals(true, AvailableInventory.IsPickByBOMKitInventory);
		}

		#endregion

		#region TestIsAcceptableMinimumShelfLife

		[TestDate(2013, 3, 7)]
		public void TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_WarehouseConsignee()
		{
			TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_Core(
				minShelfWarehoueConsignee: true,
				minShelfConsignee: false,
				minShelfProduct: false);
		}

		[TestDate(2013, 3, 7)]
		public void TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_Consignee()
		{
			TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_Core(
				minShelfWarehoueConsignee: false,
				minShelfConsignee: true,
				minShelfProduct: false);
		}

		[TestDate(2013, 3, 7)]
		public void TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_Product()
		{
			TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_Core(
				minShelfWarehoueConsignee: false,
				minShelfConsignee: false,
				minShelfProduct: true);
		}

		void TestIsAcceptableMinimumShelfLife_JulianBatchNumberPartAttributeUsed_Core(
			bool minShelfWarehoueConsignee,
			bool minShelfConsignee,
			bool minShelfProduct)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var part1ClientRelation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			var part2ClientRelation = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // Will turn on usage of Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // Will turn on usage of Expiry Date
			part2ClientRelation.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 30;

			var consignee = Helper.CreateClient("CONSIGNEE");
			var part1ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part1, "WCN");
			var part2ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part2, "WCN");

			ZShort minShelfLife = 10;
			if (minShelfWarehoueConsignee)
			{
				part1ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			if (minShelfConsignee)
			{
				consignee.MiscServ.OM_MinimumShelfLifeAccepted = minShelfLife;
			}

			if (minShelfProduct)
			{
				part1ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "", "3030ABC", "", ""); // Expiry date < Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Empty, ZDate.Empty, "", "3047ABC", "", ""); // Expiry date = Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "", "3060ABC", "", ""); // Expiry date > Min Shelf Life
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			// Normal Attribute
			var orderedInventory_NormalAttribute = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_NormalAttribute = orderedInventory_NormalAttribute.AvailableInventories[0];
			AssertEquals(true, availableInventory_NormalAttribute.IsAcceptableMinimumShelfLife);

			// Julian Batch Number
			var orderedInventory_JulianBatchNumber = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertEquals(false, availableInventory_EarlierExpiryDate.IsAcceptableMinimumShelfLife);
			AssertEquals(true, availableInventory_MatchingExpiryDate.IsAcceptableMinimumShelfLife);
			AssertEquals(true, availableInventory_LaterExpiryDate.IsAcceptableMinimumShelfLife);
		}

		[TestDate(2014, 7, 3)]
		public void TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_WarehouseConsignee()
		{
			TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_Core(
				minShelfWarehoueConsignee: true,
				minShelfConsignee: false,
				minShelfProduct: false);
		}

		[TestDate(2014, 7, 3)]
		public void TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_Consignee()
		{
			TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_Core(
				minShelfWarehoueConsignee: false,
				minShelfConsignee: true,
				minShelfProduct: false);
		}

		[TestDate(2014, 7, 3)]
		public void TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_Product()
		{
			TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_Core(
				minShelfWarehoueConsignee: false,
				minShelfConsignee: false,
				minShelfProduct: true);
		}

		void TestIsAcceptableMinimumShelfLife_ExpiryDatePartAttributeUsed_Core(
			bool minShelfWarehoueConsignee,
			bool minShelfConsignee,
			bool minShelfProduct)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var part1ClientRelation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			var part2ClientRelation = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var part1ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part1, "WCN");
			var part2ConsigneeRelation = Helper.CreateProductClientRelationShip(consignee, data.Part2, "WCN");

			ZShort minShelfLife = 10;
			if (minShelfWarehoueConsignee)
			{
				part1ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			if (minShelfConsignee)
			{
				consignee.MiscServ.OM_MinimumShelfLifeAccepted = minShelfLife;
			}

			if (minShelfProduct)
			{
				part1ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
				part2ClientRelation.OU_ConsigneeMinShelfLifeAccepted = minShelfLife;
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Today.AddDays(5), ZDate.Empty, "", "", "", ""); // Expiry date < Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Today.AddDays(10), ZDate.Empty, "", "", "", ""); // Expiry date = Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Today.AddDays(15), ZDate.Empty, "", "", "", ""); // Expiry date > Min Shelf Life

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			// No Expiry Date
			var orderedInventory_ProdWithNoExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_ProdWithNoExpDate = orderedInventory_ProdWithNoExpDate.AvailableInventories[0];
			AssertEquals(true, availableInventory_ProdWithNoExpDate.IsAcceptableMinimumShelfLife);

			// With Expiry Date
			var orderedInventory_ProdWithExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertEquals(false, availableInventory_EarlierExpiryDate.IsAcceptableMinimumShelfLife);
			AssertEquals(true, availableInventory_MatchingExpiryDate.IsAcceptableMinimumShelfLife);
			AssertEquals(true, availableInventory_LaterExpiryDate.IsAcceptableMinimumShelfLife);
		}

		#endregion

		#region TestIsExpired

		[TestDate(2019, 8, 12)]
		public void TestIsExpired_JulianBatchNumberPartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // Will turn on usage of Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // Will turn on usage of Expiry Date
			data.Part2.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 5;

			var consignee = Helper.CreateClient("CONSIGNEE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "", "9175ABC", "", ""); // Expiry date < Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Empty, ZDate.Empty, "", "9219ABC", "", ""); // Expiry date = Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "", "9230ABC", "", ""); // Expiry date > Current date
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			// Normal Attribute
			var orderedInventory_NormalAttribute = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_NormalAttribute = orderedInventory_NormalAttribute.AvailableInventories[0];
			AssertEquals("No expiry date set, should be unexpired.", false, availableInventory_NormalAttribute.IsExpired);

			// Julian Batch Number
			var orderedInventory_JulianBatchNumber = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertEquals("Expiry date set in past, should be expired.", true, availableInventory_EarlierExpiryDate.IsExpired);
			AssertEquals("Expiry date set today, should be expired.", true, availableInventory_MatchingExpiryDate.IsExpired);
			AssertEquals("Expiry date set in future, should be unexpired.", false, availableInventory_LaterExpiryDate.IsExpired);
		}

		[TestDate(2019, 8, 12)]
		public void TestIsExpired_ExpiryDatePartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Today.AddDays(-5), ZDate.Empty, "", "", "", ""); // Expiry date < Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Today, ZDate.Empty, "", "", "", ""); // Expiry date = Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Today.AddDays(5), ZDate.Empty, "", "", "", ""); // Expiry date > Current date

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			// No Expiry Date
			var orderedInventory_ProdWithNoExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_ProdWithNoExpDate = orderedInventory_ProdWithNoExpDate.AvailableInventories[0];
			AssertEquals("No expiry date set, should be unexpired.", false, availableInventory_ProdWithNoExpDate.IsExpired);

			// With Expiry Date
			var orderedInventory_ProdWithExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertEquals("Expiry date set in the past, should be expired.", true, availableInventory_EarlierExpiryDate.IsExpired);
			AssertEquals("Expiry date set today, should be expired.", true, availableInventory_MatchingExpiryDate.IsExpired);
			AssertEquals("Expiry date set in the future, should be unexpired.", false, availableInventory_LaterExpiryDate.IsExpired);
		}

		#endregion

		#region TestIsChildOfHeldInventoryPick

		public void TestIsChildOfHeldInventoryPick()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, location, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				receive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

				var pick = Helper.CreatePickNew(order);

				var inventory = pick.OrderedInventories[0].AvailableInventories[0];
				AssertEquals("Precondition", PickType.Codes.HeldInventoryOrder, pick.WP_PickType);
				AssertEquals(true, inventory.IsChildOfHeldInventoryOrderPick);

				pick.WP_PickType = PickType.Codes.Order;
				AssertEquals(false, inventory.IsChildOfHeldInventoryOrderPick);

				pick.WP_PickType = PickType.Codes.WorkOrder;
				AssertEquals(false, inventory.IsChildOfHeldInventoryOrderPick);

				pick.WP_PickType = PickType.Codes.DynamicWorkOrder;
				AssertEquals(false, inventory.IsChildOfHeldInventoryOrderPick);

				pick.WP_PickType = PickType.Codes.HeldInventoryOrder;
				AssertEquals(true, inventory.IsChildOfHeldInventoryOrderPick);
			}
		}

		#endregion

		#endregion

		#region Verified Non Empty

		#region TestVerifiedNonEmpty

		public void TestVerifiedNonEmpty()
		{
			// setup location
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");

			// create receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition - Pick should not be finalised.", false, pick.IsFinalised);
			var avaialbleInventoryLine = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var pickLines = avaialbleInventoryLine.PickLines.ToArray();
			AssertEquals("Precondition - There should be two pick lines for one available inventory line.", 2, pickLines.Length);

			// test setting VerifiedNonEmpty on avaialble inventory line
			avaialbleInventoryLine.VerifiedNonEmpty = true;
			AssertEquals("N", pickLines[0].WZ_VerifiedEmpty);
			AssertEquals("N", pickLines[1].WZ_VerifiedEmpty);

			avaialbleInventoryLine.VerifiedNonEmpty = false;
			AssertEquals("Y", pickLines[0].WZ_VerifiedEmpty);
			AssertEquals("Y", pickLines[1].WZ_VerifiedEmpty);

			// test get VerifiedNonEmpty on avaialble inventory line
			pickLines[0].WZ_VerifiedEmpty = "N";
			pickLines[1].WZ_VerifiedEmpty = "Y";
			AssertEquals(true, avaialbleInventoryLine.VerifiedNonEmpty);

			pickLines[0].WZ_VerifiedEmpty = "Y";
			pickLines[1].WZ_VerifiedEmpty = "N";
			AssertEquals(true, avaialbleInventoryLine.VerifiedNonEmpty);

			pickLines[0].WZ_VerifiedEmpty = "Y";
			pickLines[1].WZ_VerifiedEmpty = "Y";
			AssertEquals(false, avaialbleInventoryLine.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmpty_InTransit

		public void TestVerifiedNonEmpty_InTransit()
		{
			// setup location
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");

			// create receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition - Pick should not be finalised.", false, pick.IsFinalised);
			var availableInventoryLine = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var pickLines = availableInventoryLine.PickLines.ToArray();
			AssertEquals("Precondition - There should be two pick lines for one available inventory line.", 2, pickLines.Length);

			foreach (var pickLine in pickLines)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}

			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];

			availableInventoryLine.VerifiedNonEmpty = true;
			AssertEquals("N", pickLine1.WZ_VerifiedEmpty);
			AssertEquals("N", pickLine2.WZ_VerifiedEmpty);

			availableInventoryLine.VerifiedNonEmpty = false;
			AssertEquals("Y", pickLine1.WZ_VerifiedEmpty);
			AssertEquals("Y", pickLine2.WZ_VerifiedEmpty);

			pickLine1.WZ_VerifiedEmpty = "N";
			pickLine2.WZ_VerifiedEmpty = "Y";
			AssertEquals(true, availableInventoryLine.VerifiedNonEmpty);

			pickLine1.WZ_VerifiedEmpty = "Y";
			pickLine2.WZ_VerifiedEmpty = "N";
			AssertEquals(true, availableInventoryLine.VerifiedNonEmpty);

			pickLine1.WZ_VerifiedEmpty = "Y";
			pickLine2.WZ_VerifiedEmpty = "Y";
			AssertEquals(false, availableInventoryLine.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmpty_AttributeSpecifiedProducts

		public void TestVerifiedNonEmpty_AttributeSpecifiedProducts()
		{
			// setup location, clients and porducts
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, "SER", "Serial Number");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			data.Part2.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var org2 = Helper.CreateClient("O2", "Client2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.SetClientAttributeType(org2, AttributeNumber.One, "SER", "Serial #");
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[1].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			// setup receives, orders and pick
			var receiveForOrg1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveForOrg2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location, ZDate.Empty, ZDate.Empty, "T1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location, ZDate.Empty, ZDate.Empty, "T2", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part2, 1m, location, ZDate.Empty, ZDate.Empty, "T3", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg2, data.Part1, 1m, location, ZDate.Empty, ZDate.Empty, "T4", "", "", "");
			receiveForOrg1.FinaliseDocket();
			receiveForOrg2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receiveForOrg1);
			AssertIsFinalisedPrecondition(receiveForOrg2);

			var orderForOrg1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderForOrg2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", Notify);
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "T1", "", "", "", "");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "T2", "", "", "", "");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "T3", "", "", "", "");
			Helper.CreateWhsOrderLine(orderForOrg2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "T4", "", "", "", "");

			var pick = Helper.CreatePickNew(orderForOrg1, orderForOrg2);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition - There should be four ordered inventory lines.", 4, pick.OrderedInventories.Count);
			var avlInvOrg1Part1SerialNumberT1 = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var avlInvOrg1Part1SerialNumberT2 = pick.OrderedInventories[1].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var avlInvOrg1Part2SerialNumberT3 = pick.OrderedInventories[2].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var avlInvOrg2Part1SerialNumberT4 = pick.OrderedInventories[3].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			// set verifiedNonEmpty on an available inventory line with matching available inventory line
			// exists with same product and location.
			avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty = true;
			AssertEquals(true, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(true, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty = false;
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			// set verifiedNonEmpty on an available inventory line where no matching inventory lines exists since products are different
			avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty = true;
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(true, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty = false;
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			// set verifiedNonEmpty on an available inventory line where no matching inventory lines exists since clients are different
			avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty = true;
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(true, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty = false;
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmpty_Complex

		public void TestVerifiedNonEmpty_Complex()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, "SER", "Serial Number");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			data.Part2.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var org2 = Helper.CreateClient("O2", "Client2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.SetClientAttributeType(org2, AttributeNumber.One, "SER", "Serial #");
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[1].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			// setup receive, orders and pick
			var receiveForOrg1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveForOrg2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location, ZDate.Empty, ZDate.Empty, "T1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location, ZDate.Empty, ZDate.Empty, "T2", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part2, 1m, location, ZDate.Empty, ZDate.Empty, "T3", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receiveForOrg2, data.Part1, 1m, location, ZDate.Empty, ZDate.Empty, "T4", "", "", "");
			receiveForOrg1.FinaliseDocket();
			receiveForOrg2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receiveForOrg1);
			AssertIsFinalisedPrecondition(receiveForOrg2);

			var orderForOrg1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderForOrg2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", Notify);
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "T1", "", "", "", "");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "T2", "", "", "", "");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "T3", "", "", "", "");
			Helper.CreateWhsOrderLine(orderForOrg2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "T4", "", "", "", "");

			var pick = Helper.CreatePickNew(orderForOrg1, orderForOrg2);
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Precondition - Pick should not be finalised.", false, pick.IsFinalised);
			AssertEquals("There should be two ordered inventory lines.", 4, pick.OrderedInventories.Count);

			var avlInvOrg1Part1SerialNumberT1 = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var avlInvOrg1Part1SerialNumberT2 = pick.OrderedInventories[1].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var avlInvOrg1Part2SerialNumberT3 = pick.OrderedInventories[2].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var avlInvOrg2Part1SerialNumberT4 = pick.OrderedInventories[3].AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			var pickLineOrg1Part1SerialNumberT1 = avlInvOrg1Part1SerialNumberT1.PickLines.Single();
			var pickLineOrg1Part1SerialNumberT2 = avlInvOrg1Part1SerialNumberT2.PickLines.Single();
			var pickLineOrg1Part2SerialNumberT3 = avlInvOrg1Part2SerialNumberT3.PickLines.Single();
			var pickLineOrg2Part1SerialNumberT4 = avlInvOrg2Part1SerialNumberT4.PickLines.Single();

			// get verifiedNonEmpty from an available inventory line with matching pick line exists with same product and location.
			pickLineOrg1Part1SerialNumberT1.WZ_VerifiedEmpty = "N";
			AssertEquals(true, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(true, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			pickLineOrg1Part1SerialNumberT1.WZ_VerifiedEmpty = "Y";
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			// get verifiedNonEmpty from an available inventory line with pick lines exists for the same client but different products.
			pickLineOrg1Part2SerialNumberT3.WZ_VerifiedEmpty = "N";
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(true, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			pickLineOrg1Part2SerialNumberT3.WZ_VerifiedEmpty = "Y";
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			// get verifiedNonEmpty from an available inventory line with pick lines exists for the same product but different clients.
			pickLineOrg2Part1SerialNumberT4.WZ_VerifiedEmpty = "N";
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(true, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);

			pickLineOrg2Part1SerialNumberT4.WZ_VerifiedEmpty = "Y";
			AssertEquals(false, avlInvOrg1Part1SerialNumberT1.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part1SerialNumberT2.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg1Part2SerialNumberT3.VerifiedNonEmpty);
			AssertEquals(false, avlInvOrg2Part1SerialNumberT4.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmpty_InventoryIsNull

		public void TestVerifiedNonEmpty_InventoryIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			var pickLine = availableInventory.PickLines.Single();
			pickLine.WZ_WE_InventoryLine = ZGuid.Empty;
			pickLine.WZ_VerifiedEmpty = "N";
			AssertNoExceptionThrown(() => _ = availableInventory.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmpty_SupplierPKNull

		public void TestVerifiedNonEmpty_SupplierPKNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Factory.New<WhsOrderLine>();
			orderLine.WE_WD = order.PK;

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.AddOrders(new[] { order });

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = new WhsPickAvailableInventory(Factory);
			((IWhsPickAvailableInventoryInternals)availableInventory).SetAllProperties(orderedInventory, receive.Inventory[0]);

			orderedInventory.SetAvaliableInventoryForTest(availableInventory);

			AssertNoExceptionThrown(() => _ = availableInventory.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmpty_IsNotCalculatedIfDisabledOnWarehouse

		public void TestVerifiedNonEmpty_IsNotCalculatedIfDisabledOnWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			AssertEquals("Precondition", false, availableInventory.VerifiedNonEmpty);

			data.Whs1.WW_VerifyEmptyLocations = true;
			AssertEquals(false, availableInventory.VerifiedNonEmpty);

			availableInventory.VerifiedNonEmpty = true;
			AssertEquals(true, availableInventory.VerifiedNonEmpty);

			data.Whs1.WW_VerifyEmptyLocations = false;
			AssertEquals(false, availableInventory.VerifiedNonEmpty);
		}

		#endregion

		#region TestVerifiedNonEmptyInfo

		public void TestVerifiedNonEmptyInfo()
		{
			// setup locations
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			// create receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location2);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Count);
			var allocatedAvailableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories[0];
			var nonAllocatedAvailableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories[1];

			// Test when empty location verification is turned on for the warehouse
			data.Whs1.WW_VerifyEmptyLocations = true;
			AssertEquals("Precondition", WhsPickAvailableInventory.Schema.VerifiedNonEmpty, allocatedAvailableInventory.VerifiedNonEmptyInfo.Name);
			allocatedAvailableInventory.Allocate = false;
			nonAllocatedAvailableInventory.Allocate = false;
			AssertEquals("Line is still not allocated, therefore VerifiedNonEmpty should be readonly.", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("Line is still not allocated, therefore VerifiedNonEmpty should be readonly.", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);

			allocatedAvailableInventory.Allocate = true;
			AssertEquals("Line is allocated, therefore VerifiedNonEmpty should not be readonly.", false, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("Line is still not allocated, therefore VerifiedNonEmpty should be readonly.", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("If Pick is Finalised then VerifiedNonEmpty should be readonly", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("If Pick is Finalised then VerifiedNonEmpty should be readonly", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			pick.WP_PickStatus = PickStatus.Codes.Cancelled;
			AssertEquals("If Pick is Cancelled then VerifiedNonEmpty should be readonly", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("If Pick is Cancelled then VerifiedNonEmpty should be readonly", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);

			// Test when empty location verification is turned off for the warehouse
			data.Whs1.WW_VerifyEmptyLocations = false;
			pick.WP_PickStatus = PickStatus.Codes.Created;

			allocatedAvailableInventory.Allocate = false;
			nonAllocatedAvailableInventory.Allocate = false;
			AssertEquals("Line is still not allocated, therefore VerifiedNonEmpty should be readonly.", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("Line is still not allocated, therefore VerifiedNonEmpty should be readonly.", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);

			allocatedAvailableInventory.Allocate = true;
			AssertEquals("Line is allocated but VerifyEmptyLocations is turned off, therefore VerifiedNonEmpty should not be readonly.", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("Line is still not allocated, therefore VerifiedNonEmpty should be readonly.", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("If Pick is Finalised then VerifiedNonEmpty should be readonly", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("If Pick is Finalised then VerifiedNonEmpty should be readonly", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			pick.WP_PickStatus = PickStatus.Codes.Cancelled;
			AssertEquals("If Pick is Cancelled then VerifiedNonEmpty should be readonly", true, allocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
			AssertEquals("If Pick is Cancelled then VerifiedNonEmpty should be readonly", true, nonAllocatedAvailableInventory.VerifiedNonEmptyInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick

		public void TestIsLocationEmptyAfterFinalisingPick()
		{
			// setup location
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order1);
			var availableInventoryLine = pick.OrderedInventories[0].AvailableInventories[0];
			var pickLine = pick.OrderedInventories[0].AvailableInventories[0].PickLines.First();

			// test IsLocationEmptyAfterFinalisingPick property
			((IEmptyLocationAfterPickFinalisation)pickLine).IsLocationEmptyAfterFinalisingPick = true;
			AssertEquals(true, availableInventoryLine.IsLocationEmptyAfterFinalisingPick);

			((IEmptyLocationAfterPickFinalisation)pickLine).IsLocationEmptyAfterFinalisingPick = false;
			AssertEquals(false, availableInventoryLine.IsLocationEmptyAfterFinalisingPick);
		}

		#endregion

		#region TestVFDPerStockUnit

		public void TestVFDPerStockUnitInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.VFDPerStockUnit, AvailableInventory.VFDPerStockUnitInfo.Name);
		}

		public void TestVFDPerStockUnit_NoIPR()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should *not* have a value for duty.", 0m, availInv.VFDPerStockUnit);

			var bondedData = Factory.LoadTop1<WhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, inventory.PK) { FetchOnlyFromLocalCache = true });
			AssertNull("Should not have created bonded data.", bondedData);
		}

		public void TestVFDPerStockUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardsProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardsProcessingLocation.WLV_WA_PutawayArea = iprArea.PK;
			inwardsProcessingLocation.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, inwardsProcessingLocation);
			inventory.CustomsData.WB_EntryKey = "ABC123";
			inventory.CustomsData.WB_EntryDate = ZDateTime.Today.AddDays(-2);
			inventory.CustomsData.WB_ValueForDuty = 48m;
			inventory.CustomsData.WB_BondedWhsQty = 12m;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_IsInwardsProcessingJob = true;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");

			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should have a value for duty.", 4m, availInv.VFDPerStockUnit);
		}

		public void TestVFDPerStockUnit_NullPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardsProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardsProcessingLocation.WLV_WA_PutawayArea = iprArea.PK;
			inwardsProcessingLocation.WLV_WA_PickingArea = iprArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, inwardsProcessingLocation);
			inventory.CustomsData.WB_EntryKey = "ABC123";
			inventory.CustomsData.WB_EntryDate = ZDateTime.Today.AddDays(-2);
			inventory.CustomsData.WB_ValueForDuty = 48m;
			inventory.CustomsData.WB_BondedWhsQty = 12m;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_IsInwardsProcessingJob = true;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");

			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			availInv.Deactivate();

			AssertEquals("Should have an empty value for duty.", 0m, availInv.VFDPerStockUnit);
		}

		#endregion

		#region ReadOnly

		#region TestAllocateInfo

		public void TestAllocateInfo()
		{
			TestReadOnlyForChangingAllocations(WhsPickAvailableInventory.Schema.Allocate);
		}

		#endregion

		#region TestPickLineQuantityInfo

		public void TestPickLineQuantityInfo()
		{
			TestReadOnlyForChangingAllocations(WhsPickAvailableInventory.Schema.PickLineQuantity);
		}

		#endregion

		#region TestReadOnlyForChangingAllocations

		void TestReadOnlyForChangingAllocations(string infoName)
		{
			TestReadOnlyForUnfinalizedNonAvailableStock(infoName);
			TestStockIsPickedFromPutawayLocationReadOnly(infoName);
			TestReadOnlyForTaskPlanningStatus(infoName);
		}

		void TestReadOnlyForTaskPlanningStatus(string infoName)
		{
			var pick = Factory.LoadTop1<WhsPick>(new ZQuery());

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var info = availableInventory.ZPropertyInfoHash[infoName];

			AssertEquals($"Precondition - property '{info.Name}' should not be ReadOnly.", false, info.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals($"If pick is ready for planning, the user should no longer be able to change '{info.Name}'.", true, info.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals($"If pick is not ready for planning, the user should no longer be able to change '{info.Name}'.", false, info.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals($"If pick is planning, the user should no longer be able to change '{info.Name}'.", true, info.ReadOnly);
		}

		void TestStockIsPickedFromPutawayLocationReadOnly(string infoName)
		{
			var pick = Factory.LoadTop1<WhsPick>(new ZQuery());
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 20m, totalPickLineQuantity);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var info = availableInventory.ZPropertyInfoHash[infoName];
			AssertEquals($"Precondition - property '{info.Name}' should not be ReadOnly.", false, info.ReadOnly);

			AssertEquals("Precondition: More than one pick line on the AvailableInventory.", true, availableInventory.PickLines.Count() > 1);
			var pickLine = availableInventory.PickLines.ElementAt(0);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals($"If any of the allocated stock is picked, the user should no longer be able to change '{info.Name}'.", true, info.ReadOnly);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty; // clean up

			AssertEquals($"Precondition - property '{info.Name}' should not be ReadOnly.", false, info.ReadOnly);
			var newReceiveLine = Factory.New<WhsReceiveLine>();
			pickLine.WZ_WE_OriginalPickedInventoryLine = pickLine.WZ_WE_InventoryLine;
			pickLine.WZ_WE_InventoryLine = newReceiveLine.PK;
			AssertEquals($"If any of the allocated stock is in transit, the user should no longer be able to change '{info.Name}'.", true, info.ReadOnly);

			newReceiveLine.Delete(); // clean up
			pickLine.WZ_WE_InventoryLine = pickLine.WZ_WE_OriginalPickedInventoryLine;
			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
		}

		void TestReadOnlyForUnfinalizedNonAvailableStock(string infoName)
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 20m, totalPickLineQuantity);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(false, availableInventory.ZPropertyInfoHash[infoName].ReadOnly);

			TestInventoryStatusReadOnly(receive1.Lines.Concat(receive2.Lines).Cast<WhsReceiveLine>(), availableInventory.ZPropertyInfoHash[infoName]);
			TestPickStatusReadOnly(pick, availableInventory.ZPropertyInfoHash[infoName]);
		}

		static void TestInventoryStatusReadOnly(IEnumerable<WhsReceiveLine> receiveLines, ZPropertyInfo info)
		{
			var availableInventory = (WhsPickAvailableInventory)info.BizObj;
			var list = new InventoryStatus();
			foreach (CodeDescriptionPair pair in list)
			{
				receiveLines.ForEach(rl => rl.WE_CurrentInventoryStatus = pair.Code);
				AssertEquals("Precondition: AvailableInventory Status.", pair.Code, ((WhsPickAvailableInventory)info.BizObj).InventoryStatus);
				AssertEquals($"When inventory status is '{pair.Description}' and some stock is allocated, then property '{info.Name}' should not be readonly.", false, info.ReadOnly);

				availableInventory.PickLineQuantity = 0m;
				if (pair.Code != InventoryStatus.Codes.Available)
				{
					AssertEquals($"When inventory status is '{pair.Description}' and nothing is allocated, then property '{info.Name}' should be readonly.", true, info.ReadOnly);
				}
				else
				{
					AssertEquals($"When inventory status is '{pair.Description}' and nothing is allocated, then property '{info.Name}' should not be readonly.", false, info.ReadOnly);
				}

				// clean up
				receiveLines.ForEach(rl => rl.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available);
				availableInventory.PickLineQuantity = 20m;
			}
		}

		static void TestPickStatusReadOnly(WhsPick pick, ZPropertyInfo info)
		{
			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("If Pick Finalised then should be readonly", true, info.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Cancelled;
			AssertEquals("If Pick Cancelled then should be readonly", true, info.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.PickSlip;
			AssertEquals("If Pick Slip printed then should not be readonly", false, info.ReadOnly);
		}

		public void TestPickLineQuantityInfoAndAllocateInfoReadOnly_PickByBOMKits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A-1"));
			var wheelInventory = receive.Inventory[0];
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 30m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			var bikeAvailableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == bike.PK).AvailableInventories[0];
			AssertEquals(true, bikeAvailableInventory.PickLineQuantityInfo.ReadOnly);
			AssertEquals(true, bikeAvailableInventory.AllocateInfo.ReadOnly);
		}

		public void TestReadOnly_Allocate_PickLineQuantity_SerialNumber_Used()
		{
			TestReadOnly_Allocate_PickLineQuantity_SerialNumber(productAttributeUse: true);
		}

		public void TestReadOnly_Allocate_PickLineQuantity_SerialNumber_NotUsed()
		{
			TestReadOnly_Allocate_PickLineQuantity_SerialNumber(productAttributeUse: false);
		}

		void TestReadOnly_Allocate_PickLineQuantity_SerialNumber(bool productAttributeUse)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, productAttributeUse);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, productAttributeUse);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			if (productAttributeUse)
			{
				receive.Lines[0].WE_SerialNumber = "SN1";
				receive.Lines[0].SerialNumbers.AddNew().SerialNumberValue = $"SN1";
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 1m, totalPickLineQuantity);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("RedesignChanges is false.", false, availableInventory.HasSerialNumber);

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", productAttributeUse, availableInventory.HasSerialNumber);
				AssertEquals("When has serial number it should be readonly", productAttributeUse, availableInventory.AllocateInfo.ReadOnly);
				AssertEquals("When has serial number it should be readonly", productAttributeUse, availableInventory.PickLineQuantityInfo.ReadOnly);
			}
		}

		public void TestReadOnly_Allocate_PickLineQuantity_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			receive.Lines[0].WE_SerialNumber = "SN1";
			receive.Lines[0].SerialNumbers.AddNew().SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 1m, totalPickLineQuantity);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			AssertReadonly(enableSchemaRedesignChanges: true);
			AssertReadonly(enableSchemaRedesignChanges: false);

			void AssertReadonly(bool enableSchemaRedesignChanges)
			{
				using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSchemaRedesignChanges))
				{
					AssertEquals("Precondition", enableSchemaRedesignChanges, availableInventory.HasSerialNumber);
					AssertEquals("When use serial number and in Redesign mode it should be readonly", enableSchemaRedesignChanges, availableInventory.AllocateInfo.ReadOnly);
					AssertEquals("When use serial number and in Redesign mode it should be readonly", enableSchemaRedesignChanges, availableInventory.PickLineQuantityInfo.ReadOnly);
				}
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Select_ReadOnly_SelectInOtherOrderLine()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var helper = new WhsTestHelperFunctions(Factory);
				helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";
				var pivot3 = receiveLine.SerialNumbers.AddNew();
				pivot3.SerialNumberValue = "SN3";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
				orderLine2.WE_SerialNumber = "SN2";
				var pick = helper.CreatePickNew(order);

				var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => !o.SerialNumber.Equals("SN2")).AvailableInventories[0];
				var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SerialNumber.Equals("SN2")).AvailableInventories[0];

				var serialNumbers1 = availableInventory1.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(3, serialNumbers1.Count());
				AssertEquals(2, serialNumbers1.Count(s => s.Selected));
				AssertAvailableSerialNumber(serialNumbers1, "SN1", true, false, orderLine1.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN2", true, true, orderLine2.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN3", false, true, ZGuid.Empty);
				var serialNumbers2 = availableInventory2.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(1, serialNumbers2.Count());
				AssertAvailableSerialNumber(serialNumbers2, "SN2", true, false, orderLine2.PickLines[0].PK);
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Select_ReadOnly_AlreadyTaken()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var helper = new WhsTestHelperFunctions(Factory);
				helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";
				var pivot3 = receiveLine.SerialNumbers.AddNew();
				pivot3.SerialNumberValue = "SN3";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
				orderLine2.WE_SerialNumber = "SN2";
				var pick = helper.CreatePickNew(order);

				var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => !o.SerialNumber.Equals("SN2")).AvailableInventories[0];
				var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SerialNumber.Equals("SN2")).AvailableInventories[0];

				var serialNumbers1 = availableInventory1.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(3, serialNumbers1.Count());
				AssertEquals(2, serialNumbers1.Count(s => s.Selected));
				AssertAvailableSerialNumber(serialNumbers1, "SN1", true, false, orderLine1.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN2", true, true, orderLine2.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN3", false, true, ZGuid.Empty);
				var serialNumbers2 = availableInventory2.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(1, serialNumbers2.Count());
				AssertAvailableSerialNumber(serialNumbers2, "SN2", true, false, orderLine2.PickLines[0].PK);
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Select_ReadOnly_Finalised()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var helper = new WhsTestHelperFunctions(Factory);
				helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
				var pick = helper.CreatePickNew(order);

				var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

				var serialNumber = availableInventory.SerialNumbers[0];
				AssertEquals("Precondition", false, pick.IsFinalised);
				AssertEquals("Precondition", false, serialNumber.SelectedInfo.ReadOnly);

				pick.FinaliseAllOrders();
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order);
				AssertIsFinalisedPrecondition(pick);
				AssertEquals("Should not be able to change finalise job.", true, serialNumber.SelectedInfo.ReadOnly);
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Select_ReadOnly_CanChangeSelection()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var helper = new WhsTestHelperFunctions(Factory);
				helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";
				var pivot3 = receiveLine.SerialNumbers.AddNew();
				pivot3.SerialNumberValue = "SN3";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
				orderLine2.WE_SerialNumber = "SN2";
				var pick = helper.CreatePickNew(order);

				var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => !o.SerialNumber.Equals("SN2")).AvailableInventories[0];
				var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SerialNumber.Equals("SN2")).AvailableInventories[0];

				var serialNumbers1 = availableInventory1.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(3, serialNumbers1.Count());
				AssertEquals(2, serialNumbers1.Count(s => s.Selected));
				AssertAvailableSerialNumber(serialNumbers1, "SN1", true, false, orderLine1.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN2", true, true, orderLine2.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN3", false, true, ZGuid.Empty);
				var serialNumbers2 = availableInventory2.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(1, serialNumbers2.Count());
				AssertAvailableSerialNumber(serialNumbers2, "SN2", true, false, orderLine2.PickLines[0].PK);

				serialNumbers2.Single().Selected = false;
				AssertAvailableSerialNumber(serialNumbers2, "SN2", false, false, ZGuid.Empty);

				serialNumbers1.Single(s => s.SerialNumberValue.Equals("SN1")).Selected = false;
				// Should be able to select any serial number
				AssertAvailableSerialNumber(serialNumbers1, "SN1", false, false, ZGuid.Empty);
				AssertAvailableSerialNumber(serialNumbers1, "SN2", false, false, ZGuid.Empty);
				AssertAvailableSerialNumber(serialNumbers1, "SN3", false, false, ZGuid.Empty);

				serialNumbers1.Single(s => s.SerialNumberValue.Equals("SN2")).Selected = true;
				AssertAvailableSerialNumber(serialNumbers1, "SN1", false, true, ZGuid.Empty);
				AssertAvailableSerialNumber(serialNumbers1, "SN2", true, false, orderLine1.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN3", false, true, ZGuid.Empty);

				// SN2 is already taken in other colloction and cannot be changed
				AssertAvailableSerialNumber(serialNumbers2, "SN2", true, true, orderLine1.PickLines[0].PK);
			}
		}

		static void AssertAvailableSerialNumber(IEnumerable<WhsSerialNumberPivotSelector> serialNumbers, string serialNumberFilter, bool expectedSelected, bool expectedReadonly, ZGuid expectedPickLinePK)
		{
			var whsSerialNumberPivotSelector = serialNumbers.Single(s => s.SerialNumberValue.Equals(serialNumberFilter));
			AssertEquals($"{serialNumberFilter}: Selected", expectedSelected, whsSerialNumberPivotSelector.Selected);
			//AssertEquals($"{serialNumberFilter}: Readonly", expectedReadonly, whsSerialNumberPivotSelector.SelectedInfo.ReadOnly);
			AssertEquals($"{serialNumberFilter}: PickingLinePK", expectedPickLinePK, whsSerialNumberPivotSelector.PickingLinePK);
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Select_ReadOnly_AlreadyTaken_MultiOrder()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var helper = new WhsTestHelperFunctions(Factory);
				helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";
				var pivot3 = receiveLine.SerialNumbers.AddNew();
				pivot3.SerialNumberValue = "SN3";
				var pivot4 = receiveLine.SerialNumbers.AddNew();
				pivot4.SerialNumberValue = "SN4";

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
				var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine21 = helper.CreateWhsOrderLine(order2, data.Part1, 1m);
				var orderLine22 = helper.CreateWhsOrderLine(order2, data.Part1, 1m);
				orderLine22.WE_SerialNumber = "SN1";
				var orderLine23 = helper.CreateWhsOrderLine(order2, data.Part1, 1m);
				var pick = helper.CreatePickNew(order1, order2);

				var availableInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => !o.SerialNumber.Equals("SN1")).AvailableInventories[0];
				var availableInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SerialNumber.Equals("SN1")).AvailableInventories[0];

				var serialNumbers1 = availableInventory1.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(4, serialNumbers1.Count());
				AssertEquals(4, serialNumbers1.Count(s => s.Selected));
				AssertAvailableSerialNumber(serialNumbers1, "SN1", true, true, orderLine22.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN2", true, false, order1.Lines[0].PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN3", true, true, orderLine21.PickLines[0].PK);
				AssertAvailableSerialNumber(serialNumbers1, "SN4", true, true, orderLine23.PickLines[0].PK);
				var serialNumbers2 = availableInventory2.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				AssertEquals(1, serialNumbers2.Count());
				AssertAvailableSerialNumber(serialNumbers2, "SN1", true, false, orderLine22.PickLines[0].PK);
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Select_BOM()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var bike = Helper.CreateProduct(data.Org1, "BIKE");
				bike.OP_IsComponentPickedOnSalesOrder = true;
				var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
				var frame = Helper.CreateProduct(data.Org1, "FRAME");
				Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
				Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, frame, AttributeNumber.Serial, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, wheel, 4m, data.Whs1.FindLocation("A-1"));
				var receiveFrame = Helper.CreateWhsReceiveLine(receive, frame, 2m, data.Whs1.FindLocation("A-1"));
				receiveFrame.WE_SerialNumber = "SN0";
				var pivot1 = receiveFrame.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveFrame.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 2m);
				var pick = Helper.CreatePickNew(order);

				var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
				var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
				var wheelPickLine = wheelOrderLine.PickLines.Single();
				var framePickLine = frameOrderLine.PickLines.Single();

				AssertEquals("Precondition - Picked By BOM", true, orderLine1.IsBOMProductPickedOnSalesOrder);
				AssertEquals("Precondition - Picked By BOM", true, orderLine1.ChildComponentLines.Count > 0);

				var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
				var frameComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];

				AssertEquals(4m, wheelComponentInv.PickLineQuantity);
				AssertEquals(2m, frameComponentInv.PickLineQuantity);
				AssertEquals(4m, wheelPickLine.WZ_Units);
				AssertEquals(2m, framePickLine.WZ_Units);
			}
		}

		#endregion

		#endregion

		#endregion

		#region TestAvailableInventoriesSplit_NullPick

		public void TestAvailableInventoriesSplit_NullPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 18, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInExactOrder(availableInventory.AvailableInventoriesSplitByUOM, availableInventory.AvailableInventoriesSplit);

			availableInventory.Deactivate();
			AssertContainsExactElementsInExactOrder(availableInventory.AvailableInventoriesSplitByPickedDetails, availableInventory.AvailableInventoriesSplit);
		}

		#endregion

		#region TestAvailableInventoriesSplit_WithoutPickByUOM

		public void TestAvailableInventoriesSplit_WithoutPickByUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = false;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 18, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInExactOrder(availableInventory.AvailableInventoriesSplitByPickedDetails, availableInventory.AvailableInventoriesSplit);
		}

		#endregion

		#region TestAvailableInventoriesSplit_PickByUOM

		public void TestAvailableInventoriesSplit_PickByUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 18, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInExactOrder(availableInventory.AvailableInventoriesSplitByUOM, availableInventory.AvailableInventoriesSplit);
		}

		#endregion

		#region TestClearPickLinesCache_RefreshesAvailableInventorySplitDetails

		public void TestClearPickLinesCache_RefreshesAvailableInventorySplitDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var existingPickLine = order.Lines[0].PickLines.Single();
			AssertEquals("Available Inventory should have 1 Split Details.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(new[] { existingPickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);

			var newPickLine = order.Lines[0].PickLines.AddNew();
			newPickLine.WZ_WE_InventoryLine = existingPickLine.WZ_WE_InventoryLine;
			availableInventory.ClearPickLinesCache();
			AssertEquals("Available Inventory should have 1 Split Details.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertContainsExactElementsInAnyOrder(new[] { existingPickLine, newPickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
		}

		#endregion

		#region TestCanUpdatePickLineQuantity

		public void TestCanUpdatePickLineQuantity_IsCartonising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Can Update Allocated Quantity.", true, availableInventory.CanUpdatePickLineQuantity);

			using (var mutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString()))
			{
				mutex.Lock();
				AssertEquals("Cannot Update Allocated Quantity if Cartonising.", false, availableInventory.CanUpdatePickLineQuantity);

				using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
				{
					AssertEquals("AutoAllocation always allows updating Allocated Quantity.", true, availableInventory.CanUpdatePickLineQuantity);
				}

				AssertEquals("Cannot Update Allocated Quantity if Cartonising.", false, availableInventory.CanUpdatePickLineQuantity);
			}
		}

		public void TestCanUpdatePickLineQuantity_PickByBOMComponent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == data.Part2.PK).AvailableInventories[0];
			AssertEquals("Cannot Update Allocated Quantity for Pick by BOM Components.", false, availableInventory.CanUpdatePickLineQuantity);

			using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
			{
				AssertEquals("AutoAllocation always allows updating Allocated Quantity.", true, availableInventory.CanUpdatePickLineQuantity);
			}

			AssertEquals("Cannot Update Allocated Quantity for Pick by BOM Components.", false, availableInventory.CanUpdatePickLineQuantity);
		}

		public void TestCanUpdatePickLineQuantity_PickByBOMKit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A-1"));
			var wheelInventory = receive.Inventory[0];
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 30m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			var bikeAvailableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == bike.PK).AvailableInventories[0];
			AssertEquals("Cannot Update Allocated Quantity for Pick by BOM Kits.", false, bikeAvailableInventory.CanUpdatePickLineQuantity);

			using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
			{
				AssertEquals("AutoAllocation always allows updating Allocated Quantity.", true, bikeAvailableInventory.CanUpdatePickLineQuantity);
			}

			AssertEquals("Cannot Update Allocated Quantity for Pick by BOM Kits.", false, bikeAvailableInventory.CanUpdatePickLineQuantity);
		}

		#endregion

		#region TestPickLineQuantity

		public void TestPickLineQuantity_NotCached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Stock is Picked.", 10m, availableInventory.PickLineQuantity);

			availableInventory.PickLines.ElementAt(0).WZ_Units = 5m;
			AssertEquals("PickLineQuantity should be based on actual pick lines qty.", 5m, availableInventory.PickLineQuantity);
		}

		public void TestPickLineQuantity_CanPackIncreasedAllocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Nothing allocated.", 0m, availableInventory.PickLineQuantity);

			availableInventory.PickLineQuantity = 5m;
			var pickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("5 Units should be allocated.", 5m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew("CAS", "MYLABEL-1");
			package1.Pack(pickLine, order.Lines[0].ReleaseLines[0]);
			AssertEquals("5 Units Packed.", 5m, package1.PackedItemDivots.Single().KI_PackedQty);

			availableInventory.PickLineQuantity += 5m; // allocate 5 more units
			var package2 = order.PackageJob.Packages.AddNew("CAS", "MYLABEL-2");

			_ = order.Lines[0].ReleaseLines.Count;
			var packedItems = package2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			AssertEquals("Should have successfully packed the next quantity.", 5m, packedItems.Single().PackedQty);
		}

		#endregion

		#region TestSetPickLineQuantity

		#region TestSetPickLineQuantity

		public void TestSetPickLineQuantity()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;
			var result1 = availableInventoryInternals.SetPickLineQuantity(10m, orderLinePk: null);
			AssertEquals("Should have successfully allocated all units.", 0m, result1);
			AssertEquals("Should have successfully allocated all units.", 10m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1.PickLines.Single() }, availableInventory.PickLines);

			var result2 = availableInventoryInternals.SetPickLineQuantity(15m, orderLinePk: null);
			AssertEquals("Should have successfully allocated all units.", 0m, result2);
			AssertEquals("Should have successfully allocated all units.", 15m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Concat(orderLine2.PickLines), availableInventory.PickLines);

			var result3 = availableInventoryInternals.SetPickLineQuantity(0m, orderLinePk: null);
			AssertEquals("Should have successfully deallocated all units.", 0m, result3);
			AssertEquals("Should have successfully deallocated all units.", 0m, availableInventory.PickLineQuantity);
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			var result4 = availableInventoryInternals.SetPickLineQuantity(30m, orderLinePk: null);
			AssertEquals("Should *not* have allocated all units requested.", 10m, result4);
			AssertEquals("Should have successfully allocated 20m units.", 20m, availableInventory.PickLineQuantity);

			var result5 = availableInventoryInternals.SetPickLineQuantity(-10m, orderLinePk: null);
			AssertEquals("Should *not* have allocated all units requested.", -10m, result5);
			AssertEquals("Should have successfully deallocated 20m units.", 0m, availableInventory.PickLineQuantity);
		}

		#endregion

		#region TestSetPickLineQuantity_WithDocketLineSpecified

		public void TestSetQuantity_WithDocketLineSpecified()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			AssertEquals("Available Inventory should have no PickLines.", 0, availableInventory.PickLines.Count());

			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;
			var result1 = availableInventoryInternals.SetPickLineQuantity(5m, orderLinePk: orderLine1.PK);
			AssertEquals("Should have successfully allocated all units.", 0m, result1);
			AssertEquals("Should have successfully allocated all units.", 5m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1.PickLines.Single() }, availableInventory.PickLines);

			var result2 = availableInventoryInternals.SetPickLineQuantity(20m, orderLinePk: orderLine1.PK);
			AssertEquals("Should *not* have allocated all units requested.", 10m, result2);
			AssertEquals("Should have successfully allocated 10m units.", 10m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1.PickLines.Single() }, availableInventory.PickLines);

			var result3 = availableInventoryInternals.SetPickLineQuantity(-10m, orderLinePk: orderLine1.PK);
			AssertEquals("Should *not* have allocated all units requested.", -10m, result3);
			AssertEquals("Should have successfully deallocated 10m units.", 0m, availableInventory.PickLineQuantity);

			availableInventory.Allocate = true;
			AssertEquals("Precondition: Fully allocated.", 20m, availableInventory.PickLineQuantity);

			var result4 = availableInventoryInternals.SetPickLineQuantity(0m, orderLinePk: orderLine1.PK);
			AssertEquals("Should *not* have allocated all units requested.", -10m, result4);
			AssertEquals("Should have successfully deallocated 10m units.", 10m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine2.PickLines.Single() }, availableInventory.PickLines);
		}

		#endregion

		#region TestSetPickLineQuantity_UpdatePickPercentCompleted

		public void TestSetPickLineQuantity_UpdatePickPercentCompleted() => TestSetPickLineQuantity_UpdatePickPercentCompleted(docketLineSpecified: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_UpdatePickPercentCompleted() => TestSetPickLineQuantity_UpdatePickPercentCompleted(docketLineSpecified: true);

		void TestSetPickLineQuantity_UpdatePickPercentCompleted(bool docketLineSpecified)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locations[1]);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition:", (ZByte)0, pick.WP_PercentageComplete);
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory1.Allocate = true;
			AssertEquals("Item only allocated for pick, should change Pick Percent Completed value.", (ZByte)0, pick.WP_PercentageComplete);

			availableInventory1.PickLines.ElementAt(0).WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Item was picked, should change Pick Percent Completed value.", (ZByte)100, pick.WP_PercentageComplete);

			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories[1];
			var availableInventory2Internals = (IWhsPickAvailableInventoryInternals)availableInventory2;
			availableInventory2Internals.SetPickLineQuantity(5m, orderLinePk: docketLineSpecified ? orderLine1.PK : null);
			AssertEquals("More Items were allocated to Pick, should change Pick Percent Completed value.", (ZByte)50, pick.WP_PercentageComplete);
		}

		#endregion

		#region TestSetPickLineQuantity_ClearsPickIsAllocated

		public void TestSetPickLineQuantity_ClearsPickIsAllocated() => TestSetPickLineQuantity_ClearsPickIsAllocated(docketLineSpecified: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_ClearsPickIsAllocated() => TestSetPickLineQuantity_ClearsPickIsAllocated(docketLineSpecified: true);

		void TestSetPickLineQuantity_ClearsPickIsAllocated(bool docketLineSpecified)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", false, pick.IsAllocated);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;
			availableInventoryInternals.SetPickLineQuantity(10m, orderLinePk: docketLineSpecified ? orderLine1.PK : null);
			AssertEquals("Should have refreshed pick is allocated.", true, pick.IsAllocated);

			availableInventoryInternals.SetPickLineQuantity(0m, orderLinePk: docketLineSpecified ? orderLine1.PK : null);
			AssertEquals("Should have refreshed pick is allocated.", false, pick.IsAllocated);
		}

		#endregion

		#region TestSetPickLineQuantity_UpdatesSplitDetails

		public void TestSetPickLineQuantity_UpdatesSplitDetails() => TestSetPickLineQuantity_UpdatesSplitDetails(docketLineSpecified: false, pickByUOM: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_UpdatesSplitDetails() => TestSetPickLineQuantity_UpdatesSplitDetails(docketLineSpecified: true, pickByUOM: false);

		public void TestSetPickLineQuantity_UpdatesSplitDetails_PickByUOM() => TestSetPickLineQuantity_UpdatesSplitDetails(docketLineSpecified: false, pickByUOM: true);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_UpdatesSplitDetails_PickByUOM() => TestSetPickLineQuantity_UpdatesSplitDetails(docketLineSpecified: true, pickByUOM: true);

		void TestSetPickLineQuantity_UpdatesSplitDetails(bool docketLineSpecified, bool pickByUOM)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = pickByUOM;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 0, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Precondition", 0, availableInventory.AvailableInventoriesSplitByUOM.Count);

			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;
			availableInventoryInternals.SetPickLineQuantity(10m, orderLinePk: docketLineSpecified ? orderLine.PK : null);

			var pickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);

			if (pickByUOM)
			{
				AssertEquals("Should have 1 Inventory Split.", 1, availableInventory.AvailableInventoriesSplitByUOM.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesForPickingDetails);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesOnOrder);
			}
			else
			{
				AssertEquals("Should have 1 Inventory Split.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
			}
		}

		#endregion

		#region TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation

		public void TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation() => TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation(docketLineSpecified: false, pickByUOM: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_DeferSplittingByPackTypeAndPickLineQuantityValidation() => TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation(docketLineSpecified: true, pickByUOM: false);

		public void TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation_PickByUOM() => TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation(docketLineSpecified: false, pickByUOM: true);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_DeferSplittingByPackTypeAndPickLineQuantityValidation_PickByUOM() => TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation(docketLineSpecified: true, pickByUOM: true);

		void TestSetPickLineQuantity_DeferSplittingByPackTypeAndPickLineQuantityValidation(bool docketLineSpecified, bool pickByUOM)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = pickByUOM;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;

			var splitDetails = pickByUOM ? (BusinessObjectCollection)availableInventory.AvailableInventoriesSplitByUOM : availableInventory.AvailableInventoriesSplitByPickedDetails;
			AssertEquals("Precondition", 0, splitDetails.Count);

			var pickLineQuantityValidationCount = 0;
			availableInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCount++;

			using (availableInventory.DeferSplittingByPackTypeAndPickLineQuantityValidation())
			{
				using (availableInventory.DeferSplittingByPackTypeAndPickLineQuantityValidation())
				{
					for (int i = 1; i <= 10; i++)
					{
						availableInventoryInternals.SetPickLineQuantity(i, orderLinePk: docketLineSpecified ? orderLine.PK : null);
						AssertEquals("Should not have updated split details yet.", 0, splitDetails.Count);
						AssertEquals("Should not have validated pick line quantity yet.", 0, pickLineQuantityValidationCount);
					}
				}

				AssertEquals("Should not have updated split details yet.", 0, splitDetails.Count);
				AssertEquals("Should not have validated pick line quantity yet.", 0, pickLineQuantityValidationCount);
			}

			AssertEquals("Should have validated pick line quantity once.", 1, pickLineQuantityValidationCount);

			var pickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);

			if (pickByUOM)
			{
				AssertEquals("Should have 1 Inventory Split.", 1, availableInventory.AvailableInventoriesSplitByUOM.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesForPickingDetails);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesOnOrder);
			}
			else
			{
				AssertEquals("Should have 1 Inventory Split.", 1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
			}
		}

		#endregion

		#region TestSetPickLineQuantity_UpdatesAllocationLog

		public void TestSetPickLineQuantity_UpdatesAllocationLog() => TestSetPickLineQuantity_UpdatesAllocationLog(docketLineSpecified: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_UpdatesAllocationLog() => TestSetPickLineQuantity_UpdatesAllocationLog(docketLineSpecified: true);

		void TestSetPickLineQuantity_UpdatesAllocationLog(bool docketLineSpecified)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;

			availableInventoryInternals.SetPickLineQuantity(10m, orderLinePk: docketLineSpecified ? orderLine.PK : null);
			AssertEquals("5 Units manually allocated by user" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		#endregion

		#region TestSetPickLineQuantity_ClearsWE_ShortfallQuantityCached

		public void TestSetPickLineQuantity_ClearsWE_ShortfallQuantityCached() => TestSetPickLineQuantity_ClearsWE_ShortfallQuantityCached(docketLineSpecified: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_ClearsWE_ShortfallQuantityCached() => TestSetPickLineQuantity_ClearsWE_ShortfallQuantityCached(docketLineSpecified: true);

		void TestSetPickLineQuantity_ClearsWE_ShortfallQuantityCached(bool docketLineSpecified)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;
			AssertEquals("Precondition: Has shortfall quantity.", 10m, orderLine.WE_ShortfallQuantityCached);

			availableInventoryInternals.SetPickLineQuantity(10m, orderLinePk: docketLineSpecified ? orderLine.PK : null);
			AssertEquals("Should have updated shortfall quantity.", 0m, orderLine.WE_ShortfallQuantityCached);

			availableInventoryInternals.SetPickLineQuantity(0m, orderLinePk: docketLineSpecified ? orderLine.PK : null);
			AssertEquals("Should have updated shortfall quantity.", 10m, orderLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestSetPickLineQuantity_ValidatePickLineQuantity

		public void TestSetPickLineQuantity_ValidatePickLineQuantity() => TestSetPickLineQuantity_ValidatePickLineQuantity(docketLineSpecified: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_ValidatePickLineQuantity() => TestSetPickLineQuantity_ValidatePickLineQuantity(docketLineSpecified: true);

		void TestSetPickLineQuantity_ValidatePickLineQuantity(bool docketLineSpecified)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;

			availableInventoryInternals.SetPickLineQuantity(100m, orderLinePk: docketLineSpecified ? orderLine.PK : null);
			AssertHasWarning("When user try to overpick he should receive a warning.", availableInventory.PickLineQuantityInfo, "Quantity Allocated cannot be greater than Units Ordered. First deallocate stock from other inventories before allocating this stock.");
		}

		#endregion

		#region TestSetPickLineQuantity_RefreshesBindingOnPickLineQuantityInfo

		public void TestSetPickLineQuantity_RefreshesBindingOnPickLineQuantityInfo() => TestSetPickLineQuantity_RefreshesBindingOnPickLineQuantityInfo(docketLineSpecified: false);
		public void TestSetPickLineQuantity_WithDocketLineSpecified_RefreshesBindingOnPickLineQuantityInfo() => TestSetPickLineQuantity_RefreshesBindingOnPickLineQuantityInfo(docketLineSpecified: true);

		void TestSetPickLineQuantity_RefreshesBindingOnPickLineQuantityInfo(bool docketLineSpecified)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;

			var bindingRefreshed = false;
			availableInventory.PickLineQuantityInfo.ValueChanged += (s, e) => bindingRefreshed = true;

			availableInventoryInternals.SetPickLineQuantity(5m, orderLinePk: docketLineSpecified ? orderLine.PK : null);
			AssertEquals("Should have refreshed binding.", true, bindingRefreshed);
		}

		#endregion

		#region TestSetPickLineQuantity_DoesnotCreateNewPickLineFor Loading/Loaded/Departed Order

		public void TestSetPickLineQuantity_DoesnotCreateNewPickLineForLoadingOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 40m;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: one Pick Line with 40 units.", 40m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loading, order.WarehouseOrderStatus);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("No new Pick Line is Created.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Cannot allocate more.", 40m, availableInventory.PickLineQuantity);
		}

		public void TestSetPickLineQuantity_DoesnotCreateNewPickLineForLoadedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 40m;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: one Pick Line with 40 units.", 40m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loaded, order.WarehouseOrderStatus);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("No new Pick Line is Created.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Cannot allocate more.", 40m, availableInventory.PickLineQuantity);
		}

		public void TestSetPickLineQuantity_DoesnotCreateNewPickLineForDepartedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 40m;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: one Pick Line with 40 units.", 40m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("No new Pick Line is Created.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Cannot allocate more.", 40m, availableInventory.PickLineQuantity);
		}

		public void TestSetPickLineQuantity_DoesnotCreateNewPickLineForDepartedOrder_HasAnotherOrderCanBeAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 5m;

			AssertEquals("Precondition: only 1 PickLine.", 1, pick.GetAllPickLines().Count());
			var pickLine = order1.Lines[0].PickLines[0];
			AssertEquals("Precondition: this PickLine is on order1.", 5m, pickLine.WZ_Units);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package1 = order1.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			AssertEquals("Order1 Status correct", WhsOrderStatus.Codes.Departed, order1.WarehouseOrderStatus);
			AssertEquals("Order2 Status correct", DocketStatus.Codes.AttachedToPick, order2.WarehouseOrderStatus);

			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 20m;
			AssertEquals("Only 1 new Pick Line is Created.", 2, pick.GetAllPickLines().Count());
			AssertEquals("New Pick Line is on order2.", 10m, pick.GetAllPickLines().Single(l => l.WZ_WE_TransactionLine == order2.Lines[0].PK).WZ_Units);
			AssertEquals("Pick Line on order1 does not change.", 5m, pick.GetAllPickLines().Single(l => l.WZ_WE_TransactionLine == order1.Lines[0].PK).WZ_Units);
			AssertEquals("5 units cannot be allocated on order1 because it's departed.", 15m, availableInventory.PickLineQuantity);
		}

		#endregion

		#endregion

		#region TestIsPicking

		public void TestIsPicking()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();
			var availableInventoryInternals = (IWhsPickAvailableInventoryInternals)availableInventory;
			var result = availableInventoryInternals.SetPickLineQuantity(10m, orderLinePk: null);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_IsPicking = true;
			pickLine.WZ_GS_NKAssignedTo = "A";

			AssertEquals(true, availableInventory.IsCurrentlyPicking);
		}

		#endregion

		#region TestAllowPickLineQuantityReduction

		public void TestAllowPickLineQuantityReduction()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories[0].AvailableInventories.Single();

			AssertEquals("Original value", false, availableInventory.allowQuantityReduction);

			using (availableInventory.AllowPickLineQuantityReduction())
			{
				AssertEquals("First setup", true, availableInventory.allowQuantityReduction);

				AssertExceptionThrown<InvalidOperationException>("Throw exception when nesting the value", "You may not invoke AllowPickLineQuantityReduction more than once.", () =>
				{
					using (availableInventory.AllowPickLineQuantityReduction())
					{
					}
				});
			}

			AssertEquals(false, availableInventory.allowQuantityReduction);
		}

		#endregion

		#region TestDeactivate

		public void TestDeactivate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: Stock is Picked.", 10m, availableInventory.PickLineQuantity);
			AssertNotNull("Precondition", availableInventory.OrderedInventory);

			availableInventory.Deactivate();
			AssertNull("Shoudl clear ordered inventory.", availableInventory.OrderedInventory);
		}

		#endregion

		#region TestDeletingPickLineShouldClearPickLineQuantityCache

		public void TestDeletingPickLineShouldClearPickLineQuantityCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: Stock is Picked.", 10m, orderedInventory.PickLineQuantity);
			AssertEquals("Precondition: Stock is Picked.", 10m, availableInventory.PickLineQuantity);

			pick.GetAllPickLines().ToArray().ForEach(pl => pl.Delete());
			AssertEquals("Deleting Pick Line should have cleared PickLineQuantity Cache.", 0m, orderedInventory.PickLineQuantity);
			AssertEquals("Deleting Pick Line should have cleared PickLineQuantity Cache.", 0m, availableInventory.PickLineQuantity);
		}

		public void TestDeletingPickLineShouldClearPickLineQuantityCache_DataRefresh()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Available Inventory is allocated.", 6m, availableInventory.PickLineQuantity);

			orderLine1.PickLines.Single().Delete();
			AssertEquals("PickLine Quantity should have updated.", 4m, availableInventory.PickLineQuantity);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			pickInOtherFactory.GetAllPickLines().ToArray().ForEach(pl => pl.Delete());
			AssertEquals("PickLine Quantity should not have updated yet, as the deletion occured in another factory.", 4m, availableInventory.PickLineQuantity);

			otherFactory.Save();
			AssertEquals("PickLine Quantity should have updated as data refresh would have deleted the pick line in this factory.", 0m, availableInventory.PickLineQuantity);
		}

		#endregion

		#region Quantity Available To Pick

		public void TestQuantityAvailableToPick()
		{
			SetupWhsPickOrderedInventory();
			AssertEquals(20m, AvailableInventory.QuantityAvailableToPick);

			AvailableInventory.Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Available;
			AvailableInventory.Inventory[0].WI_TotalUnits = 10.5m;

			AvailableInventory.Inventory[1].WI_InventoryStatus = InventoryStatus.Codes.Available;
			AvailableInventory.Inventory[1].WI_TotalUnits = 25m;

			var order = Helper.CreateWhsOrder(AvailableInventory.OrderedInventory.Client, AvailableInventory.OrderedInventory.Pick.Warehouse, "1");
			var orderLine = Helper.CreateWhsOrderLine(order, AvailableInventory.OrderedInventory.SupplierPart, 2m);
			Helper.CreateReservePickLine(orderLine, AvailableInventory.Inventory[0], 2m);

			AssertEquals(33.5m, AvailableInventory.QuantityAvailableToPick);

			AvailableInventory.PickLineQuantity = 10m;
			AssertEquals(33.5m, AvailableInventory.QuantityAvailableToPick);
		}

		public void TestQuantityAvailableToPickInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.QuantityAvailableToPick, AvailableInventory.QuantityAvailableToPickInfo.Name);
		}

		public void TestQuantityAvailableToPick_LocationStatus_AVL() =>
			TestQuantityAvailableToPick_LocationStatusCore(
				InventoryStatus.Codes.Available,
				validLocationStatuses: [LocationStatus.Codes.Normal],
				invalidLocationStatuses: [LocationStatus.Codes.Damaged, LocationStatus.Codes.Held, LocationStatus.Codes.Void]);

		public void TestQuantityAvailableToPick_LocationStatus_HEL() // Picking held inventory from void locations will be addressed in "WI00795398 - POH - Update Business Logic to Support allocating from held locations"
			=> TestQuantityAvailableToPick_LocationStatusCore(
				InventoryStatus.Codes.Held,
				validLocationStatuses: [LocationStatus.Codes.Normal, LocationStatus.Codes.Damaged, LocationStatus.Codes.Held, LocationStatus.Codes.Void],
				invalidLocationStatuses: [],
				InventoryHoldCodes.Codes.Damaged);

		public void TestQuantityAvailableToPick_LocationStatus_HEL_HeldGoodsForOrdersDisabled()
			=> TestQuantityAvailableToPick_LocationStatusCore(
				InventoryStatus.Codes.Held,
				validLocationStatuses: [],
				invalidLocationStatuses: [LocationStatus.Codes.Normal, LocationStatus.Codes.Damaged, LocationStatus.Codes.Held, LocationStatus.Codes.Void],
				InventoryHoldCodes.Codes.Damaged,
				enableHeldGoodsForOrders: false);

		void TestQuantityAvailableToPick_LocationStatusCore(string inventoryStatus, string[] validLocationStatuses, string[] invalidLocationStatuses, string holdCode = "", bool enableHeldGoodsForOrders = true)
		{
			var expectHasIsPickableAndAvailable = enableHeldGoodsForOrders;
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 1);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, string.Empty, inventoryStatus, holdCode);
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				if (enableHeldGoodsForOrders && inventoryStatus == InventoryStatus.Codes.Held)
				{
					orderLine.WE_WHC_NKOrderedHeldCode = holdCode;
				}

				var pick = Helper.CreatePickNew(order);

				if(!enableHeldGoodsForOrders && inventoryStatus == InventoryStatus.Codes.Held)
				{
					AssertEquals("Held inventory cannot be picked", 0, pick.OrderedInventories[0].AvailableInventories.Count);
				}
				else
				{
					var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
					availableInventory.PickLineQuantity = 0;

					foreach (var locationStatus in validLocationStatuses)
					{
						availableInventory.Inventory[0].Location.WLV_LocationStatus = locationStatus;
						AssertEquals($"QuantityAvailableToPick should be available for {inventoryStatus} at {locationStatus}", 10m, availableInventory.QuantityAvailableToPick);
					}

					foreach (var locationStatus in invalidLocationStatuses)
					{
						availableInventory.Inventory[0].Location.WLV_LocationStatus = locationStatus;
						AssertEquals($"QuantityAvailableToPick should be unavailable for {inventoryStatus} at {locationStatus}", 0m, availableInventory.QuantityAvailableToPick);
					}
				}
			}
		}

		#endregion

		#region TestQuantityAvailableToPick_AfterPicking

		public void TestQuantityAvailableToPick_AfterPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5);
			var pick = Helper.CreatePickNew(order1, order2);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 20m, availableInventory.QuantityAvailableToPick);

			PickLinePick(availableInventory.PickLines.Single(pl => pl.WZ_Units == 3m));
			AssertEquals("Should decrease available to pick.", 20m - 3m, availableInventory.QuantityAvailableToPick);

			PickLinePick(availableInventory.PickLines.Single(pl => pl.WZ_Units == 5m));
			AssertEquals("Should decrease available to pick.", 20m - 3m - 5m, availableInventory.QuantityAvailableToPick);

			pick.FinalisePick();
			AssertEquals("Should show same value after finalize pick.", 20m - 3m - 5m, availableInventory.QuantityAvailableToPick);
		}

		public void TestQuantityAvailableToPick_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5);
			var pick = Helper.CreatePickNew(order1, order2);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 20m, availableInventory.QuantityAvailableToPick);

			var pickLine = availableInventory.PickLines.Single(pl => pl.WZ_Units == 3m);
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Should decrease available to pick.", 20m - 3m, availableInventory.QuantityAvailableToPick);
		}

		static void PickLinePick(WhsPickLine pickLine)
		{
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition", true, pickLine.IsPicked);
		}

		#endregion

		#region TestQuantityAvailableToPick_WhenFinalisePick

		public void TestQuantityAvailableToPick_WhenFinalisePick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5);
			var pick = Helper.CreatePickNew(order1, order2);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 20m, availableInventory.QuantityAvailableToPick);

			var orderUnRelated = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2);
			var pickUnRelated = Helper.CreatePickNew(orderUnRelated);

			AssertEquals("Orders in other pick will effect on QuantityAvailableToPick.", 18m, availableInventory.QuantityAvailableToPick);

			order1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order1);
			AssertEquals("Even user finalise order still 18 units are available for this pick.", 18m, availableInventory.QuantityAvailableToPick);
			order2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order2);
			AssertEquals("Even user finalise order still 18 units are available for this pick.", 18m, availableInventory.QuantityAvailableToPick);
			pick.FinalisePick();
			AssertEquals("After finalise pick user cannot change allocate items and Quantity available to pick, so QuantityAvailableToPick should be decreased.", 18m - 3m - 5m, availableInventory.QuantityAvailableToPick);
		}

		#endregion

		#region TestQuantityAvailableToPick_SplitPickLines

		public void TestQuantityAvailableToPick_SplitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 18, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			AssertEquals(3, availableInventory.PickLines.Count()); // 1 CAS + 1 BOX + 3 UNT
			AssertEquals("Precondotion", 18m, availableInventory.QuantityAvailableToPick);

			PickLinePick(availableInventory.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == "CAS"));
			AssertEquals("After picking CAS Quantity Available To Pick should be reduced.", 18m - 10m, availableInventory.QuantityAvailableToPick);
		}

		#endregion

		#region TestDeallocateAvailableInventoryAfterReducingOrderTransactionQuantity

		public void TestDeallocateAvailableInventoryAfterReducingOrderTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-2-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, location2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition:There must be one ordered inventory line in pick.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: There must be two available inventory lines in pick.", 2, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory1.Allocate = true;
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories[1];
			availableInventory2.Allocate = true;
			order.RunPreSaveValidation();
			pick.RunPreSaveValidation();
			AssertEquals("Precondition: Order must not have any errors.", true, !order.HasErrors);
			AssertEquals("Precondition: Pick must not have any errors.", true, !pick.HasErrors);
			Factory.Save();

			var factoryToLoadOrder = new BusinessObjectFactory();
			var factoryToLoadPick = new BusinessObjectFactory();
			var orderInAnotherFactory = factoryToLoadOrder.Load<WhsOrder>(order.PK);
			var pickInAnotherFactory = factoryToLoadPick.Load<WhsPick>(pick.PK);

			var availableInventory2InAnotherFactory = pickInAnotherFactory.OrderedInventories[0].AvailableInventories[1];
			var poke = availableInventory2InAnotherFactory.AvailableInventoriesSplitByPickedDetails;
			var poke2 = availableInventory2InAnotherFactory.PickLines;

			orderInAnotherFactory.Lines.Single().WE_PackQuantity = 1;
			orderInAnotherFactory.IsSavedFromOrderForm = true;
			orderInAnotherFactory.RunPreSaveValidation();
			AssertEquals(true, availableInventory2InAnotherFactory.Allocate);
			AssertEquals(1m, availableInventory2InAnotherFactory.PickLineQuantity);

			factoryToLoadOrder.Save();
			AssertEquals(false, availableInventory2InAnotherFactory.Allocate);
			AssertEquals(0m, availableInventory2InAnotherFactory.PickLineQuantity);
			AssertNoExceptionThrown(() => pickInAnotherFactory.RunPreSaveValidation());
		}

		#endregion

		#region Quantity Total

		public void TestQuantityTotal()
		{
			SetupWhsPickOrderedInventory();
			AssertEquals(20m, AvailableInventory.QuantityTotal);

			AvailableInventory.Inventory.AddNew();
			AvailableInventory.Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Available;
			AvailableInventory.Inventory[0].WI_TotalUnits = 10.5m;
			AvailableInventory.Inventory[1].WI_InventoryStatus = InventoryStatus.Codes.Available;
			AvailableInventory.Inventory[1].WI_TotalUnits = 25m;
			AvailableInventory.Inventory[1].WI_WL = AvailableInventory.Inventory[0].WI_WL;

			var order = Helper.CreateWhsOrder(AvailableInventory.OrderedInventory.Client, AvailableInventory.OrderedInventory.Pick.Warehouse, "1");
			var orderLine = Helper.CreateWhsOrderLine(order, AvailableInventory.OrderedInventory.SupplierPart, 2m);
			Helper.CreateReservePickLine(orderLine, AvailableInventory.Inventory[0], 2m);

			AssertEquals(35.5m, AvailableInventory.QuantityTotal);

			AvailableInventory.PickLineQuantity = 10m;
			AssertEquals(35.5m, AvailableInventory.QuantityTotal);
		}

		public void TestQuantityTotalInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.QuantityTotal, AvailableInventory.QuantityTotalInfo.Name);
		}

		#endregion

		#region Quantity Committed

		#region TestQuantityCommitted

		public void TestQuantityCommitted()
		{
			//create 2 inventories for the same product 10.5 + 25 units (same location)
			var year = ZDateTime.Now.Year;

			SetupWhsPickOrderedInventory();
			Factory.Save();
			AssertEquals(0m, AvailableInventory.QuantityCommitted);

			var order = Helper.CreateWhsOrder(AvailableInventory.OrderedInventory.Client, AvailableInventory.OrderedInventory.Pick.Warehouse, "1");
			var orderLine = Helper.CreateWhsOrderLine(order, AvailableInventory.OrderedInventory.Product.Parent, 2m);
			Helper.CreateReservePickLine(orderLine, AvailableInventory.Inventory[0], 2m);

			var order2 = Helper.CreateWhsOrder(AvailableInventory.OrderedInventory.Client, AvailableInventory.OrderedInventory.Pick.Warehouse, "2", Notify);
			Helper.CreateWhsOrderLine(order2, AvailableInventory.OrderedInventory.Product.Parent, 5.5m);
			Helper.CreatePickByAttachingOrders(order2);

			AssertEquals(5.5m, AvailableInventory.QuantityCommitted);

			AvailableInventory.PickLineQuantity = 10m;
			AssertEquals(15.5m, AvailableInventory.QuantityCommitted);
		}

		#endregion

		#region TestQuantityCommitted_Transfers

		public void TestQuantityCommitted_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit stock.

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Quantity Committed should include committed qty from both pick and transfer.", 5m, pick.OrderedInventories[0].AvailableInventories[0].QuantityCommitted);
		}

		#endregion

		#region TestQuantityCommittedInfo

		public void TestQuantityCommittedInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.QuantityCommitted, AvailableInventory.QuantityCommittedInfo.Name);
		}

		#endregion

		#endregion

		#region PickLineQuantity

		#region TestPickLineQuantity_RefreshSplitByPickerDetails

		public void TestPickLineQuantity_RefreshSplitByPickerDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = false;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(availableInventory1.PickLineQuantity, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
			availableInventory1.PickLineQuantity = 3;
			AssertEquals("StockUnitQuantity in SplitByPickedDetails should change when PickLineQuantity changes", 3m, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].StockUnitQuantity);
		}

		#endregion

		#region TestPickLineQuantity_Complex

		public void TestPickLineQuantity_Complex()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 20m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 30m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AssertEquals("Precondition", 1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];
			AvailableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition", 40m, orderedInventory.QuantityOrdered);
			AssertEquals("Precondition", 0m, orderedInventory.PickLineQuantity);
			AssertEquals("Precondition", 40m, orderedInventory.QuantityShort);
			AssertEquals("Precondition", 3, AvailableInventory.Inventory.Count);
			AssertEquals("Precondition", 60m, AvailableInventory.QuantityAvailableToPick);
			AvailableInventory.Inventory.Sort(WhsInventoryViewSchema.WI_TotalUnits.Name);

			AssertQuantities(5m, 5m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(5m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertPickLine(availableInventory.PickLines.ElementAt(0), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[0].WI_WE_InDocketLine, 5m, orderedInventory);

			AssertQuantities(8m, 8m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(8m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertPickLine(availableInventory.PickLines.ElementAt(0), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[0].WI_WE_InDocketLine, 8m, orderedInventory);

			AssertQuantities(25m, 25m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(2, availableInventory.PickLines.Count());
			AssertEquals(10m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(15m, AvailableInventory.Inventory[1].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertPickLine(availableInventory.PickLines.ElementAt(0), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[0].WI_WE_InDocketLine, 10m, orderedInventory);
			AssertPickLine(AvailableInventory.PickLines.ElementAt(1), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[1].WI_WE_InDocketLine, 15m, orderedInventory);

			AssertQuantities(30m, 30m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(2, availableInventory.PickLines.Count());
			AssertEquals(10m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(20m, AvailableInventory.Inventory[1].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertPickLine(availableInventory.PickLines.ElementAt(0), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[0].WI_WE_InDocketLine, 10m, orderedInventory);
			AssertPickLine(AvailableInventory.PickLines.ElementAt(1), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[1].WI_WE_InDocketLine, 20m, orderedInventory);

			AssertQuantities(40m, 40m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(3, availableInventory.PickLines.Count());
			AssertEquals(10m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(20m, AvailableInventory.Inventory[1].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(10m, AvailableInventory.Inventory[2].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertPickLine(availableInventory.PickLines.ElementAt(0), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[0].WI_WE_InDocketLine, 10m, orderedInventory);
			AssertPickLine(AvailableInventory.PickLines.ElementAt(1), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[1].WI_WE_InDocketLine, 20m, orderedInventory);
			AssertPickLine(AvailableInventory.PickLines.ElementAt(2), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[2].WI_WE_InDocketLine, 10m, orderedInventory);

			AssertQuantities(0m, 0m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(0, availableInventory.PickLines.Count());
			AssertEquals(0m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(0m, AvailableInventory.Inventory[1].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(0m, AvailableInventory.Inventory[2].CommittedQuantityIncludingUnfinalisedReceipt);

			AssertQuantities(100m, 40m);
			AssertEquals(60m, AvailableInventory.QuantityAvailableToPick);
			AssertEquals(3, availableInventory.PickLines.Count());
			AssertEquals(10m, AvailableInventory.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(20m, AvailableInventory.Inventory[1].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals(10m, AvailableInventory.Inventory[2].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertPickLine(availableInventory.PickLines.ElementAt(0), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[0].WI_WE_InDocketLine, 10m, orderedInventory);
			AssertPickLine(AvailableInventory.PickLines.ElementAt(1), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[1].WI_WE_InDocketLine, 20m, orderedInventory);
			AssertPickLine(AvailableInventory.PickLines.ElementAt(2), orderedInventory.Client.PK, orderedInventory.SupplierPart.PK, AvailableInventory.Inventory[2].WI_WE_InDocketLine, 10m, orderedInventory);
			AssertHasWarnings(AvailableInventory.PickLineQuantityInfo);
			AssertNoErrors(orderedInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestPickLineQuantity_CannotBeChangedForPickByBOM

		public void TestPickLineQuantity_CannotBeChangedForPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 25m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine = order.Lines[0];
			var wheelLine = Helper.CreateWhsOrderLine(order, wheel, 1m);
			var frameLine = Helper.CreateWhsOrderLine(order, frame, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine.ChildComponentLines.Count > 0);

			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var frameComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var wheelInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && !ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var frameInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && !ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];

			AssertEquals("Precondition", 20m, wheelComponentInv.PickLineQuantity);
			AssertEquals("Precondition", 10m, frameComponentInv.PickLineQuantity);
			AssertEquals("Precondition", 1m, wheelInv.PickLineQuantity);
			AssertEquals("Precondition", 1m, frameInv.PickLineQuantity);

			wheelComponentInv.PickLineQuantity = 10m;
			frameComponentInv.PickLineQuantity = 5m;
			AssertEquals("Should not have changed quantity picked", 20m, wheelComponentInv.PickLineQuantity);
			AssertEquals("Should not have changed quantity picked", 10m, frameComponentInv.PickLineQuantity);

			wheelComponentInv.Allocate = false;
			frameComponentInv.Allocate = false;
			AssertEquals("Should not have changed quantity picked", 20m, wheelComponentInv.PickLineQuantity);
			AssertEquals("Should not have changed quantity picked", 10m, frameComponentInv.PickLineQuantity);
			AssertEquals("Should not have changed quantity picked", true, wheelComponentInv.Allocate);
			AssertEquals("Should not have changed quantity picked", true, frameComponentInv.Allocate);

			wheelInv.PickLineQuantity = 0m;
			frameInv.PickLineQuantity = 0m;
			AssertEquals("Should be able to change PickLineQuantity for ordered component lines", 0m, wheelInv.PickLineQuantity);
			AssertEquals("Should be able to change PickLineQuantity for ordered component lines", 0m, frameInv.PickLineQuantity);

			wheelInv.Allocate = true;
			frameInv.Allocate = true;
			AssertEquals("Should be able to change PickLineQuantity for ordered component lines", 1m, wheelInv.PickLineQuantity);
			AssertEquals("Should be able to change PickLineQuantity for ordered component lines", 1m, frameInv.PickLineQuantity);
		}

		#endregion

		#region TestPickLineQuantity_TriesToIncrementAndDecrementExistingPickLines

		public void TestPickLineQuantity_TriesToIncrementAndDecrementExistingPickLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();
			data.Line112.WI_PartAttrib1 = "PA1";
			data.Line112.InDocketLine.WE_PartAttrib1 = "PA1";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			AssertEquals(10m, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 20m;
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			AssertEquals(20m, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);

			// now test decrement
			var pickLine = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 15m;
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
			AssertEquals(15m, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);
			AssertEquals(pickLine, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0));
		}

		public void TestPickLineQuantity_TriesToIncrementAndDecrementExistingPickLines_HeldInventoryOrder()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				receive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;

				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(order);

				pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
				AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
				AssertEquals(10m, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);

				pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 20m;
				AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
				AssertEquals(20m, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);

				// now test decrement
				var pickLine = pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0);
				pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 15m;
				AssertEquals(1, pick.OrderedInventories[0].AvailableInventories[0].PickLines.Count());
				AssertEquals(15m, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units);
				AssertEquals(pickLine, pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0));
			}
		}

		#endregion

		#region TestPickLineQuantity_WhenAttemptingToOverAllocate

		public void TestPickLineQuantity_WhenAttemptingToOverAllocate()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 80m);

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			ZDecimal? quantityThatCouldNotBeAllocatedOrDeallocated = null;
			availableInventory.PickLineQuantityInfo.ValueChanged += delegate
				{ quantityThatCouldNotBeAllocatedOrDeallocated = availableInventory.QuantityThatCouldNotBeAllocatedOrDeallocated; };

			availableInventory.PickLineQuantity = 50m;
			AssertAllocatedItem(orderedInventory, availableInventory, 50m, 30m, true);
			AssertEquals("We should have no unpicked units.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);
			quantityThatCouldNotBeAllocatedOrDeallocated = null; // clean-up

			availableInventory.PickLineQuantity = 90m;
			AssertAllocatedItem(orderedInventory, availableInventory, 80m, 0m, true);
			AssertEquals("We should have 10 unpicked units.", 10m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertHasWarning("When user try to overpick he should receive a warning.", availableInventory.PickLineQuantityInfo, "Quantity Allocated cannot be greater than Units Ordered. First deallocate stock from other inventories before allocating this stock.");
			quantityThatCouldNotBeAllocatedOrDeallocated = null; // clean-up

			orderLine1.WE_TransactionQuantity = 120m;
			availableInventory.PickLineQuantity = 110m;
			AssertAllocatedItem(orderedInventory, availableInventory, 100m, 20m, true);
			AssertEquals("We should have 20 unpicked units.", 10m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertHasWarning("When user try to Pick more stock that available to be picked he should receive an error.", availableInventory.PickLineQuantityInfo, "Only 100 Units are available for allocation.");
			quantityThatCouldNotBeAllocatedOrDeallocated = null; // clean-up

			availableInventory.PickLineQuantity = -10m;
			AssertAllocatedItem(orderedInventory, availableInventory, 0m, 120m, false);
			AssertEquals("We should have 20 unpicked units.", -10m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertHasWarning("When user try to deallocate more stock that was allocated he should get an error.", availableInventory.PickLineQuantityInfo, "Please enter a value greater than or equal to zero.");
			quantityThatCouldNotBeAllocatedOrDeallocated = null; // clean-up

			availableInventory.Allocate = true;
			AssertAllocatedItem(orderedInventory, availableInventory, 100m, 20m, true);
			AssertEquals("We should have no unpicked units.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
			AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);

			// Testing reload of pick lines
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Just to ensure that OrderedInventories are rebuilt", 1, pickInOtherFactory.Orders.Count);
			AssertAllocatedItem(pickInOtherFactory.OrderedInventories[0], pickInOtherFactory.OrderedInventories[0].AvailableInventories[0], 100m, 20m, true);
		}

		#endregion

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Attribute1()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Core(AttributeNumber.One);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Attribute2()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Core(AttributeNumber.Two);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Attribute3()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Core(AttributeNumber.Three);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_SerialNumber()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Core(AttributeNumber.Serial);
		}

		void TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_SingleLine_Core(AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var poke = orderLine.ReleaseLines.Count; // ReleaseLines Collection manages AttributesMet

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantity = 1m;
			AssertEquals(1, orderLine.ReleaseLines.Count);
			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals(1m, releaseLine1.Quantity);

			releaseLine1.Quantity = 1m;
			setPartAttribute(attributeNumber, releaseLine1);

			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: 1 Release Line", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Precondition: 1 Pick Line", 1, pickLines.Count());
			AssertEquals("Precondition: 1 Pick Line has RCA", "RED", getPartAttribute(attributeNumber, pickLines.Single()));
			AssertEquals("Precondition: This Pick Line has units of 1m", 1m, pickLines.Single().WZ_Units);

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("PickLineQuantity should be changed", 0m, orderedInventory.AvailableInventories[0].PickLineQuantity);
			AssertEquals("ReleaseLineQuantity should be changed", 0m, releaseLine1.Quantity);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);
		}

		void setPartAttribute(AttributeNumber attributeNumber, WhsReleaseLine releaseLine)
		{
			if (attributeNumber == AttributeNumber.One)
			{
				releaseLine.PartAttribute1 = "RED";
			}
			else if (attributeNumber == AttributeNumber.Two)
			{
				releaseLine.PartAttribute2 = "RED";
			}
			else if (attributeNumber == AttributeNumber.Three)
			{
				releaseLine.PartAttribute3 = "RED";
			}
			else if (attributeNumber == AttributeNumber.Serial)
			{
				releaseLine.SerialNumber = "RED";
			}
		}

		string getPartAttribute(AttributeNumber attributeNumber, WhsReleaseLine releaseLine)
		{
			switch (attributeNumber)
			{
				case AttributeNumber.One:
					return releaseLine.PartAttribute1;
				case AttributeNumber.Two:
					return releaseLine.PartAttribute2;
				case AttributeNumber.Three:
					return releaseLine.PartAttribute3;
				case AttributeNumber.Serial:
					return releaseLine.SerialNumber;
				default:
					return string.Empty;
			}
		}

		string getPartAttribute(AttributeNumber attributeNumber, WhsPickLine pickLine)
		{
			switch (attributeNumber)
			{
				case AttributeNumber.One:
					return pickLine.WZ_ReleaseCapturedPartAttrib1;
				case AttributeNumber.Two:
					return pickLine.WZ_ReleaseCapturedPartAttrib2;
				case AttributeNumber.Three:
					return pickLine.WZ_ReleaseCapturedPartAttrib3;
				case AttributeNumber.Serial:
					return pickLine.WZ_ReleaseCapturedSerialNumber;
				default:
					return string.Empty;
			}
		}

		public void TestPickLineQuantity_WithMutiplePickLinesWithRCAs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			var pickLines = orderLine.PickLines;
			availableInventory.Allocate = true;
			availableInventory.PickLineQuantity = 10;
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";

			availableInventory.PickLineQuantity = 15;
			AssertEquals("Pick line number should be 2.", 2, pickLines.Count);
			pickLines[1].WZ_ReleaseCapturedPartAttrib1 = "BLUE";
			AssertEquals("WZ_Units of PickLine 1 should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("PickLine 1 should have ReleaseCapturedAttribs.", true, pickLines[0].HasReleaseCapturedAttribs);
			AssertEquals("WZ_Units of PickLine 2 should be 5m.", 5m, pickLines[1].WZ_Units);
			AssertEquals("PickLine 2 should have ReleaseCapturedAttribs.", true, pickLines[1].HasReleaseCapturedAttribs);

			availableInventory.PickLineQuantity = 14;
			AssertEquals("Pick line number should remaining 2.", 2, pickLines.Count);
			AssertEquals("Release Captured units should revert to 15m.", 15m, availableInventory.PickLineQuantity);
			AssertEquals("WZ_Units of Pick line 1 should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("Pick line 1 should have ReleaseCapturedAttribs as RED.", "RED", pickLines[0].WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("WZ_Units of Pick line 2 should be 5m.", 5m, pickLines[1].WZ_Units);
			AssertEquals("Pick line 2 should have ReleaseCapturedAttribs as BLUE.", "BLUE", pickLines[1].WZ_ReleaseCapturedPartAttrib1);

			availableInventory.PickLineQuantity = 9;
			AssertEquals("Pick line number should reduce to 1.", 1, pickLines.Count);
			AssertEquals("Release Captured units should revert to 10m.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("WZ_Units of the only Pick line should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("The only Pick line should have ReleaseCapturedAttribs as RED.", "RED", pickLines[0].WZ_ReleaseCapturedPartAttrib1);

			availableInventory.PickLineQuantity = 0;
			AssertEquals("Pick line should be empty.", 0, pickLines.Count);
			AssertEquals("PickLineQuantity Should be 0m", 0m, availableInventory.PickLineQuantity);
			AssertEquals("Should be dellocated", false, availableInventory.Allocate);
		}

		public void TestPickLineQuantity_WithOnePickLineWithRCAsAndAnothersWithoutRCAs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			var pickLines = orderLine.PickLines;
			availableInventory.Allocate = true;
			availableInventory.PickLineQuantity = 10;
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";

			availableInventory.PickLineQuantity = 15;
			AssertEquals("Pick line number should be 2.", 2, pickLines.Count);
			AssertEquals("WZ_Units of PickLine 1 should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("PickLine 1 should have ReleaseCapturedAttribs.", true, pickLines[0].HasReleaseCapturedAttribs);
			AssertEquals("WZ_Units of PickLine 2 should be 5m.", 5m, pickLines[1].WZ_Units);
			AssertEquals("PickLine 2 should NOT have ReleaseCapturedAttribs.", false, pickLines[1].HasReleaseCapturedAttribs);

			availableInventory.PickLineQuantity = 14;
			AssertEquals("Pick line number should be 2.", 2, pickLines.Count);
			AssertEquals("Release Captured units should reduce to 14m.", 14m, availableInventory.PickLineQuantity);
			AssertEquals("WZ_Units of Pick line 1 should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("Pick line 1 should have ReleaseCapturedAttribs as RED.", "RED", pickLines[0].WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("WZ_Units of Pick line 2 should be 4m.", 4m, pickLines[1].WZ_Units);
			AssertEquals("Pick line 2 should NOT have ReleaseCapturedAttribs.", false, pickLines[1].HasReleaseCapturedAttribs);

			availableInventory.PickLineQuantity = 9;
			AssertEquals("Pick line number should be 1.", 1, pickLines.Count);
			AssertEquals("Release Captured units should be 10m.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("WZ_Units of the only Pick line should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("The only Pick line should have ReleaseCapturedAttribs as RED.", "RED", pickLines[0].WZ_ReleaseCapturedPartAttrib1);
			availableInventory.PickLineQuantity = 0;
			AssertEquals("Pick line should be empty.", 0, pickLines.Count);
			AssertEquals("PickLineQuantity Should be 0m", 0m, availableInventory.PickLineQuantity);
			AssertEquals("Should be dellocated", false, availableInventory.Allocate);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Decrease_MultipleLines_OneIsReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var poke = orderLine.ReleaseLines.Count; // ReleaseLines Collection manages AttributesMet

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantity = 5m;
			AssertEquals(1, orderLine.ReleaseLines.Count);
			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals(5m, releaseLine1.Quantity);

			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.Quantity = 4m;
			orderLine.ReleaseLines.AddNew().Quantity = 1m;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: 2 Pick Lines created", 2, pickLines.Count());
			AssertEquals("Precondition: 1 Pick Line has RCA", "RED", pickLines.Single(l => l.WZ_Units == 4m).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Precondition: 1 Pick Line does not have RCA", "", pickLines.Single(l => l.WZ_Units == 1m).WZ_ReleaseCapturedPartAttrib1);

			availableInventory.PickLineQuantity = 4m;
			pickLines = pick.GetAllPickLines();

			AssertEquals("PickLineQuantity should be changed to 4m", 4m, orderedInventory.AvailableInventories[0].PickLineQuantity);
			AssertEquals("ReleaseLineQuantity should be changed to 4m", 4m, releaseLine1.Quantity);
			AssertEquals("ReleaseLine2 should be changed to 4m", 4m, releaseLine1.Quantity);
			AssertEquals("1 Pick Line remaining", 1, pickLines.Count());
			AssertEquals("This Pick Line has RCA", "RED", pickLines.Single(l => l.WZ_Units == 4m).WZ_ReleaseCapturedPartAttrib1);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);

			availableInventory.PickLineQuantity = 3m;

			AssertEquals("PickLineQuantity can not be changed anymore", 4m, orderedInventory.AvailableInventories[0].PickLineQuantity);
			AssertEquals("ReleaseLineQuantity should not be changed", 4m, releaseLine1.Quantity);
			AssertHasWarning(availableInventory.PickLineQuantityInfo, "You cannot Allocate less Stock than has been Release Captured.");
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Attribute1()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Core(AttributeNumber.One);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Attribute2()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Core(AttributeNumber.Two);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Attribute3()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Core(AttributeNumber.Three);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_SerialNumber()
		{
			TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Core(AttributeNumber.Serial);
		}

		void TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_SingleLine_Core(AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var poke = orderLine.ReleaseLines.Count; // ReleaseLines Collection manages AttributesMet

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantity = 1m;
			AssertEquals(1, orderLine.ReleaseLines.Count);
			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals(1m, releaseLine1.Quantity);

			setPartAttribute(attributeNumber, releaseLine1);
			releaseLine1.Quantity = 1m;

			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: 1 Release Line", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Precondition: 1 Pick Line", 1, pickLines.Count());
			AssertEquals("Precondition: 1 Pick Line has RCA", "RED", getPartAttribute(attributeNumber, pickLines.Single()));
			AssertEquals("Precondition: This Pick Line has units of 1m", 1m, pickLines.Single().WZ_Units);

			availableInventory.PickLineQuantity = 2m;

			pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(l => getPartAttribute(attributeNumber, l) == "RED");
			var pickLine2 = pickLines.Single(l => string.IsNullOrEmpty(getPartAttribute(attributeNumber, l)));
			var releaseLine2 = orderLine.ReleaseLines[1];
			AssertEquals("PickLineQuantity is now 2m", 2m, orderedInventory.AvailableInventories[0].PickLineQuantity);
			AssertEquals("1 Pick Line created", 2, pickLines.Count());
			AssertEquals("The RED Pick Line has 1m untouched", 1m, pickLine1.WZ_Units);
			AssertEquals("Another Pick Line has 1m", 1m, pickLine2.WZ_Units);
			AssertEquals("1 ReleaseLine created", 2, orderLine.ReleaseLines.Count);
			AssertEquals("Attrb1 of releaseLine1 is RED", "RED", getPartAttribute(attributeNumber, releaseLine1));
			AssertEquals("Attrb2 of releaseLine2 is empty", "", getPartAttribute(attributeNumber, releaseLine2));
			AssertEquals("Quantity of releaseLine1 is 1m", 1m, releaseLine1.Quantity);
			AssertEquals("Quantity of releaseLine2 is 1m", 1m, releaseLine2.Quantity);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);
		}

		public void TestPickLineQuantity_WithReleaseCapturedAttribs_Increase_MultipleLines_OneIsReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var poke = orderLine.ReleaseLines.Count; // ReleaseLines Collection manages AttributesMet

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantity = 5m;
			AssertEquals(1, orderLine.ReleaseLines.Count);
			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals(5m, releaseLine1.Quantity);

			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.Quantity = 4m;
			orderLine.ReleaseLines.AddNew().Quantity = 1m;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: 2 Pick Lines created", 2, pickLines.Count());
			AssertEquals("Precondition: 1 Pick Line has RCA", "RED", pickLines.Single(l => l.WZ_Units == 4m).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Precondition: 1 Pick Line does not have RCA", "", pickLines.Single(l => l.WZ_Units == 1m).WZ_ReleaseCapturedPartAttrib1);

			availableInventory.PickLineQuantity = 6m;

			pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED");
			var pickLine2 = pickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "");
			var releaseLine2 = orderLine.ReleaseLines[1];
			AssertEquals("PickLineQuantity is now 6m", 6m, orderedInventory.AvailableInventories[0].PickLineQuantity);
			AssertEquals("1 Pick Line created", 2, pickLines.Count());
			AssertEquals("The RED Pick Line has 4m untouched", 4m, pickLine1.WZ_Units);
			AssertEquals("Another Pick Line has 2m", 2m, pickLine2.WZ_Units);
			AssertEquals("2 ReleaseLines", 2, orderLine.ReleaseLines.Count);
			AssertEquals("Attrb1 of releaseLine1 is RED", "RED", releaseLine1.PartAttribute1);
			AssertEquals("Attrb2 of releaseLine2 is empty", "", releaseLine2.PartAttribute1);
			AssertEquals("Quantity of releaseLine1 is 4m", 4m, releaseLine1.Quantity);
			AssertEquals("Quantity of releaseLine2 is 2m", 2m, releaseLine2.Quantity);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);
		}

		#region TestPickLineQuantity_ManagesReleaseLines_IncludingReleaseCapturedAttributes

		public void TestPickLineQuantity_ManagesReleaseLines_IncludingReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var poke = orderLine.ReleaseLines.Count; // ReleaseLines Collection manages AttributesMet

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.AvailableInventories[0].PickLineQuantity = 5m;
			AssertEquals(1, orderLine.ReleaseLines.Count);
			var releaseLine1 = orderLine.ReleaseLines[0];
			AssertEquals(5m, releaseLine1.Quantity);

			releaseLine1.PartAttribute1 = "123";
			releaseLine1.Quantity = 1m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;

			orderedInventory.AvailableInventories[0].PickLineQuantity = 4m;
			AssertEquals("Changed Release Line Quantities", 1m, releaseLine1.Quantity);
			AssertEquals("Changed Release Line Quantities", 3m, releaseLine2.Quantity);

			releaseLine2.Quantity = 2m;

			orderedInventory.AvailableInventories[0].PickLineQuantity += 1m;
			AssertEquals("Changed Release Line Quantities", 1m, releaseLine1.Quantity);
			AssertEquals("Changed Release Line Quantities", 3m, releaseLine2.Quantity);

			orderedInventory.AvailableInventories[0].PickLineQuantity = 4m;
			releaseLine2.Quantity = 4m;
			AssertEquals("Changed Release Line Quantities", 1m, releaseLine1.Quantity);
			AssertEquals("Changed Release Line Quantities", 4m, releaseLine2.Quantity);

			orderedInventory.AvailableInventories[0].PickLineQuantity = 5m;
			AssertEquals("Changed Release Line Quantities", 1m, releaseLine1.Quantity);
			AssertEquals("Changed Release Line Quantities", 4m, releaseLine2.Quantity);

			releaseLine1.Quantity = 4m;
			releaseLine2.Quantity = 1m;
			orderedInventory.AvailableInventories[0].PickLineQuantity = 4m;
			AssertEquals("Precondition: Quantity on Release Captured Line is unchanged", 4m, releaseLine1.Quantity);
			AssertEquals("Precondition: UnreleasedQty on Release Captured Line is unchanged", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should have deleted the unneccessary line with no Release Captured Attributes", true, releaseLine2.IsDeleted);

			orderedInventory.AvailableInventories[0].PickLineQuantity = 5m;
			AssertEquals("Should have added a new release line.", 2, orderLine.ReleaseLines.Count);

			releaseLine2 = orderLine.ReleaseLines[1];
			releaseLine2.Quantity = 1m;
			orderedInventory.AvailableInventories[0].PickLineQuantity = 3m;
			AssertEquals("Precondition: Quantity on Release Captured Line is unchanged", 4m, releaseLine1.Quantity);
			AssertEquals("Should have deleted the unneccessary line with no Release Captured Attributes", true, releaseLine2.IsDeleted);

			AssertEquals("4m release captured, PickLineQuantity will be set to 4 again", 4m, orderedInventory.AvailableInventories[0].PickLineQuantity);
			AssertEquals("No un-release capture qty", 0m, releaseLine1.UnreleasedQty);
		}

		#endregion

		#region TestPickLineQuantity_CalculatesOrderTotals

		public void TestPickLineQuantity_CalculatesOrderTotals()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 0.5m;
			data.Part2.OP_Cubic = 1.0m;
			data.Part1.OP_Weight = 5m;
			data.Part2.OP_Weight = 10m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AvailableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AvailableInventory.PickLineQuantity = 5m;
			var poke = orderLine.ReleaseLines; // Sent values wont be calculated unless there are release lines
			AssertEquals("Units Sent should be calculated", 5m, order.WD_UnitsSent);
			AssertEquals("Weight should be calculated", 25m, order.WD_WeightSent);
			AssertEquals("Cubic should be calculated", 2.5m, order.WD_CubicSent);
			AvailableInventory.PickLineQuantity = 10m;
			AssertEquals("Units should be recalculated", 10m, order.WD_UnitsSent);
			AssertEquals("Weight should be recalculated", 50m, order.WD_WeightSent);
			AssertEquals("Cubic should be recalculated", 5m, order.WD_CubicSent);

			AvailableInventory = pick.OrderedInventories[1].AvailableInventories[0];
			AvailableInventory.PickLineQuantity = 10m;
			AssertEquals("Units Sent should be recalculated", 20m, order.WD_UnitsSent);
			AssertEquals("Weight should be recalculated", 150m, order.WD_WeightSent);
			AssertEquals("Cubic should be recalculated", 15m, order.WD_CubicSent);
			AvailableInventory.PickLineQuantity = 20m;
			AssertEquals("Units Sent should be recalculated", 30m, order.WD_UnitsSent);
			AssertEquals("Weight should be recalculated", 250m, order.WD_WeightSent);
			AssertEquals("Cubic should be recalculated", 25m, order.WD_CubicSent);

			AvailableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AvailableInventory.PickLineQuantity = 8m;
			AssertEquals("Units Sent should be recalculated", 28m, order.WD_UnitsSent);
			AssertEquals("Weight should be recalculated", 240m, order.WD_WeightSent);
			AssertEquals("Cubic should be recalculated", 24m, order.WD_CubicSent);

			// for SOP (Single Order Pick), we auto adjust the UnitsSent when the user alters the pick
			pick.IsAlterPick = true;
			AvailableInventory.PickLineQuantity = 10m;
			AssertEquals("Units Sent should be recalculated for Alter Pick with a SOP.", 30m, order.WD_UnitsSent);
			AssertEquals("Weight should be recalculated for Alter Pick with a SOP.", 250m, order.WD_WeightSent);
			AssertEquals("Cubic should be recalculated for Alter Pick with a SOP.", 25m, order.WD_CubicSent);

			// for MOP (Multi Order Pick), we do not know which Order to auto adjust, so the user must do this manually if they alter the pick
			pick.Orders.AddNew();
			AvailableInventory.PickLineQuantity = 12m;
			AssertEquals("Units Sent should *not* be recalculated for Alter Pick with a MOP.", 30m, order.WD_UnitsSent);
			AssertEquals("Weight should *not* be recalculated for Alter Pick with a MOP.", 250m, order.WD_WeightSent);
			AssertEquals("Cubic should *not* be recalculated for Alter Pick with a MOP.", 25m, order.WD_CubicSent);
		}

		#endregion

		#region TestPickLineQuantity_CalculatesExtendedLinePrice

		public void TestPickLineQuantity_CalculatesExtendedLinePrice()
		{
			TestPickLineQuantity_CalculatesExtendedLinePriceCore(releaseLinesPoked: false, isAlterPick: false);
		}

		public void TestPickLineQuantity_CalculatesExtendedLinePrice_OnAlterPick()
		{
			TestPickLineQuantity_CalculatesExtendedLinePriceCore(releaseLinesPoked: false, isAlterPick: true);
		}

		public void TestPickLineQuantity_CalculatesExtendedLinePrice_ReleaseLinesPoked()
		{
			TestPickLineQuantity_CalculatesExtendedLinePriceCore(releaseLinesPoked: true, isAlterPick: false);
		}

		public void TestPickLineQuantity_CalculatesExtendedLinePrice_ReleaseLinesPokedAndOnAlterPick()
		{
			TestPickLineQuantity_CalculatesExtendedLinePriceCore(releaseLinesPoked: true, isAlterPick: true);
		}

		void TestPickLineQuantity_CalculatesExtendedLinePriceCore(bool releaseLinesPoked, bool isAlterPick)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			order.Lines[0].WE_UnitPriceAfterDiscount = 5m;
			AssertEquals("Precondition: LinePrice used transaction quantity prior to picking.", 50m, order.Lines[0].WE_ExtendedLinePrice);

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			if (releaseLinesPoked)
			{
				_ = order.Lines[0].ReleaseLines.Count;
			}

			pick.IsAlterPick = isAlterPick;
			AssertEquals("Precondition: LinePrice.", 0m, order.Lines[0].WE_ExtendedLinePrice);
			AssertEquals("Precondition: No units picked.", 0m, availableInventory.PickLineQuantity);

			availableInventory.PickLineQuantity = 5m;
			AssertEquals("LinePrice should be updated.", 25m, order.Lines[0].WE_ExtendedLinePrice);

			availableInventory.PickLineQuantity = 10m;
			AssertEquals("LinePrice should be updated.", 50m, order.Lines[0].WE_ExtendedLinePrice);

			availableInventory.PickLineQuantity = 2m;
			AssertEquals("LinePrice should be updated.", 10m, order.Lines[0].WE_ExtendedLinePrice);
		}

		#endregion

		#region TestPickLineQuantity_DoesNotOverrideUserEnteredExtendedLinePrice

		public void TestPickLineQuantity_DoesNotOverrideUserEnteredExtendedLinePrice()
		{
			TestPickLineQuantity_DoesNotOverrideUserEnteredExtendedLinePriceCore(releaseLinesPoked: false);
		}

		public void TestPickLineQuantity_DoesNotOverrideUserEnteredExtendedLinePrice_ReleaseLinesPoked()
		{
			TestPickLineQuantity_DoesNotOverrideUserEnteredExtendedLinePriceCore(releaseLinesPoked: true);
		}

		void TestPickLineQuantity_DoesNotOverrideUserEnteredExtendedLinePriceCore(bool releaseLinesPoked)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			order.Lines[0].WE_ExtendedLinePrice = 35m;

			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			if (releaseLinesPoked)
			{
				_ = order.Lines[0].ReleaseLines.Count;
			}

			AssertEquals("Precondition: LinePrice.", 35m, order.Lines[0].WE_ExtendedLinePrice);
			AssertEquals("Precondition: No units picked.", 0m, availableInventory.PickLineQuantity);

			availableInventory.PickLineQuantity = 5m;
			AssertEquals("LinePrice should *not* be updated.", 35m, order.Lines[0].WE_ExtendedLinePrice);

			availableInventory.PickLineQuantity = 10m;
			AssertEquals("LinePrice should *not* be updated.", 35m, order.Lines[0].WE_ExtendedLinePrice);
		}

		#endregion

		#region TestPickLineQuantity_ClearsShortfallQuantityCacheOnOrderLines

		public void TestPickLineQuantity_ClearsShortfallQuantityCacheOnOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: Order Line is not in shortfall.", 0m, orderLine.WE_ShortfallQuantityCached);

			availableInventory.Allocate = false;
			AssertEquals("Order Line should be short after deallocating.", 10m, orderLine.WE_ShortfallQuantityCached);

			availableInventory.Allocate = true;
			AssertEquals("Order Line should *not* be short after allocating full amount.", 0m, orderLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestPickLineQuantity_UpdatesAllocationLog

		public void TestPickLineQuantity_UpdatesAllocationLog()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			WhsPick pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			ZString log = "10 Units manually allocated by user" + System.Environment.NewLine;
			AssertEquals(log, pick.OrderedInventories[0].AvailableInventories[0].AllocationLog);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 6m;
			log += "4 Units manually deallocated by user" + System.Environment.NewLine;
			AssertEquals(log, pick.OrderedInventories[0].AvailableInventories[0].AllocationLog);

			pick.OrderedInventories[0].AvailableInventories[0].AllocationLogEntryStack.Push("Full Pallets");
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 15m;
			log += "9 Units allocated from Full Pallets" + System.Environment.NewLine;
			AssertEquals(log, pick.OrderedInventories[0].AvailableInventories[0].AllocationLog);

			data.Part1.OP_StockKeepingUnit = "CTN";
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 12m;
			log += "3 Cartons deallocated from Full Pallets" + System.Environment.NewLine;
			AssertEquals(log, pick.OrderedInventories[0].AvailableInventories[0].AllocationLog);
		}

		#endregion

		#region TestPickLineQuantity_DoesNotDeleteReservedPickLines

		public void TestPickLineQuantity_DoesNotDeleteReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var reservedPickline = orderLine1.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 5m, reservedPickline.ReservedQuantity);

			Factory.Save();
			var pick = Helper.CreatePickNew(order);
			var pickedPickLine = orderLine2.PickLines.Single();
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickline, pickedPickLine }, availableInventory.PickLines);
			AssertEquals(10m, availableInventory.PickLineQuantity);

			reservedPickline.WZ_PickedDateTime = new ZDateTimeOffset(2015, 1, 1);
			reservedPickline.WZ_GS_NKAssignedTo = "E";
			reservedPickline.WZ_VerifiedEmpty = "E";
			availableInventory.PickLineQuantity = 0m;
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickline }, availableInventory.PickLines);
			AssertEquals("Normal picklines should be deleted when deallocating.", true, pickedPickLine.IsDeleted);
			AssertEquals("Reserved picklines should *not* be deleted when deallocating.", false, reservedPickline.IsDeleted);
			AssertEquals("Reserved picklines be cleared out when deallocating.", ZDateTimeOffset.Empty, reservedPickline.WZ_PickedDateTime);
			AssertEquals("Reserved picklines be cleared out when deallocating.", "", reservedPickline.WZ_GS_NKAssignedTo);
			AssertEquals("Reserved picklines be cleared out when deallocating.", 0m, reservedPickline.WZ_Units);
			AssertEquals("Reserved picklines be cleared out when deallocating.", "", reservedPickline.WZ_VerifiedEmpty);
		}

		#endregion

		#region TestUpdatingPickLineQuantity_AlsoClearsRelatedCrossDockAllocations

		public void TestUpdatingPickLineQuantity_AlsoClearsRelatedCrossDockAllocations()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1m, 2m, 3m, 4m, 5m);
			data.MakeInventoryMergebleForPicking(data.Line112, data.Line111);
			data.MakeInventoryMergebleForPicking(data.Line114, data.Line113);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			Helper.CreateReservePickLine(orderLine1, data.Line111, 1m);
			Helper.CreateReservePickLine(orderLine1, data.Line112, 2m);

			Helper.CreateReservePickLine(orderLine1, data.Line113, 2m);
			Helper.CreateReservePickLine(orderLine1, data.Line114, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory1 = FindAvailableInventory(orderedInventory, data.Line111);
			var availableInventory2 = FindAvailableInventory(orderedInventory, data.Line113);

			AssertEquals(0m, orderedInventory.QuantityCrossDocked);
			AssertEquals(0m, availableInventory1.QuantityCrossDocked);
			AssertEquals(0m, availableInventory2.QuantityCrossDocked);

			availableInventory1.PickLineQuantity += 1;
			AssertEquals(0m, availableInventory1.QuantityCrossDocked);
			AssertEquals(0m, orderedInventory.QuantityCrossDocked);

			availableInventory2.PickLineQuantity += 1;
			AssertEquals(0m, availableInventory2.QuantityCrossDocked);
			AssertEquals(0m, orderedInventory.QuantityCrossDocked);
		}

		#endregion

		#region TestPickLineQuantity_BugWithPickLinesAttachedToOneOrderLineAndAttributesMetToAnother

		public void TestPickLineQuantity_BugWithPickLinesAttachedToOneOrderLineAndAttributesMetToTheAnother()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);

			foreach (WhsOrderLine line in order.Lines)
			{
				AssertEquals("SumOfUnitsMet should be the same as PickLineQuantity for each OrderLine", line.SumOfUnitsMet, line.PickLineQuantity);
			}

			// Bug will only show if pick.OrderedInventories[0].AvailableInventories[0].PickLines->OrderLine.PK order will be different from OrderedInventories[0].Owners->PK order.
			pick.OrderedInventories[0].AvailableInventories[0].PickLines.OrderBy(pl => pl.WZ_WE_TransactionLine);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 2m;
			foreach (WhsOrderLine line in order.Lines)
			{
				AssertEquals("SumOfUnitsMet should be the same as PickLineQuantity for each OrderLine", line.SumOfUnitsMet, line.PickLineQuantity);
			}
		}

		#endregion

		#region TestQuanityPicked_NotCausingSplitByPackTypeIfValueWasNotChanged

		public void TestQuanityPicked_NotCausingSplitByPackTypeIfValueWasNotChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var previousUOMinventories = availableInventory.AvailableInventoriesSplitByUOM.ToArray();
			AssertEquals(2, previousUOMinventories.Length);
			var refreshListWasCalled = false;
			((IBusinessObjectCollection)availableInventory.AvailableInventoriesSplitByUOM).ListChanged += (sender, args) =>
			 {
				 refreshListWasCalled = args.ListChangedType == ListChangedType.Reset;
			 };

			// set quantity picked to the same value
			availableInventory.PickLineQuantity = 15;
			var currentUOMinventories = availableInventory.AvailableInventoriesSplitByUOM.ToArray();
			AssertEquals(2, currentUOMinventories.Length);

			// checking that list of UOM inventories was not rebuilt
			Assert(ReferenceEquals(previousUOMinventories[0], currentUOMinventories[0]));
			Assert(ReferenceEquals(previousUOMinventories[1], currentUOMinventories[1]));
			Assert(!refreshListWasCalled);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines

		#region TestPickLineQuantity_ManagesReleaseLines_SingleOrderAndLine

		public void TestPickLineQuantity_ManagesReleaseLines_SingleOrderAndLine()
		{
			TestPickLineQuantity_ManagesReleaseLines_SingleOrderAndLine(pokeReleaseLines: false);
		}

		public void TestPickLineQuantity_ManagesReleaseLines_SingleOrderAndLine_PokeReleaseLinesFirst()
		{
			TestPickLineQuantity_ManagesReleaseLines_SingleOrderAndLine(pokeReleaseLines: true);
		}

		void TestPickLineQuantity_ManagesReleaseLines_SingleOrderAndLine(bool pokeReleaseLines)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			if (pokeReleaseLines)
			{
				var poke = order.Lines[0].ReleaseLines.Count;
			}

			availableInventory.PickLineQuantity = 8m;
			AssertEquals("Should have added one release line.", 1, order.Lines[0].ReleaseLines.Count);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Release line should be fully picked with 8 units", 8m, releaseLine.Quantity);
			AssertEquals("Release line should be fully picked with 8 units", 0m, releaseLine.UnreleasedQty);
			AssertEquals("Pick Lines should be in sync.", 1, order.Lines[0].PickLines.Count);
			AssertEquals("Pick Lines should be in sync.", 8m, order.Lines[0].PickLines[0].WZ_Units);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("Should *not* have added another release line.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals(50m, releaseLine.Quantity);
			AssertEquals(0m, releaseLine.UnreleasedQty);
			AssertEquals(50m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));

			releaseLine.Quantity = 30m;
			AssertEquals("Precondition", 20m, releaseLine.UnreleasedQty);
			AssertEquals("Pick lines should be unchanged.", 50m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity = 40m;
			AssertEquals("Precondition", 40m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Quantity should be unchanged.", 30m, releaseLine.Quantity);
			AssertEquals("Should have reduced UnreleasedQty.", 10m, releaseLine.UnreleasedQty);

			availableInventory.PickLineQuantity = 10m;
			AssertEquals("Precondition", 10m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Quantity should be reduced.", 10m, releaseLine.Quantity);
			AssertEquals("Should be fully released.", 0m, releaseLine.UnreleasedQty);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("Precondition", 50m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be fully released.", 50m, releaseLine.Quantity);
			AssertEquals("Should be fully released.", 0m, releaseLine.UnreleasedQty);

			availableInventory.PickLineQuantity = 8m;
			AssertEquals("Precondition", 8m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should *not* have removed release lines.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should have reduced Quantity.", 8m, releaseLine.Quantity);
			AssertEquals("Should be fully released.", 0m, releaseLine.UnreleasedQty);

			releaseLine.Quantity = 10m;
			AssertEquals("Precondition", 10m, releaseLine.Quantity);
			AssertEquals("Precondition", -2m, releaseLine.UnreleasedQty);
			AssertEquals("Pick lines should be unchanged", 8m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity = 6m;
			AssertEquals("Precondition", 6m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should *not* have changed Quantity, as it is already over released by the user.", 10m, releaseLine.Quantity);
			AssertEquals("Should have updated UnreleasedQty.", -4m, releaseLine.UnreleasedQty);

			availableInventory.PickLineQuantity = 10m;
			AssertEquals("Precondition", 10m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should synchronise Quantity + UnreleasedQty.", 10m, releaseLine.Quantity);
			AssertEquals("Should synchronise Quantity + UnreleasedQty.", 0m, releaseLine.UnreleasedQty);

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should have deleted release line", true, releaseLine.IsDeleted);
			AssertEquals("Should have deleted release line.", 0, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should have deleted pick line.", 0, order.Lines[0].PickLines.Count);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_DeletesReleaseLinesWhenUnPicking

		public void TestPickLineQuantity_ManagesReleaseLines_DeletesReleaseLinesWhenUnPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Should have added one release line.", 1, order.Lines[0].ReleaseLines.Count);

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should have deleted release line.", 0, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should have deleted release line.", true, releaseLine.IsDeleted);

			availableInventory.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line.", 1, order.Lines[0].ReleaseLines.Count);

			releaseLine = order.Lines[0].ReleaseLines[0];
			releaseLine.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine.Quantity);
			AssertEquals("Precondition", 5m, releaseLine.UnreleasedQty);

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should have deleted release line.", 0, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should have deleted release line.", true, releaseLine.IsDeleted);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_DeletesReleaseLinesWhenUnPicking_QuantityPlusUnreleasedQtyEqualZero

		public void TestPickLineQuantity_ManagesReleaseLines_DeletesReleaseLinesWhenUnPicking_QuantityPlusUnreleasedQtyEqualZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order1.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var releaseLine1 = orderLine1.ReleaseLines[0];
			AssertEquals("Should have added one release line.", 1, orderLine1.ReleaseLines.Count);
			releaseLine1.Quantity = 5m;

			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertEquals("Should have added one release line.", 1, orderLine2.ReleaseLines.Count);
			releaseLine2.Quantity = 6m;

			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 6m, releaseLine2.Quantity);
			AssertEquals("Precondition", -1m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", -1m, releaseLine2.UnreleasedQty);

			availableInventory.PickLineQuantity = 5m;
			AssertEquals("Should still have one release line.", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Should still have one release line - up to the user to decide they want to not release this line.", 1, orderLine2.ReleaseLines.Count);

			releaseLine2.Quantity = 0m;
			AssertEquals("Should have deleted release line", true, releaseLine2.IsDeleted);
			AssertEquals("ReleaseLine1 should be unchanged", false, releaseLine1.IsDeleted);
			AssertEquals("ReleaseLine1 should be unchanged", 5m, releaseLine1.Quantity);
			AssertEquals("ReleaseLine1 should be unchanged", 0m, releaseLine1.UnreleasedQty);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_DeletesReleaseLinesWhenUnPicking_MultiOrderCase

		public void TestPickLineQuantity_ManagesReleaseLines_DeletesReleaseLinesWhenUnPicking_MultiOrderCase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			AssertEquals("Should have added one release line.", 1, order1.Lines[0].ReleaseLines.Count);
			releaseLine1.Quantity = 5m;

			var releaseLine2 = order2.Lines[0].ReleaseLines[0];
			AssertEquals("Should have added one release line.", 1, order2.Lines[0].ReleaseLines.Count);
			releaseLine2.Quantity = 5m;

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should have deleted release line.", 0, order1.Lines[0].ReleaseLines.Count);
			AssertEquals("Should have deleted release line.", 0, order2.Lines[0].ReleaseLines.Count);
			AssertEquals("Should have deleted release line.", true, releaseLine1.IsDeleted);
			AssertEquals("Should have deleted release line.", true, releaseLine2.IsDeleted);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_MultiLine

		public void TestPickLineQuantity_ManagesReleaseLines_MultiLine()
		{
			TestPickLineQuantity_ManagesReleaseLines_MultiLine(pokeReleaseLines: false);
		}

		public void TestPickLineQuantity_ManagesReleaseLines_MultiLine_PokeReleaseLinesFirst()
		{
			TestPickLineQuantity_ManagesReleaseLines_MultiLine(pokeReleaseLines: true);
		}

		void TestPickLineQuantity_ManagesReleaseLines_MultiLine(bool pokeReleaseLines)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 15m);
			order1.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order1);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			if (pokeReleaseLines)
			{
				var poke1 = order1.Lines[0].ReleaseLines.Count;
				var poke2 = order1.Lines[1].ReleaseLines.Count;
			}

			availableInventory.PickLineQuantity = 20m;
			AssertEquals("Should have added one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order1Line2.ReleaseLines.Count);

			AssertEquals("Should have populated Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			order1Line2.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition", 5m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 5m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be unchanged", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be unchanged", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity -= 10m;
			AssertEquals("Should *not* have deleted release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have deleted release line.", 0, order1Line2.ReleaseLines.Count);

			AssertEquals("Should still have 10 Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be unchanged", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be no pick lines on order line 2", 0, order1Line2.PickLines.Count);

			availableInventory.PickLineQuantity += 10m;
			AssertEquals("Should have one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have one release line.", 1, order1Line2.ReleaseLines.Count);

			AssertEquals("Should have populated Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity += 5m;
			AssertEquals("Should have readded another release.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should *not* have added another release line.", 1, order1Line2.ReleaseLines.Count);

			AssertEquals("Should have populated Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 15m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 10 units picked", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 15 units picked", 15m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity -= 5m;
			AssertEquals("Should still have one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should still have one release line.", 1, order1Line2.ReleaseLines.Count);

			AssertEquals("Should not have changed Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should not have changed Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should have deleted all release lines.", 0, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have deleted all release lines.", 0, order1Line2.ReleaseLines.Count);
			AssertEquals("Should have deleted all pick lines.", 0, order1Line2.PickLines.Count);
			AssertEquals("Should have deleted all pick lines.", 0, order1Line2.PickLines.Count);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_MultiOrderAndLine

		public void TestPickLineQuantity_ManagesReleaseLines_MultiOrderAndLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 15m);
			var order2Line = order2.Lines[0];

			order1.WD_PickOption = WhsPickOption.Codes.Manual;
			order2.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 20m;

			AssertEquals("Should have added one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals(1, order2Line.ReleaseLines.Count);

			AssertEquals("Should *not* have populated Quantity", 0m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should *not* have populated Quantity", 0m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have unreleased units", 20m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have unreleased units", 20m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have *not* populated Quantity", 0m, order2Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 20m, order2Line.ReleaseLines[0].UnreleasedQty);

			order1Line1.ReleaseLines[0].Quantity = 10m;
			order1Line2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Should have populated Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have no pick lines", 0m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity += 30m;
			AssertEquals("Should *not* have added another release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should *not* have added another release line.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order2Line.ReleaseLines.Count);

			AssertEquals("Should *not* have changed Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should *not* have changed Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should *not* have changed Quantity", 0m, order2Line.ReleaseLines[0].Quantity);

			AssertEquals("Should have unreleased units", 30m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have unreleased units", 30m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have unreleased units", 30m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have picked additional units", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have picked additional units", 15m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have picked additional units", 25m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			order1Line2.ReleaseLines[0].Quantity = 15m;
			order2Line.ReleaseLines[0].Quantity = 25m;
			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			order1Line2.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition", 5m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 5m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 5m, order2Line.ReleaseLines[0].UnreleasedQty);

			availableInventory.PickLineQuantity -= 10m;
			AssertEquals("Should still have one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should still have one release line.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals("Should still have one release line.", 1, order2Line.ReleaseLines.Count);

			AssertEquals("Should have populated Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 25m, order2Line.ReleaseLines[0].Quantity);

			AssertEquals("Should be over released", -5m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should be over released", -5m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should be over released", -5m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have picked 40 units in total", 40m, new[] { order1Line1, order1Line2, order2Line }.SelectMany(o => o.PickLines).Sum(pl => pl.WZ_Units));

			order1Line2.ReleaseLines[0].Quantity = 15m;
			AssertEquals("Should be over released", -10m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should be over released", -10m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should be over released", -10m, order2Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Picked lines should be unchanged", 40m, new[] { order1Line1, order1Line2, order2Line }.SelectMany(o => o.PickLines).Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity -= 5m;
			AssertEquals("Should still have one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should still have one release line.", 1, order1Line2.ReleaseLines.Count);
			AssertEquals("Should still have one release line.", 1, order2Line.ReleaseLines.Count);

			AssertEquals("Should not have changed Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should not have changed Quantity", 15m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should not have changed Quantity", 25m, order2Line.ReleaseLines[0].Quantity);

			AssertEquals("Should have changed unreleased quantity.", -15m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have changed unreleased quantity.", -15m, order1Line2.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have changed unreleased quantity.", -15m, order2Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have picked 35 units in total", 35m, new[] { order1Line1, order1Line2, order2Line }.SelectMany(o => o.PickLines).Sum(pl => pl.WZ_Units));

			order1Line1.ReleaseLines[0].Quantity = 5m;
			order1Line2.ReleaseLines[0].Quantity = 10m;
			order2Line.ReleaseLines[0].Quantity = 20m;

			AssertEquals("Pick Lines should be in sync.", 5m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick Lines should be in sync.", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick Lines should be in sync.", 20m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should have deleted all release lines.", 0, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have deleted all release lines.", 0, order1Line2.ReleaseLines.Count);
			AssertEquals("Should have deleted all release lines.", 0, order2Line.ReleaseLines.Count);
			AssertEquals("Should have deleted all pick lines.", 0, order1Line2.PickLines.Count);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_WithReserveLine

		public void TestPickLineQuantity_ManagesReleaseLines_WithReserveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var reservedPickLine = Helper.CreateReservePickLine(order.Lines[0], receive.Inventory[0], 10m);

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", true, reservedPickLine.IsReserveLine);
			AssertEquals("Precondition", 10m, reservedPickLine.WZ_OriginalReservedQty);
			AssertEquals("Precondition", 10m, reservedPickLine.WZ_Units);

			availableInventory.PickLineQuantity = 0m; // Cross dock orders are always automatically allocated
			AssertEquals("Precondition, and poke release lines collection", 0, order.Lines[0].ReleaseLines.Count);

			availableInventory.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line.", 1, order.Lines[0].ReleaseLines.Count);
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Quantity should be populated from pick line.", 10m, releaseLine.Quantity);
			AssertEquals("Should be fully released.", 0m, releaseLine.UnreleasedQty);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_MultipleClients

		public void TestPickLineQuantity_ManagesReleaseLines_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 10m);
			order1.WD_PickOption = WhsPickOption.Codes.Manual;
			order2.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true;
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			availableInventory1.PickLineQuantity = 8m;
			AssertEquals("Should have added one release line.", 1, order1.Lines[0].ReleaseLines.Count);

			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 8m, releaseLine1.Quantity);
			AssertEquals("Should be fully released", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be no release lines on second order line.", 0, order2.Lines[0].ReleaseLines.Count);

			availableInventory2.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line to second order line.", 1, order2.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have added another release line on line 1.", 1, order1.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 8m, releaseLine1.Quantity);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 0m, releaseLine1.UnreleasedQty);

			var releaseLine2 = order2.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			availableInventory1.PickLineQuantity = 10m;
			AssertEquals("Should *not* have modified release lines.", 1, order1.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have modified release lines.", 1, order2.Lines[0].ReleaseLines.Count);
			AssertEquals("Should be fully released", 10m, releaseLine1.Quantity);
			AssertEquals("Should be fully released", 10m, releaseLine2.Quantity);
			AssertEquals("Should be fully released", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be fully released", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_MultiplePicks

		public void TestPickLineQuantity_ManagesReleaseLines_MultiplePicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order1.WD_PickOption = WhsPickOption.Codes.Manual;
			order2.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick1.IsAlterPick = true;
			pick2.IsAlterPick = true;
			var availableInventory1 = pick1.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick2.OrderedInventories[0].AvailableInventories[0];

			availableInventory1.PickLineQuantity = 8m;
			AssertEquals("Should have added one release line.", 1, order1.Lines[0].ReleaseLines.Count);

			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 8m, releaseLine1.Quantity);
			AssertEquals("Should be fully released", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be no release lines on second order line.", 0, order2.Lines[0].ReleaseLines.Count);

			availableInventory2.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line to second order line.", 1, order2.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have added another release line on line 1.", 1, order1.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 8m, releaseLine1.Quantity);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 0m, releaseLine1.UnreleasedQty);

			var releaseLine2 = order2.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			availableInventory1.PickLineQuantity = 10m;
			AssertEquals("Should *not* have modified release lines.", 1, order1.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have modified release lines.", 1, order2.Lines[0].ReleaseLines.Count);
			AssertEquals("Should be fully released", 10m, releaseLine1.Quantity);
			AssertEquals("Should be fully released", 10m, releaseLine2.Quantity);
			AssertEquals("Should be fully released", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be fully released", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_MultipleProducts

		public void TestPickLineQuantity_ManagesReleaseLines_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];

			availableInventory1.PickLineQuantity = 8m;
			AssertEquals("Should have added one release line.", 1, order.Lines[0].ReleaseLines.Count);

			var releaseLine1 = order.Lines[0].ReleaseLines[0];
			AssertEquals(8m, releaseLine1.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be no release lines on second order line.", 0, order.Lines[1].ReleaseLines.Count);

			availableInventory2.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line to second order line.", 1, order.Lines[1].ReleaseLines.Count);
			AssertEquals("Should *not* have added another release line on line 1.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 8m, releaseLine1.Quantity);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 0m, releaseLine1.UnreleasedQty);

			var releaseLine2 = order.Lines[1].ReleaseLines[0];
			AssertEquals(10m, releaseLine2.Quantity);
			AssertEquals(0m, releaseLine2.UnreleasedQty);

			availableInventory1.PickLineQuantity = 10m;
			AssertEquals("Should *not* have modified release lines.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have modified release lines.", 1, order.Lines[1].ReleaseLines.Count);
			AssertEquals(10m, releaseLine1.Quantity);
			AssertEquals(10m, releaseLine2.Quantity);
			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestPickLineQuantity_ManagesReleaseLines_Attributes

		public void TestPickLineQuantity_ManagesReleaseLines_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "PA1-1", "PA2-1", "PA3-1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "PA1-2", "PA2-2", "PA3-2", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories[1];

			availableInventory1.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line.", 1, order.Lines[0].ReleaseLines.Count);

			var releaseLine1 = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 10m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition", 0m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be no release lines on second order line.", 0, order.Lines[1].ReleaseLines.Count);

			availableInventory1.PickLineQuantity = 5m;
			AssertEquals("Should deallocate 5 units directly from the release line.", 5m, releaseLine1.Quantity);
			AssertEquals("Should remain fully released.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should have altered pick qty", 5m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			releaseLine1.Delete(); // "Should not happen"-type case, handled anyway
			AssertEquals("Should have no release lines.", 0, order.Lines[0].ReleaseLines.Count);

			availableInventory1.PickLineQuantity = 10m;
			AssertEquals("Should have added a release line.", 1, order.Lines[0].ReleaseLines.Count);
			releaseLine1 = order.Lines[0].ReleaseLines[0];
			AssertEquals("Should only release 5m units as Quantity Picked was increased by 5.", 5m, releaseLine1.Quantity);
			AssertEquals("Should show correct unreleased quantity.", 5m, releaseLine1.UnreleasedQty);
			AssertEquals("Should remain fully picked", 10m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals(0m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));

			releaseLine1.Quantity = 10m;
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should remain fully picked", 10m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be no picked units on second order line", 0m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));

			availableInventory2.PickLineQuantity = 10m;
			AssertEquals("Should have added one release line to second order line.", 1, order.Lines[1].ReleaseLines.Count);
			AssertEquals("Should *not* have added another release line on line 1.", 1, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 10m, releaseLine1.Quantity);
			AssertEquals("Should *not* have changed Quantity on first Release Line.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be fully picked.", 10m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be fully picked.", 10m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));

			var releaseLine2 = order.Lines[1].ReleaseLines[0];
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			availableInventory1.PickLineQuantity = 0m;
			AssertEquals("Should have removed release lines.", 0, order.Lines[0].ReleaseLines.Count);
			AssertEquals("Should *not* have modified release lines.", 1, order.Lines[1].ReleaseLines.Count);
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("Should be no picked units on first order line.", 0m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be fully picked", 10m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));

			releaseLine2.Quantity = 0m;
			AssertEquals("Should have added a release line.", 1, order.Lines[0].ReleaseLines.Count);
			releaseLine1 = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 10m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 10m, releaseLine2.UnreleasedQty);
			AssertEquals("Pick Lines should be unchanged.", 0m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick Lines should be unchanged.", 10m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));

			releaseLine1.Quantity = 10m;
			availableInventory2.PickLineQuantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);

			AssertEquals("Should be fully released.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be 5 units picked.", 5m, order.Lines[0].PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should be 0 units picked.", 0m, order.Lines[1].PickLines.Sum(pl => pl.WZ_Units));
		}

		#endregion

		#endregion

		#region TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines

		#region TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_SingleOrder

		public void TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 15m);
			order1.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order1);
			pick.IsAlterPick = true;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			availableInventory.PickLineQuantity = 20m;
			AssertEquals("Should have added one release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order1Line2.ReleaseLines.Count);

			AssertEquals("Should have populated Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			order1Line2.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition", 5m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 5m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be unchanged", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be unchanged", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity -= 5m;
			AssertEquals("Should *not* have deleted release line.", 1, order1Line1.ReleaseLines.Count);
			AssertEquals("Should *not* have deleted release line.", 1, order1Line2.ReleaseLines.Count);

			AssertEquals("Should still have 10 Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 5 Quantity", 5m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 5m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			order1Line2.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition", -5m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", -5m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be unchanged", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be unchanged", 5m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity += 5m;
			AssertEquals("Should still have 10 Quantity", 10m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 10 Quantity", 10m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 10m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 10m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			order1Line1.ReleaseLines[0].Quantity = 5m;
			order1Line2.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition", 10m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 10m, order1Line2.ReleaseLines[0].UnreleasedQty);

			availableInventory.PickLineQuantity -= 10m;
			AssertEquals("Should still have 5 Quantity", 5m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 5 Quantity", 5m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 5m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 5m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			order1Line1.ReleaseLines[0].Quantity = 7m;
			order1Line2.ReleaseLines[0].Quantity = 9m;
			AssertEquals("Precondition", -6m, order1Line1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", -6m, order1Line2.ReleaseLines[0].UnreleasedQty);

			availableInventory.PickLineQuantity += 6m;
			AssertEquals("Should still have 7 Quantity", 7m, order1Line1.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line1.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 9 Quantity", 9m, order1Line2.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 7m, order1Line1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 9m, order1Line2.PickLines.Sum(pl => pl.WZ_Units));

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_MultipleOrders

		public void TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order1Line = order1.Lines[0];

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order2Line = order2.Lines[0];

			order1.WD_PickOption = WhsPickOption.Codes.Manual;
			order2.WD_PickOption = WhsPickOption.Codes.Manual;

			var pick = Helper.CreatePickNew(order1, order2);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			availableInventory.PickLineQuantity = 20m;
			pick.IsAlterPick = true;
			AssertEquals("Should have added one release line.", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Should have added one release line.", 1, order2Line.ReleaseLines.Count);

			AssertEquals("Should have populated Quantity", 10m, order1Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have populated Quantity", 10m, order2Line.ReleaseLines[0].Quantity);

			AssertEquals("Should have no unreleased units", 0m, order1Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Should have no unreleased units", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should have 2x 10 pick lines", 10m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Should have 2x 10 pick lines", 10m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			order2Line.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition", 5m, order1Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 5m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be unchanged", 10m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be unchanged", 10m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity -= 5m;
			AssertEquals("Should *not* have deleted release line.", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Should *not* have deleted release line.", 1, order2Line.ReleaseLines.Count);

			AssertEquals("Should still have 10 Quantity", 10m, order1Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 5 Quantity", 5m, order2Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 10m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 5m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			order2Line.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition", -5m, order1Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", -5m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be unchanged", 10m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be unchanged", 5m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity += 5m;
			AssertEquals("Should still have 10 Quantity", 10m, order1Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 10 Quantity", 10m, order2Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 10m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 10m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			order1Line.ReleaseLines[0].Quantity = 5m;
			order2Line.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Precondition", 10m, order1Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 10m, order2Line.ReleaseLines[0].UnreleasedQty);

			availableInventory.PickLineQuantity -= 10m;
			AssertEquals("Should still have 5 Quantity", 5m, order1Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 5 Quantity", 5m, order2Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 5m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 5m, order2Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertNoExceptionThrown(Factory.Save);

			order1Line.ReleaseLines[0].Quantity = 7m;
			order2Line.ReleaseLines[0].Quantity = 9m;
			AssertEquals("Precondition", -6m, order1Line.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", -6m, order2Line.ReleaseLines[0].UnreleasedQty);

			availableInventory.PickLineQuantity += 6m;
			AssertEquals("Should still have 7 Quantity", 7m, order1Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order1Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Should still have 9 Quantity", 9m, order2Line.ReleaseLines[0].Quantity);
			AssertEquals("Should have no unreleased units", 0m, order2Line.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick lines should be in sync with release lines", 7m, order1Line.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Pick lines should be in sync with release lines", 9m, order2Line.PickLines.Sum(pl => pl.WZ_Units));

			availableInventory.PickLineQuantity = 7m;
			AssertNoExceptionThrown(() => availableInventory.PickLineQuantity = 16m); // "Collection was modified; enumeration operation may not execute"
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_PickByBOM

		public void TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 5m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 5m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - *NOT YET* Picked By BOM", false, kitOrderLine2.ChildComponentLines.Count > 0);

			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 5m, kitOrderLine1.ReleaseLines[0].Quantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 1m;
			kitOrderLine2.ReleaseLines[0].Quantity = 4m;

			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var frameComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			AssertEquals("Precondition", 10m, wheelComponentInv.PickLineQuantity);
			AssertEquals("Precondition", 5m, frameComponentInv.PickLineQuantity);
			AssertEquals("Precondition - Pick Lines should be in sync", 2m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Precondition - Pick Lines should be in sync", 1m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);
			AssertEquals("Precondition - Pick Lines should be in sync", 8m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Precondition - Pick Lines should be in sync", 4m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Single(pl => pl.Product.Parent == frame).WZ_Units);

			frameComponentInv.PickLineQuantity = 4m;
			AssertEquals("Should not be allowed to modify allocation for pick by BOM", 5m, frameComponentInv.PickLineQuantity);
			// NOTE: Changing allocations for Pick By BOM DOES work with release lines/pick line syncing.
			// Pick By BOM just doesnt properly handle multiple pick lines across multiple order lines
			// This data shape couldnt happen before but it can now.
		}

		#endregion

		#region TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_PickByBOM_HandlesOriginalPickedInventory

		public void TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_PickByBOM_HandlesOriginalPickedInventory()
		{
			var staff = Helper.CreateGlbStaff("BRS", "BRS");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 12m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 6m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 5m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 5m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 1, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 5m, kitOrderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition.", 1m, kitOrderLine2.ReleaseLines[0].Quantity);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_GS_NKAssignedTo = "BRS";
			}

			foreach (var pickLine in kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).ToArray())
			{
				var inTransitInv = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				var newPickLine = Factory.LoadTop1<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inTransitInv.PK));

				// Hack set WZ_WE_InventoryLine on other kit so they match
				kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Cast<WhsPickLine>().Single(pl => pl.SupplierPart.PK == pickLine.SupplierPart.PK).WZ_WE_InventoryLine = inTransitInv.PK;
			}

			kitOrderLine1.ReleaseLines[0].Quantity--;
			kitOrderLine2.ReleaseLines[0].Quantity++;
			AssertEquals("Precondition - Pick Lines should be in sync", 8m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Cast<WhsPickLine>().Single(pl => pl.Product.Parent == wheel).WZ_Units);
			AssertEquals("Precondition - Pick Lines should be in sync", 4m, kitOrderLine1.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Cast<WhsPickLine>().Single(pl => pl.Product.Parent == frame).WZ_Units);
			AssertEquals("Precondition - Pick Lines should be in sync", 4m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Cast<WhsPickLine>().Where(pl => pl.Product.Parent == wheel).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition - Pick Lines should be in sync", 2m, kitOrderLine2.ChildComponentLines.SelectMany(ccl => ccl.PickLines).Cast<WhsPickLine>().Where(pl => pl.Product.Parent == frame).Sum(pl => pl.WZ_Units));

			var kitOrderLine2ChildComponentLines = kitOrderLine2.ChildComponentLines.ToArray();
			AssertEquals("Should *not* have merged the pick lines as they have different original picked inventory.", 2, kitOrderLine2ChildComponentLines[0].PickLines.Count);
			AssertEquals("Should *not* have merged the pick lines as they have different original picked inventory.", 2, kitOrderLine2ChildComponentLines[1].PickLines.Count);
		}

		#endregion

		#region TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_PickLinesOffAnotherOrderLinesOrderedInventory

		public void TestPickLineQuantity_ReconcilesPickLinesAndReleaseLines_PickLinesOffAnotherOrderLinesOrderedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "A", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "A", "", "", "", "");
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.OrderedInventories.Count);

			var availInvA = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().First(ordInv => ordInv.PartAttrib1 == "A").AvailableInventories[0];
			availInvA.PickLineQuantity = 5m;
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 5m, orderLine2.ReleaseLines[0].Quantity);

			orderLine2.ReleaseLines[0].Quantity = 1m;
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0m, orderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition", 4m, orderLine1.ReleaseLines[0].UnreleasedQty);

			AssertNoExceptionThrown(() => orderLine1.ReleaseLines[0].Quantity = 4m);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			AssertEquals("Precondition", 4m, orderLine1.ReleaseLines[0].Quantity);
			AssertEquals("Precondition", 1m, orderLine2.ReleaseLines[0].Quantity);
			AssertEquals("Precondition", 0m, orderLine1.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Precondition", 0m, orderLine2.ReleaseLines[0].UnreleasedQty);

			AssertEquals("Pick line split and moved", 1, orderLine1.PickLines.Count);
			AssertEquals("Pick line split and moved", 1, orderLine2.PickLines.Count);
			AssertEquals("Pick line split and moved", 4m, orderLine1.PickLines[0].WZ_Units);
			AssertEquals("Pick line split and moved", 1m, orderLine2.PickLines[0].WZ_Units);
		}

		#endregion

		#endregion

		#region TestPickLineQuantity_WhenCartonising

		public void TestPickLineQuantity_WhenCartonising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 10m, availInv.PickLineQuantity);

			availInv.PickLineQuantity = 1m;
			AssertEquals("Precondition", 1m, availInv.PickLineQuantity);

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			cartonisationMutex.Lock();

			using (cartonisationMutex)
			{
				AssertEquals("Precondition", true, pick.IsCartonising);
				availInv.PickLineQuantity = 10m;
				AssertEquals("Should not have changed, as allocations changing during cartonisation will cause concurrency issues.", 1m, availInv.PickLineQuantity);

				availInv.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Now;
				availInv.PickLineQuantity = 10m;
				AssertEquals("Should not have changed, as allocations changing during cartonisation will cause concurrency issues.", 1m, availInv.PickLineQuantity);
			}

			availInv.PickLineQuantity = 10m;
			AssertEquals("Should have changed.", 10m, availInv.PickLineQuantity);

			availInv.PickLineQuantity = 0m;
			AssertEquals("Precondition", 0m, availInv.PickLineQuantity);

			pick.WP_IsCartonised = true;
			availInv.PickLineQuantity = 10m;
			AssertEquals("Should have changed, to allow the user to short pick lines and reallocate to shorted lines.", 10m, availInv.PickLineQuantity);
		}

		#endregion

		#region TestPickLineQuantity_WhenCartonising_DuringAutoAllocateItems

		public void TestPickLineQuantity_WhenCartonising_DuringAutoAllocateItems()
		{
			// We block allocation during cartonisation to show a nice message to a user manually tweaking allocations.
			// During auto allocate (especially with the rules engine), there is no message and race conditions involved.
			// This means it is best to allow the allocation to proceed and have the higher level concurrency checks OnFactorySaving block the save.
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 10m, availInv.PickLineQuantity);

			availInv.Allocate = false;
			AssertEquals("Precondition", 0m, availInv.PickLineQuantity);

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			cartonisationMutex.Lock();

			using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
			using (cartonisationMutex)
			{
				AssertEquals("Precondition", true, pick.IsCartonising);
				availInv.PickLineQuantity = 4m;
				AssertEquals("Should have allocated as the user did not manually allocate.", 4m, availInv.PickLineQuantity);
			}
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesArePacked

		#region TestPickLineQuantity_WhenPickLinesArePacked_SingleReleaseLine_SingleOrder

		public void TestPickLineQuantity_WhenPickLinesArePacked_SingleReleaseLine_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine.Quantity);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 10m);
			AssertEquals("Precondition: Release Line is Packed.", true, releaseLine.IsPacked);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.Allocate = false;
			AssertEquals("Should revert de-allocation since pick line is Packed.", true, availableInventory.Allocate);
			AssertEquals("Should revert de-allocation since pick line is Packed.", 10m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
		}

		#endregion

		#region TestPickLineQuantity_WhenPackableItemParentsAreLoaded_SingleReleaseLine_SingleOrder

		public void TestPickLineQuantity_WhenPackableItemParentsAreLoaded_SingleReleaseLine_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine.Quantity);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.Allocate = false;
			AssertEquals("PickLines have been deleted.", 0, order.PackableItemParents.Count);
			AssertEquals("PickLines have been deleted.", 0, order.PackageJob.PackableItemParents.Count);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesArePacked_SingleReleaseLine_SingleOrder_ReducePickLineQuantity

		public void TestPickLineQuantity_WhenPickLinesArePacked_SingleReleaseLine_SingleOrder_ReducePickLineQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine.Quantity);

			var package = order.PackageJob.Packages.AddNew();
			var packedItem1 = package.Pack(releaseLine, 10m).Single();
			AssertEquals("Precondition: Release Line is Packed.", true, releaseLine.IsPacked);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 5m;
			AssertEquals("Should revert de-allocation since pick line is Packed.", 10m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			packedItem1.Delete();
			var packedItem2 = package.Pack(releaseLine, 6m).Single();
			availableInventory.PickLineQuantity = 5m;
			AssertEquals("Should revert de-allocation to Packed Qty since pick line is Packed.", 6m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertNotNull("Packed Item is still correct.", packedItem2.PackedItems.Single());
			AssertEquals("Packed Item is still correct.", 6m, packedItem2.PackedItems.Single().Quantity);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesArePacked_ReleaseCapture_SingleOrder

		public void TestPickLineQuantity_WhenPickLinesArePacked_ReleaseCapture_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine.Quantity);

			releaseLine.Quantity = 8m;
			var redReleaseLine = order.Lines[0].ReleaseLines.AddNew();
			redReleaseLine.PartAttribute1 = "RED";
			redReleaseLine.Quantity = 2m;

			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: 2 Pick Lines are created, one contains Attrib1 as RED and one not", 2, pickLines.Count());
			AssertEquals("Precondition: RED one has 2 units", 2m, pickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").WZ_Units);
			AssertEquals("Precondition: empty one has 8 units", 8m, pickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "").WZ_Units);

			var package = order.PackageJob.Packages.AddNew();
			var packedItem = package.Pack(releaseLine, 8m).Single();
			AssertEquals("Precondition: Release Line is Packed.", true, releaseLine.IsPacked);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine, redReleaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine, redReleaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.Allocate = false;
			AssertEquals("Should revert de-allocation since pick line is Packed.", true, availableInventory.Allocate);
			AssertEquals("Should revert de-allocation since pick line is Packed.", 8m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine, redReleaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine, redReleaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			packedItem.Delete();
			package.Pack(redReleaseLine, 2m);
			availableInventory.Allocate = false;
			AssertEquals("Deallocation should success.", false, availableInventory.Allocate);
			AssertEquals("All PickLines deallocated.", 0m, availableInventory.PickLineQuantity);
			AssertCollectionNotContains(new[] { redReleaseLine }, order.PackableItemParents.Typed);
			AssertCollectionNotContains(new[] { redReleaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
		}

		#endregion

		#region TestPickLineQuantity_WhenPackableItemParentsAreLoaded_ReleaseCapture_SingleOrder

		public void TestPickLineQuantity_WhenPackableItemParentsAreLoaded_ReleaseCapture_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine.Quantity);

			releaseLine.Quantity = 8m;
			var redReleaseLine = order.Lines[0].ReleaseLines.AddNew();
			redReleaseLine.PartAttribute1 = "RED";
			redReleaseLine.Quantity = 2m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine, redReleaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine, redReleaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.Allocate = false;

			AssertEquals("Can deallocate.", false, availableInventory.Allocate);
			AssertEquals("PickLineQuantity of availableInventory has been changed to 0.", 0m, availableInventory.PickLineQuantity);
			AssertEquals("The RED PickLine can be deleted.", 0, order.PackableItemParents.Count);
			AssertEquals("0 ReleaseLine remains.", 0, order.PackageJob.PackableItemParents.Count);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesArePacked_MultiOrder

		public void TestPickLineQuantity_WhenPickLinesArePacked_MultiOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true; // Emulate being on the Release Form

			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			var releaseLine2 = order2.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine2.Quantity);

			var package = order1.PackageJob.Packages.AddNew();
			package.Pack(releaseLine1, 10m);
			AssertEquals("Precondition: Release Line is Packed.", true, releaseLine1.IsPacked);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order1.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine2 }, order2.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order1.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine2 },
				order2.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m;
			order2.PackageJob.Packages.AddNew().Pack(releaseLine2, 10m);
			AssertEquals("Should not allow Packing.", false, releaseLine2.IsPacked);

			// test various operations the user can attempt and make sure nothing breaks.
			releaseLine2.Quantity = 8m;
			var releaseCaptured = order2.Lines[0].ReleaseLines.AddNew("RED", "", "", "", ZDate.Empty, ZDate.Empty, 1m);

			var nonCommitted = (WhsReleaseLine)((IBindingList)order2.Lines[0].ReleaseLines).AddNew();
			nonCommitted.PartAttribute1 = "BLUE";
			nonCommitted.Quantity = 1m;
			((ICancelAddNew)order2.Lines[0].ReleaseLines).EndNew(order2.Lines[0].ReleaseLines.Count - 1);

			releaseCaptured.Delete();
			nonCommitted.Delete();
			releaseLine2.Quantity = -2m;
			AssertEquals("PickLines have been deleted.", 0, order2.PackableItemParents.Count);
			AssertEquals("PickLines have been deleted.", 0, order2.PackageJob.PackableItemParents.Count);

			releaseLine2.Quantity = 10m;
			AssertEquals("PickLines have been deleted.", 0, order2.PackableItemParents.Count);
			AssertEquals("PickLines have been deleted.", 0, order2.PackageJob.PackableItemParents.Count);

			availableInventory.Allocate = false;
			AssertEquals("Should revert de-allocation since pick line is Packed.", true, availableInventory.Allocate);
			AssertEquals("Should revert de-allocation since pick line is Packed.", 10m, availableInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order1.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order1.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertEquals("PickLines have been deleted.", 0, order2.PackableItemParents.Count);
			AssertEquals("PickLines have been deleted.", 0, order2.PackageJob.PackableItemParents.Count);
		}

		#endregion

		#region TestPickLineQuantity_WhenPackableItemParentsAreLoaded_MultiOrder

		public void TestPickLineQuantity_WhenPackableItemParentsAreLoaded_MultiOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true; // Emulate being on the Release Form

			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			var releaseLine2 = order2.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine2.Quantity);

			releaseLine2.Quantity = 8m;
			var releaseCaptured = order2.Lines[0].ReleaseLines.AddNew();
			releaseCaptured.PartAttribute1 = "RED";
			releaseCaptured.Quantity = 2m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order1.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine2, releaseCaptured }, order2.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order1.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine2, releaseCaptured },
				order2.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m; // Reduce Qty to 10 to make it insufficient for packing
			order2.PackageJob.Packages.AddNew().Pack(releaseLine2, 8m);
			order2.PackageJob.Packages.AddNew().Pack(releaseCaptured, 2m);

			AssertEquals("Should not allow Packing.", false, releaseLine2.IsPacked);
			AssertEquals("releaseCaptured is not packed.", false, releaseCaptured.IsPacked);

			availableInventory.Allocate = false;

			AssertEquals("Can deallocate all.", false, availableInventory.Allocate);
			AssertEquals("Can deallocate all.", 0m, availableInventory.PickLineQuantity);
			AssertEquals("PickLines of order1 have been deleted.", 0, order1.Lines[0].PickLines.Count);
			AssertEquals("The RED PickLine is deleted.", 0, order2.Lines[0].PickLines.Count);
		}

		#endregion

		#endregion

		#region TestPickLineQuantity_WhenPickLinesArePicked_DecreaseQty

		public void TestPickLineQuantity_WhenPickLinesArePicked_DecreaseQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			var orderLine = order.Lines[0];
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: 10 Units allocated.", 10m, pickLine.WZ_Units);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			availableInventory.PickLineQuantity -= 4m; // decrease quantity Picked to 6.
			AssertEquals("Deallocation should be successful.", 6m, availableInventory.PickLineQuantity);
			AssertEquals("PickLine should be unPicked.", false, pickLine.IsPicked);
			AssertEquals("Pickline should now have reduced Allocated Quantity to 6.", 6m, pickLine.WZ_Units);
			AssertEquals("Inventory should have full Total Units.", 10m, pickLine.InventoryLine.WE_StockOnHand);

			var pickedDateTime = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			pickLine.WZ_PickedDateTime = pickedDateTime;

			// testing that you can't deallocate PickLines with a Pick Time
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			AssertNoExceptionThrown(() => availableInventory.PickLineQuantity -= 1m);

			AssertEquals("PickLineQuantity should not be reduced.", 6m, availableInventory.PickLineQuantity);
			AssertEquals("Reduce PickLineQuantity should not clear PickedDateTime.", pickedDateTime, pickLine.WZ_PickedDateTime);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickIsPartiallyPicked_MultiOrderLines_DecreaseQty

		public void TestPickLineQuantity_WhenPickIsPartiallyPicked_MultiOrderLines_DecreaseQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			AssertEquals("Precondition: 10 Units allocated.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Precondition: 10 Units allocated.", 10m, orderedInventory.PickLineQuantity);

			availableInventory.PickLineQuantity = 5m;
			availableInventory.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			AssertEquals(2, availableInventory.PickLines.Count());

			var pickedPickLine = availableInventory.PickLines.Single(p => p.IsPickedFromPutawayLocation);
			var unpickedPickLine = availableInventory.PickLines.Single(p => !p.IsPickedFromPutawayLocation);
			AssertEquals(5m, pickedPickLine.WZ_Units);
			AssertEquals(5m, unpickedPickLine.WZ_Units);

			AssertEquals(true, orderLine1.IsPartiallyOrFullyPickedFromPutawayLocation);
			AssertEquals(false, orderLine2.IsPartiallyOrFullyPickedFromPutawayLocation);

			orderLine2.WE_TransactionQuantity = 3m;
			order.IsSavedFromOrderForm = true;

			AssertEquals("5 units should be Picked", 5m, pickedPickLine.WZ_Units);
			AssertEquals("Change quanity in Order Line should not change quantity in Unpicked PickLine.", 5m, unpickedPickLine.WZ_Units);
			AssertEquals("Change quanity in Order Line should not change quantity in AvailableInventory.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Change quanity in Order Line should not change quantity in OrderedInventory.", 10m, orderedInventory.PickLineQuantity);

			AssertNoExceptionThrown(() => order.RunPreSaveValidation());

			orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			pickedPickLine = availableInventory.PickLines.Single(p => p.IsPickedFromPutawayLocation);
			unpickedPickLine = availableInventory.PickLines.Single(p => !p.IsPickedFromPutawayLocation);

			AssertEquals("5 units should be Picked.", 5m, pickedPickLine.WZ_Units);
			AssertEquals("Quantity in Unpicked PickLine should be updated.", 3m, unpickedPickLine.WZ_Units);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesArePicked_IncreaseQty

		public void TestPickLineQuantity_WhenPickLinesArePicked_IncreaseQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 6m;

			var orderLine = order.Lines[0];
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: 6 Units allocated.", 6m, pickLine.WZ_Units);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			availableInventory.PickLineQuantity += 4m; // increase quantity Picked to 10.
			AssertEquals("Allocation should be successful.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Should have created a new Pick Line as the existing one is Picked.", 2, orderLine.PickLines.Count);
			AssertEquals("PickLine is still Picked.", true, pickLine.IsPicked);
			AssertEquals("Still have 6 Units Picked.", 6m, pickLine.WZ_Units);

			var newPickLine = orderLine.PickLines.Single(pl => pl != pickLine);
			AssertEquals("New PickLine should not be Picked.", false, newPickLine.IsPicked);
			AssertEquals("New PickLine should have allocated 4 Units.", 4m, newPickLine.WZ_Units);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesAreInTransit_DecreaseQty

		public void TestPickLineQuantity_WhenPickLinesAreInTransit_DecreaseQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: 10 Units allocated.", 10m, pickLine.WZ_Units);

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity -= 4m; // decrease quantity Picked to 6.
			AssertEquals("PickLineQuantity should not be reduced.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Pickline should not have reduced Allocated Quantity.", 10m, orderLine.PickLines.Single().WZ_Units);
		}

		#endregion

		#region TestPickLineQuantity_WhenPickLinesAreInTransit_IncreaseQty

		public void TestPickLineQuantity_WhenPickLinesAreInTransit_IncreaseQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 6m;

			var orderLine = order.Lines[0];
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: 6 Units allocated.", 6m, pickLine.WZ_Units);

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var inTransitPickLine = orderLine.PickLines.Single();
			availableInventory.PickLineQuantity += 4m; // increase quantity Picked to 10.
			AssertEquals("Allocation should be successful.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Should have created a new Pick Line as the existing one is In-Transit.", 2, orderLine.PickLines.Count);
			AssertEquals("Still have 6 Units Picked.", 6m, inTransitPickLine.WZ_Units);

			var newPickLine = orderLine.PickLines.Single(pl => pl != inTransitPickLine);
			AssertEquals("New PickLine should not be Picked.", ZGuid.Empty, newPickLine.WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("New PickLine should have allocated 4 Units.", 4m, newPickLine.WZ_Units);
		}

		#endregion

		#region AssertAllocatedItem

		void AssertAllocatedItem(WhsPickOrderedInventory orderedInventory, WhsPickAvailableInventory availableInventory, ZDecimal expectedAllocatedQty, ZDecimal expectedShortQty, bool expectedAllocateFlag)
		{
			AssertEquals("OrderedInventory Quantity Picked incorrect", expectedAllocatedQty, orderedInventory.PickLineQuantity);
			AssertEquals("OrderedInventory Quantity Short incorrect", expectedShortQty, orderedInventory.QuantityShort);

			AssertEquals("AvailableInventory Quantity Allocated incorrect", expectedAllocatedQty, availableInventory.PickLineQuantity);
			AssertEquals("AvailableInventory Allocate flag incorrect", expectedAllocateFlag, availableInventory.Allocate);
		}

		#endregion

		#region AssertQuantities

		void AssertQuantities(ZDecimal newPickLineQuantity, ZDecimal expectedPickLineQuantity)
		{
			AvailableInventory.PickLineQuantity = newPickLineQuantity;

			ZDecimal quantityOrdered = AvailableInventory.OrderedInventory.Owners[0].WE_TransactionQuantity;
			ZDecimal quantityShort = Math.Max(0m, quantityOrdered - expectedPickLineQuantity);

			AssertEquals("QuantityOrdered incorrect", quantityOrdered, AvailableInventory.OrderedInventory.QuantityOrdered);
			AssertEquals("QuantityItemPicked incorrect", expectedPickLineQuantity, AvailableInventory.OrderedInventory.PickLineQuantity);
			AssertEquals("QuantityShort incorrect", quantityShort, AvailableInventory.OrderedInventory.QuantityShort);
			AssertEquals("QuantityInventoryPicked incorrect", expectedPickLineQuantity, AvailableInventory.PickLineQuantity);
			AssertEquals("SumOfUnitsMet incorrect", expectedPickLineQuantity, AvailableInventory.OrderedInventory.Owners[0].SumOfUnitsMet);
		}

		#endregion

		#endregion

		#region QuantityUnPicked

		[TestDate(2014, 1, 1)]
		public void TestQuantityUnPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 6m;
			AssertEquals("availableInventory.QuantityUnPicked", 9m, availableInventory.QuantityUnPicked);

			availableInventory.PickLineQuantity = 11m;
			AssertEquals("availableInventory.QuantityUnPicked", 4m, availableInventory.QuantityUnPicked);

			availableInventory.PickLineQuantity = 2m;
			AssertEquals("availableInventory.QuantityUnPicked", 13m, availableInventory.QuantityUnPicked);
		}

		#endregion

		#region TestAvailableForAllocationAlgorithm

		#region TestAvailableForAllocationAlgorithm

		public void TestAvailableForAllocationAlgorithm()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);
		}

		#endregion

		#region TestAvailableForAllocationAlgorithm_HeldStock

		public void TestAvailableForAllocationAlgorithm_HeldStock() => TestAvailableForAllocationAlgorithm_HeldStock(customHoldCode: false);

		public void TestAvailableForAllocationAlgorithm_HeldStock_CustomHoldCode() => TestAvailableForAllocationAlgorithm_HeldStock(customHoldCode: true);

		void TestAvailableForAllocationAlgorithm_HeldStock(bool customHoldCode)
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m);

			if (customHoldCode)
			{
				Helper.CreateInventoryHeldCode("LOL", "CANT PICK IT");
			}

			Factory.Save();

			var receive1Line = receive1.Lines[0];
			receive1Line.HeldCodeChangeQuantity = receive1Line.WE_TransactionQuantity;
			receive1Line.HeldCodeToChangeTo = customHoldCode ? "LOL" : InventoryHoldCodes.Codes.Held;
			AssertEquals(true, receive1Line.ChangeInventoryHeldCode(true));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);
		}

		#endregion

		#region TestAvailableForAllocationAlgorithm_LocationStatus

		public void TestAvailableForAllocationAlgorithm_LocationStatus_Void() => TestAvailableForAllocationAlgorithm_LocationStatus(locationStatus: LocationStatus.Codes.Void);

		public void TestAvailableForAllocationAlgorithm_LocationStatus_Held() => TestAvailableForAllocationAlgorithm_LocationStatus(locationStatus: LocationStatus.Codes.Held);

		public void TestAvailableForAllocationAlgorithm_LocationStatus_Damaged() => TestAvailableForAllocationAlgorithm_LocationStatus(locationStatus: LocationStatus.Codes.Damaged);

		void TestAvailableForAllocationAlgorithm_LocationStatus(string locationStatus)
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m, location2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);

			location1.WLV_LocationStatus = locationStatus;
			AssertEquals(false, availableInventory.AvailableForAllocationAlgorithm);
		}

		#endregion

		#region TestAvailableForAllocationAlgorithm_ExpiryDate

		[TestDate(2021, 03, 08)]
		public void TestAvailableForAllocationAlgorithm_ExpiryDate()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, today.AddDays(1), ZDate.Empty, "", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);

			TestDateAttribute.Date = today.AddDays(1).ToDateTime();
			AssertEquals(false, availableInventory.AvailableForAllocationAlgorithm);

			TestDateAttribute.Date = today.AddDays(2).ToDateTime();
			AssertEquals(false, availableInventory.AvailableForAllocationAlgorithm);
		}

		#endregion

		#region TestAvailableForAllocationAlgorithm_MinimumShelfLife

		[TestDate(2021, 03, 08)]
		public void TestAvailableForAllocationAlgorithm_MinimumShelfLife()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			data.Org1.MiscServ.OM_MinimumShelfLifeAccepted = 5;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, today.AddDays(10), ZDate.Empty, "", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);

			TestDateAttribute.Date = today.AddDays(4).ToDateTime();
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);

			TestDateAttribute.Date = today.AddDays(5).ToDateTime();
			AssertEquals(true, availableInventory.AvailableForAllocationAlgorithm);

			TestDateAttribute.Date = today.AddDays(6).ToDateTime();
			AssertEquals(false, availableInventory.AvailableForAllocationAlgorithm);

			TestDateAttribute.Date = today.AddDays(50).ToDateTime();
			AssertEquals(false, availableInventory.AvailableForAllocationAlgorithm);
		}

		#endregion

		#region TestAvailableForAllocationAlgorithm_PickByBOM

		public void TestAvailableForAllocationAlgorithm_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
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
				wheelComponentInv.PickLineQuantity = 0m;
				AssertEquals(true, wheelComponentInv.AvailableForAllocationAlgorithm);

				using (new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore))
				{
					AssertEquals(true, wheelComponentInv.AvailableForAllocationAlgorithm);
				}

				AssertEquals(true, wheelComponentInv.AvailableForAllocationAlgorithm);
			}

			AssertEquals(false, wheelComponentInv.AvailableForAllocationAlgorithm);
		}

		#endregion

		#endregion

		#region TestQuantityThatCouldNotBeAllocatedOrDeallocated

		public void TestQuantityThatCouldNotBeAllocatedOrDeallocated()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 80m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			ZDecimal? quantityThatCouldNotBeAllocatedOrDeallocated = null;
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantityInfo.ValueChanged += delegate
				{ quantityThatCouldNotBeAllocatedOrDeallocated = availableInventory.QuantityThatCouldNotBeAllocatedOrDeallocated; };

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("We should have no over-allocated units.", 0m, quantityThatCouldNotBeAllocatedOrDeallocated);
			quantityThatCouldNotBeAllocatedOrDeallocated = null; // clean-up

			availableInventory.PickLineQuantity = 90m;
			AssertEquals("We should have 10 over-allocated units.", 10m, quantityThatCouldNotBeAllocatedOrDeallocated);
			quantityThatCouldNotBeAllocatedOrDeallocated = null; // clean-up

			orderLine1.WE_TransactionQuantity = 120m; // only 100 in stock
			availableInventory.PickLineQuantity = 120m;
			AssertEquals("We should have 20 over-allocated units.", 20m, quantityThatCouldNotBeAllocatedOrDeallocated);
		}

		#endregion

		#region TestUpdateAllocationLog_NullOrEmptyLogText_Throws

		public void TestUpdateAllocationLog_NullOrEmptyLogText_Throws()
		{
			var inv = new WhsPickAvailableInventory(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => ((IWhsPickAvailableInventoryInternals)inv).UpdateAllocationLog(null, 123m));
			AssertExceptionThrown<ArgumentException>(() => ((IWhsPickAvailableInventoryInternals)inv).UpdateAllocationLog("", 123m));
		}

		#endregion

		#region TestUpdateAllocationLog_ZeroDelta_Throws

		public void TestUpdateAllocationLog_ZeroDelta_Throws()
		{
			var inv = new WhsPickAvailableInventory(Factory);
			AssertExceptionThrown<ArgumentException>(() => ((IWhsPickAvailableInventoryInternals)inv).UpdateAllocationLog("TEST", 0m));
		}

		#endregion

		#region TestUpdateAllocationLog_ProductIsNull

		public void TestUpdateAllocationLog_ProductIsNull()
		{
			var inv = new WhsPickAvailableInventory(Factory);
			var orderedInv = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInv).SetAllProperties(Factory.New<WhsPick>(), null);
			((IWhsPickAvailableInventoryInternals)inv).SetAllProperties(orderedInv, Factory.New<WhsInventoryView>());
			AssertNoExceptionThrown(() => inv.AllocateAndValidateOrderedInventory(543213m));
			AssertNoExceptionThrown(() => ((IWhsPickAvailableInventoryInternals)inv).UpdateAllocationLog("TEST", 543213m));
		}

		#endregion

		#region TestUpdateAllocationLog_PositiveValue

		public void TestUpdateAllocationLog_PositiveValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			((IWhsPickAvailableInventoryInternals)availableInventory).UpdateAllocationLog("some process", 5m);
			AssertEquals("5 Units allocated from some process" + System.Environment.NewLine, availableInventory.AllocationLog);

			// Test popped name off stack
			availableInventory.AllocationLog = string.Empty;
			availableInventory.Allocate = true;
			AssertEquals("10 Units manually allocated by user" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		#endregion

		#region TestUpdateAllocationLog_NegativeValue

		public void TestUpdateAllocationLog_NegativeValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			((IWhsPickAvailableInventoryInternals)availableInventory).UpdateAllocationLog("some process", -5m);
			AssertEquals("5 Units deallocated from some process" + System.Environment.NewLine, availableInventory.AllocationLog);

			// Test popped name off stack
			availableInventory.AllocationLog = string.Empty;
			availableInventory.Allocate = true;
			AssertEquals("10 Units manually allocated by user" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		#endregion

		#region TestUpdateAllocationLog_Suspended

		public void TestUpdateAllocationLog_Suspended()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			using (pick.SuspendUpdatingAllocationLog())
			{
				((IWhsPickAvailableInventoryInternals)availableInventory).UpdateAllocationLog("some process", 5m);
				AssertEquals("", availableInventory.AllocationLog);

				((IWhsPickAvailableInventoryInternals)availableInventory).UpdateAllocationLog("some process", -5m);
				AssertEquals("", availableInventory.AllocationLog);
			}
		}

		#endregion

		#region Quantity CrossDocked

		public void TestQuantityCrossDocked()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1m, 2m, 3m, 4m, 5m);
			data.MakeSimpleInventoryLinesMergebleForPicking();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var reservedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var reservedOrderLine = Helper.CreateWhsOrderLine(reservedOrder, data.Part1, 15m);
			Helper.CreateReservePickLine(reservedOrderLine, data.Line111, 1m);
			Helper.CreateReservePickLine(reservedOrderLine, data.Line112, 2m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(3m, availableInventory.QuantityCrossDocked);
		}

		public void TestQuantityCrossDockedInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.QuantityCrossDocked, AvailableInventory.QuantityCrossDockedInfo.Name);
			AssertEquals(true, AvailableInventory.QuantityCrossDockedInfo.ReadOnly);
		}

		#endregion

		#region PackageGroupId

		public void TestAvailableInventory_PackageGroupId()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.SetInventoryAttributes(inventory, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "BEK-1", "SERN");
			Helper.SetInventoryCustomAttributes(inventory, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), true, true, true, true, true, "TEXTBLOB1");
			inventory.InDocketLine.WE_PackageGroupId = "ABC";
			inventory.PerPackageQty = 1m;

			AvailableInventory.Inventory.Add(inventory);
			AssertEquals("ABC", AvailableInventory.PackageGroupId);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZString.Empty, AvailableInventory.PackageGroupId);
		}

		#endregion

		#region PerPackageQty

		public void TestAvailableInventory_PerPackageQty()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(1m, AvailableInventory.PerPackageQty);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDecimal.Zero, AvailableInventory.PerPackageQty);
		}

		#endregion

		#region TestClearPickLinesCache

		public void TestClearPickLinesCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 18, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 18);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			AssertEquals(3, availableInventory.PickLines.Count()); // 1 CAS + 1 BOX + 3 UNT
			AssertEquals(3, availableInventory.AvailableInventoriesSplitByUOM.Count);
			var previousUOMAvailableInventories = availableInventory.AvailableInventoriesSplitByUOM.ToArray();

			// deallocate 3 units
			availableInventory.PickLineQuantity = 15;

			// PickLines collection is cached, but now cache must be cleared and collection must be reloaded
			AssertEquals(2, availableInventory.AvailableInventoriesSplitByUOM.Count);
			foreach (var previousUOMAvailableInventory in previousUOMAvailableInventories)
			{
				foreach (var currentUOMAvailableInventory in availableInventory.AvailableInventoriesSplitByUOM)
				{
					AssertEquals("UOMAvailableInventories must be recreated", false, ReferenceEquals(previousUOMAvailableInventory, currentUOMAvailableInventory));
				}
			}
		}

		#endregion

		#region ILineAttributes Members

		public void TestExpiryDate()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(1), AvailableInventory.ExpiryDate);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.ExpiryDate);
		}

		public void TestPackingDate()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(-1), AvailableInventory.PackingDate);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.PackingDate);
		}

		public void TestBondedEntryKey()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("BEK-1", AvailableInventory.BondedEntryKey);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.BondedEntryKey);
		}

		public void TestDeclarantsReference()
		{
			var orderedInventory = Helper.CreateOrderedInventory();
			Factory.Save();
			var receive = Helper.CreateWhsReceive(orderedInventory.Client, orderedInventory.Pick.Warehouse, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, orderedInventory.SupplierPart, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			((IWhsPickAvailableInventoryInternals)AvailableInventory).SetAllProperties(orderedInventory, inventory);

			AssertEquals(ZString.Empty, AvailableInventory.DeclarantsReference);

			receive.WD_CustomerReference = "TEST_REF";

			AssertEquals("TEST_REF", AvailableInventory.DeclarantsReference);
		}

		public void TestDeclarantsReference_NoInventory()
		{
			var availableInventory = (WhsPickAvailableInventory)GetNewBusinessObject();
			AssertEquals(string.Empty, availableInventory.DeclarantsReference);
		}

		public void TestAllocationKey_NoInventory()
		{
			var availableInventory = (WhsPickAvailableInventory)GetNewBusinessObject();
			AssertEquals("Precondition: Empty.", string.Empty, availableInventory.AllocationKey);
		}

		public void TestAllocationKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WI_AllocationKey = "ALO-123";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should have an allocation key.", "ALO-123", availInv.AllocationKey);
		}

		public void TestBondedEntryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			inventory.CustomsData.WB_EntryKey = "ABC123";
			inventory.CustomsData.WB_EntryDate = ZDateTime.Today.AddDays(-2);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should have a bonded entry date.", inventory.CustomsData.WB_EntryDate, availInv.BondedEntryDate);
		}

		public void TestBondedEntryDate_NotCustomsTransaction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should *not* have a bonded entry date.", ZDateTime.Empty, availInv.BondedEntryDate);

			var bondedData = Factory.LoadTop1<WhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, inventory.PK) { FetchOnlyFromLocalCache = true });
			AssertNull("Should not have created bonded data.", bondedData);
		}

		public void TestBondedEntryDate_NullPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			inventory.CustomsData.WB_EntryKey = "ABC123";
			inventory.CustomsData.WB_EntryDate = ZDateTime.Today.AddDays(-2);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];

			availInv.Deactivate();

			AssertEquals("Should *not* have a bonded entry date.", ZDateTime.Empty, availInv.BondedEntryDate);
		}

		public void TestPartAttrib1()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("PA1", AvailableInventory.PartAttrib1);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.PartAttrib1);
		}

		public void TestPartAttrib2()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("PA2", AvailableInventory.PartAttrib2);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.PartAttrib2);
		}

		public void TestPartAttrib3()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("PA3", AvailableInventory.PartAttrib3);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.PartAttrib3);
		}

		public void TestSerialNumber_NoInventory()
		{
			var availableInventory = (WhsPickAvailableInventory)GetNewBusinessObject();
			AssertEquals("Precondition: Empty.", string.Empty, availableInventory.SerialNumber);
		}

		public void TestHasSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory1 = GetAvailableInventory(data.Part1.PK);
			var availableInventory2 = GetAvailableInventory(data.Part2.PK);
			AssertEquals("EnableSchemaRedesignChanges is not enable.", false, availableInventory1.HasSerialNumber);
			AssertEquals("Not use serial number.", false, availableInventory2.HasSerialNumber);

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("EnableSchemaRedesignChanges is enabled.", true, availableInventory1.HasSerialNumber);
				AssertEquals("EnableSchemaRedesignChanges is enabled but serial number not used.", false, availableInventory2.HasSerialNumber);
			}

			WhsPickAvailableInventory GetAvailableInventory(ZGuid productPK)
				=> pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK.Equals(productPK)).AvailableInventories[0];
		}

		public void TestSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "", "", "", "SERN", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should have a serial number.", "SERN", availInv.SerialNumber);
		}

		public void TestSerialNumber_AttributeNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "", "", "", "SERN", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Should *not* display the serial number.", string.Empty, availInv.SerialNumber);

			owner.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PickMode is cached, should *not* display the serial number.", string.Empty, availInv.SerialNumber);

			Factory.Save();
			AssertEquals("Should display the serial number.", "SERN", availInv.SerialNumber);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestIAttributesSetAttributes()
		{
			AvailableInventory.SetAttributes(new TestILineAttributes());
		}

		public void TestExpiryDateInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.ExpiryDate, AvailableInventory.ExpiryDateInfo.Name);
		}

		public void TestPackingDateInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PackingDate, AvailableInventory.PackingDateInfo.Name);
		}

		public void TestBondedEntryKeyInfo()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.BondedEntryKey, AvailableInventory.BondedEntryKeyInfo.Name);
		}

		public void TestPartAttrib1Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PartAttrib1, AvailableInventory.PartAttrib1Info.Name);
		}

		public void TestPartAttrib2Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PartAttrib2, AvailableInventory.PartAttrib2Info.Name);
		}

		public void TestPartAttrib3Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.PartAttrib3, AvailableInventory.PartAttrib3Info.Name);
		}

		#endregion

		#region ILineCustomAttributes Members

		[ExpectException(typeof(NotSupportedException))]
		public void TestILineCustomAttributesSetCustomAttributes(ILineAttributes src)
		{
			AvailableInventory.SetCustomAttributes(new TestILineCustomAttributes());
		}

		public void TestCustomAttrib1()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("CA1", AvailableInventory.CustomAttrib1);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("CA2", AvailableInventory.CustomAttrib2);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("CA3", AvailableInventory.CustomAttrib3);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("CA4", AvailableInventory.CustomAttrib4);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("CA5", AvailableInventory.CustomAttrib5);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomAttrib5);
		}

		public void TestCustomAttrib6()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("CA6", AvailableInventory.CustomAttrib6);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomAttrib6);
		}

		public void TestCustomDecimal1()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(1.1m, AvailableInventory.CustomDecimal1);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(0m, AvailableInventory.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(2.2m, AvailableInventory.CustomDecimal2);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(0m, AvailableInventory.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(3.3m, AvailableInventory.CustomDecimal3);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(0m, AvailableInventory.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(4.4m, AvailableInventory.CustomDecimal4);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(0m, AvailableInventory.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(5.5m, AvailableInventory.CustomDecimal5);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(0m, AvailableInventory.CustomDecimal5);
		}

		public void TestCustomDate1()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(1), AvailableInventory.CustomDate1);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.CustomDate1);
		}

		public void TestCustomDate2()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(2), AvailableInventory.CustomDate2);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.CustomDate2);
		}

		public void TestCustomDate3()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(3), AvailableInventory.CustomDate3);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.CustomDate3);
		}

		public void TestCustomDate4()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(4), AvailableInventory.CustomDate4);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.CustomDate4);
		}

		public void TestCustomDate5()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(ZDateTime.Today.AddDays(5), AvailableInventory.CustomDate5);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(ZDateTime.Empty, AvailableInventory.CustomDate5);
		}

		public void TestCustomFlag1()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(true, AvailableInventory.CustomFlag1);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(false, AvailableInventory.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(true, AvailableInventory.CustomFlag2);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(false, AvailableInventory.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(true, AvailableInventory.CustomFlag3);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(false, AvailableInventory.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(true, AvailableInventory.CustomFlag4);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(false, AvailableInventory.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals(true, AvailableInventory.CustomFlag5);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals(false, AvailableInventory.CustomFlag5);
		}

		public void TestCustomTextBlob1()
		{
			SetupTestPickAvailableInventoryData(AvailableInventory);
			AssertEquals("TEXTBLOB1", AvailableInventory.CustomTextBlob1);
			AvailableInventory.Inventory.RemoveAll();
			AssertEquals("", AvailableInventory.CustomTextBlob1);
		}

		public void TestCustomAttrib1Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomAttrib1, AvailableInventory.CustomAttrib1Info.Name);
		}

		public void TestCustomAttrib2Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomAttrib2, AvailableInventory.CustomAttrib2Info.Name);
		}

		public void TestCustomAttrib3Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomAttrib3, AvailableInventory.CustomAttrib3Info.Name);
		}

		public void TestCustomAttrib4Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomAttrib4, AvailableInventory.CustomAttrib4Info.Name);
		}

		public void TestCustomAttrib5Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomAttrib5, AvailableInventory.CustomAttrib5Info.Name);
		}

		public void TestCustomAttrib6Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomAttrib6, AvailableInventory.CustomAttrib6Info.Name);
		}

		public void TestCustomDecimal1Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDecimal1, AvailableInventory.CustomDecimal1Info.Name);
		}

		public void TestCustomDecimal2Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDecimal2, AvailableInventory.CustomDecimal2Info.Name);
		}

		public void TestCustomDecimal3Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDecimal3, AvailableInventory.CustomDecimal3Info.Name);
		}

		public void TestCustomDecimal4Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDecimal4, AvailableInventory.CustomDecimal4Info.Name);
		}

		public void TestCustomDecimal5Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDecimal5, AvailableInventory.CustomDecimal5Info.Name);
		}

		public void TestCustomDate1Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDate1, AvailableInventory.CustomDate1Info.Name);
		}

		public void TestCustomDat21Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDate2, AvailableInventory.CustomDate2Info.Name);
		}

		public void TestCustomDate3Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDate3, AvailableInventory.CustomDate3Info.Name);
		}

		public void TestCustomDate4Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDate4, AvailableInventory.CustomDate4Info.Name);
		}

		public void TestCustomDate5Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomDate5, AvailableInventory.CustomDate5Info.Name);
		}

		public void TestCustomFlag1Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomFlag1, AvailableInventory.CustomFlag1Info.Name);
		}

		public void TestCustomFlag2Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomFlag2, AvailableInventory.CustomFlag2Info.Name);
		}

		public void TestCustomFlag3Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomFlag3, AvailableInventory.CustomFlag3Info.Name);
		}

		public void TestCustomFlag4Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomFlag4, AvailableInventory.CustomFlag4Info.Name);
		}

		public void TestCustomFlag5Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomFlag5, AvailableInventory.CustomFlag5Info.Name);
		}

		public void TestCustomTextBlob1Info()
		{
			AssertEquals(WhsPickAvailableInventory.Schema.CustomTextBlob1, AvailableInventory.CustomTextBlob1Info.Name);
		}

		#endregion

		#region IWhsPickAvailableInventoryInternals Members

		public void TestIWhsPickInventoryInternalsSetAllProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.Rows[0].Locations[0];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "BEK1-1");
			inventory.WI_WL = location.PK;
			inventory.WI_PalletID = "ABC-123";
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today;
			inventory.WI_InventoryStatus = InventoryStatus.Codes.Available;
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			receive.WD_GS_NKFinalizedBy = "E";
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;

			var orderedInventory = Helper.CreateOrderedInventory();
			orderedInventory.SupplierPart.OP_StockKeepingUnit = "BOX";

			var availableInventory = new WhsPickAvailableInventory(Factory);
			((IWhsPickAvailableInventoryInternals)availableInventory).SetAllProperties(orderedInventory, inventory);
			AssertEquals(orderedInventory, availableInventory.OrderedInventory);
			AssertCollectionContains(inventory, availableInventory.Inventory);
			AssertEquals(orderedInventory.SupplierPart, availableInventory.SupplierPart);
			AssertEquals(orderedInventory.SupplierPart, ((IWhsPickAvailableInventory)availableInventory).SupplierPart);
			AssertEquals(orderedInventory.SupplierPartPK, availableInventory.SupplierPartPK);
			AssertEquals(location, availableInventory.Location);
			AssertEquals(location.PK, availableInventory.LocationPK);
			AssertEquals(location.ToLocationString(), availableInventory.LocationString);
			AssertEquals(inventory.WI_InventoryStatus, availableInventory.InventoryStatus);
			AssertEquals(inventory.StatusDesc, availableInventory.InventoryStatusDesc);
			AssertEquals(10m, availableInventory.QuantityAvailableToPick);
			AssertEquals("BOX", availableInventory.QuantityUQ);
			AssertEquals("BEK1-1", availableInventory.BondedEntryKey);
			AssertEquals(ZDateTimeOffset.Today, availableInventory.ArrivalDate);
			AssertEquals(ZDateTime.Today.AddDays(1), availableInventory.ExpiryDate);
			AssertEquals(ZDateTime.Today.AddDays(-1), availableInventory.PackingDate);
			AssertEquals("PA1", availableInventory.PartAttrib1);
			AssertEquals("PA2", availableInventory.PartAttrib2);
			AssertEquals("PA3", availableInventory.PartAttrib3);
			AssertEquals("ABC-123", availableInventory.PalletID);
		}

		#endregion

		#region ILineAssigner Members

		public void TestAssignLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition:", !pick.IsPickByUOMEnabled);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var user = Helper.CreateGlbStaff("T1", "T1");
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));

			var assigner = (ILineStaffAssigner)availableInventory;
			assigner.AssignLine(user);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user.PK));
		}

		public void TestAssignLine_PickByUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition:", pick.IsPickByUOMEnabled);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var user = Helper.CreateGlbStaff("T1", "T1");
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(1, availableInventory.AvailableInventoriesSplitByUOM.Count);
			Assert(availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));

			var assigner = (ILineStaffAssigner)availableInventory;
			assigner.AssignLine(user);
			Assert(availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user.PK));
		}

		public void TestUnAssignLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition:", !pick.IsPickByUOMEnabled);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));

			var user1 = Helper.CreateGlbStaff("T1", "T1");
			var user2 = Helper.CreateGlbStaff("T2", "T2");
			var assigner = (ILineStaffAssigner)availableInventory;
			assigner.AssignLine(user1);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user1.PK));

			assigner.UnAssignLine(user1);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));
		}

		public void TestUnAssignLine_DifferentUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition:", !pick.IsPickByUOMEnabled);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));

			var user1 = Helper.CreateGlbStaff("T1", "T1");
			var user2 = Helper.CreateGlbStaff("T2", "T2");
			var assigner = (ILineStaffAssigner)availableInventory;
			assigner.AssignLine(user1);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user1.PK));

			assigner.UnAssignLine(user2);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user1.PK));
		}

		public void TestUnAssignLine_NullUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition:", !pick.IsPickByUOMEnabled);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));

			var user1 = Helper.CreateGlbStaff("T1", "T1");
			var user2 = Helper.CreateGlbStaff("T2", "T2");
			var assigner = (ILineStaffAssigner)availableInventory;
			assigner.AssignLine(user1);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user1.PK));

			assigner.UnAssignLine(null);
			Assert(availableInventory.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));
		}

		public void TestUnAssignLine_PickByUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition:", pick.IsPickByUOMEnabled);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var user = Helper.CreateGlbStaff("T1", "T1");
			AssertEquals(1, availableInventory.PickLines.Count());
			AssertEquals(1, availableInventory.AvailableInventoriesSplitByUOM.Count);
			Assert(availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));

			var assigner = (ILineStaffAssigner)availableInventory;
			assigner.AssignLine(user);
			Assert(availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == user.PK));

			assigner.UnAssignLine(user);
			Assert(availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitBase>().All(x => x.AssignedToPK == ZGuid.Empty));
		}

		public void TestCanAssignOrUnAssignLine()
		{
			SetupWhsPickOrderedInventory();
			var assigner = (ILineStaffAssigner)AvailableInventory;

			AvailableInventory.Allocate = false;
			Helper.SetPickedDate(AvailableInventory, ZDateTimeOffset.Empty);
			AssertEquals(false, assigner.CanAssignOrUnAssignLine());

			AvailableInventory.Allocate = false;
			Helper.SetPickedDate(AvailableInventory, ZDateTimeOffset.Now);
			AssertEquals(false, assigner.CanAssignOrUnAssignLine());

			AvailableInventory.Allocate = true;
			Helper.SetPickedDate(AvailableInventory, ZDateTimeOffset.Empty);
			AssertEquals(true, assigner.CanAssignOrUnAssignLine());

			AvailableInventory.Allocate = true;
			Helper.SetPickedDate(AvailableInventory, ZDateTimeOffset.Now);
			AssertEquals(false, assigner.CanAssignOrUnAssignLine());

			AvailableInventory.Allocate = true;
			Helper.SetPickedDate(AvailableInventory, ZDateTimeOffset.Empty);
			foreach (var pickLine in AvailableInventory.PickLines)
			{
				pickLine.WZ_IsPicking = true;
			}
			AssertEquals(false, assigner.CanAssignOrUnAssignLine());
		}

		public void TestCanAssignOrUnAssignLine_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { pickLine }, availableInventory.PickLines);

			ILineStaffAssigner assigner = availableInventory;
			AssertEquals("Should not be able to Assign In-Transit Pick Lines.", false, assigner.CanAssignOrUnAssignLine());
		}

		#endregion

		#region Implementation

		void AssertPickLine(WhsPickLine pickLine, ZGuid clientPK, ZGuid prodPK, ZGuid inventoryLinePK, ZDecimal units, ILineAttributes attributes)
		{
			var inventory = pickLine.Inventory;
			AssertEquals(clientPK, inventory.WI_OH_Client);
			AssertEquals(prodPK, inventory.WI_OP);
			AssertEquals(inventoryLinePK, pickLine.WZ_WE_InventoryLine);
			AssertEquals(units, pickLine.WZ_Units);
			Helper.AssertAttributes(inventory, attributes);
		}

		void SetupTestPickAvailableInventoryData(WhsPickAvailableInventory availableInventory)
		{
			var inventory = availableInventory.Inventory.AddNew();
			Helper.SetInventoryAttributes(inventory, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "BEK-1", "SERN");
			Helper.SetInventoryCustomAttributes(inventory, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), true, true, true, true, true, "TEXTBLOB1");
			inventory.PerPackageQty = 1m;
		}

		protected void SetupWhsPickOrderedInventory()
		{
			var orderedInventory = Helper.CreateOrderedInventory();
			Factory.Save();
			var receive = Helper.CreateWhsReceive(orderedInventory.Client, orderedInventory.Pick.Warehouse, "R1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, orderedInventory.SupplierPart, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, orderedInventory.SupplierPart, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			((IWhsPickAvailableInventoryInternals)AvailableInventory).SetAllProperties(orderedInventory, inventory1);
			((IWhsPickAvailableInventoryInternals)AvailableInventory).SetAllProperties(orderedInventory, inventory2);
		}

		WhsPickAvailableInventory FindAvailableInventory(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
			{
				if (availableInventory.Inventory.Contains(inventory))
				{
					return availableInventory;
				}
			}
			return null;
		}

		protected WhsPickAvailableInventory AvailableInventory
		{
			get { return availableInventory ?? (availableInventory = (WhsPickAvailableInventory)GetNewBusinessObject()); }
			set { availableInventory = value; }
		}

		protected WhsPickAvailableInventory availableInventory;

		#endregion
	}
}
