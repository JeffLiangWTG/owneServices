using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanySignatureCredentialValidation : GlbExternalPasswordValidation
	{
		public GlbCompanySignatureCredentialValidation(AutoGlbExternalPassword parent) : base(parent)
		{
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();
			MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();
			MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo);
		}
	}
}