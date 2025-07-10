namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SEBillTypesListTest : NUnit.Framework.TestCase
	{
		public void TestGetBillTypeIndicator()
		{
			AssertEquals(SEBillTypesList.Codes.MasterBill, SEBillTypesList.GetBillTypeIndicator(Customs.Business.BillTypeList.Codes.MasterBill));
			AssertEquals(SEBillTypesList.Codes.HouseBill, SEBillTypesList.GetBillTypeIndicator(Customs.Business.BillTypeList.Codes.HouseBill));
			AssertEquals(SEBillTypesList.Codes.SubHouseBill, SEBillTypesList.GetBillTypeIndicator(Customs.Business.BillTypeList.Codes.SubHouseBill));
		}
	}
}
