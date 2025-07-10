using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	public class UCPSubscribersProvider : Messaging.Business.MessageProcessor.Testing.IUCPSubscribersProvider
	{
		public IUniversalCustomsEDIMessagePacker GetMessagePacker(string applicationCode) => UCPSubscribers.GetMessagePacker(applicationCode);
	}
}
