using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(GlbExternalPasswordValidation_PL))]
sealed class GlbExternalPasswordValidation_PLTest : GlbExternalPasswordWithCertificateValidationTest<GlbExternalPassword_PL, GlbExternalPasswordValidation_PL>
{
	public void TestCheckGP_ExpiryDate()
	{
		var staff = Factory.New<GlbStaff>();
		var wrapper = GlbStaffWrapper.Get(staff);
		var password = wrapper.GlbExternalPassword;

		password.GP_ExpiryDate = ZDateTime.Now.AddDays(-3);

		AssertHasError(password.GP_ExpiryDateInfo, "This certificate is not valid – the expiry date is in the past.");
		AssertNoWarning(password.GP_ExpiryDateInfo, "This certificate will expire soon – the expiry date is within one month.");

		password.GP_ExpiryDate = ZDateTime.Now.AddDays(3);

		AssertNoError(password.GP_ExpiryDateInfo, "This certificate is not valid – the expiry date is in the past.");
		AssertHasWarning(password.GP_ExpiryDateInfo, "This certificate will expire soon – the expiry date is within one month.");

		password.GP_ExpiryDate = ZDateTime.Now.AddDays(36);

		AssertNoError(password.GP_ExpiryDateInfo, "This certificate is not valid – the expiry date is in the past.");
		AssertNoWarning(password.GP_ExpiryDateInfo, "This certificate will expire soon – the expiry date is within one month.");
	}

	public void Test_CheckCurrentDecryptedCertificatePassphrase()
	{
		var password = Factory.NewWithValidTestData<GlbExternalPassword_PL>();

		password.CurrentDecryptedCertificatePassphrase = "";
		AssertNoError(password.GP_ExpiryDateInfo, "This password is not valid for specified certificate.");

		password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		password.CurrentDecryptedCertificatePassphrase = "123";
		AssertHasError(password.CurrentDecryptedCertificatePassphraseInfo, "This password is not valid for specified certificate.");

		password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		AssertNoError(password.CurrentDecryptedCertificatePassphraseInfo, "This password is not valid for specified certificate.");
	}

	public void TestCheckCurrentDecryptedPassword()
	{
		var password = Factory.New<GlbExternalPassword_PL>();
		var passwordForLoginMessage = "The password is required for the login.";

		password.CurrentDecryptedPassword = ZString.Empty;
		password.GP_MailBoxID = ZString.Empty;

		CombineAssertions(() =>
		{
			password.Validation.ValidateCurrentDecryptedPassword();
			AssertNoError("Empty login and empty password", password.CurrentDecryptedPasswordInfo, passwordForLoginMessage);
			password.GP_MailBoxID = "Login";
			password.Validation.ValidateCurrentDecryptedPassword();
			AssertHasError("Not empty login and empty password", password.CurrentDecryptedPasswordInfo, passwordForLoginMessage);
			password.CurrentDecryptedPassword = "Password";
			password.Validation.ValidateCurrentDecryptedPassword();
			AssertNoError("Not empty login and not empty password", password.CurrentDecryptedPasswordInfo, passwordForLoginMessage);
		});
	}

	public void TestCheckGP_Certificate_CertForLogin()
	{
		var password = Factory.New<GlbExternalPassword_PL>();
		var certificateForLoginMessage = "A certificate is required for sending electronic declarations to PUESC.";
		CombineAssertions(() =>
		{
			password.GP_Certificate = null;
			AssertNoMessageError("Empty login and empty certificate", password.GP_CertificateInfo, certificateForLoginMessage);
			password.GP_MailBoxID = "Login";
			password.GP_Certificate = null;
			AssertHasMessageError("Not empty login and empty certificate", password.GP_CertificateInfo, certificateForLoginMessage);
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			AssertNoMessageError("Not empty login and not empty certificate", password.GP_CertificateInfo, certificateForLoginMessage);
		});
	}

	[TestDate(year: 2025, month: 1, day: 10)]
	public void TestValidateGP_Certificate_RevocationServerOffline()
	{
		GlbExternalPassword.GP_Certificate = TestPkcs12Certificate.Value;
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = TestPkcs12Certificate.Password;
		CombineAssertions(() =>
		{
			AssertHasWarningContaining(GlbExternalPassword.GP_CertificateInfo, "The revocation function was unable to check revocation because the revocation server was offline.");
			AssertHasWarningContaining(GlbExternalPassword.GP_CertificateInfo, "The revocation function was unable to check revocation for the certificate.");
		});
	}

	[TestDate(year: 2026, month: 1, day: 10)]
	public void TestValidateGP_Certificate_Expired()
	{
		GlbExternalPassword.GP_Certificate = TestPkcs12Certificate.Value;
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = TestPkcs12Certificate.Password;
		AssertHasWarningContaining(GlbExternalPassword.GP_CertificateInfo, "The certificate has expired. Please create a new certificate.");
	}

	protected override bool IsCertificateMandatory => false;
}
