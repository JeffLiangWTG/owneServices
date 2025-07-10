using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteGateMovementBookingFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string SourceReferenceNumber = "SourceReferenceNumber";
			public const string BookingReferenceNumber = "BookingReferenceNumber";
		}

		public override SchemaGuidColumn PKSchemaColumn => GteGateMovementBookingSchema.PK;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		#region Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var bookingReferenceNumberFilter = filters.AddNumberFilter(Schema.BookingReferenceNumber, GteGateMovementBookingSchema.GBM_BookingReferenceNumber);
			bookingReferenceNumberFilter.MultilingualDescription = Enterprise.Warehouse.GateManagement.GUI.ResString.GetMultilingualString("GteGateMovementBooking|GteGateMovementBookingFilterBusinessObject|GBM_BookingReferenceNumber", "Facility Reference Number");

			var sourceReferenceNumberFilter = filters.AddNumberFilter(Schema.SourceReferenceNumber, GteGateMovementBookingSchema.GBM_SourceReferenceNumber);
			sourceReferenceNumberFilter.MultilingualDescription = Enterprise.Warehouse.GateManagement.GUI.ResString.GetMultilingualString("GteGateMovementBooking|GteGateMovementBookingFilterBusinessObject|GBM_SourceReferenceNumber", "Source Reference Number");
		}

		#endregion
	}
}
