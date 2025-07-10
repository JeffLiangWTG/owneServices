using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	class QuotedBookingFilterControlForTest : QuotedBookingFilterControl
	{
		public QuotedBookingFilterControlForTest(BusinessObjectFactory factory)
			: base(new Business.ViewQuotedBookingCollection(factory), new QuotedBookingFilterStripBusinessObject())
		{
		}

		public ZFilterStrip NewZFilterStripForTest()
		{
			return NewZFilterStrip();
		}
	}

	class OneOffQuoteFilterControlForTest : QuotedBookingFilterControl
	{
		public OneOffQuoteFilterControlForTest(BusinessObjectFactory factory)
			: base(new Business.ViewQuotedBookingCollection(factory), new OneOffQuoteFilterStripBusinessObject(), isOneOffQuote: true)
		{
		}
	}
}
