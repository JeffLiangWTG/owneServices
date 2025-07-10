using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;

[assembly: UniversalCustomsMessageProcessor(BaseEDIMessage.ApplicationCodes.StowPlan, typeof(Enterprise.Customs.US.AMS.Messaging.Business.StowPlanUniversalCustomsMessageProcessor))]
namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class StowPlanUniversalCustomsMessageProcessor : CommonUniversalCustomsMessageProcessor
	{
		protected override ApplicationTypeMessageProcessor QueryMessageProcessorFactory(LoggingInformation logger) => new StowPlanMessageProcessorFactory(logger);
	}
}
