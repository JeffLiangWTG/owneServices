using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class Processor : BaseMessageProcessor
	{
		public Processor()
			: base()
		{
		}

		public Processor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new MessageProcessors.MessageProcessorFactory(Logger));
			return result;
		}
	}
}
