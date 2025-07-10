using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteBookingFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string ReferenceNumber = "ReferenceNumber";
		}

		public override SchemaGuidColumn PKSchemaColumn => GteBookingSchema.PK;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddReferenceNumberFilter(filters);

			return filters;
		}

		#region Filters

		void AddReferenceNumberFilter(ModuleFilterCollection filters)
		{
			var referenceNumberFilter = filters.AddNumberFilter(Schema.ReferenceNumber, GteBookingSchema.GBK_ReferenceNumber);
			referenceNumberFilter.MultilingualDescription = Enterprise.Warehouse.GateManagement.GUI.ResString.GetMultilingualString("GteBooking|GteBookingFilterBusinessObject|ReferenceNumber", "Reference Number");
		}

		#endregion
	}
}
