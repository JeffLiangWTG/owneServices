using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class DocumentDtbBookingCollection : NonPersistentBusinessObjectCollection<DocumentDtbBooking>
	{
		public DocumentDtbBookingCollection(DtbBookingCollection bookings)
			: base()
		{
			AddRange(bookings.Select(b => new DocumentDtbBooking(b)));
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		public void SelectOrUnSelectAll(bool selectAll)
		{
			foreach (DocumentDtbBooking booking in this)
			{
				booking.IncludeInDelivery = selectAll;
			}
		}
	}
}
