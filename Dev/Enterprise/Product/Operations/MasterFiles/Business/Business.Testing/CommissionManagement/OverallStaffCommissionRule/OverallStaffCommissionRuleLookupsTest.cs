using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OverallStaffCommissionRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatuses()
		{
			var salesTeam = Factory.New<SalesTeam>();
			var teamRule = salesTeam.CommissionRules.AddNew();
			var staff = salesTeam.Staff.AddNew();
			var staffRule = staff.CommissionRules.AddNew();

			var overallRule = new OverallStaffCommissionRule(staff, teamRule);
			AssertContainsExactElementsInAnyOrder(new GroupCommissionRuleStatusTypes(), overallRule.Lookups.Statuses);

			overallRule = new OverallStaffCommissionRule(staffRule);
			AssertContainsExactElementsInAnyOrder(new StaffCommissionRuleStatusTypes(), overallRule.Lookups.Statuses);
		}
	}
}
