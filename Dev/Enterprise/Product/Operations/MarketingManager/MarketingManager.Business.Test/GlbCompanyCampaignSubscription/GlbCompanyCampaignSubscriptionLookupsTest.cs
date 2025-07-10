using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSubscriptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public GlbCompanyCampaignSubscriptionLookupsTest()
		{
			subscription = new Lazy<GlbCompanyCampaignSubscription>(() => Factory.New<GlbCompanyCampaignSubscription>());
		}

		GlbCompanyCampaignSubscription Subscription => subscription.Value;

		public void TestCampaigns()
		{
			AssertNotNull("Campaigns List should not be null", Subscription.Lookups.Campaigns);
		}

		public void TestMediaTypeList()
		{
			AssertNotNull("Media Types List should not be null", Subscription.Lookups.MediaTypeList);
		}

		public void TestMediaTypeWithAllList()
		{
			var mediaTypeList = Subscription.Lookups.MediaTypeWithAllList;
			AssertNotNull("Media Types List should not be null", mediaTypeList);
			AssertEquals("Should have ALL value", true, mediaTypeList.ContainsCode("ALL"));
			AssertEquals("Description", "All Types", mediaTypeList.GetDescriptionFromCode("ALL"));
		}

		public void TestMediaCategoryList()
		{
			AssertNotNull("Media Category List should not be null", Subscription.Lookups.MediaCategoryList);
		}

		public void TestMediaCategoryWithAllList()
		{
			var mediaCategoryList = Subscription.Lookups.MediaCategoryWithAllList;
			AssertNotNull("Media Category List should not be null", mediaCategoryList);
			AssertEquals("Should have ALL value", true, mediaCategoryList.ContainsCode("ALL"));
			AssertEquals("Description", "All Categories", mediaCategoryList.GetDescriptionFromCode("ALL"));
		}

		public void TestMediaTypeListForHRCampaign()
		{
			var collectionA = new CodeDescriptionBoolCollection
			{
				{ "AAA", (NoResString)"Category A", true }
			};
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionA);

			var collectionB = new CodeDescriptionBoolCollection
			{
				{ "BBB", (NoResString)"Category A", true }
			};
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionB);

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_IsSalesAndMarketing = false;
			var campaignB = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaignB.G0_IsSalesAndMarketing = true;

			Factory.Save();

			Subscription.GCS_G0 = campaign.PK;
			Assert("Collection should use the HR list from the registry", !Subscription.Lookups.MediaTypeList.ContainsCode(collectionA[0].Code));
			Assert("Collection should use the HR list from the registry", Subscription.Lookups.MediaTypeList.ContainsCode(collectionB[0].Code));

			var newFactory = new BusinessObjectFactory();
			var subscription = new Lazy<GlbCompanyCampaignSubscription>(() => Factory.New<GlbCompanyCampaignSubscription>());
			var newSubscription = subscription.Value;
			newSubscription.GCS_G0 = campaignB.PK;

			Assert("Collection should use the CRM list from the registry", newSubscription.Lookups.MediaTypeList.ContainsCode(collectionA[0].Code));
			Assert("Collection should use the CRM list from the registry", !newSubscription.Lookups.MediaTypeList.ContainsCode(collectionB[0].Code));
		}

		readonly Lazy<GlbCompanyCampaignSubscription> subscription;

		public void TestALLTypeSecurity()
		{
			var staffWithoutAccess = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals(false, Subscription.Lookups.MediaCategoryWithAllList.ContainsCode(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode));
					AssertEquals(false, Subscription.Lookups.MediaTypeWithAllList.ContainsCode(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode));
				}
			}
		}
	}
}
