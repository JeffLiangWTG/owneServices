namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	class QuotedBookingValidationForTest : QuotedBookingValidation
	{
		public QuotedBookingValidationForTest(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
		}

		public void exposeCheckPickUpEquipment()
		{
			CheckPickupEquipment();
		}

		public void exposeCheckDeliveryEquipment()
		{
			CheckDeliveryEquipment();
		}
	}
}
