using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class GlbExternalPasswordValidation : GlbExternalPasswordValidationBase_TW
	{
		public GlbExternalPasswordValidation(GlbExternalPassword parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword Parent => (GlbExternalPassword)base.Parent;

		protected override void CheckGP_Certificate()
		{
			base.CheckGP_Certificate();
			if (Parent.CertificateStatus == GlbExternalPasswordWithCertificate.CertificateLoaded && ParentCollection != null)
			{
				if (ParentCollection.Cast<GlbExternalPassword>().Any(sub => sub.PK != Parent.PK
					&& sub.CertificateStatus == GlbExternalPasswordWithCertificate.CertificateLoaded
					&& sub.GP_Certificate != Parent.GP_Certificate))
				{
					Parent.GP_CertificateInfo.AddError(Res.GetString("9FEB7EA5-59F5-4036-885E-6DEEE4518331", "A staff can only have one certificate file."));
				}
			}
		}

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();
			var mailBoxID = Parent.GP_MailBoxID;
			var mailBoxIDInfo = Parent.GP_MailBoxIDInfo;
			if (mailBoxID.IsEmpty)
			{
				mailBoxIDInfo.AddError(MandatoryValidation.MustBeEnteredMessage(mailBoxIDInfo.HumanReadableName));
			}
			else
			{
				var passwordType = Parent.GP_PasswordType;
				if (passwordType == PasswordTypesList.Codes.TVA && !new Regex("^[A-Z0-9]{7,}-[A-Z0-9]$").IsMatch(mailBoxID))
				{
					mailBoxIDInfo.AddError(Res.GetString("82F29C74-B496-4553-8FA5-9082B8817888", "The entered Mail Box is invalid, the Mail Box format must be Mail Box - Mail Sub Box and use '-' to separate. For example: CBK0001-0."));
				}
				else if (passwordType == PasswordTypesList.Codes.UVC && !new Regex("^[A-Z0-9]{10,}-[A-Z0-9]$").IsMatch(mailBoxID))
				{
					mailBoxIDInfo.AddError(Res.GetString("391D0DA9-7D92-48D3-BA9F-3960BD2FFF1A", "The entered Mail Box is invalid, the Mail Box format must be Mail Box - Mail Sub Box and use '-' to separate. For example: PABKN00001-G."));
				}
				if (ParentCollection?.Cast<GlbExternalPassword>().Any(sub => sub.PK != Parent.PK && sub.GP_MailBoxID.EqualsIgnoringCase(mailBoxID)) ?? false)
				{
					mailBoxIDInfo.AddError(Res.GetString("7C207D54-980C-49B1-956A-58D7E5D16935", "Each subscription must have a unique Mailbox."));
				}
			}
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();
			MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
			var userID = Parent.GP_UserID;
			if (userID.IsEmpty)
			{
				Parent.GP_UserIDInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.GP_UserIDInfo.HumanReadableName));
			}
			else
			{
				if (ParentCollection?.Cast<GlbExternalPassword>().Any(sub => sub.PK != Parent.PK && sub.GP_UserID.EqualsIgnoringCase(userID)) ?? false)
				{
					Parent.GP_UserIDInfo.AddError(Res.GetString("F2CFFF99-9298-4714-BDDB-BA45E35DF892", "The same Sender ID already exists. Please enter a different Sender ID."));
				}
			}
		}

		GlbExternalPasswordCollection ParentCollection => TWGlbStaffWrapper.Get(Parent.Staff)?.TWPasswordCollection;
	}
}
