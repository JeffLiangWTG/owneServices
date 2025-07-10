using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class DtbBookingConsolidationExtenderForTransitDispatch : IDtbBookingConsolidationExtender
	{
		public IEnumerable<IDtbBooking> GetExistingDtbBookings(IDtbBookingConsolidation consolidation, string sourceKey, BusinessObjectFactory factory)
		{
			var filter = new ZQuery(StmUniversalJobLinkSchema.UCL_SourceKey, sourceKey);
			filter.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceType, nameof(DataContextType.TransitDispatch));
			filter.AddToFilter(StmUniversalJobLinkSchema.UCL_ParentTableCode, DtbBookingSchema.Constants.Prefix);
			var bookingPKs = factory.Load<StmUniversalJobLink>(filter)
				.Select(joblink => joblink.UCL_ParentID);
			return consolidation.Bookings.Where(b => (ZBool)((BusinessObject)b)[DtbBookingSchema.KM_IsActive] == true && bookingPKs.Contains(b.PK));
		}

		public bool IsCreatingBookingByPassedPackages => true;
	}
}
