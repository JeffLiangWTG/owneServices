using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class OrderItemCollectionTest : TestCaseWithFactory
	{
		public void TestOrderReferenceChangedEvent()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			OrderItemCollection items = new OrderItemCollection(cartage, Factory);

			bool orderReferenceChangedCalled;
			items.OrderReferenceChanged += (s, e) => orderReferenceChangedCalled = true;

			orderReferenceChangedCalled = false;
			OrderItem item = items.AddNew();
			AssertEquals("Item added, this is counted as a change", true, orderReferenceChangedCalled);

			orderReferenceChangedCalled = false;
			item.JT_OrderReference = "xxx";
			AssertEquals("Item changed, this is counted as a change", true, orderReferenceChangedCalled);

			orderReferenceChangedCalled = false;
			item.Delete();
			AssertEquals("Item deleted, this is counted as a change", true, orderReferenceChangedCalled);
		}

		public void TestIndexer_OrderReference()
		{
			var cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			var items = new OrderItemCollection(cartage, Factory);
			var order1 = items.AddNew();
			order1.JT_OrderReference = "ORDER1";
			var order2 = items.AddNew();
			order2.JT_OrderReference = "ORDER2";
			var order3 = items.AddNew();
			order3.JT_OrderReference = "ORDER3";
			AssertEquals(order1, items["ORDER1"]);
			AssertEquals(order2, items["ORDER2"]);
			AssertEquals(order3, items["ORDER3"]);
			AssertNull("No ORDER4", items["ORDER4"]);
		}
	}
}
