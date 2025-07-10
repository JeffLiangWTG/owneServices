using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AmendmentManagerCollectorTestCase : TestCaseWithFactory
	{
		public void TestDoActionCore()
		{
			AmendmentManagerCollectorForTest collector = new AmendmentManagerCollectorForTest(new SingleMessageManager[] { SingleManagerWithChanges }, false);
			collector.DoActionCore();
			AssertEquals(1, collector.GeneratedMessages.Length);
			AssertEquals(0, SingleManagerWithChanges.OriginalSent);
			AssertEquals(0, SingleManagerWithChanges.WithdrawalSent);
			AssertEquals(1, SingleManagerWithChanges.AmendmentSent);
		}

		public void TestGetApplicableManagersWithShouldSendTrue()
		{
			AmendmentManagerCollectorForTest collector = new AmendmentManagerCollectorForTest(new SingleMessageManager[] { SingleManagerWithChanges }, false);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(singleManagerWithChanges, managers[0]);
		}

		public void TestGetApplicableManagersWithShouldSendFalse()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			AmendmentManagerCollectorForTest collector = new AmendmentManagerCollectorForTest(new SingleMessageManager[] { manager }, false);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(0, managers.Length);
		}

		public void TestGetApplicableManagersWithAmendAllTrue()
		{
			TestHelperSingleMessageManager manager = new TestHelperSingleMessageManager();
			AmendmentManagerCollectorForTest collector = new AmendmentManagerCollectorForTest(new SingleMessageManager[] { manager }, true);
			SingleMessageManager[] managers = collector.GetApplicableManagers();
			AssertEquals(1, managers.Length);
			manager.AdditionalAmendmentNotifications.AddWarning("Amendment notification");
			MessageSendingNotificationCollection notifications = collector.GetNotifications();
			ZString expectedNotifications = @"Name
Amendment notification
";
			AssertEquals("Notifications", expectedNotifications, notifications.NotificationsAsString());
		}

		TestHelperSingleMessageManager singleManagerWithChanges;
		TestHelperSingleMessageManager SingleManagerWithChanges
		{
			get
			{
				if (singleManagerWithChanges == null)
				{
					singleManagerWithChanges = new TestHelperSingleMessageManager();
					singleManagerWithChanges.BusinessObject.Factory.Save();
					singleManagerWithChanges.canSendWithdrawal = true;
					singleManagerWithChanges.ReturnDifferentOriginalMessages = true;
				}
				return singleManagerWithChanges;
			}
		}
	}

	class AmendmentManagerCollectorForTest : AmendmentManagerCollector
	{
		public AmendmentManagerCollectorForTest(SingleMessageManager[] managersToCheck, bool amendAll) : base(managersToCheck, amendAll)
		{
		}

		new internal void DoActionCore() => base.DoActionCore();
		new internal SingleMessageManager[] GetApplicableManagers() => base.GetApplicableManagers();
		new internal MessageSendingNotificationCollection GetNotifications() => base.GetNotifications();
	}
}
