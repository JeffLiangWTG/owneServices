using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsJobServiceDependentCollection))]
	public class WhsVASOrderDependentCollectionTest : BusinessObjectCollectionTestCaseBase<WhsVASOrder>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new WhsJobServiceDependentCollection(Factory.New<WhsVASOrder>(), Factory);

		public void TestAllowNew()
		{
			AssertEquals(true, GetCollectionToTest().AllowNew);
		}
	}
}
