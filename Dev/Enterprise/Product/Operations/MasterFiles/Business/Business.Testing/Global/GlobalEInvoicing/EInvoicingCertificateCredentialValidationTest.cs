using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal abstract class EInvoicingCertificateCredentialValidationTest<T, S> : GlbExternalPasswordValidationTest<T, S> where T : EInvoicingCertificateCredential where S : EInvoicingCertificateCredentialValidation
	{
		protected virtual string IssuerNameSample => "CN = VMS ICA1 Staging;O = FRCS;C = FJ ";
		protected virtual string SubjectNameSample => @"CN = PTY7 Wisetech Global Limited;OU = Wisetech Global Limited;O = Wisetech Global Limited;STREET = 72 O'Riodran St Post Code 2015;L = Alexendria NSW;S = UNKNOWN;C = AU ";

		public void TestCertificateValidPeriod()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");
			var settingsMock = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: CountryCodes.Brazil).WithExpiryWarningDays(2);

			var branchCredential = Factory.NewWithValidTestData<T>();
			branchCredential.CredentialSettings = settingsMock.Object;

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(IssuerNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			{
				using (var futureValidChild = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(14)))
				{
					branchCredential.GP_Certificate = futureValidChild.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertHasWarning(branchCredential.GP_IssueDateInfo, "Certificate is not yet valid");
					AssertNoErrors(branchCredential.GP_ExpiryDateInfo);
					AssertNoWarnings(branchCredential.GP_ExpiryDateInfo);
				}

				using (var nowValidChild = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
				{
					branchCredential.GP_Certificate = nowValidChild.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertNoWarnings(branchCredential.GP_IssueDateInfo);
					AssertNoErrors(branchCredential.GP_ExpiryDateInfo);
					AssertNoWarnings(branchCredential.GP_ExpiryDateInfo);
				}

				using (var soonInvalidChild = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(2)))
				{
					branchCredential.GP_Certificate = soonInvalidChild.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertNoWarnings(branchCredential.GP_IssueDateInfo);
					AssertHasWarning(branchCredential.GP_ExpiryDateInfo, "Certificate will expire soon, please renew");
				}

				using (var pastValidChild = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-14), ZDateTime.Today.AddDays(-7)))
				{
					branchCredential.GP_Certificate = pastValidChild.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertNoWarnings(branchCredential.GP_IssueDateInfo);
					AssertHasError(branchCredential.GP_ExpiryDateInfo, "Certificate is expired");
					AssertNoWarnings(branchCredential.GP_ExpiryDateInfo);
				}

				using (var pastValidChild = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-14), ZDateTime.Today.AddDays(-7)))
				{
					branchCredential.GP_Certificate = pastValidChild.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.Factory.Save();
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertNoWarnings(branchCredential.GP_IssueDateInfo);
					AssertHasWarning(branchCredential.GP_ExpiryDateInfo, "The certificate has already expired, please renew");
				}
			}
		}

		public void TestCertificateSelfSignedNotAllowed()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");

			var branchCredential = Factory.NewWithValidTestData<T>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("CN=SelfSigned;SERIALNUMBER=BLAH", X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			{
				branchCredential.GP_Certificate = root.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate has not been issued correctly because the certificate is self-signed.");
			}
		}

		public void TestCertificateStrangeCommonName()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");

			var branchCredential = Factory.NewWithValidTestData<T>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(IssuerNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName("CN = Some name with an = in it", X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
			{
				branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				AssertEquals(nameof(branchCredential.SubjectNameCommonName), "\"Some name with an = in it\"", branchCredential.SubjectNameCommonName);
			}
		}

		public void TestCertificateMissingCommonNameNotAllowed()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");

			var branchCredential = Factory.NewWithValidTestData<T>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(IssuerNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName("OU = Some OU;SERIALNUMBER=BLAH", X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
			{
				branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate has not been issued correctly because the Subject field is missing a Common Name.");
			}
		}

		public void TestCertificateIssuerMissingCommonNameNotAllowed()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");

			var branchCredential = Factory.NewWithValidTestData<T>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName("SERIALNUMBER = BLAH", X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
			{
				branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
				AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate has not been issued correctly because the Issuer field is missing a Common Name.");
			}
		}

		public void TestCertificateIncorrectPassPhrase()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");
			var branchCredential = Factory.NewWithValidTestData<T>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(IssuerNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
			{
				branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				branchCredential.CurrentDecryptedCertificatePassphrase = "TheWrongPassPhrase";
				AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate cannot be read because the certificate file is corrupt or the pass phrase is incorrect.");
			}
		}

		public void TestCertificatePassPhraseIsNotEntered()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");
			var branchCredential = Factory.NewWithValidTestData<T>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(IssuerNameSample, X500DistinguishedNameFlags.UseSemicolons)))
			using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons)))
			{
				branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
				AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate cannot be read because the pass phrase is blank and the certificate file requires a pass phrase.");
			}
		}

		public void TestMissingCertificate()
		{
			var branchCredential = Factory.NewWithValidTestData<T>();
			branchCredential.CurrentDecryptedCertificatePassphrase = "AAA";
			AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate must be loaded.");
		}

		public void TestPassPhrase()
		{
			var branchCredential = Factory.NewWithValidTestData<T>();
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "A12";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SaudiArabia;
			branchCredential.GP_GC = company1.PK;

			branchCredential.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertHasError(branchCredential.CurrentDecryptedCertificatePassphraseInfo, "Please enter a Pass Phrase.");

			branchCredential.CurrentDecryptedCertificatePassphrase = "abcdefg";
			AssertHasWarning(branchCredential.CurrentDecryptedCertificatePassphraseInfo, "Invalid Base64 encoded string has been entered. Please confirm this has been entered correctly.");

			branchCredential.CurrentDecryptedCertificatePassphrase = "aGVsbG8gd29ybGQ=";
			AssertHasWarning(branchCredential.CurrentDecryptedCertificatePassphraseInfo, "Unexpected length of Pass Phrase. Please confirm this has been entered correctly.");

			branchCredential.CurrentDecryptedCertificatePassphrase = "Xlj15LyMCgSC66ObnEO/qVPfhSbs3kDTjWnGheYhfSs=";
			AssertNoWarnings(branchCredential.CurrentDecryptedCertificatePassphraseInfo);
		}
	}

	[TestedType(typeof(EInvoicingCertificateCredentialValidation))]
	internal class GlbBranchEInvoicingCredentialValidationTest : EInvoicingCertificateCredentialValidationTest<GlbBranchEInvoicingCertificateCredential, EInvoicingCertificateCredentialValidation>
	{
	}

	[TestedType(typeof(EInvoicingCertificateCredentialValidation))]
	internal class GlbCompanyEInvoicingCredentialValidationTest : EInvoicingCertificateCredentialValidationTest<GlbCompanyEInvoicingCertificateCredential, EInvoicingCertificateCredentialValidation>
	{
	}
}
