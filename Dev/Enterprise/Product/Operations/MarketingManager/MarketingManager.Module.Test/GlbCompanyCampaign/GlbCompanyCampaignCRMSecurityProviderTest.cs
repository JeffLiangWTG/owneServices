using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class GlbCompanyCampaignCRMSecurityProviderTest : CRMSecurityProviderTest<GlbCompanyCampaign>
	{
		protected override CRMSecurityProvider<GlbCompanyCampaign> GetNewProviderForTest() => new GlbCompanyCampaignCRMSecurityProvider();

		protected override IEnumerable<GlbCompanyCampaign> GetTestObjectWithoutStaffAssignment()
		{
			var obj = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			obj.G0_GS_NKCampaignCoordinator = obj.G0_GS_NKCampaignManager = "U00";
			return new GlbCompanyCampaign[] { obj };
		}

		protected override IEnumerable<GlbCompanyCampaign> GetTestObjectWithOrgStaffAssignment() => System.Array.Empty<GlbCompanyCampaign>();

		protected override IEnumerable<GlbCompanyCampaign> GetTestObjectWithBizObjStaffAssignment()
		{
			var obj1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			obj1.G0_GS_NKCampaignCoordinator = GlbStaff.CurrentUser.GS_Code;
			obj1.G0_GS_NKCampaignManager = "U00";

			var obj2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			obj2.G0_GS_NKCampaignCoordinator = "U00";
			obj2.G0_GS_NKCampaignManager = GlbStaff.CurrentUser.GS_Code;

			return new GlbCompanyCampaign[] { obj1, obj2 };
		}

		protected override IEnumerable<GlbCompanyCampaign> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org) => System.Array.Empty<GlbCompanyCampaign>();

		protected override void AddStaffAssignmentForCompany(GlbCompanyCampaign obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
		}

		public override void TestObjectFoundByOtherFilter() => Assert(true);
	}
}
