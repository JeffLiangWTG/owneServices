using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.US.AIM.Messaging.Constants;
using AsycudaManifestHeader = Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	class AIMBillsSelectionDialogTest : TestCaseWithFactory
	{
		public void TestIsValidToSend()
		{
			var message = "It is likely that your message(s) will be rejected by Customs. Would you like to send despite these errors?";
			var billAlreadySentMessage = "Message Error - Checked: Bill 10000001 has already been sent.";
			var billNotBeenSentMessage = "Message Error - Checked: Bill 10000001 has not been sent to Customs yet.";
			var reasonInvalidMessage = "Message Error - Reason: You have not entered a value.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "10000001";

			var chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRI);	// Send
			using (var dlg = new AIMBillsSelectionDialogForTest(chooser, "Bills"))
			{
				dlg.SelectAll();

				bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Registered;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertContains(message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(billAlreadySentMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				bill.ABL_MessageStatus = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				dlg.CheckIsValidToSend();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRC);	// Amend
			using (var dlg = new AIMBillsSelectionDialogForTest(chooser, "Bills"))
			{
				dlg.SelectAll();

				bill.ABL_MessageStatus = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertContains(message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(billNotBeenSentMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(reasonInvalidMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Registered;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertContains(message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(billNotBeenSentMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(reasonInvalidMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				chooser.Reason = AIMReasonCodes.Codes.R01;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRX);	// Cancel
			using (var dlg = new AIMBillsSelectionDialogForTest(chooser, "Bills"))
			{
				dlg.SelectAll();

				bill.ABL_MessageStatus = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertContains(message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(billNotBeenSentMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(reasonInvalidMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Registered;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertContains(message, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(billNotBeenSentMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(reasonInvalidMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				chooser.Reason = AIMReasonCodes.Codes.R01;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				dlg.CheckIsValidToSend();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIsValidToSendReturnValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "10000001";

			var chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRC);
			using (var dlg = new AIMBillsSelectionDialogForTest(chooser, "Bills"))
			{
				dlg.SelectAll();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert(!dlg.CheckIsValidToSend());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert(dlg.CheckIsValidToSend());
			}
		}

		class AIMBillsSelectionDialogForTest : AIMBillsSelectionDialog
		{
			public AIMBillsSelectionDialogForTest(AIMMessageChooser messageChooser, string itemsType) : base(messageChooser, itemsType)
			{
			}

			public bool CheckIsValidToSend()
			{
				return base.IsValidToSend();
			}
		}
	}
}
