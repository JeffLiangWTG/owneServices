using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750MessageInstructionsTest : TestCaseWithFactory
	{
		public void TestCIN750MessageInstructions_In() =>
			TestCIN750MessageInstructionsCore(CIN750NotificationMessageTypes.CIN750InNotification, "CIN750InNotification", "CIN750WarehouseIn", "CIN_WAREHOUSE", "CIN750/In");

		public void TestCIN750MessageInstructions_Cons() =>
			TestCIN750MessageInstructionsCore(CIN750NotificationMessageTypes.CIN750ConsNotification, "CIN750ConsNotification", "CIN750WarehouseCons", "CIN_WAREHOUSE", "CIN750/Cons");

		public void TestCIN750MessageInstructions_Cor() =>
			TestCIN750MessageInstructionsCore(CIN750NotificationMessageTypes.CIN750CorNotification, "CIN750CorNotification", "CIN750WarehouseCor", "CIN_WAREHOUSE", "CIN750/Cor");

		public void TestCIN750MessageInstructions_Decons() =>
			TestCIN750MessageInstructionsCore(CIN750NotificationMessageTypes.CIN750DeconsNotification, "CIN750DeconsNotification", "CIN750WarehouseDecons", "CIN_WAREHOUSE", "CIN750/Decons");

		public void TestCIN750MessageInstructions_Out() =>
			TestCIN750MessageInstructionsCore(CIN750NotificationMessageTypes.CIN750OutNotification, "CIN750OutNotification", "CIN750WarehouseOut", "CIN_WAREHOUSE", "CIN750/Out");

		void TestCIN750MessageInstructionsCore(CIN750NotificationMessageTypes messageType, string expectedDocumentName, string expectedDataContext, string expectedEhubClientID, string expectedXmlNamespace)
		{
			var instructions = new CIN750MessageInstructions(new CIN750NotificationDocData(messageType));
			AssertEquals(expectedDocumentName, instructions.DocumentName);
			AssertEquals(expectedDataContext, instructions.DataContext);
			AssertEquals("Terminal", instructions.Recipient);
			AssertEquals(expectedEhubClientID, instructions.EHubClientID);
			AssertEquals(expectedXmlNamespace, instructions.XmlNamespace);
			AssertEquals(true, instructions.AllowSendMessage);
			AssertEquals(false, instructions.AllowSendMessageAmendment);
			AssertEquals(true, instructions.AllowSendMessageWithdrawal);
			AssertEquals(true, instructions.AllowResetToOriginal);
			AssertEquals(true, instructions.OrderLogsByLocalTime);
			AssertEquals(false, instructions.RequireMessageAmendmentReason);
			AssertEquals(null, instructions.AmendmentOptions);
			AssertEquals(null, instructions.WidthdrawalOptions);
		}
	}
}
