using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class BusinessObjectCollectionTestCaseBase<T> : BusinessObjectCollectionTestCase
			where T : BusinessObject
	{
		protected abstract override BusinessObjectCollection GetCollectionToTest();

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<WhsJobService>();
	}
}
