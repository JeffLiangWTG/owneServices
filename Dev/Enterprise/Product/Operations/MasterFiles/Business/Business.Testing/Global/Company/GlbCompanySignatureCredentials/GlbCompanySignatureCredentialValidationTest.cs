using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanySignatureCredentialValidation))]
	sealed class GlbCompanySignatureCredentialsValidationTest : GlbExternalPasswordValidationTest<GlbCompanySignatureCredential, GlbCompanySignatureCredentialValidation>
	{
		public void TestValidateUserID()
		{
			GlbExternalPassword.GP_UserID = ZString.Empty;
			AssertHasError(GlbExternalPassword.GP_UserIDInfo, "Please enter a Credential User ID.");

			GlbExternalPassword.GP_UserID = "XXX";
			AssertNoError(GlbExternalPassword.GP_UserIDInfo, "Please enter a Credential User ID.");
		}

		public void TestValidateCurrentDecryptedPassword()
		{
			GlbExternalPassword.CurrentDecryptedPassword = ZString.Empty;
			AssertHasError(GlbExternalPassword.CurrentDecryptedPasswordInfo, "Please enter a Credential Password.");

			GlbExternalPassword.CurrentDecryptedPassword = "XXX";
			AssertNoError(GlbExternalPassword.CurrentDecryptedPasswordInfo, "Please enter a Credential Password.");
		}
	}
}
