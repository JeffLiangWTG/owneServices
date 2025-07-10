using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class DefaultHouseBillMessageInstructionsTest : TestCaseWithFactory
	{
		public void TestImplementations()
		{
			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.MenuName).Returns("Bill Of Lading");

			var template = new Mock<IHouseBillTemplate>();
			template.SetupGet(d => d.DataContext).Returns("BillOfLading");

			var instructions = new DefaultHouseBillMessageInstructions(documentPivot.Object, template.Object);

			CombineAssertions(() =>
			{
				AssertEquals("DataContext", "BillOfLading", instructions.DataContext);
				AssertEquals("MenuName", "Bill Of Lading", instructions.DocumentName);
				AssertNullOrEmpty("Recipient", instructions.Recipient);
				AssertNullOrEmpty("EHubClientID", instructions.EHubClientID);
				AssertNullOrEmpty("DirectXTClientID", instructions.DirectXTClientID);
				AssertNullOrEmpty("XmlNamespace", instructions.XmlNamespace);

				Assert("AllowSendMessage", !instructions.AllowSendMessage);
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
