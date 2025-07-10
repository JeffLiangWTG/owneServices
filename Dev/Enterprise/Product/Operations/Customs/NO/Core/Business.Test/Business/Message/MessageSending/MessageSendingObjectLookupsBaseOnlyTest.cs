using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ImportMessageSendingObjectLookups))]
sealed class MessageSendingObjectLookupsBaseOnlyTest : MessageSendingObjectLookupsAbstractTest<MessageSendingObjectLookups>
{
	public void TestMessageTypeList()
	{
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.MessageTypeList, lookups.MessageTypeList);
		});
	}

	public void TestMessageTypeList_SecondSend()
	{
		CombineAssertions(() =>
		{
			entryHeader.CH_BGMReference = "1234567890123456789012301";
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.UAR;
			AssertEquals("UAR -> EN", MessageSendingMessageTypes.Codes.FinalDeclaration, lookups.MessageTypeList.CodesAsString);

			entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.MEC;
			AssertEquals("ME -> KO", MessageSendingMessageTypes.Codes.Correction, lookups.MessageTypeList.CodesAsString);

			entryHeader.CH_EntryStatus = MessageSendingStatusCodes.Codes.FinalApproval;
			AssertEquals("TK -> ''", ZString.Empty, lookups.MessageTypeList.CodesAsString);

			entryHeader.CH_EntryStatus = MessageSendingStatusCodes.Codes.RefusalOfDeclaration;
			AssertEquals("IU -> ''", ZString.Empty, lookups.MessageTypeList.CodesAsString);
		});
	}

	public void TestParentType()
	{
		AssertType<MessageSendingObject>(lookups.Parent);
	}

	protected override string MessageType => JobMessageTypeList.Codes.MiscellaneousCustoms;
}
