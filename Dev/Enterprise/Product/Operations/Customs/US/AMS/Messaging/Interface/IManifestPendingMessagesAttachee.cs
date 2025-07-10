using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IManifestPendingMessagesAttachee
	{
		AMSEDIMessageCollection PendingMessages { get; }
	}
}
