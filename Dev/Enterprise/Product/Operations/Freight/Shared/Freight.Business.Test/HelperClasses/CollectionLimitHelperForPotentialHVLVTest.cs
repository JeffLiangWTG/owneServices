using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class CollectionLimitHelperForPotentialHVLVTest<TParent> : TestCaseWithFactory where TParent : BusinessObject
	{
		protected abstract IDisposable SetLimit(int limit);
		protected abstract IDisposable SetLimitIntroductionTime(DateTime time);
		protected abstract TParent GetNewParentBizO();
		protected abstract CollectionLimitHelperForPotentialHVLV GetNewHelper(TParent bizO);
		protected abstract void AddNewCollectionElement(TParent bizO);
		protected abstract string NotificationForLimitOfFour { get; }

		public void TestNotification()
		{
			using (SetLimit(4))
			{
				var futureDate = DateTime.Now.AddDays(1);
				var pastDate = DateTime.Now.AddDays(-1);

				var bizO = GetNewParentBizO();
				var helper = GetNewHelper(bizO);

				AddNewCollectionElement(bizO);
				AssertNoNotification(helper, futureDate);
				AssertNoNotification(helper, pastDate);

				AddNewCollectionElement(bizO);
				AssertNoNotification(helper, futureDate);
				AssertNoNotification(helper, pastDate);

				AddNewCollectionElement(bizO);
				AssertNoNotification(helper, futureDate);
				AssertNotification(helper, NotificationType.Warning, pastDate);

				AddNewCollectionElement(bizO);
				AssertNoNotification(helper, futureDate);
				AssertNotification(helper, NotificationType.Warning, pastDate);

				AddNewCollectionElement(bizO);
				AssertNoNotification(helper, futureDate);
				AssertNotification(helper, NotificationType.Error, pastDate);
			}
		}

		void AssertNoNotification(CollectionLimitHelperForPotentialHVLV helper, DateTime limitIntroduction)
		{
			using (SetLimitIntroductionTime(limitIntroduction))
			{
				AssertNull(helper.CreateNotification());
			}
		}

		void AssertNotification(CollectionLimitHelperForPotentialHVLV helper, INotificationType notificationType, DateTime limitIntroduction)
		{
			using (SetLimitIntroductionTime(limitIntroduction))
			{
				var notification = helper.CreateNotification();
				AssertEquals(notificationType, notification.Type);
				AssertEquals(NotificationForLimitOfFour, notification.Message);
			}
		}
	}
}
