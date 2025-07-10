using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class StaffReportingManagerRoleModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateReportingRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var filter = GetNewFilter();
			filter.ReportingRole = "DRM";
			AssertNoErrors(filter.ReportingRoleInfo);

			filter.ReportingRole = "EEE";
			AssertHasErrors(filter.ReportingRoleInfo);
		}

		StaffReportingManagerRoleModuleFilter GetNewFilter()
		{
			var filterBizO = new GlbStaffFilterBusinessObject();
			return new StaffReportingManagerRoleModuleFilter("Test");
		}
	}
}
