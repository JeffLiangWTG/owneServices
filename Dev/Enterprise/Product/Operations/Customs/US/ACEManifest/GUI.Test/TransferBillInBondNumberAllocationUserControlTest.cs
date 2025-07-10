using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	class TransferBillInBondNumberAllocationUserControlTest : TestCaseWithFactory
	{
		public void TestInBondNumberAllocationButton_Click()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "STD";
			using (var form = new ZForm(arrivalHeader))
			using (var control = new TransferBillInBondNumberAllocationUserControl())
			{
				form.Controls.Add(control);
				control.SetBindingMember("TransferHeaders.TransferBills");
				form.Show();
				var inBondNumberAllocationButton = control.FindSingle<ZButton>("InBondNumberAllocationButton");
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertEquals("This Transfer must be saved before an In-Bond Number is allocated.\r\nDo you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("", transferBill.InBondNumber);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("", transferBill.InBondNumber);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondNumberAllocationButton.PerformClick();
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("", transferBill.InBondNumber);
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondNumberAllocationButton.PerformClick();
				AssertEquals(US.Business.CusInBondMoveHeader.Constants.InBondNumberAlreadyAllocated(transferBill.InBondNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				transferBill.InBondNumber = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondNumberAllocationButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotEquals("", transferBill.InBondNumber);
			}
		}

		public void TestInBondNumberAllocation_DeletedInAnotherFactory()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "STD";
			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var arrivalHeaderIOF = newFactory.Load<AsycudaArrivalHeader>(arrivalHeader.PK);
			var transferBillIOF = newFactory.Load<AsycudaTransferBill>(transferBill.PK);
			using (var form = new ZForm(arrivalHeaderIOF))
			using (var control = new TransferBillInBondNumberAllocationUserControl())
			{
				form.Controls.Add(control);
				control.SetBindingMember("TransferHeaders.TransferBills");
				form.Show();
				var inBondNumberAllocationButton = control.FindSingle<ZButton>("InBondNumberAllocationButton");
				transferBill.Delete();
				Factory.Save();
				Assert(transferBillIOF.HasBeenDeleted);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberAllocationButton.PerformClick();
				AssertEquals("This In-Bond Movement Header has been deleted by another user while you had the job open. Please close the job, re-open and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestInBondNumberResetButton_Click()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "STD";
			transferBill.ATB_BillNumber = "B123";
			transferBill.AllocateInBondNumber("1234");
			using (var form = new ZForm(arrivalHeader))
			using (var control = new TransferBillInBondNumberAllocationUserControl())
			{
				form.Controls.Add(control);
				control.SetBindingMember("TransferHeaders.TransferBills");
				form.Show();
				var inBondNumberResetButton = control.FindSingle<ZButton>("InBondNumberResetButton");
				Env.Security.USInBondResetToOriginal.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondNumberResetButton.PerformClick();
				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.USInBondResetToOriginal), UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.USInBondResetToOriginal.IsAllowed = true;
				transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				inBondNumberResetButton.PerformClick();
				AssertEquals("This Transfer is currently waiting or lodged at customs. The In-Bond Number cannot be reset at this time.", UnitTestUserNotification.Instance.LastMessage.Text);
				transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
				transferBill.ATB_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				inBondNumberResetButton.PerformClick();
				AssertEquals("This Transfer must be saved before an In-Bond Number is reset.\r\nDo you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Not Reset", !transferBill.InBondNumber.IsEmpty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddUserResponse("TEST RESET");
				UnitTestUserNotification.Instance.AddOKAnswer();
				inBondNumberResetButton.PerformClick();
				AssertEquals("Please enter the reason for resetting this In-Bond number.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("In-Bond number is reset.", ZString.Empty, transferBill.InBondNumber);
			}
		}
	}
}
