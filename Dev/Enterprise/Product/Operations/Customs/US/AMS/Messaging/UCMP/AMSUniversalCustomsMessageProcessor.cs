using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;

[assembly: UniversalCustomsMessageProcessor(BaseEDIMessage.ApplicationCodes.AMS, typeof(Enterprise.Customs.US.AMS.Messaging.Business.AMSUniversalCustomsMessageProcessor))]
namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public sealed class AMSUniversalCustomsMessageProcessor : CommonUniversalCustomsMessageProcessor
	{
		protected override ApplicationTypeMessageProcessor QueryMessageProcessorFactory(LoggingInformation logger) => new AMSMessageProcessorFactory(logger);
	}
}
