namespace Enterprise.Customs.UY.Manifest.Business
{
	public partial class AsycudaManifestHeader
	{
		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new UYMessageSendingNotificationHelper(this);
		}
	}
}
