using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.PL.Business;

public abstract class MessageProcessorFactoryBase
{
	public ApplicationTypeMessageProcessor CreateProcessor(EnterpriseEDIMessage message, LoggingInformation logger)
		=> message != null ? CreateProcessorCore(message, logger) : null;

	protected abstract ApplicationTypeMessageProcessor CreateProcessorCore(EnterpriseEDIMessage message, LoggingInformation logger);
}
