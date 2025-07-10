using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.GUI.TradeSingleWindow;
using Enterprise.Customs.NZ.Manifest.Business;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestOutwardReportMenu()
		{
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var menuItem = menu.MenuItems[0];
				AssertEquals("Outward Report", menuItem.Text);
				AssertEquals("Submit Outward Report", menuItem.MenuItems[0].Text);
				AssertEquals("-", menuItem.MenuItems[1].Text);
				AssertEquals("Submit Outward Report with comment", menuItem.MenuItems[2].Text);
				AssertEquals("Submit Outward Report with attachments", menuItem.MenuItems[3].Text);
			});
		}

		public void TestOutwardReportMenuCancelVisibleAfterSent()
		{
			header.AMA_MessageStatus = NZMessageStatusList.Codes.Sent;
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var menuItem = menu.MenuItems[0];
				AssertEquals("Outward Report", menuItem.Text);
				AssertEquals("Submit Outward Report", menuItem.MenuItems[0].Text);
				AssertEquals("Cancel Outward Report", menuItem.MenuItems[1].Text);
				AssertEquals("-", menuItem.MenuItems[2].Text);
				AssertEquals("Submit Outward Report with comment", menuItem.MenuItems[3].Text);
				AssertEquals("Submit Outward Report with attachments", menuItem.MenuItems[4].Text);
			});
		}

		public void TestOCRSendsOriginalUntilORNReceived()
		{
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			header.AMA_JobReference = "MAN0000023";
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Outward Report");
				AssertNotNull("Send OCR", sendManifestMenuItem);
				var sendOCR = sendManifestMenuItem.MenuItems[0];
				sendOCR.PerformClick();
				AssertNotContains("Ensure message sending doesn't have environment validation error", "Unable to send Original due to the following errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should send ORG from the header.", MessageSubTypeCodes.Codes.Original, header.MessageFunctionSubTypeForSend);

				header.RegistrationNumber = "62938847";
				header.AMA_MessageStatus = NZMessageStatusList.Codes.Accepted;
				sendOCR.PerformClick();
				AssertNotContains("Ensure message sending doesn't have environment validation error", "Unable to send Replace due to the following errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should send ORG from the header.", MessageSubTypeCodes.Codes.Original, header.MessageFunctionSubTypeForSend);
			});
		}

		public void TestSubmitOutwardReportUntilCustomsRespond()
		{
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			header.AMA_MessageStatus = NZMessageStatusList.Codes.Sent;
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Outward Report");
				AssertNotNull("Send OCR", sendManifestMenuItem);
				var sendOCR = sendManifestMenuItem.MenuItems[0];
				sendOCR.PerformClick();
				AssertContains("Submit Outward Report before Customs respond", "There is a message pending. Please wait for the Customs response.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				header.AMA_MessageStatus = NZMessageStatusList.Codes.Acknowledged;
				sendOCR.PerformClick();
				AssertContains("Submit Outward Report after Customs acknowledged", "There is a message pending. Please wait for the Customs response.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				header.AMA_MessageStatus = NZMessageStatusList.Codes.Accepted;
				sendOCR.PerformClick();
				AssertNotContains("Submit Outward Report after Customs respond", "There is a message pending. Please wait for the Customs response.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestICRMenuItemVisibility()
		{
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("Send Manifest", sendManifestMenuItem);
				var ammendManifestMenuItem = menu.MenuItems.FindByText("Amend Manifest");
				AssertNull("Amend Manifest", ammendManifestMenuItem);
				var cancelManifestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertNull("Cancel Manifest", cancelManifestMenuItem);
			});
			header.AMA_MessageStatus = "SNT";
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNull("Send Manifest", sendManifestMenuItem);
				var ammendManifestMenuItem = menu.MenuItems.FindByText("Amend Manifest");
				AssertNull("Amend Manifest", ammendManifestMenuItem);
				var cancelManifestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertNotNull("Cancel Manifest", cancelManifestMenuItem);
			});
			header.RegistrationNumber = "123456789";
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNull("Send Manifest", sendManifestMenuItem);
				var ammendManifestMenuItem = menu.MenuItems.FindByText("Amend Manifest");
				AssertNotNull("Amend Manifest", ammendManifestMenuItem);
				var cancelManifestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertNotNull("Cancel Manifest", cancelManifestMenuItem);
			});
		}

		public void TestICRSendManifest_MessageQueued()
		{
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("Send Manifest", sendManifestMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Accept this dummy message is riddled with errors
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // Click-through the Supporting Documents Window.
				sendManifestMenuItem.PerformClick();
				AssertEquals("Original message queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(typeof(TSWSendFormWithAttachments), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}

		public void TestICRSendManifest_WithAttachment()
		{
			var imageBytes = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.NZ.Manifest.GUI.Testing.TestDocs.Blank.pdf");
			var eDoc = header.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", Core.Constants.RefDocTypes.CommercialInvoice);
			Factory.Save();
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("Send Manifest", sendManifestMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendManifestMenuItem.PerformClick();
				AssertEquals("Original message queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				var supportingDocumentPickingForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertType(typeof(TSWSendFormWithAttachments), supportingDocumentPickingForm);
				var dataSource = ((TSWSendFormWithAttachments)supportingDocumentPickingForm).LastDataSourceForTest;
				AssertType(typeof(AdditionalMessageInformation), dataSource);
				Assert(((AdditionalMessageInformation)dataSource).SupportingDocuments.StorageDocs.ContainsCode("CIV-Invoice.pdf"));
			});
		}

		public void TestICRSendManifest_SendFailed()
		{
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				AssertNotNull("Send Manifest", sendManifestMenuItem);
				Factory.Saving += (factory) =>
				{
					throw new ZCannotSaveException("Test Error", "");
				};
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendManifestMenuItem.PerformClick();
				AssertEquals("Unable to send message", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!header.Messages.Any());
			});
		}

		public void TestICRAmendManifest()
		{
			header.RegistrationNumber = "123456789";
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var ammendManifestMenuItem = menu.MenuItems.FindByText("Amend Manifest");
				AssertNotNull("Amend Manifest", ammendManifestMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ammendManifestMenuItem.PerformClick();
				AssertEquals("Replace message queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(typeof(TSWReplaceForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}

		public void TestICRCancelManifest()
		{
			header.AMA_MessageStatus = "SNT";
			CreateManifestMenuOnForm((AsycudaMenuForTest menu) =>
			{
				var cancelManifestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertNotNull("Cancel Manifest", cancelManifestMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				cancelManifestMenuItem.PerformClick();
				AssertEquals("Cancel message queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(typeof(TSWCancelForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}

		AsycudaManifestHeader header;
		IDisposable icrBrokerIdDisposable;
		protected override void SetUp()
		{
			icrBrokerIdDisposable = NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			header.AMA_RL_NKPortOfLoading = "DEFRA";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			header.Bills.AddNew();
			Factory.Save();
		}

		protected override void TearDown()
		{
			icrBrokerIdDisposable.Dispose();
			base.TearDown();
		}

		void CreateManifestMenuOnForm(Action<AsycudaMenuForTest> testFunc)
		{
			using (var form = new ZForm(header))
			{
				var menu = new AsycudaMenuForTest(header); // will be disposed when form is disposed.
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				testFunc(menu);
			}
		}
	}
}
