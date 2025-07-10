using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordWithPasswordTypeValidation))]
	public class GlbExternalPasswordWithPasswordTypeValidationTest : GlbExternalPasswordValidationTest<GlbExternalPasswordWithPasswordType, GlbExternalPasswordWithPasswordTypeValidation>
	{
		public void TestCheckCurrentDecryptedPassword()
		{
			var extPassword = Factory.NewWithValidTestData<DummyGlbExternalPasswordWithPasswordType>();

			extPassword.CurrentDecryptedPassword = ZString.Empty;
			extPassword.GP_UserID = ZString.Empty;
			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.CurrentDecryptedPasswordInfo);

			extPassword.GP_UserID = "login name";
			extPassword.Validation.ValidateAll();
			AssertHasError(extPassword.CurrentDecryptedPasswordInfo, "Please enter a value.");

			extPassword.CurrentDecryptedPassword = "s3Cr3tPa$$w0rd";
			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.CurrentDecryptedPasswordInfo);

			extPassword.GP_UserID = ZString.Empty;
			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.CurrentDecryptedPasswordInfo);
		}

		public void TestCheckGP_UserID()
		{
			var extPassword = Factory.NewWithValidTestData<DummyGlbExternalPasswordWithPasswordType>();

			extPassword.CurrentDecryptedPassword = ZString.Empty;
			extPassword.GP_UserID = ZString.Empty;
			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.GP_UserIDInfo);

			extPassword.GP_UserID = "login name";
			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.GP_UserIDInfo);

			extPassword.CurrentDecryptedPassword = "s3Cr3tPa$$w0rd";
			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.GP_UserIDInfo);

			extPassword.GP_UserID = ZString.Empty;
			extPassword.Validation.ValidateAll();
			AssertHasError(extPassword.GP_UserIDInfo, "Please enter a value.");
		}

		public void TestCheckGP_StatusReason()
		{
			var extPassword = Factory.NewWithValidTestData<DummyGlbExternalPasswordWithPasswordType>();

			extPassword.Validation.ValidateAll();
			AssertNoErrors(extPassword.GP_StatusReasonInfo);

			extPassword.GP_StatusReason = "Error";
			AssertHasWarning(extPassword.GP_StatusReasonInfo, "Please correct the error and re-enter your Current Decrypted Password");
		}

		class DummyGlbExternalPasswordWithPasswordType : GlbExternalPasswordWithPasswordType
		{
			public DummyGlbExternalPasswordWithPasswordType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override string PasswordTypeCode => "TST";

			public override string PasswordTypeDescription => "Test Description";
		}
	}
}
