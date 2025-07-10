using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class InboundInterchangeProcessorForTesting : InboundInterchangeProcessor
	{
		public InboundInterchangeProcessorForTesting(LoggingInformation logger)
			: base(logger)
		{
		}

		public const string AppCode = "~#@";

		protected override string[] ApplicationCodes => new string[] { AppCode };

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreatorForTesting();
	}
}
