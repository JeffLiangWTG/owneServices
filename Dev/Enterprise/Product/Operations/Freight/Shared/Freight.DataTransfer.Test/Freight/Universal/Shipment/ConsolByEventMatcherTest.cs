using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ConsolByEventMatcherTest : TestCaseWithFactory
	{
		public void TestMatchConsolByEvent_DoesNotCrashOnNullArguments()
		{
			AssertNull(new ConsolByEventMatcher().MatchConsolByEvent(null, null));

			var shipment = Factory.New<CommonShipment>();
			AssertNull(new ConsolByEventMatcher().MatchConsolByEvent(shipment, null));

			var eventLog = Factory.New<StmALog>();
			AssertNull(new ConsolByEventMatcher().MatchConsolByEvent(null, eventLog));
		}

		public void TestMatchConsolByEvent_MatchByEventParent()
		{
			var shipment = Factory.New<CommonShipment>();

			var consol1 = shipment.Consols.AddNew();
			var transport11 = consol1.Transports.AddNew();
			var transport12 = consol1.Transports.AddNew();

			var consol2 = shipment.Consols.AddNew();
			var transport21 = consol2.Transports.AddNew();
			var transport22 = consol2.Transports.AddNew();

			Action<CommonConsol, string, ZGuid> assertMatching = (expectedConsol, tableCode, parentPK) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_Table = tableCode;
					triggeringEvent.SL_Parent = parentPK;
				}

				AssertEquals(expectedConsol, new ConsolByEventMatcher().MatchConsolByEvent(shipment, triggeringEvent));
			};

			assertMatching(null, "", ZGuid.NewZGuid());
			assertMatching(null, "JobConsol", ZGuid.NewZGuid());

			assertMatching(consol1, "JobConsol", consol1.PK);
			assertMatching(consol2, "JobConsol", consol2.PK);

			assertMatching(null, "JobConsolTransport", ZGuid.NewZGuid());
			assertMatching(consol1, "JobConsolTransport", transport11.PK);
			assertMatching(consol1, "JobConsolTransport", transport12.PK);

			assertMatching(consol2, "JobConsolTransport", transport21.PK);
			assertMatching(consol2, "JobConsolTransport", transport22.PK);
		}

		public void TestMatchConsolByPropagatedEvent()
		{
			var eventTime = ZDateTimeOffset.Now;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUMEL";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "CNSHA";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			var container = consol1.Containers.AddNew();

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "AUMEL";

			var packline = shipment.OuterPackLines.AddNew();
			packline.Containers.Add(container);

			var floEvent = container.Logs.AddNew(Events.FreightLoaded, "|FAC=CTO|LOC=AUSYD|VFL=BA345", eventTime);

			var logs = shipment.Logs.GetAllLogs();
			var shipmentFloEvent = logs.Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == Events.FreightLoadedCode);
			AssertEquals("consol1 should be matched because event belongs to a container which is related to consol1", consol1, new ConsolByEventMatcher().MatchConsolByEvent(shipment, shipmentFloEvent));
		}

		public void TestMatchConsolByEvent_MatchByFacility()
		{
			var shipment = Factory.New<CommonShipment>();

			var consol1 = shipment.Consols.AddNew();
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;

			var consol2 = shipment.Consols.AddNew();
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(5);

			var consol3 = shipment.Consols.AddNew();
			consol3.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(10);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: LatestConsol", consol3, shipment.Consols.GetLatestConsol());

			Action<CommonConsol, string> assertMatching = (expectedConsol, eventReference) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_Reference = eventReference;
					triggeringEvent.SL_EventTime = ZDateTime.Empty;
				}

				AssertEquals(expectedConsol, new ConsolByEventMatcher().MatchConsolByEvent(shipment, triggeringEvent));
			};

			assertMatching(null, "FOOBAR");
			assertMatching(null, "|FAC=FOO");

			assertMatching(null, "|FAC=CTO");
			assertMatching(null, "|FAC=CY");

			assertMatching(consol1, "|FAC=CNR");
			assertMatching(consol3, "|FAC=CNE");

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestMatchConsolByEvent_MatchByLocationToConsolLegs()
		{
			var shipment = Factory.New<CommonShipment>();

			Action<CommonConsol, string, string> addConsolLeg = (consol, load, discharge) =>
			{
				var transport = consol.Transports.AddNew();
				transport.JW_RL_NKLoadPort = load;
				transport.JW_RL_NKDiscPort = discharge;
			};

			var consol1 = shipment.Consols.AddNew();
			addConsolLeg(consol1, "AUMEL", "AUBNE");
			addConsolLeg(consol1, "AUBNE", "AUSYD");

			var consol2 = shipment.Consols.AddNew();
			addConsolLeg(consol2, "NZAKL", "NZCHC");
			addConsolLeg(consol2, "NZCHC", "NZWLG");

			var consol3 = shipment.Consols.AddNew();
			addConsolLeg(consol3, "NZWLG", "SGSIN");

			Action<CommonConsol, string> assertMatching = (expectedConsol, eventReference) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_Reference = eventReference;
				}

				AssertEquals(expectedConsol, new ConsolByEventMatcher().MatchConsolByEvent(shipment, triggeringEvent));
			};

			assertMatching(null, "FOOBAR");
			assertMatching(null, "|LOC=FOO");
			assertMatching(null, "|LOC=DEHAM");

			assertMatching(consol1, "|LOC=AUMEL");
			assertMatching(consol1, "|LOC=AUBNE");
			assertMatching(consol1, "|LOC=AUSYD");

			assertMatching(consol2, "|LOC=NZAKL");
			assertMatching(consol2, "|LOC=NZCHC");

			assertMatching(null, "|LOC=NZWLG");
			assertMatching(consol3, "|LOC=SGSIN");
		}

		public void TestMatchConsolByEvent_MatchByLocationToShipmentOrConsolPorts()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "SGSIN";
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(-10);

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "CAVAN";
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = "CAVAN";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(10);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: LatestConsol", consol3, shipment.Consols.GetLatestConsol());

			Action<CommonConsol, string> assertMatching = (expectedConsol, eventReference) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_Reference = eventReference;
				}

				AssertEquals(expectedConsol, new ConsolByEventMatcher().MatchConsolByEvent(shipment, triggeringEvent));
			};

			assertMatching(null, "FOOBAR");
			assertMatching(null, "|LOC=FOO");

			assertMatching(null, "|LOC=CNSHA");
			assertMatching(null, "|LOC=HKHKG");

			assertMatching(null, "|LOC=CN");
			assertMatching(null, "|LOC=HK");

			assertMatching(null, "|LOC=AUSY");
			assertMatching(null, "|LOC=DEH");

			// To shipment ports, first direct UNLOCO, fallback to country
			assertMatching(consol1, "|LOC=AUSYD");
			assertMatching(consol1, "|LOC=AusYd");
			assertMatching(consol1, "|LOC=AUMEL");
			assertMatching(consol1, "|LOC=AU");
			assertMatching(consol1, "|LOC=au");

			assertMatching(consol3, "|LOC=DEHAM");
			assertMatching(consol3, "|LOC=dEHam");
			assertMatching(consol3, "|LOC=DEBRE");
			assertMatching(consol3, "|LOC=DE");
			assertMatching(consol3, "|LOC=dE");

			// To first consol load/last consol discharge ports, first direct UNLOCO, fallback to country
			assertMatching(consol1, "|LOC=NZAKL");
			assertMatching(consol1, "|LOC=NZCHC");
			assertMatching(consol1, "|LOC=NZ");

			assertMatching(consol3, "|LOC=USLAX");
			assertMatching(consol3, "|LOC=USSEA");
			assertMatching(consol3, "|LOC=US");

			// No match for domestic shipments
			assertMatching(consol1, "|LOC=AU");

			shipment.JS_RL_NKDestination = "AUMEL";
			assertMatching(null, "|LOC=AU");

			// No match when other consols have same country as first load/last discharge port
			assertMatching(consol1, "|LOC=NZ");

			consol2.JK_RL_NKLoadPort = "NZCHC";
			assertMatching(null, "|LOC=NZ");

			assertMatching(consol3, "|LOC=US");

			consol2.JK_RL_NKLoadPort = "USSEA";
			assertMatching(null, "|LOC=US");
		}

		public void TestMatchConsolByEvent_MatchByFlightDetails()
		{
			var shipment = Factory.New<CommonShipment>();

			Action<CommonConsol, string, string, ZDateTime, ZDateTime> addConsolLeg = (consol, transportMode, voyageFlight, estimatedDeparture, estimatedArrival) =>
			{
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = transportMode;
				transport.JW_VoyageFlight = voyageFlight;
				transport.JW_ETD = estimatedDeparture;
				transport.JW_ETA = estimatedArrival;
			};

			var today = new ZDateTime(2017, 05, 30);

			var consol1 = shipment.Consols.AddNew();
			addConsolLeg(consol1, "AIR", "QF100", today, today.AddDays(1));
			addConsolLeg(consol1, "AIR", "NZ100", today.AddDays(1), today.AddDays(2));

			var consol2 = shipment.Consols.AddNew();
			addConsolLeg(consol2, "SEA", "AA200", today, today.AddDays(1));
			addConsolLeg(consol2, "AIR", "NZ200", today, today.AddDays(1));

			Action<CommonConsol, string, string> assertMatching = (expectedConsol, eventCode, eventReference) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_SE_NKEvent = eventCode;
					triggeringEvent.SL_Reference = eventReference;
					triggeringEvent.SL_EventTime = today;
				}

				AssertEquals(expectedConsol, new ConsolByEventMatcher().MatchConsolByEvent(shipment, triggeringEvent));
			};

			CombineAssertions("Prerequisites", () =>
			{
				AssertEquals("ARV", Events.ArrivalCode);
				AssertEquals("DEP", Events.DepartureCode);
				AssertEquals("VFL", Constants.EventReferenceParameters.Codes.VoyageFlightNumber);
				AssertEquals("FDT", Constants.EventReferenceParameters.Codes.FlightDate);
			});

			assertMatching(null, "DEP", "FOOBAR");
			assertMatching(null, "DEP", "|VFL=FOO");
			assertMatching(null, "DEP", "|VFL=QF100");
			assertMatching(null, "DEP", "|VFL=QF100|FDT=IWILLNOTCRASH");
			assertMatching(null, "DEP", "|VFL=QF100|FDT=32-32-17");
			assertMatching(null, "DEP", "|VFL=QF100|FDT=17-MAY-30");

			assertMatching(null, "DEP", "|VFL=QF100|FDT=20-MAY-17");
			assertMatching(null, "DEP", "|VFL=QF200|FDT=30-MAY-17");

			assertMatching(consol1, "DEP", "|VFL=QF100|FDT=MAY-30-17");
			assertMatching(consol1, "DEP", "|VFL=QF100|FDT=2017-05-30 17:30");
			assertMatching(consol1, "DEP", "|VFL=QF100|FDT=2017-05-30");
			assertMatching(null, "DEP", "|VFL=QF100|FDT=2017-05");

			assertMatching(consol1, "DEP", "|VFL=QF100|FDT=30-MAY-17");
			assertMatching(null, "ARV", "|VFL=QF100|FDT=30-MAY-17");
			assertMatching(null, "XXX", "|VFL=QF100|FDT=30-MAY-17");

			assertMatching(null, "DEP", "|VFL=QF100|FDT=31-MAY-17");
			assertMatching(consol1, "ARV", "|VFL=QF100|FDT=31-MAY-17");
			assertMatching(null, "XXX", "|VFL=QF100|FDT=31-MAY-17");

			assertMatching(consol1, "DEP", "|VFL=NZ100|FDT=31-MAY-17");
			assertMatching(consol1, "ARV", "|VFL=NZ100|FDT=01-JUN-17");

			assertMatching(null, "DEP", "|VFL=AA200|FDT=30-MAY-17");
			assertMatching(null, "ARV", "|VFL=AA200|FDT=31-MAY-17");

			assertMatching(consol2, "DEP", "|VFL=NZ200|FDT=30-MAY-17");
			assertMatching(consol2, "ARV", "|VFL=NZ200|FDT=31-MAY-17");

			var consol3 = shipment.Consols.AddNew();
			addConsolLeg(consol3, "AIR", "NZ200", today, today.AddDays(1));

			assertMatching(null, "DEP", "|VFL=NZ200|FDT=30-MAY-17");
			assertMatching(null, "ARV", "|VFL=NZ200|FDT=31-MAY-17");
		}

		public void TestMatchConsolByEvent_MatchByEventTime()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var consol1 = shipment.Consols.AddNew();
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(-10);
			consol1.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Today.AddDays(-5);

			var consol2 = shipment.Consols.AddNew();
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;
			consol2.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Today.AddDays(5);

			var consol3 = shipment.Consols.AddNew();
			consol3.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(10);
			consol3.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Today.AddDays(20);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: LatestConsol", consol3, shipment.Consols.GetLatestConsol());

			Action<CommonConsol, ZDateTime> assertMatching = (expectedConsol, eventTime) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_EventTime = eventTime;
				}

				AssertEquals(expectedConsol, new ConsolByEventMatcher().MatchConsolByEvent(shipment, triggeringEvent));
			};
			assertMatching(null, ZDateTime.Empty);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			assertMatching(consol1, ZDateTime.Today.AddDays(-15));
			assertMatching(consol1, ZDateTime.Today.AddDays(-7));
			assertMatching(null, ZDateTime.Today.AddDays(-3));
			assertMatching(consol2, ZDateTime.Today.AddDays(1));
			assertMatching(null, ZDateTime.Today.AddDays(7));
			assertMatching(consol3, ZDateTime.Today.AddDays(12));
			assertMatching(consol3, ZDateTime.Today.AddDays(25));

			// No match when consols have overlapping dates
			consol2.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Today.AddDays(15);
			assertMatching(null, ZDateTime.Today.AddDays(12));

			// No match when any of the 'middle' dates are empty, i.e. open date
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;
			consol2.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Empty;
			assertMatching(null, ZDateTime.Today.AddDays(12));

			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;
			consol2.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Today.AddDays(5);
			assertMatching(consol3, ZDateTime.Today.AddDays(12));

			// No match when earliest consol has open arrival date and event time is after first departure
			consol1.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Empty;
			assertMatching(null, ZDateTime.Today.AddDays(12));

			// Still a match when eventtime is before first departure
			assertMatching(consol1, ZDateTime.Today.AddDays(-20));
		}
	}
}
