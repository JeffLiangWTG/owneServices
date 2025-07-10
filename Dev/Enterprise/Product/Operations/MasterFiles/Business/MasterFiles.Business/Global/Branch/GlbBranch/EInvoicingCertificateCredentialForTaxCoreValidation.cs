using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingCertificateCredentialForTaxCoreValidation : EInvoicingCertificateCredentialValidation
	{
		public EInvoicingCertificateCredentialForTaxCoreValidation(EInvoicingCertificateCredentialForTaxCore parent)
			: base(parent)
		{
		}

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();

			MandatoryValidation.CheckEntered(Parent.GP_MailBoxIDInfo, Res.GetString("85e0e0d0-9c2a-4537-b562-e2b44024fbc7", "PAC"));
		}

		protected override void CheckGP_PasswordStatus()
		{
			base.CheckGP_PasswordStatus();

			if (string.IsNullOrEmpty(Parent.SerialNumber))
			{
				Parent.GP_PasswordStatusInfo.AddError(Res.GetString("58e26ba2-e1a9-4932-a68b-95802791dbea", "Certificate has not been issued correctly because the Subject field is missing a Serial Number."));
			}
		}
	}
}
