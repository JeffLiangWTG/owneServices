using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC604CMessageProcessor))]
sealed class CC604CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC604CMessageProcessor, ICC604C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC604;

	protected override bool ExpectedIsFailureNotification => false;
}
