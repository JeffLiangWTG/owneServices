using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingBranchRegister))]
	public class EInvoicingBranchRegisterTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			return new EInvoicingBranchRegister(branch);
		}

		#endregion

		#region Tests

		public void TestOTP()
		{
			var register = GetNewBusinessObject() as EInvoicingBranchRegister;
			Assert(register.OTP.IsEmpty);
			register.OTP = "1234";
			AssertEquals("1234", register.OTP);
		}

		public void TestProgressLog()
		{
			var register = GetNewBusinessObject() as EInvoicingBranchRegister;
			Assert(register.ProgressLog.IsEmpty);
			register.ProgressLog = "some log here";
			AssertEquals("some log here", register.ProgressLog);
		}

		public void TestValidationType()
		{
			var register = GetNewBusinessObject() as EInvoicingBranchRegister;
			Assert(register.Validation is EInvoicingBranchRegisterValidation);
		}

		#endregion
	}
}
