using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(MessageProcessorFactoryLegacy))]
sealed class MessageProcessorFactoryLegacyTest : MessageProcessorFactoryTest
{
	protected override MessageProcessorFactoryBase CreateMessageProcessorFactory()
		=> new MessageProcessorFactoryLegacy(Logger);
}
