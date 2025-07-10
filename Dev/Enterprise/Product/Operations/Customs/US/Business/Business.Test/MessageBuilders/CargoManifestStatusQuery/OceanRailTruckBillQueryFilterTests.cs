using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class OceanRailTruckBillQueryFilterTests : TestCaseWithFactory
	{
		public void TestRequestForRelatedBOL()
		{
			var filter = new OceanRailTruckBillQueryFilter();
			Assert("RequestForRelatedBOL flag should be set to true for OceanRailTruck bill messages", filter.RequestForRelatedBOL);
		}

		public void TestFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var filter = new OceanRailTruckBillQueryFilter();
			Assert("Master bill query should be sent", filter.Filter(masterBill));
			Assert("House bill query should not be sent if", !filter.Filter(houseBill));
		}
	}
}
