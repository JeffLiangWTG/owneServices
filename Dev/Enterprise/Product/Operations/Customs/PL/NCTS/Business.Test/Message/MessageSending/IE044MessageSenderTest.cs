using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(IE044MessageSender))]
sealed class IE044MessageSenderTest : MessageSenderAbstractTest<IE044MessageSender>
{
	public void TestSend()
	{
		(var nctsHeader, var sender) = CreateMessageSender();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		sender.Send();
		var message = (EDIMessage)nctsHeader.Messages.First();
		CombineAssertions(() =>
		{
			var expectedMessageType = EUJobMessageTypeList.Codes.NctsArrivalNotification;
			AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
			var expectedMessageSubType = Constants.MessageSubTypeCodes.IE044;
			AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
			var expectedFragmentOfContent = "IE044PL";
			Assert("EM_MessageText", message.EM_MessageText.Contains(expectedFragmentOfContent));
			var expectedPhaseCode = NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
			AssertEquals($"BM_Phase should be updated with MessageType:{expectedMessageType}", expectedPhaseCode, nctsHeader.ArrivalMovementHeader.BM_Phase);
		});
	}

	protected override ZString HeaderType => NctsMovementType.Codes.Arrival;
}
