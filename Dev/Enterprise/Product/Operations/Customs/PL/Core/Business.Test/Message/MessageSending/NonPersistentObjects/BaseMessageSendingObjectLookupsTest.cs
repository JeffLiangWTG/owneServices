using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class BaseMessageSendingObjectLookupsTest : TestCaseWithFactory
{
	public void TestSecurityList()
	{
		var sendingObj = GetNewBaseMessageSendingObject();
		var list = sendingObj.Lookups.SecurityList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "0, 2", list.CodesAsString);
			AssertSame("Cached", list, sendingObj.Lookups.SecurityList);
		});
	}

	public void TestCorrectionAcceptanceList()
	{
		var sendingObj = GetNewBaseMessageSendingObject();
		var list = sendingObj.Lookups.CorrectionAcceptanceList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "0, 1", list.CodesAsString);
			AssertSame("Cached", list, sendingObj.Lookups.CorrectionAcceptanceList);
		});
	}

	public void TestMessageNumList() => CombineAssertions(() =>
	{
		TestMessageNumListForAction("CC566", "560");
		TestMessageNumListForAction("CC583", "582");

		void TestMessageNumListForAction(string action, string incomingMessageType)
		{
			var sendingObj = GetNewBaseMessageSendingObject();
			_ = CreateTestMessage(sendingObj, "515", "TestMessage1", ZDateTime.Now);
			sendingObj.Action = action;
			AssertEquals($"{incomingMessageType}: None {incomingMessageType} message.", 0, sendingObj.Lookups.MessageNumList.Count);

			_ = CreateTestMessage(sendingObj, incomingMessageType, "TestMessage2", ZDateTime.Now);
			sendingObj.Action = "CC515";
			AssertEquals($"{incomingMessageType}: Single {incomingMessageType} message but action is not {action}.", 0, sendingObj.Lookups.MessageNumList.Count);

			_ = CreateTestMessage(sendingObj, incomingMessageType, "TestMessage3", ZDateTime.Now.AddDays(-2));
			sendingObj.Action = action;
			AssertEquals($"{incomingMessageType}: Multiple {incomingMessageType} messages.", 2, sendingObj.Lookups.MessageNumList.Count);
			AssertEquals($"{incomingMessageType}: Messages are sorted.", "TestMessage3", sendingObj.Lookups.MessageNumList[0].Code);
		}

		EDIMessage CreateTestMessage(BaseMessageSendingObject sendingObject, ZString messageSubType, ZString messageNumber, ZDateTime messageDateTime)
		{
			var result = sendingObject.Header.Messages.AddNew();
			result.EM_MessageSubType = messageSubType;
			result.EM_MessageNum = messageNumber;
			result.EM_SystemCreateTimeUtc = messageDateTime;
			return result;
		}
	});

	BaseMessageSendingObject GetNewBaseMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new BaseMessageSendingObject(entryHeader);
	}
}
