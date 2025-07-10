using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class GlbExternalPasswordValidation_SG : GlbExternalPasswordValidation
	{
		protected GlbExternalPasswordValidation_SG(GlbExternalPassword_SG parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_SG Parent
		{
			get { return (GlbExternalPassword_SG)base.Parent; }
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();
			if (!Parent.GP_MailBoxID.IsEmpty)
			{
				if (Parent.CurrentDecryptedPassword.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo, Res.GetString("01a484aa-ee26-4dfd-834b-4635473d582d", "Current Password"));
				}
			}

			ValidateNextDecryptedPassword();
		}

		protected override void CheckNextDecryptedPassword()
		{
			base.CheckNextDecryptedPassword();
			if (!Parent.NextDecryptedPassword.IsEmpty && Parent.NextDecryptedPassword == Parent.CurrentDecryptedPassword)
			{
				Parent.NextDecryptedPasswordInfo.AddError(Res.GetString("f91aad5d-3771-4670-a843-7bd8ae3bd6a1", "Next password cannot be the same as current password"));
			}
		}
	}
}
