using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class OriginalManagerCollectorTestCase : TestCaseWithFactory
	{
		public void TestGetApplicableManagersWithSendAllTrue()
		{
			SingleMessageManager manager = new TestHelperSingleMessageManager();
			OriginalManagerCollectorForTest collector = new OriginalManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(manager, managers[0]);
			((TestHelperSingleMessageManager)manager).AdditionalOriginalNotifications.AddWarning("Original notification");
			MessageSendingNotificationCollection notifications = collector.GetNotifications();
			ZString expectedNotifications = @"Name
Original notification
";
			AssertEquals("Notifications", expectedNotifications, notifications.NotificationsAsString());
		}

		public void TestGetApplicableManagersWithSendAllFalseShouldSendTrue()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			manager.shouldSendOriginalOnSave = true;
			OriginalManagerCollectorForTest collector = new OriginalManagerCollectorForTest(new SingleMessageManager[] { manager }, false);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(manager, managers[0]);
		}

		public void TestGetApplicableManagersWithSendAllFalseShouldSendFalse()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			manager.shouldSendOriginalOnSave = false;
			OriginalManagerCollectorForTest collector = new OriginalManagerCollectorForTest(new SingleMessageManager[] { manager }, false);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(0, managers.Length);
		}

		public void TestDoActionCore()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			OriginalManagerCollectorForTest collector = new OriginalManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			collector.DoActionCore();
			AssertEquals(1, collector.GeneratedMessages.Length);
			AssertEquals(1, manager.OriginalSent);
			AssertEquals(0, manager.WithdrawalSent);
			AssertEquals(0, manager.AmendmentSent);
		}

		public void TestDoActionCoreWithPreventSendOn()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			OriginalManagerCollectorForTest collector = new OriginalManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			manager.PreventSendTestBool = true;
			collector.DoActionCore();
			AssertEquals(0, collector.GeneratedMessages.Length);
			AssertEquals(0, manager.OriginalSent);
			AssertEquals(0, manager.WithdrawalSent);
			AssertEquals(0, manager.AmendmentSent);
			manager.PreventSendTestBool = false;
			collector.DoActionCore();
			AssertEquals(1, collector.GeneratedMessages.Length);
			AssertEquals(1, manager.OriginalSent);
			AssertEquals(0, manager.WithdrawalSent);
			AssertEquals(0, manager.AmendmentSent);
		}
	}

	class OriginalManagerCollectorForTest : OriginalManagerCollector
	{
		public OriginalManagerCollectorForTest(SingleMessageManager[] managersToCheck, bool sendAll) : base(managersToCheck, sendAll)
		{
		}

		new internal void DoActionCore() => base.DoActionCore();
		new internal SingleMessageManager[] GetApplicableManagers() => base.GetApplicableManagers();
		new internal MessageSendingNotificationCollection GetNotifications() => base.GetNotifications();
	}
}
