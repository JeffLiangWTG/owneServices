using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingMultiJobConsolidationCollection : ActiveBusinessObjectCollection<DtbBookingConsolidation>
	{
		public DtbBookingMultiJobConsolidationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public DtbBookingMultiJobConsolidationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(DtbBookingConsolidationSchema.KB_JobType, TransportConsolidationJobTypes.Codes.BookingTransportConsolidation);
		}
	}
}
