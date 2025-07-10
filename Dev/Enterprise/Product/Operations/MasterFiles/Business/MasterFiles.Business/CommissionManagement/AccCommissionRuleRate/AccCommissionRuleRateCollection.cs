using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleRateCollection : ActiveBusinessObjectCollection<AccCommissionRuleRate>
	{
		public AccCommissionRuleRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccCommissionRuleRateCollection(AccCommissionRule rule)
			: base(rule)
		{
		}

		public AccCommissionRuleRateCollection(AccCommissionRuleStaffOverride ruleOverride)
			: base(ruleOverride)
		{
		}
	}
}
