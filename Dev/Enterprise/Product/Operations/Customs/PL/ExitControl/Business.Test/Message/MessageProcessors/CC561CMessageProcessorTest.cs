using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CC561CMessageProcessor))]
sealed class CC561CMessageProcessorTest : ExitControlMessageProcessorBaseTestCase<CC561CMessageProcessor, ICC561C>
{
	protected override string ExpectedMessageFriendlyName => ExitControlMessageCodes.Descriptions.CC561;

	protected override bool ExpectedIsFailureNotification => false;
}
