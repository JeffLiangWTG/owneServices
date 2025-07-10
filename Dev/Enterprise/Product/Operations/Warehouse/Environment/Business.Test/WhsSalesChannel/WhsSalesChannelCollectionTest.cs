using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsSalesChannelCollection))]
	class WhsSalesChannelCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsSalesChannelCollection>
	{
		protected override WhsSalesChannelCollection GetCollectionToTest()
		{
			return new WhsSalesChannelCollection(Factory);
		}

		public void TestIWhsSalesChannelCollection()
		{
			var collection = new WhsSalesChannelCollection(Factory);
			((IWhsSalesChannelCollection)collection).AddNew();
			AssertEquals(1, collection.Count);
		}
	}
}
