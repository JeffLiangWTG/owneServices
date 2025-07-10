using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	public class EDIMenuTest : TestCaseWithFactory
	{
		public void TestSendMessages_ShouldSaveFormFirst()
		{
			var header = Factory.New<CusUSLVClearance>();
			header.HasChanges = true;

			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendOriginalMessageMenuItem(testMenu);
				AssertNotNull(item);

				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("The data has not yet been saved. Do you want to save and proceed?"));
			}
		}

		public void TestSendOriginalMessages_ParentFormWouldNotSaveAfterMessagesSubmitFormClose()
		{
			var header = Factory.New<CusUSLVClearance>();
			header.CusUSLVConsignments.AddNew();

			using (var testMenu = GetEDIMenu())
			using (var form = new MockForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendOriginalMessageMenuItem(testMenu);
				AssertNotNull(item);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				item.PerformClick();
			}
		}

		public void TestSendMessages_MutexLockMessage()
		{
			var header = Factory.New<CusUSLVClearance>();
			header.CusUSLVConsignments.AddNew();
			Factory.Save();
			header.LockSendCustomsMessageMutex();

			using (var testMenu = GetEDIMenu())
			using (var form = new MockForm(header))
			{
				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendOriginalMessageMenuItem(testMenu);
				AssertNotNull(item);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				item.PerformClick();
			}
			header.UnlockSendCustomsMessageMutex();
			AssertEndsWith("mutex error message", "is sending messages for this Low Value Entries job, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public virtual void TestSendOriginalMessages_Click()
		{
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			Factory.Save();

			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendOriginalMessageMenuItem(testMenu);
				AssertNotNull(item);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There is no house bill eligible for sending Original Messages."));
				consignment.CE_EntryStatus = "";
				item.PerformClick();
				AssertEquals(typeof(MessagesSubmitForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public virtual void TestSendReplacementMessages_Click()
		{
			var consignment2 = header.CusUSLVConsignments.AddNew();

			Factory.Save();

			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendReplacementMessageMenuItem(testMenu);
				AssertNotNull(item);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There is no house bill eligible for sending Replacement Messages."));
				consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
				consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
				item.PerformClick();
				AssertEquals(typeof(MessagesSubmitForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public virtual void TestSendUpdateMessages_Click()
		{
			var consignment2 = header.CusUSLVConsignments.AddNew();

			Factory.Save();

			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendUpdateMessageMenuItem(testMenu);
				AssertNotNull(item);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There is no house bill eligible for sending Update Messages."));
				consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
				consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
				item.PerformClick();
				AssertEquals(typeof(MessagesSubmitForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public virtual void TestSendDeletionMessages_Click()
		{
			var consignment2 = header.CusUSLVConsignments.AddNew();

			Factory.Save();

			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendDeletionMessageMenuItem(testMenu);
				AssertNotNull(item);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There is no house bill eligible for sending Deletion Messages."));
				consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
				consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
				item.PerformClick();
				AssertEquals(typeof(MessagesSubmitForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestCancelMessageSubmitForm_WillNotSaveParentForm()
		{
			header.ULH_ContactName = "Mr. Lazy";
			header.ULH_ContactPhone = "0404040404";
			header.ULH_ContainerMode = "ABC";
			header.ULH_TransportMode = "AIR";
			header.ULH_EntryFilerCode = "XJ5";
			header.ULH_MasterBillIssuerSCAC = "AA";
			header.ULH_CarrierSCAC = "AA";
			var consignment2 = header.CusUSLVConsignments.AddNew();

			Factory.Save();

			var hasSaved = false;
			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(header))
			{
				form.Saved += Form_Saved;

				testMenu.Header = header;
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendReplacementMessageMenuItem(testMenu);
				AssertNotNull(item);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
				consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
				item.PerformClick();

				AssertEquals("Header will not be saved to database if choose Cancel", false, hasSaved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				item.PerformClick();

				AssertEquals("Post condition: Header can be saved to database if choose Yes", true, hasSaved);
			}

			void Form_Saved(object sender, System.EventArgs e)
			{
				hasSaved = true;
			}
		}

		protected override void SetUp()
		{
			header = Factory.New<CusUSLVClearance>();
			consignment = header.CusUSLVConsignments.AddNew();
		}

		protected CusUSLVClearance header;
		protected CusUSLVConsignment consignment;

		protected virtual EDIMenu GetEDIMenu()
		{
			return new EDIMenu() { Header = header };
		}

		protected virtual MenuItem GetSendOriginalMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Original Messages");

		protected virtual MenuItem GetSendReplacementMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Replacement Messages");

		protected virtual MenuItem GetSendUpdateMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Update Messages");

		protected virtual MenuItem GetSendDeletionMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Deletion Messages");

		class MockForm : ZForm
		{
			public MockForm(BusinessObject bo) : base(bo)
			{
			}

			public override ContinueWithSave FireSaveButton(object sender = null)
			{
				Assert("should not call FireSaveButton", false);
				return base.FireSaveButton(sender);
			}
		}
	}
}
