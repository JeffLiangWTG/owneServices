using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class EInvoicingBranchRegisterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOTP()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var register = new EInvoicingBranchRegister(branch);
			Assert(register.OTP.IsEmpty);
			register.Validation.ValidateOTP();
			AssertHasErrors(register.OTPInfo);
			register.OTP = "1234";
			AssertNoErrors(register.OTPInfo);
		}
	}
}
