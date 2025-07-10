using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	class USAMSInBondUserControlTest : TestCaseWithFactory
	{
		public void TestCannotAllocateWhenSomeoneElseIsAllocating()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var header = factory1.New<CusInBondHeader>();
			var moveHeader = header.InBondMovementHeaders.AddNew();
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var moveHeaderInFactory2 = factory2.Load<CusInBondMoveHeader>(moveHeader.PK);
			moveHeaderInFactory2.LockInBondNumberAllocationMutex();
			using (var form = new ZForm(header))
			using (var userControl = new USAMSInBondUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals(true, userControl.Visible);
				var inBondSplitContainer = (CargoWise.Windows.UI.KSplitContainer)userControl.Controls["InBondSplitContainer"];
				var inBondMovementHeadersGroupBox = (ZGroupBox)inBondSplitContainer.Panel1.Controls["InBondMovementHeadersGroupBox"];
				var inBondMovementHeaderDetailsGroupBox = (ZGroupBox)inBondMovementHeadersGroupBox.Controls["InBondMovementHeaderDetailsGroupBox"];
				var inBondMovementHeaderTabControl = (ZTabControl)inBondMovementHeaderDetailsGroupBox.Controls["InBondMovementHeaderTabControl"];
				var inBondMovementHeaderDepartureTabPage = (ZTabPage)inBondMovementHeaderTabControl.Controls["InBondMovementHeaderDepartureTabPage"];
				var inBondMovementHeaderInBondNumberAllocationButton = (ZButton)inBondMovementHeaderDepartureTabPage.Controls["InBondMovementHeaderInBondNumberAllocationButton"];
				var inBondMovementHeadersGrid = (ZGrid)inBondMovementHeadersGroupBox.Controls["InBondMovementHeadersGrid"];
				AssertEquals(false, inBondMovementHeaderInBondNumberAllocationButton.ReadOnly);
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondMovementHeaderInBondNumberAllocationButton.PerformClick();
				AssertEquals("Allocate In-Bond Number", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(CusInBondMoveHeader.InBondNumberAllocationMutexLockText(moveHeader.GetInBondNumberAllocationMutexLockInfo()), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("", moveHeader.InBondNumber);
			}

			moveHeaderInFactory2.UnLockInBondNumberAllocationMutex();
		}

		public void TestInBondMovementHeaderInBondNumberAllocationButton_Click()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var header = Factory.New<CusInBondHeader>();
			using (var form = new ZForm(header))
			using (var userControl = new USAMSInBondUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				header.InBondMovementHeaders.DeleteAll();
				AssertEquals(true, userControl.Visible);
				var inBondSplitContainer = (CargoWise.Windows.UI.KSplitContainer)userControl.Controls["InBondSplitContainer"];
				var inBondMovementHeadersGroupBox = (ZGroupBox)inBondSplitContainer.Panel1.Controls["InBondMovementHeadersGroupBox"];
				var inBondMovementHeaderDetailsGroupBox = (ZGroupBox)inBondMovementHeadersGroupBox.Controls["InBondMovementHeaderDetailsGroupBox"];
				var inBondMovementHeaderTabControl = (ZTabControl)inBondMovementHeaderDetailsGroupBox.Controls["InBondMovementHeaderTabControl"];
				var inBondMovementHeaderDepartureTabPage = (ZTabPage)inBondMovementHeaderTabControl.Controls["InBondMovementHeaderDepartureTabPage"];
				var inBondMovementHeaderInBondNumberAllocationButton = (ZButton)inBondMovementHeaderDepartureTabPage.Controls["InBondMovementHeaderInBondNumberAllocationButton"];
				var inBondMovementHeadersGrid = (ZGrid)inBondMovementHeadersGroupBox.Controls["InBondMovementHeadersGrid"];
				AssertEquals(true, inBondMovementHeaderInBondNumberAllocationButton.ReadOnly);
				var moveHeader1 = header.InBondMovementHeaders.AddNew();
				var moveHeader2 = header.InBondMovementHeaders.AddNew();
				AssertEquals(false, inBondMovementHeaderInBondNumberAllocationButton.ReadOnly);
				inBondMovementHeadersGrid.SelectSingleElement(moveHeader1);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondMovementHeaderInBondNumberAllocationButton.PerformClick();
				AssertEquals("The In-Bond Job must be saved before an In-Bond Number is allocated.\r\nDo you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("", moveHeader1.InBondNumber);
				AssertEquals("", moveHeader2.InBondNumber);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				inBondMovementHeaderInBondNumberAllocationButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("", moveHeader1.InBondNumber);
				AssertEquals("", moveHeader2.InBondNumber);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondMovementHeaderInBondNumberAllocationButton.PerformClick();
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("", moveHeader1.InBondNumber);
				AssertEquals("", moveHeader2.InBondNumber);
				moveHeader1.InBondNumber = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondMovementHeaderInBondNumberAllocationButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(US.GUI.AllocateInBondNumberForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				moveHeader1.Messages.AddNew();
				ZFormModaliser.LastFormShownDialogForTest = null;
				inBondMovementHeaderInBondNumberAllocationButton.PerformClick();
				AssertEquals(CusInBondMoveHeader.Constants.InBondNumberAlreadyAllocated(moveHeader1.InBondNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				moveHeader1.UnLockInBondNumberAllocationMutex();
				moveHeader2.UnLockInBondNumberAllocationMutex();
			}
		}
	}
}
