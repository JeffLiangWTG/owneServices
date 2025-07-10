using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsJobServiceDependentCollection))]
	public class WhsAdHocServiceJobDependentCollectionTest : BusinessObjectCollectionTestCaseBase<WhsAdHocServiceJob>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new WhsJobServiceDependentCollection(Factory.New<WhsAdHocServiceJob>(), Factory);

		public void TestAllowNew()
		{
			AssertEquals(true, GetCollectionToTest().AllowNew);
		}
	}
}
