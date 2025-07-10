using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefMessagingBussCarrierInfoFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var codeFilter = filters.AddTextFilter("Code", RefMessagingBussCarrierInfoSchema.ZMC_CarrierCode);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("RefMessagingBussCarrierInfoFilter|Code", "Code");

			var nameFilter = filters.AddTextFilter("Name", RefMessagingBussCarrierInfoSchema.ZMC_CarrierName);
			nameFilter.MultilingualDescription = ResString.GetMultilingualString("RefMessagingBussCarrierInfoFilter|Name", "Name");

			var countryFilter = filters.AddTextFilter("Country", RefMessagingBussCarrierInfoSchema.ZMC_CountryCode);
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("RefMessagingBussCarrierInfoFilter|Country", "Country");
		}
	}
}
