using System;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
	public class GPSCartageLegUpdater_RadiusTest : GPSCartageLegUpdaterTest
	{
		protected override OrgHeader CreateOrgWithAddress(ZString code, ZString addressCode, LatLong latLong)
		{
			var orgHeader = HelperGPS.CreateOrgWithAddress(code, addressCode, latLong.Latitude, latLong.Longitude);
			orgHeader.MainAddress.OA_GeofencePolygon = null;
			return orgHeader;
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_PickupAndDeliveryCloseTogether()
		{
			// create additional points around A
			var a_100e = CalculateLatLongWithDistance(A, 100, 0);
			var a_50w = CalculateLatLongWithDistance(A, 50, 180);
			var a_400w = CalculateLatLongWithDistance(A, 400, 180);
			// create points for Z
			var z = CalculateLatLongWithDistance(A, 600, 180); // Z (600m west of A)
			var z_50e = CalculateLatLongWithDistance(z, 50, 0);
			var orgZ = CreateOrgWithAddress("OZ", "Address OZ", z);
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, orgZ, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, a_100e, a_50w, a_400w, z_50e); // A_400w is 200m away from Z (is inside registry 500m radius)
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 3, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, a_100e, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, a_400w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2)); // event though A_400w is inside 500m, the radius is reduced to 300m because the next Address Z is 600m away (600/2=300)
				AssertGPSClientActivity(inOuts[2], orgZ, a_400w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2)); // A_400w is 200m away from Z, which is inside Z radius of 500m
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}
	}
}
