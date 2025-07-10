using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.Integration.GPS;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
	public abstract class GPSCartageLegUpdaterTest : TestCaseWithFactory
	{
		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_FutureCompatibility()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w);
			// date value higher than smalldate time max value
			CreateOrderedEvents(ZDateTime.MaxSmallDateTimeValue.AddDays(1), new LatLong(0, 0));
			Factory.Save();
			TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbDeviceAssignmentDivot SET V7_EndTimeUtc = null, V7_SystemLastEditUser = 'E', V7_SystemLastEditTimeUtc = GetDate() where V7_V3_Device = '{Device1PK}'");
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertEquals("Should set to last location time processed (C_600w was last, however it was not used).", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
				AssertArrayEqualsByElements("Should have a begin message, an end message, but no warning for small date time because it's more than one day in the future", new[] { "Begin: Running GPS Cartage Leg Updater Processor.", "End: Running GPS Cartage Leg Updater Processor." }, notifications.Select(x => x.Message).ToArray());
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_PastCompatibility()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w);
			// datetime lower than small datetime min value
			CreateOrderedEvents(new ZDateTime(ZDateTime.MinSmallDateTimeValue.AddDays(-1)), new LatLong(0, 0));
			Factory.Save();
			TestConnection.ExecuteNonQuery($"UPDATE dbo.GlbDeviceAssignmentDivot SET V7_EndTimeUtc = null, V7_StartTimeUtc = '{ZDateTime.MinSmallDateTimeValue.AddDays(-2).ToISO8601String()}', V7_SystemLastEditUser = 'E', V7_SystemLastEditTimeUtc = GetDate() where V7_V3_Device = '{Device1PK}'");
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.MinSmallDateTimeValue.ToDateTime().AddDays(-2))) // Datetime here needs to be before the date above
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertEquals("Should set to last location time processed (C_600w was last, however it was not used).", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
				AssertArrayEqualsByElements("Should have a begin message and an end only", new[] { "Begin: Running GPS Cartage Leg Updater Processor.", "Ignored 1 location(s) with out of range measurement time", "End: Running GPS Cartage Leg Updater Processor." }, notifications.Select(x => x.Message).ToArray());
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertEquals("Should set to last location time processed (C_600w was last, however it was not used).", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_WaitPoint()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w, C_50w, C, C_50e, C_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 6, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[4], OrgC, C_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertGPSClientActivity(inOuts[5], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(12));
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), EventTime.AddMinutes(4), EventTime.AddMinutes(7), EventTime.AddMinutes(9), EventTime.AddMinutes(12));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(12), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_MultipleLegs()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgD, RunSheet1, Today.AddHours(6), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w, C_50w, C, C_50e, C_600e, D_600w, D_50w, D, D_50e, D_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 8, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[4], OrgC, C_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertGPSClientActivity(inOuts[5], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(12));
				AssertGPSClientActivity(inOuts[6], OrgD, D_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(14));
				AssertGPSClientActivity(inOuts[7], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(17));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertTimeInOut(leg2, EventTime.AddMinutes(9), EventTime.AddMinutes(12), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(14), EventTime.AddMinutes(17));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(17), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_MultipleLegs_SameDeliveryAndPickup()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgB, null, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w, C_50w, C, C_50e, C_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 6, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[4], OrgC, C_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertGPSClientActivity(inOuts[5], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(12));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertTimeInOut(leg2, EventTime.AddMinutes(4), EventTime.AddMinutes(7), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(9), EventTime.AddMinutes(12));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(12), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_SequenceChanged_PickupTimeOut()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgD, RunSheet1, Today.AddHours(7), Truck1.PK);
			var leg3 = CreateAndDispatchLegWithPickupDeliveryTime(OrgD, null, OrgE, RunSheet1, Today.AddHours(8), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			CreateOrderedEvents(UTCEventTime, A);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 1, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertTimeInOut(leg1, EventTime, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			leg1.JU_RunSheetSequence = 3;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime.AddMinutes(1), A_600e, C, C_600e, D, D_600e, E, E_600w, B_600e, B);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime()))
			{
				AssertTimeInOut(leg1, EventTime, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 9, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[6], OrgE, E, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(6));
				AssertGPSClientActivity(inOuts[7], OrgE, E_600w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[8], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(9), ZDateTime.Empty);
				AssertTimeInOut(leg2, EventTime.AddMinutes(2), EventTime.AddMinutes(3), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(5));
				AssertTimeInOut(leg3, EventTime.AddMinutes(4), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(6), EventTime.AddMinutes(7));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(9), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_SequenceChanged_DeliveryTimeOut()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgD, RunSheet1, Today.AddHours(7), Truck1.PK);
			var leg3 = CreateAndDispatchLegWithPickupDeliveryTime(OrgD, null, OrgE, RunSheet1, Today.AddHours(8), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			CreateOrderedEvents(UTCEventTime, A, A_600e, B);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 3, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			leg1.JU_RunSheetSequence = 3;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime.AddMinutes(3), B_600e, C, C_600e, D, D_600e, E);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.AddMinutes(2).ToDateTime()))
			{
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 9, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[6], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(6));
				AssertGPSClientActivity(inOuts[7], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[8], OrgE, E, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(8));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), EventTime.AddMinutes(3));
				AssertTimeInOut(leg2, EventTime.AddMinutes(4), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(6), EventTime.AddMinutes(7));
				AssertTimeInOut(leg3, EventTime.AddMinutes(6), EventTime.AddMinutes(7), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(8), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_SequenceChanged_WaitPointTimeOut()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgB, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgD, null, OrgE, RunSheet1, Today.AddHours(7), Truck1.PK);
			var leg3 = CreateAndDispatchLegWithPickupDeliveryTime(OrgE, null, OrgC, RunSheet1, Today.AddHours(8), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			CreateOrderedEvents(UTCEventTime, A, A_600e, B);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 3, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			leg1.JU_RunSheetSequence = 3;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime.AddMinutes(3), B_600e, D, D_600e, E, E_600w, C);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.AddMinutes(2).ToDateTime()))
			{
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 9, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[6], OrgE, E, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(6));
				AssertGPSClientActivity(inOuts[7], OrgE, E_600w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[8], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(8));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), EventTime.AddMinutes(2), EventTime.AddMinutes(3), EventTime.AddMinutes(8), ZDateTime.Empty);
				AssertTimeInOut(leg2, EventTime.AddMinutes(4), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(6), EventTime.AddMinutes(7));
				AssertTimeInOut(leg3, EventTime.AddMinutes(6), EventTime.AddMinutes(7), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(8), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_MultipleRunSheets_WorkingOnDifferentAddressesAtTheSameTime_()
		{
			var truck2 = Factory.NewWithValidTestData<RefEquipment>();
			var device2PK = Helper.CreateDeviceAndDivot(truck2.PK, "m2", "two", new byte[] { 2 });
			var runSheet2 = HelperGPS.CreateRunSheet(truck2, Today, Today.AddDays(1));
			// First leg for Truck 1 + 2nd Leg for Truck 2
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgD, runSheet2, Today.AddHours(6), truck2.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e);
			CreateOrderedEvents(UTCEventTime.AddSeconds(3), device2PK, truck2.PK, C_600w, C_50w, C, C_50e, C_600e, D_600w, D_50w, D, D_50e, D_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 8, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgC, C_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(1).AddSeconds(3), truck2.PK);
				AssertGPSClientActivity(inOuts[2], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[4], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(4).AddSeconds(3), truck2.PK);
				AssertGPSClientActivity(inOuts[5], OrgD, D_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(6).AddSeconds(3), truck2.PK);
				AssertGPSClientActivity(inOuts[6], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[7], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(9).AddSeconds(3), truck2.PK);
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertTimeInOut(leg2, EventTime.AddMinutes(1).AddSeconds(3), EventTime.AddMinutes(4).AddSeconds(3), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(6).AddSeconds(3), EventTime.AddMinutes(9).AddSeconds(3));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(9).AddSeconds(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_MultipleRunSheets_WorkingOnSameAddressesAtTheSameTime()
		{
			var truck2 = Factory.NewWithValidTestData<RefEquipment>();
			var device2PK = Helper.CreateDeviceAndDivot(truck2.PK, "m2", "two", new byte[] { 2 });
			var runSheet2 = HelperGPS.CreateRunSheet(truck2, Today, Today.AddDays(1));
			// First leg for Truck 1 + 2nd Leg for Truck 2 (Both doing very similar legs)
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, runSheet2, Today.AddHours(6), truck2.PK);
			// Truck 2 is following Truck 1, Truck 2 is behind 3 seconds
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e);
			CreateOrderedEvents(UTCEventTime.AddSeconds(3), device2PK, truck2.PK, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 8, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddSeconds(3), truck2.PK);
				AssertGPSClientActivity(inOuts[2], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2).AddSeconds(3), truck2.PK);
				AssertGPSClientActivity(inOuts[4], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4).AddSeconds(3), truck2.PK);
				AssertGPSClientActivity(inOuts[6], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[7], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7).AddSeconds(3), truck2.PK);
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertTimeInOut(leg2, EventTime.AddSeconds(3), EventTime.AddMinutes(2).AddSeconds(3), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4).AddSeconds(3), EventTime.AddMinutes(7).AddSeconds(3));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(7).AddSeconds(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_NextOnly()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, B, B_50e, B_600e, A, A_50e, A_600e, B_600w, B_50w, B, B_50e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 3, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(7));
				AssertTimeInOut(leg, EventTime.AddMinutes(3), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(7), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(9), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_NextOnly_2SameLegs()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(7), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, B, B_50e, B_600e, A, A_50e, A_600e, A, B);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 3, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[2], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(7));
				AssertTimeInOut(leg1, EventTime.AddMinutes(3), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(7), ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(7), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GroupedLegs_Same2Legs()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(7), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			CreateOrderedEvents(UTCEventTime, B, B_50e, B_600e, A, A_50e, A_600e, A, B);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 3, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[2], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(7));
				AssertTimeInOut(leg1, EventTime.AddMinutes(3), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(7), ZDateTime.Empty);
				AssertTimeInOut(leg2, EventTime.AddMinutes(3), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(7), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(7), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GroupedLegs_MultiPickup()
		{
			// Allocator has indicated to the driver to follow the route C, A, B, D, E
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgB, RunSheet1, Today.AddHours(7), Truck1.PK);
			var leg3 = CreateAndDispatchLegWithPickupDeliveryTime(OrgD, null, OrgE, RunSheet1, Today.AddHours(8), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, C, C_600e, A, A_600e, B, B_600e, D, D_600e, E);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 9, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[6], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(6));
				AssertGPSClientActivity(inOuts[7], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[8], OrgE, E, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(8));
				AssertTimeInOut(leg1, EventTime.AddMinutes(2), EventTime.AddMinutes(3), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(5));
				AssertTimeInOut(leg2, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(5));
				AssertTimeInOut(leg3, EventTime.AddMinutes(6), EventTime.AddMinutes(7), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(8), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GroupedLegs_MultiPickup_CompletGroupFirst()
		{
			// Allocator has indicated to the driver to follow the route C, A, B, D, E
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgB, RunSheet1, Today.AddHours(7), Truck1.PK);
			var leg3 = CreateAndDispatchLegWithPickupDeliveryTime(OrgD, null, OrgE, RunSheet1, Today.AddHours(8), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 2;
			// has D event inserted before B
			CreateOrderedEvents(UTCEventTime, C, C_600e, A, A_600e, D, B, B_600e, D, D_600e, E);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg3, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 9, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[5], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(6));
				AssertGPSClientActivity(inOuts[6], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(7));
				AssertGPSClientActivity(inOuts[7], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(8));
				AssertGPSClientActivity(inOuts[8], OrgE, E, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertTimeInOut(leg1, EventTime.AddMinutes(2), EventTime.AddMinutes(3), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(5), EventTime.AddMinutes(6));
				AssertTimeInOut(leg2, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(5), EventTime.AddMinutes(6));
				AssertTimeInOut(leg3, EventTime.AddMinutes(7), EventTime.AddMinutes(8), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(9), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(9), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GroupedLegs_MultiDelivery()
		{
			// Allocator has indicated to the driver to follow the route A or C, then B or D
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgC, RunSheet1, Today.AddHours(7), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			CreateOrderedEvents(UTCEventTime, A, A_600e, C, C_600e, B, B_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 6, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(5));
				AssertTimeInOut(leg2, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), EventTime.AddMinutes(3));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(5), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_GroupedLegs_Mixed()
		{
			// Allocator has indicated to the driver to follow the route A or C, then B or D
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgD, RunSheet1, Today.AddHours(7), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			CreateOrderedEvents(UTCEventTime, C, C_600e, A, A_600e, B, B_600e, D, D_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 8, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[4], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[5], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5));
				AssertGPSClientActivity(inOuts[6], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(6));
				AssertGPSClientActivity(inOuts[7], OrgD, D_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg1, EventTime.AddMinutes(2), EventTime.AddMinutes(3), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(5));
				AssertTimeInOut(leg2, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(6), EventTime.AddMinutes(7));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(7), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_PickupAndDeliveryReallyCloseTogether()
		{
			// create Z Address, that is 120 west of A (100m lower limit for Radius is reached)
			var z = CalculateLatLongWithDistance(A, 120, 180);
			var z_110e_A_10w = CalculateLatLongWithDistance(A, 10, 180);
			var z_50e_A_70w = CalculateLatLongWithDistance(A, 70, 180);
			var z_10e_A_110w = CalculateLatLongWithDistance(A, 110, 180);
			var orgZ = CreateOrgWithAddress("OZ", "Address OZ", z);
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, orgZ, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, z_110e_A_10w, z_50e_A_70w, z_10e_A_110w, D);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, z_110e_A_10w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], orgZ, z_50e_A_70w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(1)); // we are still in A, and are now also in Z. (we hit the 100m radius reduction limit! so don't wait to exit A before looking for Z!)
				AssertGPSClientActivity(inOuts[2], OrgA, z_10e_A_110w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2)); // we are now outside A, still in Z.
				AssertGPSClientActivity(inOuts[3], orgZ, D, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3)); // now also outside Z.
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(1), EventTime.AddMinutes(3)); // in this extreme case, we timed in at delivery before timing out at pickup
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_PickupAndPickupGroupedReallyCloseTogether_1EventAtPickup()
		{
			// create Z Address, that is 10m west of A!
			var z = CalculateLatLongWithDistance(A, 10, 180);
			var orgZ = CreateOrgWithAddress("OZ", "Address OZ", z);
			// allocator has indicated to the driver to follow the route A (&Z), B, where A and Z are really close together!
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(orgZ, null, OrgB, RunSheet1, Today.AddHours(7), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			CreateOrderedEvents(UTCEventTime, A, A_600e, B, B_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 6, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], orgZ, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[2], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[3], orgZ, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[4], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[5], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(3));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), EventTime.AddMinutes(3));
				AssertTimeInOut(leg2, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), EventTime.AddMinutes(3));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(3), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_PickupAndPickupGroupedReallyCloseTogether_2EventsAtPickup()
		{
			// create Z Address, that is 10m west of A!
			var z = CalculateLatLongWithDistance(A, 10, 180);
			var orgZ = CreateOrgWithAddress("OZ", "Address OZ", z);
			// allocator has indicated to the driver to follow the route A (&Z), B, where A and Z are really close together!
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(orgZ, null, OrgB, RunSheet1, Today.AddHours(7), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B, B_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 6, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], orgZ, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[2], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], orgZ, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[4], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(3));
				AssertGPSClientActivity(inOuts[5], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(4));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(3), EventTime.AddMinutes(4));
				AssertTimeInOut(leg2, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(3), EventTime.AddMinutes(4));
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(4), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_Leg1DeliveryReallyCloseToLeg2PickupTogether()
		{
			// create Z Address, that is 120 west of A (100m lower limit for Radius is reached)
			var z = CalculateLatLongWithDistance(A, 120, 180);
			var z_110e_A_10w = CalculateLatLongWithDistance(A, 10, 180);
			var z_50e_A_70w = CalculateLatLongWithDistance(A, 70, 180);
			var z_10e_A_110w = CalculateLatLongWithDistance(A, 110, 180);
			var orgZ = CreateOrgWithAddress("OZ", "Address OZ", z);
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgC, null, OrgA, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(orgZ, null, OrgD, RunSheet1, Today.AddHours(7), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, C, C_600w, z_110e_A_10w, z_50e_A_70w, z_10e_A_110w, D);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 7, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgC, C, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgC, C_600w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(1));
				AssertGPSClientActivity(inOuts[2], OrgA, z_110e_A_10w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[3], orgZ, z_50e_A_70w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(3)); // we are still in A, and are now also in Z. (we hit the 100m radius reduction limit! so don't wait to exit A before looking for Z!)
				AssertGPSClientActivity(inOuts[4], OrgA, z_10e_A_110w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(4)); // we are now outside A, still in Z.
				AssertGPSClientActivity(inOuts[5], OrgD, D, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(5)); // same event used for Deliver in at D
				AssertGPSClientActivity(inOuts[6], orgZ, D, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(5)); // now also outside Z.
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(1), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(2), EventTime.AddMinutes(4));
				AssertTimeInOut(leg2, EventTime.AddMinutes(3), EventTime.AddMinutes(5), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(5), ZDateTime.Empty);
				AssertEquals("Should set to last location time.", UTCEventTime.AddMinutes(5), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_OverriddenAddress_WithoutLatLong_Pickup()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgF_NoLatLong, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 2, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[1], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertEquals("Should set to last location time processed (C_600w was last, however it was not used).", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_OverriddenAddress_WithoutLatLong_Delivery()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgF_NoLatLong, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 2, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertEquals("Should set to last location time processed (C_600w was last, however it was not used).", UTCEventTime.AddMinutes(8), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_OverriddenAddress_FreeTextInMiddleOf3Addresses()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgF_NoLatLong, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgF_NoLatLong, null, OrgC, RunSheet1, Today.AddHours(6), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w, C_50w, C, C_50e, C_600e, D_600w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgC, C_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertGPSClientActivity(inOuts[3], OrgC, C_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(12));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(9), EventTime.AddMinutes(12));
				AssertEquals("Should set to last location time (D_600w was last, however it was not used).", UTCEventTime.AddMinutes(13), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_OverriddenAddress_FreeTextDeliveryAndPickUpBackToBack()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgB, null, OrgF_NoLatLong, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgF_NoLatLong, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, B, B_50e, B_600e, C_600w, C_50w, C, C_50w, C_600w, B_600e, B_50e, B, B_50w, B_600w, A_600e);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgB, B, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50e, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(9));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600w, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(12));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(9), EventTime.AddMinutes(12));
				AssertEquals("Should set to last location time (A_600e was last, however it was not used).", UTCEventTime.AddMinutes(13), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_UpdateLegTimes_OverriddenAddress_FreeTextAtEndOf3Addresses()
		{
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgB, null, OrgF_NoLatLong, RunSheet1, Today.AddHours(6), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w, C_50w, C, C_50e, C_600e, D_600w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSClientActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg1, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
				AssertTimeInOut(leg2, EventTime.AddMinutes(4), EventTime.AddMinutes(7), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertEquals("Should set to last location time (D_600w was last, however it was not used).", UTCEventTime.AddMinutes(13), LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_SkipProcess_IfVehicleDoesNotMatch()
		{
			var truck2 = Factory.NewWithValidTestData<RefEquipment>();
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgC, OrgE, RunSheet1, Today.AddHours(6), truck2.PK);
			CreateOrderedEvents(UTCEventTime, A); // truck 1
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMinutes(-1).ToDateTime()))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Device location created for truck1, but Runsheet Leg associated with truck2, should not create any GPSClientActivity data", 0, inOuts.Length);
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		[TestDate(2017, 06, 13)]
		[TestUtcOffset(10, 30, 0)]
		public void TestProcessEvent_IfVehicleDoesNotMatchCheckForLoggedInDriver()
		{
			var pickupTimeIn = Today.AddHours(6);
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgC, OrgE, RunSheet1, pickupTimeIn, ZGuid.Empty);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.GS_Code = "TST";
			staff.StaffPlainTextPassword = "TEST";
			leg.WorkSheet.EY_GS_NKTruckDriver = staff.GS_Code;
			CreateOrderedEvents(pickupTimeIn, A); // truck 1
			helper.CreateTelEdgeAssociation(Truck1.PK, "RQ", staff.PK, "GS", 0, ZDateTimeOffset.Today.AddHours(-10.5), ZDateTimeOffset.Empty, "opt");
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMinutes(-1).ToDateTime()))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("GPSClientActivity data not created", 1, inOuts.Length);
				AssertTimeInOut(leg, EnvProxy.Instance.Time.GetLocalTimeFromUtc(pickupTimeIn.ToDateTime()), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		[TestDate(2017, 06, 13)]
		[TestUtcOffset(10, 30, 0)]
		public void TestProcessEvent_SkipProcess_IfVehicleDoesNotMatchAndOnlyMatchOnDriverHasAssignedTruck()
		{
			var truck2 = Factory.NewWithValidTestData<RefEquipment>();
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgC, OrgE, RunSheet1, Today.AddHours(6), truck2.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.GS_Code = "TST";
			staff.StaffPlainTextPassword = "TEST";

			leg.WorkSheet.EY_GS_NKTruckDriver = staff.GS_Code;

			CreateOrderedEvents(UTCEventTime, A); // truck 1
			helper.CreateTelEdgeAssociation(Truck1.PK, "RQ", staff.PK, "GS", 0, ZDateTimeOffset.Today.AddHours(-10.5), ZDateTimeOffset.Empty, "opt");
			Factory.Save();

			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMinutes(-1).ToDateTime()))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);

				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Truck1 GPS Activity signed to Runsheet Leg associated with truck2 with same driver, should not create any GPSClientActivity data", 0, inOuts.Length);
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_SkipRunsheetOutsideTimeFrame()
		{
			var runSheet2 = HelperGPS.CreateRunSheet(Truck1, Today.AddDays(2), Today.AddDays(3)); // run sheet in 2 days time
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgC, OrgE, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A); // now
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today.AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("No Runsheet available for this event time, should not create any GPSClientActivity data", 0, inOuts.Length);
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_MeasurementTimeUtc_InDistantFuture_FilteredOut()
		{
			// Set everything one year in the future
			UTCEventTime = UTCEventTime.AddYears(1);
			EventTime = EventTime.AddYears(1);
			Today = Today.AddYears(1);
			RunSheet1.EY_StartTime = RunSheet1.EY_StartTime.AddYears(1);
			RunSheet1.EY_EndTime = RunSheet1.EY_EndTime.AddYears(1);
			Helper.CreateDeviceAssignment(Device1PK, Truck1.PK, UTCEventTime.AddDays(-1), UTCEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			var leg1 = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			var leg2 = CreateAndDispatchLegWithPickupDeliveryTime(OrgB, null, OrgF_NoLatLong, RunSheet1, Today.AddHours(6), Truck1.PK);
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w, C_50w, C, C_50e, C_600e, D_600w);
			Factory.Save();
			var lastProcessedEventTime = UTCEventTime.ToDateTime().AddMinutes(-1);
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lastProcessedEventTime))
			{
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should filter out events from distant future", 0, inOuts.Length);
				AssertTimeInOut(leg1, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertTimeInOut(leg2, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				AssertEquals("Should not set last location time", lastProcessedEventTime, LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessEvent_SkipProcess_IfFeatureFlagTurnedOff()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, OrgC, OrgE, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMinutes(-1).ToDateTime()))
			using (LocalCartageDataRegistry.Instance.UseGlbDeviceLocationSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				Process(Notifications);
				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should not create any GPSClientActivity data", 0, inOuts.Length);
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		[TestDate(2017, 06, 13)]
		public void TestProcessLocation_UsesFactorySuppliedToProcessor()
		{
			var leg = CreateAndDispatchLegWithPickupDeliveryTime(OrgA, null, OrgB, RunSheet1, Today.AddHours(6), Truck1.PK);
			CreateOrderedEvents(UTCEventTime, A, A_50e, A_600e, B_600w, B_50w, B, B_50e, B_600e, C_600w);
			Factory.Save();
			using (LocalCartageDataRegistry.Instance.GeoFenceRadiusSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, UTCEventTime.ToDateTime().AddMinutes(-1)))
			{
				AssertTimeInOut(leg, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
				var factoryForProcessor = new BusinessObjectFactory();
				var processor = new GPSCartageLegUpdater(factoryForProcessor);
				foreach (var deviceLocation in DeviceLocations)
				{
					var deviceLocationWithEntity = factoryForProcessor.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
					processor.ProcessLocation(deviceLocationWithEntity);
				}
				var changedObjects = factoryForProcessor.GetChanges().GetChangedObjects();
				AssertGreaterThan("Factory for processor should have changes", changedObjects.Length, 0);
				// Simulate factory save being called after all of the processing has been done
				factoryForProcessor.Save();

				var inOuts = GetPortTransportGPSActivities();
				AssertEquals("Should create GPSSupporterActivity data", 4, inOuts.Length);
				AssertGPSClientActivity(inOuts[0], OrgA, A, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime);
				AssertGPSClientActivity(inOuts[1], OrgA, A_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(2));
				AssertGPSClientActivity(inOuts[2], OrgB, B_50w, GPSConstants.GPSInOutActivityType.Codes.GIN, EventTime.AddMinutes(4));
				AssertGPSClientActivity(inOuts[3], OrgB, B_600e, GPSConstants.GPSInOutActivityType.Codes.GOT, EventTime.AddMinutes(7));
				AssertTimeInOut(leg, EventTime, EventTime.AddMinutes(2), ZDateTime.Empty, ZDateTime.Empty, EventTime.AddMinutes(4), EventTime.AddMinutes(7));
			}
		}

		public void TestUpdateLastProcessedEventTime()
		{
			using (LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime()))
			{
				AssertEquals("Precondition: GetLastProcessedEventTime not set", ZDateTime.Empty, LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);

				var eventTime = new ZDateTime(2022, 2, 2);
				Processor.UpdateLastProcessedEventTime(eventTime);
				AssertEquals("Should update time from empty date to specified time", eventTime, LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);

				var laterEventTime = new ZDateTime(2033, 3, 3);
				Processor.UpdateLastProcessedEventTime(laterEventTime);
				AssertEquals("Should update time from valid time to a later valid time", laterEventTime, LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);

				Processor.UpdateLastProcessedEventTime(eventTime);
				AssertEquals("Should not update time from a later time to an earlier time", laterEventTime, LocalCartageDataRegistry.Instance.GetLastProcessedEventTime.Value);
			}
		}

		protected void AssertGPSClientActivity(IGPSSupporterActivity inOut, OrgHeader org, LatLong latLong, ZString inOutType, ZDateTime time)
		{
			AssertGPSClientActivity(inOut, org, latLong, inOutType, time, Truck1.PK);
		}

		protected void AssertGPSClientActivity(IGPSSupporterActivity inOut, OrgHeader org, LatLong latLong, ZString inOutType, ZDateTime time, ZGuid truck)
		{
			var speed = ZByte.ParseSafe(Speedkmh.ToString(), byte.MinValue);
			var headingDegree = ZShort.ParseSafe(HeadingDegrees.ToString(), ZShort.Zero);
			CombineAssertions(() =>
			{
				AssertEquals("Geofence Name", GetAddressCode(org), inOut.EN_ActivityInformation);
				AssertEquals("Geofence Event Type", GPSConstants.GPSEventTypeList.Codes.Custom, inOut.EN_EventType);
				AssertEquals("Geofence Activity Type", inOutType, inOut.EN_ActivityType);
				AssertEquals("Geofence Time Stamp", time, inOut.EN_ActivityTime);
				AssertEquals("Geofence Vehicle", truck, inOut.EN_RQ_Vehicle);
				AssertEquals("Geofence Speed", speed, inOut.EN_Speed);
				AssertEquals("Geofence HeadingDegree", headingDegree, inOut.EN_Heading);
				AssertEquals("Geofence Latitude", latLong.Latitude, inOut.EN_Latitude);
				AssertEquals("Geofence Longitude", latLong.Longitude, inOut.EN_Longitude);
			});
		}

		protected virtual ZString GetAddressCode(OrgHeader org)
		{
			return org.OH_Code + " - " + org.MainAddress.OA_Code;
		}

		protected void AssertTimeInOut(CommonCartageLeg leg, ZDateTime pickupIn, ZDateTime pickupOut, ZDateTime waitPointIn, ZDateTime waitPointOut, ZDateTime deliveryIn, ZDateTime deliveryOut)
		{
			CombineAssertions(() =>
			{
				AssertEquals("PickupTimeIn should be correct", pickupIn, leg.JU_PickupTimeIn);
				AssertEquals("PickupTimeOut should be correct", pickupOut, leg.JU_PickupTimeOut);
				AssertEquals("WaitingPointIn should be correct", waitPointIn, leg.JU_WaitPointTimeIn);
				AssertEquals("WaitingPointOut should be correct", waitPointOut, leg.JU_WaitPointTimeOut);
				AssertEquals("DeliveryIn should be correct", deliveryIn, leg.JU_DeliverTimeIn);
				AssertEquals("DeliveryOut should be correct", deliveryOut, leg.JU_DeliverTimeOut);
			});
		}

		/// <summary>
		/// Ordered Events created 1 minute apart
		/// </summary>
		protected void CreateOrderedEvents(ZDateTime eventTime, params LatLong[] latLongs)
		{
			CreateOrderedEvents(eventTime, Device1PK, Truck1.PK, latLongs);
		}

		/// <summary>
		/// Ordered Events created 1 minute apart
		/// </summary>
		void CreateOrderedEvents(ZDateTime eventTime, ZGuid device, ZGuid truck, params LatLong[] latLongs)
		{
			var incrementalEventTime = eventTime;
			foreach (var latlong in latLongs)
			{
				var deviceLocation = Helper.CreateDeviceLocation(device, truck, (double)latlong.Longitude, (double)latlong.Latitude, incrementalEventTime, Speedkmh, HeadingDegrees);
				DeviceLocations.Add(deviceLocation);
				incrementalEventTime = incrementalEventTime.AddMinutes(1);
			}
		}

		protected virtual OrgHeader CreateOrgWithAddress(ZString code, ZString addressCode, LatLong latLong)
		{
			return HelperGPS.CreateOrgWithAddress(code, addressCode, latLong.Latitude, latLong.Longitude);
		}

		protected virtual CommonCartageLeg CreateAndDispatchLegWithPickupDeliveryTime(OrgHeader fromAddress, OrgHeader waitAddress, OrgHeader toAddress, CommonWorkSheet workSheet, ZDateTime plannedPickupAndDispatchedTime, ZGuid truckPK)
		{
			return HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(fromAddress, waitAddress, toAddress, workSheet, plannedPickupAndDispatchedTime, truckPK);
		}

		public IGPSSupporterActivity[] GetPortTransportGPSActivities()
		{
			var query = new ZQuery(LocalCartageVehicleActivitySchema.EN_EventType, GPSConstants.GPSEventTypeList.Codes.Custom)
			{ OrderBy = "EN_ActivityTime, EN_ActivityInformation ASC" };
			return Factory.Load<IGPSSupporterActivity>(query);
		}

		public NotificationCollection Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = new NotificationCollection();
				}

				return notifications;
			}
		}

		NotificationCollection notifications;

		protected ITelematicsTestHelper Helper
		{
			get
			{
				return helper ?? (helper = ObjectFactory.Get<ITelematicsTestHelper>("ITelematicsTestHelper", Factory));
			}
		}

		ITelematicsTestHelper helper;
		protected GPSTestHelper HelperGPS
		{
			get
			{
				return helperGPS ?? (helperGPS = new GPSTestHelper(Factory));
			}
		}

		GPSTestHelper helperGPS;

		// Addresses 2km apart going east
		protected LatLong A => new LatLong(LatitudeWTG, LongitudeWTG);
		LatLong B => CalculateLatLongWithDistance(A, 2000, 0);
		LatLong C => CalculateLatLongWithDistance(B, 2000, 0);
		protected LatLong D => CalculateLatLongWithDistance(C, 2000, 0);
		LatLong E => CalculateLatLongWithDistance(D, 2000, 0);
		LatLong ZeroLatLong => new LatLong(0m, 0m);
		// Event Lat/Long East or West xxx meters of their Address
		LatLong A_50e => CalculateLatLongWithDistance(A, 50, 0);
		LatLong A_600e => CalculateLatLongWithDistance(A, 600, 0);
		LatLong B_600w => CalculateLatLongWithDistance(A, 1400, 0);
		LatLong B_50w => CalculateLatLongWithDistance(A, 1950, 0);
		LatLong B_50e => CalculateLatLongWithDistance(B, 50, 0);
		LatLong B_600e => CalculateLatLongWithDistance(B, 600, 0);
		LatLong C_600w => CalculateLatLongWithDistance(B, 1400, 0);
		LatLong C_50w => CalculateLatLongWithDistance(B, 1950, 0);
		LatLong C_50e => CalculateLatLongWithDistance(C, 50, 0);
		LatLong C_600e => CalculateLatLongWithDistance(C, 600, 0);
		LatLong D_600w => CalculateLatLongWithDistance(C, 1400, 0);
		LatLong D_50w => CalculateLatLongWithDistance(C, 1950, 0);
		LatLong D_50e => CalculateLatLongWithDistance(D, 50, 0);
		LatLong D_600e => CalculateLatLongWithDistance(D, 600, 0);
		LatLong E_600w => CalculateLatLongWithDistance(D, 1400, 0);

		readonly ZDecimal LongitudeWTG = 151.19513m;
		readonly ZDecimal LatitudeWTG = -33.91629m;
		protected OrgHeader OrgA;
		OrgHeader OrgB;
		OrgHeader OrgC;
		OrgHeader OrgD;
		OrgHeader OrgE;
		OrgHeader OrgF_NoLatLong;
		protected GPSCartageLegUpdater Processor;
		protected ZDecimal Speedkmh = 5;
		protected ZDecimal HeadingDegrees = 100;
		protected ZDateTime Today;
		protected ZDateTime EventTime;
		protected ZDateTime UTCEventTime;
		protected RefEquipment Truck1;
		protected ZGuid Device1PK;
		protected CommonWorkSheet RunSheet1;
		readonly List<IDeviceLocation> DeviceLocations = new List<IDeviceLocation>();
		readonly List<IDisposable> Disposables = new List<IDisposable>();
		protected override void SetUp()
		{
			base.SetUp();
			OrgA = CreateOrgWithAddress("OA", "Address OA", A);
			OrgB = CreateOrgWithAddress("OB", "Address OB", B);
			OrgC = CreateOrgWithAddress("OC", "Address OC", C);
			OrgD = CreateOrgWithAddress("OD", "Address OD", D);
			OrgE = CreateOrgWithAddress("OE", "Address OE", E);
			OrgF_NoLatLong = CreateOrgWithAddress("OF", "Address OF", ZeroLatLong);
			Today = ZDateTime.Today;
			EventTime = ZDateTime.Now;
			UTCEventTime = ZDateTime.UtcNow;
			Truck1 = Factory.NewWithValidTestData<RefEquipment>();
			Device1PK = Helper.CreateDeviceAndDivot(Truck1.PK, "m1", "one", new byte[] { 1 });
			RunSheet1 = HelperGPS.CreateRunSheet(Truck1, Today, Today.AddDays(1));
			Processor = new GPSCartageLegUpdater(new BusinessObjectFactory());
			DeviceLocations.Clear();
			Disposables.Add(LocalCartageDataRegistry.Instance.UseGlbDeviceLocationSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false));
		}

		protected override void TearDown()
		{
			OrgA = null;
			OrgB = null;
			OrgC = null;
			OrgD = null;
			OrgE = null;
			OrgF_NoLatLong = null;
			Processor = null;
			DeviceLocations.Clear();
			Disposables.ForEach(disposable => disposable.Dispose());
			base.TearDown();
		}

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

		protected class LatLong
		{
			public LatLong(ZDecimal latitude, ZDecimal longitude)
			{
				Latitude = latitude;
				Longitude = longitude;
			}

			public ZDecimal Latitude { get; set; }

			public ZDecimal Longitude { get; set; }
		}

		protected LatLong CalculateLatLongWithDistance(LatLong source, double range, double bearing)
		{
			var earthRadius = 6340960;
			double latA = ConvertDegreesToRadians((double)source.Latitude);
			double lonA = ConvertDegreesToRadians((double)source.Longitude);
			double angularDistance = range / earthRadius;
			double trueCourse = ConvertDegreesToRadians(bearing);
			double lat = Math.Asin(Math.Sin(latA) * Math.Cos(angularDistance) + Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));
			double dlon = Math.Atan2(Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA), Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));
			double lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;
			return new LatLong(ConvertRadiansToDegrees(lat), ConvertRadiansToDegrees(lon));
		}

		protected double ConvertDegreesToRadians(double degrees)
		{
			return Math.Round(degrees * Math.PI / 180, 5); // GPSClientActivity handle 5 decimal points
		}

		protected double ConvertRadiansToDegrees(double degrees)
		{
			return Math.Round(degrees * 180 / Math.PI, 5); // GPSClientActivity handle 5 decimal points
		}

		public void Process(INotifications notifications)
		{
			Processor.Process(notifications, CancellationToken.None);
		}
	}
}
