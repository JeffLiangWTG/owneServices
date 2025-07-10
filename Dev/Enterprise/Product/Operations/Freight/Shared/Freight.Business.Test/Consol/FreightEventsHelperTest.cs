using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightEventsHelperTest : TestCaseWithFactory
	{
		public void TestCascadeIfApplicable_ExistingEventsInDatabase_AreIgnored()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.Logs.AddNew(Events.FreightLoaded);

			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			FreightEventsHelper.CascadeIfApplicable(consol, Events.FreightLoadedCode, new EnterpriseBusinessObject[] { shipment });

			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded));
		}

		public void TestCascadeIfApplicable_PropagatedEvents_AreIgnored()
		{
			var consol = Factory.New<CommonConsol>();
			var loadedLog = consol.Logs.AddNew(Events.FreightLoaded, "Propagated: Dummy");

			AssertEquals("Prerequisite", true, PropagationHandler.IsPropagatedEventLog(loadedLog));

			var shipment = consol.Shipments.AddNew();
			FreightEventsHelper.CascadeIfApplicable(consol, Events.FreightLoadedCode, new EnterpriseBusinessObject[] { shipment });

			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded));
		}

		public void TestCascadeIfApplicable_EventsWithPartialParameter_AreIgnored()
		{
			var consol = Factory.New<CommonConsol>();
			var loadedLog = consol.Logs.AddNew(Events.FreightLoaded, "|PTL=1");

			AssertEquals("Prerequisite", true, loadedLog.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.Partial));

			var shipment = consol.Shipments.AddNew();
			FreightEventsHelper.CascadeIfApplicable(consol, Events.FreightLoadedCode, new EnterpriseBusinessObject[] { shipment });

			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded));
		}

		public void TestCascadeIfApplicable_CheckIfCanUpdateProperty()
		{
			var eventTime = ZDateTimeOffset.Now;

			var consol = Factory.New<CommonConsol>();
			var loadedLog = consol.Logs.AddNew(Events.FreightLoaded, "Loaded reference|CMP=BAR", eventTime, false);

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Received;

			AssertEquals("Prerequisite", false, ((IEventDatePropertyChecker)shipment2).CanUpdateProperty(loadedLog));

			FreightEventsHelper.CascadeIfApplicable(consol, Events.FreightLoadedCode, new EnterpriseBusinessObject[] { shipment1, shipment2 });

			var cascadedLoadedLog = shipment1.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals("Loaded reference|CMP=BAR", cascadedLoadedLog.SL_Reference);
			AssertEquals(eventTime, cascadedLoadedLog.SL_EventTimeOffset);
			AssertEquals(false, cascadedLoadedLog.SL_IsEstimate);

			AssertNull(shipment2.Logs.MostRecentLogByEventTime(Events.FreightLoaded));
		}

		public void TestPropagateArivalDepartureEventToMatchedTransportLeg()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_VoyageFlight = "QF111";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 20);
			firstTransport.JW_ETD = new ZDateTime(2015, 05, 20);

			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_VoyageFlight = "QF222";
			secondTransport.JW_ETA = new ZDateTime(2015, 05, 20);
			secondTransport.JW_ETD = new ZDateTime(2015, 05, 20);

			AssertEquals("Precondition: Arrival event not recorded", null, firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertEquals("Precondition: Departure event not recorded", null, secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));

			var arrivalLog = Factory.NewWithValidTestData<StmALog>();
			using (arrivalLog.LockForUpdatingKeyFieldsForTesting())
			{
				arrivalLog.SL_SE_NKEvent = Events.ArrivalCode;
				arrivalLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF111|FDT=20-MAY-15";
				arrivalLog.SL_EventTime = new ZDateTime(2015, 05, 20);
			}

			var departureLog = Factory.NewWithValidTestData<StmALog>();
			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF222|FDT=20-MAY-15";
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 18);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(arrivalLog, shipment.Transports);
			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(departureLog, shipment.Transports);

			AssertNotNull("Arrival Event has been propagated to arrival transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated to arrival transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Departure));
			AssertNotNull("Departure Event has been propagated to departure transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
			AssertNull("Arrival Event has not been propagated to departure transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
		}

		public void TestPropagateArivalDepartureEventToMatchedTransportLeg_WithFlightNumber()
		{
			var sailing0 = CreateSailing("QF111", "AUSYD", "AUMEL", new ZDateTime(2015, 5, 25));
			var sailing1 = CreateSailing("QF222", "AUMEL", "DEFRA", new ZDateTime(2012, 5, 25));

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = "AIR";
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing0.PK;
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportType = "FL1";
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = new ZDateTime(2016, 5, 19, 11, 59, 0);
			transport1.JW_ETA = new ZDateTime(2016, 5, 20, 22, 59, 0);

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing1.PK;
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportType = "FL2";
			transport2.JW_VoyageFlight = "QF222";
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "DEFRA";
			transport2.JW_ETD = new ZDateTime(2016, 5, 21, 11, 59, 0);
			transport2.JW_ETA = new ZDateTime(2016, 5, 21, 22, 59, 0);

			AssertEquals("Precondition: Arrival event not recorded", null, transport1.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertEquals("Precondition: Departure event not recorded", null, transport2.Logs.MostRecentLogByEventTime(Events.Departure));

			var arrivalLog = Factory.NewWithValidTestData<StmALog>();
			using (arrivalLog.LockForUpdatingKeyFieldsForTesting())
			{
				arrivalLog.SL_SE_NKEvent = Events.ArrivalCode;
				arrivalLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF111|FDT=20-MAY-16";
				arrivalLog.SL_EventTime = new ZDateTime(2015, 05, 20);
			}

			var departureLog = Factory.NewWithValidTestData<StmALog>();
			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF222|FDT=21-MAY-16";
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 20);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(arrivalLog, shipment.Transports);

			AssertNotNull("Arrival Event has been propagated to arrival transport leg", transport1.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated to arrival transport leg", transport1.Logs.MostRecentLogByEventTime(Events.Departure));

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "ARV");
			query.AddToFilter(StmALogSchema.SL_Parent, transport1.PK);
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2015, 05, 20));
			var transportlog = Factory.Load<StmALog>(query);
			AssertEquals("SL_Reference", "|FAC=CTO|FDT=20-MAY-16|LOC=AUMEL|VFL=QF111", transportlog[0].SL_Reference);
		}

		public void TestPropagateArivalDepartureEventToMatchedTransportLeg_WithMatchedFlightNumber()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = "AIR";
			transport1.JW_LegOrder = 1;
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = new ZDateTime(2016, 5, 19, 11, 59, 0);
			transport1.JW_ETA = new ZDateTime(2016, 5, 20, 22, 59, 0);

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_LegOrder = 2;
			transport2.JW_VoyageFlight = "QF222";
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "DEFRA";
			transport2.JW_ETD = new ZDateTime(2016, 5, 21, 11, 59, 0);
			transport2.JW_ETA = new ZDateTime(2016, 5, 21, 22, 59, 0);

			var arrivalLog = Factory.NewWithValidTestData<StmALog>();
			using (arrivalLog.LockForUpdatingKeyFieldsForTesting())
			{
				arrivalLog.SL_SE_NKEvent = Events.ArrivalCode;
				arrivalLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF0111|FDT=20-MAY-16";
				arrivalLog.SL_EventTime = new ZDateTime(2015, 05, 20);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(arrivalLog, shipment.Transports);

			AssertNotNull("Arrival Event has been propagated to arrival transport leg", transport1.Logs.MostRecentLogByEventTime(Events.Arrival));
		}

		public void TestArrivalDepartureEventIsNotPropagatedWhenTransportHasSameDate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 21);

			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_ATD = new ZDateTime(2015, 05, 19);

			var arrivalLog = Factory.NewWithValidTestData<StmALog>();
			using (arrivalLog.LockForUpdatingKeyFieldsForTesting())
			{
				arrivalLog.SL_SE_NKEvent = Events.ArrivalCode;
				arrivalLog.SL_Reference = "|FAC=CTO|LOC=AUMEL";
				arrivalLog.SL_IsEstimate = true;
				arrivalLog.SL_EventTime = new ZDateTime(2015, 05, 21);
			}

			var departureLog = Factory.NewWithValidTestData<StmALog>();
			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|LOC=AUMEL";
				departureLog.SL_IsEstimate = false;
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 19);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(arrivalLog, shipment.Transports);
			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(departureLog, shipment.Transports);

			AssertNull("Arrival Event has been propagated transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has been propagated transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		public void TestArrivalDepartureEventIsPropagatedWhenTransportHasDifferentDate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_VoyageFlight = "QF222";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 21);
			firstTransport.JW_ETD = new ZDateTime(2015, 05, 22);

			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_VoyageFlight = "QF222";
			secondTransport.JW_ATD = new ZDateTime(2015, 05, 19);
			secondTransport.JW_ETD = new ZDateTime(2015, 05, 22);

			var arrivalLog = Factory.NewWithValidTestData<StmALog>();
			using (arrivalLog.LockForUpdatingKeyFieldsForTesting())
			{
				arrivalLog.SL_SE_NKEvent = Events.ArrivalCode;
				arrivalLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF222|FDT=21-MAY-15";
				arrivalLog.SL_IsEstimate = true;
				arrivalLog.SL_EventTime = new ZDateTime(2015, 05, 22);
			}

			var departureLog = Factory.NewWithValidTestData<StmALog>();
			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|LOC=AUMEL|VFL=QF222|FDT=22-MAY-15";
				departureLog.SL_IsEstimate = false;
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 18);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(arrivalLog, shipment.Transports);
			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(departureLog, shipment.Transports);

			AssertNotNull("Arrival Event has not been propagated transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNotNull("Departure Event has not been propagated transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		[ExpectNoExceptions]
		public void TestArrivalDepartureEventNoTransports()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";

			var departureLog = Factory.NewWithValidTestData<StmALog>();
			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|LOC=AUMEL";
				departureLog.SL_IsEstimate = false;
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 18);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(departureLog, shipment.Transports);

			firstTransport.Delete();

			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";

			var arrivalLog = Factory.NewWithValidTestData<StmALog>();
			using (arrivalLog.LockForUpdatingKeyFieldsForTesting())
			{
				arrivalLog.SL_SE_NKEvent = Events.ArrivalCode;
				arrivalLog.SL_Reference = "|FAC=CTO|LOC=AUMEL";
				arrivalLog.SL_IsEstimate = true;
				arrivalLog.SL_EventTime = new ZDateTime(2015, 05, 20);
			}

			FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(arrivalLog, shipment.Transports);

			AssertNull("Arrival Event has not been propagated transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		[ExpectNoExceptions]
		public void TestGetTransportLegsForMatching_InvalidFlightDateParameter()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var transport = shipment.Transports.AddNew();
			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETA = new ZDateTime(2016, 12, 3);

			var departureLog = Factory.NewWithValidTestData<StmALog>();

			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|FDT=INVALIDDATE|LOC=AUSYD|VFL=QF123";
				departureLog.SL_IsEstimate = false;
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 18);
			}

			var matchedTransports = FreightEventsHelper.GetTransportLegsForMatching(departureLog, shipment.Transports.Cast<Transport>());
			AssertEquals(0, matchedTransports.Count());
		}

		public void TestGetTransportLegsForMatching_MatchUsingFuzzyVoyageFlight()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var transport = shipment.Transports.AddNew();
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "CNSHA";
			transport.JW_ETD = new ZDateTime(2016, 5, 21);
			transport.JW_ETA = new ZDateTime(2016, 5, 28);

			var departureLog = Factory.NewWithValidTestData<StmALog>();

			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|FDT=21-MAY-16|LOC=AUSYD|VFL=QF123T";
				departureLog.SL_IsEstimate = false;
				departureLog.SL_EventTime = new ZDateTime(2015, 05, 18);
			}

			var matchedTransports = FreightEventsHelper.GetTransportLegsForMatching(departureLog, shipment.Transports.Cast<Transport>());
			AssertEquals(1, matchedTransports.Count());
			AssertEquals("Apply a fuzzy matching by comparing flight numbers without an optinal letter in the end", transport.PK, matchedTransports.First().PK);
		}

		public void TestGetTransportLegsForMatching_WithMode()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = "ROA";
			transport1.JW_RL_NKLoadPort = "AUSYD";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_VoyageFlight = "QF123";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETA = new ZDateTime(2016, 12, 3);

			var departureLog = Factory.NewWithValidTestData<StmALog>();

			using (departureLog.LockForUpdatingKeyFieldsForTesting())
			{
				departureLog.SL_SE_NKEvent = Events.DepartureCode;
				departureLog.SL_Reference = "|FAC=CTO|LOC=AUSYD|MOD=SEA";
				departureLog.SL_IsEstimate = false;
			}

			var matchedTransports = FreightEventsHelper.GetTransportLegsForMatching(departureLog, shipment.Transports.Cast<Transport>());
			AssertEquals(1, matchedTransports.Count());
			AssertEquals(transport2.PK, matchedTransports.First().PK);
		}

		public void TestGetTransportLegsForMatching_FromEvent_WithMode()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = "ROA";
			transport1.JW_RL_NKLoadPort = "AUSYD";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_VoyageFlight = "QF123";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETA = new ZDateTime(2016, 12, 3);

			var eventParameters = new EventParameters();
			eventParameters.TransportMode = "SEA";

			var universalEvent = new UniversalEvent();
			universalEvent.EventTime = ZDateTimeOffset.Now;
			universalEvent.EventType = Events.DepartureCode;
			universalEvent.EventParameters = eventParameters;

			var matchedTransports = FreightEventsHelper.GetTransportLegsForMatching(universalEvent, shipment.Transports.Cast<Transport>());
			AssertEquals(1, matchedTransports.Count());
			AssertEquals(transport2.PK, matchedTransports.First().PK);
		}

		JobSailing CreateSailing(string voyageFlight, string portOfLoading, string portOfDischarge, ZDateTime flightDate)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_FlightDate = flightDate;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = portOfLoading;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;

			var sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			return sailing;
		}
	}
}
