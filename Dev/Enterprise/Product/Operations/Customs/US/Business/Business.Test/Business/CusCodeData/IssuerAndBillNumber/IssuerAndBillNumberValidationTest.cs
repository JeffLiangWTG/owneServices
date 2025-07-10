using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IssuerAndBillNumberValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			var bizObj = Factory.New<IssuerAndBillNumber>();
			bizObj.RunPreSaveValidation();
			AssertNoErrors(bizObj.CY_CodeInfo);
		}
	}
}
