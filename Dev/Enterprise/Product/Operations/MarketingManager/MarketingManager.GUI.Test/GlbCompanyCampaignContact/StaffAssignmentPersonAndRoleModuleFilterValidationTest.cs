using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class StaffAssignmentPersonAndRoleModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateStaffAssignmentPerson()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NEO";
			Factory.Save();

			var filter = GetNewFilter();
			filter.StaffAssignmentPerson = "ZZZ";
			AssertHasErrors(filter.StaffAssignmentPersonInfo);

			filter.StaffAssignmentPerson = "NEO";
			AssertNoErrors(filter.StaffAssignmentPersonInfo);
		}

		public void TestValidateStaffAssignmentRole()
		{
			var filter = GetNewFilter();
			filter.StaffAssignmentRole = "SAL";
			AssertNoErrors(filter.StaffAssignmentRoleInfo);

			filter.StaffAssignmentRole = "EEE";
			AssertHasErrors(filter.StaffAssignmentRoleInfo);
		}

		StaffAssignmentPersonAndRoleModuleFilter GetNewFilter()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignContactFilterBusinessObject filterBizO = new GlbCompanyCampaignContactFilterBusinessObject(campaign);
			return new StaffAssignmentPersonAndRoleModuleFilter("Test", filterBizO);
		}
	}
}
