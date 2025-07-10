using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC609CMessageProcessor))]
sealed class CC609CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC609CMessageProcessor, ICC609C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC609;

	protected override bool ExpectedIsFailureNotification => false;
}
