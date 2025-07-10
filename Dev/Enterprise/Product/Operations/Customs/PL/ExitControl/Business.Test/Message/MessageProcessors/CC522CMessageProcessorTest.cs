using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CC522CMessageProcessor))]
sealed class CC522CMessageProcessorTest : ExitControlMessageProcessorBaseTestCase<CC522CMessageProcessor, ICC522C>
{
	protected override string ExpectedMessageFriendlyName => ExitControlMessageCodes.Descriptions.CC522;

	protected override bool ExpectedIsFailureNotification => false;
}
