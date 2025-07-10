using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsActiveBusinessObjectCollectionTestCaseWithHelper<T> : WhsActiveBusinessObjectCollectionTestCase<T> where T : IActiveBusinessObjectCollection
	{
		protected new WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;
	}
}
