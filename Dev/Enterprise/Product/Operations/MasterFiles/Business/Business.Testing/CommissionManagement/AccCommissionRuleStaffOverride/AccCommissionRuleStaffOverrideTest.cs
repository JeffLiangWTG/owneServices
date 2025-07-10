using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCommissionRuleStaffOverride))]
	sealed class AccCommissionRuleStaffOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertyMaxLength()
		{
			AssertEquals(AccCommissionRule.Schema.ACM_CommissionBasisMaxLength, AccCommissionRuleStaffOverride.Schema.CRO_CommissionBasisMaxLength);
			AssertEquals(AccCommissionRule.Schema.ACM_CommissionTriggerTypeMaxLength, AccCommissionRuleStaffOverride.Schema.CRO_CommissionTriggerTypeMaxLength);
		}
	}
}
