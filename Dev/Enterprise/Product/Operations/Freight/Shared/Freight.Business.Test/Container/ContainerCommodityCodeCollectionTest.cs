using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class ContainerCommodityCodeCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<T>
			where T : ContainerCommodityCodeCollection
	{
		public void TestSetFilterBusinessObjectDefaults()
		{
			var collection = GetCollectionToTest();
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Commodity Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
		}

		public abstract void TestSetDefaultsForNewElementCore();

		public abstract void TestCreateRelationshipFilter();

		protected override T GetCollectionToTest()
		{
			return GetNewCollectionCore();
		}

		protected abstract T GetNewCollectionCore();
	}
}
