using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusGuaranteeRuleLookupsTest<TCusPermitRuleLookups, TCusPermitRule> : SharedCusPermitRuleLookupsTest<TCusPermitRuleLookups, TCusPermitRule>
			where TCusPermitRuleLookups : CusGuaranteeRuleLookups
			where TCusPermitRule : CusGuaranteeRule
	{
		#region Implementation

		protected override TCusPermitRule GetNewRule(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			return (TCusPermitRule)guaranteeHeader.CusGuaranteeRules.AddNew();
		}

		#endregion
	}
}
