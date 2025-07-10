using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.Freight.LocalCartage.Business.GPS.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(GPSEventAllocationCollection))]
	public class GPSEventAllocationCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<GPSEventAllocationCollection>
	{
		public void TestGPSEventAllocationCollectionConstructor()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", ZDateTime.Now);
			var cartageLeg = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var cartageLegAllocationManager = new CartageLegAllocationManager(cartageLeg);
			var gpsEvent1 = new GPSEvent();
			var gpsEvent2 = new GPSEvent();
			var gpsEvents = new GPSEvent[] { gpsEvent1, gpsEvent2 };
			var gpsEventAllocations = new GPSEventAllocationCollection(gpsEvents, cartageLegAllocationManager);
			AssertEquals(2, gpsEventAllocations.Count);
			AssertEquals("gpsEvent1", gpsEvent1, gpsEventAllocations[0].GPSEvent);
			AssertEquals("gpsEvent2", gpsEvent2, gpsEventAllocations[1].GPSEvent);
		}

		protected override GPSEventAllocationCollection GetCollectionToTest()
		{
			return new GPSEventAllocationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GPSEventAllocation(Factory);
		}
	}
}
