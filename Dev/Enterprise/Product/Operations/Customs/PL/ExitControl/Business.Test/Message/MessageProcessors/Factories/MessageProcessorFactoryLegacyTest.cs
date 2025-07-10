using Enterprise.Customs.PL.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(MessageProcessorFactoryLegacy))]
sealed class MessageProcessorFactoryLegacyTest : MessageProcessorFactoryTest
{
	protected override MessageProcessorFactoryBase CreateMessageProcessorFactory()
		=> new MessageProcessorFactoryLegacy(Logger);
}
