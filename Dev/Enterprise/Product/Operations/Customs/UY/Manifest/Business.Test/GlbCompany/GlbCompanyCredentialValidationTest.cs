using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialValidation))]
	public class GlbCompanyCredentialValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
	{
		public void TestCheckGP_UserID()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Factory.New<GlbCompany>()).GlbExternalPassword;

			credential.GP_UserID = ZString.Empty;
			AssertHasMessageError(credential.GP_UserIDInfo, "You have not entered a value.");

			credential.GP_UserID = "12345678";
			AssertNoNotifications(credential.GP_UserIDInfo);
			Assert(!credential.GP_UserIDInfo.HasMessageError("Username must be less than 9 characters."));
		}

		public void TestCheckCurrentDecryptedPassword()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Factory.New<GlbCompany>()).GlbExternalPassword;

			credential.CurrentDecryptedPassword = ZString.Empty;
			AssertHasMessageError(credential.CurrentDecryptedPasswordInfo, "You have not entered a value.");

			credential.CurrentDecryptedPassword = "123456789012345";
			AssertNoNotifications(credential.CurrentDecryptedPasswordInfo);
			Assert(!credential.CurrentDecryptedPasswordInfo.HasMessageError("Password must be less than 16 characters."));
		}

		public void TestCheckCurrentDecryptedCertificatePassphrase()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Factory.New<GlbCompany>()).GlbExternalPassword;

			credential.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertHasMessageError(credential.CurrentDecryptedCertificatePassphraseInfo, "You have not entered a value.");

			credential.CurrentDecryptedCertificatePassphrase = "1234567890";
			AssertNoNotifications(credential.CurrentDecryptedCertificatePassphraseInfo);
			Assert(!credential.CurrentDecryptedCertificatePassphraseInfo.HasMessageError("Password must be less than 11 characters."));
		}

		protected override bool IsCertificateMandatory => false;
	}
}
