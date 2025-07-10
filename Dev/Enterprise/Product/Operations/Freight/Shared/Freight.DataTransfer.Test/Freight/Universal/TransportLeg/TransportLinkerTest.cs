using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using AutoEvents = Enterprise.ZArchitecture.Business.AutoEvents;
using CommonConsol = Enterprise.Freight.Business.CommonConsol;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using Events = Enterprise.ZArchitecture.Business.Events;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class TransportLinkerTest : TestCaseWithFactory
	{
		public void TestGetLogParent_IATACodesMatchLegPorts_ReturnLeg()
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_IATA, "YUL");
			var unlocos = Factory.Load<RefUNLOCO>(query);

			AssertContainsExactElementsInAnyOrder("prerequisite", unlocos.Select(x => x.RL_Code), new[] { "CADOR", "CAYUL" });

			var consol1 = Factory.New<CommonConsol>();
			var transport1 = consol1.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "CADOR";
			transport1.JW_RL_NKDiscPort = "ZAJNB";
			consol1.JK_MasterBillNum = "7241971000";

			var consol2 = Factory.New<CommonConsol>();
			var transport2 = consol2.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "CAYUL";
			transport2.JW_RL_NKDiscPort = "ZAJNB";
			consol2.JK_MasterBillNum = "72419718790";

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Terminal;

			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.OriginIATAAirportCode) }, Value = "YUL" });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.DestinationIATAAirportCode) }, Value = "ZRH" });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol2, incomingEvent, eventLegPorts);
			AssertEquals(transport2.PK, logParent.PK);
		}

		public void TestGetLogParent_BestMatchingTransportLeg_WhenEventDoesNotHaveTransportMode()
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_VoyageFlight = "QF12";
			transport1.JW_ETD = date;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_VoyageFlight = "QF1";
			transport2.JW_ETD = date;

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Terminal;

			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = date.ToString() });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QF01" });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);
			AssertEquals(transport2.PK, logParent.PK);
		}

		public void TestGetLogParent_BestMatchingTransportLeg_ShouldGetBasedOnTransportMode_WhenEventHasTransportMode()
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_VoyageFlight = "QF12";
			transport1.JW_ETD = date;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_VoyageFlight = "QF1";
			transport2.JW_ETD = date;

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = "SEA";
			transport3.JW_VoyageFlight = "QF1";
			transport3.JW_ETD = date;

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Terminal;
			incomingEvent.EventParameters.TransportMode = Core.Constants.TransportModes.Air;

			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = date.ToString() });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QF1" });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);
			AssertEquals(transport2.PK, logParent.PK);
		}

		public void TestGetLogParent_BestMatchingTransportLeg_ShouldReturnNull_WhenEventHasTransportModeWithoutMatchInLegs()
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "SEA";
			transport1.JW_VoyageFlight = "QF1";
			transport1.JW_ETD = date;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_VoyageFlight = "QF12";
			transport2.JW_ETD = date;

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Terminal;
			incomingEvent.EventParameters.TransportMode = Core.Constants.TransportModes.Air;

			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = date.ToString() });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QF01" });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);
			AssertNull(logParent);
		}

		public void TestGetLogParent_UnLinkTransport_UNLOCO()
		{
			AssertEventToUnlinkTransportByUNLOCO(AutoEvents.DepartureCode);
			AssertEventToUnlinkTransportByUNLOCO(AutoEvents.FreightLoadedCode, null, false);
			AssertEventToUnlinkTransportByUNLOCO(AutoEvents.BookingConfirmedCode);
			AssertEventToUnlinkTransportByUNLOCO(AutoEvents.StatusUpdatedCode, "Manifested");
			AssertEventToUnlinkTransportByUNLOCO(AutoEvents.StatusUpdatedCode, "Prepared For Loading");
			AssertEventToUnlinkTransportByUNLOCO(AutoEvents.ArrivalCode);
		}

		public void TestGetLogParent_UnLinkTransport_IATA()
		{
			AssertEventToUnlinkTransportByIATA(AutoEvents.DepartureCode);
			AssertEventToUnlinkTransportByIATA(AutoEvents.FreightLoadedCode, null, false);
			AssertEventToUnlinkTransportByIATA(AutoEvents.BookingConfirmedCode);
			AssertEventToUnlinkTransportByIATA(AutoEvents.StatusUpdatedCode, "Manifested");
			AssertEventToUnlinkTransportByIATA(AutoEvents.StatusUpdatedCode, "Prepared For Loading");
			AssertEventToUnlinkTransportByIATA(AutoEvents.ArrivalCode);
		}

		public void TestGetLogParent_UnLinkTransport_IATA_FallbackToLOC()
		{
			AssertEventToUnlinkTransportByIATA_FallbackToLOC(AutoEvents.DepartureCode, "NUE");
			AssertEventToUnlinkTransportByIATA_FallbackToLOC(AutoEvents.FreightLoadedCode, "NUE", null, false);
			AssertEventToUnlinkTransportByIATA_FallbackToLOC(AutoEvents.BookingConfirmedCode, "NUE");
			AssertEventToUnlinkTransportByIATA_FallbackToLOC(AutoEvents.StatusUpdatedCode, "NUE", "Manifested");
			AssertEventToUnlinkTransportByIATA_FallbackToLOC(AutoEvents.StatusUpdatedCode, "NUE", "Prepared For Loading");
			AssertEventToUnlinkTransportByIATA_FallbackToLOC(AutoEvents.ArrivalCode, "LUX");
		}

		void AssertEventToUnlinkTransportByIATA(string eventCode, string parameter = null, bool returnTransportAsParent = true)
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DENUE";
			transport1.JW_RL_NKDiscPort = "LULUX";
			transport1.JW_IsLinked = true;
			transport1.JW_VoyageFlight = "QE001";
			transport1.JW_ETD = date;
			transport1.JW_ETA = date.AddDays(1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_VoyageFlight = "QE002";
			transport2.JW_ETD = date.AddDays(2);

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = eventCode;
			incomingEvent.EventParameters = new EventParameters() { FlightDate = date.AddDays(1), Type = parameter, VoyageFlightNumber = "QE001" };
			incomingEvent.ContextCollection = new List<Context>()
			{
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.OriginIATAAirportCode) }, Value = "NUE" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.DestinationIATAAirportCode) }, Value = "LUX" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QE001" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = (eventCode == AutoEvents.ArrivalCode ? date.AddDays(1) : date).ToString() }
			};

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			if (returnTransportAsParent)
			{
				AssertEquals(transport1.PK, logParent.PK);
			}
			else
			{
				AssertEquals(null, logParent);
			}

			Assert(!transport1.JW_IsLinked);
		}

		void AssertEventToUnlinkTransportByIATA_FallbackToLOC(string eventCode, string iataCode, string parameter = null, bool returnTransportAsParent = true)
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DENUE";
			transport1.JW_RL_NKDiscPort = "LULUX";
			transport1.JW_IsLinked = true;
			transport1.JW_VoyageFlight = "QE001";
			transport1.JW_ETD = date;
			transport1.JW_ETA = date.AddDays(1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_VoyageFlight = "QE002";
			transport2.JW_ETD = date.AddDays(2);

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = eventCode;
			incomingEvent.EventParameters = new EventParameters() { Location = iataCode, FlightDate = date.AddDays(1), Type = parameter, VoyageFlightNumber = "QE001" };
			incomingEvent.ContextCollection = new List<Context>();

			if (eventCode != Events.ArrivalCode)
			{
				incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.DestinationIATAAirportCode) }, Value = "LUX" });
			}
			else
			{
				incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.OriginIATAAirportCode) }, Value = "NUE" });
			}

			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QE001" });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = (eventCode == AutoEvents.ArrivalCode ? date.AddDays(1) : date).ToString() });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			if (returnTransportAsParent)
			{
				AssertEquals(transport1.PK, logParent.PK);
			}
			else
			{
				AssertEquals(null, logParent);
			}

			Assert(!transport1.JW_IsLinked);
		}

		void AssertEventToUnlinkTransportByUNLOCO(string eventCode, string parameter = null, bool returnTransportAsParent = true)
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DENUE";
			transport1.JW_RL_NKDiscPort = "LULUX";
			transport1.JW_IsLinked = true;
			transport1.JW_VoyageFlight = "QE001";
			transport1.JW_ETD = date;
			transport1.JW_ETA = date.AddDays(1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_VoyageFlight = "QE002";
			transport2.JW_ETD = date.AddDays(2);

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = eventCode;
			incomingEvent.EventParameters = new EventParameters() { FlightDate = date.AddDays(1), Type = parameter, VoyageFlightNumber = "QE001" };
			incomingEvent.ContextCollection = new List<Context>()
			{
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegOriginUNLOCO) }, Value = "DENUE" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegDestinationUNLOCO) }, Value = "LULUX" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QE001" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = (eventCode == AutoEvents.ArrivalCode ? date.AddDays(1) : date).ToString() }
			};

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			if (returnTransportAsParent)
			{
				AssertEquals(transport1.PK, logParent.PK);
			}
			else
			{
				AssertEquals(null, logParent);
			}

			Assert(!transport1.JW_IsLinked);
		}

		public void TestEventToUnlinkTransportByUNLOCO_DoNotUnlinkForSea()
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DENUE";
			transport1.JW_RL_NKDiscPort = "LULUX";
			transport1.JW_IsLinked = true;
			transport1.JW_VoyageFlight = "QE001";
			transport1.JW_ETD = date;
			transport1.JW_ETA = date.AddDays(1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_VoyageFlight = "QE002";
			transport2.JW_ETD = date.AddDays(2);

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.ArrivalCode;
			incomingEvent.EventParameters = new EventParameters() { FlightDate = date.AddDays(1), VoyageFlightNumber = "QE001" };
			incomingEvent.ContextCollection = new List<Context>()
			{
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegOriginUNLOCO) }, Value = "DENUE" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegDestinationUNLOCO) }, Value = "LULUX" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "QE001" },
				new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = date.AddDays(1).ToString() }
			};

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);
			Assert(!transport1.JW_IsLinked);
		}

		public void TestGetLogParent_DepartureOrArrivalEvent_NonTerminalFacility_Ignore()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_BookingReference = "BKR00001";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "TWKHH";
			transport.JW_RL_NKDiscPort = "DEHAM";
			transport.JW_ETD = ZDateTime.Now;

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;
			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegOriginUNLOCO) }, Value = "TWKHH" });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegDestinationUNLOCO) }, Value = "DEHAM" });

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Depot;

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();
			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			AssertNull(logParent);
		}

		public void TestGetLogParent_DepartureOrArrivalEvent_NoFacility_Process()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_BookingReference = "BKR00001";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "TWKHH";
			transport.JW_RL_NKDiscPort = "DEHAM";
			transport.JW_ETD = ZDateTime.Now;

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;
			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegOriginUNLOCO) }, Value = "TWKHH" });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegDestinationUNLOCO) }, Value = "DEHAM" });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();
			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			AssertNotNull(logParent);
		}

		public void TestGetLogParent_TerminalFacility_Process()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_BookingReference = "BKR00001";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "TWKHH";
			transport.JW_RL_NKDiscPort = "DEHAM";
			transport.JW_ETD = ZDateTime.Now;

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;
			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegOriginUNLOCO) }, Value = "TWKHH" });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.LegDestinationUNLOCO) }, Value = "DEHAM" });

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Terminal;

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();
			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			AssertNotNull(logParent);
		}

		public void TestGetLogParent_MatchUsingFuzzyVoyageFlight()
		{
			var date = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_VoyageFlight = "UA5421";
			transport1.JW_ETD = date;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_VoyageFlight = "UA5422";
			transport2.JW_ETD = date;

			Factory.Save();

			var incomingEvent = new Event();
			incomingEvent.EventType = AutoEvents.DepartureCode;

			incomingEvent.EventParameters = new EventParameters();
			incomingEvent.EventParameters.Facility = Constants.Facilities.Code.Terminal;

			incomingEvent.ContextCollection = new List<Context>();
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightDate) }, Value = date.ToString() });
			incomingEvent.ContextCollection.Add(new Context() { Type = new ContextType() { Type = nameof(ContextTypes.FlightNumber) }, Value = "UA5422T" });

			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);
			AssertEquals("Fuzzy match", transport2.PK, logParent.PK);
		}

		public void TestGetLogParent_IgnoreEvents()
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_IATA, "YUL");
			var unlocos = Factory.Load<RefUNLOCO>(query);

			AssertContainsExactElementsInAnyOrder("prerequisite", unlocos.Select(x => x.RL_Code), new[] { "CADOR", "CAYUL" });

			var consol1 = Factory.New<CommonConsol>();
			var transport1 = consol1.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "CADOR";
			transport1.JW_RL_NKDiscPort = "ZAJNB";
			consol1.JK_MasterBillNum = "7241971000";

			var consol2 = Factory.New<CommonConsol>();
			var transport2 = consol2.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "CAYUL";
			transport2.JW_RL_NKDiscPort = "ZAJNB";
			consol2.JK_MasterBillNum = "72419718790";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIgnoreEvent(consol2, Events.FreightLoadedCode);
				AssertIgnoreEvent(consol2, Events.ReceivedCode);
				AssertIgnoreEvent(consol2, Events.FreightUnloadedCode);
			});
		}

		public void TestGetLogParent_ByVessel_ActualDEP_NotMatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.DepartureCode, "VESSEL_AU", "DNN_VSSL", returnTransportAsParent: true, isEstimate: false, shouldUnlink: true, shouldLogSTU: true);

		public void TestGetLogParent_ByVessel_EstimateDEP_NotMatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.DepartureCode, "VESSEL_AU", "DNN_VSSL", returnTransportAsParent: true, isEstimate: true, shouldUnlink: true, shouldLogSTU: true);

		public void TestGetLogParent_ByVessel_ActualARV_NotMatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.ArrivalCode, "VESSEL_AU", "DNN_VSSL", returnTransportAsParent: true, isEstimate: false, shouldUnlink: true, shouldLogSTU: true);

		public void TestGetLogParent_ByVessel_EstimateARV_NotMatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.ArrivalCode, "VESSEL_AU", "DNN_VSSL", returnTransportAsParent: true, isEstimate: true, shouldUnlink: true, shouldLogSTU: true);

		public void TestGetLogParent_ByVessel_FLO_NotMatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.FreightLoadedCode, "VESSEL_AU", "DNN_VSSL", returnTransportAsParent: false, isEstimate: false, shouldUnlink: true, shouldLogSTU: true);

		public void TestGetLogParent_ByVessel_ActualDEP_WithoutVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.DepartureCode, "VESSEL_AU", string.Empty, returnTransportAsParent: true, isEstimate: false, shouldUnlink: true);

		public void TestGetLogParent_ByVessel_EstimateDEP_WithoutVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.DepartureCode, "VESSEL_AU", string.Empty, returnTransportAsParent: true, isEstimate: true, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_ActualARV_WithoutVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.ArrivalCode, "VESSEL_AU", string.Empty, returnTransportAsParent: true, isEstimate: false, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_EstimateARV_WithoutVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.ArrivalCode, "VESSEL_AU", string.Empty, returnTransportAsParent: true, isEstimate: true, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_FLO_WithoutVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.FreightLoadedCode, "VESSEL_AU", string.Empty, returnTransportAsParent: false, isEstimate: false, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_ActualDEP_MatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.DepartureCode, "VESSEL_AU", "VESSEL_AU", returnTransportAsParent: true, isEstimate: false, shouldUnlink: true);

		public void TestGetLogParent_ByVessel_EstimateDEP_MatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.DepartureCode, "VESSEL_AU", "VESSEL_AU", returnTransportAsParent: true, isEstimate: true, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_ActualARV_MatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.ArrivalCode, "VESSEL_AU", "VESSEL_AU", returnTransportAsParent: true, isEstimate: false, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_EstimateARV_MatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.ArrivalCode, "VESSEL_AU", "VESSEL_AU", returnTransportAsParent: true, isEstimate: true, shouldUnlink: false);

		public void TestGetLogParent_ByVessel_FLO_MatchVessel()
			=> AssertGetLogParent_ByVessel(eventCode: AutoEvents.FreightLoadedCode, "VESSEL_AU", "VESSEL_AU", returnTransportAsParent: false, isEstimate: false, shouldUnlink: false);

		void AssertIgnoreEvent(ITransportParent parent, string eventCode)
		{
			var repository = new MockRepository(MockBehavior.Default);
			var eventValueObjectMock = repository.Create<IXmlEventValueObject>();
			var contextValueListMock = repository.Create<IXmlEventValueObjectContextValueList>();

			contextValueListMock.Setup(m => m.OriginIATAAirportCode).Returns("YUL");
			contextValueListMock.Setup(m => m.DestinationIATAAirportCode).Returns("ZRH");

			eventValueObjectMock.Setup(m => m.Context).Returns(contextValueListMock.Object);
			eventValueObjectMock.Setup(m => m.EventType).Returns(eventCode);
			var eventLegPorts = new EventLegPorts(eventValueObjectMock.Object, Factory);
			var transportLinker = new TransportLinker();

			var logParent = transportLinker.GetLogParent(parent, eventValueObjectMock.Object, eventLegPorts);

			AssertNull(string.Format("expected to ignore {0} event", eventCode), logParent);
		}

		(CommonConsol, Transport) CreateSeaConsolAndTransportWithLinkedSailingSchedule(
			JobSailing sailing,
			ZDateTime date)
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_JX = sailing.PK;
			transport.JW_IsLinked = true;
			transport.JW_Vessel = sailing.JX_JV_NKVessel;
			transport.JW_ETD = date;
			transport.JW_ETA = date.AddDays(1);

			return (consol, transport);
		}

		Event CreateEventForModeSeaFacilityTerminal(ZString eventType, ZString origin, ZString destination, ZString vesselName, bool isEstimate = false)
		{
			var @event = new Event();
			@event.IsEstimate = isEstimate;
			@event.EventType = eventType;
			@event.EventParameters = new EventParameters
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				Facility = Constants.Facilities.Code.Terminal
			};

			@event.ContextCollection = new List<Context>()
			{
				new Context { Type = new ContextType { Type = nameof(ContextTypes.LegOriginUNLOCO) }, Value = origin },
				new Context { Type = new ContextType { Type = nameof(ContextTypes.LegDestinationUNLOCO) }, Value = destination },
				new Context { Type = new ContextType { Type = nameof(ContextTypes.VesselName) }, Value = vesselName }
			};

			return @event;
		}

		void AssertGetLogParent_ByVessel(string eventCode, string sailingVessel, string eventVessel, bool returnTransportAsParent, bool isEstimate, bool shouldUnlink, bool shouldLogSTU = false)
		{
			var date = ZDateTime.Now;
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "DENUE", "LULUX", sailingVessel, "VOY123");

			var (consol, transport1) = CreateSeaConsolAndTransportWithLinkedSailingSchedule(sailing, date);
			var (_, transport2) = CreateSeaConsolAndTransportWithLinkedSailingSchedule(sailing, date);

			Factory.Save();

			var incomingEvent = CreateEventForModeSeaFacilityTerminal(eventCode, "DENUE", "LULUX", eventVessel, isEstimate);
			var eventLegPorts = new EventLegPorts(incomingEvent, Factory);
			var transportLinker = new TransportLinker();
			var logParent = transportLinker.GetLogParent(consol, incomingEvent, eventLegPorts);

			if (returnTransportAsParent)
			{
				AssertNotNull(logParent);
				AssertEquals(transport1.PK, logParent.PK);
			}
			else
			{
				AssertNull(logParent);
			}

			AssertEquals(!shouldUnlink, transport1.JW_IsLinked);
			AssertSTUEvent(shouldLogSTU, transport1, $"|FAC=CTO|LOC=DENUE|MST=Container Automation|TYP=Change of Vessel Detected|VFL={eventVessel}");
			Assert("Consol 2 should remain linked.", transport2.JW_IsLinked);
		}

		void AssertSTUEvent(bool expectedSTU, Transport transport, ZString logReference)
		{
			var stuEvents = transport.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).ToArray();

			if (expectedSTU)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Should create STU event.", 1, stuEvents.Length);
					AssertEquals("STU log reference for Container Automation.", logReference, stuEvents.Single().SL_Reference);
				});
			}
			else
			{
				AssertEquals("Should not create STU event.", 0, stuEvents.Length);
			}
		}
	}
}
