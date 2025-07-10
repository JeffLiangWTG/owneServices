using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitRuleLookupsTest<TSharedCusPermitRuleLookups, TSharedCusPermitRule> : BusinessObjectLookupsTestCase
			where TSharedCusPermitRuleLookups : SharedCusPermitRuleLookups
			where TSharedCusPermitRule : SharedCusPermitRule
	{
		public virtual void TestPermitRuleCodes()
		{
			AssertType<PermitRuleCodeList>(PermitRule.Lookups.PermitRuleCodes);
		}

		public virtual void TestCPR_ValueFromList()
		{
			Assert(typeof(ICollection).IsAssignableFrom(PermitRule.Lookups.CPR_ValueFromList.GetType()));
		}

		public virtual void TestCPR_ValueToList()
		{
			Assert(typeof(ICollection).IsAssignableFrom(PermitRule.Lookups.CPR_ValueToList.GetType()));
		}

		#region Implementation

		protected abstract TSharedCusPermitRule GetNewRule(BusinessObjectFactory factory);

		protected TSharedCusPermitRule PermitRule => permitRule ?? (permitRule = GetNewRule(Factory));
		TSharedCusPermitRule permitRule;

		protected SharedCusPermitHeader PermitHeader => PermitRule.PermitHeader;

		#endregion
	}
}
