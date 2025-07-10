using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCommissionRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSalesTeams()
		{
			var rule = Factory.New<AccCommissionRule>();
			var lookups = new AccCommissionRuleLookups(rule);

			rule.ACM_GS_NKStaff = ZString.Empty;
			AssertNotNull(lookups.SalesTeams);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			rule.ACM_GS_NKStaff = "ADL";
			AssertNotNull(lookups.SalesTeams);
		}
	}
}
