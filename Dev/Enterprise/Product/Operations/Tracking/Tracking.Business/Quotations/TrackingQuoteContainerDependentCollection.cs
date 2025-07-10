using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	public class TrackingQuoteContainerDependentCollection : RateOneOffContainersCollection<TrackingQuoteContainer>
	{
		#region Constructors

		public TrackingQuoteContainerDependentCollection(TrackingQuotedBooking booking, bool isLooseCargo)
			: base(booking.Quote.CurrentOneOffQuote, isLooseCargo)
		{
			this.listProvider = booking;
		}

		public TrackingQuoteContainerDependentCollection(TrackingQuote quote, bool isLooseCargo)
			: base(quote.CurrentOneOffQuote, isLooseCargo)
		{
			this.listProvider = quote;
		}

		#endregion

		#region Overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (listProvider is TrackingQuotedBooking)
			{
				((TrackingQuotedBooking)listProvider).Quote.CurrentOneOffQuote.Containers.Add(bizOAdded);
			}

			var container = bizOAdded as TrackingQuoteContainer;
			if (container != null)
			{
				container.ListProvider = ListProvider;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (listProvider is TrackingQuotedBooking)
			{
				((TrackingQuotedBooking)listProvider).Quote.CurrentOneOffQuote.Containers.Remove(bizO);
			}

			var container = bizO as TrackingQuoteContainer;
			if (container != null)
			{
				container.ListProvider = null;
			}
		}

		#endregion

		#region Implementation

		protected IContainerListProvider ListProvider
		{
			get { return listProvider; }
		}

		readonly IContainerListProvider listProvider;

		#endregion
	}
}
