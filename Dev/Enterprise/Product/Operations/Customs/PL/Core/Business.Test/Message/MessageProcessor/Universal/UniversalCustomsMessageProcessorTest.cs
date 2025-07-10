using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(UniversalCustomsMessageProcessor))]
sealed class UniversalCustomsMessageProcessorTest
	: UniversalCustomsMessageProcessorBaseTest<UniversalCustomsMessageProcessor, MessageProcessorFactory, EDIMessage>
{
	protected override string ApplicationCode => ApplicationCodeList.Codes.PLCustoms;
}
