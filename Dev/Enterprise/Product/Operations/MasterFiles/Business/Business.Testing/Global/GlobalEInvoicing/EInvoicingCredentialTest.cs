using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class EInvoicingCredentialTest<T> : GlbExternalPasswordTest<T> where T : GlbExternalPassword
	{
		protected virtual string ExpectedPasswordType => PasswordTypesList.Codes.EIM;
		protected virtual string SubjectNameSample => "CN=Child";

		public virtual void TestSetDefaultValues()
		{
			var credential = Factory.New<T>() as EInvoicingCertificateCredential;

			AssertEquals(nameof(credential.GP_GS), ZGuid.Empty, credential.GP_GS);
			AssertEquals(nameof(credential.GP_GC), GlbCompany.CurrentCompany.PK, credential.GP_GC);
			AssertEquals(nameof(credential.GP_PasswordStatus), PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals(nameof(credential.PasswordStatus), PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
			AssertEquals(nameof(credential.GP_PasswordType), ExpectedPasswordType, credential.GP_PasswordType);
		}

		public void TestGetCertificateDetails()
		{
			var rootIssueDate = ZDateTime.Today.AddYears(-1);
			var rootExpiryDate = ZDateTime.Today.AddYears(5);
			var childIssueDate = new ZDateTime(ZDateTime.Today.AddDays(-1), DateTimeKind.Utc);
			var childExpiryDate = new ZDateTime(ZDateTime.Today.AddDays(7), DateTimeKind.Utc);

			var credential = Factory.New<T>() as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;
			credential.GP_MailBoxID = ZString.Empty;

			using (var rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=Root"), rootIssueDate, rootExpiryDate))
			using (var childCertificate = SampleCertificateHelper.CreateSampleChildCertificate(rootCertificate, new X500DistinguishedName(SubjectNameSample), childIssueDate, childExpiryDate))
			{
				var userCredentials = new NetworkCredential("user", "password12345678");
				var certificatePfx = childCertificate.Export(X509ContentType.Pfx, userCredentials.SecurePassword);

				AssertEquals(nameof(credential.GP_CertificateAuthority), ZString.Empty, credential.GP_CertificateAuthority);
				AssertEquals(nameof(credential.IssuerNameCommonName), ZString.Empty, credential.IssuerNameCommonName);
				AssertEquals(nameof(credential.GP_IssueDate), ZDateTime.Empty, credential.GP_IssueDate);
				AssertEquals(nameof(credential.GP_ExpiryDate), ZDateTime.Empty, credential.GP_ExpiryDate);
				AssertEquals(nameof(credential.CurrentDecryptedCertificatePassphrase), ZString.Empty, credential.CurrentDecryptedCertificatePassphrase);
				AssertEquals(nameof(credential.GP_CertificatePassPhrase), ZString.Empty, credential.GP_CertificatePassPhrase);
				AssertEquals(nameof(credential.GP_UserID), ZString.Empty, credential.GP_UserID);
				AssertEquals(nameof(credential.GP_MailBoxID), ZString.Empty, credential.GP_MailBoxID);
				AssertEquals(nameof(credential.GP_PasswordStatus), "INV", credential.GP_PasswordStatus);
				AssertEquals(nameof(credential.PasswordStatus), "Invalid", credential.PasswordStatus);
				AssertEquals(nameof(credential.SerialNumber), ZString.Empty, credential.SerialNumber);
				AssertEquals(nameof(credential.SubjectNameCommonName), ZString.Empty, credential.SubjectNameCommonName);

				credential.GP_Certificate = certificatePfx;
				credential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				var encoder = new TwoWayEncoder(credential.PK.ToGuid());
				AssertEquals(nameof(credential.GP_CertificateAuthority), ZString.Empty, credential.GP_CertificateAuthority);
				AssertEquals(nameof(credential.IssuerNameCommonName), "Root", credential.IssuerNameCommonName);
				AssertEquals(nameof(credential.GP_CertificatePassPhrase), encoder.Encrypt(userCredentials.Password), credential.GP_CertificatePassPhrase);
				AssertEquals(nameof(credential.CurrentDecryptedCertificatePassphrase), userCredentials.Password, credential.CurrentDecryptedCertificatePassphrase);
				AssertEquals(nameof(credential.GP_UserID), childCertificate.Thumbprint, credential.GP_UserID);
				AssertEquals(nameof(credential.GP_MailBoxID), ZString.Empty, credential.GP_MailBoxID);
				AssertEquals(nameof(credential.GP_PasswordStatus), "VAL", credential.GP_PasswordStatus);
				AssertEquals(nameof(credential.PasswordStatus), "Valid", credential.PasswordStatus);
				AssertEquals(nameof(credential.SubjectNameCommonName), "Child", credential.SubjectNameCommonName);
				AssertNotEquals(nameof(credential.SerialNumber), ZString.Empty, credential.SerialNumber);
				AssertSequencesEqual(nameof(credential.GP_Certificate), certificatePfx, (byte[])credential.GP_Certificate);

				var issueDateDifference = Math.Abs((childIssueDate - credential.GP_IssueDate).TotalHours);
				var expriryDateDifference = Math.Abs((childExpiryDate - credential.GP_ExpiryDate).TotalHours);
				AssertLessThan(nameof(credential.GP_IssueDate), issueDateDifference, 12);
				AssertLessThan(nameof(credential.GP_ExpiryDate), expriryDateDifference, 12);
			}
		}

		public virtual void TestLookups()
		{
			Assert("EInvoicingCredential.Lookups is EInvoicingCredentialLookups", GlbExternalPassword.Lookups is EInvoicingCredentialLookups);
			AssertEquals("EInvoicingCredential.Lookups.PasswordTypeList.Count", 1, GlbExternalPassword.Lookups.PasswordTypeList.Count);
			AssertEquals("EInvoicingCredential.Lookups.PasswordTypeList[0].Code", ExpectedPasswordType, GlbExternalPassword.Lookups.PasswordTypeList[0].Code);
		}

		public virtual void TestValidationType()
		{
			Assert(GlbExternalPassword.Validation is EInvoicingCertificateCredentialValidation);
		}

		public void TestCertificateRawData()
		{
			var company = CreateCompanyForTest();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			var credential = CreateEInvoicingCredential(company.PK, ZGuid.Empty, true);
			var expectedDecodedData = "TUlJRDFEQ0NBM21nQXdJQkFnSVRid0FBZTNVQVlWVTM0SS8rNVFBQkFBQjdkVEFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURZeE1qRTNOREExTWxvWERUSTBNRFl4TVRFM05EQTFNbG93U1RFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCV0ZuYVd4bE1SWXdGQVlEVlFRTEV3MW9ZWGxoSUhsaFoyaHRiM1Z5TVJJd0VBWURWUVFERXdreE1qY3VNQzR3TGpFd1ZqQVFCZ2NxaGtqT1BRSUJCZ1VyZ1FRQUNnTkNBQVRUQUs5bHJUVmtvOXJrcTZaWWNjOUhEUlpQNGI5UzR6QTRLbTdZWEorc25UVmhMa3pVMEhzbVNYOVVuOGpEaFJUT0hES2FmdDhDL3V1VVk5MzR2dU1ObzRJQ0p6Q0NBaU13Z1lnR0ExVWRFUVNCZ0RCK3BId3dlakViTUJrR0ExVUVCQXdTTVMxb1lYbGhmREl0TWpNMGZETXRNVEV5TVI4d0hRWUtDWkltaVpQeUxHUUJBUXdQTXpBd01EYzFOVGc0TnpBd01EQXpNUTB3Q3dZRFZRUU1EQVF4TVRBd01SRXdEd1lEVlFRYURBaGFZWFJqWVNBeE1qRVlNQllHQTFVRUR3d1BSbTl2WkNCQ2RYTnphVzVsYzNNek1CMEdBMVVkRGdRV0JCU2dtSVdENmJQZmJiS2ttVHdPSlJYdkliSDlIakFmQmdOVkhTTUVHREFXZ0JSMllJejdCcUNzWjFjMW5jK2FyS2NybVRXMUx6Qk9CZ05WSFI4RVJ6QkZNRU9nUWFBL2hqMW9kSFJ3T2k4dmRITjBZM0pzTG5waGRHTmhMbWR2ZGk1ellTOURaWEowUlc1eWIyeHNMMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEV1WTNKc01JR3RCZ2dyQmdFRkJRY0JBUVNCb0RDQm5UQnVCZ2dyQmdFRkJRY3dBWVppYUhSMGNEb3ZMM1J6ZEdOeWJDNTZZWFJqWVM1bmIzWXVjMkV2UTJWeWRFVnVjbTlzYkM5VVUxcEZhVzUyYjJsalpWTkRRVEV1WlhoMFoyRjZkQzVuYjNZdWJHOWpZV3hmVkZOYVJVbE9WazlKUTBVdFUzVmlRMEV0TVNneEtTNWpjblF3S3dZSUt3WUJCUVVITUFHR0gyaDBkSEE2THk5MGMzUmpjbXd1ZW1GMFkyRXVaMjkyTG5OaEwyOWpjM0F3RGdZRFZSMFBBUUgvQkFRREFnZUFNQjBHQTFVZEpRUVdNQlFHQ0NzR0FRVUZCd01DQmdnckJnRUZCUWNEQXpBbkJna3JCZ0VFQVlJM0ZRb0VHakFZTUFvR0NDc0dBUVVGQndNQ01Bb0dDQ3NHQVFVRkJ3TURNQW9HQ0NxR1NNNDlCQU1DQTBrQU1FWUNJUUNWd0RNY3E2UE8rTWNtc0JYVXovdjFHZGhHcDdycVNhMkF4VEtTdjgzOElBSWhBT0JOREJ0OSszRFNsaWpvVmZ4enJkRGg1MjhXQzM3c21FZG9HV1ZyU3BHMQ==";
			AssertEquals(expectedDecodedData, System.Text.Encoding.UTF8.GetString(credential.CertificateRawData));
			AssertEquals("", credential.SubjectNameCommonName);
			AssertEquals("", credential.IssuerNameCommonName);
			AssertEquals("", credential.GP_IssueDate.ToShortDateString());
			AssertEquals("", credential.GP_ExpiryDate.ToShortDateString());
			AssertEquals("", credential.GP_UserID);

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SaudiArabia;
			credential = CreateEInvoicingCredential(company.PK, ZGuid.Empty, true);
			expectedDecodedData = "MIID1DCCA3mgAwIBAgITbwAAe3UAYVU34I/+5QABAAB7dTAKBggqhkjOPQQDAjBjMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRwwGgYDVQQDExNUU1pFSU5WT0lDRS1TdWJDQS0xMB4XDTIyMDYxMjE3NDA1MloXDTI0MDYxMTE3NDA1MlowSTELMAkGA1UEBhMCU0ExDjAMBgNVBAoTBWFnaWxlMRYwFAYDVQQLEw1oYXlhIHlhZ2htb3VyMRIwEAYDVQQDEwkxMjcuMC4wLjEwVjAQBgcqhkjOPQIBBgUrgQQACgNCAATTAK9lrTVko9rkq6ZYcc9HDRZP4b9S4zA4Km7YXJ+snTVhLkzU0HsmSX9Un8jDhRTOHDKaft8C/uuUY934vuMNo4ICJzCCAiMwgYgGA1UdEQSBgDB+pHwwejEbMBkGA1UEBAwSMS1oYXlhfDItMjM0fDMtMTEyMR8wHQYKCZImiZPyLGQBAQwPMzAwMDc1NTg4NzAwMDAzMQ0wCwYDVQQMDAQxMTAwMREwDwYDVQQaDAhaYXRjYSAxMjEYMBYGA1UEDwwPRm9vZCBCdXNzaW5lc3MzMB0GA1UdDgQWBBSgmIWD6bPfbbKkmTwOJRXvIbH9HjAfBgNVHSMEGDAWgBR2YIz7BqCsZ1c1nc+arKcrmTW1LzBOBgNVHR8ERzBFMEOgQaA/hj1odHRwOi8vdHN0Y3JsLnphdGNhLmdvdi5zYS9DZXJ0RW5yb2xsL1RTWkVJTlZPSUNFLVN1YkNBLTEuY3JsMIGtBggrBgEFBQcBAQSBoDCBnTBuBggrBgEFBQcwAYZiaHR0cDovL3RzdGNybC56YXRjYS5nb3Yuc2EvQ2VydEVucm9sbC9UU1pFaW52b2ljZVNDQTEuZXh0Z2F6dC5nb3YubG9jYWxfVFNaRUlOVk9JQ0UtU3ViQ0EtMSgxKS5jcnQwKwYIKwYBBQUHMAGGH2h0dHA6Ly90c3RjcmwuemF0Y2EuZ292LnNhL29jc3AwDgYDVR0PAQH/BAQDAgeAMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDAzAnBgkrBgEEAYI3FQoEGjAYMAoGCCsGAQUFBwMCMAoGCCsGAQUFBwMDMAoGCCqGSM49BAMCA0kAMEYCIQCVwDMcq6PO+McmsBXUz/v1GdhGp7rqSa2AxTKSv838IAIhAOBNDBt9+3DSlijoVfxzrdDh528WC37smEdoGWVrSpG1";
			AssertEquals(expectedDecodedData, System.Text.Encoding.UTF8.GetString(credential.CertificateRawData));
			AssertEquals("127.0.0.1", credential.SubjectNameCommonName);
			AssertEquals("TSZEINVOICE-SubCA-1", credential.IssuerNameCommonName);
			AssertEquals("12-Jun-22", credential.GP_IssueDate.ToShortDateString());
			AssertEquals("11-Jun-24", credential.GP_ExpiryDate.ToShortDateString());
			AssertEquals("8D2100469037CEF64725F9AB19194D56D58BA21F", credential.GP_UserID);
		}

		public void TestCountriesWithCertificateDataBase64EncodedTwice()
		{
			AssertContainsExactElementsInAnyOrder(GetExpectedCountriesWithCertificateDataBase64EncodedTwice, EInvoicingCertificateCredential.CountriesWithCertificateDataBase64EncodedTwice);
		}

		string[] GetExpectedCountriesWithCertificateDataBase64EncodedTwice => new string[]
		{
			Core.Constants.CountryCodes.SaudiArabia
		};

		public void TestUserId_IsReadonly_WhenCertificate()
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;

			AssertEquals("UserID is readonly for certificates; it stores the certificate thumbprint and is not user editable", true, credential.GP_UserIDInfo.ReadOnly);
		}

		public void TestUserId_IsEditable_WhenPassword()
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingPasswordCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;

			AssertEquals("Certificates should not be editing user ID", true, credential.GP_UserIDInfo.ReadOnly);
		}

		#region Unique Index Handler

		public void TestUniqueIndexHandler_IsTriggered_ForDuplicateCertificateInSameBranch()
		{
			var branch = CreateBranchForTest();
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;
			credential.GP_MailBoxID = ZString.Empty;
			credential.GP_GC = branch.GB_GC;
			credential.GP_GB = branch.PK;

			using (var rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=Root")))
			using (var childCertificate = SampleCertificateHelper.CreateSampleChildCertificate(rootCertificate, new X500DistinguishedName(SubjectNameSample)))
			{
				var userCredentials = new NetworkCredential("user", "password12345678");
				var certificatePfx = childCertificate.Export(X509ContentType.Pfx, userCredentials.SecurePassword);

				credential.GP_Certificate = certificatePfx;
				credential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				Factory.Save();

				var duplicate = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
				duplicate.CredentialSettings = settingsMock.Object;
				duplicate.GP_MailBoxID = ZString.Empty;
				duplicate.GP_GC = branch.GB_GC;
				duplicate.GP_GB = branch.PK;
				duplicate.GP_Certificate = certificatePfx;
				duplicate.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				AssertNotEquals("Precondition: two different bizos", credential.PK, duplicate.PK);
				AssertEquals("Precondition: bizos have same branch", credential.GP_GB, duplicate.GP_GB);

				try
				{
					Factory.Save();
					Fail("Expected unique index violation");
				}
				catch (ZSaveException ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);
					Assert("Unique index handler should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals($@"This Certificate has already been entered.

Company: '{branch.Company.GC_Code}', Branch: '{branch.GB_Code}', Password Type: '{credential.GP_PasswordType}', Thumb-print: '{childCertificate.Thumbprint}'.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUniqueIndexHandler_IsTriggered_ForDuplicateCertificateInSameCompany()
		{
			var company = CreateCompanyForTest();
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;
			credential.GP_MailBoxID = ZString.Empty;
			credential.GP_GC = company.PK;
			credential.GP_GB = ZGuid.Empty;

			using (var rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=Root")))
			using (var childCertificate = SampleCertificateHelper.CreateSampleChildCertificate(rootCertificate, new X500DistinguishedName(SubjectNameSample)))
			{
				var userCredentials = new NetworkCredential("user", "password12345678");
				var certificatePfx = childCertificate.Export(X509ContentType.Pfx, userCredentials.SecurePassword);

				credential.GP_Certificate = certificatePfx;
				credential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				Factory.Save();

				var duplicate = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
				duplicate.CredentialSettings = settingsMock.Object;
				duplicate.GP_MailBoxID = ZString.Empty;
				duplicate.GP_GC = company.PK;
				duplicate.GP_GB = ZGuid.Empty;
				duplicate.GP_Certificate = certificatePfx;
				duplicate.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				AssertNotEquals("Precondition: two different bizos", credential.PK, duplicate.PK);
				AssertEquals("Precondition: bizos have same company", credential.GP_GC, duplicate.GP_GC);

				try
				{
					Factory.Save();
					Fail("Expected unique index violation");
				}
				catch (ZSaveException ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);
					Assert("Unique index handler should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals($@"This Certificate has already been entered.

Company: '{company.GC_Code}', Branch: '', Password Type: '{credential.GP_PasswordType}', Thumb-print: '{childCertificate.Thumbprint}'.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUniqueIndexHandler_IsNotTriggered_ForDuplicateCertificateInDifferentBranch()
		{
			var branch = CreateBranchForTest();
			var otherBranch = CreateBranchForTest();
			otherBranch.GB_GC = branch.GB_GC;
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;
			credential.GP_MailBoxID = ZString.Empty;
			credential.GP_GC = branch.GB_GC;
			credential.GP_GB = branch.PK;

			using (var rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=Root")))
			using (var childCertificate = SampleCertificateHelper.CreateSampleChildCertificate(rootCertificate, new X500DistinguishedName(SubjectNameSample)))
			{
				var userCredentials = new NetworkCredential("user", "password12345678");
				var certificatePfx = childCertificate.Export(X509ContentType.Pfx, userCredentials.SecurePassword);

				credential.GP_Certificate = certificatePfx;
				credential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				Factory.Save();

				var duplicate = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
				duplicate.CredentialSettings = settingsMock.Object;
				duplicate.GP_MailBoxID = ZString.Empty;
				duplicate.GP_GC = branch.GB_GC;
				duplicate.GP_GB = otherBranch.PK;
				duplicate.GP_Certificate = certificatePfx;
				duplicate.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				AssertNotEquals("Precondition: two different bizos", credential.PK, duplicate.PK);
				AssertNotEquals("Precondition: bizos have different branch", credential.GP_GB, duplicate.GP_GB);

				AssertNoExceptionThrown("The same certificate should be saved in different branches", () => Factory.Save());
				AssertEquals("Credential is in database", true, credential.IsInDatabase);
				AssertEquals("Duplicate is in database", true, duplicate.IsInDatabase);
			}
		}

		public void TestUniqueIndexHandler_IsNotTriggered_ForDuplicateCertificateInDifferentCompany()
		{
			var company = CreateCompanyForTest();
			var otherCompany = CreateCompanyForTest();
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			credential.CredentialSettings = settingsMock.Object;
			credential.GP_MailBoxID = ZString.Empty;
			credential.GP_GC = company.PK;
			credential.GP_GB = ZGuid.Empty;

			using (var rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=Root")))
			using (var childCertificate = SampleCertificateHelper.CreateSampleChildCertificate(rootCertificate, new X500DistinguishedName(SubjectNameSample)))
			{
				var userCredentials = new NetworkCredential("user", "password12345678");
				var certificatePfx = childCertificate.Export(X509ContentType.Pfx, userCredentials.SecurePassword);

				credential.GP_Certificate = certificatePfx;
				credential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				Factory.Save();

				var duplicate = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
				duplicate.CredentialSettings = settingsMock.Object;
				duplicate.GP_MailBoxID = ZString.Empty;
				duplicate.GP_GC = otherCompany.PK;
				duplicate.GP_GB = ZGuid.Empty;
				duplicate.GP_Certificate = certificatePfx;
				duplicate.CurrentDecryptedCertificatePassphrase = userCredentials.Password;

				AssertNotEquals("Precondition: two different bizos", credential.PK, duplicate.PK);
				AssertNotEquals("Precondition: bizos have different companies", credential.GP_GC, duplicate.GP_GC);

				AssertNoExceptionThrown("The same certificate should be saved in different companies", () => Factory.Save());
				AssertEquals("Credential is in database", true, credential.IsInDatabase);
				AssertEquals("Duplicate is in database", true, duplicate.IsInDatabase);
			}
		}

		#endregion

		#region GetWarningBeforeDelete

		public void TestGetWarningBeforeDelete_DoesNotThrow_WhenCertificateNotPartOfACollection()
		{
			var certificateCredential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var certificateSettingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingCertificateCredentialSettings>();
			certificateCredential.CredentialSettings = certificateSettingsMock.Object;
			AssertNullOrEmpty("GetWarningBeforeDelete on certificate credential should be empty (and not throw) if not part of a collection", certificateCredential.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeDelete_DoesNotThrow_WhenPasswordNotPartOfACollection()
		{
			var passwordCredential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			var passwordSettingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMock<IEInvoicingPasswordCredentialSettings>();
			passwordCredential.CredentialSettings = passwordSettingsMock.Object;
			AssertNullOrEmpty("GetWarningBeforeDelete on password credential should be empty (and not throw) if not part of a collection", passwordCredential.GetWarningBeforeBeingDeleted());
		}

		#endregion

		#region CertificateHasExpired

		public void TestCertificateHasExpired()
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;

			credential.GP_Certificate = new byte[4];
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("True when certificate data and expiry in past", true, credential.CertificateHasExpired);

			credential.GP_Certificate = ZBlob.Empty;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertEquals("False when no certificate", false, credential.CertificateHasExpired);

			credential.GP_Certificate = new byte[4];
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertEquals("False when expiry in future", false, credential.CertificateHasExpired);
		}

		#endregion

		#region IsValidCertificate

		public void TestIsValidCertificate()
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;

			credential.GP_Certificate = new byte[4];
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertEquals("True when valid status, certificate data and expiry in future", true, credential.IsValidCertificate);

			credential.GP_Certificate = new byte[4];
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertEquals("False when invalid status", false, credential.IsValidCertificate);

			credential.GP_Certificate = ZBlob.Empty;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertEquals("False when no certificate", false, credential.IsValidCertificate);

			credential.GP_Certificate = new byte[4];
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("False when expiry in past", false, credential.IsValidCertificate);
		}

		#endregion

		#region PreferredSequenceNumber

		public void TestPreferredSequenceNumber()
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			AssertEquals("PreferredSequenceNumber is zero by default", 0, credential.PreferredSequenceNumber);
			AssertEquals("PreferredSequenceNumber is readonly", true, credential.PreferredSequenceNumberInfo.ReadOnly);
			AssertEquals("PreferredSequenceNumberForDisplay is empty when zero", "", credential.PreferredSequenceNumberForDisplay);

			credential.PreferredSequenceNumber = 1;
			AssertEquals("PreferredSequenceNumber can be set programatically", 1, credential.PreferredSequenceNumber);
			AssertEquals("PreferredSequenceNumberForDisplay is numeric when non-zero", "1", credential.PreferredSequenceNumberForDisplay);

			credential.PreferredSequenceNumber = 1234;
			AssertEquals("PreferredSequenceNumberForDisplay is numeric when non-zero", "1234", credential.PreferredSequenceNumberForDisplay);

			credential.PreferredSequenceNumber = -1;
			AssertEquals("PreferredSequenceNumberForDisplay is empty when less than zero", "", credential.PreferredSequenceNumberForDisplay);
		}

		#endregion

		#region PasswordStatus

		public void TestPasswordStatus()
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;

			credential.GP_Certificate = new byte[4];
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			credential.GP_ExpiryDate = ZDateTime.Invalid;
			AssertEquals("Uses Lookup Description when no expiry", "Invalid", credential.PasswordStatus);

			credential.GP_Certificate = new byte[4];
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertEquals("Uses Lookup Description when not expired", "Deactivated", credential.PasswordStatus);

			credential.GP_Certificate = new byte[4];
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("Uses 'Expired' when valid but expired", "Expired", credential.PasswordStatus);

			credential.GP_Certificate = ZBlob.Empty;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("Uses Lookup Description when no certificate (no cert + Valid may occur for password based credentials)", "Valid", credential.PasswordStatus);
		}

		#endregion

		#region ExpiryDate and IssueDate

		public void TestCertificateDates_ForCompanies()
		{
			var company = CreateCompanyForTest();
			var credential = CreateEInvoicingCredential(company.PK, ZGuid.Empty);
			var expectedIssue = new ZDateTime("21/07/2017 3:14:15");
			var expectedExpiryDate = new ZDateTime("21/07/2020 3:24:00");

			AssertEquals(expectedIssue, credential.GP_IssueDate);
			AssertEquals(expectedExpiryDate, credential.GP_ExpiryDate);
		}

		public void TestCertificateDates_ForCompanies_NullData()
		{
			var company = Factory.New<GlbCompany>();
			var credential = CreateEInvoicingCredential(company.PK, ZGuid.Empty);
			AssertCertificateDatesNotFormatted();

			var org = Factory.New<OrgHeader>();
			company.GC_OH_OrgProxy = org.PK;
			credential = CreateEInvoicingCredential(company.PK, ZGuid.Empty);
			AssertCertificateDatesNotFormatted();

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			var orgAddress = Factory.Load<OrgAddress>(org.MainAddress.PK);
			orgAddress.OA_RL_NKRelatedPortCode = unloco.RL_Code;
			credential = CreateEInvoicingCredential(company.PK, ZGuid.Empty);
			AssertCertificateDatesNotFormatted();

			void AssertCertificateDatesNotFormatted()
			{
				using (var certificate = new X509Certificate2(credential.GP_Certificate, credential.CurrentDecryptedCertificatePassphrase))
				{
					AssertEquals(certificate.NotBefore, credential.GP_IssueDate);
					AssertEquals(certificate.NotAfter, credential.GP_ExpiryDate);
				}
			}
		}

		public void TestCertificateDates_ForBranches()
		{
			var expectedIssue = new ZDateTime("21/07/2017 5:14:15");
			var expectedExpiryDate = new ZDateTime("21/07/2020 5:24:00");
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "UYMVD";

			var credential = CreateEInvoicingCredential(ZGuid.Empty, branch.PK);
			AssertCertificateDates_ForBranches();

			credential = CreateEInvoicingCredential(branch.Company.PK, branch.PK);
			AssertCertificateDates_ForBranches();

			void AssertCertificateDates_ForBranches()
			{
				AssertEquals(expectedIssue, credential.GP_IssueDate);
				AssertEquals(expectedExpiryDate, credential.GP_ExpiryDate);
			}
		}

		public void TestCertificateDates_ForBranches_NullData()
		{
			var branch = Factory.New<GlbBranch>();
			var credential = CreateEInvoicingCredential(ZGuid.Empty, branch.PK);
			AssertCertificateDatesNotFormatted();

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			branch.GB_RL_NKHomePort = unloco.Code;
			credential = CreateEInvoicingCredential(ZGuid.Empty, branch.PK);
			AssertCertificateDatesNotFormatted();

			void AssertCertificateDatesNotFormatted()
			{
				using (var certificate = new X509Certificate2(credential.GP_Certificate, credential.CurrentDecryptedCertificatePassphrase))
				{
					AssertEquals(certificate.NotBefore, credential.GP_IssueDate);
					AssertEquals(certificate.NotAfter, credential.GP_ExpiryDate);
				}
			}
		}

		public void TestCertificateDates_WithOutParent()
		{
			var credential = CreateEInvoicingCredential(ZGuid.Empty, ZGuid.Empty);
			using (var certificate = new X509Certificate2(credential.GP_Certificate, credential.CurrentDecryptedCertificatePassphrase))
			{
				AssertEquals(certificate.NotBefore, credential.GP_IssueDate);
				AssertEquals(certificate.NotAfter, credential.GP_ExpiryDate);
			}
		}

		#endregion

		#region Implementation

		GlbBranch CreateBranchForTest(string countryCode = "BR")
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "BRBSB";
			return branch;
		}

		GlbCompany CreateCompanyForTest()
		{
			var unloco = Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, "MXMEX"))?.FirstOrDefault();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgMainAddress = Factory.Load<OrgAddress>(org.MainAddress.PK);
			orgMainAddress.OA_RL_NKRelatedPortCode = unloco.RL_Code;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org.PK;

			return company;
		}

		EInvoicingCertificateCredential CreateEInvoicingCredential(ZGuid companyPk, ZGuid branchPk, bool useBase64EncodedTwiceData = false)
		{
			var credential = CreateNewGlbExternalPassword(Factory) as EInvoicingCertificateCredential;
			credential.GP_GC = companyPk;
			credential.GP_GB = branchPk;
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_Certificate = useBase64EncodedTwiceData ? X509Certificate2TestHelper.ValidCertificateSA : X509Certificate2TestHelper.ValidCertificate;

			return credential;
		}

		#endregion
	}
}
