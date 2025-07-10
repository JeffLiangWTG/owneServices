using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	class TransferHeaderItemSelectionDialogTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var header = Factory.New<Business.AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, false);
			using (var chooserDialog = new TransferHeaderItemSelectionDialog(chooser, "Arrival Header"))
			{
				chooserDialog.Show();
				var grid = chooserDialog.FindSingle<ZGrid>("ItemsGrid");
				var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertContainsExactElementsInAnyOrder(new string[] { "Checked", "VoyageFlightNo", "ETAAtDischargePort", "ArrivalReference", "DestinationPort", "TransferType", "InBondCarrier", "InBondCarrierID", "OnwardCarrier", "BondedPremises", "BondedPremisesID" }, columnNames);
			}
		}

		public void TestGridColumns_ArrivalMessage()
		{
			var header = Factory.New<Business.AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			using (var chooserDialog = new TransferHeaderItemSelectionDialog(chooser, "Arrival Header"))
			{
				chooserDialog.Show();
				var grid = chooserDialog.FindSingle<ZGrid>("ItemsGrid");
				var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertContainsExactElementsInAnyOrder(new string[] { "Checked", "StatusCode", "VoyageFlightNo", "ETAAtDischargePort", "ArrivalReference", "DestinationPort", "TransferType", "InBondCarrier", "InBondCarrierID", "OnwardCarrier", "BondedPremises", "BondedPremisesID" }, columnNames);
			}
		}

		public void TestIsValidToSend()
		{
			var header = Factory.New<Business.AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BIL01";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader.TransferHeaders.AddNew();
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill1.ABL_BillNumber;
			var transferHeaderSelectionItem1 = new TransferHeaderSelectionItem(transferHeader1);
			var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem2 = new TransferHeaderSelectionItem(transferHeader2);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem1, transferHeaderSelectionItem2 }, true, true);
			using (var dlg = new TransferHeaderItemSelectionDialogForTest(chooser, "Bills"))
			{
				chooser.ChooserItems[0].Checked = false;
				chooser.ChooserItems[1].Checked = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var chooserItem1 = chooser.ChooserItems[0];
				chooserItem1.Checked = true;
				Assert("Can send message", dlg.CheckIsValidToSend());
			}

			using (var dlg = new TransferHeaderItemSelectionDialogForTest(chooser, "Bills"))
			{
				chooser.ChooserItems[0].Checked = false;
				chooser.ChooserItems[1].Checked = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dlg.SelectAll();
				UnitTestUserNotification.Instance.AddOKAnswer();
				Assert("Cannot send message. Only can select 1 bill", !dlg.CheckIsValidToSend());
				chooser.ChooserItems[0].Checked = false;
				chooser.ChooserItems[1].Checked = false;
			}

			using (var dlg = new TransferHeaderItemSelectionDialogForTest(chooser, "Bills"))
			{
				chooser.ChooserItems[0].Checked = false;
				chooser.ChooserItems[1].Checked = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var chooserItem2 = chooser.ChooserItems[1];
				chooserItem2.Checked = true;
				UnitTestUserNotification.Instance.AddOKAnswer();
				Assert("Cannot send message. There is no valid bill", !dlg.CheckIsValidToSend());
				AssertContains(TransferHeaderMessageChooserItemValidation.TransferHasNoValidBills, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			transferBill1.ATB_MessageStatus = US.AIM.Messaging.AIMTransferStatusCodes.Codes.Arrived;
			using (var dlg = new TransferHeaderItemSelectionDialogForTest(chooser, "Bills"))
			{
				chooser.ChooserItems[0].Checked = false;
				chooser.ChooserItems[1].Checked = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var chooserItem1 = chooser.ChooserItems[0];
				chooserItem1.Checked = true;
				UnitTestUserNotification.Instance.AddYesAnswer();
				Assert("Can send message with warning.", dlg.CheckIsValidToSend());
				AssertContains(TransferHeaderMessageChooserItemValidation.TransferAlreadyArrivedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var dlg = new TransferHeaderItemSelectionDialogForTest(chooser, "Bills"))
			{
				chooser.ChooserItems[0].Checked = false;
				chooser.ChooserItems[1].Checked = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var chooserItem1 = chooser.ChooserItems[0];
				chooserItem1.Checked = true;
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				Assert("Cannot send message.", !dlg.CheckIsValidToSend());
				AssertContains(TransferHeaderMessageChooserItemValidation.TransferAlreadyArrivedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class TransferHeaderItemSelectionDialogForTest : TransferHeaderItemSelectionDialog
		{
			public TransferHeaderItemSelectionDialogForTest(TransferHeaderMessageChooser messageChooser, string itemsType) : base(messageChooser, itemsType)
			{
			}

			public bool CheckIsValidToSend() => base.IsValidToSend();
		}
	}
}
