using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPutawayGroupCollection))]
	class WhsPutawayGroupCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsPutawayGroupCollection>
	{
		protected override WhsPutawayGroupCollection GetCollectionToTest()
		{
			return new WhsPutawayGroupCollection(Factory);
		}

		public void TestIWhsPutawayGroupCollection()
		{
			var collection = new WhsPutawayGroupCollection(Factory);
			((IWhsPutawayGroupCollection)collection).AddNew();
			AssertEquals(1, collection.Count);
		}
	}
}
