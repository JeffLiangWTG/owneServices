using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbExternalPasswordWithCertificateValidation : GlbExternalPasswordValidation
	{
		protected GlbExternalPasswordWithCertificateValidation(GlbExternalPasswordWithCertificate parent)
			: base(parent)
		{
		}

		protected new GlbExternalPasswordWithCertificate Parent => (GlbExternalPasswordWithCertificate)base.Parent;

		protected override void CheckCurrentDecryptedCertificatePassphrase()
		{
			base.CheckCurrentDecryptedCertificatePassphrase();
			CheckCurrentDecryptedCertificatePassphraseCore();

			ValidateGP_Certificate();
		}

		protected virtual void CheckCurrentDecryptedCertificatePassphraseCore()
		{
			if (IsCurrentDecryptedCertificatePassphraseMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.CurrentDecryptedCertificatePassphraseInfo);
			}
		}

		protected virtual bool IsCurrentDecryptedCertificatePassphraseMandatory => true;

		protected override void CheckGP_Certificate()
		{
			base.CheckGP_Certificate();
			CheckGP_CertificateCore();
		}

		protected virtual void CheckGP_CertificateCore()
		{
			var certificateStatus = Parent.CertificateStatus;
			if (certificateStatus == GlbExternalPasswordWithCertificate.CertificateInvalid)
			{
				Parent.GP_CertificateInfo.AddError(Res.GetString("94B97FC2-EBB5-4E07-AD7B-6C83B2157E0F", "The Certificate or accompanying password is invalid."));
			}
			else if (IsCertificateMandatory && certificateStatus == GlbExternalPasswordWithCertificate.CertificateEmpty)
			{
				Parent.GP_CertificateInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.GP_CertificateInfo.HumanReadableName));
			}
		}

		protected virtual bool IsCertificateMandatory => true;

		protected override void CheckGP_ExpiryDate()
		{
			base.CheckGP_ExpiryDate();
			var expiryDate = Parent.GP_ExpiryDate;
			if (!expiryDate.IsEmpty && expiryDate < ZDateTime.Now)
			{
				AddGP_ExpiryDateExpiredNotification();
			}
		}

		protected virtual void AddGP_ExpiryDateExpiredNotification()
		{
			Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("2ABC1E80-2010-4809-9244-DAA190F1B699", "The certificate has expired."));
		}
	}
}
