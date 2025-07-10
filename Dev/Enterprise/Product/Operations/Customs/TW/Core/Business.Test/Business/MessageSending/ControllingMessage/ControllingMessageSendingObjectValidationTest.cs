using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ControllingMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckShouldSend()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			header.TW1_ControllingMessageType = "";
			header.TW1_RequestDescription = "B";
			var action = new ControllingMessageSendingObject(header);
			action.ShouldSend = true;
			var targetInfo = action.ShouldSendInfo;
			AssertHasErrorContaining(targetInfo, "A valid message type is missing. The message cannot be sent.");
			header.TW1_ControllingMessageType = "A";
			header.TW1_RequestDescription = "B";
			action = new ControllingMessageSendingObject(header);
			targetInfo = action.ShouldSendInfo;
			action.ShouldSend = true;
			AssertNoErrorContaining(targetInfo, "A valid message type is missing. The message cannot be sent.");
		}
	}
}
