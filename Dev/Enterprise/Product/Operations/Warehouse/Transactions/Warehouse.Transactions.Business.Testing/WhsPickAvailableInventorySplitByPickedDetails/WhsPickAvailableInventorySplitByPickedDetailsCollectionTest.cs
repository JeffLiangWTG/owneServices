using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickAvailableInventorySplitByPickedDetailsCollection))]
	class WhsPickAvailableInventorySplitByPickedDetailsCollectionTest : WhsPickAvailableInventorySplitBaseCollectionTest<WhsPickAvailableInventorySplitByPickedDetailsCollection, WhsPickAvailableInventorySplitByPickedDetails>
	{
		#region TestLoadForAvailableInventory

		protected override void TestRebuildCollectionCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 24m);
			var orderLine12 = Helper.CreateWhsOrderLine(order1, data.Part1, 8m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, data.Part1, 12m);
			var orderLine22 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition: Expecting 4 pick lines created", 4, pick.GetAllPickLines().Count());

			var availableInventorySplitByPickedDetailsCollection = GetNewCollection(pick.OrderedInventories[0].AvailableInventories[0]);
			availableInventorySplitByPickedDetailsCollection.RebuildCollection();
			AssertEquals(1, availableInventorySplitByPickedDetailsCollection.Count);
		}

		#endregion

		#region Implementation

		protected override WhsPickAvailableInventorySplitByPickedDetailsCollection GetNewCollection(WhsPickAvailableInventory availableInventory)
		{
			return new WhsPickAvailableInventorySplitByPickedDetailsCollection(Factory, availableInventory);
		}

		protected override WhsPickAvailableInventorySplitByPickedDetailsCollection GetCollectionToTest()
		{
			var availableInventory = new WhsPickAvailableInventory(Factory);
			return new WhsPickAvailableInventorySplitByPickedDetailsCollection(Factory, availableInventory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new WhsPickAvailableInventorySplitByPickedDetails(Factory);

		#endregion
	}
}
