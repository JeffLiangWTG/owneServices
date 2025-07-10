using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI.CampaignManagement
{
	public class HRCampaignFilterStrip_Test : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var strip = new HRCampaignFilterStrip())
			{
				strip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				var result = strip.InternalGetCurrentFilterControls(new StaffSecurityModuleFilter("hello", new GlbBranchCollection(Factory), new GlbDepartmentCollection(Factory)));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(StaffSecurityFilterControl), result[0].GetType());
				result[0].Dispose();
				var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
				result = strip.InternalGetCurrentFilterControls(new StaffAssignmentPersonAndRoleModuleFilter("hello", new HRGlbCompanyCampaignContactFilterBusinessObject(campaign)));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(StaffAssignmentPersonAndRoleFilterControl), result[0].GetType());
				result[0].Dispose();
				result = strip.InternalGetCurrentFilterControls(new ModuleTextFilter("hello", GlbStaffSchema.GS_Code));
				AssertNull(result);
			}
		}
	}
}
