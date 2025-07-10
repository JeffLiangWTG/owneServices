using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AllocationAdjustmentCollection))]
	sealed class AllocationAdjustmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AllocationAdjustmentCollection>
	{
		public void TestAllowNew()
		{
			AllocationAdjustmentCollection collection = new AllocationAdjustmentCollection();
			AssertEquals(false, collection.AllowNew);
		}

		protected override AllocationAdjustmentCollection GetCollectionToTest()
		{
			return new AllocationAdjustmentCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AllocationAdjustment(Factory.New<ForwardingConsol>());
		}
	}
}
