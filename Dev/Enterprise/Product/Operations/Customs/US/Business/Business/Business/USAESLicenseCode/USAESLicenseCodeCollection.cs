using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class USAESLicenseCodeCollection : ZZRefCusCodeListCombinedCollection
	{
		public USAESLicenseCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (uSAESLicenseCodeFilter == null)
			{
				uSAESLicenseCodeFilter = new ZDBOnlyQuery(typeof(RefCusCodeList));
				uSAESLicenseCodeFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
				uSAESLicenseCodeFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode);
				uSAESLicenseCodeFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;
			}

			return uSAESLicenseCodeFilter;
		}
		ZDBOnlyQuery uSAESLicenseCodeFilter;
	}
}
