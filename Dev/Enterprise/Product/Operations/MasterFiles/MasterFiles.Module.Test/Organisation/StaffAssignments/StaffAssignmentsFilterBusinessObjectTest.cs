using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test.Organisation.StaffAssignments
{
	[TestedType(typeof(StaffAssignmentsFilterBusinessObject))]
	public class StaffAssignmentsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestRoleTextFilter()
		{
			var filter = (ModuleTextFilter)filterStripBizO["Role"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			filter.Property = StaffAssignmentRoles.Codes.AccountManager;
			AssertFilterResults(new[] { orgStaffAssignment1, orgStaffAssignment2 });

			filter.Property = StaffAssignmentRoles.Codes.SalesRep;
			AssertFilterResults(new[] { orgStaffAssignment3 });
		}

		public void TestRoleTextFilter_DropdownList()
		{
			var filter = (ModuleTextFilter)filterStripBizO["Role"];
			AssertContainsExactElementsInExactOrder(Env.Registry.OrgStaffMemberAssignmentRoles, filter.List);
		}

		public void TestStaffGuidFilter()
		{
			var filter = (ModuleGuidFilter)filterStripBizO["Staff"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			filter.Property = staff1.PK;
			AssertFilterResults(new[] { orgStaffAssignment1, orgStaffAssignment4 });

			filter.Property = staff2.PK;
			AssertFilterResults(new[] { orgStaffAssignment2, orgStaffAssignment3 });
		}

		public void TestDepartmentTextFilter()
		{
			var filter = (ModuleTextFilter)filterStripBizO["Department"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			filter.Property = "BBB";
			AssertFilterResults(new[] { orgStaffAssignment2 });

			filter.Property = "ALL";
			AssertFilterResults(new[] { orgStaffAssignment3 });
		}

		public void TestDepartmentTextFilter_DropdownList()
		{
			var filter = (ModuleTextFilter)filterStripBizO["Department"];
			AssertContainsExactElementsInExactOrder(OrgStaffAssignmentsLookupsImplementer.Get(Factory).DepartmentCodes, filter.List);
		}

		public void TestCompanyGuidFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			orgStaffAssignment1.O8_GC = company1.PK;
			orgStaffAssignment2.O8_GC = ZGuid.Empty;
			orgStaffAssignment3.O8_GC = company1.PK;
			orgStaffAssignment4.O8_GC = company2.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterStripBizO["Company"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			filter.Property = company1.PK;
			AssertFilterResults(new[] { orgStaffAssignment1, orgStaffAssignment3 });

			filter.Property = company2.PK;
			AssertFilterResults(new[] { orgStaffAssignment4 });
		}

		public void TestCompanyGuidFilter_ReadOnlyIfViewingOtherCompanyAssignmentsIsDisallowed()
		{
			Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = true;
			var filter = (ModuleGuidFilter)filterStripBizO["Company"];
			Assert("Company filter should be editable when security checkpoint is allowed", !filter.ReadOnly);

			Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = false;
			filter = (ModuleGuidFilter)new StaffAssignmentsFilterBusinessObject()["Company"];
			Assert("Company filter should not be editable when security checkpoint is disallowed", filter.ReadOnly);
			AssertEquals("Company filter should be set to current company when security checkpoint is disallowed", GlbCompany.CurrentCompany.PK, filter.Property);
		}

		public void TestFiltersAreAlwaysVisible()
		{
			AssertEquals("Role", FilterVisibility.AlwaysVisible, filterStripBizO["Role"].Visibility);
			AssertEquals("Staff", FilterVisibility.AlwaysVisible, filterStripBizO["Staff"].Visibility);
			AssertEquals("Department", FilterVisibility.AlwaysVisible, filterStripBizO["Department"].Visibility);
			AssertEquals("Company", FilterVisibility.AlwaysVisible, filterStripBizO["Company"].Visibility);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StaffAssignmentsFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			collection = new ActiveBusinessObjectCollection<OrgStaffAssignments>(Factory);
			filterStripBizO = new StaffAssignmentsFilterBusinessObject();

			staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff2 = Factory.NewWithValidTestData<GlbStaff>();

			orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "AAA";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;

			orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "BBB";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;

			orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = staff2.GS_Code;

			orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_Department = "AAA";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = staff1.GS_Code;

			Factory.Save();
		}

		void AssertFilterResults(OrgStaffAssignments[] expectedResults)
		{
			var results = collection.Find(filterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Staff assignment 1", expectedResults.Contains(orgStaffAssignment1), results.Contains(orgStaffAssignment1));
				AssertEquals("Staff assignment 2", expectedResults.Contains(orgStaffAssignment2), results.Contains(orgStaffAssignment2));
				AssertEquals("Staff assignment 3", expectedResults.Contains(orgStaffAssignment3), results.Contains(orgStaffAssignment3));
				AssertEquals("Staff assignment 4", expectedResults.Contains(orgStaffAssignment4), results.Contains(orgStaffAssignment4));
			});
		}

		ActiveBusinessObjectCollection<OrgStaffAssignments> collection;
		StaffAssignmentsFilterBusinessObject filterStripBizO;
		GlbStaff staff1;
		GlbStaff staff2;
		OrgStaffAssignments orgStaffAssignment1;
		OrgStaffAssignments orgStaffAssignment2;
		OrgStaffAssignments orgStaffAssignment3;
		OrgStaffAssignments orgStaffAssignment4;
	}
}

