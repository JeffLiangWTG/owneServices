using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(GPSSupporterActivityTestDataCollection))]
	class GPSClientActivityTestDataCollectionTest : BusinessObjectCollectionTestCase
	{
		[TestDate(2016, 03, 22, 11, 11, 11, 11)]
		public void TestSetDefaultsForNewChild()
		{
			var truck = Factory.New<RefEquipment>();
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.EY_RQ_Truck = truck.PK;
			var collection = new GPSSupporterActivityTestDataCollection(workSheet);
			var newActivity = collection.AddNew();
			AssertEquals("Vehicle PK should have set.", newActivity.EN_RQ_Vehicle, truck.PK);
			AssertEquals("EventType should have set.", newActivity.EN_EventType, GPSConstants.GPSEventTypeList.Codes.Custom);
			AssertEquals("ActivityID should have set", "6359424187", newActivity.EN_ActivityID);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			return new GPSSupporterActivityTestDataCollection(workSheet);
		}
	}
}
