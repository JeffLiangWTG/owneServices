using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(TransitWarehouseLocationCollection))]
	class TransitWarehouseLocationCollectionTestCase : BusinessObjectCollectionTestCase
	{
		public void TestIFilterModuleExtraNotificationProvider()
		{
			var zoneRating = CreateRefZoneHeader("TST1", "TST1 Zone", ZoneTypeCodeDescriptionPair.Rating.Code);
			var zoneTransitWarehouse = CreateRefZoneHeader("TST2", "TST2 Zone", ZoneTypeCodeDescriptionPair.TransitWarehouse.Code);

			var collection = new TransitWarehouseLocationCollection(Factory);

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var notification = notificationProvider.GetExtraNotification(zoneRating);
			AssertEquals("This zone is not for Transit Warehouse, should have error notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
			AssertEquals("Notification message", "This Zone cannot be chosen here as it is not for Transit Warehouse.", notification.Message);

			notification = notificationProvider.GetExtraNotification(zoneTransitWarehouse);
			AssertNull("This zone is for Transit Warehouse, notification should be null", notification);
		}

		RefZoneHeader CreateRefZoneHeader(string code, string description, string type)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = code;
			zone.FZ_Description = description;
			zone.FZ_ZoneType = type;

			return zone;
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransitWarehouseLocationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(RefUNLOCO));
		}

		#endregion
	}
}
