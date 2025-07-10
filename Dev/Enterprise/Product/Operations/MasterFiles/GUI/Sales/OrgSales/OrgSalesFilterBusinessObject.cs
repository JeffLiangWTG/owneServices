using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class OrgSalesFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddGuidFilter(FilterDescription.Origin, ModuleIDs.ViewLocation, OrgSalesSchema.OW_OriginID, ViewLocations).MultilingualDescription = ResString.GetMultilingualString("8b514aed-64f2-4e2a-b1af-2e799bf95992", "Origin");
			filters.AddGuidFilter(FilterDescription.Destination, ModuleIDs.ViewLocation, OrgSalesSchema.OW_DestinationID, ViewLocations).MultilingualDescription = ResString.GetMultilingualString("af3640b2-841e-413f-a080-aad45c189c6b", "Destination");

			filters.AddGuidFilter(FilterDescription.Buyer, ModuleIDs.Organisation, OrgSalesSchema.OW_OH_Buyer, Organisations).MultilingualDescription = ResString.GetMultilingualString("19d20a4e-2d07-4bea-a6ab-acc1d81c2775", "Buyer");
			filters.AddGuidFilter(FilterDescription.Supplier, ModuleIDs.Organisation, OrgSalesSchema.OW_OH_Supplier, Organisations).MultilingualDescription = ResString.GetMultilingualString("02448bfc-1410-4320-82e5-83adf52e9d36", "Supplier");

			return filters;
		}

		#endregion

		#region Lookups

		public ViewLocationCollection ViewLocations
		{
			get
			{
				if (viewLocations == null)
				{
					viewLocations = new ViewLocationCollection(Factory);
				}
				return viewLocations;
			}
		}
		ViewLocationCollection viewLocations;

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (organisations == null)
				{
					organisations = new OrgHeaderCollection(Factory);
				}

				return organisations;
			}
		}
		OrgHeaderCollection organisations;

		#endregion

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string ActualsLastTraded = "ActualsLastTraded";
			public const string Buyer = "Buyer";
			public const string Destination = "Destination";
			public const string Origin = "Origin";
			public const string Status = "Status";
			public const string Supplier = "Supplier";

			#endregion
		}
	}
}
