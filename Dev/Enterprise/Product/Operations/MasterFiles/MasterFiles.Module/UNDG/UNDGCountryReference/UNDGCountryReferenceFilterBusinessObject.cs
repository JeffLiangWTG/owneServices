using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGCountryReferenceFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			var countryFilter = filters.AddNkFilter("Country", UNDGCountryReferenceSchema.DCR_RN_NKCountry, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("b2107d02-3710-eda0-4f5d-a33dd0b24083", "Country/Region");

			var typeFilter = filters.AddTextFilter("Type", UNDGCountryReferenceSchema.DCR_Type, UNDGCountryReferenceLookups.Types);
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("160d48f1-908f-eabc-4de6-e4ccca5cbf6b", "Type");

			return filters;
		}

		#endregion
	}
}
