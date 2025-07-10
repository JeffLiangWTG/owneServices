using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderHeaderCollection))]
	class MNRWorkOrderHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<MNRWorkOrderHeaderCollection>
	{
		protected override MNRWorkOrderHeaderCollection GetCollectionToTest()
		{
			return new MNRWorkOrderHeaderCollection(Factory);
		}

		public void TestInitCollection()
		{
			var collection = new MNRWorkOrderHeaderCollection(Factory);
			AssertEquals("Precondition: collection count", 0, collection.Count);
		}
	}
}
