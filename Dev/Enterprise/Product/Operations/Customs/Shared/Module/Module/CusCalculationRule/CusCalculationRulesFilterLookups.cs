using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class CusCalculationRulesFilterLookups
	{
		public CusCalculationRulesFilterLookups(CusCalculationRulesFilterBusinessObject filterBizObj)
		{
			this.FilterBizObj = filterBizObj;
		}

		protected readonly CusCalculationRulesFilterBusinessObject FilterBizObj;

		protected BusinessObjectFactory Factory => FilterBizObj.Factory;

		public CodeDescriptionPairList RuleTypeList => CusCalculationRule.Lookups.RuleTypeList;

		public CodeDescriptionPairList TransportModeList => CusCalculationRule.Lookups.TransportModeList;

		public OrgHeaderCollection ImporterList => CusCalculationRule.Lookups.Importers;

		protected virtual CusCalculationRule CusCalculationRule => cusCalculationRule ?? (cusCalculationRule = Factory.GetNull<CusCalculationRule>());
		CusCalculationRule cusCalculationRule;
	}
}
