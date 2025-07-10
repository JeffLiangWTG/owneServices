using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	public class GlbCompanyCredentialTest : GlbExternalPasswordWithCertificateTest<GlbCompanyCredential>
	{
		protected override GlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.New<GlbCompanyCredential>();
		}

		public override void TestSetDefaultValues()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(PasswordTypesList.Codes.UTB, credential.GP_PasswordType);
			AssertNotEquals(ZGuid.Empty, credential.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
		}

		public void TestPasswordStatus()
		{
			GlbExternalPassword.GP_PasswordStatus = "VAL";
			AssertEquals("Valid", GlbExternalPassword.PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = "INV";
			AssertEquals("Invalid", GlbExternalPassword.PasswordStatus);
		}

		public override void TestReadOnly()
		{
			AssertEquals("GP_PasswordStatus.ReadOnly", true, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);
			AssertEquals("GP_StatusReason.ReadOnly", true, GlbExternalPassword.GP_StatusReasonInfo.ReadOnly);
			AssertEquals("GP_UserIDInfo.ReadOnly", false, GlbExternalPassword.GP_UserIDInfo.ReadOnly);
		}

		public void TestGP_UserIDMaxLength()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(8, credential.GP_UserIDInfo.MaxLength);
		}

		public void TestCurrentDecryptedPasswordMaxLength()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(15, credential.CurrentDecryptedPasswordInfo.MaxLength);
		}

		public void TestCurrentDecryptedCertificatePassphraseMaxLength()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(30, credential.CurrentDecryptedCertificatePassphraseInfo.MaxLength);
		}

		public override void TestDataDefaultFromCertificate()
		{
			void SetValidCertificate()
			{
				GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			}
			var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
			SetValidCertificate();
			AssertEquals("Expiry Date has been defaulted from valid certificate", validCertificate.NotAfter, GlbExternalPassword.GP_ExpiryDate);

			SetValidCertificate();
			var invalidCertificate = new byte[] { 1, 2, 3, 4 };
			GlbExternalPassword.GP_Certificate = invalidCertificate;
			AssertEquals("Expiry Date has been cleared (invalid certificate data)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);

			SetValidCertificate();
			var invalidPassword = "123456";
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = invalidPassword;
			AssertEquals("Expiry Date has been cleared (invalid certificate data - password)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);
		}

		public void TestGetMessageAttrDictionary()
		{
			var credential = Business.GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			GlbCompany.CurrentCompany.Factory.Save();

			var dictionary = credential.GetMessageAttrDictionary();

			string value;
			dictionary.TryGetValue("httpclient.user", out value);
			AssertEquals("user", credential.GP_UserID, value);
		}

		public void TestShouldSendCredential()
		{
			var credential = Factory.New<GlbCompanyCredentialForTest>();
			AssertEquals("ShouldSendCredential", false, credential.ShouldSendCredentialExposed());
		}
	}

	class GlbCompanyCredentialForTest : GlbCompanyCredential
	{
		public GlbCompanyCredentialForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldSendCredentialExposed() => ShouldSendCredential();
	}
}
