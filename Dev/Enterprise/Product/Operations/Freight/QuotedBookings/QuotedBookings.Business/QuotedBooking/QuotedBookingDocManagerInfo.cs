using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingDocManagerInfo : ForwardingShipmentDocManagerInfo
	{
		public QuotedBookingDocManagerInfo(QuotedBooking parent)
			: base(parent != null ? parent.Booking : null)
		{
			this.quotedBooking = parent;
		}

		readonly QuotedBooking quotedBooking;

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();
			list.AddRange(base.GetRelatedObjects());

			if (quotedBooking != null)
			{
				list.AddRange(TransportBookingLoader.GetRelatedTransportBookingEDocs(quotedBooking));
			}

			return list.ToArray();
		}
	}
}
