using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	class QuotedBookingRatingAdaptersProvider : RatingAdaptersProvider<QuotedBooking>
	{
		public QuotedBookingRatingAdaptersProvider(QuotedBooking parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(QuotedBooking parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new QuotedBookingRatingAdapter(parent) };
		}
	}
}
