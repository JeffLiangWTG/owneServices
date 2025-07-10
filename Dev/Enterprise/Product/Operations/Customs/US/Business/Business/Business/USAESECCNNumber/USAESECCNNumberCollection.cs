using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class USAESECCNNumberCollection : ZZRefCusCodeListCombinedCollection
	{
		public USAESECCNNumberCollection(BusinessObjectFactory factory, ZString licenseType)
			: base(factory)
		{
			this.licenseType = licenseType;
		}
		readonly ZString licenseType;

		protected override ZQuery CreateRelationshipFilter()
		{
			if (uSAESECCNNumberFilter == null)
			{
				uSAESECCNNumberFilter = new ZDBOnlyQuery(typeof(RefCusCodeList));
				uSAESECCNNumberFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.UnitedStates);
				uSAESECCNNumberFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber);
				uSAESECCNNumberFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;

				if (!licenseType.IsEmpty)
				{
					var codeListAttributeSubQuery = new ZDBOnlySubQuery(typeof(RefCusCodeListAttribute), RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, ZZRefCusCodeListCombinedSchema.PK);
					codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, RefCusCodeListAttributeTypes.Codes.LicenseType);
					codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_Value, licenseType);
					uSAESECCNNumberFilter.AddSubQuery(codeListAttributeSubQuery, JoinCondition.And);
				}
			}

			return uSAESECCNNumberFilter;
		}
		ZDBOnlyQuery uSAESECCNNumberFilter;
	}
}
