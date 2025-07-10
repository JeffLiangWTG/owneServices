using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	class AMSMessageProcessorFactory : MessageProcessorFactory
	{
		public AMSMessageProcessorFactory(LoggingInformation logger)
			: base(logger, AMSEDIMessage.ApplicationCodes.AMS, "US Customs AMS Message Processor")
		{
		}

		protected override bool RequiresPreProcessingCore => true;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			base.PreProcessMessageCore(message);
			var messageNum = message.EM_MessageNum;
			if (!messageNum.IsEmpty && new CBPEDIMessage.Loader(message.Factory).LoadTop1WithDirectionOrderByCreatTime(message.EM_ApplicationCode, message.EM_MessageNum, CBPEDIMessage.Direction.Transmit, null) is CBPEDIMessage originalMessage)
			{
				if (message.EM_GB != originalMessage.EM_GB)
				{
					message.EM_GB = originalMessage.EM_GB;
				}
				if (originalMessage.EM_LinkedObject is BusinessObject originalMessageBizObj && message.EM_LinkedObject != originalMessageBizObj)
				{
					message.EM_LinkedObject = originalMessageBizObj;
				}
				if (message.EM_ApplicationReference != originalMessage.EM_ApplicationReference)
				{
					message.EM_ApplicationReference = originalMessage.EM_ApplicationReference;
				}
			}
		}
	}
}
