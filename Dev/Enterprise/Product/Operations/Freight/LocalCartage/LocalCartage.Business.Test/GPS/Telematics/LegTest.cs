using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	public class LegTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var cartageLeg1 = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var cartageLeg2 = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			cartageLeg1.JU_RunSheetSequence = 1;
			cartageLeg2.JU_RunSheetSequence = 3;
			var leg1 = new Leg(cartageLeg1);
			var leg2 = new Leg(cartageLeg2);
			AssertEquals("Leg 1 Pickup", true, IsSameLatLong(leg1.Pickup, OrgA.MainAddress));
			AssertEquals("Leg 1 WaitPoint, none so should be null.", false, leg1.WaitPoint.HasValidAddress);
			AssertEquals("Leg 1 Delivery", true, IsSameLatLong(leg1.Delivery, OrgB.MainAddress));
			AssertEquals("Leg 1 Sequence", 1, leg1.Sequence);
			AssertEquals("Leg 2 Pickup", true, IsSameLatLong(leg2.Pickup, OrgA.MainAddress));
			AssertEquals("Leg 2 WaitPoint", true, IsSameLatLong(leg2.WaitPoint, OrgB.MainAddress));
			AssertEquals("Leg 2 Delivery", true, IsSameLatLong(leg2.Delivery, OrgC.MainAddress));
			AssertEquals("Leg 2 Sequence", 3, leg2.Sequence);
		}

		bool IsSameLatLong(LegSegment segment, OrgAddress address)
		{
			return (ZGeography.NormalizeLatitudeDegree(segment.Latitude) == ZGeography.NormalizeLatitudeDegree(address.Latitude) && ZGeography.NormalizeLongitudeDegree(segment.Longitude) == ZGeography.NormalizeLongitudeDegree(address.Longitude));
		}

		public void TestOrderedGeofenceAddresses()
		{
			var cartageLeg1 = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, null, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg1 = new Leg(cartageLeg1);
			var cartageLeg2 = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, null, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = new Leg(cartageLeg2);
			var cartageLeg3 = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg3 = new Leg(cartageLeg3);
			AssertGeoFenceAddresses(leg1, 1, OrgA.MainAddress);
			AssertGeoFenceAddresses(leg2, 2, OrgA.MainAddress, OrgB.MainAddress);
			AssertGeoFenceAddresses(leg3, 3, OrgA.MainAddress, OrgB.MainAddress, OrgC.MainAddress);
		}

		void AssertGeoFenceAddresses(Leg leg, int count, params OrgAddress[] addresses)
		{
			AssertEquals(count, leg.OrderedGeofenceAddresses().Count());
			var geofenceAddress = leg.OrderedGeofenceAddresses();
			for (int i = 0; i < addresses.Length; i++)
			{
				AssertEquals(true, IsSameLatLong(geofenceAddress.ElementAt(i), addresses[i]));
			}
		}

		public void IsComplete_PickupDelivery()
		{
			var cartageLegPickupDelivery = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var legPickupDelivery = new Leg(cartageLegPickupDelivery);
			AssertEquals("No times entered, should be incomplete.", false, legPickupDelivery.IsComplete);
			PopulateLegTimes(cartageLegPickupDelivery, includingWaitPoint: false);
			AssertEquals("All times entered, should be complete.", true, legPickupDelivery.IsComplete);
			cartageLegPickupDelivery.JU_PickupTimeIn = ZDateTime.Empty;
			AssertEquals("No Pickup In Time, should not be complete.", false, legPickupDelivery.IsComplete);
			cartageLegPickupDelivery.JU_PickupTimeIn = EventTime.AddMinutes(1);
			cartageLegPickupDelivery.JU_PickupTimeOut = ZDateTime.Empty;
			AssertEquals("No Pickup Out Time, should not be complete.", false, legPickupDelivery.IsComplete);
			cartageLegPickupDelivery.JU_PickupTimeOut = EventTime.AddMinutes(2);
			cartageLegPickupDelivery.JU_DeliverTimeIn = ZDateTime.Empty;
			AssertEquals("No Delivery In Time, should not be complete.", false, legPickupDelivery.IsComplete);
			cartageLegPickupDelivery.JU_DeliverTimeIn = EventTime.AddMinutes(3);
			cartageLegPickupDelivery.JU_DeliverTimeOut = ZDateTime.Empty;
			AssertEquals("No Delivery Out Time, should not be complete.", false, legPickupDelivery.IsComplete);
		}

		public void IsComplete_PickupWaitPointDelivery()
		{
			var cartageLegPickupWaitPointDelivery = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var legPickupWaitPointDelivery = new Leg(cartageLegPickupWaitPointDelivery);
			AssertEquals("No times entered, should be incomplete.", false, legPickupWaitPointDelivery.IsComplete);
			PopulateLegTimes(cartageLegPickupWaitPointDelivery, includingWaitPoint: true);
			AssertEquals("All times entered, should be complete.", true, legPickupWaitPointDelivery.IsComplete);
			cartageLegPickupWaitPointDelivery.JU_PickupTimeIn = ZDateTime.Empty;
			AssertEquals("No Pickup In Time, should be not be complete.", false, legPickupWaitPointDelivery.IsComplete);
			cartageLegPickupWaitPointDelivery.JU_PickupTimeIn = EventTime.AddMinutes(1);
			cartageLegPickupWaitPointDelivery.JU_PickupTimeOut = ZDateTime.Empty;
			AssertEquals("No Pickup Out Time, should be not be complete.", false, legPickupWaitPointDelivery.IsComplete);
			cartageLegPickupWaitPointDelivery.JU_PickupTimeOut = EventTime.AddMinutes(2);
			cartageLegPickupWaitPointDelivery.JU_WaitPointTimeIn = ZDateTime.Empty;
			AssertEquals("No WaitPoint In Time, should not be complete.", false, legPickupWaitPointDelivery.IsComplete);
			cartageLegPickupWaitPointDelivery.JU_WaitPointTimeIn = EventTime.AddMinutes(3);
			cartageLegPickupWaitPointDelivery.JU_WaitPointTimeOut = ZDateTime.Empty;
			AssertEquals("No WaitPoint Out Time, should not be complete.", false, legPickupWaitPointDelivery.IsComplete);
			cartageLegPickupWaitPointDelivery.JU_WaitPointTimeOut = EventTime.AddMinutes(4);
			cartageLegPickupWaitPointDelivery.JU_DeliverTimeIn = ZDateTime.Empty;
			AssertEquals("No Delivery In Time, should not be complete.", false, legPickupWaitPointDelivery.IsComplete);
			cartageLegPickupWaitPointDelivery.JU_DeliverTimeIn = EventTime.AddMinutes(5);
			cartageLegPickupWaitPointDelivery.JU_DeliverTimeOut = ZDateTime.Empty;
			AssertEquals("No Delivery Out Time, should not be complete.", false, legPickupWaitPointDelivery.IsComplete);
		}

		void PopulateLegTimes(CommonCartageLeg leg, bool includingWaitPoint = false)
		{
			leg.JU_PickupTimeIn = EventTime.AddMinutes(1);
			leg.JU_PickupTimeOut = EventTime.AddMinutes(2);
			if (includingWaitPoint)
			{
				leg.JU_WaitPointTimeIn = EventTime.AddMinutes(3);
				leg.JU_WaitPointTimeOut = EventTime.AddMinutes(4);
			}

			leg.JU_DeliverTimeIn = EventTime.AddMinutes(5);
			leg.JU_DeliverTimeOut = EventTime.AddMinutes(6);
		}

		GPSTestHelper HelperGPS
		{
			get
			{
				return helperGPS ?? (helperGPS = new GPSTestHelper(Factory));
			}
		}

		GPSTestHelper helperGPS;

		OrgHeader OrgA;
		OrgHeader OrgB;
		OrgHeader OrgC;
		ZDateTime Today;
		ZDateTime EventTime;
		RefEquipment Truck1;
		CommonWorkSheet RunSheet1;
		ZDecimal Longitude;
		ZDecimal Latitude;
		protected override void SetUp()
		{
			base.SetUp();
			Longitude = 151.195135m;
			Latitude = -33.916295m;
			OrgA = HelperGPS.CreateOrgWithAddress("OA", "Address OA", Longitude + 10, Latitude + 10);
			OrgB = HelperGPS.CreateOrgWithAddress("OB", "Address OB", Longitude + 20, Latitude + 20);
			OrgC = HelperGPS.CreateOrgWithAddress("OC", "Address OC", Longitude + 30, Latitude + 30);
			EventTime = ZDateTime.Now;
			Today = ZDateTime.Today;
			Truck1 = Factory.NewWithValidTestData<RefEquipment>();
			RunSheet1 = HelperGPS.CreateRunSheet(Truck1, Today, Today.AddDays(1));
		}

		protected override void TearDown()
		{
			OrgA = null;
			OrgB = null;
			OrgC = null;
			Truck1 = null;
			RunSheet1 = null;
			base.TearDown();
		}
	}
}
