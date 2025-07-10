using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class TRMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public TRMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Turkey ? (ZString)ValidationConstants.MustBeLoggedInUnderTRToSendTRMessages : base.GetExtraMessageSendingNotificationCore();
		}
	}
}
