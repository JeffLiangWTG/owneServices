using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyExternalPasswordHUI))]
	sealed class GlbCompanyExternalPasswordHUITest : GlbExternalPasswordTest<GlbCompanyExternalPasswordHUI>
	{
		public void TestLookupsAndValidationType()
		{
			Assert("GlbExternalPassword.Lookups is GlbExternalPasswordLookups", GlbExternalPassword.Lookups is GlbExternalPasswordLookups);
			Assert("GlbExternalPassword.Validation is GlbCompanyExternalPasswordHUIValidation", GlbExternalPassword.Validation is GlbCompanyExternalPasswordHUIValidation);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Password type should be HUI", "HUI", GlbExternalPassword.GP_PasswordType);
			AssertEquals("Company should be CurrentCompany", GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertEquals("Staff should be ZGuid.Empty", ZGuid.Empty, GlbExternalPassword.GP_GS);
			AssertEquals("Branch should be ZGuid.Empty", ZGuid.Empty, GlbExternalPassword.GP_GB);
			AssertEquals("Group should be ZGuid.Empty", ZGuid.Empty, GlbExternalPassword.GP_GG);
			AssertNullOrEmpty("Password Status should be Empty", GlbExternalPassword.GP_PasswordStatus);
		}

		public void TestIsAllowed()
		{
			var company = Factory.New<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.Australia);
			Assert("Should not be allowed for non-Hungary company", !GlbCompanyExternalPasswordHUI.IsAllowed(company));

			var huCompany = Factory.New<GlbCompany>();
			huCompany.SetCountry(Core.Constants.CountryCodes.Hungary);
			Assert("Should be allowed for Hungary company", GlbCompanyExternalPasswordHUI.IsAllowed(huCompany));
		}

		public void TestSetAndGet_Login()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			AssertNullOrEmpty("Login should be blank by default", credential.Login);

			credential.Login = "h9nupbpmgi8yhet";
			AssertEquals("Login should round trip", "h9nupbpmgi8yhet", credential.Login);
			AssertEquals("Login should be stored unencrypted", credential.Login, credential.GP_UserID);
		}

		public void TestSetAndGet_PasswordHash_IsEncrypted()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			AssertNullOrEmpty("PasswordHash should be blank by default", credential.PasswordHash);

			credential.PasswordHash = "0123456789ABCDEF";
			AssertEquals("PasswordHash should round trip", "0123456789ABCDEF", credential.PasswordHash);
			AssertNotEquals("PasswordHash should be stored encrypted", credential.PasswordHash, credential.GP_CurrentPassword);
		}

		public void TestPasswordHash_RetainsNonHexString()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();

			credential.PasswordHash = "I am not a hex string";
			AssertEquals("PasswordHash should retain the invalid value", "I am not a hex string", credential.PasswordHash);
		}

		public void TestSetAndGet_SignatureKey_IsEncrypted()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			AssertNullOrEmpty("SignatureKey should be blank by default", credential.SignatureKey);

			credential.SignatureKey = "fb-8530-0ead7916e5432DA4ZSW1UKWX";
			AssertEquals("SignatureKey should round trip", "fb-8530-0ead7916e5432DA4ZSW1UKWX", credential.SignatureKey);
			AssertNotEquals("SignatureKey should be stored encrypted", credential.SignatureKey, credential.GP_CertificatePassPhrase);
		}

		public void TestSetAndGet_ReplacementKey_IsEncrypted()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			AssertNullOrEmpty("ReplacementKey should be blank by default", credential.ReplacementKey);

			credential.ReplacementKey = "8a042DA4ZSW17HY6";
			AssertEquals("ReplacementKey should round trip", "8a042DA4ZSW17HY6", credential.ReplacementKey);
			AssertNotEquals("ReplacementKey should be stored encrypted", credential.ReplacementKey, credential.GP_NextPassword);
		}

		public void TestLoadForCompany_ReturnsNullWhenNoCredentialsInDB()
		{
			var credential = GlbCompanyExternalPasswordHUI.LoadForCompany(Factory, GlbCompany.CurrentCompany);
			AssertNull("Null credential should be returned when none in database.", credential);
		}

		public void TestLoadForCompany_ReturnsBizoWhenCredentialInDB()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var credentialInOtherFactory = newFactory.New<GlbCompanyExternalPasswordHUI>();
			credentialInOtherFactory.Login = "h9nupbpmgi8yhet";
			newFactory.Save();

			var credential = GlbCompanyExternalPasswordHUI.LoadForCompany(Factory, GlbCompany.CurrentCompany);
			AssertNotNull("Bizo should be returned when one in database.", credential);
			AssertEquals("Bizo.PK should match when fetched from database.", credentialInOtherFactory.PK, credential.PK);
		}

		public void TestLoadForCompany_RaisesErrorReportWhenTwoCredentialsInDB()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var credentialInOtherFactory = newFactory.New<GlbCompanyExternalPasswordHUI>();
			credentialInOtherFactory.Login = "h9nupbpmgi8yhet";
			var credentialInOtherFactory2 = newFactory.New<GlbCompanyExternalPasswordHUI>();
			credentialInOtherFactory2.Login = "h9nupbpmgi8yhet2";
			newFactory.Save();
			CargoWise.Data.Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbExternalPassword SET GP_SystemLastEditTimeUtc = DATEADD(minute, -5, GP_SystemLastEditTimeUtc), GP_SystemLastEditUser = 'E' WHERE GP_PK = '" + credentialInOtherFactory.PK + "'");

			var credential = GlbCompanyExternalPasswordHUI.LoadForCompany(Factory, GlbCompany.CurrentCompany);
			AssertEquals("ErrorReporter should raise error when multiple credentials found in DB", "GlbCompanyExternalPasswordHUI.LoadForCompany.NotUnique", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertNotNull("Bizo should be returned when one in database.", credential);
			AssertEquals("Bizo.PK should match most recently modified credential when fetched from database.", credentialInOtherFactory2.PK, credential.PK);
		}

		public void TestLoadForCompanyOrNew_ReturnsNewBizoWhenNoCredentialsInDB()
		{
			var credential = GlbCompanyExternalPasswordHUI.LoadForCompanyOrNew(Factory, GlbCompany.CurrentCompany);
			AssertNotNull("New bizo should be returned when none in database.", credential);
			Assert("Bizo should be not in DB when none in database.", !credential.IsInDatabase);
		}

		public void TestLoadForCompanyOrNew_ReturnsBizoWhenCredentialInDB()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var credentialInOtherFactory = newFactory.New<GlbCompanyExternalPasswordHUI>();
			credentialInOtherFactory.Login = "h9nupbpmgi8yhet";
			newFactory.Save();

			var credential = GlbCompanyExternalPasswordHUI.LoadForCompanyOrNew(Factory, GlbCompany.CurrentCompany);
			AssertNotNull("Bizo should be returned when one in database.", credential);
			AssertEquals("Bizo.PK should match when fetched from database.", credentialInOtherFactory.PK, credential.PK);
		}

		public void TestStatusIsOK_WhenAllFieldsEntered_OnSave()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			credential.Login = "h9nupbpmgi8yhet";
			credential.PasswordHash = "0123456789ABCDEF";
			credential.ReplacementKey = "8a042DA4ZSW17HY6";
			credential.SignatureKey = "fb-8530-0ead7916e5432DA4ZSW1UKWX";
			Factory.Save();

			AssertEquals("Password status is OK when all fields are entered after save.", PasswordStatusList.Codes.PasswordOK, credential.GP_PasswordStatus);
		}

		public void TestStatusIsEmpty_WhenNoFieldsEntered_OnSave()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			credential.GP_MailBoxID = "unused field";
			Factory.Save();

			AssertNullOrEmpty("Password status is empty when no fields are entered after save.", credential.GP_PasswordStatus);
		}

		public void TestEHubMessage_IsNotSent_OnSave()
		{
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			credential.GP_MailBoxID = "unused field";
			AssertNull("Precondition: no credential message queued", Factory.GetLatestEHubConfigurationInterchange());
			Factory.Save();

			var interchangeMessage = Factory.GetLatestEHubConfigurationInterchange();
			AssertNull("No credential message should be queued after save", interchangeMessage);
		}

		#region Implementation

		protected override GlbCompanyExternalPasswordHUI CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<GlbCompanyExternalPasswordHUI>();
		}

		#endregion
	}
}
