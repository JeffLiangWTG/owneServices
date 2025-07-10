using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class FIATAHouseBillMessageInstructionsTest : TestCaseWithFactory
	{
		public void TestImplementations()
		{
			var documentPivot = new Mock<IDocumentPivot>();
			documentPivot.SetupGet(d => d.MenuName).Returns("Bill Of Lading");

			var template = new Mock<IHouseBillTemplate>();
			template.SetupGet(d => d.DataContext).Returns("BillOfLading");

			var instructions = new FIATAHouseBillMessageInstructions(documentPivot.Object, template.Object);

			using (FreightDataRegistry.Instance.EnableFIATAHouseBillsFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					AssertEquals("DataContext", "BillOfLading", instructions.DataContext);
					AssertEquals("MenuName", "Bill Of Lading", instructions.DocumentName);
					AssertEquals("Recipient", "HOUSEBILLOFLADING", instructions.Recipient);
					AssertEquals("Recipient", "HOUSEBILLOFLADING", instructions.EHubClientID);
					AssertEquals("XmlNamespace", "/HouseBillOfLading/1", instructions.XmlNamespace);

					Assert("Disabled sending FIATA message using registry", !instructions.AllowSendMessage);
					Assert("AllowSendMessageAmendment", instructions.AllowSendMessageAmendment);
					Assert("AllowSendMessageWithdrawal", !instructions.AllowSendMessageWithdrawal);
					Assert("AllowResetToOriginal", !instructions.AllowResetToOriginal);
					Assert("OrderLogsByLocalTime", !instructions.OrderLogsByLocalTime);
					Assert("RequireMessageAmendmentReason", instructions.RequireMessageAmendmentReason);

					AssertNull("instructions.AmendmentOptions", instructions.AmendmentOptions);
					AssertNull("instructions.AmendmentOptions", instructions.WidthdrawalOptions);
				});
			}

			using (FreightDataRegistry.Instance.EnableFIATAHouseBillsFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("DataContext", "BillOfLading", instructions.DataContext);
					AssertEquals("MenuName", "Bill Of Lading", instructions.DocumentName);
					AssertEquals("Recipient", "HOUSEBILLOFLADING", instructions.Recipient);
					AssertEquals("Recipient", "HOUSEBILLOFLADING", instructions.EHubClientID);
					AssertEquals("XmlNamespace", "/HouseBillOfLading/1", instructions.XmlNamespace);

					Assert("Enabled sending FIATA message using registry", instructions.AllowSendMessage);
					Assert("AllowSendMessageAmendment", instructions.AllowSendMessageAmendment);
					Assert("AllowSendMessageWithdrawal", !instructions.AllowSendMessageWithdrawal);
					Assert("AllowResetToOriginal", instructions.AllowResetToOriginal);
					Assert("OrderLogsByLocalTime", !instructions.OrderLogsByLocalTime);
					Assert("RequireMessageAmendmentReason", instructions.RequireMessageAmendmentReason);

					AssertNull("instructions.AmendmentOptions", instructions.AmendmentOptions);
					AssertNull("instructions.AmendmentOptions", instructions.WidthdrawalOptions);
				});
			}
		}
	}
}
