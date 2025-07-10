using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer.Testing;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	class ConvertToShipmentProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestCreateConvertToShipmentProcessor()
		{
			var factory = new BusinessObjectFactory();
			var creator = new ConvertToShipmentProcessorCreator();
			AssertNull("Should not create processor if null provider", creator.CreateConvertToShipmentProcessor(null));
			AssertNull("Should not create processor if not booking", creator.CreateConvertToShipmentProcessor(new DummyIWorkflowProvider()));
			foreach (QuoteBookingType quoteBookingType in Enum.GetValues(typeof(QuoteBookingType)))
			{
				var booking = QuotedBooking.New(quoteBookingType, factory);
				AssertEquals($"{quoteBookingType} - Should create a ConvertToShipment processor if provider is a booking", creator.CreateConvertToShipmentProcessor(booking).GetType(), typeof(ConvertToShipmentProcessor));
			}
		}
	}
}
