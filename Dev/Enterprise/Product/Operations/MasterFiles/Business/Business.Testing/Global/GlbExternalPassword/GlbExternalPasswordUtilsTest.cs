using System.Security.Cryptography;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GlbExternalPasswordUtilsTest : TestCaseWithFactory
	{
		public void TestExportPEMString_RSA()
		{
			var testPassword = Factory.New<GlbExternalPassword>();
			testPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			testPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			(var certKey, var cert) = testPassword.ExportRSAKeyAndCertificateStringAsPEM();

			AssertMultilineASCIIEquals(X509Certificate2TestHelper.ValidCertificate_KeyPEM, certKey);
			AssertMultilineASCIIEquals(X509Certificate2TestHelper.ValidCertificate_CertPEM, cert);
		}

		public void TestExportPEMString_PrivateKeyNotInRSAFormat()
		{
			var testPassword = Factory.New<GlbExternalPassword>();
			testPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidDSAPassword;
			testPassword.GP_Certificate = X509Certificate2TestHelper.ValidDSACertificate;
			AssertExceptionThrown<CryptographicException>("Exception should be thrown as Private Key in DSA, not RSA format", "Private key is either null or not in expected RSA form.", () => testPassword.ExportRSAKeyAndCertificateStringAsPEM());
		}

		public void TestExportPEMString_Empty()
		{
			var testPassword = Factory.New<GlbExternalPassword>();

			(var certKey, var cert) = testPassword.ExportRSAKeyAndCertificateStringAsPEM();

			AssertEquals(ZString.Empty, certKey);
			AssertEquals(ZString.Empty, cert);
		}

		public void TestExportPEMString_WrongPassword()
		{
			var testPassword = Factory.New<GlbExternalPassword>();
			testPassword.CurrentDecryptedCertificatePassphrase = "WRONGPASSWORD";
			testPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			AssertExceptionThrown<CryptographicException>("Exception should be thrown as valid certificate object cannot be built from Cert/Incorrect Password combination", () => testPassword.ExportRSAKeyAndCertificateStringAsPEM());
		}

		public void TestExportPEMString_InvalidData()
		{
			var testPassword = Factory.New<GlbExternalPassword>();
			testPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			testPassword.GP_Certificate = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			AssertExceptionThrown<CryptographicException>("Exception should be thrown as valid certificate object cannot be built from Invalid Cert/Password combination", () => testPassword.ExportRSAKeyAndCertificateStringAsPEM());
		}

		public void TestExportPEMString_NoPrivateKey()
		{
			var testPassword = Factory.New<GlbExternalPassword>();
			testPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			testPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate_NoPrivateKey;

			AssertExceptionThrown<CryptographicException>("Exception should be thrown as Private Key does not exist", "Certificate does not contain a private key.", () => testPassword.ExportRSAKeyAndCertificateStringAsPEM());
		}
	}
}
