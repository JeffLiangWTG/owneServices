using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialValidation))]
	sealed class GlbCompanyCredentialValidationTest : TW.Business.Testing.GlbExternalPasswordValidationBase_TWTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
	{
		public override void TestCheckGP_PasswordType()
		{
			var sub = GlbExternalPassword;
			sub.GP_PasswordType = ZString.Empty;
			AssertHasErrorContaining(sub.GP_PasswordTypeInfo, MandatoryValidation.MustBeEntered);

			sub.GP_PasswordType = "AAA";
			AssertNoErrorContaining(sub.GP_PasswordTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasError("GP_PasswordType", sub.GP_PasswordTypeInfo, SelectionMustBeValid);

			sub.GP_PasswordType = "TVF";
			AssertNoError("GP_PasswordType", sub.GP_PasswordTypeInfo, SelectionMustBeValid);
		}

		public void TestCheckGP_UserID()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credential.GP_UserID = "abcd";
			AssertNoErrorContaining(credential.GP_UserIDInfo, MandatoryValidation.MustBeEntered);

			credential.GP_UserIDInfo.ClearValue();
			AssertHasErrorContaining(credential.GP_UserIDInfo, MandatoryValidation.MustBeEntered);

			credential.GP_CertificateInfo.ClearValue();
			credential.Validation.ValidateGP_UserID();
			AssertNoErrorContaining(credential.GP_UserIDInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckGP_MailBoxID()
		{
			var errorMessage = "The entered Mailbox is invalid, the Mail Box format must be 7 characters.";
			var credential = Factory.New<GlbCompanyCredential>();

			credential.GP_MailBoxID = ZString.Empty;
			AssertNoErrorContaining(credential.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);

			credential.GP_UserID = "123";
			credential.GP_MailBoxID = ZString.Empty;
			AssertHasErrorContaining(credential.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);

			credential.GP_MailBoxID = "111";
			AssertNoErrorContaining(credential.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(credential.GP_MailBoxIDInfo, errorMessage);

			credential.GP_MailBoxID = "CBK0123-A";
			AssertHasErrorContaining(credential.GP_MailBoxIDInfo, errorMessage);

			credential.GP_MailBoxID = "CBKZ012";
			AssertNoErrorContaining(credential.GP_MailBoxIDInfo, errorMessage);

			credential.GP_MailBoxID = "CBKZ0123";
			AssertNoErrorContaining(credential.GP_MailBoxIDInfo, errorMessage);
		}

		public override void TestCheckCurrentDecryptedPassword()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			credential.CurrentDecryptedPassword = ZString.Empty;
			AssertNoErrorContaining(credential.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);

			credential.GP_UserID = "123";
			credential.CurrentDecryptedPassword = ZString.Empty;
			AssertHasErrorContaining(credential.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);

			credential.CurrentDecryptedPassword = "password";
			AssertNoErrorContaining(credential.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCurrentDecryptedCertificatePassphrase()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			credential.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertNoErrorContaining(credential.CurrentDecryptedCertificatePassphraseInfo, MandatoryValidation.MustBeEntered);

			credential.GP_Certificate = new byte[] { 120, 200 };
			credential.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertHasErrorContaining(credential.CurrentDecryptedCertificatePassphraseInfo, MandatoryValidation.MustBeEntered);

			credential.CurrentDecryptedCertificatePassphrase = "password";
			AssertNoErrorContaining(credential.CurrentDecryptedCertificatePassphraseInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckGP_CertificateWhenEmpty()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.GP_Certificate = ZBlob.Empty;
			AssertNoErrorContaining(credential.GP_CertificateInfo, MandatoryValidation.MustBeEntered);
			credential.GP_UserID = "123";
			credential.GP_Certificate = ZBlob.Empty;
			AssertHasErrorContaining(credential.GP_CertificateInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
