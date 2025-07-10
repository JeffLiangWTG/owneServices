using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class GlbCompanyCredentialValidation : TW.Business.GlbExternalPasswordValidationBase_TW
	{
		public GlbCompanyCredentialValidation(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected new GlbCompanyCredential Parent => (GlbCompanyCredential)base.Parent;

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();
			if (!Parent.IsEmpty)
			{
				var mailBoxID = Parent.GP_MailBoxID;
				var mailBoxIDInfo = Parent.GP_MailBoxIDInfo;
				if (mailBoxID.IsEmpty)
				{
					mailBoxIDInfo.AddError(MandatoryValidation.MustBeEnteredMessage(mailBoxIDInfo.HumanReadableName));
				}
				else if (!new Regex("^[A-Z0-9]{7,}$").IsMatch(mailBoxID))
				{
					mailBoxIDInfo.AddError(Res.GetString("7B2AD4CE-33F7-479D-BBAF-2D9B9CE04F32", "The entered Mailbox is invalid, the Mail Box format must be 7 characters."));
				}
			}
		}

		protected override void CheckCurrentDecryptedPasswordCore()
		{
			if (!Parent.IsEmpty)
			{
				base.CheckCurrentDecryptedPasswordCore();
			}
		}

		protected override void CheckCurrentDecryptedCertificatePassphraseCore()
		{
			if (!Parent.IsEmpty)
			{
				base.CheckCurrentDecryptedCertificatePassphraseCore();
			}
		}

		protected override void CheckGP_CertificateCore()
		{
			if (!Parent.IsEmpty)
			{
				base.CheckGP_CertificateCore();
			}
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();
			var parent = Parent;
			if (parent.CertificateStatus == GlbExternalPasswordWithCertificate.CertificateLoaded)
			{
				MandatoryValidation.CheckEntered(parent.GP_UserIDInfo);
			}
		}
	}
}
