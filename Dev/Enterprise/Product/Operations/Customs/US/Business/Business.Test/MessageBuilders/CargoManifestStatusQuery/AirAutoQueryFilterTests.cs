using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AirAutoQueryFilterTests : TestCaseWithFactory
	{
		public void TestRequestForRelatedBOL()
		{
			var filter = new AirAutoQueryFilter();
			Assert("RequestForRelatedBOL flag should be set to false for Air bill messages", !filter.RequestForRelatedBOL);
		}

		public void TestFilterWhenHouseBillExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var filter = new AirAutoQueryFilter();
			Assert("House bill query should be sent", filter.Filter(houseBill));
			Assert("Master bill query should not be sent if a House bill exists", !filter.Filter(masterBill));
		}

		public void TestFilterWhenNoHouseBillExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var filter = new AirAutoQueryFilter();
			Assert("Master bill query should not be sent if no House bill exists", filter.Filter(masterBill));
		}

		public void TestFilterSomeMasterBillsHaveHouseBillsAndOthersNot()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "m1";
			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "m2";
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "h1";
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_ParentBillUniqueCode = masterBill2.CU_BillUniqueCode;
			houseBill2.CU_BillNum = "h2";

			var filter = new AirAutoQueryFilter();
			Assert("Master bill query should be sent if no child House bill exists", filter.Filter(masterBill1));
			Assert("Master bill query should not be sent if a child House bill exists", !filter.Filter(masterBill2));
			Assert("House bill query should be sent", filter.Filter(houseBill1));
			Assert("House bill query should be sent", filter.Filter(houseBill2));
		}
	}
}
