using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatingRefContainerCollection))]
	public class RatingRefContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<RatingRefContainerCollection>
	{
		public void TestGetExtraNotifications()
		{
			var airContainer = Factory.NewWithValidTestData<RefContainer>();
			airContainer.RC_ShippingMode = "AIR";
			airContainer.RC_IsActive = true;

			var seaContainer = Factory.NewWithValidTestData<RefContainer>();
			seaContainer.RC_ShippingMode = "SEA";
			seaContainer.RC_IsActive = true;

			var roadContainer = Factory.NewWithValidTestData<RefContainer>();
			roadContainer.RC_ShippingMode = "ROA";
			roadContainer.RC_IsActive = true;

			Factory.Save();

			var seaFreightError = "This is a Sea Freight entry - please choose a Sea Freight FCL Container.";
			var collection = new RatingRefContainerCollection(Factory, "SEA");
			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var notification = notificationProvider.GetExtraNotification(airContainer);
			AssertContains(seaFreightError, notification.Message);
			AssertNotContains("This container is not active - it may not be used.", notification.Message);

			notification = notificationProvider.GetExtraNotification(seaContainer);
			AssertNull(notification);

			notification = notificationProvider.GetExtraNotification(roadContainer);
			AssertContains(seaFreightError, notification.Message);

			var roadFreightError = "This is a Road Freight entry - please choose a Road or Sea Freight Container.";
			collection = new RatingRefContainerCollection(Factory, "ROA");
			notificationProvider = collection;

			notification = notificationProvider.GetExtraNotification(airContainer);
			AssertContains(roadFreightError, notification.Message);

			notification = notificationProvider.GetExtraNotification(seaContainer);
			AssertNull(notification);

			notification = notificationProvider.GetExtraNotification(roadContainer);
			AssertNull(notification);

			var airFreightError = "This is an Air Freight entry - please choose an Air Freight ULD Container.";
			collection = new RatingRefContainerCollection(Factory, "AIR");
			notificationProvider = collection;
			notification = notificationProvider.GetExtraNotification(airContainer);
			AssertNull(notification);

			notification = notificationProvider.GetExtraNotification(seaContainer);
			AssertContains(airFreightError, notification.Message);

			notification = notificationProvider.GetExtraNotification(roadContainer);
			AssertContains(airFreightError, notification.Message);

			airContainer.RC_IsActive = false;
			Factory.Save();

			collection = new RatingRefContainerCollection(Factory, "AIR");
			notificationProvider = collection;
			notification = notificationProvider.GetExtraNotification(airContainer);
			AssertContains("This container is not active - it may not be used.", notification.Message);
		}
	}
}
