using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

public interface IMessageProcessorFactoryLegacyResolver
{
	Integration.Customs.PL.IMessageProcessorFactoryLegacy ResolveFactory(LoggingInformation logger, BaseEDIMessage message);
}
