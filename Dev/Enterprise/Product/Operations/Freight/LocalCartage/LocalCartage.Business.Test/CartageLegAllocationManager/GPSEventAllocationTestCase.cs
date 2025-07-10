using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.Freight.LocalCartage.Business.GPS.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(GPSEventAllocation))]
	public class GPSEventAllocationTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestGPSEventAllocationConstructor()
		{
			var gpsEvent = new GPSEvent();
			var gpsEventAllocation = CreateGPSEventAllocation(gpsEvent);
			AssertEquals("GPSEvent", gpsEvent, gpsEventAllocation.GPSEvent);
			AssertEquals("Selected", false, gpsEventAllocation.Selected);
		}

		public void TestSelected()
		{
			var gpsEvent = new GPSEvent();
			var gpsEventAllocation = CreateGPSEventAllocation(gpsEvent);
			AssertEquals("Selected should be false", false, gpsEventAllocation.Selected);
			gpsEventAllocation.Selected = true;
			AssertEquals("Selected should be true", true, gpsEventAllocation.Selected);
		}

		GPSEventAllocation CreateGPSEventAllocation(GPSEvent gpsEvent)
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", ZDateTime.Now);
			var cartageLeg = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var cartageLegAllocationManager = new CartageLegAllocationManager(cartageLeg);
			return new GPSEventAllocation(gpsEvent, cartageLegAllocationManager);
		}
	}
}
