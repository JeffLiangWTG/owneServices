using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.GPS.Testing;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class GPSEventAllocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSelected()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			var now = ZDateTime.Now;
			data.CreateVehicleAndRunSheet("TruckA", now.AddDays(-1));
			data.RunSheet.EY_EndTime = now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.PickupAddressCode, now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.PickupAddressCode, now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			AssertEquals("activity 1 should be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 should be valid", false, manager.GPSEventAllocations[1].HasErrors);
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", now, manager.PickupSelectedIn);
			AssertEquals("Pickup out", now.AddHours(2), manager.PickupSelectedOut);
		}

		public void TestValidateAllSelectedGpsEvent_Invalid()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			var now = ZDateTime.Now;
			data.CreateVehicleAndRunSheet("TruckA", now.AddDays(-1));
			data.RunSheet.EY_EndTime = now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.PickupAddressCode, now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.PickupAddressCode, now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			foreach (GPSEventAllocation gpsEventAllocation in manager.GPSEventAllocations)
			{
				gpsEventAllocation.Selected = true;
			}

			AssertEquals("activity 1 should be in error", true, manager.GPSEventAllocations[0].HasErrors);
			AssertHasError(manager.GPSEventAllocations[0].SelectedInfo, "Too many events have been selected for Pickup - In. Please select only one In event per Pickup.");
			AssertEquals("activity 2 should be in error", true, manager.GPSEventAllocations[1].HasErrors);
			AssertHasError(manager.GPSEventAllocations[1].SelectedInfo, "Too many events have been selected for Pickup - In. Please select only one In event per Pickup.");
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", ZDateTime.Empty, manager.PickupSelectedIn);
			AssertEquals("Pickup out", now.AddHours(2), manager.PickupSelectedOut);
			manager.GPSEventAllocations[1].Selected = false;
			AssertEquals("activity 1 should be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 should be valid", false, manager.GPSEventAllocations[1].HasErrors);
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", now, manager.PickupSelectedIn);
			AssertEquals("Pickup out", now.AddHours(2), manager.PickupSelectedOut);
		}

		public void TestValidateAllSelectedGpsEvent_Invalid_DeliveryOut()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			var now = ZDateTime.Now;
			data.CreateVehicleAndRunSheet("TruckA", now.AddDays(-1));
			data.RunSheet.EY_EndTime = now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.DeliveryAddressCode, now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.DeliveryAddressCode, now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.DeliveryAddressCode, now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			foreach (GPSEventAllocation gpsEventAllocation in manager.GPSEventAllocations)
			{
				gpsEventAllocation.Selected = true;
			}

			AssertEquals("activity 1 should be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 should be in error", true, manager.GPSEventAllocations[1].HasErrors);
			AssertHasError(manager.GPSEventAllocations[1].SelectedInfo, "Too many events have been selected for Delivery - Out. Please select only one Out event per Delivery.");
			AssertEquals("activity 3 should be in error valid", true, manager.GPSEventAllocations[2].HasErrors);
			AssertHasError(manager.GPSEventAllocations[2].SelectedInfo, "Too many events have been selected for Delivery - Out. Please select only one Out event per Delivery.");
			AssertEquals("Delivery In time", now, manager.DeliverySelectedIn);
			AssertEquals("Delivery Out", ZDateTime.Empty, manager.DeliverySelectedOut);
			manager.GPSEventAllocations[1].Selected = false;
			AssertEquals("activity 1 should be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 should be valid", false, manager.GPSEventAllocations[1].HasErrors);
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Delivery In time", now, manager.DeliverySelectedIn);
			AssertEquals("Delivery out", now.AddHours(2), manager.DeliverySelectedOut);
		}
	}
}
