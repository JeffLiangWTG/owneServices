using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.GPS.Business.Testing
{
	[TestedType(typeof(GPSSupporterActivityCollectionFilterProvider))]
	sealed class GPSSupporterActivityCollectionFilterProviderTests : NonPersistentBusinessObjectTestCase
	{
		#region Event type

		public void TestActivityTypeAll()
		{
			LoadCollection("ALL");
			AssertEquals("filter doesn't filter", 2, Collection.Count);
			foreach (GPSSupporterActivity act in Collection)
			{
				AssertEquals("incorrect activity", "CUS", act.EN_EventType);
			}
		}

		public void TestEventTypeMessage_ActivityTypeAllList()
		{
			foreach (CodeDescriptionPair pair in new GPSConstants.GPSNotificationEventList())
			{
				string code = pair.Code;
				LoadCollection(code);
				AssertEquals("filter doesn't filter", 1, Collection.Count);
				AssertEquals("didn't return correct activity", "CUS_" + code, Collection[0].EN_ActivityInformation);
			}
		}

		#endregion

		#region From-to date

		public void TestFromDate()
		{
			LoadCollection("ALL", ZDateTime.Today.AddMonths(-11), ZDateTime.Today.AddDays(1));
			AssertEquals("filter doesn't filter", 2, Collection.Count);
		}

		public void TestToDate()
		{
			LoadCollection("ALL", ZDateTime.Today, ZDateTime.Today.AddMonths(11));
			AssertEquals("filter doesn't filter", 2, Collection.Count);
		}

		public void TestFromDateToDate()
		{
			LoadCollection("ALL", ZDateTime.Today.AddMonths(-11), ZDateTime.Today.AddMonths(11));
			AssertEquals("filter doesn't filter", 2, Collection.Count);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CreateActivities();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "EQUIP";
			return new GPSSupporterActivityCollectionFilterProvider(new GPSSupporterActivityCollection(equipment));
		}

		void LoadCollection(ZString activityType)
		{
			Filter.FilterActivityType = activityType;
			Collection.Load();
		}

		void LoadCollection(ZString activityType, ZDateTime fromTime, ZDateTime toTime)
		{
			Filter.ActivityFilterDateFrom = fromTime;
			Filter.ActivityFilterDateTo = toTime;
			LoadCollection(activityType);
		}

		GPSSupporterActivityCollectionFilterProvider Filter
		{
			get
			{
				if (fFilter == null)
				{
					fFilter = new GPSSupporterActivityCollectionFilterProvider(Collection);
					fFilter.ActivityFilterDateFrom = ZDateTime.Today.AddDays(-1);
					fFilter.ActivityFilterDateTo = ZDateTime.Today.AddDays(1);
				}
				return fFilter;
			}
		}
		GPSSupporterActivityCollectionFilterProvider fFilter;

		GPSSupporterActivityCollection Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = new GPSSupporterActivityCollection(Vehicle);
				}
				return fCollection;
			}
		}
		GPSSupporterActivityCollection fCollection;

		void CreateActivities()
		{
			foreach (CodeDescriptionPair pair in new GPSConstants.GPSNotificationEventList())
			{
				var code = pair.Code;
				CreateActivity("CUS", code, "CUS_" + code);
			}
		}

		GPSSupporterActivity CreateActivity(ZString eventType, ZString activityType, ZString activityInfo)
		{
			var activity = Factory.NewWithValidTestData<GPSSupporterActivity>();
			activity.EN_EventType = eventType;
			activity.EN_ActivityType = activityType;
			activity.EN_ActivityInformation = activityInfo;
			activity.EN_RQ_Vehicle = Vehicle.PK;
			activity.EN_ActivityTime = ZDateTime.Now;
			Factory.Save();

			return activity;
		}

		RefEquipment fVehicle;
		RefEquipment Vehicle
		{
			get
			{
				if (fVehicle == null)
				{
					fVehicle = Factory.New<RefEquipment>();
					fVehicle.RQ_GeoProviderID = "189";
					fVehicle.RQ_GeoProviderType = "NAV";
					fVehicle.RQ_ShortCode = "RAKHSH";
				}
				return fVehicle;
			}
		}

		#endregion
	}
}
