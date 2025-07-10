using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	public class TWCIncomingMessageProcessor : BaseMessageProcessor
	{
		public TWCIncomingMessageProcessor()
		: base()
		{
		}

		public TWCIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool MessageShouldBeProcessedInASeparateFactory
		{
			get { return true; }
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new TWMessageProcessor(Logger));
			return result;
		}
	}
}
