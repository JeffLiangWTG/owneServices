using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class MessageManagementMenuTest : TestCaseWithFactory
	{
		public void TestFireSaveButtonIfNeeded()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummyBizObj))
			{
				form.Menu.MenuItems.Add(Menu);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dummyBizObj.HasChanges = true;
				Manager.allMessageManagers[0].canSendOriginal = true;
				Menu.OnPopup(EventArgs.Empty);
				Menu.sendMessages.PerformClick();
				AssertEquals("There are changes on this form. Do you want to save changes first?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCellSuspendedWhenSendMessages()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			using (var form = new TestHelperFormForCellSuspend(dummyBizObj))
			{
				menu = new TestHelperMessageManagementMenuForCellSuspend(Manager);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dummyBizObj.HasChanges = true;
				Manager.allMessageManagers[0].canSendOriginal = true;
				menu.OnPopup(EventArgs.Empty);
				menu.sendMessages.PerformClick();
			}
		}

		public void TestCheckError()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummyBizObj))
			{
				form.Menu.MenuItems.Add(Menu);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dummyBizObj.AddRowError("Test error");
				AssertEquals(true, dummyBizObj.HasErrors);
				Manager.allMessageManagers[0].canSendOriginal = true;
				Menu.OnPopup(EventArgs.Empty);
				Menu.sendMessages.PerformClick();
				AssertContains("There are errors that need to be corrected", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestText()
		{
			AssertEquals("Menu.Text", "Messaging", Menu.Text);
		}

		public void TestManager()
		{
			AssertEquals("Manager", Manager, Menu.manager);
		}

		public void TestSendMessages()
		{
			Manager.allMessageManagers[0].canSendOriginal = true;
			Menu.OnPopup(EventArgs.Empty);
			Menu.sendMessages.PerformClick();
			AssertEquals("SaveFactoryCalled", true, Menu.SaveFactoryCalled);
		}

		public void TestSendMessagesWhenMessagingIsSuppressed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Menu.MessagingSuppressed = true;
			Menu.SuppressedReason = "Reason messaging is suppressed.";
			Manager.allMessageManagers[0].canSendOriginal = true;
			Menu.OnPopup(EventArgs.Empty);
			Menu.sendMessages.PerformClick();
			AssertEquals("SaveFactoryCalled", false, Menu.SaveFactoryCalled);
			AssertEquals("Reason messaging is suppressed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWithdrawMessages()
		{
			Manager.allMessageManagers[0].canSendWithdrawal = true;
			Menu.OnPopup(EventArgs.Empty);
			Menu.withdrawMessages.PerformClick();
			AssertEquals("SaveFactoryCalled", true, Menu.SaveFactoryCalled);
		}

		public void TestWithdrawMessagesWhenMessagingIsSuppressed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Menu.MessagingSuppressed = true;
			Menu.SuppressedReason = "Reason messaging is suppressed.";
			Manager.allMessageManagers[0].canSendWithdrawal = true;
			Menu.OnPopup(EventArgs.Empty);
			Menu.withdrawMessages.PerformClick();
			AssertEquals("SaveFactoryCalled", false, Menu.SaveFactoryCalled);
			AssertEquals("Reason messaging is suppressed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAmendMessages()
		{
			Manager.allMessageManagers[0].canSendWithdrawal = true;
			Menu.OnPopup(EventArgs.Empty);
			Menu.amendMessages.PerformClick();
			AssertEquals("SaveFactoryCalled", true, Menu.SaveFactoryCalled);
		}

		public void TestAmendMessagesWhenMessagingIsSuppressed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Menu.MessagingSuppressed = true;
			Menu.SuppressedReason = "Reason messaging is suppressed.";
			Manager.allMessageManagers[0].canSendWithdrawal = true;
			Menu.OnPopup(EventArgs.Empty);
			Menu.amendMessages.PerformClick();
			AssertEquals("SaveFactoryCalled", false, Menu.SaveFactoryCalled);
			AssertEquals("Reason messaging is suppressed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMessageHelpTextMenuItem()
		{
			Menu.OnPopup(EventArgs.Empty);
			Assert(Menu.messagingHelpMenuItem.Visible);
			AssertEquals("Message Help Menu Text", Menu.messagingHelpMenuItem.Text);
		}

		public void TestResetMessages()
		{
			Menu.OnPopup(EventArgs.Empty);
			Menu.resetToOriginal.PerformClick();
			Assert(Manager.allMessageManagers[0].ResetToOriginalCalled);
		}

		public void TestResetMessagesWhenMessagingIsSuppressed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Menu.MessagingSuppressed = true;
			Menu.SuppressedReason = "Reason messaging is suppressed.";
			Menu.OnPopup(EventArgs.Empty);
			Menu.resetToOriginal.PerformClick();
			Assert(!Manager.allMessageManagers[0].ResetToOriginalCalled);
			AssertEquals("Reason messaging is suppressed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSaveFactory()
		{
			using (MessageManagementMenu menu = new MessageManagementMenu(Manager))
			{
				menu.SaveFactoryInternal();
				AssertEquals(true, Dummy.FactorySavedSucceded);
			}
		}

		public void TestSender()
		{
			using (MessageManagementMenu menu = new MessageManagementMenu(Manager))
			{
				AssertEquals("SenderType", typeof(SendsMessagesToCustomsGUI), menu.SenderInternal.GetType());
			}
		}

		public void TestAmendMessagesVisibility()
		{
			Manager.allowManualAmendments = true;
			using (TestHelperMessageManagementMenu menu = new TestHelperMessageManagementMenu(Manager))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("AmendMessages.Visible", true, menu.amendMessages.Visible);
			}

			Manager.allowManualAmendments = false;
			using (TestHelperMessageManagementMenu menu = new TestHelperMessageManagementMenu(Manager))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("AmendMessages.Visible", false, menu.amendMessages.Visible);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (menu != null)
			{
				menu.Dispose();
			}
		}

		TestHelperMessageManagementMenu menu;
		TestHelperMessageManagementMenu Menu
		{
			get
			{
				if (menu == null)
				{
					menu = new TestHelperMessageManagementMenu(Manager);
				}

				return menu;
			}
		}

		TestHelperMultiMessageManager manager;
		TestHelperMultiMessageManager Manager
		{
			get
			{
				if (manager == null)
				{
					manager = new TestHelperMultiMessageManager(Dummy);
					manager.SendResultOverride = new[] { Factory.NewWithValidTestData<EDIMessage>() };
					manager.WithdrawResultOverride = true;
					manager.allMessageManagers = new TestHelperSingleMessageManager[] { new TestHelperSingleMessageManager() };
				}

				return manager;
			}
		}

		MessageManageableBusinessObject dummy;
		MessageManageableBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<MessageManageableBusinessObject>();
				}

				return dummy;
			}
		}

		class TestHelperMessageManagementMenu : MessageManagementMenu
		{
			public TestHelperMessageManagementMenu(MultiMessageManager manager) : base(manager)
			{
			}

			protected override void SaveFactory()
			{
				SaveFactoryCalled = true;
			}

			public bool SaveFactoryCalled;
			protected override ZString MessagingHelpText => "Message Help Menu Text";

			protected override ZString MessagingHelpURL => "Message Help URL";

			protected override ISendsMessagesToCustoms Sender
			{
				get
				{
					var result = new SendsMessagesToCustomsShutterUpperer();
					result.ReturnAllForWhichMessagesShouldWeReset = true;
					result.ReturnAllForWhichMessagesShouldWeSend = true;
					result.ReturnAllForWhichMessagesShouldWeWithdraw = true;
					return result;
				}
			}

			protected override bool IsMessagingSuppressed => MessagingSuppressed;

			internal bool MessagingSuppressed;
			protected override ZString ReasonMessagingIsSuppressed => SuppressedReason;

			internal ZString SuppressedReason;
		}

		sealed class TestHelperMessageManagementMenuForCellSuspend : TestHelperMessageManagementMenu
		{
			public TestHelperMessageManagementMenuForCellSuspend(MultiMessageManager manager) : base(manager)
			{
			}

			protected override bool SendMessagesClickCore(object sender)
			{
				var mainMenu = GetMainMenu();
				var form = mainMenu != null ? mainMenu.GetForm() as TestHelperFormForCellSuspend : null;
				AssertNotNull(form);
				AssertEquals(1, form.GetGridSuspendCellNotificationCount());
				return base.SendMessagesClickCore(sender);
			}
		}

		sealed class TestHelperFormForCellSuspend : ZForm
		{
			public TestHelperFormForCellSuspend(object dataSource) : base(dataSource)
			{
			}

			ZGrid grid1;
			protected override void InitializeComponent()
			{
				grid1 = new ZGrid();
				Controls.Add(grid1);
				base.InitializeComponent();
			}

			public int? GetGridSuspendCellNotificationCount()
			{
				FieldInfo fi = typeof(ZGrid).GetField("suspendCellNotificationCount", BindingFlags.NonPublic | BindingFlags.Instance);
				return (int?)(fi.GetValue(grid1));
			}
		}
	}
}
