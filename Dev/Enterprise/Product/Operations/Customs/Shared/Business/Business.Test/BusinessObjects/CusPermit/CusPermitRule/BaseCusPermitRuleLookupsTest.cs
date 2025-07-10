using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseCusPermitRuleLookupsTest<TCusPermitRuleLookups, TCusPermitRule> : SharedCusPermitRuleLookupsTest<TCusPermitRuleLookups, TCusPermitRule>
			where TCusPermitRuleLookups : BaseCusPermitRuleLookups
			where TCusPermitRule : BaseCusPermitRule
	{
		#region Implementation

		protected override TCusPermitRule GetNewRule(BusinessObjectFactory factory)
		{
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			return (TCusPermitRule)permitHeader.CusPermitRules.AddNew();
		}

		#endregion
	}
}
