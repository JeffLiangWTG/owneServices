using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USMessagingProvider : MessagingProvider
	{
		public override MessageStatusProvider MessageStatusProvider => new USMessageStatusProvider();
	}
}
