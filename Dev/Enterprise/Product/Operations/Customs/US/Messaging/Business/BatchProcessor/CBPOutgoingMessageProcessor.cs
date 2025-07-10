using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class CBPOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		protected CBPOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new CBPInterchangeProvider(readyMessages, !IsBranchFilter);
		}
	}
}
