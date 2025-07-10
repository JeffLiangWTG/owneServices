using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class OrderDataTest : TestCaseWithFactory
	{
		public void TestGetBusinessObjectCollection()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXX_TST_1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "XXX_TST_2";

			var order1 = Factory.New<Order>();
			order1.BuyerPK = org1.PK;

			var order2 = Factory.New<Order>();
			order2.BuyerPK = org2.PK;

			Factory.Save();

			IBusinessObjectCollection collection = new OrderData().GetBusinessObjectCollection(new BusinessObjectFactory());

			AssertEquals(2, collection.Count);
			AssertNotNull(collection.FindByPK(order1.PK));
			AssertNotNull(collection.FindByPK(order2.PK));
		}

		public void TestGetBusinessObjectCollectionWithParameter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXX_TST_1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "XXX_TST_2";

			var order1 = Factory.New<Order>();
			order1.BuyerPK = org1.PK;

			var order2 = Factory.New<Order>();
			order2.BuyerPK = org2.PK;

			Factory.Save();

			IBusinessObjectCollection collection = new OrderData().GetBusinessObjectCollection(new BusinessObjectFactory(), new AssemblyDataParams { CompanyCode = "XXX_TST_2" });

			AssertEquals(1, collection.Count);
			AssertNull(collection.FindByPK(order1.PK));
			AssertNotNull(collection.FindByPK(order2.PK));
		}
	}
}
