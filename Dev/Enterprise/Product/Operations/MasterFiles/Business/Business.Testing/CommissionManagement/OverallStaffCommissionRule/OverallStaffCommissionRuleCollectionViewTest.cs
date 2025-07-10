using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OverallStaffCommissionRuleCollectionView))]
	sealed class OverallStaffCommissionRuleCollectionViewTest : NonPersistentBusinessObjectCollectionTestCase<OverallStaffCommissionRuleCollectionView>
	{
		[TestDate(2000, 1, 1)]
		public void TestIncludeExpiredRules()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var pastRule = staff.OverallCommissionRules.AddNew();
			pastRule.EndDate = new ZDateTime(1999, 1, 1);
			var currentRule = staff.OverallCommissionRules.AddNew();
			currentRule.StartDate = new ZDateTime(1991, 1, 1);
			currentRule.EndDate = new ZDateTime(2005, 1, 1);
			var futureRule = staff.OverallCommissionRules.AddNew();
			futureRule.StartDate = new ZDateTime(2005, 1, 1);
			var indefiniteRule = staff.OverallCommissionRules.AddNew();

			staff.OverallCommissionRulesView.IncludeExpiredRules = true;
			AssertContainsExactElementsInAnyOrder(new CompareSourceRulePkOnly(), new[] { pastRule, currentRule, futureRule, indefiniteRule }, staff.OverallCommissionRulesView.Cast<OverallStaffCommissionRule>());

			staff.OverallCommissionRulesView.IncludeExpiredRules = false;
			AssertContainsExactElementsInAnyOrder(new CompareSourceRulePkOnly(), new[] { currentRule, futureRule, indefiniteRule }, staff.OverallCommissionRulesView.Cast<OverallStaffCommissionRule>());
		}

		public void TestIncludeDisabledRules()
		{
			var team = Factory.NewWithValidTestData<SalesTeam>();
			var teamRule1 = team.CommissionRules.AddNew();
			teamRule1.FillWithValidTestData();
			var teamRule2 = team.CommissionRules.AddNew();
			teamRule2.FillWithValidTestData();
			var teamRule3 = team.CommissionRules.AddNew();
			teamRule3.FillWithValidTestData();
			var staff = team.Staff.AddNew();
			staff.FillWithValidTestData();

			Factory.Save();

			var inheritedTeamRule = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == teamRule1.PK);
			inheritedTeamRule.Status = GroupCommissionRuleStatusTypes.Codes.Inherited;
			var overridenTeamRule = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == teamRule2.PK);
			overridenTeamRule.Status = GroupCommissionRuleStatusTypes.Codes.Overridden;
			var disabledTeamRule = staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().First(x => x.BaseRule.PK == teamRule3.PK);
			disabledTeamRule.Status = GroupCommissionRuleStatusTypes.Codes.Disabled;
			var enabledStaffRule = staff.OverallCommissionRules.AddNew();
			enabledStaffRule.Status = StaffCommissionRuleStatusTypes.Codes.Enabled;

			staff.OverallCommissionRulesView.IncludeDisabledRules = true;
			AssertContainsExactElementsInAnyOrder(new CompareSourceRulePkOnly(), new[] { inheritedTeamRule, overridenTeamRule, disabledTeamRule, enabledStaffRule }, staff.OverallCommissionRulesView.Cast<OverallStaffCommissionRule>());

			staff.OverallCommissionRulesView.IncludeDisabledRules = false;
			AssertContainsExactElementsInAnyOrder(new CompareSourceRulePkOnly(), new[] { inheritedTeamRule, overridenTeamRule, enabledStaffRule }, staff.OverallCommissionRulesView.Cast<OverallStaffCommissionRule>());
		}

		protected override OverallStaffCommissionRuleCollectionView GetCollectionToTest()
		{
			return new OverallStaffCommissionRuleCollectionView(Staff);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OverallStaffCommissionRule(Staff.CommissionRules.AddNew());
		}

		GlbStaff Staff
		{
			get { return staff ?? (staff = Factory.NewWithValidTestData<GlbStaff>()); }
		}
		GlbStaff staff;

		class CompareSourceRulePkOnly : IEqualityComparer<OverallStaffCommissionRule>
		{
			public bool Equals(OverallStaffCommissionRule x, OverallStaffCommissionRule y)
			{
				return x.BaseRule.PK == y.BaseRule.PK;
			}

			public int GetHashCode(OverallStaffCommissionRule obj)
			{
				return obj.BaseRule.PK.GetHashCode();
			}
		}
	}
}
