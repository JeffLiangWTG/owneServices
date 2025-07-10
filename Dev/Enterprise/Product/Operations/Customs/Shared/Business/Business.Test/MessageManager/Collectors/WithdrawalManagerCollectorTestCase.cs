using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WithdrawalManagerCollectorTestCase : TestCaseWithFactory
	{
		public void TestDoActionCore()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			WithdrawalManagerCollectorForTest collector = new WithdrawalManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			collector.DoActionCore();
			AssertEquals(1, collector.GeneratedMessages.Length);
			AssertEquals(0, manager.OriginalSent);
			AssertEquals(1, manager.WithdrawalSent);
			AssertEquals(0, manager.AmendmentSent);
		}

		public void TestGetApplicableManagersWithSendAllTrue()
		{
			SingleMessageManager manager = new TestHelperSingleMessageManager();
			WithdrawalManagerCollectorForTest collector = new WithdrawalManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(manager, managers[0]);
			((TestHelperSingleMessageManager)manager).AdditionalWithdrawalNotifications.AddWarning("Withdrawl notification");
			MessageSendingNotificationCollection notifications = collector.GetNotifications();
			ZString expectedNotifications = @"Name
Withdrawl notification
";
			AssertEquals("Notifications", expectedNotifications, notifications.NotificationsAsString());
		}

		public void TestGetApplicableManagersWithSendAllFalseShouldSendTrue()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			manager.shouldSendWithdrawalOnSave = true;
			WithdrawalManagerCollectorForTest collector = new WithdrawalManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(manager, managers[0]);
		}

		public void TestGetApplicableManagersWithSendAllFalseShouldSendFalse()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			manager.shouldSendWithdrawalOnSave = false;
			WithdrawalManagerCollectorForTest collector = new WithdrawalManagerCollectorForTest(new SingleMessageManager[] { manager }, false);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(0, managers.Length);
		}
	}

	class WithdrawalManagerCollectorForTest : WithdrawalManagerCollector
	{
		public WithdrawalManagerCollectorForTest(SingleMessageManager[] managersToCheck, bool sendAll) : base(managersToCheck, sendAll)
		{
		}

		new internal void DoActionCore() => base.DoActionCore();
		new internal SingleMessageManager[] GetApplicableManagers() => base.GetApplicableManagers();
		new internal MessageSendingNotificationCollection GetNotifications() => base.GetNotifications();
	}
}
