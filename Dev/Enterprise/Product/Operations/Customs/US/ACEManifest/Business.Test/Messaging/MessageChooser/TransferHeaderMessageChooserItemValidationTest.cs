using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class TransferHeaderMessageChooserItemValidationTest : TestCaseWithFactory
	{
		public void TestCheckChecked_Error()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem1 = new TransferHeaderSelectionItem(transferHeader1);
			var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem2 = new TransferHeaderSelectionItem(transferHeader2);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem1, transferHeaderSelectionItem2 }, true, true);
			var transferHeaderMessageChooserItem1 = chooser.ChooserItems[0];
			transferHeaderMessageChooserItem1.Checked = true;
			AssertNoError(transferHeaderMessageChooserItem1.CheckedInfo, "More than 1 transfer is selected");
			var transferHeaderMessageChooserItem2 = chooser.ChooserItems[1];
			transferHeaderMessageChooserItem2.Checked = true;
			AssertHasError(transferHeaderMessageChooserItem2.CheckedInfo, "More than 1 transfer is selected");
		}

		public void TestCheckChecked_TransferArrivedWarning()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem1 = new TransferHeaderSelectionItem(transferHeader1);
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill.ABL_BillNumber;
			transferBill1.ATB_MessageStatus = US.AIM.Messaging.AIMTransferStatusCodes.Codes.Arrived;
			var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem2 = new TransferHeaderSelectionItem(transferHeader2);
			var transferBill2 = transferHeader2.TransferBills.AddNew();
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem1, transferHeaderSelectionItem2 }, true, true);
			var transferHeaderMessageChooserItem1 = chooser.ChooserItems[0];
			transferHeaderMessageChooserItem1.Checked = true;
			AssertHasWarning(transferHeaderMessageChooserItem1.CheckedInfo, "This transfer appears to be already Arrived.  Are you sure you wish to send this again?");
			transferHeaderMessageChooserItem1.Checked = false;
			var transferHeaderMessageChooserItem2 = chooser.ChooserItems[1];
			transferHeaderMessageChooserItem2.Checked = true;
			AssertNoWarning(transferHeaderMessageChooserItem1.CheckedInfo, "This transfer appears to be already Arrived.  Are you sure you wish to send this again?");
			var chooser2 = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem1 }, true, false);
			transferHeaderMessageChooserItem1 = chooser.ChooserItems[0];
			transferHeaderMessageChooserItem1.Checked = true;
			AssertNoWarning(transferHeaderMessageChooserItem1.CheckedInfo, "This transfer appears to be already Arrived.  Are you sure you wish to send this again?");
		}

		public void TestCheckChecked_TransferHasNoValidBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader1 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem1 = new TransferHeaderSelectionItem(transferHeader1);
			var transferBill1 = transferHeader1.TransferBills.AddNew();
			transferBill1.ATB_BillNumber = bill.ABL_BillNumber;
			var transferHeader2 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem2 = new TransferHeaderSelectionItem(transferHeader2);
			var transferHeader3 = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem3 = new TransferHeaderSelectionItem(transferHeader3);
			var transferBill3 = transferHeader3.TransferBills.AddNew();
			transferBill3.ATB_BillNumber = "AAA";
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem1, transferHeaderSelectionItem2, transferHeaderSelectionItem3 }, true, true);
			var transferHeaderMessageChooserItem1 = chooser.ChooserItems[0];
			transferHeaderMessageChooserItem1.Checked = true;
			AssertNoWarning(transferHeaderMessageChooserItem1.CheckedInfo, "This transfer does not have any valid bill. No message will be sent.");
			transferHeaderMessageChooserItem1.Checked = false;
			var transferHeaderMessageChooserItem2 = chooser.ChooserItems[1];
			transferHeaderMessageChooserItem2.Checked = true;
			AssertHasWarning(transferHeaderMessageChooserItem2.CheckedInfo, "This transfer does not have any valid bill. No message will be sent.");
			transferHeaderMessageChooserItem2.Checked = false;
			var transferHeaderMessageChooserItem3 = chooser.ChooserItems[2];
			transferHeaderMessageChooserItem3.Checked = true;
			AssertHasWarning(transferHeaderMessageChooserItem3.CheckedInfo, "This transfer does not have any valid bill. No message will be sent.");
			transferHeaderMessageChooserItem3.Checked = false;
		}
	}
}
