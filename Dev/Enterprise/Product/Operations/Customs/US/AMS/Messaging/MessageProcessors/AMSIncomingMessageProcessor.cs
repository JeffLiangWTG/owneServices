using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSIncomingMessageProcessor : BranchMessageProcessor
	{
		public AMSIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;

		protected override void SortProcessableMessageEvenFurther(EDIMessage[] messages)
		{
		}

		protected override bool ExcludeBranchFilter => true;

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return AMSMessageProcessor;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = new List<ApplicationTypeMessageProcessor>();
			result.Add(AMSMessageProcessor);
			return result;
		}

		AMSMessageProcessorFactory AMSMessageProcessor => amsMessageProcessor ?? (amsMessageProcessor = new AMSMessageProcessorFactory(Logger));
		AMSMessageProcessorFactory amsMessageProcessor;
	}
}
