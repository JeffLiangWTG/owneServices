using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class GlbCompanyCredentialValidation : GlbExternalPasswordValidation
	{
		public GlbCompanyCredentialValidation(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		protected new GlbCompanyCredential Parent
		{
			get { return (GlbCompanyCredential)base.Parent; }
		}

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();

			GlbExternalPasswordHelper.CheckGP_MailBoxID(Parent);
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();

			GlbExternalPasswordHelper.CheckGP_UserID(Parent);

			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Parent.Company);

			var duplicateCredential = wrapper
				?.PasswordCollection
				?.Cast<GlbCompanyCredential>()
				.FirstOrDefault(c => !c.IsDeleted && c.PK != Parent.PK && c.GP_GS == Parent.GP_GS && c.GP_UserID == Parent.GP_UserID);

			if (duplicateCredential != null)
			{
				Parent.GP_UserIDInfo.AddError(GlbExternalPasswordHelper.UserNameMustBeUnique);
			}
		}

		protected override void CheckCurrentDecryptedCertificatePassphrase()
		{
			GlbExternalPasswordHelper.CheckCurrentDecryptedCertificatePassphrase(Parent);
		}
	}
}
