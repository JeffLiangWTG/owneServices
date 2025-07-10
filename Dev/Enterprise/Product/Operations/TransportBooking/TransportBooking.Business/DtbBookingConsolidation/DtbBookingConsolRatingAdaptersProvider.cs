using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolRatingAdaptersProvider : RatingOrCostingAdaptersProvider<DtbBookingConsolidation>
	{
		public DtbBookingConsolRatingAdaptersProvider(DtbBookingConsolidation parent)
			: base(parent, null) { }

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(DtbBookingConsolidation parent)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(parent));
			result.AddRange(parent.Bookings);
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}
	}
}
