using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsPickAvailableInventorySplitBaseCollectionTest<T1, T2> : WhsNonPersistentBusinessObjectCollectionTestCase<T1>
			where T1 : WhsPickAvailableInventorySplitBaseCollection<T2>
			where T2 : WhsPickAvailableInventorySplitBase
	{
		#region TestConstructor

		public void TestConstructor_NullAvailableInventory()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GetNewCollection(null));
		}

		#endregion

		#region TestRebuildCollection

		public void TestRebuildCollection()
		{
			TestRebuildCollectionCore();
		}

		protected abstract void TestRebuildCollectionCore();

		#endregion

		#region TestRebuildCollection_SuspendsSettingHasChanges

		public void TestRebuildCollection_SuspendsSettingHasChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff1 = Helper.CreateGlbStaff("AAA", "Antman");
			var staff2 = Helper.CreateGlbStaff("ZZZ", "Zeroman");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			AdditionalSetupForLoadAvailableInventoryTest(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 24m);
			var pick = Helper.CreatePickNew(order);

			var collection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			collection.RebuildCollection();
			AssertEquals("Should have one set of Picker Details Split.", 1, collection.Count);
			AssertEquals("Picker Details Collection should not have Changes.", false, collection.HasChanges);

			collection.RebuildCollection();
			AssertEquals("Picker Details Collection should not have Changes.", false, collection.HasChanges);
		}

		#endregion

		#region TestRebuildCollection_WithInTransitPickLines

		public void TestRebuildCollection_WithInTransitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff1 = Helper.CreateGlbStaff("AAA", "Antman");
			var staff2 = Helper.CreateGlbStaff("ZZZ", "Zeroman");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			AdditionalSetupForLoadAvailableInventoryTest(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 24m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			var originalPickLine = orderLine1.PickLines.Single();
			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			var inTransitLine = Helper.PickAndMakeInTransitTransfer(originalPickLine, now);

			var newPickLine = inTransitLine.PickLines.Single();
			newPickLine.WZ_GS_NKAssignedTo = staff1.GS_Code;
			originalPickLine.WZ_GS_NKAssignedTo = staff2.GS_Code;

			var collection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			collection.RebuildCollection();
			AssertEquals("Should have two sets of Picker Details Split.", 2, collection.Count);

			var splitItem1 = collection.Cast<T2>().Single(a => a.AssignedToPK == staff1.PK);
			AssertEquals("Picked Date should be Original Pick Time.", now, splitItem1.PickedDate);

			var splitItem2 = collection.Cast<T2>().Single(a => a.AssignedToPK.IsEmpty);
			AssertEquals("Picked Date should be Empty.", ZDateTimeOffset.Empty, splitItem2.PickedDate);
		}

		public void TestRebuildCollection_WithInTransitPickLines_MultipleSteps()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var staff1 = Helper.CreateGlbStaff("AAA", "Antman");
			var staff2 = Helper.CreateGlbStaff("ZZZ", "Zeroman");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			AdditionalSetupForLoadAvailableInventoryTest(data);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 24m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = orderLine1.PickLines.Single();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit, now.AddDays(i), allowMultipleSteps: true);

				var location = i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				newPickLine.WZ_GS_NKAssignedTo = i > 0 ? staff2.GS_Code : staff1.GS_Code;

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = staff2.GS_Code;

			var collection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			collection.RebuildCollection();
			AssertEquals("Should have two sets of Picker Details Split.", 2, collection.Count);

			var splitItem1 = collection.Cast<T2>().Single(a => a.AssignedToPK == staff1.PK);
			AssertEquals("Picked Date should be Original Pick Time.", now, splitItem1.PickedDate);

			var splitItem2 = collection.Cast<T2>().Single(a => a.AssignedToPK.IsEmpty);
			AssertEquals("Picked Date should be Empty.", ZDateTimeOffset.Empty, splitItem2.PickedDate);
		}

		protected virtual void AdditionalSetupForLoadAvailableInventoryTest(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		#endregion

		#region TestAllowRemove

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Implementation

		protected abstract T1 GetNewCollection(WhsPickAvailableInventory availableInventory);

		#endregion
	}
}
