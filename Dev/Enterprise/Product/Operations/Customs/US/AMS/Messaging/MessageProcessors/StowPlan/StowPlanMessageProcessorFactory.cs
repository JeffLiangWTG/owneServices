using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	class StowPlanMessageProcessorFactory : ApplicationTypeMessageProcessor
	{
		public StowPlanMessageProcessorFactory(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIInterchange.ApplicationCodes.StowPlan; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return "Stow Plan Message"; }
		}

		protected override bool RequiresPreProcessingCore => true;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			base.PreProcessMessageCore(message);
			var stowPlanMessage = message as StowPlanMessage;
			if (stowPlanMessage != null && stowPlanMessage.OriginalMessage is StowPlanMessage originalMessage)
			{
				if (message.EM_GB != originalMessage.EM_GB)
				{
					message.EM_GB = originalMessage.EM_GB;
				}
				if (originalMessage.EM_LinkedObject is BusinessObject originalMessageBizObj && message.EM_LinkedObject != originalMessageBizObj)
				{
					message.EM_LinkedObject = originalMessageBizObj;
				}
			}
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var processor = new CusresMessageProcessor(Logger);
			processor.ProcessMessage(message);
		}
	}
}
