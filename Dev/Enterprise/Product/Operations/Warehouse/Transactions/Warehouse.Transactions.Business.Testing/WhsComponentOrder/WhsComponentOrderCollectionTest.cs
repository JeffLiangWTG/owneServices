namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderCollectionTest<T> : WhsPickableDocketCollectionTest<T>
		where T : WhsComponentOrderCollection
	{
		#region TestCollectionContainsOnlyWorkOrders

		public void TestCollectionContainsOnlyWorkOrders()
		{
			var workOrder = GetNewElementToAddToTheCollection();
			var order = Factory.New<WhsOrder>();

			AssertCollectionContains(workOrder, Collection);
			AssertCollectionNotContains(order, Collection);
		}

		#endregion
	}
}
