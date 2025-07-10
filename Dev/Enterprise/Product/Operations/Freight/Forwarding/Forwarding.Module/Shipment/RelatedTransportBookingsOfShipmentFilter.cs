using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class RelatedTransportBookingsOfShipmentFilter : ModuleGuidPivotFilter
	{
		public RelatedTransportBookingsOfShipmentFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.DtbBooking, DtbBookingConsolidationSchema.PK, DtbBookingConsolidationSchema.KB_ParentID, listDelegate, typeof(ForwardingShipment), typeof(IDtbBookingConsolidation))
		{
			MultilingualDescription = ResString.GetMultilingualString("7617ea86-ecc3-4597-9376-054871d0837a", "Related Transport Bookings");
		}

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		protected override SchemaColumn SubQueryColumn => DtbBookingSchema.KM_KB_Booking;

		protected override string ParentPkColumnNameForAllMatch => JobShipmentSchema.PK.Name;

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			if (ComparisonOperator == ComparisonConstants.AllMatch)
			{
				return base.GetQueryForSelectedFiltersCore(filterBusinessObject, subModuleFilter);
			}

			var query = GetNewQueryForSelectedFilters();
			var pivotSubQuery = GetPivotSubQuery(filterBusinessObject, subModuleFilter);
			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return query;
		}
	}
}
