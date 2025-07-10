using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingLinerAndAgencyBookingCollection : AgencyBookingCollection
	{
		public TrackingLinerAndAgencyBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new TrackingLinerAndAgencyBooking this[int index]
		{
			get { return (TrackingLinerAndAgencyBooking)base[index]; }
		}

		public new TrackingLinerAndAgencyBooking AddNew()
		{
			return (TrackingLinerAndAgencyBooking)base.AddNew();
		}
	}
}
