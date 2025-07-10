using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSubscriptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckG0_Category()
		{
			var list = new CodeDescriptionBoolCollection
			{
				{ "***", (NoResString)"Category" }
			};
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_MediaCategory = "***";
			AssertNoErrors("Campaign Type is valid, should not have errors", subscription.GCS_MediaCategoryInfo);
			subscription.GCS_MediaCategory = ";;;";
			AssertHasErrors("Campaign Type is NOT valid, should have errors", subscription.GCS_MediaCategoryInfo);
			subscription.GCS_MediaCategory = ZString.Empty;
			AssertNoErrors("Campaign Type is valid, should not have errors", subscription.GCS_MediaCategoryInfo);
		}

		public void TestCheckG0_Category_EmptyList()
		{
			AssertCheckG0_Category_EmptyList(false);
		}

		public void TestCheckG0_Category_EmptyList_HRCampaign()
		{
			AssertCheckG0_Category_EmptyList(true);
		}

		void AssertCheckG0_Category_EmptyList(bool isHRCampaign)
		{
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());

			var staffWithoutAccess = GetStaffWithoutOrganisationControlSubscriptionPreferencesToAllAccess(Factory);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var subscription = Factory.New<GlbCompanyCampaignSubscription>();
				SetupHRCampaign(subscription, isHRCampaign);
				subscription.GCS_MediaCategory = ";;;";
				AssertHasErrors("Campaign Type is NOT valid, should have errors", subscription.GCS_MediaCategoryInfo);
				subscription.GCS_MediaCategory = ZString.Empty;
				AssertHasErrors("Campaign Type is NOT valid, should have errors", subscription.GCS_MediaCategoryInfo);
			}
		}

		public void TestCheckGCS_MediaType()
		{
			var collection = new CodeDescriptionBoolCollection
			{
				{ "AAA", (NoResString)"Category A", true }
			};
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var subscription = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			subscription.GCS_MediaType = "AAA";
			AssertNoErrors("Campaign Category is valid, should not have errors", subscription.GCS_MediaTypeInfo);
			subscription.GCS_MediaType = "CCC";
			AssertHasErrors("Campaign Category is NOT valid, should have errors", subscription.GCS_MediaTypeInfo);
			subscription.GCS_MediaType = ZString.Empty;
			AssertNoErrors("Campaign Category is valid, should not have errors", subscription.GCS_MediaTypeInfo);
		}

		public void TestCheckGCS_MediaType_EmptyList()
		{
			AssertCheckGCS_MediaType_EmptyList(false);
		}

		public void TestCheckGCS_MediaType_EmptyList_HRCampaign()
		{
			AssertCheckGCS_MediaType_EmptyList(true);
		}

		void AssertCheckGCS_MediaType_EmptyList(bool isHRCampaign)
		{
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());

			var staffWithoutAccess = GetStaffWithoutOrganisationControlSubscriptionPreferencesToAllAccess(Factory);

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var subscription = Factory.New<GlbCompanyCampaignSubscription>();
				SetupHRCampaign(subscription, isHRCampaign);
				subscription.GCS_MediaType = "CCC";
				AssertHasErrors("Campaign Category is NOT valid, should have errors", subscription.GCS_MediaTypeInfo);
				subscription.GCS_MediaType = ZString.Empty;
				AssertHasErrors("Campaign Category is NOT valid, should have errors", subscription.GCS_MediaTypeInfo);
			}
		}

		internal static GlbStaff GetStaffWithoutOrganisationControlSubscriptionPreferencesToAllAccess(BusinessObjectFactory factory)
		{
			var staffWithoutAccess = factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferencesToAll.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			factory.Save();

			return staffWithoutAccess;
		}

		void SetupHRCampaign(GlbCompanyCampaignSubscription subscription, bool isHRCampaign)
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_IsSalesAndMarketing = !isHRCampaign;
			subscription.GCS_G0 = campaign.PK;
		}

		public void TestEmailAndOrgHeaderAreEmpty()
		{
			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			subscription.Validation.ValidateAll();
			AssertHasErrors("Email is empty, OH is empty, should have errors.", subscription.GCS_EmailInfo);
			AssertHasErrors("Email is empty, OH is empty, should have errors.", subscription.GCS_OHInfo);
			AssertHasErrorContaining(subscription.GCS_EmailInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(subscription.GCS_OHInfo, MandatoryValidation.MustBeEntered);

			subscription.GCS_OH = ZGuid.BrettsGuid;
			subscription.GCS_Email = "e@ma.il";
			subscription.Validation.ValidateAll();
			AssertHasErrors("Email is NOT empty, OH is NOT empty, should have errors.", subscription.GCS_EmailInfo);
			AssertHasErrors("Email is NOT empty, OH is NOT empty, should have errors.", subscription.GCS_OHInfo);
			AssertHasErrorContaining(subscription.GCS_EmailInfo, MandatoryValidation.DoNotEntered);
			AssertHasErrorContaining(subscription.GCS_OHInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckGCS_EmailIsNotEmpty()
		{
			var subscription = Factory.New<GlbCompanyCampaignSubscription>();

			subscription.Validation.ValidateAll();
			AssertHasErrors("Email is empty, OH is empty, should have errors.", subscription.GCS_EmailInfo);
			AssertHasErrorContaining(subscription.GCS_EmailInfo, MandatoryValidation.MustBeEntered);

			subscription.GCS_OH = ZGuid.BrettsGuid;
			subscription.Validation.ValidateAll();
			AssertNoErrors("Email is empty, OH is NOT empty, should not have errors.", subscription.GCS_EmailInfo);
		}

		public void TestCheckGCS_OHIsValidZGuid()
		{
			var subscription = Factory.New<GlbCompanyCampaignSubscription>();

			subscription.Validation.ValidateAll();
			AssertHasErrors("Email is empty, OH is empty, should have errors.", subscription.GCS_OHInfo);
			AssertHasErrorContaining(subscription.GCS_OHInfo, MandatoryValidation.MustBeEntered);

			subscription.GCS_Email = "e@ma.il";
			subscription.Validation.ValidateAll();
			AssertNoErrors("Email is NOT empty, OH is empty, should not have errors.", subscription.GCS_OHInfo);
		}
	}
}
