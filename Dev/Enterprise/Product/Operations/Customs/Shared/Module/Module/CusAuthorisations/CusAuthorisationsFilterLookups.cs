using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class CusAuthorisationsFilterLookups
	{
		public CusAuthorisationsFilterLookups(CusAuthorisationsFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		readonly CusAuthorisationsFilterStripBusinessObject filterBizObj;

		BusinessObjectFactory Factory => filterBizObj.Factory;

		public OrganisationsFindBoxCollection OrganisationList => new OrganisationsFindBoxCollection(Factory);

		public CodeDescriptionPairList CurrentStatusList => Factory.GetCachedValue<CurrentStatusList>();

		public CodeDescriptionPairList RuleCodeList => CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).GetRuleCodeListForModule(Factory);
	}
}
