using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportBookings.Business
{
	public class ViewTransportBookingParents : AutoViewTransportBookingParents, IViewTransportBookingParents
	{
		public ViewTransportBookingParents(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void Delete()
		{
			throw new NotSupportedException("Deletion of the ViewTransportBookingParents is not supported.");
		}
	}
}
