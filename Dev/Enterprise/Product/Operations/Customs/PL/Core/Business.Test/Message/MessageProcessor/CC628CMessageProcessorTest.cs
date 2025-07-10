using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC628CMessageProcessor))]
sealed class CC628CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC628CMessageProcessor, ICC628C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC628;

	protected override bool ExpectedIsFailureNotification => false;
}
