using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingBookingOrderLinkCollection : NonPersistentBusinessObjectCollection<TrackingBookingOrderLink>
	{
		#region Constructors

		public TrackingBookingOrderLinkCollection(TrackingBooking booking)
			: base()
		{
			this.booking = booking;
		}

		#endregion

		#region Properties

		public TrackingBooking Booking
		{
			get
			{
				return booking;
			}
		}

		#endregion

		#region Methods

		public void Validate()
		{
			foreach (TrackingBookingOrderLink link in this)
			{
				link.Validate();
			}
		}

		#endregion

		#region Overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			Order orderToAdd = ((TrackingBookingOrderLink)bizOAdded).LinkedOrder;
			if (orderToAdd != null)
			{
				((TrackingBookingOrderLink)bizOAdded).Validate();
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Order orderToRemove = ((TrackingBookingOrderLink)bizO).LinkedOrder;
			if (orderToRemove != null)
			{
				if (Booking.AttachedOrders.Contains(orderToRemove))
				{
					Booking.AttachedOrders.RemoveFromRelationship(orderToRemove);
				}
			}
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			foreach (Order attachedOrder in Booking.AttachedOrders)
			{
				Add(new TrackingBookingOrderLink(Booking, attachedOrder.PK));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TrackingBookingOrderLink(Booking, ZGuid.Empty);
		}

		#endregion

		#region Implementation

		readonly TrackingBooking booking;

		#endregion
	}
}
