using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration.Test;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class TransportEventTransformerTest : TestCaseWithFactory
	{
		public void TestTransform_DepartureToStatusUpdated()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var firstTransport = consol.Transports.Cast<Transport>().First();
			firstTransport.JW_ATD = ZDateTime.Now;

			var eventValue = new EventValue(Events.Departure, true, reference: "Dummy Description|FAC=CTO|LOC=AUSYD");
			var destEventValue = TransportEventTransformer.Transform(eventValue, null, firstTransport);

			AssertEquals("STU event", Events.StatusUpdatedCode, destEventValue.Code);
			AssertEquals("STU description", Events.StatusUpdated.Description, destEventValue.Description);
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETD rejected")));
			Assert("RES parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("RES", "ETD received after ATD")));
			Assert("LOC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("LOC", "AUSYD")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(destEventValue.Reference));
		}

		public void TestTransform_ArrivalToStatusUpdated()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var firstTransport = consol.Transports.Cast<Transport>().First();
			firstTransport.JW_ATA = ZDateTime.Now;

			var eventValue = new EventValue(Events.Arrival, true, reference: "Dummy Description|FAC=CTO|LOC=NZAKL");
			var destEventValue = TransportEventTransformer.Transform(eventValue, null, firstTransport);

			AssertEquals("STU event", Events.StatusUpdatedCode, destEventValue.Code);
			AssertEquals("STU description", Events.StatusUpdated.Description, destEventValue.Description);
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETA rejected")));
			Assert("RES parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("RES", "ETA received after ATA")));
			Assert("LOC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("LOC", "NZAKL")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(destEventValue.Reference));
		}

		public void TestTransform_BKC_AddEstimateToEventParametersFromEstimatedTimeOfArrivalOfContext()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var firstTransport = consol.Transports.Cast<Transport>().First();
			firstTransport.JW_ATA = ZDateTime.Now;

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
			universalEvent.EventTime = new ZDateTimeOffset(2023, 4, 9, 14, 0, 0);
			universalEvent.ContextCollection = new List<UniversalDataBuss.DataObjects.Universal.Context>
				{
					new UniversalDataBuss.DataObjects.Universal.Context()
					{
						Type = "EstimatedTimeOfArrival",
						Value = "09-Apr-23 14:00"
					}
				};

			var eventValue = new EventValue(Events.BookingConfirmed, true, reference: "Dummy Description|FAC=CTO|LOC=NZAKL");
			var destEventValue = TransportEventTransformer.Transform(eventValue, universalEvent, firstTransport);

			Assert("EST parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("ETA", "09-Apr-23 14:00")));
		}

		#region OnlineScheduleStatus

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Active()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Active Flight Advice|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Active, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_NotUpdated_StatusUpdated_FlightDateMismatches()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-03-01|LOC=NZAKL|MST=AWB Automation|TYP=Active Flight Advice|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Unmatched, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Active_PartialMatched()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Active Flight Advice|VFL=TG5",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Active, "TG005");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Cancelled()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Flight Cancelation Alert|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Cancelled, "TG5431");

			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Flight Cancellation Alert|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Cancelled, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Cancelled_PartialMatched()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Flight Cancelation Alert|VFL=TG5",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Cancelled, "TG05");

			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Flight Cancellation Alert|VFL=TG5",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Cancelled, "TG05");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_PreDeparture()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Pre-Departure Advice|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.PreDeparture, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_PreDeparture_PartialMatched()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Pre-Departure Advice|VFL=TG5",
				Events.StatusUpdated, Constants.FlightScheduleStatus.PreDeparture, "TG05");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_DepartureDelay()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Departure Delay Alert|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.DepartureDelay, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Departed_FromStatusUpdated()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Booked Flight Departed|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Departed, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Departed_FromDeparture()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=AUSYD|VFL=TG5431",
				Events.Departure, Constants.FlightScheduleStatus.Departed, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_NotUpdated_Departure_FlightDateMismatches()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-03-01|LOC=AUSYD|VFL=TG5431",
				Events.Departure, Constants.FlightScheduleStatus.Unmatched, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Departed_FromDeparture_PartialMatched()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=AUSYD|VFL=TG5",
				Events.Departure, Constants.FlightScheduleStatus.Departed, "TG005");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Diversion()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Flight Diversion Alert|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Diversion, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_ArrivalDelay()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|LOC=NZAKL|MST=AWB Automation|TYP=Arrival Delay Alert|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.ArrivalDelay, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_PreArrival()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|LOC=NZAKL|MST=AWB Automation|TYP=Pre-Arrival Advice|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.PreArrival, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_PreArrival_WhenEventParametersExist()
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "NZAKL";
			parameters[EventConstants.EventReferenceParameters.Codes.MessageType] = "AWB Automation";
			parameters[EventConstants.EventReferenceParameters.Codes.Type] = "Pre-Arrival Advice";
			parameters[EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber] = "TG5431";

			AssertOnlineScheduleStatus(string.Empty, Events.StatusUpdated, Constants.FlightScheduleStatus.PreArrival, "TG5431", false, parameters);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Arrived_FromStatusUpdated()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-25|LOC=NZAKL|MST=AWB Automation|TYP=Booked Flight Arrived|VFL=TG5431",
				Events.StatusUpdated, Constants.FlightScheduleStatus.Arrived, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Arrived_FromArrival()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-06-26|LOC=NZAKL|VFL=TG5431", Events.Arrival,
				Constants.FlightScheduleStatus.Arrived, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_Arrived_FromArrival_PartialMatched()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-06-26|LOC=NZAKL|VFL=TG5", Events.Arrival,
				Constants.FlightScheduleStatus.Arrived, "TG05");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_NotUpdated_Arrival_FlightDateMismatches()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-07-01|LOC=NZAKL|VFL=TG5", Events.Arrival,
				Constants.FlightScheduleStatus.Unmatched, "TG05");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_NotUpdated_SeaTransportMode()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-26|LOC=NZAKL|MST=AWB Automation|TYP=Booked Flight Arrived|VFL=TG1",
				Events.Arrival, Constants.FlightScheduleStatus.Unknown, "TG5431",
				transportMode: Constants.TransportModes.Sea);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_NotUpdated_FlightNumberMismatches()
		{
			AssertOnlineScheduleStatus(
				"Dummy Description|FAC=CTO|FDT=2020-06-26|LOC=NZAKL|MST=AWB Automation|TYP=Booked Flight Arrived|VFL=TG1",
				Events.Arrival, Constants.FlightScheduleStatus.Unmatched, "TG5431");
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestTransform_OnlineScheduleStatus_NotUpdated_EventIsEstimate()
		{
			AssertOnlineScheduleStatus("Dummy Description|FAC=CTO|FDT=2020-06-26|LOC=NZAKL|VFL=TG5431", Events.Arrival,
				Constants.FlightScheduleStatus.Unmatched, "TG5431", true);
		}

		void AssertOnlineScheduleStatus(string reference, Event eventValue, string status, string flightNumber, bool isEstimate = false,
			IDictionary<string, string> eventParameters = null,
			string transportMode = Constants.TransportModes.Air)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = transportMode == Constants.TransportModes.Air ? Constants.ContainerModes.LCL : Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew().Cast<Transport>().First();
			transport.JW_VoyageFlight = flightNumber;
			transport.JW_ETD = new ZDateTime(2020, 6, 25, 18, 0, 0);
			transport.JW_ETA = new ZDateTime(2020, 6, 26, 3, 0, 0);

			var eventToUpdate = new EventValue(eventValue, isEstimate, reference: reference, parameters: eventParameters);

			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			AssertEquals("Online Schedule Status", status, transport.JW_OnlineScheduleStatus);
		}

		#endregion

		#region VesselEventDates

		public void TestTransform_VesselEventDates_Terminal_CargoAvailable()
		{
			TestTransform_VesselEventDates_Updated(Events.CargoAvailable, "NZAKL", EventConstants.Facilities.Code.Terminal);
		}

		public void TestTransform_VesselEventDates_Terminal_CutOff()
		{
			TestTransform_VesselEventDates_Updated(Events.CutOffDate, "AUSYD", EventConstants.Facilities.Code.Terminal);
		}

		public void TestTransform_VesselEventDates_Terminal_ReceiptCommenced()
		{
			TestTransform_VesselEventDates_Updated(Events.ReceiptCommenced, "AUSYD", EventConstants.Facilities.Code.Terminal);
		}

		public void TestTransform_VesselEventDates_Terminal_StorageCommenced()
		{
			TestTransform_VesselEventDates_Updated(Events.StorageCommenced, "NZAKL", EventConstants.Facilities.Code.Terminal);
		}

		public void TestTransform_VesselEventDates_Depot_CargoAvailable()
		{
			TestTransform_VesselEventDates_Updated(Events.CargoAvailable, "NZAKL", EventConstants.Facilities.Code.Depot);
		}

		public void TestTransform_VesselEventDates_Depot_CutOff()
		{
			TestTransform_VesselEventDates_Updated(Events.CutOffDate, "AUSYD", EventConstants.Facilities.Code.Depot);
		}

		public void TestTransform_VesselEventDates_Depot_ReceiptCommenced()
		{
			TestTransform_VesselEventDates_Updated(Events.ReceiptCommenced, "AUSYD", EventConstants.Facilities.Code.Depot);
		}

		public void TestTransform_VesselEventDates_Depot_StorageCommenced()
		{
			TestTransform_VesselEventDates_Updated(Events.StorageCommenced, "NZAKL", EventConstants.Facilities.Code.Depot);
		}

		void TestTransform_VesselEventDates_Updated(Event eventValue, string location, string facility)
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = location;
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = facility;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew().Cast<Transport>().First();
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;

			var eventDate = new ZDateTimeOffset(2020, 6, 9, 6, 9, 0);
			var eventToUpdate = new EventValue(eventValue, eventTime: eventDate, parameters: parameters);
			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			var actualDate = GetTransportVesselEventDateFromParameters(eventValue.Code, facility, transport);
			AssertEquals("Date targeted should be updated", eventDate.ToZDateTime(), actualDate);
		}

		ZDateTime GetTransportVesselEventDateFromParameters(string eventType, string facility, Transport transport)
		{
			var isTerminalFacility = facility == EventConstants.Facilities.Code.Terminal;
			switch (eventType)
			{
				case Events.CargoAvailableCode:
					return isTerminalFacility ? transport.JW_TerminalAvailabilityDate : transport.JW_DepotAvailabilityDate;
				case Events.CutOffDateCode:
					return isTerminalFacility ? transport.JW_TerminalCutOff : transport.JW_DepotCutOff;
				case Events.ReceiptCommencedCode:
					return isTerminalFacility ? transport.JW_TerminalReceivalCommences : transport.JW_DepotReceivalCommences;
				case Events.StorageCommencedCode:
					return isTerminalFacility ? transport.JW_TerminalStorageDate : transport.JW_DepotStorageDate;
				default:
					return ZDateTime.Empty;
			}
		}

		public void TestTransform_VesselEventDates_NotUpdated_WhenLocationDoesNotMatchTransport()
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew().Cast<Transport>().First();
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;

			var eventTime = new ZDateTimeOffset(2020, 6, 9, 6, 9, 0);
			var eventToUpdate = new EventValue(Events.CargoAvailable, eventTime: eventTime, parameters: parameters);
			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			AssertVesselEventDatesAreNotUpdated(transport);

			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "NZAKL";

			eventToUpdate = new EventValue(Events.CutOffDate, eventTime: eventTime, parameters: parameters);
			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			AssertVesselEventDatesAreNotUpdated(transport);
		}

		public void TestTransform_VesselEventDates_NotUpdated_WhenNotVesselEvent()
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "NZAKL";
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew().Cast<Transport>().First();
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;

			var eventTime = new ZDateTimeOffset(2020, 6, 9, 6, 9, 0);
			var eventToUpdate = new EventValue(Events.Arrival, eventTime: eventTime, parameters: parameters);
			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			AssertVesselEventDatesAreNotUpdated(transport);
		}

		void AssertVesselEventDatesAreNotUpdated(Transport transport)
		{
			AssertEquals(ZDateTime.Empty, transport.JW_TerminalAvailabilityDate);
			AssertEquals(ZDateTime.Empty, transport.JW_TerminalCutOff);
			AssertEquals(ZDateTime.Empty, transport.JW_TerminalReceivalCommences);
			AssertEquals(ZDateTime.Empty, transport.JW_TerminalStorageDate);
			AssertEquals(ZDateTime.Empty, transport.JW_DepotAvailabilityDate);
			AssertEquals(ZDateTime.Empty, transport.JW_DepotCutOff);
			AssertEquals(ZDateTime.Empty, transport.JW_DepotReceivalCommences);
			AssertEquals(ZDateTime.Empty, transport.JW_DepotStorageDate);
		}

		static readonly string[] notUpdateTestEventTypes =
		{
			"Empty",
			"VGM",
			"Reefer",
			"Haz",
		};

		public void TestTransform_CutOffDates_NotUpdated_WhenUpdateVGMOrReeferOrHazOrEmpty()
		{
			foreach (string eventType in notUpdateTestEventTypes)
			{
				AssertCutOffDatesNotUpdated(eventType);
			}
		}

		void AssertCutOffDatesNotUpdated(string eventType)
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
			parameters[EventConstants.EventReferenceParameters.Codes.Type] = eventType;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew().Cast<Transport>().First();
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;

			var eventTime = new ZDateTimeOffset(2020, 6, 9, 6, 9, 0);
			var eventToUpdate = new EventValue(Events.CutOffDate, eventTime: eventTime, parameters: parameters);
			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			AssertEquals(ZDateTime.Empty, transport.JW_TerminalCutOff);
		}

		public void TestTransform_ReceivalDates_NotUpdated_WhenUpdateVGMOrReeferOrHazOrEmpty()
		{
			foreach (string eventType in notUpdateTestEventTypes)
			{
				AssertReceivalDatesNotUpdated(eventType);
			}
		}

		void AssertReceivalDatesNotUpdated(string eventType)
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
			parameters[EventConstants.EventReferenceParameters.Codes.Type] = eventType;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.AddNew().Cast<Transport>().First();
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;

			var eventTime = new ZDateTimeOffset(2020, 6, 9, 6, 9, 0);
			var eventToUpdate = new EventValue(Events.ReceiptCommenced, eventTime: eventTime, parameters: parameters);
			TransportEventTransformer.Transform(eventToUpdate, null, transport);

			AssertEquals(ZDateTime.Empty, transport.JW_TerminalReceivalCommences);
		}

		#endregion
	}
}
