using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	public class QuotedBookingICartageParentTestCase : ICartageParentTestCase
	{
		protected override ICartageParent GetNewParent()
		{
			return QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
		}
	}
}
