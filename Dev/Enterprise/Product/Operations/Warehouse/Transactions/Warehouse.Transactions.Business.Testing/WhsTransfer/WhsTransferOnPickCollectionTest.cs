using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferOnPickCollection))]
	class WhsTransferOnPickCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsTransferOnPickCollection>
	{
		#region TestCollection

		public void TestCollection()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "W1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew();
			var collection = new WhsTransferOnPickCollection(pick);
			AssertEquals("Precondition: Empty.", false, collection.Count > 0);

			pick.Orders.Add(order);
			AssertEquals("Precondition: Order shouldn't be included in the collection.", false, collection.Count > 0);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer1.WD_WP_ParentPickForTransfer = pick.PK;

			AssertContainsExactElementsInAnyOrder("Should only contain transfer1.", transfer1, collection);

			collection.Add(transfer2);
			AssertContainsExactElementsInAnyOrder("Should contain both transfers.", new[] { transfer1, transfer2 },
				collection);
			AssertEquals("Should have set WD_WP_ParentPickForTransfer on transfer2.",
				transfer2.WD_WP_ParentPickForTransfer, pick.PK);

			collection.RemoveFromRelationship(transfer1);
			AssertContainsExactElementsInAnyOrder("Should only contain transfer2.", transfer2, collection);
			AssertEquals("Should have cleared WD_WP_ParentPickForTransfer on transfer1.",
				transfer2.WD_WP_ParentPickForTransfer, pick.PK);
		}

		#endregion

		#region Implementation

		#region GetCollectionToTest

		protected override WhsTransferOnPickCollection GetCollectionToTest()
		{
			return new WhsTransferOnPickCollection(Helper.CreatePickNew());
		}

		#endregion

		#region Helper

		protected new WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
