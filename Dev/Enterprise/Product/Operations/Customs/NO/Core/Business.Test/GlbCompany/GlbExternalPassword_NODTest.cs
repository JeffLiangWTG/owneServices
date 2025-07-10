using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(GlbExternalPassword_NOD))]
sealed class GlbExternalPassword_NODTest : GlbExternalPasswordWithCertificateTest<GlbExternalPassword_NOD>
{
	public void TestGP_UserID_Attributes() => CombineAssertions(() =>
		AssertEntity<GlbExternalPassword_NOD>()
			.HasProperty(x => x.GP_UserID)
			.WithCaption("Client id"));

	public void TestCurrentDecryptedCertificatePassphrase_Attributes() => CombineAssertions(() =>
		AssertEntity<GlbExternalPassword_NOD>()
			.HasProperty(x => x.CurrentDecryptedCertificatePassphrase)
			.WithCaption("Password"));

	public void TestGP_PasswordStatus_Attributes() => CombineAssertions(() =>
		AssertEntity<GlbExternalPassword_NOD>()
			.HasProperty(x => x.GP_PasswordStatus)
			.WithCaption("Status")
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly));

	public void TestGP_ExpiryDate_Attributes() => CombineAssertions(() =>
		AssertEntity<GlbExternalPassword_NOD>()
			.HasProperty(x => x.GP_ExpiryDate)
			.WithCaption("Expiry Date")
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly));

	public void TestDefaultValues() => CombineAssertions(() =>
	{
		var credential = Factory.New<GlbExternalPassword_NOD>();
		AssertEquals("GP_PasswordType should be 'NOD'", PasswordTypesList.Codes.NOD, credential.GP_PasswordType);
		AssertNotEquals("GP_GC should not be empty", ZGuid.Empty, credential.GP_GC);
		AssertEquals("GP_PasswordStatus should be 'INV'", PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
	});

	public override void TestDataDefaultFromCertificate() => CombineAssertions(() =>
	{
		var invalidCertificate = new byte[] { 1, 2, 3, 4 };
		GlbExternalPassword.GP_Certificate = invalidCertificate;
		AssertEquals("Expiry Date has been cleared (invalid certificate data)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);

		SetValidCertificate();
		AssertEquals("Issue Date has been defaulted from valid certificate", validCertificate.NotBefore, GlbExternalPassword.GP_IssueDate);
		AssertEquals("Expiry Date has been defaulted from valid certificate", validCertificate.NotAfter, GlbExternalPassword.GP_ExpiryDate);

		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = invalidPassword;
		AssertEquals("Expiry Date has been cleared (invalid certificate data - password)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);
	});

	public void TestGP_PasswordStatus() => CombineAssertions(() =>
	{
		SetValidCertificate();
		AssertEquals("Password is valid", PasswordStatusList.Codes.Valid, GlbExternalPassword.GP_PasswordStatus);

		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = invalidPassword;
		AssertEquals("Password is invalid", PasswordStatusList.Codes.Invalid, GlbExternalPassword.GP_PasswordStatus);
	});

	public void TestGP_GC()
	{
		SetValidCertificate();
		AssertEquals("Company is valid", GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
	}

	public void TestGetMessageAttrDictionary() => CombineAssertions(() =>
	{
		var wrapper = new GlbCompanyWrapper(GlbCompany.CurrentCompany);

		var credential = wrapper.Credential;
		credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var dictionary = (credential as IxTMessageAttributeProvider).GetMessageAttrDictionary();

		dictionary.TryGetValue("cw1.key", out var value);
		AssertEquals("key", X509Certificate2TestHelper.ValidCertificate_KeyPEM, value);

		dictionary.TryGetValue("cw1.certificate", out value);
		AssertEquals("certificate", X509Certificate2TestHelper.ValidCertificate_CertPEM, value);
	});

	void SetValidCertificate()
	{
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
	}

	protected override void SetUp()
	{
		base.SetUp();
		validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
	}

	X509Certificate2 validCertificate;
	readonly ZString invalidPassword = "1234567890";

	protected override GlbExternalPassword_NOD CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<GlbExternalPassword_NOD>();
}
