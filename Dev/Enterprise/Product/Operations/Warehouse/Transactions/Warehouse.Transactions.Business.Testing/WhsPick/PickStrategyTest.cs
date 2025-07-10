using System;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PickStrategyTest : WhsTestCaseWithFactory
	{
		#region TestConstructor_NullThrows

		public void TestConstructor_NullThrows()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickStrategy(null));
		}

		#endregion

		#region TestCanInventoryBeAllocated

		public void TestCanInventoryBeAllocated()
		{
			AssertEquals(true, new PickStrategy(Factory.New<WhsPick>()).CanInventoryBeAllocated(null, null));
		}

		#endregion

		#region TestCanAllocateInQuantitiesDifferentToAutoAllocateQuantity

		public void TestCanAllocateInQuantitiesDifferentToAutoAllocateQuantity()
		{
			AssertEquals(true, new PickStrategy(Factory.New<WhsPick>()).CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(null));
		}

		#endregion

		#region TestGetAutoAllocateQuantity

		public void TestGetAutoAllocateQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 22m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory1 = orderedInventory.AvailableInventories[0];
			var availableInventory2 = orderedInventory.AvailableInventories[1];
			var availableInventory3 = orderedInventory.AvailableInventories[2];
			var pickStrategy = new PickStrategy(pick);
			AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));
			AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));
			AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));

			availableInventory1.PickLineQuantity = 9m;
			AssertEquals(1m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));
			AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));
			AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));

			availableInventory2.PickLineQuantity = 8m;
			AssertEquals(1m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));
			AssertEquals(2m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));
			AssertEquals(5m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));

			availableInventory3.PickLineQuantity = 5m;
			AssertEquals(0m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory1));
			AssertEquals(0m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory2));
			AssertEquals(0m, pickStrategy.GetAutoAllocateQuantity(orderedInventory.QuantityShort, availableInventory3));
		}

		#endregion

		#region TestCacheQuantityUnPicked

		public void TestCacheQuantityUnPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 22m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory1 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(ai => ai.Inventory.Single().PK == inventory1.PK);
			var availableInventory2 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(ai => ai.Inventory.Single().PK == inventory2.PK);

			var pickStrategy = new PickStrategy(pick);

			using (pickStrategy.CacheQuantityUnPicked())
			{
				AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory1));
				AssertEquals(10m, pickStrategy.GetQuantityUnPicked(availableInventory1));

				using (pickStrategy.CacheQuantityUnPicked())
				{
					inventory1.WI_TotalUnits = 5m;
					inventory2.WI_TotalUnits = 5m;
					AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory1));
					AssertEquals(10m, pickStrategy.GetQuantityUnPicked(availableInventory1));
					AssertEquals(9m, pickStrategy.GetAutoAllocateQuantity(9m, availableInventory1));

					AssertEquals(5m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory2));
					AssertEquals(5m, pickStrategy.GetQuantityUnPicked(availableInventory2));
				}

				inventory1.WI_TotalUnits = 1m;
				inventory2.WI_TotalUnits = 2m;
				AssertEquals(10m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory1));
				AssertEquals(5m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory2));
				AssertEquals(10m, pickStrategy.GetQuantityUnPicked(availableInventory1));
				AssertEquals(5m, pickStrategy.GetQuantityUnPicked(availableInventory2));
			}

			AssertEquals(1m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory1));
			AssertEquals(2m, pickStrategy.GetAutoAllocateQuantity(10m, availableInventory2));
			AssertEquals(1m, pickStrategy.GetQuantityUnPicked(availableInventory1));
			AssertEquals(2m, pickStrategy.GetQuantityUnPicked(availableInventory2));
		}

		#endregion
	}
}
