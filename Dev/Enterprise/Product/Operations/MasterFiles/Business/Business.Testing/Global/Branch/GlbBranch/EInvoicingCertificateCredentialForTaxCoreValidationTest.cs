using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateCredentialForTaxCoreValidation))]
	internal class EInvoicingCertificateCredentialForTaxCoreValidationTest : EInvoicingCertificateCredentialValidationTest<EInvoicingCertificateCredentialForTaxCore, EInvoicingCertificateCredentialForTaxCoreValidation>
	{
		protected override string SubjectNameSample => @"CN = PTY7 Wisetech Global Limited;SERIALNUMBER = PTY7TTHW;OU = Wisetech Global Limited;O = Wisetech Global Limited;STREET = 72 O'Riodran St Post Code 2015;L = Alexendria NSW;S = UNKNOWN;C = AU ";

		public void TestSubjectSerialNumberAndCertificateAuthority()
		{
			var userCredentials = new NetworkCredential("user", "password12345678");

			var branchCredential = Factory.NewWithValidTestData<EInvoicingCertificateCredentialForTaxCore>();

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(IssuerNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(5)))
			{
				using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName("CN=CHILD", X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
				{
					branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertEquals(nameof(branchCredential.IssuerNameCommonName), "VMS ICA1 Staging", branchCredential.IssuerNameCommonName);
					AssertEquals(nameof(branchCredential.SerialNumber), ZString.Empty, branchCredential.SerialNumber);
					AssertEquals(nameof(branchCredential.SubjectNameCommonName), "CHILD", branchCredential.SubjectNameCommonName);
					AssertHasError(branchCredential.GP_PasswordStatusInfo, "Certificate has not been issued correctly because the Subject field is missing a Serial Number.");
				}

				using (var child = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(SubjectNameSample, X500DistinguishedNameFlags.UseSemicolons), ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(7)))
				{
					branchCredential.GP_Certificate = child.Export(X509ContentType.Pfx, userCredentials.SecurePassword);
					branchCredential.CurrentDecryptedCertificatePassphrase = userCredentials.Password;
					AssertEquals(nameof(branchCredential.IssuerNameCommonName), "VMS ICA1 Staging", branchCredential.IssuerNameCommonName);
					AssertEquals(nameof(branchCredential.SerialNumber), "PTY7TTHW", branchCredential.SerialNumber);
					AssertEquals(nameof(branchCredential.SubjectNameCommonName), "PTY7 Wisetech Global Limited", branchCredential.SubjectNameCommonName);
					AssertNoErrors(branchCredential.GP_PasswordStatusInfo);
				}
			}
		}

		public void TestMissingMailBoxID()
		{
			var branchCredential = Factory.NewWithValidTestData<EInvoicingCertificateCredentialForTaxCore>();
			branchCredential.GP_MailBoxID = ZString.Empty;
			AssertHasError(branchCredential.GP_MailBoxIDInfo, "Please enter a PAC.");
		}
	}
}
