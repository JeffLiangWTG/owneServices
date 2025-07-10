using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC548CMessageProcessor))]
sealed class CC548CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC548CMessageProcessor, ICC548C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC548;

	protected override bool ExpectedIsFailureNotification => false;
}
