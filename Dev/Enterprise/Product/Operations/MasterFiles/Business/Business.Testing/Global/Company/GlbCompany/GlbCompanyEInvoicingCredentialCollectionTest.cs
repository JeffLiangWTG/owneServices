using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyEInvoicingCredentialCollection))]
	sealed class GlbCompanyEInvoicingCredentialCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
			=> CreateCompanyAndCollectionForTest();

		#region CredentialSettings

		public void TestCredentialSettings_IsBasedOnCompany()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateCompanyAndCollectionForTest(countryCode: "BR");
			AssertNotNull("When country code matches company, CredentialSettings should not be null", brazilCollection.CredentialSettings);

			var noCountryCollection = CreateCompanyAndCollectionForTest(countryCode: "ZZ");
			AssertNull("When country code does not match company, CredentialSettings should be null", noCountryCollection.CredentialSettings);
		}

		public void TestCredentialSettings_IsSetOnChildren()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateCompanyAndCollectionForTest(countryCode: "BR");
			AssertNotNull("Precondition", brazilCollection.CredentialSettings);

			var credential = brazilCollection.AddNew();
			credential.GP_Certificate = new ZBlob(new byte[] { 1, 2 });
			AssertSame("Child bizo CredentialSettings should same as collection", brazilCollection.CredentialSettings, credential.CredentialSettings);

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var newCollection = new GlbCompanyEInvoicingCredentialCollection(newFactory.Load<GlbCompany>(brazilCollection.Master.PK));
			newCollection.Load();
			AssertSame("Reloaded collection should have same CredentialSettings as original collection", brazilCollection.CredentialSettings, newCollection.CredentialSettings);
			AssertSame("Child credential should have same CredentialSettings as original collection", brazilCollection.CredentialSettings, newCollection[0].CredentialSettings);
		}

		#endregion

		#region IsEditAllowed

		public void TestIsEditAllowed()
		{
			var credentialsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = (GlbCompanyEInvoicingCredentialCollection)GetCollectionToTest();

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);
			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(false);
			Assert("IsEditAllowed should be false when not IsCompanyCredentialsRequired", !collection.IsEditAllowed);

			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(true);
			Assert("IsEditAllowed should not be affected by IsBranchCredentialsRequired", !collection.IsEditAllowed);

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(false);
			Assert("IsEditAllowed should be true when IsCompanyCredentialsRequired", collection.IsEditAllowed);

			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(true);
			Assert("IsEditAllowed should not be affected by IsBranchCredentialsRequired", collection.IsEditAllowed);
		}

		#endregion

		#region IsEnabled

		public void TestIsEnabled_IsFalse_WhenNullCredentialSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var noCountryCollection = CreateCompanyAndCollectionForTest(countryCode: "ZZ");
			AssertNotNull("When CredentialSettings is null, collection should be disabled", !noCountryCollection.IsEnabled);
		}

		public void TestIsEnabled_IsFalse_WhenUnknownCredentialSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateCompanyAndCollectionForTest(countryCode: "BR");
			AssertNotNull("When CredentialSettings is neither certificate or password, collection should be disabled", !brazilCollection.IsEnabled);
		}

		public void TestIsEnabled_IsTrue_WhenCredentialSettingsAvailableForCompany()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateCompanyAndCollectionForTest(countryCode: "BR");
			AssertNotNull("When CredentialSettings is known (certificate or password), collection should be enabled", brazilCollection.IsEnabled);
		}

		#endregion IsEnabled

		#region Filter (PasswordType)

		public void TestFilter_IncludesNoResult_WhenNullCredentialSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var noCountryCollection = CreateCompanyAndCollectionForTest(countryCode: "ZZ");
			AssertEquals("When CredentialSettings is null, filter should contain impossible predicate", "", noCountryCollection.CompleteFilter.LiteralTextSqlFormatted);
		}

		public void TestFilter_IncludesPasswordType_WhenCredentialSettingsAvailableForCompany()
		{
			var collectionMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");

			collectionMock.Setup(x => x.PasswordType).Returns("ABC");
			var brazilCollection = CreateCompanyAndCollectionForTest(countryCode: "BR");
			AssertContains("When CredentialSettings.PasswordType is set, filter should contain predicate", nameof(GlbExternalPassword.GP_PasswordType) + " = 'ABC'", brazilCollection.CompleteFilter.LiteralTextSqlFormatted);

			collectionMock.Setup(x => x.PasswordType).Returns("EIM");
			AssertContains("When CredentialSettings.PasswordType is set, filter should contain predicate", nameof(GlbExternalPassword.GP_PasswordType) + " = 'EIM'", brazilCollection.CompleteFilter.LiteralTextSqlFormatted);

			collectionMock.Setup(x => x.PasswordType).Returns("");
			AssertContains("When CredentialSettings.PasswordType is empty, filter should contain predicate", nameof(GlbExternalPassword.GP_PasswordType) + " = ''", brazilCollection.CompleteFilter.LiteralTextSqlFormatted);
		}

		#endregion

		#region GetWarningBeforeDelete

		public void TestGetWarningBeforeDelete_IsEmpty_WhenPasswordCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingPasswordCredentialSettings>();
			var collection = CreateCompanyAndCollectionForTest();
			var bizo = collection.AddNew();
			AssertNullOrEmpty("Password credentials never show a warning before deletion when collection has single bizo", bizo.GetWarningBeforeBeingDeleted());
			var bizo2 = collection.AddNew();
			AssertNullOrEmpty("Password credentials never show a warning before deletion when collection has more than one bizo", bizo2.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_IsEmpty_WhenManyValidCertificateCredentials()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateCompanyAndCollectionForTest();
			var bizo = collection.AddNew();
			bizo.GP_Certificate = new byte[4];
			bizo.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			bizo.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			var bizo2 = collection.AddNew();
			bizo2.GP_Certificate = new byte[4];
			bizo2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			bizo2.GP_ExpiryDate = ZDateTime.Now.AddDays(2);
			AssertNullOrEmpty("Certificate credentials should not show a warning before deletion when collection has more than one bizo", bizo.GetWarningBeforeBeingDeleted());
			AssertNullOrEmpty("Certificate credentials should not show a warning before deletion when collection has more than one bizo", bizo2.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_IsEmpty_WhenInvalidCertificateCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateCompanyAndCollectionForTest();
			var bizo = collection.AddNew();
			bizo.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			AssertNullOrEmpty("Certificate credentials should not show a warning before deletion when the bizo is not valid", bizo.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_IsEmpty_WhenExpiredCertificateCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateCompanyAndCollectionForTest();
			var bizo = collection.AddNew();
			bizo.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertNullOrEmpty("Certificate credentials should not show a warning before deletion when the bizo is expired", bizo.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_HasContent_WhenLastValidCertificateCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateCompanyAndCollectionForTest();
			var bizo = collection.AddNew();
			bizo.GP_Certificate = new byte[4];
			bizo.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			bizo.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			var invalidBizo = collection.AddNew();
			invalidBizo.GP_Certificate = new byte[4];
			invalidBizo.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			var expiredBizo = collection.AddNew();
			expiredBizo.GP_Certificate = new byte[4];
			expiredBizo.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("Certificate credentials should show a warning before deletion when it is the last bizo in the collection", "Deleting the last certificate may cause E-Invoicing errors.", bizo.GetWarningBeforeBeingDeleted());
		}

		#endregion

		#region Implementation

		GlbCompanyEInvoicingCredentialCollection CreateCompanyAndCollectionForTest(string countryCode = "BR")
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			return new GlbCompanyEInvoicingCredentialCollection(company);
		}

		#endregion
	}
}
