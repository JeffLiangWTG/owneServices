using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefVesselFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumbersAndReferencesFilters(filters);
			AddLocationFilters(filters);
			AddOrganisationFilters(filters);
			AddModesAndTypesFilters(filters);
			return filters;
		}

		#region AddNumberFilters

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(RefVesselCollection.FilterConstants.VesselName, RefVesselSchema.RV_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|VesselName", "Vessel Name");
			filters.AddNumberFilter(RefVesselCollection.FilterConstants.CarrierCode, RefVesselSchema.RV_CarrierCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|CarrierCode", "Carrier Code");
			filters.AddNumberFilter(RefVesselCollection.FilterConstants.LloydsNumber, RefVesselSchema.RV_LloydsNumber).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|LloydsNumber", "Lloyds/IMO Number");
			filters.AddNumberFilter(RefVesselCollection.FilterConstants.RadioCallSign, RefVesselSchema.RV_RadioCallSign).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|RadioCallSign", "Radio Call Sign");
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter(RefVesselCollection.FilterConstants.CountryOfRegistration, RefVesselSchema.RV_RN_NKCountryOfReg, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|CountryOfRegistration", "Country/Region of Registration");
		}

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter(RefVesselCollection.FilterConstants.Carrier, ModuleIDs.Organisation, RefVesselSchema.RV_OH, new TransportShippingProviderCollection(Factory));
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|Carrier", "Carrier");

			filter = filters.AddGuidFilter(RefVesselCollection.FilterConstants.Consortium, ModuleIDs.RefCarrierConsortium, RefVesselSchema.RV_RG, new RefCarrierConsortiumCollection(Factory));
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|Consortium", "Consortium");
		}

		#endregion

		#region AddModesAndTypesFilters

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter(RefVesselCollection.FilterConstants.VesselType, RefVesselSchema.RV_VesselType, new CodeDescriptionPairList(OLookUpEditType.VesselType));
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefVesselFilter|VesselType", "Vessel Type");
		}

		#endregion
	}
}
