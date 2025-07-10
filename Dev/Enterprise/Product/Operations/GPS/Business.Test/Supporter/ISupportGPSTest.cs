using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.GPS.Testing
{
	sealed class ISupportGPSTest : TestCaseWithFactory
	{
		#region ISupportGPS

		public void TestImplementsISupportGPS()
		{
			Assert("Should implement ISupportGPS", GPSEquipment is ISupportGPS);
		}

		public void TestIsVehicleAblesGEO()
		{
			Equipment.RQ_IsVehicle = !Equipment.RQ_IsVehicle;
			AssertEquals("RQ_IsVehicle should able GEO provider properties", !Equipment.RQ_IsVehicle, Equipment.RQ_GeoProviderIDInfo.ReadOnly);
			AssertEquals("RQ_IsVehicle should able GEO provider properties", !Equipment.RQ_IsVehicle, Equipment.RQ_GeoProviderTypeInfo.ReadOnly);
			Equipment.RQ_IsVehicle = !Equipment.RQ_IsVehicle;
			AssertEquals("RQ_IsVehicle should able GEO provider properties", !Equipment.RQ_IsVehicle, Equipment.RQ_GeoProviderIDInfo.ReadOnly);
			AssertEquals("RQ_IsVehicle should able GEO provider properties", !Equipment.RQ_IsVehicle, Equipment.RQ_GeoProviderTypeInfo.ReadOnly);
		}

		public void TestISupportGPSActivities()
		{
			var activity1 = GPSEquipment.Activities.AddNew();
			var activity2 = GPSEquipment.Activities.AddNew();
			activity1.EN_Latitude = ZDecimal.Parse("20");
			activity2.EN_Latitude = ZDecimal.Parse("30");
			activity1.EN_EventType = "CUS";
			activity2.EN_EventType = "CUS";
			activity1.EN_ActivityTime = ZDateTime.Now;
			activity2.EN_ActivityTime = ZDateTime.Now;
			Factory.Save();
			var col = new GPSSupporterActivityCollection(Equipment);

			var filter = new GPSSupporterActivityCollectionFilterProvider(col);
			filter.ActivityFilterDateFrom = ZDateTime.Today.AddDays(-1);
			filter.ActivityFilterDateTo = ZDateTime.Today.AddDays(1);

			col.Load();
			AssertEquals(2, col.Count);
			AssertEquals(activity1, col[0]);
			AssertEquals(activity2, col[1]);
		}

		public void TestDeleteRemovesActivityCollection()
		{
			GPSEquipment.Activities.AddNew();
			GPSEquipment.Activities.AddNew();
			Factory.Save();
			Equipment.Delete();
			AssertEquals("should delete all activities", 0, GPSEquipment.Activities.Count);
		}

		#endregion

		RefEquipment Equipment
		{
			get
			{
				if (equipment == null)
				{
					equipment = Factory.New<RefEquipment>();
					equipment.RQ_ShortCode = "SHORTY";
				}

				return equipment;
			}
		}
		RefEquipment equipment;

		GPSSupporter GPSEquipment
		{
			get { return fGPSEquipment ?? (fGPSEquipment = new GPSSupporter(Equipment)); }
		}
		GPSSupporter fGPSEquipment;
	}
}
