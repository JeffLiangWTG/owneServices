using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountLocationVarianceCollection))]
	class WhsCycleCountLocationVarianceCollectionTest : WhsActiveBusinessObjectCollectionTestCaseWithHelper<WhsCycleCountLocationVarianceCollection>
	{
		public void TestCycleCount()
		{
			var collection = GetCollectionToTest();
			Assert("Cycle Count == Master in base",
				object.ReferenceEquals(collection.Relationship.Master, collection.CycleCount));
		}

		protected override WhsCycleCountLocationVarianceCollection GetCollectionToTest()
		{
			return new WhsCycleCountLocationVarianceCollection(Factory.New<WhsCycleCountLocation>());
		}
	}
}
