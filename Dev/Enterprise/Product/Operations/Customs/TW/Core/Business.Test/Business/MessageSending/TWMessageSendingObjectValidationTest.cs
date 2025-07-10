
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWMessageSendingObjectValidationTest : TWMessageSendingObjectValidationAbstractTest<MessageSendingObjectForTest>
	{
		public void TestCheckShouldSend()
		{
			var typeRequiredError = "Message should not be selected to send with missing Message Type.Please check the Declaration Shipment Type.";
			var cusHead = NewCusEntryHeader("");
			var action = new MessageSendingObjectForTest(cusHead);
			action.MessageType = ZString.Empty;
			action.ShouldSend = true;
			var targetInfo = action.ShouldSendInfo;
			AssertHasErrorContaining(targetInfo, typeRequiredError);
			cusHead = NewCusEntryHeader("");
			action = new MessageSendingObjectForTest(cusHead);
			action.MessageType = "ICD";
			action.ShouldSend = true;
			targetInfo = action.ShouldSendInfo;
			AssertNoErrorContaining(targetInfo, typeRequiredError);
			cusHead = NewCusEntryHeader("NO1");
			action = new MessageSendingObjectForTest(cusHead);
			action.MessageType = "ICD";
			action.ShouldSend = true;
			targetInfo = action.ShouldSendInfo;
			AssertNoErrorContaining(targetInfo, typeRequiredError);
		}
	}
}
