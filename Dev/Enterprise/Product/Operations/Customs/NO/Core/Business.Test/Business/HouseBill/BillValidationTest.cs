using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class BillValidationTest : BusinessObjectValidationTestCase
	{
		JobDeclaration declaration;
		Bill bill;

		public void TestCheckCU_BillNum()
		{
			declaration.JE_MasterBill = "111111";
			AssertNoMessageErrors(bill.CU_BillNumInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			bill = declaration.Bills.AddNew();
		}
	}
}
