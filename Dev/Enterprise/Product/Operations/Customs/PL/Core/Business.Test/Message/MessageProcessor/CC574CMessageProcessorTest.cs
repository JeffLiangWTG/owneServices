using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC574CMessageProcessor))]
sealed class CC574CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC574CMessageProcessor, ICC574C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC574;

	protected override bool ExpectedIsFailureNotification => false;
}
