using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AirlineTrackingEventHandler))]
	sealed class AirlineTrackingEventHandlerTest : TestCaseWithFactory
	{
		#region Process Log

		public void TestProcessLog_UpdateDeliveredTime()
		{
			var eventTime = ZDateTime.Now;
			AssertProcessEvent_UpdateDeliveredTime(Constants.TransportModes.Air, eventTime);
			AssertProcessEvent_UpdateDeliveredTime(Constants.TransportModes.Sea, ZDateTime.Empty);

			void AssertProcessEvent_UpdateDeliveredTime(ZString transportMode, ZDateTime expectedDeliveredTime)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = transportMode;

				AddEventLog(consol,
					Events.HandedOverCode,
					ZString.Empty,
					eventTime,
					false);
				AssertEquals(expectedDeliveredTime, consol.ShipmentDeliveredTime);

				var factory2 = new BusinessObjectFactory();
				var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
				((IAirlineTrackingEventProvider)consol2).UpdateShipmentDeliveredTime();
				AssertEquals(expectedDeliveredTime, consol2.ShipmentDeliveredTime);
				AssertEquals(false, consol2.HasChanges);
			}
		}

		public void TestProcessLog_UpdateBookingConfirmations()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=15-MAY-22 19:00|ETA=16-MAY-22 14:00",
				eventTime,
				false);

			var arrivalTimeFromBkc1 = new ZDateTime(DateTime.ParseExact("16-MAY-22 14:00", "dd-MMM-yy HH:mm", CultureInfo.InvariantCulture));
			AssertEquals(1, consol.BookingConfirmations.Count);
			var bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals(Events.BookingConfirmedCode, bookingConfirmation.EventCode);
			AssertEquals("AUADL", bookingConfirmation.LoadPort);
			AssertEquals("AUMEL", bookingConfirmation.DischargePort);
			AssertEquals("UA6800T", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15), bookingConfirmation.ScheduleDate);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 0, 0), bookingConfirmation.DepartureTime);
			AssertEquals(arrivalTimeFromBkc1, bookingConfirmation.ArrivalTime);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(1),
				true);

			AssertEquals(1, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("Estimated Departure event won't be processed", Events.BookingConfirmedCode, bookingConfirmation.EventCode);
			AssertEquals("AUADL", bookingConfirmation.LoadPort);
			AssertEquals("AUMEL", bookingConfirmation.DischargePort);
			AssertEquals("UA6800T", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15), bookingConfirmation.ScheduleDate);
			AssertEquals("DepartureTime should not be changed", new ZDateTime(2022, 5, 15, 19, 0, 0), bookingConfirmation.DepartureTime);
			AssertEquals(arrivalTimeFromBkc1, bookingConfirmation.ArrivalTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=16-MAY-22",
				eventTime.AddHours(12),
				true);

			AssertEquals(1, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("AUADL", bookingConfirmation.LoadPort);
			AssertEquals("AUMEL", bookingConfirmation.DischargePort);
			AssertEquals("UA6800T", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 0, 0), bookingConfirmation.DepartureTime);
			AssertEquals("Arrival time should not be updated", arrivalTimeFromBkc1, bookingConfirmation.ArrivalTime);

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=15|FDT=16-MAY-22 05:30|ETA=16-May-22 18:00",
				eventTime.AddHours(3),
				false);

			var arrivalTimeFromBkc2 = new ZDateTime(DateTime.ParseExact("16-May-22 18:00", "dd-MMM-yy HH:mm", CultureInfo.InvariantCulture));
			AssertEquals(2, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[1];
			AssertEquals("AUMEL", bookingConfirmation.LoadPort);
			AssertEquals("NZAKL", bookingConfirmation.DischargePort);
			AssertEquals("UA6780", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 5, 30, 0), bookingConfirmation.DepartureTime);
			AssertEquals(arrivalTimeFromBkc2, bookingConfirmation.ArrivalTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=15|FDT=16-MAY-22",
				eventTime.AddHours(4),
				true);

			AssertEquals(2, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[1];
			AssertEquals("AUMEL", bookingConfirmation.LoadPort);
			AssertEquals("NZAKL", bookingConfirmation.DischargePort);
			AssertEquals("UA6780", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 5, 30, 0), bookingConfirmation.DepartureTime);
			AssertEquals("Arrival time should not be updated", arrivalTimeFromBkc2, bookingConfirmation.ArrivalTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=15|FDT=15-MAY-22",
				eventTime.AddHours(3.5),
				true);

			AssertEquals(2, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[1];
			AssertEquals("AUMEL", bookingConfirmation.LoadPort);
			AssertEquals("NZAKL", bookingConfirmation.DischargePort);
			AssertEquals("UA6780", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 5, 30, 0), bookingConfirmation.DepartureTime);
			AssertEquals("Arrival time should not be updated for second Arrival event", arrivalTimeFromBkc2, bookingConfirmation.ArrivalTime);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertEquals(2, consol2.BookingConfirmations.Count);

			bookingConfirmation = consol2.BookingConfirmations[0];
			AssertEquals("AUADL", bookingConfirmation.LoadPort);
			AssertEquals("AUMEL", bookingConfirmation.DischargePort);
			AssertEquals("UA6800T", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 0, 0), bookingConfirmation.DepartureTime);
			AssertEquals(arrivalTimeFromBkc1, bookingConfirmation.ArrivalTime);

			bookingConfirmation = consol2.BookingConfirmations[1];
			AssertEquals("AUMEL", bookingConfirmation.LoadPort);
			AssertEquals("NZAKL", bookingConfirmation.DischargePort);
			AssertEquals("UA6780", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)15, bookingConfirmation.BookedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 5, 30, 0), bookingConfirmation.DepartureTime);
			AssertEquals("SL_PostedTimeUtc should be used to sort", arrivalTimeFromBkc2, bookingConfirmation.ArrivalTime);

			AssertEquals("consol2 should not have any change", false, consol2.HasChanges);
		}

		public void TestProcessLog_UpdateBookingConfirmations_NoBookingConfirmed()
		{
			var eventTime = ZDateTime.Now;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=15-MAY-22 19:10",
				eventTime.AddHours(1),
				true);
			AssertEquals("No BookingConfirmed event", 0, consol.BookingConfirmations.Count);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=16-MAY-22 05:40",
				eventTime.AddHours(2),
				true);
			AssertEquals("No BookingConfirmed event", 0, consol.BookingConfirmations.Count);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertEquals("No BookingConfirmed event", 0, consol2.BookingConfirmations.Count);
			AssertEquals("consol2 should not have any change", false, consol2.HasChanges);
		}

		public void TestProcessLog_UpdateBookingConfirmations_NoBookingConfirmed_WhenFlightNumberAndDateMismatched()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 0, 0);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=15-MAY-22 19:10",
				eventTime.AddHours(1),
				true);
			AssertEquals("No BookingConfirmed event", 0, consol.BookingConfirmations.Count);

			var arrivalTimeFromBkc1 = new ZDateTime(DateTime.ParseExact("16-MAY-22 18:00", "dd-MMM-yy HH:mm", CultureInfo.InvariantCulture));
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=16-MAY-22 05:40|ETA=16-MAY-22 18:00",
				eventTime.AddHours(2),
				false);
			AssertEquals("BookingConfirmation added", 1, consol.BookingConfirmations.Count);
			AssertEquals("ArrivalTime update based on ETA parameter", arrivalTimeFromBkc1, consol.BookingConfirmations[0].ArrivalTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6780|PTL=15|TTL=20|FDT=16-MAY-22 19:10",
				eventTime.AddHours(3),
				true);
			AssertEquals("No new BookingConfirmation added when flight number and schedule date mismatched", 1, consol.BookingConfirmations.Count);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=15|TTL=20|FDT=16-MAY-22 05:40",
				eventTime.AddHours(4),
				true);
			AssertEquals(1, consol.BookingConfirmations.Count);
			AssertEquals("UA6800T", consol.BookingConfirmations[0].VoyageFlight);
			AssertEquals("ArrivalTime should not be updated by arrival event", arrivalTimeFromBkc1, consol.BookingConfirmations[0].ArrivalTime);

			var arrivalTimeFromBkc2 = new ZDateTime(DateTime.ParseExact("15-MAY-22 23:00", "dd-MMM-yy HH:mm", CultureInfo.InvariantCulture));
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6780|PTL=15|TTL=20|FDT=15-MAY-22 19:20|ETA=15-MAY-22 23:00",
				eventTime.AddHours(5),
				false);
			AssertEquals("New BookingConfirmation added", 2, consol.BookingConfirmations.Count);
			AssertEquals(arrivalTimeFromBkc2, consol.BookingConfirmations[1].ArrivalTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6780|PTL=15|TTL=20|FDT=15-MAY-22 19:20",
				eventTime.AddHours(6),
				true);
			AssertEquals(2, consol.BookingConfirmations.Count);
			AssertEquals("UA6780", consol.BookingConfirmations[1].VoyageFlight);
			AssertEquals("Arrival time should not be updated", arrivalTimeFromBkc2, consol.BookingConfirmations[1].ArrivalTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6780|PTL=15|TTL=20|FDT=16-MAY-22 01:20",
				eventTime.AddHours(7),
				true);
			AssertEquals(2, consol.BookingConfirmations.Count);
			AssertEquals("UA6780", consol.BookingConfirmations[1].VoyageFlight);
			AssertEquals("Arrival time is updated", arrivalTimeFromBkc2, consol.BookingConfirmations[1].ArrivalTime);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertEquals(2, consol2.BookingConfirmations.Count);
			AssertEquals("UA6800T", consol.BookingConfirmations[0].VoyageFlight);
			AssertEquals(arrivalTimeFromBkc1, consol.BookingConfirmations[0].ArrivalTime);
			AssertEquals("UA6780", consol.BookingConfirmations[1].VoyageFlight);
			AssertEquals(arrivalTimeFromBkc2, consol.BookingConfirmations[1].ArrivalTime);
			AssertEquals("consol2 should not have any change", false, consol2.HasChanges);
		}

		public void TestProcessLog_UpdateActualEvents()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 10, 0);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22|ETA=15-MAY-22",
				eventTime.AddHours(1),
				false);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(1),
				false);

			AssertEquals(1, consol.ActualEvents.Count);
			var actualEvent = consol.ActualEvents[0];
			AssertEquals("AUADL", actualEvent.LoadPort);
			AssertEquals("AUMEL", actualEvent.DischargePort);
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("0", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15), actualEvent.ScheduleDate);
			AssertEquals("DepartedTime should be updated", new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6888T|PTL=10|TTL=20|FDT=15-MAY-22|ETA=15-MAY-22",
				eventTime.AddHours(2),
				false);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6888T|PTL=10|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(2),
				false);

			AssertEquals(2, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[1];
			AssertEquals("AUADL", actualEvent.LoadPort);
			AssertEquals("AUMEL", actualEvent.DischargePort);
			AssertEquals("UA6888T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("0", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(3),
				false);

			AssertEquals(2, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[0];
			AssertEquals("AUADL", actualEvent.LoadPort);
			AssertEquals("AUMEL", actualEvent.DischargePort);
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)10, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("10", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 22, 10, 0), actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.FreightUnloadedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6888T|PTL=10|TTL=20|FDT=15-MAY-22",
				new ZDateTime(2022, 5, 16, 9, 55, 0),
				false);

			AssertEquals(2, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[1];
			AssertEquals("AUADL", actualEvent.LoadPort);
			AssertEquals("AUMEL", actualEvent.DischargePort);
			AssertEquals("UA6888T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)10, actualEvent.UnloadedPieces);
			AssertEquals("0/10", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=20|FDT=16-MAY-22|ETA=16-MAY-22",
				eventTime.AddHours(5),
				false);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=20|FDT=16-MAY-22",
				eventTime.AddHours(5),
				false);

			AssertEquals(3, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[2];
			AssertEquals("AUMEL", actualEvent.LoadPort);
			AssertEquals("NZAKL", actualEvent.DischargePort);
			AssertEquals("UA6780", actualEvent.VoyageFlight);
			AssertEquals((ZInt)20, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("0", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 0, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=20|FDT=16-MAY-22",
				eventTime.AddHours(6),
				false);

			AssertEquals(3, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[2];
			AssertEquals("AUMEL", actualEvent.LoadPort);
			AssertEquals("NZAKL", actualEvent.DischargePort);
			AssertEquals("UA6780", actualEvent.VoyageFlight);
			AssertEquals((ZInt)20, actualEvent.ShippedPieces);
			AssertEquals((ZInt)20, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("20", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 0, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 16, 1, 10, 0), actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUMEL|TO=NZAKL|VFL=UA6780|TTL=20|FDT=16-MAY-22",
				eventTime.AddHours(5.5),
				false);

			AssertEquals(3, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[2];
			AssertEquals("AUMEL", actualEvent.LoadPort);
			AssertEquals("NZAKL", actualEvent.DischargePort);
			AssertEquals("UA6780", actualEvent.VoyageFlight);
			AssertEquals((ZInt)20, actualEvent.ShippedPieces);
			AssertEquals((ZInt)20, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("20", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 0, 10, 0), actualEvent.DepartedTime);
			AssertEquals("Arrived time should be updated by most recent event", new ZDateTime(2022, 5, 16, 0, 40, 0), actualEvent.ArrivedTime);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertEquals(3, consol2.ActualEvents.Count);

			actualEvent = consol2.ActualEvents[0];
			AssertEquals("AUADL", actualEvent.LoadPort);
			AssertEquals("AUMEL", actualEvent.DischargePort);
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)10, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("10", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 22, 10, 0), actualEvent.ArrivedTime);

			actualEvent = consol2.ActualEvents[1];
			AssertEquals("AUADL", actualEvent.LoadPort);
			AssertEquals("AUMEL", actualEvent.DischargePort);
			AssertEquals("UA6888T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)10, actualEvent.UnloadedPieces);
			AssertEquals("0/10", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			actualEvent = consol2.ActualEvents[2];
			AssertEquals("AUMEL", actualEvent.LoadPort);
			AssertEquals("NZAKL", actualEvent.DischargePort);
			AssertEquals("UA6780", actualEvent.VoyageFlight);
			AssertEquals((ZInt)20, actualEvent.ShippedPieces);
			AssertEquals((ZInt)20, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals("20", actualEvent.ArrivedAndUnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 16, 0, 10, 0), actualEvent.DepartedTime);
			AssertEquals("SL_PostedTimeUtc should be used to sort", new ZDateTime(2022, 5, 16, 0, 40, 0), actualEvent.ArrivedTime);

			AssertEquals("Consol2 should not have any change", false, consol2.HasChanges);
		}

		public void TestProcessLog_FreightUnloaded_UnloadedPiecesAddedTogether()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 10, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22|ETA=15-MAY-22",
				eventTime,
				false);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22",
				eventTime,
				false);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(1),
				false);

			AssertEquals(1, consol.ActualEvents.Count);
			var actualEvent = consol.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)10, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.FreightUnloadedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=3|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(3),
				false);

			AssertEquals(1, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)10, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)3, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.ArrivedTime);

			AddEventLog(consol,
				Events.FreightUnloadedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6780|PTL=6|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(3.5),
				false);

			AddEventLog(consol,
				Events.FreightUnloadedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=6|TTL=20|FDT=15-MAY-22",
				eventTime.AddHours(4),
				false);

			AssertEquals(2, consol.ActualEvents.Count);
			actualEvent = consol.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)10, actualEvent.ArrivedPieces);
			AssertEquals("Unloaded pieces should be added", (ZInt)9, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.ArrivedTime);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);

			AssertEquals(2, consol2.ActualEvents.Count);
			actualEvent = consol2.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)10, actualEvent.ArrivedPieces);
			AssertEquals("Unloaded pieces should be added", (ZInt)9, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 20, 10, 0), actualEvent.ArrivedTime);
		}

		public void TestProcessLog_Departure_ShippedPiecesAddedTogether()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 10, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22 19:10",
				eventTime,
				false);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=5|TTL=20|FDT=15-MAY-22 19:10",
				eventTime.AddHours(1),
				false);

			AssertEquals(1, consol.ActualEvents.Count);
			var actualEvent = consol.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)5, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);

			AssertEquals(1, consol2.ActualEvents.Count);
			actualEvent = consol2.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)5, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
		}

		public void TestProcessLog_Arrival_ArrivedPiecesAddedTogether_WhenBKCEventExists()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 10, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22 19:10|ETA=16-MAY-22 03:50",
				eventTime,
				false);

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22 19:10",
				eventTime,
				false);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=1|TTL=20|FDT=16-MAY-22 02:45",
				eventTime.AddHours(1),
				false);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=5|TTL=20|FDT=16-MAY-22 03:45",
				eventTime.AddHours(2),
				false);

			AddEventLog(consol,
				Events.FreightUnloadedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=3|TTL=20|FDT=16-MAY-22 04:00",
				eventTime.AddHours(3),
				false);

			AssertEquals(1, consol.ActualEvents.Count);
			var actualEvent = consol.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)5, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)3, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), actualEvent.ArrivedTime);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);

			AssertEquals(1, consol2.ActualEvents.Count);
			actualEvent = consol2.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)5, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)3, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), actualEvent.ArrivedTime);
		}

		public void TestProcessLog_Arrival_ArrivedPiecesAddedTogether_WhenBKCEventIsMissing()
		{
			var eventTime = new ZDateTime(2022, 5, 15, 19, 10, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.DepartureCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=10|TTL=20|FDT=15-MAY-22 19:10",
				eventTime,
				false);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=1|TTL=20|FDT=15-MAY-22 20:45",
				eventTime.AddHours(1),
				false);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=5|TTL=20|FDT=15-MAY-22 20:45",
				eventTime.AddHours(2),
				false);

			AddEventLog(consol,
				Events.FreightUnloadedCode,
				"|FRM=AUADL|TO=AUMEL|VFL=UA6800T|PTL=3|TTL=20|FDT=15-MAY-22 21:00",
				eventTime.AddHours(3),
				false);

			AssertEquals(2, consol.ActualEvents.Count);
			var actualEvent = consol.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			var secondActualEvent = consol.ActualEvents[1];
			AssertEquals("UA6800T", secondActualEvent.VoyageFlight);
			AssertEquals((ZInt)0, secondActualEvent.ShippedPieces);
			AssertEquals((ZInt)5, secondActualEvent.ArrivedPieces);
			AssertEquals((ZInt)3, secondActualEvent.UnloadedPieces);
			AssertEquals(ZDateTime.Empty, secondActualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), secondActualEvent.ArrivedTime);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol.PK);

			AssertEquals(2, consol2.ActualEvents.Count);
			actualEvent = consol2.ActualEvents[0];
			AssertEquals("UA6800T", actualEvent.VoyageFlight);
			AssertEquals((ZInt)10, actualEvent.ShippedPieces);
			AssertEquals((ZInt)0, actualEvent.ArrivedPieces);
			AssertEquals((ZInt)0, actualEvent.UnloadedPieces);
			AssertEquals(new ZDateTime(2022, 5, 15, 19, 10, 0), actualEvent.DepartedTime);
			AssertEquals(ZDateTime.Empty, actualEvent.ArrivedTime);

			secondActualEvent = consol2.ActualEvents[1];
			AssertEquals("UA6800T", secondActualEvent.VoyageFlight);
			AssertEquals((ZInt)0, secondActualEvent.ShippedPieces);
			AssertEquals((ZInt)5, secondActualEvent.ArrivedPieces);
			AssertEquals((ZInt)3, secondActualEvent.UnloadedPieces);
			AssertEquals(ZDateTime.Empty, secondActualEvent.DepartedTime);
			AssertEquals(new ZDateTime(2022, 5, 15, 21, 10, 0), secondActualEvent.ArrivedTime);
		}

		public void TestMatchAndUpdateRoutingLeg_BookingConfirmation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETD = new ZDateTime(2023, 5, 17, 9, 0, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 17, 20, 0, 0);

			var bookingConfirmation = new BookingConfirmation
			{
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				VoyageFlight = "QF100",
				BookedPieces = 20,
				DepartureTime = new ZDateTime(2023, 5, 17, 9, 10, 0),
				ArrivalTime = new ZDateTime(2023, 5, 17, 20, 10, 0)
			};

			var provider = consol as IAirlineTrackingEventProvider;
			var matched = provider.MatchAndUpdateRoutingLeg(bookingConfirmation, () => true);
			AssertEquals(false, matched);
			AssertEquals("Mismatch and no update", "QF123", transport.JW_VoyageFlight);
			AssertEquals("Mismatch and no update", new ZDateTime(2023, 5, 17, 9, 0, 0), transport.JW_STD);
			AssertEquals("Mismatch and no update", new ZDateTime(2023, 5, 17, 20, 0, 0), transport.JW_STA);

			bookingConfirmation.DischargePort = "HKHKG";
			matched = provider.MatchAndUpdateRoutingLeg(bookingConfirmation, () => false);
			AssertEquals(true, matched);
			AssertEquals("Matched but update is cancelled", "QF123", transport.JW_VoyageFlight);
			AssertEquals("Matched but update is cancelled", new ZDateTime(2023, 5, 17, 9, 0, 0), transport.JW_STD);
			AssertEquals("Matched but update is cancelled", new ZDateTime(2023, 5, 17, 20, 0, 0), transport.JW_STA);

			matched = provider.MatchAndUpdateRoutingLeg(bookingConfirmation, () => true);
			AssertEquals(true, matched);
			AssertEquals("Matched and updated", "QF100", transport.JW_VoyageFlight);
			AssertEquals("Matched and updated", new ZDateTime(2023, 5, 17, 9, 10, 0), transport.JW_STD);
			AssertEquals("Matched and updated", new ZDateTime(2023, 5, 17, 20, 10, 0), transport.JW_STA);

			var cidLog = consol.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier);
			AssertNotNull(cidLog);
			AssertEquals("|NEW=QF100,17-May-23 09:10,17-May-23 20:10|OLD=QF123,17-May-23 09:00,17-May-23 20:00|RES=Updated from Booking Confirmation|TYP=Routing Leg", cidLog.SL_Reference);
		}

		public void TestMatchAndUpdateRoutingLeg_ActualEvent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETD = new ZDateTime(2023, 5, 17, 9, 0, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 17, 20, 0, 0);

			var actualEvent = new ActualEvent
			{
				EventCode = Events.ArrivalCode,
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				VoyageFlight = "QF100",
				ShippedPieces = 20,
				DepartedTime = new ZDateTime(2023, 5, 17, 9, 10, 0),
				ArrivedPieces = 20,
				ArrivedTime = new ZDateTime(2023, 5, 17, 20, 10, 0)
			};

			var provider = consol as IAirlineTrackingEventProvider;
			var matched = provider.MatchAndUpdateRoutingLeg(actualEvent, () => true);
			AssertEquals(false, matched);
			AssertEquals("Mismatch and no update", "QF123", transport.JW_VoyageFlight);
			AssertEquals("Mismatch and no update", ZDateTime.Empty, transport.JW_ATD);
			AssertEquals("Mismatch and no update", ZDateTime.Empty, transport.JW_ATA);

			actualEvent.DischargePort = "HKHKG";
			matched = provider.MatchAndUpdateRoutingLeg(actualEvent, () => false);
			AssertEquals(true, matched);
			AssertEquals("Matched but update is cancelled", "QF123", transport.JW_VoyageFlight);
			AssertEquals("Matched but update is cancelled", ZDateTime.Empty, transport.JW_ATD);
			AssertEquals("Matched but update is cancelled", ZDateTime.Empty, transport.JW_ATA);

			matched = provider.MatchAndUpdateRoutingLeg(actualEvent, () => true);
			AssertEquals(true, matched);
			AssertEquals("Matched and updated", "QF100", transport.JW_VoyageFlight);
			AssertEquals("Matched and updated", new ZDateTime(2023, 5, 17, 9, 10, 0), transport.JW_ATD);
			AssertEquals("Matched and updated", new ZDateTime(2023, 5, 17, 20, 10, 0), transport.JW_ATA);

			var cidLog = consol.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier);
			AssertNotNull(cidLog);
			AssertEquals("|EVT=ARV|NEW=QF100,17-May-23 09:10,17-May-23 20:10|OLD=QF123,,|RES=Updated from Actual Events|TYP=Routing Leg", cidLog.SL_Reference);
		}

		public void TestProcessLog_UpdateBookingConfirmations_AutoUpdateTransport()
		{
			using (FreightDataRegistry.Instance.AutomaticUpdatingofPlannedLegs_Air.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "HKHKG";
				transport.JW_VoyageFlight = "QF123";
				transport.JW_ETD = new ZDateTime(2023, 5, 17, 9, 0, 0);
				transport.JW_ETA = new ZDateTime(2023, 5, 17, 20, 0, 0);

				var eventTime = new ZDateTime(2023, 5, 17, 9, 10, 0);
				AddEventLog(consol,
					Events.BookingConfirmedCode,
					"|FRM=AUSYD|TO=HKHKG|VFL=QF100|PTL=15|TTL=20|FDT=17-MAY-23 09:10|ETA=17-May-23 11:00",
					eventTime,
					false);

				AssertEquals("Matched and auto updated", "QF100", transport.JW_VoyageFlight);
				AssertEquals("Matched and auto updated", new ZDateTime(2023, 5, 17, 9, 10, 0), transport.JW_STD);
				AssertEquals("Arrival time update based on ETA parameter", new ZDateTime(2023, 5, 17, 11, 0, 0), transport.JW_STA);

				var cidLog = consol.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier);
				AssertNotNull(cidLog);
				AssertEquals("|EVT=BKC|NEW=QF100,17-May-23 09:10,17-May-23 11:00|OLD=QF123,17-May-23 09:00,17-May-23 20:00|RES=Updated from Booking Confirmation|TYP=Routing Leg", cidLog.SL_Reference);

				AddEventLog(consol,
					Events.ArrivalCode,
					"|FRM=AUSYD|TO=HKHKG|VFL=QF100|PTL=15|TTL=20|FDT=17-MAY-23",
					eventTime.AddHours(1),
					true);

				AssertEquals("Matched and auto updated", "QF100", transport.JW_VoyageFlight);
				AssertEquals("Matched and auto updated", new ZDateTime(2023, 5, 17, 9, 10, 0), transport.JW_STD);
				AssertEquals("Transport STA should not be updated by arrival event", new ZDateTime(2023, 5, 17, 11, 0, 0), transport.JW_STA);

				cidLog = consol.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier);
				AssertNotNull(cidLog);
				AssertEquals("|EST=Y|EVT=ARV|NEW=QF100,17-May-23 09:10,17-May-23 11:00|OLD=QF100,17-May-23 09:10,17-May-23 11:00|RES=Updated from Booking Confirmation|TYP=Routing Leg", cidLog.SL_Reference);
			}
		}

		public void TestProcessLog_UpdateActualEvents_AutoUpdateTransport()
		{
			using (FreightDataRegistry.Instance.AutomaticUpdatingofPlannedLegs_Air.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "HKHKG";
				transport.JW_VoyageFlight = "QF123";
				transport.JW_ETD = new ZDateTime(2023, 5, 17, 9, 0, 0);
				transport.JW_ETA = new ZDateTime(2023, 5, 17, 20, 0, 0);

				var eventTime = new ZDateTime(2023, 5, 17, 9, 10, 0);

				AddEventLog(consol,
					Events.DepartureCode,
					"|FRM=AUSYD|TO=HKHKG|VFL=QF100|PTL=15|TTL=20|FDT=17-MAY-23",
					eventTime,
					false);

				AssertEquals("Matched and auto updated", "QF100", transport.JW_VoyageFlight);
				AssertEquals("Matched and auto updated", new ZDateTime(2023, 5, 17, 9, 10, 0), transport.JW_ATD);
				AssertEquals("Arrival time is not available and no update", ZDateTime.Empty, transport.JW_ATA);

				var cidLog = consol.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier);
				AssertNotNull(cidLog);
				AssertEquals("|EVT=DEP|NEW=QF100,17-May-23 09:10,|OLD=QF123,,|RES=Updated from Actual Events|TYP=Routing Leg", cidLog.SL_Reference);

				AddEventLog(consol,
					Events.BookingConfirmedCode,
					"|FRM=AUSYD|TO=HKHKG|VFL=QF100|PTL=15|TTL=20|FDT=17-MAY-23|ETA=17-MAY-23",
					eventTime.AddHours(1),
					false);

				AddEventLog(consol,
					Events.ArrivalCode,
					"|FRM=AUSYD|TO=HKHKG|VFL=QF100|PTL=15|TTL=20|FDT=17-MAY-23",
					eventTime.AddHours(1),
					false);

				AssertEquals("Matched and auto updated", "QF100", transport.JW_VoyageFlight);
				AssertEquals("Matched and auto updated", new ZDateTime(2023, 5, 17, 9, 10, 0), transport.JW_ATD);
				AssertEquals("Matched and auto updated", new ZDateTime(2023, 5, 17, 10, 10, 0), transport.JW_ATA);

				cidLog = consol.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier);
				AssertNotNull(cidLog);
				AssertEquals("|EVT=ARV|NEW=QF100,17-May-23 09:10,17-May-23 10:10|OLD=QF100,17-May-23 09:10,|RES=Updated from Actual Events|TYP=Routing Leg", cidLog.SL_Reference);
			}
		}

		public void TestProcessLog_PropagateTransportEventToConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETD = new ZDateTime(2023, 5, 17, 9, 0, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 17, 20, 0, 0);

			var reference = "|FDT=17-MAY-23 09:10|FRM=AUSYD|PTL=15|TO=HKHKG|TTL=20|VFL=QF100";
			AddEventLog(transport,
				Events.DepartureCode,
				reference,
				ZDateTime.Now,
				false);

			var depEvent = consol.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull(depEvent);
			AssertEquals(false, depEvent.SL_IsEstimate);
			AssertEquals(reference, depEvent.SL_Reference);
		}

		[TestDate(2023, 11, 23)]
		public void TestIAirlineTrackingEventProvider_PropagationShouldNotDuplicateReference()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CNTAO";
			consol.JK_RL_NKDischargePort = "CAYYZ";
			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "CZ1111";
			transport.JW_ETD = ZDateTime.Now;
			Factory.Save();

			AddEventLog(transport,
						Events.DepartureCode,
						"|FAC=CTO|FDT=2023-11-23|FRM=CNTAO|LOC=CNTAO|MOD=AIR|TO=CAYYZ|TTL=60|VFL=CZ1111",
						ZDateTime.Now.AddHours(5),
						false);

			var depEvents = consol.Logs.GetAllLogs().OfType<StmALog>()
			  .Where(l => l.SL_SE_NKEvent == Events.DepartureCode && !l.SL_IsEstimate)
			  .ToArray();

			AssertEquals(1, depEvents.Length);
			AssertEquals("Changed To: 23-Nov-23|FAC=CTO|FDT=2023-11-23|FRM=CNTAO|LOC=CNTAO|MOD=AIR|TO=CAYYZ|TTL=60|VFL=CZ1111", depEvents[0].SL_Reference);
		}

		public void TestPrcessLog_UpdateBookedPiece_OnlyBookingConfirmationEventCouldUpdate()
		{
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("preconfirm", 0, consol.BookingConfirmations.Count);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=2|FDT=04-OCT-23",
				eventTime.AddHours(2),
				true);
			AssertEquals("Only BKC should create BookingConfirmation", 0, consol.BookingConfirmations.Count);

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(3),
				false);

			AssertEquals(1, consol.BookingConfirmations.Count);
			var bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals(Events.BookingConfirmedCode, bookingConfirmation.EventCode);
			AssertEquals("AUSYD", bookingConfirmation.LoadPort);
			AssertEquals("SGSIN", bookingConfirmation.DischargePort);
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)4, bookingConfirmation.BookedPieces);

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=8|FDT=04-OCT-23 19:00",
				eventTime.AddHours(4),
				false);

			AssertEquals(1, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals(Events.BookingConfirmedCode, bookingConfirmation.EventCode);
			AssertEquals("AUSYD", bookingConfirmation.LoadPort);
			AssertEquals("SGSIN", bookingConfirmation.DischargePort);
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)8, bookingConfirmation.BookedPieces);

			AddEventLog(consol,
				Events.ArrivalCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=2|FDT=04-OCT-23",
				eventTime.AddHours(5),
				true);

			AssertEquals(1, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("AUSYD", bookingConfirmation.LoadPort);
			AssertEquals("SGSIN", bookingConfirmation.DischargePort);
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)8, bookingConfirmation.BookedPieces);
		}

		void AddEventLog(EnterpriseBusinessObject bo, ZString eventCode, ZString reference, ZDateTime eventTime, bool isEstimate)
		{
			Thread.Sleep(500); // Simulate to get different SL_PostedTimeUtc for events

			var log = bo.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = eventCode;
				log.SL_Reference = reference;
				log.SL_EventTime = eventTime;
				log.SL_IsEstimate = isEstimate;
			}

			Factory.Save();
		}

		#endregion

		#region FlightNumber

		public void TestValidFlightNumberIsAddedToBookingConfirmationCollection()
		{
			// Arrange
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			// Act
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(1),
				false);

			// Assert
			AssertEquals(1, consol.BookingConfirmations.Count);
			var bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals(Events.BookingConfirmedCode, bookingConfirmation.EventCode);
			AssertEquals("AUSYD", bookingConfirmation.LoadPort);
			AssertEquals("SGSIN", bookingConfirmation.DischargePort);
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);
			AssertEquals((ZInt)4, bookingConfirmation.BookedPieces);
		}

		public void TestValidFlightNumberUpdatesBookingConfirmationCollectionForExistingConfirmation()
		{
			// Arrange
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(1),
				false);

			AssertEquals(1, consol.BookingConfirmations.Count);
			var bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);

			// Act
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ456|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(2),
				false);

			// Assert
			AssertEquals(2, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[1];
			AssertEquals("Updated flight number has been added", "SQ456", bookingConfirmation.VoyageFlight);
		}

		public void TestValidFlightNumberVariousFormats()
		{
			CombineAssertions(() =>
			{
				Test("AB123");
				Test("SQ1234");
				Test("sq1234");
				Test("S12345");
				Test("1Q2345");
				Test("SQ0123");
				Test("SQ01234");
				Test("A201234");
				Test("1A01234");
			});

			void Test(string flightNumber)
			{
				// Arrange
				var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				// Act
				AddEventLog(consol,
					Events.BookingConfirmedCode,
					$"|FRM=AUSYD|TO=SGSIN|VFL={flightNumber}|TTL=4|FDT=04-OCT-23 19:00",
					eventTime.AddHours(1),
					false);

				// Assert
				AssertEquals(1, consol.BookingConfirmations.Count);
				var bookingConfirmation = consol.BookingConfirmations[0];
				AssertEquals(flightNumber, bookingConfirmation.VoyageFlight);
			}
		}

		public void TestInvalidFlightNumberFormatIsNotAddedToBookingConfirmationCollection()
		{
			// Arrange
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			// Act
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=001|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(1),
				false);

			// Assert
			AssertEquals("EventLog with invalid flight number should not be actioned", 0, consol.BookingConfirmations.Count);
		}

		public void TestInvalidFlightNumberFormatIsNotUpdatedInBookingConfirmationCollection()
		{
			// Arrange
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(1),
				false);

			AssertEquals(1, consol.BookingConfirmations.Count);
			var bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);

			// Act
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=002|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(2),
				false);

			// Assert
			AssertEquals(1, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("Updated flight number should NOT be added", "SQ123", bookingConfirmation.VoyageFlight);
		}

		public void TestInvalidFlightNumberVariousFormats()
		{
			CombineAssertions(() =>
			{
				Test("ABC123");
				Test("123456");
				Test("SQ12345");
				Test("S123456");
				Test("1Q23456");
			});

			void Test(string flightNumber)
			{
				// Arrange
				var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				// Act
				AddEventLog(consol,
					Events.BookingConfirmedCode,
					$"|FRM=AUSYD|TO=SGSIN|VFL={flightNumber}|TTL=4|FDT=04-OCT-23 19:00",
					eventTime.AddHours(1),
					false);

				// Assert
				AssertEquals("EventLog with invalid flight number should not be actioned", 0, consol.BookingConfirmations.Count);
			}
		}

		public void TestInvalidFlightReferenceIsNotAddedToBookingConfirmationCollection()
		{
			// Arrange
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			// Act
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"Invalid flight number reported by airline|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(1),
				false);

			// Assert
			AssertEquals("EventLog with invalid flight reference should not be actioned", 0, consol.BookingConfirmations.Count);
		}

		public void TestInvalidFlightReferenceIsNotUpdatedInBookingConfirmationCollection()
		{
			// Arrange
			var eventTime = new ZDateTime(2023, 10, 4, 19, 0, 0);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"|FRM=AUSYD|TO=SGSIN|VFL=SQ123|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(1),
				false);

			AssertEquals(1, consol.BookingConfirmations.Count);
			var bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("SQ123", bookingConfirmation.VoyageFlight);

			// Act
			AddEventLog(consol,
				Events.BookingConfirmedCode,
				"Invalid flight number reported by airline|FRM=AUSYD|TO=SGSIN|VFL=SQ456|TTL=4|FDT=04-OCT-23 19:00",
				eventTime.AddHours(2),
				false);

			// Assert
			AssertEquals(1, consol.BookingConfirmations.Count);
			bookingConfirmation = consol.BookingConfirmations[0];
			AssertEquals("Updated flight number should NOT be added", "SQ123", bookingConfirmation.VoyageFlight);
		}

		#endregion
	}
}
