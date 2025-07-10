using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsJobServiceDependentCollection))]
	public class WhsDocketDependentCollectionTest : BusinessObjectCollectionTestCaseBase<WhsOrder>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new WhsJobServiceDependentCollection(Factory.New<WhsOrder>(), Factory);

		public void TestAllowNew()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var collection = new WhsJobServiceDependentCollection(receive, Factory);
			AssertEquals("Receive should allow Services to be added by default.", true, collection.AllowNew);

			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;
			AssertEquals("Pick by BOM Receive should not allow Services to be added.", false, collection.AllowNew);
		}
	}
}
