using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	public class ZACIncomingMessageProcessor : BranchMessageProcessor
	{
		public ZACIncomingMessageProcessor()
			: base()
		{
		}

		public ZACIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ExcludeBranchFilter => true;

		protected override bool MessageShouldBeProcessedInASeparateFactory
		{
			get { return true; }
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new CUSRESMessageProcessor(Logger));
			result.Add(new CONTRLMessageProcessor(Logger));
			result.Add(new CUSRES_REQDOCMessageProcessor(Logger));
			result.Add(new STATACMessageProcessor(Logger));
			result.Add(new CUSCARMessageProcessor(Logger));
			result.Add(new GENRALMessageProcessor(Logger));
			return result;
		}
	}
}
