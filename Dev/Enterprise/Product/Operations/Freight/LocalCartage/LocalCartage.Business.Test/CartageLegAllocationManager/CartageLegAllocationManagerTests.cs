using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.Freight.LocalCartage.Business.GPS.Testing;
using Enterprise.GPS.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageLegAllocationManager))]
	public class CartageLegAllocationManagerTests : NonPersistentBusinessObjectTestCase
	{
		public void TestCartageLegAllocationManagerConstructor()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals(0, manager.GPSEventAllocations.Count);
			AssertEquals(cartageLeg1, manager.CartageLeg);
		}

		public void TestGPSEventAllocations()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var cartageLeg2 = data.CreateAndAssignLeg(helper.Org2.MainAddress, helper.Org1.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.DeliveryAddressCode, Now.AddHours(3), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.DeliveryAddressCode, Now.AddHours(4), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			var gpsClientActivity4 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity4.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			var gpsClientActivity5 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddDays(-3), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity5.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			var gpsClientActivity6 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(5), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity6.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			gpsClientActivity6.EN_JU = cartageLeg2.PK;
			var gpsClientActivity7 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity7.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 6, manager.GPSEventAllocations.Count);
			var gpsEventAllocation1 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity1.PK));
			AssertNotNull("gpsEventAllocation1", gpsEventAllocation1);
			AssertEquals("gpsEventAllocation1 selected (Pickup In)", true, gpsEventAllocation1.Selected);
			var gpsEventAllocation2 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity2.PK));
			AssertNotNull("gpsEventAllocation2", gpsEventAllocation2);
			AssertEquals("gpsEventAllocation2 selected (Delivery In)", true, gpsEventAllocation2.Selected);
			var gpsEventAllocation3 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity3.PK));
			AssertNotNull("gpsEventAllocation3", gpsEventAllocation3);
			AssertEquals("gpsEventAllocation3 selected (Delivery Out)", true, gpsEventAllocation3.Selected);
			var gpsEventAllocation4 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity4.PK));
			AssertNotNull("gpsEventAllocation4", gpsEventAllocation4);
			AssertEquals("gpsEventAllocation4 selected (Pickup Out)", true, gpsEventAllocation4.Selected);
			AssertNull("gpsEventAllocation5 (Out of Run Sheet time)", manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity5.PK)));
			var gpsEventAllocation6 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity6.PK));
			AssertNotNull("gpsEventAllocation6", gpsEventAllocation6);
			AssertEquals("gpsEventAllocation6 not selected (Already assigned)", false, gpsEventAllocation6.Selected);
			var gpsEventAllocation7 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity7.PK));
			AssertNotNull("gpsEventAllocation7", gpsEventAllocation7);
			AssertEquals("gpsEventAllocation7 not selected (Second Pickup In -> Invalid)", false, gpsEventAllocation7.Selected);
		}

		public void TestGPSEventAllocations_WithMissingEvent()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var cartageLeg2 = data.CreateAndAssignLeg(helper.Org2.MainAddress, helper.Org1.MainAddress);
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.DeliveryAddressCode, Now.AddHours(3), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity5 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddDays(-3), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity5.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			var gpsClientActivity6 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(5), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity6.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			gpsClientActivity6.EN_JU = cartageLeg2.PK;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 2, manager.GPSEventAllocations.Count);
			var gpsEventAllocation2 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity2.PK));
			AssertNotNull("gpsEventAllocation2", gpsEventAllocation2);
			AssertEquals("gpsEventAllocation2 selected (Delivery In)", true, gpsEventAllocation2.Selected);
			AssertNull("gpsEventAllocation5 (Out of Run Sheet time)", manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity5.PK)));
			var gpsEventAllocation6 = manager.GPSEventAllocations.Cast<GPSEventAllocation>().FirstOrDefault(g => g.GPSEvent.EventPK.Equals(gpsClientActivity6.PK));
			AssertNotNull("gpsEventAllocation6", gpsEventAllocation6);
			AssertEquals("gpsEventAllocation6 not selected (Already assigned)", false, gpsEventAllocation6.Selected);
		}

		public void TestAllocateAndSetTimeAsSelected()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			var result = manager.AllocateAndSetTimeAsSelected();
			AssertEquals("Should be valid", true, result);
			AssertEquals("activity 1 should be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 should be valid", false, manager.GPSEventAllocations[1].HasErrors);
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", Now, cartageLeg1.JU_PickupTimeIn);
			AssertEquals("Pickup in activity", cartageLeg1.PK, gpsClientActivity1.EN_JU);
			AssertEquals("Pickup out", Now.AddHours(2), cartageLeg1.JU_PickupTimeOut);
			AssertEquals("Pickup out activity", cartageLeg1.PK, gpsClientActivity3.EN_JU);
		}

		public void TestAllocateAndSetTimeAsSelected_Invalid()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			foreach (GPSEventAllocation gpsEventAllocation in manager.GPSEventAllocations)
			{
				gpsEventAllocation.Selected = true;
			}

			var result = manager.AllocateAndSetTimeAsSelected();
			AssertEquals("Should be invalid", false, result);
			AssertEquals("activity 1 should be in error", true, manager.GPSEventAllocations[0].HasErrors);
			AssertHasError(manager.GPSEventAllocations[0].SelectedInfo, "Too many events have been selected for Pickup - In. Please select only one In event per Pickup.");
			AssertEquals("activity 2 should be in error", true, manager.GPSEventAllocations[1].HasErrors);
			AssertHasError(manager.GPSEventAllocations[1].SelectedInfo, "Too many events have been selected for Pickup - In. Please select only one In event per Pickup.");
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
		}

		public void TestSelectedGPSEventsForLegSegmentsNullRef()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var manager = new CartageLegAllocationManager(cartageLeg1);
			var gpsEvts = manager.GPSEventAllocations;
			gpsEvts.RemoveAll();
			var gps1 = new GPSEventAllocation(Factory);
			gps1.Selected = true;
			gpsEvts.Add(gps1);
			var gps2 = new GPSEventAllocation(Factory);
			gps2.Selected = true;
			gpsEvts.Add(gps2);
			var gps3 = new GPSEventAllocation(Factory);
			gps3.Selected = true;
			gpsEvts.Add(gps3);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			GPSEvent pickupInGpsEvent = null;
			GPSEvent pickupOutGpsEvent = null;
			GPSEvent waitingPointInGpsEvent = null;
			GPSEvent waitingPointOutGpsEvent = null;
			GPSEvent deliveryInGpsEvent = null;
			GPSEvent deliveryOutGpsEvent = null;
			AssertNoExceptionThrown(delegate()
			{
				manager.SelectedGPSEventsForLegSegments(out pickupInGpsEvent, out pickupOutGpsEvent, out waitingPointInGpsEvent, out waitingPointOutGpsEvent, out deliveryInGpsEvent, out deliveryOutGpsEvent);
			});
		}

		public void TestValidateAllSelectedGpsEvent()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			manager.SetupAllSelectedGpsEvent();
			AssertEquals("activity 1 should be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 should be valid", false, manager.GPSEventAllocations[1].HasErrors);
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", Now, manager.PickupSelectedIn);
			AssertEquals("Pickup out", Now.AddHours(2), manager.PickupSelectedOut);
		}

		public void TestValidateAllSelectedGpsEvent_Invalid()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			var gpsClientActivity1 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now, GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity1.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity2 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(1), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity2.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			var gpsClientActivity3 = helper.CreateActivity(cartageLeg1.PickupAddressCode, Now.AddHours(2), GPSConstants.GPSEventTypeList.Codes.Custom, data.Vehicle.PK);
			gpsClientActivity3.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GOT;
			Factory.Save();
			var manager = new CartageLegAllocationManager(cartageLeg1);
			AssertEquals("GPSEventAllocations Count", 3, manager.GPSEventAllocations.Count);
			foreach (GPSEventAllocation gpsEventAllocation in manager.GPSEventAllocations)
			{
				gpsEventAllocation.Selected = true;
			}

			manager.SetupAllSelectedGpsEvent();
			AssertEquals("activity 1 should be in error", true, manager.GPSEventAllocations[0].HasErrors);
			AssertHasError(manager.GPSEventAllocations[0].SelectedInfo, "Too many events have been selected for Pickup - In. Please select only one In event per Pickup.");
			AssertEquals("activity 2 should be in error", true, manager.GPSEventAllocations[1].HasErrors);
			AssertHasError(manager.GPSEventAllocations[1].SelectedInfo, "Too many events have been selected for Pickup - In. Please select only one In event per Pickup.");
			AssertEquals("activity 3 should be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", ZDateTime.Empty, manager.PickupSelectedIn);
			AssertEquals("Pickup out", Now.AddHours(2), manager.PickupSelectedOut);
			manager.GPSEventAllocations[1].Selected = false;
			manager.SetupAllSelectedGpsEvent();
			AssertEquals("activity 1 sould be valid", false, manager.GPSEventAllocations[0].HasErrors);
			AssertEquals("activity 2 sould be valid", false, manager.GPSEventAllocations[1].HasErrors);
			AssertEquals("activity 3 sould be valid", false, manager.GPSEventAllocations[2].HasErrors);
			AssertEquals("Pickup in time", Now, manager.PickupSelectedIn);
			AssertEquals("Pickup out", Now.AddHours(2), manager.PickupSelectedOut);
		}

		readonly ZDateTime Now = ZDateTime.Now;

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new GPSTestData(this.Factory);
			var helper = new GPSTestHelper(this.Factory);
			data.CreateVehicleAndRunSheet("TruckA", Now.AddDays(-1));
			data.RunSheet.EY_EndTime = Now.AddDays(1);
			var cartageLeg1 = data.CreateAndAssignLeg(helper.Org1.MainAddress, helper.Org2.MainAddress);
			return new CartageLegAllocationManager(cartageLeg1);
		}
	}
}
