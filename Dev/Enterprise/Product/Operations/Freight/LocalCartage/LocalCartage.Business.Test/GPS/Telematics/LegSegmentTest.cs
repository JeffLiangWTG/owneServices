using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	public class LegSegmentTest : TestCaseWithFactory
	{
		public void TestHasNoTimeIn()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(true, leg.Pickup.HasNoTimeIn);
			cartageLeg.JU_PickupTimeIn = EventTime.AddMinutes(1);
			AssertEquals(false, leg.Pickup.HasNoTimeIn);
			cartageLeg.JU_PickupTimeOut = EventTime.AddMinutes(1);
			AssertEquals(false, leg.Pickup.HasNoTimeIn);
			cartageLeg.JU_PickupTimeIn = ZDateTime.Empty;
			AssertEquals(true, leg.Pickup.HasNoTimeIn);
		}

		public void TestHasTimeInOnly()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(false, leg.Pickup.HasTimeInOnly);
			cartageLeg.JU_PickupTimeIn = EventTime.AddMinutes(1);
			AssertEquals(true, leg.Pickup.HasTimeInOnly);
			cartageLeg.JU_PickupTimeIn = ZDateTime.Empty;
			cartageLeg.JU_PickupTimeOut = EventTime.AddMinutes(1);
			AssertEquals(false, leg.Pickup.HasTimeInOnly);
			cartageLeg.JU_PickupTimeIn = EventTime.AddMinutes(1);
			cartageLeg.JU_PickupTimeOut = EventTime.AddMinutes(2);
			AssertEquals(false, leg.Pickup.HasTimeInOnly);
		}

		public void TestHasTimeInAndOut()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(false, leg.Pickup.HasTimeInAndOut);
			cartageLeg.JU_PickupTimeIn = EventTime.AddMinutes(1);
			AssertEquals(false, leg.Pickup.HasTimeInAndOut);
			cartageLeg.JU_PickupTimeOut = EventTime.AddMinutes(2);
			AssertEquals(true, leg.Pickup.HasTimeInAndOut);
		}

		public void TestAddress()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			var geofenceAddress = leg.OrderedGeofenceAddresses();
			AssertEquals(true, IsSameLatLong(geofenceAddress.ElementAt(0), OrgA.MainAddress));
			AssertEquals(true, IsSameLatLong(geofenceAddress.ElementAt(1), OrgB.MainAddress));
			AssertEquals(true, IsSameLatLong(geofenceAddress.ElementAt(2), OrgC.MainAddress));
			AssertEquals(false, IsSameLatLong(geofenceAddress.ElementAt(0), OrgC.MainAddress));
			AssertEquals(false, IsSameLatLong(geofenceAddress.ElementAt(1), OrgA.MainAddress));
			AssertEquals(false, IsSameLatLong(geofenceAddress.ElementAt(2), OrgB.MainAddress));
		}

		bool IsSameLatLong(LegSegment segment, OrgAddress address)
		{
			return (ZGeography.NormalizeLatitudeDegree(segment.Latitude) == ZGeography.NormalizeLatitudeDegree(address.Latitude) && ZGeography.NormalizeLongitudeDegree(segment.Longitude) == ZGeography.NormalizeLongitudeDegree(address.Longitude));
		}

		public void TestAddress_OverridenAddress_ReturnsAddress()
		{
			var orgE = HelperGPS.CreateOrgWithAddress("OE", "Address OE", Longitude + 10, Latitude + 10);
			var orgF = HelperGPS.CreateOrgWithAddress("OF", "Address OF", Longitude + 20, Latitude + 20);
			var orgG = HelperGPS.CreateOrgWithAddress("OG", "Address OG", Longitude + 30, Latitude + 30);
			var cartageLeg3Addresses = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(orgE, orgF, orgG, RunSheet1, Today.AddHours(6), Truck1.PK);
			OverrideAddress(cartageLeg3Addresses.PickupFromDocAddress);
			OverrideAddress(cartageLeg3Addresses.WaitPointDocAddress);
			OverrideAddress(cartageLeg3Addresses.DeliverToDocAddress);
			var leg3Addresses = new Leg(cartageLeg3Addresses);
			var geofenceAddress3Addresses = leg3Addresses.OrderedGeofenceAddresses();
			AssertEquals(3, geofenceAddress3Addresses.Count());
			AssertEquals(true, geofenceAddress3Addresses.ElementAt(0).IsSameAddress(leg3Addresses.Pickup));
			AssertEquals(true, geofenceAddress3Addresses.ElementAt(1).IsSameAddress(leg3Addresses.WaitPoint));
			AssertEquals(true, geofenceAddress3Addresses.ElementAt(2).IsSameAddress(leg3Addresses.Delivery));
			var cartageLeg2Addresses = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(orgE, null, orgG, RunSheet1, Today.AddHours(6), Truck1.PK);
			OverrideAddress(cartageLeg2Addresses.PickupFromDocAddress);
			OverrideAddress(cartageLeg2Addresses.WaitPointDocAddress);
			OverrideAddress(cartageLeg2Addresses.DeliverToDocAddress);
			var leg2Addresses = new Leg(cartageLeg2Addresses);
			var geofenceAddress2Addresses = leg2Addresses.OrderedGeofenceAddresses();
			AssertEquals(2, geofenceAddress2Addresses.Count());
			AssertEquals(true, geofenceAddress2Addresses.ElementAt(0).IsSameAddress(leg2Addresses.Pickup));
			AssertEquals(true, geofenceAddress2Addresses.ElementAt(1).IsSameAddress(leg2Addresses.Delivery));
		}

		public void TestAddressCode()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			var geofenceAddress = leg.OrderedGeofenceAddresses();
			AssertEquals(OrgA.MainAddress.Header.OH_Code + GPSConstants.FenceSeperator + OrgA.MainAddress.OA_Code, geofenceAddress.ElementAt(0).AddressCode);
			AssertEquals(OrgB.MainAddress.Header.OH_Code + GPSConstants.FenceSeperator + OrgB.MainAddress.OA_Code, geofenceAddress.ElementAt(1).AddressCode);
			AssertEquals(OrgC.MainAddress.Header.OH_Code + GPSConstants.FenceSeperator + OrgC.MainAddress.OA_Code, geofenceAddress.ElementAt(2).AddressCode);
		}

		public void TestAddressCode_WithOverriddenAddresses()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			OverrideAddress(cartageLeg.WaitPointDocAddress);
			OverrideAddress(cartageLeg.DeliverToDocAddress);
			var leg = new Leg(cartageLeg);
			var geofenceAddress = leg.OrderedGeofenceAddresses();
			AssertEquals("OA PTY LTD - Address OA ST", geofenceAddress.ElementAt(0).AddressCode);
			AssertEquals("OB PTY LTD - Address OB ST", geofenceAddress.ElementAt(1).AddressCode);
			AssertEquals("OC PTY LTD - Address OC ST", geofenceAddress.ElementAt(2).AddressCode);
		}

		public void TestTimeInAndOut()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			cartageLeg.JU_PickupTimeIn = EventTime.AddMinutes(1);
			cartageLeg.JU_PickupTimeOut = EventTime.AddMinutes(2);
			cartageLeg.JU_WaitPointTimeIn = EventTime.AddMinutes(3);
			cartageLeg.JU_WaitPointTimeOut = EventTime.AddMinutes(4);
			cartageLeg.JU_DeliverTimeIn = EventTime.AddMinutes(5);
			cartageLeg.JU_DeliverTimeOut = EventTime.AddMinutes(6);
			var leg = new Leg(cartageLeg);
			AssertEquals(EventTime.AddMinutes(1), leg.Pickup.TimeIn);
			AssertEquals(EventTime.AddMinutes(2), leg.Pickup.TimeOut);
			AssertEquals(EventTime.AddMinutes(3), leg.WaitPoint.TimeIn);
			AssertEquals(EventTime.AddMinutes(4), leg.WaitPoint.TimeOut);
			AssertEquals(EventTime.AddMinutes(5), leg.Delivery.TimeIn);
			AssertEquals(EventTime.AddMinutes(6), leg.Delivery.TimeOut);
		}

		public void TestLatitude()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(ZGeography.NormalizeLatitudeDegree(OrgA.MainAddress.OA_Latitude), leg.Pickup.Latitude);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			var legWithOverridenAddress = new Leg(cartageLeg);
			AssertEquals(ZGeography.NormalizeLatitudeDegree(OrgA.MainAddress.OA_Latitude), legWithOverridenAddress.Pickup.Latitude);
			cartageLeg.PickupFromDocAddress.E2_Latitude = 123m;
			AssertEquals(ZGeography.NormalizeLatitudeDegree(123m), legWithOverridenAddress.Pickup.Latitude);
		}

		public void TestLongitude()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(OrgA.MainAddress.OA_Longitude, leg.Pickup.Longitude);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			var legWithOverridenAddress = new Leg(cartageLeg);
			AssertEquals(OrgA.MainAddress.OA_Longitude, legWithOverridenAddress.Pickup.Longitude);
			cartageLeg.PickupFromDocAddress.E2_Longitude = 123m;
			AssertEquals(123m, legWithOverridenAddress.Pickup.Longitude);
		}

		public void TestGeofencePolygon()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(OrgA.MainAddress.OA_GeofencePolygon, leg.Pickup.GeofencePolygon);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			var legWithOverridenAddress = new Leg(cartageLeg);
			AssertEquals(ZGeography.Empty, legWithOverridenAddress.Pickup.GeofencePolygon);
		}

		public void TestHasValidGeoLocation()
		{
			var orgE = HelperGPS.CreateOrgWithAddress("OE", "Address OE", Latitude, Longitude);
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(orgE, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(true, leg.Pickup.HasValidGeoLocation);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			cartageLeg.PickupFromDocAddress.E2_Latitude = 0;
			cartageLeg.PickupFromDocAddress.E2_Longitude = 0;
			AssertEquals("We treat 0,0 as invalid for the time being.", false, leg.Pickup.HasValidGeoLocation);
		}

		public void TestHasValidAddress()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals("Waitpoint is null, and therefore invalid.", false, leg.WaitPoint.HasValidAddress);
			AssertEquals("Pickup is and organisation, therefore valid.", true, leg.Pickup.HasValidAddress);
			AssertEquals(true, leg.Pickup.HasValidOrgAddress);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			var legWithOverridenAddress = new Leg(cartageLeg);
			AssertEquals("Pickup is no longer and Org, bt it's overridden, therefore should still be valid.", true, legWithOverridenAddress.Pickup.HasValidAddress);
			AssertEquals(false, legWithOverridenAddress.Pickup.HasValidOrgAddress);
		}

		public void TestHasValidOrgAddress()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			AssertEquals(true, leg.Pickup.HasValidOrgAddress);
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			var legWithOverridenAddress = new Leg(cartageLeg);
			AssertEquals(false, legWithOverridenAddress.Pickup.HasValidOrgAddress);
		}

		public void TestIsSameAddress()
		{
			var cartageLeg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var cartageLegMatch = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var cartageLegNoMatch = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(OrgB, OrgC, OrgA, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg = new Leg(cartageLeg);
			var legMatch = new Leg(cartageLegMatch);
			var legNoMatch = new Leg(cartageLegNoMatch);
			AssertEquals(true, leg.Pickup.IsSameAddress(legMatch.Pickup));
			AssertEquals(false, leg.Pickup.IsSameAddress(legNoMatch.Pickup));
			OverrideAddress(cartageLeg.PickupFromDocAddress);
			OverrideAddress(cartageLegMatch.PickupFromDocAddress);
			AssertEquals(true, leg.Pickup.IsSameAddress(legMatch.Pickup));
			cartageLegMatch.PickupFromDocAddress.E2_CompanyName = "ABC";
			AssertEquals("E2_CompanyName is a variable used for the IsSameAddress comparison.", false, leg.Pickup.IsSameAddress(legMatch.Pickup));
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
		protected void OverrideAddress(JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				var orgAddress = docAddress.Address;
				docAddress.E2_AddressOverride = true;
				docAddress.E2_Latitude = orgAddress.OA_Latitude;
				docAddress.E2_Longitude = orgAddress.OA_Longitude;
			}
		}

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
