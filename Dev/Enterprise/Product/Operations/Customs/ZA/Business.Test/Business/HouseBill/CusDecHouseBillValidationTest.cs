using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusDecHouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestMasterBillValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.Bills.AddNew();
			declaration.PrimaryMasterBill.CU_BillNum = "";
			AssertHasMessageErrors(declaration.PrimaryMasterBill.CU_BillNumInfo);
			declaration.PrimaryMasterBill.CU_BillNum = "123";
			AssertNoMessageErrors(declaration.PrimaryMasterBill.CU_BillNumInfo);
		}

		public void TestPackTypeValidationIsDisabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_PackType = "XYZ";
			AssertNoMessageErrors(declaration.PrimaryMasterBill.CU_PackTypeInfo);
			bill.CU_PackType = ZString.Empty;
			AssertNoMessageErrors(declaration.PrimaryMasterBill.CU_PackTypeInfo);
		}
	}
}
