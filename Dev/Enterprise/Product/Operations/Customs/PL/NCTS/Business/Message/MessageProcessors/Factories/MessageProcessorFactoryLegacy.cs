using CargoWise.Common;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class MessageProcessorFactoryLegacy(LoggingInformation logger) : MessageProcessorFactory, Integration.Customs.PL.IMessageProcessorFactoryLegacy
{
	public LoggingInformation Logger { get; } = Argument.NotNull(logger, nameof(logger));

	public object CreateProcessor(Integration.Customs.PL.IEDIMessage message)
		=> base.CreateProcessor(message as EnterpriseEDIMessage, Logger);
}
