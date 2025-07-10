using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ZA.Business
{
	public class ExternalWarehouseIncomingMessageProcessor : BranchMessageProcessor
	{
		public ExternalWarehouseIncomingMessageProcessor()
			: base()
		{
		}

		public ExternalWarehouseIncomingMessageProcessor(LoggingInformation logger)
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
			var result = base.GetMessageProcessors();
			result.Add(new ExternalWarehouseMessageProcessor(Logger));
			return result;
		}
	}
}
