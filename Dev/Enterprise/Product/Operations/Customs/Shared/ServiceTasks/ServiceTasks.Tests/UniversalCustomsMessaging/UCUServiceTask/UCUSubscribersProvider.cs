using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	public class UCUSubscribersProvider : Messaging.Business.MessageProcessor.Testing.IUCUSubscribersProvider
	{
		public IUniversalCustomsInterchangeUnpacker GetInterchangeUnpacker(string applicationCode) => UCUSubscribers.GetInterchangeUnpacker(applicationCode);
	}
}
