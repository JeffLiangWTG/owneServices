using Enterprise.Customs.TW.Business.N5167;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5167MessageSendingObjectValidation))]
	sealed class N5167MessageSendingObjectValidationTest : TWMessageSendingObjectValidationAbstractTest<N5167MessageSendingObject>
	{
		public void TestCheckShouldSendForIEM()
		{
			var entry1 = NewCusEntryHeader("NO1");
			entry1.CH_EntryStatus = EntryStatusCodeList.Codes.IEM;
			var message1 = new N5167MessageSendingObject(entry1);
			message1.MessageType = MessageTypeList.Codes.IEA;
			message1.ShouldSend = true;
			AssertNoMessageError(message1.ShouldSendInfo, ValidationConstants.MessageSendingObject.EntryStatusShouldBeIEM);
			var entry2 = NewCusEntryHeader("NO2");
			entry2.CH_EntryStatus = MessageTypeList.Codes.RFM;
			var message2 = new N5167MessageSendingObject(entry2);
			message2.MessageType = MessageTypeList.Codes.ADM;
			message2.ShouldSend = true;
			AssertHasMessageError(message2.ShouldSendInfo, ValidationConstants.MessageSendingObject.EntryStatusShouldBeIEM);
		}
	}
}
