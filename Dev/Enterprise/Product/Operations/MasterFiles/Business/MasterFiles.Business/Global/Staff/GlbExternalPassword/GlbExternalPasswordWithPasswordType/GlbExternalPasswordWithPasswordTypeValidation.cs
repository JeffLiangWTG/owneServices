using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordWithPasswordTypeValidation : GlbExternalPasswordValidation
	{
		public GlbExternalPasswordWithPasswordTypeValidation(GlbExternalPasswordWithPasswordType parent)
			: base(parent)
		{
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();

			if (!Parent.GP_UserID.IsEmpty && Parent.GP_PasswordStatus.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo);
			}
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			if (!Parent.CurrentDecryptedPassword.IsEmpty && Parent.GP_PasswordStatus.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
			}
		}

		protected override void CheckGP_StatusReason()
		{
			base.CheckGP_StatusReason();

			if (!Parent.GP_StatusReason.IsEmpty)
			{
				Parent.GP_StatusReasonInfo.AddWarning(Res.GetString("73b1869d-77da-44aa-806a-91e6f4fc89b6", "Please correct the error and re-enter your {0}", Parent.CurrentDecryptedPasswordInfo.HumanReadableName));
			}
		}
	}
}
