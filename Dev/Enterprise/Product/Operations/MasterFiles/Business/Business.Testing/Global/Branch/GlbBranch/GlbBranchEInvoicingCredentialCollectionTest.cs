using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchEInvoicingCredentialCollection))]
	sealed class GlbBranchEInvoicingCredentialCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewCollection_WithNullCompany_DoesNotThrow()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.Empty;
			AssertNoExceptionThrown(() => new GlbBranchEInvoicingCredentialCollection(branch));
		}

		#region CredentialSettings

		public void TestCredentialSettings_IsBasedOnCompany()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateBranchAndCollectionForTest(countryCode: "BR");
			AssertNotNull("When country code matches company, CredentialSettings should not be null", brazilCollection.CredentialSettings);

			var noCountryCollection = CreateBranchAndCollectionForTest(countryCode: "ZZ");
			AssertNull("When country code does not match company, CredentialSettings should be null", noCountryCollection.CredentialSettings);
		}

		public void TestCredentialSettings_IsSetOnChildren()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateBranchAndCollectionForTest(countryCode: "BR");
			AssertNotNull("Precondition", brazilCollection.CredentialSettings);

			var validBranchCredential = brazilCollection.AddNew();
			validBranchCredential.GP_Certificate = new ZBlob(new byte[] { 1, 2 });
			AssertSame("Child bizo CredentialSettings should same as collection", brazilCollection.CredentialSettings, validBranchCredential.CredentialSettings);

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var newCollection = new GlbBranchEInvoicingCredentialCollection(newFactory.Load<GlbBranch>(brazilCollection.Master.PK));
			newCollection.Load();
			AssertSame("Reloaded collection should have same CredentialSettings as original collection", brazilCollection.CredentialSettings, newCollection.CredentialSettings);
			AssertSame("Child credential should have same CredentialSettings as original collection", brazilCollection.CredentialSettings, newCollection[0].CredentialSettings);
		}

		#endregion

		#region Branch Collection

		public void TestBranchCollection_IsLoaded_WhenEnabled()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: CountryCodes.Brazil);
			var branch = Factory.NewCompanyAndBranchWith(countryCode: CountryCodes.Brazil);
			branch.Factory.Save();

			AssertNotNull("EInvoicingCredentials should never be null", branch.EInvoicingCertificateCredentials);
			AssertEquals("Branch.EInvoicingCredentials.IsEnabled should be enabled", true, branch.EInvoicingCertificateCredentials.IsEnabled);
			AssertEquals("Branch.EInvoicingCredentials.IsEditAllowed should be enabled", true, branch.EInvoicingCertificateCredentials.IsEditAllowed);
			AssertEquals("Branch.EInvoicingCredentials.IsLoaded is expected to be true prior to test", true, branch.EInvoicingCertificateCredentials.IsLoaded);
			AssertEquals("Branch.EInvoicingCredentials.Count is expected to be 0 prior to test", 0, branch.EInvoicingCertificateCredentials.Count);

			var validBranchCredential = branch.EInvoicingCertificateCredentials.AddNew();
			validBranchCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			validBranchCredential.GP_Certificate = new ZBlob(new byte[] { 1, 2 });
			AssertEquals(nameof(validBranchCredential.GP_GB), branch.PK, validBranchCredential.GP_GB);
			AssertEquals(nameof(validBranchCredential.GP_GC), branch.GB_GC, validBranchCredential.GP_GC);
			AssertEquals("Branch.EInvoicingCredentials.Count is expected to be 1 after an EInvoicingCertificate has been added for the branch", 1, branch.EInvoicingCertificateCredentials.Count);
			branch.Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBranch = newFactory.Load<GlbBranch>(branch.PK);
			AssertEquals("Branch.EInvoicingCredentials.IsLoaded is expected to be true after reloading a branch with credentials already added", true, loadedBranch.EInvoicingCertificateCredentials.IsLoaded);
			AssertEquals("Branch.EInvoicingCredentials.Count is expected to be 1 after a reloading a branch with credentials already added", 1, loadedBranch.EInvoicingCertificateCredentials.Count);
		}

		public void TestEInvoicingCredentials_IsNotLoaded_WhenCountryNotConfigured()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: CountryCodes.Uzbekistan);
			var branch = Factory.NewCompanyAndBranchWith(countryCode: CountryCodes.Brazil);
			branch.Factory.Save();

			AssertNotNull("EInvoicingCredentials should never be null", branch.EInvoicingCertificateCredentials);
			AssertEquals("Branch.EInvoicingCredentials.IsEnabled should be false when EInvoicingCertificateCredentials is not configured", false, branch.EInvoicingCertificateCredentials.IsEnabled);
			AssertEquals("Branch.EInvoicingCredentials.IsEditAllowed should be false when country is not configured", false, branch.EInvoicingCertificateCredentials.IsEditAllowed);
			AssertEquals("Branch.EInvoicingCredentials.IsLoaded should be false when country is not enabled", false, branch.EInvoicingCertificateCredentials.IsLoaded);
			AssertEquals("Branch.EInvoicingCredentials.Count is expected to be 0", 0, branch.EInvoicingCertificateCredentials.Count);
		}

		#endregion

		#region IsEditAllowed

		public void TestIsEditAllowed()
		{
			var credentialsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = (GlbBranchEInvoicingCredentialCollection)GetCollectionToTest();

			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(false);
			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);
			Assert("IsEditAllowed should be false when not IsBranchCredentialsRequired", !collection.IsEditAllowed);

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			Assert("IsEditAllowed should not be affected by IsCompanyCredentialsRequired", !collection.IsEditAllowed);

			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(true);
			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);
			Assert("IsEditAllowed should be true when IsBranchCredentialsRequired", collection.IsEditAllowed);

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			Assert("IsEditAllowed should not be affected by IsCompanyCredentialsRequired", collection.IsEditAllowed);
		}

		#endregion

		#region IsEnabled

		public void TestIsEnabled_IsFalse_WhenNullCredentialSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var noCountryCollection = CreateBranchAndCollectionForTest(countryCode: "ZZ");
			AssertNotNull("When CredentialSettings is null, collection should be disabled", !noCountryCollection.IsEnabled);
		}

		public void TestIsEnabled_IsFalse_WhenUnknownCredentialSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateBranchAndCollectionForTest(countryCode: "BR");
			AssertNotNull("When CredentialSettings is neither certificate or password, collection should be disabled", !brazilCollection.IsEnabled);
		}

		public void TestIsEnabled_IsTrue_WhenCredentialSettingsAvailableForCompany()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var brazilCollection = CreateBranchAndCollectionForTest(countryCode: "BR");
			AssertNotNull("When CredentialSettings is known (certificate or password), collection should be enabled", brazilCollection.IsEnabled);
		}

		#endregion IsEnabled

		#region Filter (PasswordType)

		public void TestFilter_IncludesNoResult_WhenNullCredentialSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");
			var noCountryCollection = CreateBranchAndCollectionForTest(countryCode: "ZZ");
			AssertEquals("When CredentialSettings is null, filter should contain impossible predicate", "", noCountryCollection.CompleteFilter.LiteralTextSqlFormatted);
		}

		public void TestFilter_IncludesPasswordType_WhenCredentialSettingsAvailableForCompany()
		{
			var collectionMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>(countryCode: "BR");

			collectionMock.Setup(x => x.PasswordType).Returns("ABC");
			var brazilCollection = CreateBranchAndCollectionForTest(countryCode: "BR");
			AssertContains("When CredentialSettings.PasswordType is set, filter should contain predicate", nameof(GlbExternalPassword.GP_PasswordType) + " = 'ABC'", brazilCollection.CompleteFilter.LiteralTextSqlFormatted);

			collectionMock.Setup(x => x.PasswordType).Returns("EIM");
			AssertContains("When CredentialSettings.PasswordType is set, filter should contain predicate", nameof(GlbExternalPassword.GP_PasswordType) + " = 'EIM'", brazilCollection.CompleteFilter.LiteralTextSqlFormatted);

			collectionMock.Setup(x => x.PasswordType).Returns("");
			AssertContains("When CredentialSettings.PasswordType is empty, filter should contain predicate", nameof(GlbExternalPassword.GP_PasswordType) + " = ''", brazilCollection.CompleteFilter.LiteralTextSqlFormatted);
		}

		#endregion

		#region Preferred Sequence Number

		public void TestPreferredSequenceNumber_SingleValidCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest(countryCode: "BR");
			var credential = collection.AddNew();
			var userCredentials = new NetworkCredential("user", "password12345678");

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName(), X500DistinguishedNameFlags.UseSemicolons)))
			{
				credential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				credential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				AssertEquals("Precondition: valid certificate", true, credential.IsValidCertificate);
				AssertEquals("PreferredSequenceNumber should be one for single credential in collection", credential.PreferredSequenceNumber, 1);
			}
		}

		public void TestPreferredSequenceNumber_SingleInvalidCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest(countryCode: "BR");
			var credential = collection.AddNew();
			var userCredentials = new NetworkCredential("user", "password12345678");

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName(), X500DistinguishedNameFlags.UseSemicolons)))
			{
				credential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);

				AssertEquals("Precondition: invalid certificate", false, credential.IsValidCertificate);
				AssertEquals("PreferredSequenceNumber should be zero for invalid credential in collection", credential.PreferredSequenceNumber, 0);
			}
		}

		[TestDate(2020, 12, 22)]
		public void TestPreferredSequenceNumber_VariousCredentials()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest(countryCode: "BR");

			var userCredentials = new NetworkCredential("user", "password12345678");
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var expiredCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName("Expired"), X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-30), expiryDate: ZDateTime.Today.AddDays(-7)))
			using (var almostExpiredCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName("Expires Tomorrow"), X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-7), expiryDate: ZDateTime.Today.AddDays(1)))
			using (var validCert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName("Valid"), X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-3), expiryDate: ZDateTime.Today.AddDays(7)))
			using (var issuedYesterdayCert1 = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName("Issued Yesterday 1"), X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-1), expiryDate: ZDateTime.Today.AddDays(7)))
			using (var issuedYesterdayCert2 = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName("Issued Yesterday 2"), X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-1), expiryDate: ZDateTime.Today.AddDays(7)))
			{
				var expiredCredential = collection.AddNew();
				expiredCredential.GP_Certificate = expiredCert.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				expiredCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				var almostExpiredCredential = collection.AddNew();
				almostExpiredCredential.GP_Certificate = almostExpiredCert.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				almostExpiredCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				var validCredential = collection.AddNew();
				validCredential.GP_Certificate = validCert.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				validCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				var issuedYesterdayCredential1 = collection.AddNew();
				issuedYesterdayCredential1.GP_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-5);
				issuedYesterdayCredential1.GP_Certificate = issuedYesterdayCert1.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				issuedYesterdayCredential1.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				var issuedYesterdayCredential2 = collection.AddNew();
				issuedYesterdayCredential2.GP_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-1);
				issuedYesterdayCredential2.GP_Certificate = issuedYesterdayCert2.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				issuedYesterdayCredential2.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				var emptyCredential = collection.AddNew();

				CombineAssertions("PreferredSequenceNumber should update on add and as certificate data is set", () =>
				{
					AssertEquals("issuedYesterdayCredential2", 1, issuedYesterdayCredential2.PreferredSequenceNumber);
					AssertEquals("issuedYesterdayCredential1", 2, issuedYesterdayCredential1.PreferredSequenceNumber);
					AssertEquals("validCredential", 3, validCredential.PreferredSequenceNumber);
					AssertEquals("almostExpiredCredential", 4, almostExpiredCredential.PreferredSequenceNumber);
					AssertEquals("emptyCredential", 0, emptyCredential.PreferredSequenceNumber);
					AssertEquals("expiredCredential", 0, expiredCredential.PreferredSequenceNumber);
				});
				Factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
				CombineAssertions("Passing of time does not automatically update PreferredSequenceNumber", () =>
				{
					AssertEquals("issuedYesterdayCredential2", 1, issuedYesterdayCredential2.PreferredSequenceNumber);
					AssertEquals("issuedYesterdayCredential1", 2, issuedYesterdayCredential1.PreferredSequenceNumber);
					AssertEquals("validCredential", 3, validCredential.PreferredSequenceNumber);
					AssertEquals("almostExpiredCredential", 4, almostExpiredCredential.PreferredSequenceNumber);
					AssertEquals("emptyCredential", 0, emptyCredential.PreferredSequenceNumber);
					AssertEquals("expiredCredential", 0, expiredCredential.PreferredSequenceNumber);
				});

				collection.UpdatePreferredSequenceNumberOnChildren();
				CombineAssertions("UpdatePreferredSequenceNumberOnChildren() should update PreferredSequenceNumber", () =>
				{
					AssertEquals("issuedYesterdayCredential2", 1, issuedYesterdayCredential2.PreferredSequenceNumber);
					AssertEquals("issuedYesterdayCredential1", 2, issuedYesterdayCredential1.PreferredSequenceNumber);
					AssertEquals("validCredential", 3, validCredential.PreferredSequenceNumber);
					AssertEquals("almostExpiredCredential", 0, almostExpiredCredential.PreferredSequenceNumber);
					AssertEquals("emptyCredential", 0, emptyCredential.PreferredSequenceNumber);
					AssertEquals("expiredCredential", 0, expiredCredential.PreferredSequenceNumber);
				});

				var newFactory = Factory.CreateNewFactory();
				var newCollection = new GlbBranchEInvoicingCredentialCollection(newFactory.Load<GlbBranch>(collection.Master.PK));
				newCollection.Load();
				var loadedExpiredCredential = (EInvoicingCertificateCredential)newCollection.FindByPK(expiredCredential.PK);
				var loadedAlmostExpiredCredential = (EInvoicingCertificateCredential)newCollection.FindByPK(almostExpiredCredential.PK);
				var loadedValidCredential = (EInvoicingCertificateCredential)newCollection.FindByPK(validCredential.PK);
				var loadedIssuedYesterdayCredential1 = (EInvoicingCertificateCredential)newCollection.FindByPK(issuedYesterdayCredential1.PK);
				var loadedIssuedYesterdayCredential2 = (EInvoicingCertificateCredential)newCollection.FindByPK(issuedYesterdayCredential2.PK);
				var loadedEmptyCredential = (EInvoicingCertificateCredential)newCollection.FindByPK(emptyCredential.PK);

				CombineAssertions("PreferredSequenceNumber should be applied on collection Load", () =>
				{
					AssertEquals("issuedYesterdayCredential2", 1, loadedIssuedYesterdayCredential2.PreferredSequenceNumber);
					AssertEquals("issuedYesterdayCredential1", 2, loadedIssuedYesterdayCredential1.PreferredSequenceNumber);
					AssertEquals("validCredential", 3, loadedValidCredential.PreferredSequenceNumber);
					AssertEquals("almostExpiredCredential", 0, loadedAlmostExpiredCredential.PreferredSequenceNumber);
					AssertNull("emptyCredential will not be loaded due to missing PasswordType and Credential", loadedEmptyCredential);
					AssertEquals("expiredCredential", 0, loadedExpiredCredential.PreferredSequenceNumber);
				});
			}
		}

		#endregion

		#region GetWarningBeforeDelete

		public void TestGetWarningBeforeDelete_IsEmpty_WhenPasswordCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingPasswordCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest();
			var bizo = collection.AddNew();
			AssertNullOrEmpty("Password credentials never show a warning before deletion when collection has single bizo", bizo.GetWarningBeforeBeingDeleted());
			var bizo2 = collection.AddNew();
			AssertNullOrEmpty("Password credentials never show a warning before deletion when collection has more than one bizo", bizo2.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_IsEmpty_WhenManyValidCertificateCredentials()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest();
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
			var collection = CreateBranchAndCollectionForTest();
			var bizo = collection.AddNew();
			bizo.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			AssertNullOrEmpty("Certificate credentials should not show a warning before deletion when the bizo is not valid", bizo.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_IsEmpty_WhenExpiredCertificateCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest();
			var bizo = collection.AddNew();
			bizo.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertNullOrEmpty("Certificate credentials should not show a warning before deletion when the bizo is expired", bizo.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_HasContent_WhenLastValidCertificateCredential()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			var collection = CreateBranchAndCollectionForTest();
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

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CreateBranchAndCollectionForTest();
		}

		GlbBranchEInvoicingCredentialCollection CreateBranchAndCollectionForTest(string countryCode = "BR")
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: countryCode);
			return new GlbBranchEInvoicingCredentialCollection(branch);
		}

		const string CertificateRootName = "CN = VMS ICA1 Staging;O = FRCS;C = FJ";
		static string CertificateChildName(string subjectName = null)
			=> $"CN = {subjectName ?? "PTY7 Wisetech Global Limited"};OU = Wisetech Global Limited;O = Wisetech Global Limited;STREET = 72 O'Riodran St Post Code 2015;L = Alexendria NSW;S = UNKNOWN;C = AU";

		#endregion
	}
}
