using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsPickAvailableInventorySplitBaseTest<T> : WhsNonPersistentBusinessObjectTestCase
			where T : WhsPickAvailableInventorySplitBase
	{
		#region Related Entities

		#region TestClient

		public void TestClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories were picked.", 2, availableInventory1.PickLines.Count()); // because of 2 inventories

			var availableInventory2 = CreateAvailableInventory();
			availableInventory2.SetData(availableInventory1.PickLines.ToPickLinePairs(), availableInventory1);
			AssertEquals("Client on line should match Available Inventory Client.", data.Org1, availableInventory2.Client);
		}

		#endregion

		#region TestOrderedInventory

		public void TestOrderedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory1 = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories were picked.", 2, availableInventory1.PickLines.Count()); // because of 2 inventories

			var availableInventory2 = CreateAvailableInventory();
			availableInventory2.SetData(availableInventory1.PickLines.ToPickLinePairs(), availableInventory1);
			AssertEquals("Pick on line should match Available Inventory Pick.", orderedInventory, availableInventory2.OrderedInventory);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories were picked.", 2, availableInventory1.PickLines.Count()); // because of 2 inventories

			var availableInventory2 = CreateAvailableInventory();
			availableInventory2.SetData(availableInventory1.PickLines.ToPickLinePairs(), availableInventory1);
			AssertEquals("Product on line should match Available Inventory Product.", data.Part1, availableInventory2.Product.Parent);
		}

		#endregion

		#endregion

		#region TestSetDataAndCheckProperties

		public virtual void TestSetDataAndCheckProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Assert(receive.IsFinalised);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories.Count);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(2, availableInventory.PickLines.Count()); // because of 2 inventories

			var splitInventory = CreateAvailableInventory();
			AssertExceptionThrown<ArgumentNullException>(() => splitInventory.SetData(availableInventory.PickLines.ToPickLinePairs(), null));
			AssertExceptionThrown<ArgumentNullException>(() => splitInventory.SetData(null, availableInventory));

			splitInventory.SetData(availableInventory.PickLines.ToPickLinePairs(), availableInventory);
			AssertEquals(12m, splitInventory.StockUnitQuantity);
			AssertEquals("KEG", splitInventory.StockKeepingUnit);
			AssertExceptionThrown<InvalidOperationException>(() => splitInventory.SetData(availableInventory.PickLines.ToPickLinePairs(), availableInventory));
		}

		#endregion

		#region TestPickedDate

		public virtual void TestPickedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Assert(receive.IsFinalised);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 85m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			AssertNotNull(pick.OrderedInventories[0]);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull(availableInventory);

			availableInventory.PickLineQuantity = 85;
			AssertEquals(85m, availableInventory.PickLineQuantity);

			AssertEquals(5, availableInventory.PickLines.Count());

			AssertEquals(1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);

			var inventory1 = availableInventory.AvailableInventoriesSplitByPickedDetails[0];
			var testDate = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			inventory1.PickedDate = testDate;
			foreach (WhsPickLine pickLine in availableInventory.PickLines)
			{
				AssertEquals(testDate, pickLine.WZ_PickedDateTime);
			}

			inventory1.PickedDate = ZDateTimeOffset.Empty;
			foreach (WhsPickLine pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			}
		}

		#endregion

		#region TestPickedDate_SuspendsPickPercentageRecalculation

		public void TestPickedDate_SuspendsPickPercentageRecalculation()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location2, "");
			AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Percentage Complete should be zero.", (byte)0, pick.WP_PercentageComplete);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var collection = GetCollectionFromAvailableInventory(availableInventory);
			AssertEquals(1, collection.Count);

			var pickedDateTime1 = ZDateTimeOffset.Now.AddSeconds(-10);
			var pickedDateTime2 = ZDateTimeOffset.Now;

			var inventorySplit = collection[0];
			inventorySplit.PickedDate = pickedDateTime1;
			pick.WP_PercentageComplete = 0;

			var pickLine1 = inventorySplit.PickLinesForPickingDetails.First();
			var pickLine2 = inventorySplit.PickLinesForPickingDetails.First(pl => pl != pickLine1);
			int assertionHitCount = 0;
			EventHandler handler = (sender, e) =>
			{
				assertionHitCount++;
				AssertEquals("Percentage Complete Calculation is suspended during finalise, should remain as zero.", (byte)0, pick.WP_PercentageComplete);
			};

			pickLine1.WZ_PickedDateTimeInfo.ValueChanged += handler;
			pickLine2.WZ_PickedDateTimeInfo.ValueChanged += handler;

			inventorySplit.PickedDate = pickedDateTime2;
			AssertEquals("Percentage Complete should be correct after setting Picked Time.", (byte)50, pick.WP_PercentageComplete);
			AssertEquals("Ensure Assertions were hit.", 2, assertionHitCount);
		}

		protected abstract WhsPickAvailableInventorySplitBaseCollection<T> GetCollectionFromAvailableInventory(WhsPickAvailableInventory availableInventory);

		protected virtual void AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestPickedDate_SetsAssignedTo

		[TestDate(2016, 07, 26)]
		public void TestPickedDate_SetsAssignedTo()
		{
			var staff1 = Helper.CreateGlbStaff("BRS", "BRS");
			var staff2 = Helper.CreateGlbStaff("A.V", "A.V");

			var availInventory = CreateAvailableInventoryWithPickLine();
			AssertEquals("Precondition", ZGuid.Empty, availInventory.AssignedToPK);

			var refreshBindingCalled = 0;
			availInventory.AssignedToPKInfo.ValueChanged += (s, e) => refreshBindingCalled++;

			availInventory.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				availInventory.PickedDate = now.AddDays(2);
			}

			AssertEquals("Precondition.", now.AddDays(2), availInventory.PickedDate);
			AssertEquals("Should have set AssignedToPK to current user", staff1.PK, availInventory.AssignedToPK);
			AssertEquals("Should have refreshed binding for AssignedToPK.", 1, refreshBindingCalled);

			availInventory.PickedDate = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, availInventory.PickedDate);
			AssertEquals("Should *not* have cleared AssignedToPK.", staff1.PK, availInventory.AssignedToPK);

			availInventory.AssignedToPK = staff2.PK;

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				availInventory.PickedDate = ZDateTimeOffset.Now;
			}

			AssertEquals("Precondition.", ZDateTimeOffset.Now, availInventory.PickedDate);
			AssertEquals("Should *not* have overridden AssignedToPK.", staff2.PK, availInventory.AssignedToPK);

			availInventory.AssignedToPK = ZGuid.Empty;
			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				availInventory.PickedDate = ZDateTimeOffset.Empty;
			}

			AssertEquals("Clearing PickedDate should *not* set AssignedToPK.", ZGuid.Empty, availInventory.AssignedToPK);
		}

		#endregion

		#region TestAssignedToPK

		public virtual void TestAssignedToPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Assert(receive.IsFinalised);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 85m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			AssertNotNull(pick.OrderedInventories[0]);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull(availableInventory);

			availableInventory.PickLineQuantity = 85;
			AssertEquals(85m, availableInventory.PickLineQuantity);

			AssertEquals(5, availableInventory.PickLines.Count());

			AssertEquals(1, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);

			foreach (WhsPickLine pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZString.Empty, pickLine.WZ_GS_NKAssignedTo);
			}

			var inventroy1 = availableInventory.AvailableInventoriesSplitByPickedDetails[0];
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";

			inventroy1.AssignedToPK = staff.PK;
			foreach (WhsPickLine pickLine in availableInventory.PickLines)
			{
				AssertEquals("TST", pickLine.WZ_GS_NKAssignedTo);
			}

			inventroy1.AssignedToPK = ZGuid.Empty;
			foreach (WhsPickLine pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZString.Empty, pickLine.WZ_GS_NKAssignedTo);
			}
		}

		#endregion

		#region TestReadOnly

		public void TestPickedDateInfo()
		{
			TestReadOnlyForUnfinalizedNonAvailableStock(WhsPickAvailableInventorySplitBase.Schema.PickedDate);
		}

		public void TestAssignedToPKInfo()
		{
			TestReadOnlyForUnfinalizedNonAvailableStock(WhsPickAvailableInventorySplitBase.Schema.AssignedToPK);
		}

		// Since the ReadOnlyForUnfinalizedNonAvailableStock is proxied we need only very basic test, all cases will be tested for the actual calculated property.
		void TestReadOnlyForUnfinalizedNonAvailableStock(string infoName)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 10m, totalPickLineQuantity);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var pickLines = availableInventory.PickLines.ToArray();

			var splitInventory1 = CreateAvailableInventory();
			var splitInventory2 = CreateAvailableInventory();
			AssertEquals("If our object is not properly linked to AvailableInventory then all properties should be readonly.", true, splitInventory1.ZPropertyInfoHash[infoName].ReadOnly);
			AssertEquals("If our object is not properly linked to AvailableInventory then all properties should be readonly.", true, splitInventory2.ZPropertyInfoHash[infoName].ReadOnly);

			pickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			splitInventory1.SetData(new[] { PickLinePair.New(pickLines[0]) }, availableInventory);
			splitInventory2.SetData(new[] { PickLinePair.New(pickLines[1]) }, availableInventory);
			AssertEquals($"When split inventory is Partially or Fully picked the '{infoName}' should be readonly.", true, splitInventory1.ZPropertyInfoHash[infoName].ReadOnly);
			AssertEquals($"When split inventory is not Partially or Fully picked the '{infoName}' should not be readonly.", false, splitInventory2.ZPropertyInfoHash[infoName].ReadOnly);

			pickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition", false, splitInventory1.ZPropertyInfoHash[infoName].ReadOnly);
			AssertEquals("Precondition.", false, splitInventory2.ZPropertyInfoHash[infoName].ReadOnly);

			pickLines[0].WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;
			AssertEquals($"When split inventory is Partially or Fully picked the '{infoName}' should be readonly.", true, splitInventory1.ZPropertyInfoHash[infoName].ReadOnly);
			AssertEquals($"When split inventory is not Partially or Fully picked the '{infoName}' should not be readonly.", false, splitInventory2.ZPropertyInfoHash[infoName].ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals($"When pick is finalised all properties should be readonly.", true, splitInventory1.ZPropertyInfoHash[infoName].ReadOnly);
			AssertEquals($"When pick is finalised all properties should be readonly.", true, splitInventory2.ZPropertyInfoHash[infoName].ReadOnly);
		}

		public void TestReadOnly_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			if (EnableUOM)
			{
				data.Whs1.WW_IsPickByUOMEnabled = true;
			}
			else
			{
			}
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A-1"));
			var wheelInventory = receive.Inventory[0];
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

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
			var collection = GetCollectionFromAvailableInventory(bikeAvailableInventory);
			AssertEquals(true, collection.Count > 0);
			foreach (T bikeSplitInventory in collection)
			{
				AssertEquals(true, bikeSplitInventory.PickedDateInfo.ReadOnly);
				AssertEquals(true, bikeSplitInventory.AssignedToPKInfo.ReadOnly);
			}
		}

		#endregion

		#region TestLinkPickLinesToTask

		public void TestLinkPickLinesToTask()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Percentage Complete should be zero.", (byte)0, pick.WP_PercentageComplete);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var collection = GetCollectionFromAvailableInventory(availableInventory);
			AssertEquals(1, collection.Count);
			var inventorySplit = collection[0];

			AssertExceptionThrown<ArgumentException>(() => inventorySplit.LinkPickLinesToTask(-1m, ZGuid.BrettsGuid));
			AssertExceptionThrown<ArgumentException>(() => inventorySplit.LinkPickLinesToTask(0m, ZGuid.BrettsGuid));
			AssertExceptionThrown<ArgumentException>(() => inventorySplit.LinkPickLinesToTask(1000m, ZGuid.BrettsGuid));

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			inventorySplit.LinkPickLinesToTask(10m, pk1);
			AssertEquals("First pick line should be linked to the first task.", pk1, inventorySplit.PickLinesForPickingDetails.ElementAt(0).WZ_P9_Task);
			AssertEquals("The second pick line should *not* be linked to a task.", ZGuid.Empty, inventorySplit.PickLinesForPickingDetails.ElementAt(1).WZ_P9_Task);
			AssertEquals("Should *not* have split pick lines", 2, inventorySplit.PickLinesForPickingDetails.Count());

			inventorySplit.LinkPickLinesToTask(10m, pk2);
			AssertEquals("First pick line should be linked to the first task.", pk1, inventorySplit.PickLinesForPickingDetails.ElementAt(0).WZ_P9_Task);
			AssertEquals("Second pick line should be linked to the second task.", pk2, inventorySplit.PickLinesForPickingDetails.ElementAt(1).WZ_P9_Task);
			AssertEquals("Should *not* have split pick lines", 2, inventorySplit.PickLinesForPickingDetails.Count());

			AssertExceptionThrown<ArgumentException>(() => inventorySplit.LinkPickLinesToTask(10m, ZGuid.BrettsGuid));
		}

		public void TestLinkPickLinesToTask_Picked()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Percentage Complete should be zero.", (byte)0, pick.WP_PercentageComplete);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var collection = GetCollectionFromAvailableInventory(availableInventory);
			AssertEquals(1, collection.Count);
			var inventorySplit = collection[0];

			inventorySplit.PickedDate = ZDateTimeOffset.Now;

			AssertExceptionThrown<ArgumentException>(() => inventorySplit.LinkPickLinesToTask(1m, ZGuid.BrettsGuid));
		}

		public void TestLinkPickLinesToTask_MultipleLines()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Percentage Complete should be zero.", (byte)0, pick.WP_PercentageComplete);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var collection = GetCollectionFromAvailableInventory(availableInventory);
			AssertEquals(1, collection.Count);
			var inventorySplit = collection[0];

			var pk1 = ZGuid.NewZGuid();
			inventorySplit.LinkPickLinesToTask(20m, pk1);
			AssertEquals("First pick line should be linked to the task.", pk1, inventorySplit.PickLinesForPickingDetails.ElementAt(0).WZ_P9_Task);
			AssertEquals("Second pick line should be linked to the task.", pk1, inventorySplit.PickLinesForPickingDetails.ElementAt(1).WZ_P9_Task);
			AssertEquals("Should *not* have split pick lines", 2, inventorySplit.PickLinesForPickingDetails.Count());
		}

		public void TestLinkPickLinesToTask_Split()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, location1, "");
			AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Percentage Complete should be zero.", (byte)0, pick.WP_PercentageComplete);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var collection = GetCollectionFromAvailableInventory(availableInventory);
			AssertEquals(1, collection.Count);

			var inventorySplit = collection[0];
			var pk1 = ZGuid.NewZGuid();
			inventorySplit.LinkPickLinesToTask(16m, pk1);
			AssertEquals("First pick line should be linked to the task.", pk1, inventorySplit.PickLinesForPickingDetails.ElementAt(0).WZ_P9_Task);
			AssertEquals("First pick line should be unchanged.", 10m, inventorySplit.PickLinesForPickingDetails.ElementAt(0).WZ_Units);

			var splitLine = inventorySplit.PickLinesForPickingDetails.Single(pl => pl.WZ_Units == 6m);
			var originalLine = inventorySplit.PickLinesForPickingDetails.Single(pl => pl.WZ_Units == 4m);
			AssertEquals("Second pick line should be linked to the task.", pk1, splitLine.WZ_P9_Task);
			AssertEquals("Remaining pick line should *not* be linked to the task.", ZGuid.Empty, originalLine.WZ_P9_Task);

			AssertEquals("Should have split the pick line.", 6m, splitLine.WZ_Units);
			AssertEquals("Should have split the pick line.", 4m, originalLine.WZ_Units);
			AssertEquals("Should have split the pick line.", originalLine.WZ_WE_InventoryLine, splitLine.WZ_WE_InventoryLine);
			AssertEquals("Should have split the pick line.", originalLine.WZ_WE_TransactionLine, splitLine.WZ_WE_TransactionLine);
			AssertEquals("Should have split pick lines", 3, inventorySplit.PickLinesForPickingDetails.Count());
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			AssertType(ExpectedValidationType, CreateAvailableInventory().Validation);
		}

		protected abstract Type ExpectedValidationType { get; }

		#endregion

		#region Implementation

		protected abstract T CreateAvailableInventory();

		protected abstract T CreateAvailableInventoryWithPickLine();

		protected virtual bool EnableUOM => false;

		#endregion
	}
}
