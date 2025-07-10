using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class GlbStaffCredentialValidation : GlbExternalPasswordValidation
	{
		public GlbStaffCredentialValidation(GlbStaffCredential parent)
			: base(parent)
		{
		}

		protected new GlbStaffCredential Parent
		{
			get { return (GlbStaffCredential)base.Parent; }
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

			var wrapper = GlbStaffWrapper.Get(Parent.Staff);

			var duplicateCredential = wrapper
				?.PasswordCollection
				?.Cast<GlbStaffCredential>()
				.FirstOrDefault(c => !c.IsDeleted && c.PK != Parent.PK && c.GP_UserID == Parent.GP_UserID);

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
