using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using MessageSendingObject = Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject;
using SupportingDocument = Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(SendToCustomsMessageSendingForm))]
	sealed class SendToCustomsMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SendToCustomsMessageSendingForm(sendingObjectParent);
		}

		public void TestMessageSendingObjectGridColumns()
		{
			using (var form = new SendToCustomsMessageSendingForm(sendingObjectParent))
			{
				form.Show();
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				CombineAssertions(() =>
				{
					AssertNotNull("MessageSendingObjectsGrid should have ShouldSend column", grid.GetColumnStyle(BaseMessageSendingObject.SchemaShouldSend));
					AssertNotNull("MessageSendingObjectsGrid should have Action column", grid.GetColumnStyle(nameof(MessageSendingObject.Action)));
					AssertNotNull("MessageSendingObjectsGrid should have MessageType column", grid.GetColumnStyle(nameof(MessageSendingObject.MessageType)));
					AssertNotNull("MessageSendingObjectsGrid should have Description column", grid.GetColumnStyle(nameof(MessageSendingObject.Description)));
					AssertNotNull("MessageSendingObjectsGrid should have EntryNumber column", grid.GetColumnStyle(nameof(MessageSendingObject.EntryNumber)));
				});
			}
		}

		public void TestSupportingDocumentsGridColumns()
		{
			using (var form = new SendToCustomsMessageSendingForm(sendingObjectParent))
			{
				form.Show();
				var grid = form.Controls.Find("SupportingDocumentsGrid", true)[0] as ZGrid;
				CombineAssertions(() =>
				{
					AssertNotNull("SupportingDocumentsGrid should have BillNumber column", grid.GetColumnStyle(nameof(SupportingDocument.BillNumber)));
					AssertNotNull("SupportingDocumentsGrid should have eDoc column", grid.GetColumnStyle(nameof(SupportingDocument.EDoc)));
					AssertNotNull("SupportingDocumentsGrid should have DocumentNo column", grid.GetColumnStyle(nameof(SupportingDocument.DocumentNo)));
					AssertNotNull("SupportingDocumentsGrid should have Remarks column", grid.GetColumnStyle(nameof(SupportingDocument.Remarks)));
					AssertNotNull("SupportingDocumentsGrid should have ControllingAgency column", grid.GetColumnStyle(nameof(SupportingDocument.ControllingAgency)));
					AssertNotNull("SupportingDocumentsGrid should have Type column", grid.GetColumnStyle(nameof(SupportingDocument.Type)));
				});
			}
		}

		public void TestSendWithAdditionalWarningCheckBoxVisible()
		{
			using (var form = new SendToCustomsMessageSendingForm(sendingObjectParent))
			{
				var sendWithAdditionalWarningCheckBox = form.Controls.Find("SendWithAdditionalWarningCheckBox", true)[0] as ZCheckBox;
				AssertEquals("sendWithAdditionalWarningCheckBox is hidden", false, sendWithAdditionalWarningCheckBox.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			sendingObjectParent = new MessageSendingObjectParent(header, MessageTypeList.Codes.IBC);
		}

		MessageSendingObjectParent sendingObjectParent;
	}
}
