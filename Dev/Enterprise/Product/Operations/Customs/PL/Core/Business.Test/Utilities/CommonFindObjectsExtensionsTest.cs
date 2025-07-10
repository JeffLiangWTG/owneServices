using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using static Enterprise.Messaging.Integration.ReceiveTransmitList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CommonFindObjectsExtensionsTest : TestCaseWithFactory
{
	public void TestFindMessageByMessageNum()
	{
		const string testMessageNum = "TEST_NUM";

		var (_, message) = Factory.CreateInterchangeAndMessageForTest<EDIInterchange, EDIMessage>(
			applicationCode: PLCustoms,
			receiveTransmit: Transmit,
			interchangeStatus: EDIInterchange.Status.Sent,
			messageStatus: EDIMessage.Status.Sent);
		message.EM_MessageNum = testMessageNum;

		CombineAssertions(() =>
		{
			AssertEquals("Correct params", message, Factory.FindMessageByMessageNum(PLCustoms, testMessageNum, EDIInterchange.Direction.Transmit));
			AssertNull("Wrong application code", Factory.FindMessageByMessageNum(PLCustomsNCTS, testMessageNum, EDIInterchange.Direction.Transmit));
			message.EM_ApplicationCode = PLCustomsNCTS;
			AssertEquals("Other Application code found", message.PK, Factory.FindMessageByMessageNum(PLCustomsNCTS, testMessageNum, EDIInterchange.Direction.Transmit).PK);
			message.EM_ApplicationCode = PLCustoms;

			AssertNull("Wrong message num", Factory.FindMessageByMessageNum(PLCustoms, "WRONG_NUM", EDIInterchange.Direction.Transmit));

			message.EM_ReceiveTransmit = Receive;
			AssertNull("Receive message not found", Factory.FindMessageByMessageNum(PLCustoms, testMessageNum, EDIInterchange.Direction.Transmit));
			AssertEquals("Receive message found", message, Factory.FindMessageByMessageNum(PLCustoms, testMessageNum, EDIInterchange.Direction.Receive));
			message.EM_ReceiveTransmit = Transmit;

			message.EM_ApplicationReference = "111";
			AssertEquals("Filter, 111 message found", message, Factory.FindMessageByMessageNum(PLCustoms, testMessageNum, EDIInterchange.Direction.Transmit, new ZQuery(EDIMessageSchema.EM_ApplicationReference, "111")));
			AssertNull("Filter, 222 message not found", Factory.FindMessageByMessageNum(PLCustoms, testMessageNum, EDIInterchange.Direction.Transmit, new ZQuery(EDIMessageSchema.EM_ApplicationReference, "222")));
			AssertEquals("Filter, not 222 message found", message, Factory.FindMessageByMessageNum(PLCustoms, testMessageNum, EDIInterchange.Direction.Transmit, new ZQuery(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.NotEqual, "222")));
		});
	}

	public void TestFindTransmittedMessageByApplicationReference()
	{
		const string testApplicationReference = "TEST_REF";

		var (_, message) = Factory.CreateInterchangeAndMessageForTest<EDIInterchange, EDIMessage>(
			applicationCode: PLCustoms,
			receiveTransmit: Transmit,
			interchangeStatus: EDIInterchange.Status.Sent,
			messageStatus: EDIMessage.Status.Sent);
		message.EM_ApplicationReference = testApplicationReference;

		CombineAssertions(() =>
		{
			AssertEquals("Correct params", message, Factory.FindSentTransmittedMessageByExternalSystemID(PLCustoms, testApplicationReference));
			AssertNull("Wrong application code", Factory.FindSentTransmittedMessageByExternalSystemID(PLCustomsNCTS, testApplicationReference));
			message.EM_ApplicationCode = PLCustomsNCTS;
			AssertEquals("Other application code found", message.PK, Factory.FindSentTransmittedMessageByExternalSystemID(PLCustomsNCTS, testApplicationReference).PK);
			message.EM_ApplicationCode = PLCustoms;

			AssertNull("Wrong message reference", Factory.FindSentTransmittedMessageByExternalSystemID(PLCustoms, "WRONG_REF"));

			message.EM_ReceiveTransmit = Receive;
			AssertNull("Receive message not found", Factory.FindSentTransmittedMessageByExternalSystemID(PLCustoms, testApplicationReference));
		});
	}

	public void TestFindTransmittedInterchangeBySessionGuid()
	{
		var (interchange, message) = Factory.CreateInterchangeAndMessageForTest<EDIInterchange, EDIMessage>(
			applicationCode: PLCustoms,
			receiveTransmit: EDIInterchange.Direction.Transmit,
			interchangeStatus: EDIInterchange.Status.Sent,
			messageStatus: EDIMessage.Status.Sent);

		CombineAssertions(() =>
		{
			AssertNull("Empty session Guid", Factory.FindTransmittedMessageBySessionGuid(PLCustoms, sessionGuid: ZGuid.Empty));
			AssertEquals("Try locate correct", message, Factory.FindTransmittedMessageBySessionGuid(PLCustoms, interchange.EI_SessionGUID));
			AssertNull("Try locate incorrect Application Code", Factory.FindTransmittedMessageBySessionGuid(PLCustomsNCTS, interchange.EI_SessionGUID));
			interchange.EI_ApplicationCode = PLCustomsNCTS;
			message.EM_ApplicationCode = PLCustomsNCTS;
			AssertEquals("Try NCTS", message.PK, Factory.FindTransmittedMessageBySessionGuid(PLCustomsNCTS, interchange.EI_SessionGUID).PK);
			message.EM_ApplicationCode = PLCustoms;
			interchange.EI_ApplicationCode = PLCustoms;
			AssertNull("Try locate incorrect Session Guid", Factory.FindTransmittedMessageBySessionGuid(PLCustoms, ZGuid.NewZGuid()));
		});
	}
}
