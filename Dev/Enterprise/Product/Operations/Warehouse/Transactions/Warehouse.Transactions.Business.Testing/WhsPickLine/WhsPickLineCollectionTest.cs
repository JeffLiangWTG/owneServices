using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickLineCollection))]
	class WhsPickLineCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsPickLineCollection>
	{
		#region TestConstructor

		#region TestConstructor_OrderedInventory

		public void TestConstructor_OrderedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - 4 pick lines should be created 2 for Part1 and 2 for Part2.", 4, pick.GetAllPickLines().Count());

			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { orderLine1 }, orderedInventory1.Owners);

			var pickLinesCollection1 = new WhsPickLineCollection(orderedInventory1);
			var expectedPickLines1 = orderedInventory1.Owners.SelectMany(o => o.PickLines).ToArray();
			AssertContainsExactElementsInAnyOrder("Ordered inventory's PickLines collection should contain all pick lines from all of its owners.",
				expectedPickLines1,
				pickLinesCollection1);

			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { orderLine2, orderLine3 }, orderedInventory2.Owners);

			var pickLinesCollection2 = new WhsPickLineCollection(orderedInventory2);
			var expectedPickLines2 = orderedInventory2.Owners.SelectMany(o => o.PickLines).ToArray();
			AssertContainsExactElementsInAnyOrder("Ordered inventory's PickLines collection should contain all pick lines from all of its owners.",
				expectedPickLines2,
				pickLinesCollection2);

			var newPickLine = orderLine3.PickLines.AddNew();
			newPickLine.WZ_WE_InventoryLine = receive.Lines[0].PK;
			// Only pickLineCollection2 should have the new PickLine as its relationship matches.
			AssertContainsExactElementsInAnyOrder(expectedPickLines1, pickLinesCollection1);
			AssertContainsExactElementsInAnyOrder(expectedPickLines2.Concat(new[] { newPickLine }), pickLinesCollection2);
		}

		public void TestConstructor_OrderedInventory_ThrowsExceptionWhenNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsPickLineCollection(default(WhsPickOrderedInventory)));
		}

		#endregion

		#region TestEmptyInDocketLineForInventoryThrowsException

		public void TestEmptyInDocketLineForInventoryThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ReservedPickLineCollection(Factory.New<WhsInventoryView>()));
		}

		#endregion

		#region TestDocketLineConstructor

		public void TestDocketLineConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			var collection = new WhsPickLineCollection(transferLine);
			AssertEquals(0, collection.Count);

			var pickLine1 = collection.AddNew();
			AssertEquals("New pickline has correct relationship.", transferLine.PK, pickLine1.WZ_WE_TransactionLine);

			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1, pickLine2 }, collection);
		}

		#endregion

		#endregion

		#region TestOrderedInventoryRelationship

		#region TestOrderedInventoryRelationship_DoesNotSupportAdd

		public void TestOrderedInventoryRelationship_DoesNotSupportAdd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var collection = new WhsPickLineCollection(pick.OrderedInventories[0]);
			AssertExceptionThrown<InvalidOperationException>(() => collection.Add(Factory.New<WhsPickLine>()));
		}

		#endregion

		#region TestOrderedInventoryRelationship_DoesNotSupportRemove

		public void TestOrderedInventoryRelationship_DoesNotSupportRemove()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var collection = new WhsPickLineCollection(pick.OrderedInventories[0]);
			AssertExceptionThrown<InvalidOperationException>(() => collection.RemoveFromRelationship(orderLine.PickLines.Single()));
		}

		#endregion

		#region TestOrderedInventoryRelationship_OwnersChange

		public void TestOrderedInventoryRelationship_OwnersChange()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 6m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 4m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			var orderedInventory = pick.OrderedInventories[0];
			var collection = new WhsPickLineCollection(orderedInventory);
			AssertEquals("Should have no Pick Lines.", 0, collection.Count);

			var pickLine = orderLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = receive1.Lines[0].PK;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, collection);

			var newOrderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderedInventory.Owners.Add(newOrderLine);

			var newPickLine = newOrderLine.PickLines.AddNew();
			newPickLine.WZ_WE_InventoryLine = receive2.Lines[0].PK;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine, newPickLine }, collection);
		}

		#endregion

		#region TestOrderedInventoryRelationship_AdditionalFilter

		public void TestOrderedInventoryRelationship_AdditionalFilter()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 4m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines.Single(pl => pl.WZ_Units == 6m);
			var pickLine2 = orderLine.PickLines.Single(pl => pl.WZ_Units == 4m);
			var collection = new WhsPickLineCollection(pick.OrderedInventories[0]);
			collection.AdditionalFilter = new ZQuery(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.NotEqual, 6m);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, collection);
		}

		#endregion

		#region TestOrderedInventoryRelationship_InTransitPickLines

		public void TestOrderedInventoryRelationship_InTransitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var collection = new WhsPickLineCollection(pick.OrderedInventories[0]);

			var pickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, collection);

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			var inTransitPickLine = orderLine.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { inTransitPickLine }, collection);
		}

		#endregion

		#region TestOrderedInventoryRelationship_NoOrderedInventory

		public void TestOrderedInventoryRelationship_NoOrderedInventory()
		{
			var orderedInventory = new WhsPickOrderedInventory(Factory);
			var collection = new WhsPickLineCollection(orderedInventory);
			AssertEquals("Should have no Elements.", 0, collection.Count);
			AssertEquals("Should have no Elements.", true, collection.Relationship.RelationshipFilter.IsNoResultQuery);
		}

		#endregion

		#region TestOrderedInventoryRelationship_WithDeletedOrderedInventory

		public void TestOrderedInventoryRelationship_WithDeletedOrderedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.Delete();

			var collection = new WhsPickLineCollection(orderedInventory);
			AssertEquals("Should have no Elements.", 0, collection.Count);
			AssertEquals("Should have no Elements.", true, collection.Relationship.RelationshipFilter.IsNoResultQuery);
		}

		#endregion

		#region TestOrderedInventoryRelationship_ClearRelationship

		public void TestOrderedInventoryRelationship_ClearRelationship()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var collection = new WhsPickLineCollection(orderedInventory);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, collection);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, orderedInventory.PickLines);

			int relationshipChangedHitCount = 0;
			collection.Relationship.RelationshipFilterChanged += (sender, e) => relationshipChangedHitCount++;

			collection.Relationship.Clear();
			AssertEquals("Clearing Ordered Inventory Relationship should have fired Relationship Change.", 1, relationshipChangedHitCount);

			orderedInventory.Owners.Add(order.Lines.AddNew());
			AssertEquals("Clearing Ordered Inventory Relationship should unhook Relationship Change, so it should not be fired.", 1, relationshipChangedHitCount);
		}

		#endregion

		#region TestOrderedInventoryRelationship_WhenChangingOwnersOfOrderedInventoryRebuildsFilter

		public void TestOrderedInventoryRelationship_WhenChangingOwnersOfOrderedInventoryRebuildsFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var collection = new WhsPickLineCollection(orderedInventory);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, collection);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, orderedInventory.PickLines);

			int relationshipChangedHitCount = 0;
			collection.Relationship.RelationshipFilterChanged += (sender, e) => relationshipChangedHitCount++;

			orderedInventory.Owners.Add(order.Lines.AddNew());
			AssertEquals("Adding Owner should have fired Relationship Change.", 1, relationshipChangedHitCount);
		}

		#endregion

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)new WhsPickLineCollection(Factory)).AllowNew);
		}

		#endregion

		#region TestGetQtyCommitted

		public void TestGetQtyCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			var collection = new WhsPickLineCollection(transferLine);
			var pickline1 = Factory.New<WhsPickLine>();
			pickline1.WZ_WE_TransactionLine = transferLine.PK;
			pickline1.WZ_Units = 5m;

			var pickline2 = Factory.New<WhsPickLine>();
			pickline2.WZ_WE_TransactionLine = transferLine.PK;
			pickline2.WZ_Units = 3m;

			AssertEquals("Quantity committed should be the sum of Pick Lines.", 8m, collection.GetQtyCommitted());
		}

		#endregion

		#region TestGetQtyPicked

		public void TestGetQtyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			var collection = new WhsPickLineCollection(transferLine);
			var pickline1 = Factory.New<WhsPickLine>();
			pickline1.WZ_WE_TransactionLine = transferLine.PK;
			pickline1.WZ_Units = 5m;

			var pickline2 = Factory.New<WhsPickLine>();
			pickline2.WZ_WE_TransactionLine = transferLine.PK;
			pickline2.WZ_Units = 3m;

			AssertEquals("Quantity committed should be the sum of picked Pick Lines.", 0m, collection.GetQtyPicked());

			pickline1.WZ_PickedDateTime = DateTimeOffset.Now;
			AssertEquals("Quantity committed should be the sum of picked Pick Lines.", 5m, collection.GetQtyPicked());

			pickline2.WZ_PickedDateTime = DateTimeOffset.Now;
			AssertEquals("Quantity committed should be the sum of picked Pick Lines.", 8m, collection.GetQtyPicked());
		}

		#endregion

		#region TestGetLinesWithUnitsGreaterThanZero

		public void TestGetLinesWithUnitsGreaterThanZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			var collection = new WhsPickLineCollection(transferLine);
			var pickline1 = Factory.New<WhsPickLine>();
			pickline1.WZ_WE_TransactionLine = transferLine.PK;
			pickline1.WZ_Units = 0m;

			var pickline2 = Factory.New<WhsPickLine>();
			pickline2.WZ_WE_TransactionLine = transferLine.PK;
			pickline2.WZ_Units = 5m;
			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline2 }, collection);
			AssertContainsExactElementsInAnyOrder(new[] { pickline2 }, collection.GetLinesWithUnitsGreaterThanZero());
		}

		#endregion

		#region TestRunPreSaveValidation

		public void TestRunPreSaveValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			var adHocCollection = new WhsPickLineCollection(Factory);
			var nonAdHocCollection = new WhsPickLineCollection(transferLine);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();

			var orderLinePickLine = Factory.New<WhsPickLine>();
			orderLinePickLine.WZ_WE_TransactionLine = orderLine.PK;
			adHocCollection.Add(orderLinePickLine);

			int orderLineValidationHitCount = 0;
			int transferLineValidationHitCount = 0;
			var transferLinePickLine = nonAdHocCollection.AddNew();
			orderLinePickLine.WZ_UnitsInfo.AdditionalValidation += () => orderLineValidationHitCount++;
			transferLinePickLine.WZ_UnitsInfo.AdditionalValidation += () => transferLineValidationHitCount++;

			((IBusiness)adHocCollection).RunPreSaveValidation();
			((IBusiness)nonAdHocCollection).RunPreSaveValidation();
			AssertEquals("Adhoc collection should have Validation disabled.", 0, orderLineValidationHitCount);
			AssertEquals("Normal collection should have Validation enabled.", 1, transferLineValidationHitCount);
		}

		#endregion

		#region TestOrderLinePickLineRelationship

		public void TestOrderLinePickLineRelationship()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();
			var collection = new WhsPickLineCollection(orderLine);

			var pickline1 = Factory.New<WhsPickLine>();
			pickline1.WZ_WE_TransactionLine = orderLine.PK;

			AssertContainsExactElementsInAnyOrder(new[] { pickline1 }, collection);

			var orderline2 = Factory.New<WhsOrderLine>();
			orderline2.WE_WE_ParentDocketLine = orderLine.PK;
			var pickline2 = Factory.New<WhsPickLine>();

			// attach to different order 
			pickline2.WZ_WE_TransactionLine = Guid.NewGuid();
			AssertContainsExactElementsInAnyOrder(new[] { pickline1 }, collection);

			pickline2.WZ_WE_TransactionLine = orderline2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline2 }, collection);

			// attach to master
			var pickline3 = Factory.New<WhsPickLine>();
			pickline3.WZ_WE_TransactionLine = orderLine.PK;

			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline2, pickline3 }, collection);

			var bizo = Factory.New(typeof(DummyBusinessObject));
			AssertExceptionThrown(typeof(InvalidOperationException), () => collection.Relationship.AddToRelationship(bizo));
			AssertExceptionThrown(typeof(InvalidOperationException), () => collection.Relationship.RemoveFromRelationship(bizo));
			Assert(!collection.Relationship.SupportsAddToRelationship());
		}

		#endregion

		#region TestPickLocationSortedProperly

		public void TestPickLocationSortedProperly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-2"));
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-10"));
			Factory.Save();

			var pickline1 = Factory.New<WhsPickLine>();
			pickline1.WZ_Units = 10;
			pickline1.WZ_WE_TransactionLine = orderLine.PK;
			pickline1.WZ_WE_InventoryLine = receiveLine1.PK;

			var pickline2 = Factory.New<WhsPickLine>();
			pickline2.WZ_Units = 10;
			pickline2.WZ_WE_TransactionLine = orderLine.PK;
			pickline2.WZ_WE_InventoryLine = receiveLine2.PK;

			var pickline3 = Factory.New<WhsPickLine>();
			pickline3.WZ_Units = 10;
			pickline3.WZ_WE_TransactionLine = orderLine.PK;
			pickline3.WZ_WE_InventoryLine = receiveLine3.PK;

			var collection = new WhsPickLineCollection(orderLine);
			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline2, pickline3 }, collection);

			collection.ApplySort(WhsPickLineCollection.PickLineInventoryLocationPropertyName, ListSortDirection.Ascending);
			AssertEquals("A-1", collection[0].Inventory.LocationString);
			AssertEquals("A-2", collection[1].Inventory.LocationString);
			AssertEquals("A-10", collection[2].Inventory.LocationString);

			collection.ApplySort(WhsPickLineCollection.PickLineInventoryLocationPropertyName, ListSortDirection.Descending);
			AssertEquals("A-10", collection[0].Inventory.LocationString);
			AssertEquals("A-2", collection[1].Inventory.LocationString);
			AssertEquals("A-1", collection[2].Inventory.LocationString);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		NotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}

		NotificationBuffer notify;

		#endregion
	}
}
