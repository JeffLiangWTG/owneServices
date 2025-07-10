using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	abstract class GlbCompanyCampaignSubscriptionTest<T> : EnterpriseBusinessObjectTestCase
			where T : GlbCompanyCampaignSubscription
	{
		public virtual void TestGCS_G0_ReadOnly()
		{
			var gccs = Factory.New<T>();
			Assert("Should always be readonly", gccs.GCS_G0Info.ReadOnly);
		}

		public virtual void TestMediaCategory_ReadOnly()
		{
			var gccs = Factory.New<T>();
			Assert("Should be empty", gccs.GCS_G0.IsEmpty);
			Assert("Should be not readonly", !gccs.GCS_MediaCategoryInfo.ReadOnly);
			gccs.GCS_G0 = ZGuid.BrettsGuid;
			Assert("Should be not readonly", !gccs.GCS_MediaCategoryInfo.ReadOnly);
		}

		public virtual void TestMediaCategoryWithAll_ReadOnly()
		{
			var gccs = Factory.New<T>();
			Assert("Should be empty", gccs.GCS_G0.IsEmpty);
			Assert("Should be not readonly", !gccs.GCS_MediaCategoryWithAllInfo.ReadOnly);
			gccs.GCS_G0 = ZGuid.BrettsGuid;
			Assert("Should be not readonly", !gccs.GCS_MediaCategoryWithAllInfo.ReadOnly);
		}

		public virtual void TestMediaType_ReadOnly()
		{
			var gccs = Factory.New<T>();
			Assert("Should be empty", gccs.GCS_G0.IsEmpty);
			Assert("Should be not readonly", !gccs.GCS_MediaTypeInfo.ReadOnly);
			gccs.GCS_G0 = ZGuid.BrettsGuid;
			Assert("Should be not readonly", !gccs.GCS_MediaTypeInfo.ReadOnly);
		}

		public virtual void TestMediaTypeWithAll_ReadOnly()
		{
			var gccs = Factory.New<T>();
			Assert("Should be empty", gccs.GCS_G0.IsEmpty);
			Assert("Should be not readonly", !gccs.GCS_MediaTypeWithAllInfo.ReadOnly);
			gccs.GCS_G0 = ZGuid.BrettsGuid;
			Assert("Should be not readonly", !gccs.GCS_MediaTypeWithAllInfo.ReadOnly);
		}

		public virtual void TestSubscriptionType_ReadOnly()
		{
			var gccs = Factory.New<T>();
			Assert("Should be empty", gccs.GCS_G0.IsEmpty);
			Assert("Should be not readonly", !gccs.GCS_IsSubscribedInfo.ReadOnly);
			gccs.GCS_G0 = ZGuid.BrettsGuid;
			Assert("Should be not readonly", !gccs.GCS_IsSubscribedInfo.ReadOnly);
		}

		public void TestMediaCategory_SpecialAllValue()
		{
			var gccs = Factory.New<T>();
			AssertEquals("Default empty value shoud be empty", "", gccs.GCS_MediaCategory);
			AssertEquals("Default empty value shoud be ALL", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaCategoryWithAll);

			gccs.GCS_MediaCategoryWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			AssertEquals("Value shoud be empty", "", gccs.GCS_MediaCategory);
			AssertEquals("Value shoud be ALL", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaCategoryWithAll);

			gccs.GCS_MediaCategory = "";
			AssertEquals("Empty value shoud be empty", "", gccs.GCS_MediaCategory);
			AssertEquals("Empty value shoud be ALL", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaCategoryWithAll);
		}

		public void TestMediaCategory_SpecialAllValueWithNoRightsAndEmptyList()
		{
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());

			var staffWithoutAccess = GlbCompanyCampaignSubscriptionValidationTest.GetStaffWithoutOrganisationControlSubscriptionPreferencesToAllAccess(Factory);
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var gccs = Factory.New<T>();
				AssertEquals("Default empty value shoud be empty", ZString.Empty, gccs.GCS_MediaCategory);
				AssertEquals("Should be ALL regardless of security rights", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaCategoryWithAll);
			}
		}

		public void TestGCS_MediaCategoryWithAll_AllCampaignCategoryAndTypeCodeReturnedForEmptyCategory()
		{
			var gccs = Factory.New<T>();

			Env.Security.OrganisationControlSubscriptionPreferencesToAll.IsAllowed = true;
			AssertEquals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaCategoryWithAll);

			Env.Security.OrganisationControlSubscriptionPreferencesToAll.IsAllowed = false;
			AssertEquals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaCategoryWithAll);
		}

		public void TestGCS_MediaCategoryWithAll_ReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			contact.OC_ContactName = "eclipse";
			contact.OC_Email = "eclipse@wisetechglobal.com";
			var contactSub1 = (T)contact.Subscriptions.AddNew();
			var contactSub2 = (T)contact.Subscriptions.AddNew();
			var contactSub3 = (T)contact.Subscriptions.AddNew();
			var contactSub4 = (T)contact.Subscriptions.AddNew();

			contactSub1.GCS_MediaCategoryWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub1.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub1.GCS_IsSubscribed = true;

			contactSub2.GCS_MediaCategoryWithAll = "PRINT";
			contactSub2.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub2.GCS_IsSubscribed = true;

			contactSub3.GCS_MediaCategoryWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub3.GCS_MediaTypeWithAll = "PROBE";
			contactSub3.GCS_IsSubscribed = true;

			contactSub4.GCS_MediaCategoryWithAll = "RADIO";
			contactSub4.GCS_MediaTypeWithAll = "PREAP";
			contactSub4.GCS_IsSubscribed = true;

			Factory.Save();

			Env.Security.OrganisationControlSubscriptionPreferencesToAll.IsAllowed = true;
			Assert(!contactSub1.ReadOnly);
			Assert(!contactSub2.ReadOnly);
			Assert(!contactSub3.ReadOnly);
			Assert(!contactSub4.ReadOnly);

			Env.Security.OrganisationControlSubscriptionPreferencesToAll.IsAllowed = false;
			Assert(contactSub1.ReadOnly);
			Assert(contactSub2.ReadOnly);
			Assert(contactSub3.ReadOnly);
			Assert(!contactSub4.ReadOnly);
		}

		public void TestMediaType_SpecialAllValue()
		{
			var gccs = Factory.New<T>();
			AssertEquals("Default empty value shoud be empty", "", gccs.GCS_MediaType);
			AssertEquals("Default empty value shoud be ALL", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaTypeWithAll);

			gccs.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			AssertEquals("Value shoud be empty", "", gccs.GCS_MediaType);
			AssertEquals("Value shoud be ALL", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaTypeWithAll);

			gccs.GCS_MediaType = "";
			AssertEquals("Empty value shoud be empty", "", gccs.GCS_MediaType);
			AssertEquals("Empty value shoud be ALL", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaTypeWithAll);
		}

		public void TestMediaType_SpecialAllValueWithNoRightsAndEmptyList()
		{
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection());

			var staffWithoutAccess = GlbCompanyCampaignSubscriptionValidationTest.GetStaffWithoutOrganisationControlSubscriptionPreferencesToAllAccess(Factory);
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var gccs = Factory.New<T>();
				AssertEquals("Default empty value shoud be empty", ZString.Empty, gccs.GCS_MediaType);
				AssertEquals("Should be ALL regardless of security rights", GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode, gccs.GCS_MediaTypeWithAll);
			}
		}

		public void TestIsOrgLevel()
		{
			var gccs = Factory.New<GlbCompanyCampaignSubscriptionForContact>();
			AssertEquals("Always readonly", true, gccs.GCS_IsOrgLevelInfo.ReadOnly);
			AssertEquals("Not Org level", false, gccs.GCS_IsOrgLevel);
			gccs.GCS_OH = ZGuid.BrettsGuid;
			AssertEquals("Is Org level", true, gccs.GCS_IsOrgLevel);
		}

		public void TestResettingOrgLevelSubscriptionsShouldResetContactLevelSubscriptions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_ContactName = "eclipse";
			contact1.OC_Email = "eclipse@wisetechglobal.com";
			contact2.OC_ContactName = "ellipse";
			contact2.OC_Email = "ellipse@wisetechglobal.com";
			var contactSub1 = (T)contact1.Subscriptions.AddNew();
			var contactSub2 = (T)contact1.Subscriptions.AddNew();
			var contactSub3 = (T)contact1.Subscriptions.AddNew();
			var contactSub4 = (T)contact1.Subscriptions.AddNew();
			var contactSub5 = (T)contact1.Subscriptions.AddNew();
			var contactSub6 = (T)contact2.Subscriptions.AddNew();
			var orgSub = org.Subscriptions.AddNew();

			contactSub1.GCS_MediaCategoryWithAll = "PRINT";
			contactSub1.GCS_MediaTypeWithAll = "PREAP";
			contactSub1.GCS_IsSubscribed = true;

			contactSub2.GCS_MediaCategoryWithAll = "PRINT";
			contactSub2.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub2.GCS_IsSubscribed = true;

			contactSub3.GCS_MediaCategoryWithAll = "PRINT";
			contactSub3.GCS_MediaTypeWithAll = "PROBE";
			contactSub3.GCS_IsSubscribed = true;

			contactSub4.GCS_MediaCategoryWithAll = "RADIO";
			contactSub4.GCS_MediaTypeWithAll = "PREAP";
			contactSub4.GCS_IsSubscribed = true;

			contactSub5.GCS_MediaCategoryWithAll = "RADIO";
			contactSub5.GCS_MediaTypeWithAll = "EXIST";
			contactSub5.GCS_IsSubscribed = true;

			contactSub6.GCS_MediaCategoryWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub6.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			contactSub6.GCS_IsSubscribed = true;

			orgSub.GCS_MediaCategoryWithAll = "PRINT";
			orgSub.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			orgSub.GCS_IsSubscribed = true;
			AssertEquals(true, orgSub.GCS_IsOrgLevel);

			Factory.Save();

			contactSub2.GCS_IsSubscribed = false;
			Factory.Save();

			AssertEquals(true, contactSub1.GCS_IsSubscribed);
			contactSub2.GCS_IsSubscribed = true;
			Factory.Save();

			orgSub.GCS_IsSubscribed = false;
			AssertEquals(true, contactSub1.GCS_IsSubscribed);
			Factory.Save();

			AssertEquals(false, contactSub1.GCS_IsSubscribed);
			AssertEquals(false, contactSub2.GCS_IsSubscribed);
			AssertEquals(false, contactSub3.GCS_IsSubscribed);
			AssertEquals(true, contactSub4.GCS_IsSubscribed);
			AssertEquals(true, contactSub5.GCS_IsSubscribed);
			AssertEquals(true, contactSub6.GCS_IsSubscribed);

			orgSub.GCS_IsSubscribed = true;
			Factory.Save();

			AssertEquals(false, contactSub1.GCS_IsSubscribed);
			AssertEquals(false, contactSub2.GCS_IsSubscribed);
			AssertEquals(false, contactSub3.GCS_IsSubscribed);
			AssertEquals(true, contactSub4.GCS_IsSubscribed);
			AssertEquals(true, contactSub5.GCS_IsSubscribed);
			AssertEquals(true, contactSub6.GCS_IsSubscribed);

			orgSub.GCS_MediaCategoryWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			orgSub.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			orgSub.GCS_IsSubscribed = false;
			Factory.Save();

			AssertEquals(false, contactSub1.GCS_IsSubscribed);
			AssertEquals(false, contactSub2.GCS_IsSubscribed);
			AssertEquals(false, contactSub3.GCS_IsSubscribed);
			AssertEquals(false, contactSub4.GCS_IsSubscribed);
			AssertEquals(false, contactSub5.GCS_IsSubscribed);
			AssertEquals(false, contactSub6.GCS_IsSubscribed);
		}
		public void TestALLTypeSecurity()
		{
			AssertALLTypeSecurity();
		}

		public void TestALLTypeSecurity_WithEmptyRegistrySettings()
		{
			var emptyList = new CodeDescriptionBoolCollection();
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyList);
			AssertALLTypeSecurity();
		}

		void AssertALLTypeSecurity()
		{
			var staffWithoutAccess = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferencesToAll.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var gccs = Factory.New<T>();
				gccs.GCS_Email = "not.a@email.com";
				gccs.GCS_MediaCategoryWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
				gccs.GCS_MediaTypeWithAll = GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode;

				AssertEquals(true, gccs.CanDelete);
				AssertEquals(false, gccs.ReadOnly);

				Factory.Save();

				var reloaded_gccs = Factory.Load<T>(gccs.PK);

				AssertEquals(false, reloaded_gccs.CanDelete);
				AssertEquals(true, reloaded_gccs.ReadOnly);
			}
		}
	}

	[TestedType(typeof(GlbCompanyCampaignSubscription))]
	class GlbCompanyCampaignSubscriptionTest : GlbCompanyCampaignSubscriptionTest<GlbCompanyCampaignSubscription>
	{
	}
}
