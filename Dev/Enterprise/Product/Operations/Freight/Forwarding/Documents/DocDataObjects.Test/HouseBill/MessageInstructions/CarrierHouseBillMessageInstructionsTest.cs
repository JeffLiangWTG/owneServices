using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class CarrierHouseBillMessageInstructionsTest : TestCaseWithFactory
	{
		public void TestImplementations()
		{
			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.MenuName).Returns("Bill Of Lading");

			var template = new Mock<IHouseBillTemplate>();
			template.SetupGet(d => d.DataContext).Returns("BillOfLading");

			var instructions = new CarrierHouseBillMessageInstructions(documentPivot.Object, template.Object);

			CombineAssertions(() =>
			{
				AssertEquals("DataContext", "BillOfLading", instructions.DataContext);
				AssertEquals("MenuName", "Bill Of Lading", instructions.DocumentName);
				AssertEquals("Recipient", "Booking Party", instructions.Recipient);
				AssertEquals("EHubClientID", "SHIPPING_INSTRUCTION", instructions.EHubClientID);
				AssertEquals("XmlNamespace", "/BLData/1", instructions.XmlNamespace);

				Assert("Enabled sending Carrier message using registry", instructions.AllowSendMessage);
				Assert("AllowSendMessageAmendment", !instructions.AllowSendMessageAmendment);
				Assert("AllowSendMessageWithdrawal", !instructions.AllowSendMessageWithdrawal);
				Assert("AllowResetToOriginal", !instructions.AllowResetToOriginal);
				Assert("OrderLogsByLocalTime", !instructions.OrderLogsByLocalTime);
				Assert("RequireMessageAmendmentReason", !instructions.RequireMessageAmendmentReason);

				AssertEquals("instructions.AmendmentOptions.Count", 0, instructions.AmendmentOptions.Count);
				AssertEquals("instructions.AmendmentOptions.Count", 0, instructions.WidthdrawalOptions.Count);
			});
		}

		public void TestDocumentName()
		{
			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuCarrierBillOfLadingPK);
			AssertEquals("Please also update this document name", ShipmentDocumentNames.DraftBill, menuItem.SU_MenuName);
		}
	}
}
