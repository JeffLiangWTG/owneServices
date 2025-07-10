using System.Collections;
using CargoWise.Application;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business;

public sealed class MessageProcessorFactoryLegacyResolver : IMessageProcessorFactoryLegacyResolver
{
	public Integration.Customs.PL.IMessageProcessorFactoryLegacy ResolveFactory(LoggingInformation logger, BaseEDIMessage message)
	{
		var hashTable = (Hashtable)ObjectFactory.Get(name: "PLMessageProcessorFactoryLegacy");
		var objectHandle = hashTable[(string)message.EM_ApplicationCode] as ObjectHandle;
		return objectHandle?.GetObject(logger) as Integration.Customs.PL.IMessageProcessorFactoryLegacy;
	}
}
