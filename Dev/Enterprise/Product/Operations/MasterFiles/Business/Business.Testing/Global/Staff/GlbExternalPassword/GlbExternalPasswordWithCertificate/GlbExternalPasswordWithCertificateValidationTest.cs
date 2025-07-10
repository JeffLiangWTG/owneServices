using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPasswordWithCertificateValidation))]
	public abstract class GlbExternalPasswordWithCertificateValidationTest<T, TValidation> : GlbExternalPasswordValidationTest<T, TValidation>
			where T : GlbExternalPasswordWithCertificate
			where TValidation : GlbExternalPasswordWithCertificateValidation
	{
		[TestDate(2018, 12, 18)]
		public void TestCheckGP_Certificate()
		{
			var sub = GlbExternalPassword;

			sub.GP_Certificate = new byte[] { 241, 40 };
			sub.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			var certificateInvalidMessage = "The Certificate or accompanying password is invalid.";
			AssertHasError(sub.GP_CertificateInfo, certificateInvalidMessage);

			sub.GP_Certificate = ValidCertificate;
			AssertHasError(sub.GP_CertificateInfo, certificateInvalidMessage);
			sub.CurrentDecryptedCertificatePassphrase = ValidPassword;
			sub.Validation.ValidateGP_Certificate();
			AssertNoError(sub.GP_CertificateInfo, certificateInvalidMessage);

			sub.CurrentDecryptedCertificatePassphrase = "password";
			sub.Validation.ValidateGP_Certificate();
			AssertHasError(sub.GP_CertificateInfo, certificateInvalidMessage);

			sub.GP_Certificate = null;
			sub.Validation.ValidateGP_Certificate();
			if (IsCertificateMandatory)
			{
				AssertHasErrorContaining(sub.GP_CertificateInfo, MandatoryValidation.MustBeEntered);
			}
			else
			{
				Assert("It doesn't need validation", true);
			}
		}
		protected virtual string ValidPassword => X509Certificate2TestHelper.ValidPassword;
		protected virtual byte[] ValidCertificate => X509Certificate2TestHelper.ValidCertificate;

		protected virtual bool IsCertificateMandatory => true;
	}
}
