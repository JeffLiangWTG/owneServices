using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OverallStaffCommissionRule))]
	sealed class OverallStaffCommissionRuleTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor_HasChanges()
		{
			var team = Factory.NewWithValidTestData<SalesTeam>();
			var staff = team.Staff.AddNew();
			var staffRule = staff.CommissionRules.AddNew();
			staffRule.FillWithValidTestData();
			var teamRule = team.CommissionRules.AddNew();
			teamRule.FillWithValidTestData();

			Factory.Save();

			AssertEquals(false, new OverallStaffCommissionRule(staffRule).HasChanges);
			AssertEquals(false, new OverallStaffCommissionRule(staff, teamRule).HasChanges);
		}

		public void TestIsCurrent()
		{
			var date = new ZDate(2014, 11, 11);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var expiredRule = staff.OverallCommissionRules.AddNew();
			var currentRule = staff.OverallCommissionRules.AddNew();
			var futureRule = staff.OverallCommissionRules.AddNew();
			var indefiniteRule = staff.OverallCommissionRules.AddNew();

			expiredRule.EndDate = new ZDate(2013, 1, 1);

			currentRule.StartDate = new ZDate(2014, 1, 1);
			currentRule.EndDate = new ZDate(2015, 1, 1);

			futureRule.StartDate = new ZDate(2015, 1, 1);

			indefiniteRule.StartDate = ZDate.Empty;
			indefiniteRule.EndDate = ZDate.Empty;

			AssertEquals(false, expiredRule.IsCurrent(date));
			AssertEquals(true, currentRule.IsCurrent(date));
			AssertEquals(false, futureRule.IsCurrent(date));
			AssertEquals(true, indefiniteRule.IsCurrent(date));
		}

		public void TestPropertyMaxLength()
		{
			AssertEquals(GlbStaff.Schema.GS_CodeMaxLength, OverallStaffCommissionRule.Schema.StaffCodeMaxLength);
			AssertEquals(AccCommissionRule.Schema.ACM_CommissionBasisMaxLength, OverallStaffCommissionRule.Schema.CommissionBasisMaxLength);
			AssertEquals(AccCommissionRule.Schema.ACM_CommissionTriggerTypeMaxLength, OverallStaffCommissionRule.Schema.CommissionTriggerTypeMaxLength);
			AssertEquals(AccCommissionRule.Schema.ACM_ProductMaxLength, OverallStaffCommissionRule.Schema.ProductMaxLength);
			AssertEquals(AccCommissionRule.Schema.ACM_ServiceMaxLength, OverallStaffCommissionRule.Schema.ServiceMaxLength);
			AssertEquals(AccCommissionRule.Schema.ACM_SubModuleMaxLength, OverallStaffCommissionRule.Schema.SubModuleMaxLength);
		}

		public void TestPropertiesReadOnly_StaffRule()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffRule = staff.CommissionRules.AddNew();
			var overallRule = new OverallStaffCommissionRule(staffRule);
			CombineAssertions(() =>
			{
				AssertReadOnly(true, overallRule.SourceDescriptionInfo);
				AssertReadOnly(false, overallRule.StatusInfo);

				AssertReadOnly(false, overallRule.CompanyPkInfo);
				AssertReadOnly(false, overallRule.GroupPkInfo);
				AssertReadOnly(false, overallRule.ProductInfo);
				AssertReadOnly(false, overallRule.ServiceInfo);
				AssertReadOnly(false, overallRule.SubModuleInfo);
				AssertReadOnly(false, overallRule.EndDateInfo);
				AssertReadOnly(false, overallRule.StartDateInfo);

				AssertReadOnly(false, overallRule.CommissionBasisInfo);
				AssertReadOnly(false, overallRule.CommissionTriggerTypeInfo);
			});
		}

		public void TestPropertiesReadOnly_InheritedGroupRule()
		{
			var group = Factory.New<SalesTeam>();
			var groupRule = group.CommissionRules.AddNew();
			var staff = group.Staff.AddNew();

			var overallRule = new OverallStaffCommissionRule(staff, groupRule);
			overallRule.Status = GroupCommissionRuleStatusTypes.Codes.Inherited;
			CombineAssertions(() =>
			{
				AssertReadOnly(true, overallRule.SourceDescriptionInfo);
				AssertReadOnly(false, overallRule.StatusInfo);

				AssertReadOnly(true, overallRule.CompanyPkInfo);
				AssertReadOnly(true, overallRule.GroupPkInfo);
				AssertReadOnly(true, overallRule.ProductInfo);
				AssertReadOnly(true, overallRule.ServiceInfo);
				AssertReadOnly(true, overallRule.SubModuleInfo);
				AssertReadOnly(true, overallRule.EndDateInfo);
				AssertReadOnly(true, overallRule.StartDateInfo);

				AssertReadOnly(true, overallRule.CommissionBasisInfo);
				AssertReadOnly(true, overallRule.CommissionTriggerTypeInfo);
			});
		}

		public void TestPropertiesReadOnly_OverridenGroupRule()
		{
			var group = Factory.New<SalesTeam>();
			var groupRule = group.CommissionRules.AddNew();
			var staff = group.Staff.AddNew();

			var overallRule = new OverallStaffCommissionRule(staff, groupRule);
			overallRule.Status = GroupCommissionRuleStatusTypes.Codes.Overridden;
			CombineAssertions(() =>
			{
				AssertReadOnly(true, overallRule.SourceDescriptionInfo);
				AssertReadOnly(false, overallRule.StatusInfo);

				AssertReadOnly(true, overallRule.CompanyPkInfo);
				AssertReadOnly(true, overallRule.GroupPkInfo);
				AssertReadOnly(true, overallRule.ProductInfo);
				AssertReadOnly(true, overallRule.ServiceInfo);
				AssertReadOnly(true, overallRule.SubModuleInfo);
				AssertReadOnly(true, overallRule.EndDateInfo);
				AssertReadOnly(true, overallRule.StartDateInfo);

				AssertReadOnly(false, overallRule.CommissionBasisInfo);
				AssertReadOnly(false, overallRule.CommissionTriggerTypeInfo);
			});
		}

		public void TestPropertiesReadOnly_DisabledGroupRule()
		{
			var group = Factory.New<SalesTeam>();
			var groupRule = group.CommissionRules.AddNew();
			var staff = group.Staff.AddNew();

			var overallRule = new OverallStaffCommissionRule(staff, groupRule);
			overallRule.Status = GroupCommissionRuleStatusTypes.Codes.Disabled;
			CombineAssertions(() =>
			{
				AssertReadOnly(true, overallRule.SourceDescriptionInfo);
				AssertReadOnly(false, overallRule.StatusInfo);

				AssertReadOnly(true, overallRule.CompanyPkInfo);
				AssertReadOnly(true, overallRule.GroupPkInfo);
				AssertReadOnly(true, overallRule.ProductInfo);
				AssertReadOnly(true, overallRule.ServiceInfo);
				AssertReadOnly(true, overallRule.SubModuleInfo);
				AssertReadOnly(true, overallRule.EndDateInfo);
				AssertReadOnly(true, overallRule.StartDateInfo);

				AssertReadOnly(true, overallRule.CommissionBasisInfo);
				AssertReadOnly(true, overallRule.CommissionTriggerTypeInfo);
			});
		}

		void AssertReadOnly(bool expectedReadOnly, ZPropertyInfo propertyInfo)
		{
			AssertReadOnly(null, expectedReadOnly, propertyInfo);
		}

		void AssertReadOnly(string message, bool expectedReadOnly, ZPropertyInfo propertyInfo)
		{
			AssertEquals(propertyInfo.Name + (message != null ? ": " + message : ""), expectedReadOnly, propertyInfo.ReadOnly);
		}

		public void TestDeleteWhenBaseRuleIsDeleted()
		{
			var salesTeam = Factory.New<SalesTeam>();
			var teamRule = salesTeam.CommissionRules.AddNew();
			teamRule.FillWithValidTestData();
			var staff = salesTeam.Staff.AddNew();
			staff.GS_Code = "ADL";

			Factory.Save();

			AssertEquals(1, staff.OverallCommissionRules.Count);
			var overallRule = staff.OverallCommissionRules[0];

			teamRule.Delete();

			AssertEquals(true, overallRule.IsDeleted);
			AssertEquals(0, staff.OverallCommissionRules.Count);
		}

		public void TestCanDelete()
		{
			var salesTeam = Factory.New<SalesTeam>();
			salesTeam.GG_Code = "ASAPC";
			var staff = salesTeam.Staff.AddNew();
			staff.GS_Code = "ADL";

			var staffRule = staff.CommissionRules.AddNew();
			var overallRule = new OverallStaffCommissionRule(staffRule);
			AssertEquals(true, staffRule.CanDelete);

			var teamRule = salesTeam.CommissionRules.AddNew();
			overallRule = new OverallStaffCommissionRule(staff, teamRule);
			AssertEquals(false, overallRule.CanDelete);
			AssertEquals("Can not delete this rule as it is inherited from Sales Team (ASAPC). If this rule does not apply to this staff, disable this rule instead.", overallRule.ReasonForNotAbleToDelete);
		}

		public void TestValidation()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffRule = staff.CommissionRules.AddNew();
			var overallRule = new OverallStaffCommissionRule(staffRule);
			AssertType(typeof(OverallStaffCommissionRuleValidation), overallRule.Validation);
		}

		public void TestLookups()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffRule = staff.CommissionRules.AddNew();
			var overallRule = new OverallStaffCommissionRule(staffRule);
			AssertType(typeof(OverallStaffCommissionRuleLookups), overallRule.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			return new OverallStaffCommissionRule(staff.CommissionRules.AddNew());
		}
	}
}
