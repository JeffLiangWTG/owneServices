using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OverallStaffCommissionRuleCollection))]
	sealed class OverallStaffCommissionRuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OverallStaffCommissionRuleCollection>
	{
		public void TestSetDefaultsForNewChild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			var rule = staff.OverallCommissionRules.AddNew();
			AssertEquals(GlbCompany.CurrentCompany.PK, rule.CompanyPk);
		}

		public void TestLoadRules()
		{
			var team = Factory.New<SalesTeam>();
			var teamRule1 = team.CommissionRules.AddNew();
			teamRule1.FillWithValidTestData();
			var teamRule2 = team.CommissionRules.AddNew();
			teamRule2.FillWithValidTestData();
			var teamRule3 = team.CommissionRules.AddNew();
			teamRule3.FillWithValidTestData();
			var staff = team.Staff.AddNew();
			staff.GS_Code = "ADL";
			var staffRule1 = staff.CommissionRules.AddNew();
			staffRule1.FillWithValidTestData();
			var staffRule2 = staff.CommissionRules.AddNew();
			staffRule2.FillWithValidTestData();

			Factory.Save();

			staff.OverallCommissionRules.LoadRules();
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionRule>.PKOnlyComparer,
				new AccCommissionRule[] { teamRule1, teamRule2, teamRule3, staffRule1, staffRule2 },
				staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().Select(rule => rule.BaseRule));
		}

		public void TestLoadRules_StaffRuleWithGroup()
		{
			var team = Factory.New<SalesTeam>();
			var staff = team.Staff.AddNew();
			staff.GS_Code = "ADL";
			var staffRule = staff.CommissionRules.AddNew();
			staffRule.ACM_GG = team.PK;
			staffRule.FillWithValidTestData();

			Factory.Save();

			staff.OverallCommissionRules.LoadRules();
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionRule>.PKOnlyComparer,
				new AccCommissionRule[] { staffRule },
				staff.OverallCommissionRules.Cast<OverallStaffCommissionRule>().Select(rule => rule.BaseRule));
		}

		protected override OverallStaffCommissionRuleCollection GetCollectionToTest()
		{
			return new OverallStaffCommissionRuleCollection(Staff);
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
	}
}
