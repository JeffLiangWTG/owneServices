using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test.DeliveryHeader
{
	[TestedType(typeof(CYDAdHocServiceOrderCollection))]
	class CYDAdHocServiceOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDAdHocServiceOrderCollection>
	{
		protected override CYDAdHocServiceOrderCollection GetCollectionToTest()
		{
			return new CYDAdHocServiceOrderCollection(Factory);
		}

		public void TestInitCollection()
		{
			var collection = new CYDAdHocServiceOrderCollection(Factory);
			AssertEquals("Precondition: collection count", 0, collection.Count);
		}
	}
}
