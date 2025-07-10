using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	sealed class EDIInterchangeUnPackerUtilsTest : TestCaseWithFactory
	{
		public void TestPopulateEDIMessage()
		{
			var appCode = "APP";
			var messageType = "TYP";
			var messageSubType = "ZZZ";
			var messageNum = "001";
			var messageText = "text";

			var ediMessage = Factory.New<EDIMessage>();

			EDIInterchangeUnPackerUtils.PopulateEDIMessage(ediMessage, appCode, messageType, messageSubType, messageNum, messageText);

			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", appCode, ediMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", messageType, ediMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", messageSubType, ediMessage.EM_MessageSubType);
				AssertEquals("EM_MessageNum", messageNum, ediMessage.EM_MessageNum);
				AssertEquals("EM_MessageText", messageText, ediMessage.EM_MessageText);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals("EM_IsActive", true, ediMessage.EM_IsActive);
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, ediMessage.EM_ReceiveTransmit);
			});
		}
	}
}
