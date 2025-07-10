using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class MessagePreSendingValidationBaseTest<T> : TestCaseWithFactory
	where T : MessagePreSendingValidationBase
{
	public void TestValidate_MissingCredentials()
	{
		const string expectedMessage = "PUESC Credentials are not set. Please, ensure that 'PUESC login', 'Password', 'PLB Certificate' and 'Certificate Password' are configured for current Staff member.";
		var certificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		CombineAssertions(() =>
		{
			validation.Validate(notifications);
			Assert("Certificate is missing", notifications.Contains(expectedMessage));

			notifications.Clear();
			certificate.GP_MailBoxID = "UserName";
			validation.Validate(notifications);
			Assert("Email is set, Certificate is missing", notifications.Contains(expectedMessage));

			notifications.Clear();
			certificate.GP_MailBoxID = "";
			certificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			certificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			validation.Validate(notifications);
			Assert("Email is missing, Certificate is set", notifications.Contains(expectedMessage));

			notifications.Clear();
			certificate.GP_MailBoxID = "UserName";
			validation.Validate(notifications);
			Assert("Email is set, Certificate is set", !notifications.Contains(expectedMessage));
		});
	}

	[TestDate(2022, 9, 12)]
	public void TestValidate_ExpiredCertificate()
	{
		const string errorMessage = "The certificate has expired, add a new certificate before sending customs message.";
		var certificate = SetCredentials();
		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		CombineAssertions(() =>
		{
			certificate.GP_ExpiryDate = new DateTime(2022, 9, 5);
			validation.Validate(notifications);
			Assert("Certificate is expired", notifications.Contains(errorMessage));

			notifications.Clear();
			certificate.GP_ExpiryDate = ZDateTime.Empty;
			validation.Validate(notifications);
			Assert("Certificate has no expiration", !notifications.Contains(errorMessage));

			notifications.Clear();
			certificate.GP_ExpiryDate = new DateTime(2022, 9, 19);
			validation.Validate(notifications);
			Assert("Certificate is not expired", !notifications.Contains(errorMessage));
		});
	}

	[TestDate(year: 2025, month: 1, day: 10)]
	public void TestValidate_CertificateChainIsInvalid()
	{
		const string errorMessage = "PUESC certificate chain is not valid";
		SetCredentials(TestPkcs12Certificate.Value, TestPkcs12Certificate.Password);
		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		validation.Validate(notifications);
		Assert("Certificate chain is not valid", notifications.ContainsNotificationContaining(errorMessage));
	}

	protected static IGlbExternalPasswordWithCertificate SetCredentials()
		=> SetCredentials(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);

	static IGlbExternalPasswordWithCertificate SetCredentials(ZBlob pkcs12Certificate, ZString password)
	{
		var certificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		certificate.GP_MailBoxID = "ABCXYZ";
		certificate.GP_Certificate = pkcs12Certificate;
		certificate.CurrentDecryptedCertificatePassphrase = password;
		certificate.GP_ExpiryDate = ZDateTime.Now.AddDays(7);
		return certificate;
	}

	protected abstract T GetInstanceForTest();
}
