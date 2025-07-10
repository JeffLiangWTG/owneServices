using System;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
	public class GPSCartageLegUpdater_GeofenceTest : GPSCartageLegUpdaterTest
	{
		protected override OrgHeader CreateOrgWithAddress(ZString code, ZString addressCode, LatLong latLong)
		{
			var orgHeader = HelperGPS.CreateOrgWithAddress(code, addressCode, 0, 0);
			var geofence = (latLong.Latitude != 0 && latLong.Longitude != 0) ? CreateGeofenceFromLatLong(latLong) : null;
			orgHeader.MainAddress.OA_GeofencePolygon = geofence;
			return orgHeader;
		}

		ZGeography CreateGeofenceFromLatLong(LatLong latLong)
		{
			var diagonalLength = 150;
			var latLongNE = CalculateLatLongWithDistance(latLong, diagonalLength, 45);
			var latLongNW = CalculateLatLongWithDistance(latLong, diagonalLength, 135);
			var latLongSW = CalculateLatLongWithDistance(latLong, diagonalLength, 225);
			var latLongSE = CalculateLatLongWithDistance(latLong, diagonalLength, 315);
			var longLatString = latLongNE.Longitude.ToString() + " " + latLongNE.Latitude.ToString() + "," + latLongSE.Longitude.ToString() + " " + latLongSE.Latitude.ToString() + "," + latLongSW.Longitude.ToString() + " " + latLongSW.Latitude.ToString() + "," + latLongNW.Longitude.ToString() + " " + latLongNW.Latitude.ToString() + "," + latLongNE.Longitude.ToString() + " " + latLongNE.Latitude.ToString();
			return ZGeography.CreatePolygon(longLatString);
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GeofencesOverlap()
		{
			var z = CalculateLatLongWithDistance(A, 70, 180);
			var a_200e = CalculateLatLongWithDistance(A, 200, 0);
			var a_50e = CalculateLatLongWithDistance(A, 50, 0);
			var z_35e_A_35w = CalculateLatLongWithDistance(A, 35, 180);
			var z_50e_A_120w = CalculateLatLongWithDistance(A, 120, 180);
			var a_1000w = CalculateLatLongWithDistance(A, 1000, 180);
			var orgZ = CreateOrgWithAddress("OZ", "Address OZ", z);
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, orgZ, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, a_200e, a_50e, A, z_35e_A_35w, z, z_50e_A_120w, a_1000w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, a_50e, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[1], orgZ, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgA, z_50e_A_120w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[3], orgZ, a_1000w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(6));
				AssertTimeInOut(leg, EventTime.AddMinutes(1), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), EventTime.AddMinutes(6));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(6), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GeofenceAndRadiusOverlap()
		{
			// create Z Address, that is 120 west of A (100m lower limit for Radius is reached)
			var z = CalculateLatLongWithDistance(A, 120, 180);
			var z_110e_A_10w = CalculateLatLongWithDistance(A, 10, 180);
			var z_50e_A_70w = CalculateLatLongWithDistance(A, 70, 180);
			var z_10e_A_110w = CalculateLatLongWithDistance(A, 110, 180);
			var orgZ = HelperGPS.CreateOrgWithAddress("OZ", "Address OZ", z.Latitude, z.Longitude);
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, orgZ, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, z_110e_A_10w, z_50e_A_70w, z_10e_A_110w, D);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, z_110e_A_10w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], orgZ, z_50e_A_70w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(1)); // we are still in A, and are now also in Z. (we hit the 100m radius reduction limit! so don't wait to exit A before looking for Z!)
				AssertGPSClientActivity(inOuts[2], OrgA, z_10e_A_110w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2)); // we are now outside A, still in Z.
				AssertGPSClientActivity(inOuts[3], orgZ, D, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3)); // now also outside Z.
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(1), EventTime.AddMinutes(3)); // in this extreme case, we timed in at delivery before timing out at pickup
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_PickupAndDelivery_Mixed_InvalidLatLong()
		{
			// create Z Address, that has an invalid Long/Lat!
			var z = CalculateLatLongWithDistance(A, 120, 180);
			var z_110e_A_10w = CalculateLatLongWithDistance(A, 10, 180);
			var z_50e_A_70w = CalculateLatLongWithDistance(A, 70, 180);
			var z_10e_A_110w = CalculateLatLongWithDistance(A, 110, 180);
			var orgZ = HelperGPS.CreateOrgWithAddress("OZ", "Address OZ", (ZDecimal)23m, (ZDecimal)12m);
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, orgZ, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, z_110e_A_10w, z_50e_A_70w, z_10e_A_110w, D);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 2, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, z_110e_A_10w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, z_10e_A_110w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2)); // we are now outside A geofence.
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty); // no delivery times as Z Lat/Long is not valid
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}
	}
}
