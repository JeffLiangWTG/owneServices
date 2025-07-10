using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(IE013MessageSender))]
sealed class IE013MessageSenderTest : IE013IE015MessageSenderTest<IE013MessageSender>
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
			var expectedMessageSubType = Constants.MessageSubTypeCodes.IE013;
			AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
			var expectedFragmentOfContent = "IE013PL";
			Assert("EM_MessageText", message.EM_MessageText.Contains(expectedFragmentOfContent));
			var expectedPhaseCode = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			AssertEquals($"BM_Phase should be updated with MessageType:{expectedMessageType}", expectedPhaseCode, nctsHeader.MovementHeader.BM_Phase);
		});
	}

	[TestDate]
	public void TestSettingValuationDate()
	{
		(var nctsHeader, var sender) = CreateMessageSender();
		sender.Send();

		AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);

		nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;
		sender.Send();

		AssertEquals(ZDateTime.BrettsBirthday, nctsHeader.MovementHeader.BM_ValuationDate);
	}

	protected override ZString HeaderType => NctsMovementType.Codes.Departure;
}
