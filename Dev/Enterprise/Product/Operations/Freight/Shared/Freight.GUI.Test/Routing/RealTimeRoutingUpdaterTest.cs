using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class RealTimeRoutingUpdaterTest : TestCaseWithFactory
	{
		public void TestUpdateRouteFromChosenRealTimeRoute_SingleConnection()
		{
			string testMessageLine = "132 SYD SIN 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 2+08:30 ZZ   123            0 9012   >";
			RoutingResponseHeader realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			DummyRoutingParent parent = Factory.New<DummyRoutingParent>();
			Transport transport = parent.Transports.AddNew();
			AssertEquals(1, parent.Transports.Count);

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, parent.Transports, realTimeRoute, new ZDateTime(2008, 4, 13, 9, 30, 0), true);
			AssertEquals(1, parent.Transports.Count);

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2008, 4, 13, 20, 45, 0), transport.JW_ETD);
			AssertEquals(new ZDateTime(2008, 4, 15, 8, 30, 0), transport.JW_ETA);
			AssertEquals("ZZ123", transport.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);
		}

		public void TestUpdateRoutePropagationToShipment()
		{
			TestCase(true);
			TestCase(false);

			void TestCase(bool importFlight)
			{
				var departureDateTime = new ZDateTime(2024, 2, 28, 17, 0, 0);
				var arrivalDateTime = new ZDateTime(2024, 2, 29, 5, 0, 0);

				var sailing1PK = CreateVoyageAndSailing(departureDateTime, "SQ111",
					departureDateTime, "0",
					"AUSYD", departureDateTime,
					"SGSIN", arrivalDateTime);

				var consol = Factory.New<CommonConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKLoadPort = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_E_DEP = departureDateTime;
				shipment.JS_E_ARV = arrivalDateTime;

				AssertEquals("Precondition", 1, consol.Transports.Count);
				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_JX = sailing1PK;
				AssertEquals("Precondition", departureDateTime, transport.JW_ETD);
				AssertEquals("Precondition", arrivalDateTime, transport.JW_ETA);
				AssertEquals("Precondition", departureDateTime, transport.JW_STD);
				AssertEquals("Precondition", arrivalDateTime, transport.JW_STA);

				Factory.Save();

				var testMessageLine = "000 SYD SIN  8:10   19:10 1+00:20 SQ           2024/02/14 2024/03/30 1234567 <SYD 1 SIN     19:10 1+00:20 SQ   242    359     0  3908                                        1234567 24/02/14 24/03/30 J>";
				var realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
				if (importFlight)
				{
					RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(realTimeRoute, new ZDateTime[] { departureDateTime.Date.AddDays(1) }, Factory);
				}

				RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, consol.Transports, realTimeRoute, departureDateTime.Date.AddDays(1), true);
				AssertEquals(1, consol.Transports.Count);

				var newDepartureDateTime = departureDateTime.Date.AddDays(1).AddHours(19).AddMinutes(10);
				var newArrivalDateTime = departureDateTime.Date.AddDays(2).AddMinutes(20);

				AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
				AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
				AssertEquals(newDepartureDateTime, transport.JW_ETD);
				AssertEquals(newArrivalDateTime, transport.JW_ETA);
				AssertEquals("STD should not change", departureDateTime, transport.JW_STD);
				AssertEquals("STA should not change", arrivalDateTime, transport.JW_STA);
				AssertEquals("Shipment ETD should be updated", newDepartureDateTime, shipment.JS_E_DEP);
				AssertEquals("Shipment ETA should be updated", newArrivalDateTime, shipment.JS_E_ARV);
				AssertEquals("SQ242", transport.JW_VoyageFlight);
				AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);
			}
		}

		public void TestReportObjectReferenceIsNullIssure()
		{
			try
			{
				string testMessageLine = "132 SYD SIN 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 2+08:30 ZZ   123            0 9012   >";
				RoutingResponseHeader realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
				Transport transport = Factory.New<Transport>();
				RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, null, null, new ZDateTime(2008, 4, 13, 9, 30, 0), true);
			}
			catch (NullReferenceException)
			{
				AssertEquals(true, ErrorReporter.LastKeyReported.Contains("Issue 00819046 International Logistics"));
				ErrorReporter.Clear();
			}
		}

		public void TestUpdateRouteFromChosenRealTimeRoute_MultipleConnections()
		{
			string testMessageLine = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";
			RoutingResponseHeader realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			DummyRoutingParent parent = Factory.New<DummyRoutingParent>();
			Transport transport = parent.Transports.AddNew();
			AssertEquals(1, parent.Transports.Count);

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, parent.Transports, realTimeRoute, new ZDateTime(2008, 4, 13, 9, 30, 0), true);
			AssertEquals(2, parent.Transports.Count);

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2008, 4, 13, 20, 45, 0), transport.JW_ETD);
			AssertEquals(new ZDateTime(2008, 4, 14, 9, 45, 0), transport.JW_ETA);
			AssertEquals("ZZ123", transport.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);

			AssertEquals("SGSIN", parent.Transports[1].JW_RL_NKLoadPort);
			AssertEquals("INBOM", parent.Transports[1].JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2008, 4, 14, 10, 45, 0), parent.Transports[1].JW_ETD);
			AssertEquals(new ZDateTime(2008, 4, 15, 8, 30, 0), parent.Transports[1].JW_ETA);
			AssertEquals("XX090", parent.Transports[1].JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, parent.Transports[1].JW_TransportMode);
		}

		public void TestUpdateRouteFromChosenRealTimeRoute_MultipleConnections_FindMatchingSailing()
		{
			var departureDate = ZDateTime.Today;

			var sailing1PK = CreateVoyageAndSailing(departureDate, "BA9437",
				new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 1, 0, 0), "332",
				"AUSYD", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 7, 0, 0),
				"AUMEL", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 35, 0));

			var sailing2PK = CreateVoyageAndSailing(departureDate, "BA4138",
				new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 1, 0, 0), "333",
				"AUMEL", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 50, 0),
				"HKHKG", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 15, 15, 0));

			Factory.Save();

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			var parent = Factory.New<DummyRoutingParent>();
			var transport1 = parent.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1PK;
			AssertEquals(1, parent.Transports.Count);

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport1, parent.Transports, realTimeRoute, departureDate, true);
			AssertEquals(2, parent.Transports.Count);

			AssertEquals("AUSYD", transport1.JW_RL_NKLoadPort);
			AssertEquals("AUMEL", transport1.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 7, 0, 0), transport1.JW_ETD);
			AssertEquals(new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 35, 0), transport1.JW_ETA);
			AssertEquals("BA9437", transport1.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport1.JW_TransportMode);
			AssertEquals("332", transport1.JW_AircraftType);
			Assert(transport1.JW_IsLinked);
			AssertEquals(sailing1PK, transport1.JW_JX);

			var transport2 = parent.Transports[1];
			AssertEquals("AUMEL", transport2.JW_RL_NKLoadPort);
			AssertEquals("HKHKG", transport2.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 50, 0), transport2.JW_ETD);
			AssertEquals(new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 15, 15, 0), transport2.JW_ETA);
			AssertEquals("BA4138", transport2.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport2.JW_TransportMode);
			AssertEquals("333", transport2.JW_AircraftType);
			Assert(transport2.JW_IsLinked);
			AssertEquals(sailing2PK, transport2.JW_JX);
		}

		public void TestUpdateRouteFromChosenRealTimeRoute_MultipleConnections_WhenDoubleClickingHeaders()
		{
			var departureDate = ZDateTime.Today;

			var sailing1PK = CreateVoyageAndSailing(departureDate, "BA9437",
				departureDate.AddHours(1), "332",
				"AUSYD", departureDate.AddHours(7),
				"AUMEL", departureDate.AddHours(8).AddMinutes(35));

			var sailing2PK = CreateVoyageAndSailing(departureDate, "BA4138",
				departureDate.AddHours(1), "333",
				"AUMEL", departureDate.AddHours(8).AddMinutes(50),
				"HKHKG", departureDate.AddHours(15).AddMinutes(15));

			Factory.Save();

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			var parent = Factory.New<DummyRoutingParent>();
			var transport1 = parent.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1PK;
			AssertEquals(1, parent.Transports.Count);

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport1, parent.Transports, realTimeRoute, departureDate, false);

			AssertEquals(2, parent.Transports.Count);

			AssertEquals("AUSYD", transport1.JW_RL_NKLoadPort);
			AssertEquals("AUMEL", transport1.JW_RL_NKDiscPort);
			AssertEquals(departureDate.AddHours(7), transport1.JW_ETD);
			AssertEquals(departureDate.AddHours(8).AddMinutes(35), transport1.JW_ETA);
			AssertEquals("BA9437", transport1.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport1.JW_TransportMode);
			AssertEquals("332", transport1.JW_AircraftType);
			Assert("transport1 JW_IsLinked should keep checked when double clicking headers.", transport1.JW_IsLinked);
			AssertEquals("transport1 should link to previous sailing.", sailing1PK, transport1.JW_JX);

			var transport2 = parent.Transports[1];
			AssertEquals("AUMEL", transport2.JW_RL_NKLoadPort);
			AssertEquals("HKHKG", transport2.JW_RL_NKDiscPort);
			AssertEquals(departureDate.AddHours(8).AddMinutes(50), transport2.JW_ETD);
			AssertEquals(departureDate.AddHours(15).AddMinutes(15), transport2.JW_ETA);
			AssertEquals("BA4138", transport2.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport2.JW_TransportMode);
			AssertEquals("333", transport2.JW_AircraftType);
			Assert("transport2 JW_IsLinked should keep unchecked when double clicking headers.", !transport2.JW_IsLinked);
			AssertEquals("transport2 JW_JX should not link to a sailing.", ZGuid.Empty, transport2.JW_JX);
		}

		ZGuid CreateVoyageAndSailing(ZDateTime departureDate, ZString voyageFlight, ZDateTime flightDate, ZString aircraftType,
			ZString portLoading, ZDateTime originEstimatedDepartureTime,
			ZString portOfDischarge, ZDateTime destEstimatedArrivalTime)
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			var sailing = Factory.New<JobSailing>();

			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_FlightDate = flightDate;
			voyage.JV_IsCargoOnly = false;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_AircraftType = aircraftType;
			voyage.JV_IsActive = true;

			origin.JA_RL_NKPortOfLoading = portLoading;
			origin.JA_AutoCreated = true;
			origin.JA_JV = voyage.PK;
			origin.JA_E_DEP = originEstimatedDepartureTime;

			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_AutoCreated = true;
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = destEstimatedArrivalTime;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_IsPublished = true;

			return sailing.PK;
		}

		public void TestUpdateRouteFromChosenRealTimeRoute_MultipleConnections_ITransportParentCore()
		{
			string testMessageLine = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";
			var realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			var parent = Factory.New<IDtbBookingConsolidation>();
			var parentCore = (ITransportParentCore)parent;
			var transportCollection = new TransportCollection(parentCore);
			var transport = transportCollection.AddNew();
			AssertEquals(1, transportCollection.Count);

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, transportCollection, realTimeRoute, new ZDateTime(2008, 4, 13, 9, 30, 0), true);
			AssertEquals(2, transportCollection.Count);

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2008, 4, 13, 20, 45, 0), transport.JW_ETD);
			AssertEquals(new ZDateTime(2008, 4, 14, 9, 45, 0), transport.JW_ETA);
			AssertEquals("ZZ123", transport.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);

			AssertEquals("SGSIN", transportCollection[1].JW_RL_NKLoadPort);
			AssertEquals("INBOM", transportCollection[1].JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2008, 4, 14, 10, 45, 0), transportCollection[1].JW_ETD);
			AssertEquals(new ZDateTime(2008, 4, 15, 8, 30, 0), transportCollection[1].JW_ETA);
			AssertEquals("XX090", transportCollection[1].JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transportCollection[1].JW_TransportMode);
		}

		public void TestUpdateRouteFromChosenRealTimeRoute_InvalidIATACode()
		{
			string testMessageLine = "132 SYD SIN 13:30   20:45 2+08:30 ZZ AA BB    <000 1 SIN 3   20:45 2+08:30 ZZ   123            0 9012   >";
			RoutingResponseHeader realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			DummyRoutingParent parent = Factory.New<DummyRoutingParent>();
			Transport transport = parent.Transports.AddNew();

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, parent.Transports, realTimeRoute, new ZDateTime(2008, 4, 13, 9, 30, 0), true);
			AssertEquals("", transport.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
		}

		public void TestUpdateRouteFromChosenRealTimeRoute_UpdateCargoOnly()
		{
			string testMessageLine = "132 SYD SIN 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 F>";
			var realTimeRoute = new RoutingResponseHeader(testMessageLine, Factory);
			var parent = Factory.New<DummyRoutingParent>();
			var transport = parent.Transports.AddNew();
			transport.JW_IsCargoOnly = false;
			AssertEquals(1, parent.Transports.Count);
			AssertEquals(FlightTypeConstants.CargoOnly, realTimeRoute.FlightType);

			RealTimeRoutingUpdater.UpdateRouteFromChosenRealTimeRoute(transport, parent.Transports, realTimeRoute, new ZDateTime(2008, 4, 13, 9, 30, 0), true);
			AssertEquals(1, parent.Transports.Count);

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
			Assert(transport.JW_IsCargoOnly);
		}

		#region Test Objects

		class DummyRoutingParent : DummyBusinessObject, ITransportParent
		{
			public DummyRoutingParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			public TransportCollection Transports
			{
				get { return transports ?? (transports = new TransportCollection(this)); }
			}
			TransportCollection transports;

			public TransportSupporter TransportSupporter
			{
				get { return new DummyTransportSupporter(this); }
			}

			public ZString TypeCode { get { return ""; } }

			class DummyTransportSupporter : TransportSupporter<DummyRoutingParent>
			{
				public DummyTransportSupporter(DummyRoutingParent parent)
					: base(parent) { }

				public override ZString Description
				{
					get { throw new NotImplementedException(); }
				}

				public override ZString ConsignmentRef
				{
					get { throw new NotImplementedException(); }
				}

				public override ZString TransportMode
				{
					get { return ""; }
				}

				public override ZString ContainerMode
				{
					get { throw new NotImplementedException(); }
				}

				public override ZString BillOfLading
				{
					get { throw new NotImplementedException(); }
				}

				public override ZGuid ShippingLine
				{
					get { return ZGuid.Empty; }
					set { }
				}

				public override SecurityCheckpoint DistanceCalculationCheckpoint
				{
					get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
				}
			}

			public Directions JobDirection
			{
				get { return Directions.Domestic; }
			}

			void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
			{
			}
		}

		#endregion
	}
}
