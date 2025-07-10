using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using EnterpriseMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.PL.Business;

public class IncomingMessagesProcessingLegacyRouter(IMessageProcessorFactoryLegacyResolver messageProcessorFactoryResolver)
	: BranchCustomsMessageProcessor([ApplicationCodeList.Codes.PLCustoms, ApplicationCodeList.Codes.PLCustomsNCTS, ApplicationCodeList.Codes.PLCustomsExitControl], messageTypes: null)
{
	public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EnterpriseMessage enterpriseMessage)
	{
		if (enterpriseMessage is not BaseEDIMessage message)
		{
			return null;
		}

		var messageProcessorFactory = messageProcessorFactoryResolver.ResolveFactory(Logger, message);
		return messageProcessorFactory?.CreateProcessor(message) as ApplicationTypeMessageProcessor;
	}
}
