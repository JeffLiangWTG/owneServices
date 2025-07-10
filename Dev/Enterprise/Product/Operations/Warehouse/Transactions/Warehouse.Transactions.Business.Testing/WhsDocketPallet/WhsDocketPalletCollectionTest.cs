using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketPalletCollection))]
	class WhsDocketPalletCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsDocketPalletCollection>
	{
		protected override WhsDocketPalletCollection GetCollectionToTest()
		{
			var master = Factory.New<WhsReceive>();
			return new WhsDocketPalletCollection(master, Factory);
		}

		public void TestAllowNew()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var collection = new WhsDocketPalletCollection(receive, Factory);
			AssertEquals("Receive should allow Pallets to be added by default.", true, ((IBindingList)collection).AllowNew);

			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;
			AssertEquals("Pick by BOM Receive should not allow Pallets to be added.", false, ((IBindingList)collection).AllowNew);
		}
	}
}
