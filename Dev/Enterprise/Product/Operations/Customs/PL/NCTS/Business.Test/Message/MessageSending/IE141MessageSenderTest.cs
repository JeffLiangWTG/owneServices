using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(IE141MessageSender))]
sealed class IE141MessageSenderTest : MessageSenderAbstractTest<IE141MessageSender>
{
	public void TestSend()
	{
		(var nctsHeader, var sender) = CreateMessageSender();
		sender.Send();
		var message = (EDIMessage)nctsHeader.MovementHeader.Messages.Single();
		CombineAssertions(() =>
		{
			var expectedMessageType = EUJobMessageTypeList.Codes.NctsDeparture;
			AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
			var expectedMessageSubType = Constants.MessageSubTypeCodes.IE141;
			AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
			var expectedFragmentOfContent = "IE141PL";
			Assert("EM_MessageText", message.EM_MessageText.Contains(expectedFragmentOfContent));
			var expectedPhaseCode = NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement;
			AssertEquals($"BM_Phase should be updated with MessageType:{expectedMessageType}", expectedPhaseCode, nctsHeader.MovementHeader.BM_Phase);
			var expectedMessageInterpretation = "<style>body, p, td { font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px; } table .tdGoodsItemsTitle { border-bottom: solid 1px; width: 25%; }</style><H3>NCTS Message (Phase 5)</H3><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td width=\"25%\">Name</td><td width=\"75%\">Value</td></tr><tr><td>Message Type</td><td>IE141</td></tr></table>";
			AssertEquals("EM_MessageInterpretation", expectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}

	protected override ZString HeaderType => NctsMovementType.Codes.Departure;
}
