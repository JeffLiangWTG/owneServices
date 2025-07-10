using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(ReservedPickLineCollection))]
	class ReservedPickLineCollectionTest : ActiveBusinessObjectCollectionTestCase<ReservedPickLineCollection>
	{
		#region TestConstructor

		#region TestInventoryConstructor

		public void TestInventoryConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 2m, transferLine.QtyCommittedIncludingMatchingLines);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 5m, reservedPickLine.ReservedQuantity);

			var collection = new ReservedPickLineCollection(inventory);
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine }, collection);

			reservedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Collection should only contain unpicked lines.", 0, collection.Count);
		}

		#endregion

		#region TestConstructor_DocketLineWithRelationship

		public void TestConstructor_DocketLineWithRelationship()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			var orderLine = order.Lines[0];
			orderLine.ReserveStockIfAbleTo(receiveLine.Inventory[0], 3m);
			AssertEquals("Precondition - ensure stock was reserved.", 3m, receiveLine.ReservedQuantity);

			var reservedInventory = new ReservedPickLineCollection(receiveLine, WhsPickLineSchema.WZ_WE_InventoryLine);
			AssertEquals("We should be easily able to locate how much stock from the inventory has been reserved.", 3m,
				reservedInventory.Sum(l => l.WZ_Units));

			var reservedOrder = new ReservedPickLineCollection(orderLine);
			AssertEquals(
				"We should be easily able to locate how much stock for particular order line has been reserved.", 3m,
				reservedOrder.Sum(l => l.WZ_Units));

			reservedOrder.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Reserved PickLines does not contain Picked PickLines.", 0, reservedInventory.Count);
			AssertEquals("Reserved PickLines does not contain Picked PickLines.", 0, reservedOrder.Count);
		}

		#endregion

		#endregion

		#region TestInvalidateAll

		public void TestInvalidateAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Lines[0].Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var reserveLine1 = orderLine1.ReserveStockIfAbleTo(inventory);
			var reserveLine2 = orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 4m, reserveLine1.WZ_Units);
			AssertEquals("Precondition: Stock is reserved.", 6m, reserveLine2.WZ_Units);

			Factory.Save();

			var collection1 = new ReservedPickLineCollection(orderLine1);
			var collection2 = new ReservedPickLineCollection(orderLine2);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine1 }, collection1);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine2 }, collection2);

			var pickLineCollection1 = new DummyPickLineCollection(orderLine1);
			var pickLineCollection2 = new DummyPickLineCollection(orderLine2);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine1 }, pickLineCollection1);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine2 }, pickLineCollection2);

			var subClassCollection1 = new DummySubclassCollection(orderLine1);
			var subClassCollection2 = new DummySubclassCollection(orderLine2);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine1 }, subClassCollection1);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine2 }, subClassCollection2);

			var listChangedHitCount = 0;
			((IBindingList)collection1).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)collection2).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)pickLineCollection1).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)pickLineCollection2).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)subClassCollection1).ListChanged += (sender, e) => listChangedHitCount++;
			((IBindingList)subClassCollection2).ListChanged += (sender, e) => listChangedHitCount++;

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine1.PK);
			var reserveLineInOtherFactory = otherFactory.Load<WhsPickLine>(reserveLine1.PK);
			var collectionInOtherFactory = new ReservedPickLineCollection(orderLineInOtherFactory);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLineInOtherFactory }, collectionInOtherFactory);

			var pick = Factory.New<WhsPick>();
			((IBusinessObjectInternals)order).Row[WhsDocketSchema.Constants.WD_WP] = pick.PK.ToGuid();
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine1 }, collection1);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine2 }, collection2);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine1 }, pickLineCollection1);
			AssertContainsExactElementsInAnyOrder(new[] { reserveLine2 }, pickLineCollection2);

			ReservedPickLineCollection.InvalidateAll(Factory);
			AssertEquals("Collection should have refreshed as the order is attached to a Pick.", 0, collection1.Count);
			AssertEquals("Collection should have refreshed as the order is attached to a Pick.", 0, collection2.Count);
			AssertEquals(
				"Collection should have refreshed the subclass collections as the order is attached to a Pick.", 0,
				subClassCollection1.Count);
			AssertEquals(
				"Collection should have refreshed the subclass collections as the order is attached to a Pick.", 0,
				subClassCollection2.Count);
			AssertContainsExactElementsInAnyOrder(
				"Invalidate All should only have affected ReservedPickLineCollections.", new[] { reserveLine1 },
				pickLineCollection1);
			AssertContainsExactElementsInAnyOrder(
				"Invalidate All should only have affected ReservedPickLineCollections.", new[] { reserveLine2 },
				pickLineCollection2);
			AssertContainsExactElementsInAnyOrder("Other factory should be unaffected.",
				new[] { reserveLineInOtherFactory }, collectionInOtherFactory);
			AssertEquals("List changed should not have fired.", 0, listChangedHitCount);
		}

		class DummyPickLineCollection : WhsPickLineCollection
		{
			public DummyPickLineCollection(WhsDocketLine master)
				: base(master)
			{
			}

			// Same in memory filter as reserved pick line collection
			protected override bool MatchesFilterCore(WhsPickLine element, bool fetchOnlyFromLocalCache)
			{
				var matchesFilter = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
				if (matchesFilter)
				{
					var docketLine = element.DocketLine as WhsPickableDocketLine;
					matchesFilter = docketLine != null && docketLine.IsDocketUnpicked;
				}

				return matchesFilter;
			}
		}

		class DummySubclassCollection : ReservedPickLineCollection
		{
			public DummySubclassCollection(WhsPickableDocketLine master)
				: base(master)
			{
			}
		}

		#endregion

		#region TestMatchesFilter

		public void TestMatchesFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var collection = new ReservedPickLineCollection(inventory);
			var reservedPickLine = Factory.New<WhsPickLine>();
			reservedPickLine.IsReserveLine = true;
			reservedPickLine.WZ_WE_TransactionLine = orderLine.PK;
			reservedPickLine.ReservedQuantity = 5m;
			collection.Add(reservedPickLine);
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine }, collection);
			AssertEquals("Should have relationship with Inventory.", inventory.WI_WE_InDocketLine,
				reservedPickLine.WZ_WE_InventoryLine);

			collection.RemoveFromRelationship(reservedPickLine);
			AssertEquals("Should remove relationship with Inventory.", ZGuid.Empty,
				reservedPickLine.WZ_WE_InventoryLine);
			AssertEquals(0, collection.Count);

			order.WD_WP = Factory.New<WhsPick>().PK;
			collection.Add(reservedPickLine);
			AssertEquals("Should not add pick line that is attached to a pick.", 0, collection.Count);
		}

		#endregion

		#region Implementation

		protected override ReservedPickLineCollection GetCollectionToTest()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			return new ReservedPickLineCollection(orderLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var pickLine = Factory.NewWithValidTestData<WhsPickLine>();
			((IBusinessObjectInternals)pickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m;
			return pickLine;
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
