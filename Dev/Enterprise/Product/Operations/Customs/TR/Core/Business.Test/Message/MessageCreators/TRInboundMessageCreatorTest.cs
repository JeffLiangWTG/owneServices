using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRInboundMessageCreatorTest : TestCaseWithFactory
	{
		public void TestEmptyCustomsData()
		{
			var interchange = MessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRO);
			interchange.EI_BodyText = ZString.Empty;
			var creator = new TRInboundMessageCreator();
			creator.CreateMessagesForInterchange(interchange);
			CombineAssertions(() =>
			{
				AssertEquals("Should not have a message", 0, interchange.ContainedMessages.Count);
				AssertEquals("interchange status", EDIInterchange.Status.Error, interchange.EI_Status);
				AssertErrorLog(interchange, "NO TR CUSTOMS DATA");
			});
		}

		void AssertErrorLog(EDIInterchange interchange, ZString expectedText) => AssertEquals("Error Log", expectedText, interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);

		public void TestCreateMessagesForInterchange()
		{
			var interchange = MessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRO);
			var creator = new TRInboundMessageCreator();
			creator.CreateMessagesForInterchange(interchange);
			var message = interchange.ContainedMessages[0];

			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
				AssertEquals(message.EM_MessageType, TRMessageTypes.Codes.TRO);
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.TRCustoms);
				AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
				AssertEquals(message.EM_MessageText, TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRO));
				AssertEquals(message.EM_LinkUniqueID, ZGuid.Empty);
				AssertEquals(message.EM_EI, interchange.PK);
			});
		}

		public void TestCreateMessagesForInterchangexTFailure()
		{
			var interchange = MessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.XER);
			var creator = new TRInboundMessageCreator();
			creator.CreateMessagesForInterchange(interchange);
			var message = interchange.ContainedMessages[0];
			const string failHeaderText = "Test Fail Header";

			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
				AssertEquals(message.EM_MessageType, TRMessageTypes.Codes.XER);
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.TRCustoms);
				AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
				AssertEquals(message.EM_MessageText, failHeaderText);
				AssertEquals(message.EM_LinkUniqueID, ZGuid.Empty);
				AssertEquals(message.EM_EI, interchange.PK);
			});
		}
	}
}
