using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class MainFormGenericConsolCollectionBOCollectionTest<T> : BusinessObjectCollectionTestCase where T : BusinessObject
	{
		public abstract void TestIndexer();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<T>();
		}
	}
}
