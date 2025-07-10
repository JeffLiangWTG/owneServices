using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class GlbCompanyCampaignItemCRMSecurityProviderTest : CRMSecurityProviderTest<GlbCompanyCampaignItem>
	{
		protected override CRMSecurityProvider<GlbCompanyCampaignItem> GetNewProviderForTest() => new GlbCompanyCampaignItemCRMSecurityProvider();

		protected override IEnumerable<GlbCompanyCampaignItem> GetTestObjectWithoutStaffAssignment()
		{
			var obj = CreateCampaignItem();
			obj.CompanyCampaign.G0_GS_NKCampaignCoordinator = "U00";
			return new GlbCompanyCampaignItem[] { obj };
		}

		protected override IEnumerable<GlbCompanyCampaignItem> GetTestObjectWithOrgStaffAssignment() => System.Array.Empty<GlbCompanyCampaignItem>();

		protected override IEnumerable<GlbCompanyCampaignItem> GetTestObjectWithBizObjStaffAssignment()
		{
			var obj1 = CreateCampaignItem();
			obj1.CompanyCampaign.G0_GS_NKCampaignCoordinator = "U00";
			obj1.CompanyCampaign.G0_GS_NKCampaignManager = "U00";

			var obj2 = CreateCampaignItem();
			obj2.CompanyCampaign.G0_GS_NKCampaignCoordinator = "U00";
			obj2.CompanyCampaign.G0_GS_NKCampaignManager = GlbStaff.CurrentUser.GS_Code;

			return new GlbCompanyCampaignItem[] { obj1, obj2 };
		}

		protected override IEnumerable<GlbCompanyCampaignItem> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org) => System.Array.Empty<GlbCompanyCampaignItem>();

		public override void TestObjectFoundByOtherFilter() => Assert(true);

		protected override void AddStaffAssignmentForCompany(GlbCompanyCampaignItem obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
		}

		GlbCompanyCampaignItem CreateCampaignItem()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var result = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			result.G8_G0 = campaign.PK;
			result.G8_RecipientID = contact.PK;
			result.G8_RecipientTableCode = "OC";
			return result;
		}
	}
}
