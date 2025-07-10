using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public UYMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Uruguay)
			{
				return (ZString)ValidationsConstants.MustBeLoggedInUnderUYToSendUYMessages;
			}
			else
			{
				var companyCredential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.UTB);
				if (companyCredential != null && companyCredential?.GP_PasswordStatus.ToString() == PasswordStatusList.Codes.Invalid)
				{
					return (ZString)ValidationsConstants.CompanyCredentialMustBeUpdatedToSendUYMessages;
				}
			}
			return base.GetExtraMessageSendingNotificationCore();
		}
	}
}
