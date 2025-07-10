using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialICS2))]
	sealed class GlbCompanyCredentialICS2Test : GlbExternalPasswordWithCertificateTest<GlbCompanyCredentialICS2>
	{
		protected override GlbCompanyCredentialICS2 CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<GlbCompanyCredentialICS2>();

		public void TestConfigurationName()
		{
			var credential = Factory.New<GlbCompanyCredentialICS2>();
			AssertEquals("EUICS2Credential", credential.ConfigurationName);
		}

		public override void TestCredentialRecipient()
		{
			var credential = Factory.New<GlbCompanyCredentialICS2>();
			AssertEquals(CredentialRecipient.DirectxT, credential.CredentialRecipient);
		}

		public override void TestSetDefaultValues()
		{
			var credential = Factory.New<GlbCompanyCredentialICS2>();
			AssertEquals(PasswordTypesList.Codes.IC2, credential.GP_PasswordType);
			AssertNotEquals(ZGuid.Empty, credential.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
			AssertEquals("EUICS2Credential", credential.ConfigurationName);
			AssertEquals("IC2", typeof(GlbCompanyCredentialICS2).GetProperty("InterchangeTypeForSending", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(credential));
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
		}

		public override void TestOnlySendCredentialIfNeeded()
		{
			var credential = Factory.NewWithValidTestData<GlbCompanyCredentialICS2>();
			credential.GP_MailBoxID = string.Empty;
			credential.CurrentDecryptedCertificatePassphrase = string.Empty;
			credential.GP_Certificate = Array.Empty<byte>();

			Factory.Save();

			credential.GP_MailBoxID = "ICS22023";
			Assert(credential.ShouldSendCredential());

			credential.ReloadSafe();
			Assert(!credential.ShouldSendCredential());

			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			Assert(credential.ShouldSendCredential());

			credential.ReloadSafe();
			Assert(!credential.ShouldSendCredential());

			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			Assert(credential.ShouldSendCredential());
		}

		public void TestShouldSendAllICS2Credentials()
		{
			ZGuid CreateCredential(string companyCode, string mailBoxID)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = companyCode;

				var credential = Factory.NewWithValidTestData<GlbCompanyCredentialICS2>();
				credential.GP_GC = company.PK;
				credential.GP_MailBoxID = string.Empty;
				credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

				credential.DisableConfigurationToSender = true;

				Factory.Save();

				return credential.PK;
			}

			var credentialPk = CreateCredential("TZ1", "TSTICS2001");
			CreateCredential("TZ3", "TSTICS2003");
			CreateCredential("TZ2", "TSTICS2002");

			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);

			var factory = NewFactory();
			var credential = factory.Load<GlbCompanyCredentialICS2>(credentialPk);
			credential.GP_MailBoxID = "TSTICS2UPG01";

			factory.Save();

			var interchange = ConfigurationTestHelper.GetQueuedDxTConfigurationInterchangeMessages(factory, Customs.XmlCredential.Constants.IdentifierList.IC2).Single();
			CombineAssertions("Should send all ICS2 credentials.", () =>
			{
				var interchangeText = interchange.EI_BodyText;
				AssertContains(@"Name=""EUICS2Credential""", interchangeText);
				AssertContains(@"<Group Type=""System"" Reference=""EDIDAT"">", interchangeText);
				AssertContains(@"<Group Type=""Company"" Reference=""TZ1"">", interchangeText);
				AssertContains(@"<Group Type=""Company"" Reference=""TZ2"">", interchangeText);
				AssertContains(@"<Group Type=""Company"" Reference=""TZ3"">", interchangeText);
				AssertContains(@"<Item Name=""Platform"">IC2</Item>", interchangeText);
				AssertContains($@"<File>{Convert.ToBase64String(X509Certificate2TestHelper.ValidCertificate)}</File>", interchangeText);
				AssertContains(@"<Passphrase>", interchangeText);
			});
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
			AssertEquals("Issue Date has been defaulted from valid certificate", validCertificate.NotBefore, GlbExternalPassword.GP_IssueDate);
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
	}
}
