using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class StowPlanIncomingMessageProcessor : BranchMessageProcessor
	{
		public StowPlanIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ExcludeBranchFilter => true;
		protected override bool MessageShouldBeProcessedInASeparateFactory => true;
		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return StowPlanMessageProcessor;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = new List<ApplicationTypeMessageProcessor>();
			result.Add(StowPlanMessageProcessor);
			return result;
		}

		StowPlanMessageProcessorFactory StowPlanMessageProcessor => stowPlanMessageProcessor ?? (stowPlanMessageProcessor = new StowPlanMessageProcessorFactory(Logger));
		StowPlanMessageProcessorFactory stowPlanMessageProcessor;
	}
}
