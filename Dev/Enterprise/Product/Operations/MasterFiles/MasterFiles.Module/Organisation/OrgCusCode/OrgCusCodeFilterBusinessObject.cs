using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCusCodeFilterBusinessObject : FilterStripBusinessObject, IDropEditCodeFindBoxSupportFilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Customs Reg No", OrgCusCodeSchema.OK_CustomsRegNo).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCusCodeFilter|CustomsRegNo", "Customs Reg No");

			filters.AddFilter(new OrgCusCodeFilter("Code Type", OrgCusCodeSchema.OK_CodeType)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCusCodeFilter|CodeType", "Code Type"),
			});
		}

		#endregion

		#region Related

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, OrgCusCodeSchema.OK_OH, OrgHeaderList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCusCodeFilter|Organisation", "Organization");
			filters.AddGuidFilter("Premises Address", ModuleIDs.OrgAddresses, OrgCusCodeSchema.OK_OA_PremisesAddress, OrgAddressList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCusCodeFilter|PremisesAddress", "Premises Address");
			filters.AddNkFilter("Country", OrgCusCodeSchema.OK_RN_NKCodeCountry, ModuleIDs.RefCountry, RefCountryList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCusCodeFilter|Country", "Country/Region");
		}

		#endregion

		#endregion

		#region Lookups

		#region OrgHeaderList

		public OrganisationsFindBoxCollection OrgHeaderList
		{
			get { return fOrgHeaderList ?? (fOrgHeaderList = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection fOrgHeaderList;

		#endregion

		#region RefCountryList

		public RefCountryCollection RefCountryList
		{
			get { return fRefCountryList ?? (fRefCountryList = new RefCountryCollection(Factory)); }
		}
		RefCountryCollection fRefCountryList;

		#endregion

		#region OrgAddressList

		public OrgAddressCollection OrgAddressList
		{
			get { return fOrgAddressList ?? (fOrgAddressList = new OrgAddressCollection(Factory)); }
		}
		OrgAddressCollection fOrgAddressList;

		#endregion

		#endregion

		#region IDropEditCodeFindBoxSupportFilterStripBusinessObject Members

		CargoWise.Schema.SchemaColumn IDropEditCodeFindBoxSupportFilterStripBusinessObject.CodeTypeSchema
		{
			get { return OrgCusCodeSchema.OK_CodeType; }
		}

		#endregion
	}
}
