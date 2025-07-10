using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbExternalPasswordWithCertificateValidationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCheckCurrentDecryptedCertificatePassphrase()
		{
			var sub = Factory.New<GlbExternalPasswordWithCertificateForTest>();

			sub.GP_Certificate = new byte[] { 120, 200 };
			sub.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertHasErrorContaining(sub.CurrentDecryptedCertificatePassphraseInfo, MandatoryValidation.MustBeEntered);

			sub.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			AssertNoErrorContaining(sub.CurrentDecryptedCertificatePassphraseInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckGP_ExpiryDate()
		{
			var glbExternalPassword = Factory.New<GlbExternalPasswordWithCertificateForTest>();
			var errorCertiticateExpired = "The certificate has expired.";
			glbExternalPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertHasWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateExpired);
			glbExternalPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateExpired);
			glbExternalPassword.GP_ExpiryDate = ZDateTime.Empty;
			AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateExpired);
		}

		class GlbExternalPasswordWithCertificateForTest : GlbExternalPasswordWithCertificate
		{
			public GlbExternalPasswordWithCertificateForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			protected override GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPasswordWithCertificateValidationForTest(this);
		}

		class GlbExternalPasswordWithCertificateValidationForTest : GlbExternalPasswordWithCertificateValidation
		{
			public GlbExternalPasswordWithCertificateValidationForTest(GlbExternalPasswordWithCertificate parent) : base(parent)
			{
			}
		}
	}
}
