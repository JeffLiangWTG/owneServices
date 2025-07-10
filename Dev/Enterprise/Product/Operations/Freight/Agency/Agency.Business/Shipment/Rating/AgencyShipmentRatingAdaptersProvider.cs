using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentRatingAdaptersProvider : ShipmentRatingAdaptersProvider<AgencyShipment>
	{
		protected internal AgencyShipmentRatingAdaptersProvider(AgencyShipment parent) : base(parent)
		{
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(AgencyShipment parent)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(parent));
			var bookings = TransportBookingLoader.GetRelatedTransportBookingJobInvoicingPlugIn(parent);
			result.AddRange(bookings);
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}
	}
}
