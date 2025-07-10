using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class USFTZLicenseTypeCollection : ZZRefCusCodeListCombinedCollection
	{
		public USFTZLicenseTypeCollection(BusinessObjectFactory factory, ZDate effectiveDate)
			: base(factory)
		{
			this.effectiveDate = effectiveDate;
		}

		readonly ZDate effectiveDate;

		protected override ZQuery CreateRelationshipFilter()
		{
			if (uSLicenseTypeFilter == null)
			{
				uSLicenseTypeFilter = new ZDBOnlyQuery(typeof(RefCusCodeList));
				uSLicenseTypeFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
				uSLicenseTypeFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType);
				uSLicenseTypeFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate);
				uSLicenseTypeFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveDate);
				uSLicenseTypeFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;
			}

			return uSLicenseTypeFilter;
		}
		ZDBOnlyQuery uSLicenseTypeFilter;
	}
}
