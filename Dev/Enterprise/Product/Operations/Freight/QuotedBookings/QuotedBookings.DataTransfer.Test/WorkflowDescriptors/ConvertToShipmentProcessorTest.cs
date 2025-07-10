using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	class ConvertToShipmentProcessorTest : TestCaseWithFactory
	{
		#region Constructor
		public void TestConstructor_RequiredArguments()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ConvertToShipmentProcessor(null));
			AssertNoExceptionThrown(() => new ConvertToShipmentProcessor(booking));
		}

		#endregion
		#region Process
		public void TestProcess_ConvertsBookingWithQuote_ToShipment()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();
			AssertEquals("Pre-Condition - the booking is not a shipment", false, booking.Booking.JS_IsForwardRegistered);
			var processor = new ConvertToShipmentProcessor(booking);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			var notification = notifications.Events;
			var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
			CombineAssertions(() =>
			{
				AssertEquals(0, notifications.Events.Length);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);
			}

			);
		}

		public void TestProcess_ConvertsQuickBooking_ToShipment()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();
			AssertEquals("Pre-Condition - the booking is not a shipment", false, booking.Booking.JS_IsForwardRegistered);
			var processor = new ConvertToShipmentProcessor(booking);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			var notification = notifications.Events;
			var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
			CombineAssertions(() =>
			{
				AssertEquals(0, notifications.Events.Length);
				Assert("The shipment was a booking", shipment.JS_IsBooking);
				Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);
			}

			);
		}

		public void TestProcess_DoesNotProcess_IfDirectBooking()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Booking.JS_IsDirectBooking = true;
			Factory.Save();
			AssertEquals("Pre-Condition - the booking is not a shipment", false, booking.Booking.JS_IsForwardRegistered);
			var processor = new ConvertToShipmentProcessor(booking);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			var notification = notifications.Events[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should be an error notification", NotificationType.Error, notification.Type);
				AssertEquals("The booking has not been converted", false, booking.Booking.JS_IsForwardRegistered);
				AssertEquals("Unable to convert Booking to Shipment: You cannot convert a direct booking to a shipment. Please choose 'Consolidate' from the menu to create a new Consol.", notification.Message);
			}

			);
		}

		public void TestProcess_DoesProcess_IfNotBooked_AndShipmentStatusVisibility_FromRegistry()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			Factory.Save();

			AssertEquals("Pre-Condition - the booking is not a shipment", false, booking.Booking.JS_IsForwardRegistered);
			var processor = new ConvertToShipmentProcessor(booking);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			var notification = notifications.Events[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should be an error notification", NotificationType.Error, notification.Type);
				AssertEquals("The booking has not been converted", false, booking.Booking.JS_IsForwardRegistered);
				AssertEquals("Unable to convert Booking to Shipment: The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", notification.Message);
			}

			);
		}

		public void TestProcess_DoesNotProcess_IfNoBooking()
		{
			var booking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			Factory.Save();
			var processor = new ConvertToShipmentProcessor(booking);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			var notification = notifications.Events[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should be an error notification", NotificationType.Error, notification.Type);
				AssertEquals("Unable to convert Booking to Shipment: Cannot convert a Spot Quote to Shipment without a Booking", notification.Message);
			}

			);
		}
		#endregion
	}
}
