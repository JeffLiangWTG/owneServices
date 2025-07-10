using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class USRegionDistrictPortCollection : ZZRefCusCodeListCombinedCollection
	{
		public USRegionDistrictPortCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (regionDistrictPortFilter == null)
			{
				regionDistrictPortFilter = new ZDBOnlyQuery(typeof(RefCusCodeList));
				regionDistrictPortFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
				regionDistrictPortFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
				regionDistrictPortFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;
			}

			return regionDistrictPortFilter;
		}
		ZDBOnlyQuery regionDistrictPortFilter;
	}
}
