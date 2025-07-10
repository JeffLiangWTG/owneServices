using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC571CMessageProcessor))]
sealed class CC571CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC571CMessageProcessor, ICC571C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC571;

	protected override bool ExpectedIsFailureNotification => false;
}
