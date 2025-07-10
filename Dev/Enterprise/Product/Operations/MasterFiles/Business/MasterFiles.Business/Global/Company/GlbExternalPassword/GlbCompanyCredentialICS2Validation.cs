using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyCredentialICS2Validation : GlbExternalPasswordWithCertificateValidation
	{
		public GlbCompanyCredentialICS2Validation(GlbCompanyCredentialICS2 parent) : base(parent)
		{
		}
		protected new GlbCompanyCredentialICS2 Parent => (GlbCompanyCredentialICS2)base.Parent;

		protected override void CheckCurrentDecryptedCertificatePassphrase()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CurrentDecryptedCertificatePassphraseInfo);
		}

		protected override void CheckGP_MailBoxID()
		{
			if (Parent.GP_PasswordStatus.EqualsIgnoringCase(PasswordStatusList.Codes.Valid))
			{
				var eori = Parent.GP_MailBoxID;
				if (eori.IsEmpty)
				{
					var info = Parent.GP_MailBoxIDInfo;
					info.AddError(MandatoryValidation.MustBeEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info)));
				}
				ValidateReporterEoriCodeForCheckBox(eori, Parent.GP_MailBoxIDInfo);
			}
		}

		void ValidateReporterEoriCodeForCheckBox(ZString eoriCode, ZPropertyInfo propertyOnWhichToWarn)
		{
			if (eoriCode.Length < 3 || eoriCode.Length > 17
				|| !ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsMemberOfEU(eoriCode.Left(2))
				|| !Regex.IsMatch(eoriCode.Substring(2), @"^[0-9]{1,15}$"))
			{
				propertyOnWhichToWarn.AddMessageError(Res.GetString("c113fa61-9a73-4977-85f2-68b85b65357e", "Maximum Length of the EORI is 17 including the Country/Region code prefix."));
			}
		}
	}
}
