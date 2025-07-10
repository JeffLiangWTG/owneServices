using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	public class UniversalCustomsMessagingSubscribersProvider : Messaging.Business.MessageProcessor.Testing.IUniversalCustomsMessagingSubscribersProvider
	{
		public IUniversalCustomsMessageProcessor GetMessageProcessor(string applicationCode) => UniversalCustomsMessagingSubscribers.GetMessageProcessor(applicationCode);
	}
}
