using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ElectronicBOLMessageInstructionsTest : TestCaseWithFactory
	{
		public void TestImplementations()
		{
			var template = new Mock<IHouseBillTemplate>();
			template.SetupGet(d => d.DataContext).Returns("BillOfLading");

			var instructions = new ElectronicBOLMessageInstructions(template.Object);

			CombineAssertions(() =>
			{
				AssertEquals("DataContext", "BillOfLading", instructions.DataContext);
				AssertEquals("MenuName", "Electronic House Bill", instructions.DocumentName);
				AssertEquals("Recipient", "Title Registry", instructions.Recipient);
				AssertNullOrEmpty("EHubClientID", instructions.EHubClientID);
				AssertEquals("DirectXTClientID", "ELECTRONIC_BILL_OF_LADING", instructions.DirectXTClientID);
				AssertEquals("XmlNamespace", DocDataConstants.XmlNamespaces.EHBL, instructions.XmlNamespace);

				Assert("AllowSendMessage", instructions.AllowSendMessage);
				Assert("AllowSendMessageAmendment", !instructions.AllowSendMessageAmendment);
				Assert("AllowSendMessageWithdrawal", !instructions.AllowSendMessageWithdrawal);
				Assert("AllowResetToOriginal", !instructions.AllowResetToOriginal);
				Assert("OrderLogsByLocalTime", !instructions.OrderLogsByLocalTime);
				Assert("RequireMessageAmendmentReason", !instructions.RequireMessageAmendmentReason);

				AssertEquals("instructions.AmendmentOptions.Count", 0, instructions.AmendmentOptions.Count);
				AssertEquals("instructions.AmendmentOptions.Count", 0, instructions.WidthdrawalOptions.Count);
			});
		}
	}
}
