using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	public sealed class GlbCompanyCampaignTestHelper
	{
		public static void PopulateCampaign(GlbCompanyCampaign campaign, BusinessObjectFactory factory, bool addName = false)
		{
			var categoryList = new CodeDescriptionBoolCollection();
			categoryList.Add("***", (NoResString)"Category");
			CodeDescriptionPairList stageList = new CodeDescriptionPairList();
			stageList.AddPair("***", "Category");

			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryList);
			OrganisationsDataRegistry.Instance.CampaignStageList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stageList);

			var collection = new CodeDescriptionBoolCollection();
			collection.Add("***", (NoResString)"Category");
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			staff.StaffPlainTextPassword = "1";
			staff.StaffConfirmPassword = "1";
			staff.GS_City = "AUSYD";
			staff.GS_FullName = "full name";
			staff.GS_UserAddress1 = "address1";

			campaign.G0_BatchCountDefault = 1;
			campaign.G0_Stage = "***";
			campaign.G0_Category = "***";
			campaign.G0_Type = "***";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;

			if (addName)
			{
				campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			}
		}
	}
}
