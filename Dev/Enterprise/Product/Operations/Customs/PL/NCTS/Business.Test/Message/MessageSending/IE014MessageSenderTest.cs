using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(IE014MessageSender))]
sealed class IE014MessageSenderTest : MessageSenderAbstractTest<IE014MessageSender>
{
	public void TestSend()
	{
		(var nctsHeader, var sender) = CreateMessageSender();
		sender.Send();
		var message = (EDIMessage)nctsHeader.MovementHeader.Messages.First();
		CombineAssertions(() =>
		{
			var expectedMessageType = EUJobMessageTypeList.Codes.NctsDeparture;
			AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
			var expectedMessageSubType = Constants.MessageSubTypeCodes.IE014;
			AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
			var expectedFragmentOfContent = "IE014PL";
			Assert("EM_MessageText", message.EM_MessageText.Contains(expectedFragmentOfContent));
			var expectedPhase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			AssertEquals($"BM_Phase should be updated with MessageType:{expectedMessageType}", expectedPhase, nctsHeader.MovementHeader.BM_Phase);
		});
	}

	protected override ZString HeaderType => NctsMovementType.Codes.Departure;
}
