using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class GlbCompanyCredentialValidation : GlbExternalPasswordWithCertificateValidation
	{
		public GlbCompanyCredentialValidation(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected new GlbCompanyCredential Parent => (GlbCompanyCredential)base.Parent;

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.GP_UserIDInfo);
			if (Parent.GP_UserID.Length > 8)
			{
				Parent.GP_UserIDInfo.AddMessageError(Res.GetString("53DAC9E1-5B10-4A43-943D-5FEF51E127F8", "Username must be less than 9 characters."));
			}
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CurrentDecryptedPasswordInfo);
			if (Parent.CurrentDecryptedPassword.Length > 15)
			{
				Parent.CurrentDecryptedPasswordInfo.AddMessageError(Res.GetString("9004BA91-F3EB-47DF-9D62-5593AC3A3287", "Password must be less than 16 characters."));
			}
		}

		protected override void CheckCurrentDecryptedCertificatePassphrase()
		{
			base.CheckCurrentDecryptedCertificatePassphrase();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CurrentDecryptedCertificatePassphraseInfo);
		}

		protected override bool IsCertificateMandatory => false;
		protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => false;
	}
}
