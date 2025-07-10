using CargoWise.EntityFramework;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingBookingContainerDependentCollection : QuotedBookingContainerDependentCollection<TrackingContainer>
	{
		#region Constructors

		public TrackingBookingContainerDependentCollection(TrackingBooking booking)
			: base(booking.QuotedBooking.Booking, booking.Factory)
		{
			this.listProvider = booking;
		}

		#endregion

		#region Overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var container = bizOAdded as TrackingContainer;
			if (container != null)
			{
				container.ListProvider = ListProvider;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var container = bizO as TrackingContainer;
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
