using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Test;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportAirValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEstimatedDepartureAndArrival()
		{
			GenericValidateDateRangeForEstimatedDate(Transport.JW_ETDInfo, Transport.JW_ETAInfo);
		}

		void GenericValidateDateRangeForEstimatedDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			ZDateTime date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertHasWarning(arrivalInfo, "You have not entered an " + arrivalInfo.Description + ".");

			arrivalInfo.Value = new ZDateTime(date.AddDays(-2));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure > Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(2));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure < Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure < Arrival.", arrivalInfo);
		}

		public void TestValidateActualDepartureAndArrival()
		{
			GenericValidateDateRangeForActualDate(Transport.JW_ATDInfo, Transport.JW_ATAInfo);
		}

		void GenericValidateDateRangeForActualDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			ZDateTime date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertNoNotifications("Arrival Date empty. No error expected on Arrival", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(-2));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure > Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(2));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure < Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure < Arrival.", arrivalInfo);
		}

		public void TestValidateJW_VoyageFlight()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_VoyageFlight = "";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertHasErrors("Blank Flight - Error expected", Transport.JW_VoyageFlightInfo);

			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "USCHI";
			Transport.JW_VoyageFlight = "FLAV";
			AssertHasErrors("Garbage Flight - Linked - Has Error", Transport.JW_VoyageFlightInfo);
			AssertNoWarnings("Garbage Flight - Linked - No Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_RL_NKLoadPort = "USNYC";
			Transport.JW_RL_NKDiscPort = "USCHI";
			AssertNoErrors("Garbage Flight - Linked - No Errors", Transport.JW_VoyageFlightInfo);
			AssertHasWarnings("Garbage Flight - Linked - Has Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "FLAG";
			AssertNoErrors("Garbage Flight - Linked - No Errors", Transport.JW_VoyageFlightInfo);
			AssertHasWarnings("Garbage Flight - Linked - Has Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "QF415";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertNoErrors("Valid Flight - Linked - No Error", Transport.JW_VoyageFlightInfo);
			AssertNoWarnings("Valid Flight - Linked - No Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_IsLinked = false;
			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "USCHI";
			Transport.JW_VoyageFlight = "QF415";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertNoErrors("Valid Flight - Not Linked - No Error", Transport.JW_VoyageFlightInfo);
			AssertNoWarnings("Valid Flight - Not Linked - No Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertNoErrors("Blank Flight - Not Linked - No Error", Transport.JW_VoyageFlightInfo);
			AssertNoWarnings("Blank Flight - Not Linked - No Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "FLAV";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertNoErrors("Garbage Flight - Not Linked - No Error", Transport.JW_VoyageFlightInfo);
			AssertHasWarnings("Garbage Flight - Not Linked - Has Warning", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "QF415";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertNoErrors("Valid Flight - Not Linked - No Error", Transport.JW_VoyageFlightInfo);
			AssertNoWarnings("Valid Flight - Not Linked - No Warning", Transport.JW_VoyageFlightInfo);
		}

		#region TestValidateJW_VoyageFlight_ForTemplateRecord

		public void TestValidateJW_VoyageFlight_ForTemplateRecord_SingleLeg()
		{
			var sailing = CreateSailing("SQ22", "AUSYD", "HKHKG");
			var templateRecord = CreateTemplateRecord(sailing);

			var consolType = ObjectFactory.GetType(typeof(IForwardingConsol));
			var consol = (CommonConsol)Factory.NewWithValidTestData(consolType);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			var transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			var provider = consol as ITemplateRecordProvider;
			provider.TemplateRecord = templateRecord;
			provider.IsTemplateRecord = false;

			var message = "If the flight number is populated on the template, it may only be selected for that flight.";
			transport.Validation.ValidateJW_VoyageFlight();
			AssertNoWarnings(message, transport.JW_VoyageFlightInfo);

			var sailing2 = CreateSailing("CX100", "AUSYD", "HKHKG");
			transport.JW_JX = sailing2.PK;
			transport.Validation.ValidateJW_VoyageFlight();
			AssertHasWarnings(message, transport.JW_VoyageFlightInfo);
		}

		public void TestValidateJW_VoyageFlight_ForTemplateRecord_MultipleLegs()
		{
			var sailing = CreateSailing("SQ22", "AUSYD", "HKHKG");
			var templateRecord = CreateTemplateRecord(sailing);

			var consolType = ObjectFactory.GetType(typeof(IForwardingConsol));
			var consol = (CommonConsol)Factory.NewWithValidTestData(consolType);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";

			var transport1 = consol.Transports[0];
			transport1.JW_IsLinked = true;
			transport1.JW_JX = CreateSailing("SQ55", "SGSIN", "HKHKG").PK;
			transport1.JW_LegOrder = 3;
			AssertEquals("SQ55", transport1.JW_VoyageFlight);
			AssertEquals("SGSIN", transport1.JW_RL_NKLoadPort);
			AssertEquals("HKHKG", transport1.JW_RL_NKDiscPort);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_JX = CreateSailing("SQ44", "NZAKL", "SGSIN").PK;
			transport2.JW_LegOrder = 2;
			AssertEquals("SQ44", transport2.JW_VoyageFlight);
			AssertEquals("NZAKL", transport2.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport2.JW_RL_NKDiscPort);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3.JW_JX = CreateSailing("SQ33", "AUSYD", "NZAKL").PK;
			transport3.JW_LegOrder = 1;
			AssertEquals("SQ33", transport3.JW_VoyageFlight);
			AssertEquals("AUSYD", transport3.JW_RL_NKLoadPort);
			AssertEquals("NZAKL", transport3.JW_RL_NKDiscPort);

			var provider = consol as ITemplateRecordProvider;
			provider.TemplateRecord = templateRecord;
			provider.IsTemplateRecord = false;

			var message = "If the flight number is populated on the template, it may only be selected for that flight.";
			transport1.Validation.ValidateJW_VoyageFlight();
			AssertHasWarnings(message, transport1.JW_VoyageFlightInfo);

			transport2.Validation.ValidateJW_VoyageFlight();
			AssertNoWarnings(message, transport2.JW_VoyageFlightInfo);

			transport3.Validation.ValidateJW_VoyageFlight();
			AssertHasWarnings(message, transport3.JW_VoyageFlightInfo);
		}

		StmTemplateRecord CreateTemplateRecord(JobSailing sailing)
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";

			var consolType = ObjectFactory.GetType(typeof(IForwardingConsol));
			var consol = (CommonConsol)Factory.NewWithValidTestData(consolType);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.Transports[0].JW_JX = sailing.PK;

			var provider = consol as ITemplateRecordProvider;
			provider.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				provider.SaveToTemplateRecord();
			}

			return templateRecord;
		}

		JobSailing CreateSailing(ZString voyageFlight, ZString loadPort, ZString dischargePort)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = voyageFlight;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = ZDate.Today.AddDays(3);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		#endregion

		public void TestUsingCorrectValidation()
		{
			AssertEquals(typeof(TransportAirValidation), TransportValidation.New(Transport).GetType());
			AssertEquals(typeof(TransportAirValidation), Transport.Validation.GetType());
		}

		public void TestValidateScheduleArrivalDate()
		{
			var today = ZDateTime.Today;
			var sailing1 = GetNewAirSailing("QF1", "USLAX", today, today, "SGSIN", today, today.AddDays(1));
			var sailing2 = GetNewAirSailing("QF2", "USLAX", today, today, "SGSIN", today, today.AddDays(2));
			var sailing3 = GetNewAirSailing("QF3", "USLAX", today, today, "SGSIN", today, today.AddDays(-1));
			var sailing4 = GetNewAirSailing("QF4", "USLAX", today, today, "SGSIN", today, today.AddDays(-2));

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports[0];

			transport.JW_JX = sailing1.PK;
			AssertNoWarnings("STA should not have warnings.", transport.JW_STAInfo);

			transport.JW_JX = sailing2.PK;
			AssertHasWarning("STA should have warnings.", transport.JW_STAInfo, "STA is more than a day after ETA");

			transport.JW_JX = sailing3.PK;
			AssertNoWarnings("STA should not have warnings.", transport.JW_STAInfo);

			transport.JW_JX = sailing4.PK;
			AssertHasWarning("STA should have warnings.", transport.JW_STAInfo, "ETA is more than a day after STA");
		}

		public void TestValidateScheduleDepartureDate()
		{
			var today = ZDateTime.Today;
			var sailing1 = GetNewAirSailing("QF1", "USLAX", today, today.AddDays(1), "SGSIN", today, today);
			var sailing2 = GetNewAirSailing("QF2", "USLAX", today, today.AddDays(2), "SGSIN", today, today);
			var sailing3 = GetNewAirSailing("QF3", "USLAX", today, today.AddDays(-1), "SGSIN", today, today);
			var sailing4 = GetNewAirSailing("QF4", "USLAX", today, today.AddDays(-2), "SGSIN", today, today);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports[0];

			transport.JW_JX = sailing1.PK;
			AssertNoWarnings("STD should not have warnings.", transport.JW_STDInfo);

			transport.JW_JX = sailing2.PK;
			AssertHasWarning("STD should have warnings.", transport.JW_STDInfo, "STD is more than a day after ETD");

			transport.JW_JX = sailing3.PK;
			AssertNoWarnings("STD should not have warnings.", transport.JW_STDInfo);

			transport.JW_JX = sailing4.PK;
			AssertHasWarning("STD should have warnings.", transport.JW_STDInfo, "ETD is more than a day after STD");
		}

		JobSailing GetNewAirSailing(string voyageFlight, string portOfLoading, ZDateTime estimatedDeparture, ZDateTime scheduleDeparture, string portOfDischarge, ZDateTime estimatedArrival, ZDateTime scheduledArrival)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = voyageFlight;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = estimatedDeparture;
			origin.JA_S_DEP = scheduleDeparture;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = estimatedArrival;
			destination.JB_S_ARV = scheduledArrival;

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_NoWarnings()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(1);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertNoWarnings("Full Match - No warnings", transport.JW_OnlineScheduleStatusInfo);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_Unmatch_Generic()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Unmatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(ScheduleInfo.Empty);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");
			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF11";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_ETD = ZDate.Today.AddDays(1);
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_ETA = ZDate.Today.AddDays(2);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Unmatch - Generic Warning", transport.JW_OnlineScheduleStatusInfo, "This Air Routing Leg does not match any of the Global Flight Schedule flights. This flight may not be tracked correctly. You may want to check Global Flight Schedules for correct flight details by using \"Import Global Flights\" to search and import most appropriate flight.");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_PartialMatch_Generic()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;

			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.PartiallyMatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(ScheduleInfo.Empty);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF11";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_ETD = ZDate.Today.AddDays(1);
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_ETA = ZDate.Today.AddDays(2);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Partially Matched - Generic Warning", transport.JW_OnlineScheduleStatusInfo, "This Air Routing Leg only partially matches to a Global Flight Schedule flight. This flight may not be fully tracked correctly. You may want to check Global Flight Schedules for correct flight details by using \"Import Global Flights\" to search and import most appropriate flight.");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_Unmatch()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Unmatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF11";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_ETD = ZDate.Today.AddDays(1);
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_ETA = ZDate.Today.AddDays(2);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Unmatched - expected warning", transport.JW_OnlineScheduleStatusInfo, @"The following flight details do not match
Load Port: SYD
Departure Date: 06-Jun-18
Discharge Port: JFK
Arrival Date: 07-Jun-18
");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_DischargePortMismatch()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.PartiallyMatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "NZAKL";
				transport.JW_ETA = ZDate.Today.AddDays(1);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Partial match - expected warning", transport.JW_OnlineScheduleStatusInfo, @"The following flight details do not match
Discharge Port: JFK
");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_DischargePort_ETA_Mismatch()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.PartiallyMatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(3)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "NZAKL";
				transport.JW_ETA = ZDate.Today.AddDays(1);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Partial match - expected warning", transport.JW_OnlineScheduleStatusInfo, @"The following flight details do not match
Discharge Port: JFK
Arrival Date: 09-Jun-18
");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_LoadPort_Mismatch()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.PartiallyMatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(3)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(3);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Partial match - expected warning", transport.JW_OnlineScheduleStatusInfo, @"The following flight details do not match
Load Port: SYD
");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2018, 6, 6)]
		public void TestOnlineAirlineSchedulesValidation_LoadPort_ETD_Mismatch()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.PartiallyMatched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today.AddDays(2), "JFK", ZDate.Today.AddDays(3)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Factory.New<CommonShipment>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(3);

				transport.TryMatchAgainstOnlineFlights();

				transport.Validation.ValidateJW_OnlineScheduleStatus();

				AssertHasWarning("Partial match - expected warning", transport.JW_OnlineScheduleStatusInfo, @"The following flight details do not match
Load Port: SYD
Departure Date: 08-Jun-18
");
			}
		}

		#region Implementation

		void ValidateInfo(ZPropertyInfo info)
		{
			((IBusinessObjectInternals)info.BizObj).Validate(info);
		}

		Transport Transport
		{
			get
			{
				if (transport == null)
				{
					transport = Factory.New<CommonShipment>().Transports.AddNew();
					transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				}
				return transport;
			}
		}
		Transport transport;

		#endregion
	}
}
