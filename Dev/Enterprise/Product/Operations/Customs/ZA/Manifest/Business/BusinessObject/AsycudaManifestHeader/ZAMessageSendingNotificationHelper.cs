using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ZAMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public ZAMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			return AsycudaManifestHeaderExtensions.ZaOrgProxyForManifestMessaging(header.Factory) == null ? (ZString)ValidationConstants.MustBeLoggedInUnderZaToSendZaMessages : base.GetExtraMessageSendingNotificationCore();
		}
	}
}
