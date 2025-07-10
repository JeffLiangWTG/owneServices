using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatingLocationCollection))]
	public class RatingLocationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RatingLocationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(RefZoneHeader));
		}

		public void GetExtraNotificationsForZones()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "CC";
			zone.FZ_Description = "Zone A Desc";
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			zone.FZ_IsActive = true;
			Factory.Save();

			var collection = new RatingLocationCollection(Factory, true);
			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var notification = notificationProvider.GetExtraNotification(zone);
			AssertNull(notification);

			zone.FZ_IsActive = false;
			Factory.Save();
			notification = notificationProvider.GetExtraNotification(zone);
			AssertNotNull(notification);
			AssertContains("This Zone is inactive - it may not be used.", notification.Message);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			zone.FZ_IsActive = true;
			Factory.Save();

			notification = notificationProvider.GetExtraNotification(zone);
			AssertNotNull(notification);
			AssertNotContains("This Zone is inactive - it may not be used.", notification.Message);
			AssertContains("This Zone is not available for Rating purposes. The Zone type is Rates Service Ocean.", notification.Message);
		}

		public void GetExtraNotificationsForPorts()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "ARBOK";
			port.RL_IsActive = false;

			Factory.Save();
			var collection = new RatingLocationCollection(Factory, true);

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var notification = notificationProvider.GetExtraNotification(port);

			AssertNotNull(notification);
			AssertContains("This Port is inactive - it may not be used.", notification.Message);
		}

		public void GetExtraNotificationsForCountries()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "US";
			country.RN_Desc = "United States";
			country.RN_IsActive = false;

			Factory.Save();
			var collection = new RatingLocationCollection(Factory, true);

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var notification = notificationProvider.GetExtraNotification(country);

			AssertNotNull(notification);
			AssertContains("This Country is inactive - it may not be used.", notification.Message);
		}
	}
}
