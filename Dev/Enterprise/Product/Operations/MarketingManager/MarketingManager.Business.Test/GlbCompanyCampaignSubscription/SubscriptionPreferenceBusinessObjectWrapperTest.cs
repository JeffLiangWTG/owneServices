using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SubscriptionPreferenceBusinessObjectAdapter))]
	sealed class SubscriptionPreferenceBusinessObjectWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProcessUnsubscribeWithSubscriptionProperties()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var gccu = new SubscriptionPreferenceBusinessObjectAdapter(Factory, campaignItem.PK, UnsubscribeType.Sender.ToString("G"), null);
			var subscriptionProperties = new SubscriptionProperties();
			subscriptionProperties.CampaignPK = campaign.PK;
			subscriptionProperties.MediaCategoryWithAll = "CT";
			subscriptionProperties.MediaTypeWithAll = SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode;
			subscriptionProperties.IsOrgLevel = false;
			subscriptionProperties.IsSubscribed = true;
			gccu.Process(subscriptionProperties, true);

			AssertEquals("You have successfully changed the subscription preferences.", gccu.ResultMessage);
			Assert(!gccu.UnsubscribeItemPk.IsEmpty);

			var unsubscribeItem = Factory.Load<GlbCompanyCampaignSubscription>(gccu.UnsubscribeItemPk);
			AssertNotNull(unsubscribeItem);
			var orgContact = (OrgContact)campaignItem.Recipient;

			AssertEquals("Empty Org reference", true, unsubscribeItem.GCS_OH.IsEmpty);
			AssertEquals(campaignItem.EmailAddress, unsubscribeItem.GCS_Email);

			AssertEquals(true, unsubscribeItem.GCS_IsSubscribed);
			AssertNullOrEmpty(unsubscribeItem.GCS_MediaType);
			AssertEquals(campaign.G0_Category, unsubscribeItem.GCS_MediaCategory);
			AssertNullOrEmpty(unsubscribeItem.GCS_MediaType);
		}

		public void TestSubscriptionListSetup()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = "DRM";

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			campaign.G0_G0_Master = master.PK;
			campaign.G0_GC = company.PK;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_TrackingStatus = "QUE";

			var existingSubscription1 = Factory.New<GlbCompanyCampaignSubscription>();
			existingSubscription1.GCS_OH = org1.PK;
			existingSubscription1.GCS_MediaCategory = "";
			existingSubscription1.GCS_MediaType = "";
			existingSubscription1.GCS_IsSubscribed = true;

			var systemSubcriptionRules = GetValidRegistryValues();
			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemSubcriptionRules);
			campaign.G0_PublishedListCode = OrganisationsDataRegistry.Instance.SubscriptionRules.Value.DefaultCRM.Code;

			Factory.Save();
			var unsub = new SubscriptionPreferenceBusinessObjectAdapter(Factory, campaignItem1.PK, "", "");
			var subscriptionList = unsub.SubscriptionList;

			AssertEquals("Use system level registry", 1, subscriptionList.Count);
			AssertEquals(true, subscriptionList[0].IsSubscribed);

			var existingSubscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			existingSubscription2.GCS_OH = org1.PK;
			existingSubscription2.GCS_MediaCategory = "CT";
			existingSubscription2.GCS_MediaType = "";
			existingSubscription2.GCS_IsSubscribed = false;

			subscriptionList = unsub.SubscriptionList;
			AssertEquals(false, subscriptionList[0].IsSubscribed);

			var existingSubscription3 = Factory.New<GlbCompanyCampaignSubscription>();
			existingSubscription3.GCS_OH = org1.PK;
			existingSubscription3.GCS_MediaCategory = "CT";
			existingSubscription3.GCS_MediaType = "TP";
			existingSubscription3.GCS_IsSubscribed = true;

			subscriptionList = unsub.SubscriptionList;
			AssertEquals(true, subscriptionList[0].IsSubscribed);

			var companySubcriptionRules2 = new SubscriptionRuleCollection();
			var rule2 = companySubcriptionRules2.AddNewRule("CD1", (NoResString)"Default Pubished List AAA", true, false, new string[] { "CT;TP;DESC1;SUM1", "AA;BB;DESC2;SUM2" });
			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(campaign.G0_GC.ToGuid(), Guid.Empty, Guid.Empty, companySubcriptionRules2);

			unsub = new SubscriptionPreferenceBusinessObjectAdapter(Factory, campaignItem1.PK, "", "");
			subscriptionList = unsub.SubscriptionList;
			AssertEquals("Use company level registry", 2, subscriptionList.Count);
		}

		public void TestGetSubscriptionList()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = "DRM";

			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_OH = orgHeader.PK;
			subscription.GCS_MediaCategory = "";
			subscription.GCS_MediaType = "";
			subscription.GCS_IsSubscribed = true;

			var systemSubcriptionRules = GetValidRegistryValues();
			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemSubcriptionRules);

			Factory.Save();

			var unsub = new SubscriptionPreferenceBusinessObjectAdapter(Factory, ZGuid.NewZGuid(), "", "");
			AssertNull(unsub.SubscriptionList);
		}

		SubscriptionRuleCollection GetValidRegistryValues()
		{
			var rules1 = new SubscriptionRuleCollection();
			var rule1 = rules1.AddNewRule("CD1", (NoResString)"Default Pubished List AAA", false, false, new string[] { "CT;TP;DESC1;SUM1" });
			rule1.IsDefault = true;
			return rules1;
		}

		public void TestContactSubsctiptions()
		{
			var adapter = new SubscriptionPreferenceBusinessObjectAdapterForTest(Factory, ZGuid.Empty, "", "");
			AssertNull(adapter.ContactSubscriptions_Exposed);

			adapter = new SubscriptionPreferenceBusinessObjectAdapterForTest(Factory, ZGuid.Empty, "");
			AssertNull(adapter.ContactSubscriptions_Exposed);
		}

		class SubscriptionPreferenceBusinessObjectAdapterForTest : SubscriptionPreferenceBusinessObjectAdapter
		{
			public SubscriptionPreferenceBusinessObjectAdapterForTest(BusinessObjectFactory factory, ZGuid campaignItemPk, string unsubscribeTypeString, string resubscribe) : base(factory, campaignItemPk, unsubscribeTypeString, resubscribe)
			{
			}

			public SubscriptionPreferenceBusinessObjectAdapterForTest(BusinessObjectFactory factory, ZGuid contactPK, ZString code) : base(factory, contactPK, code)
			{
			}

			public GlbCompanyCampaignSubscriptionForOrganisationContactCollection ContactSubscriptions_Exposed
			{
				get { return base.ContactSubscriptions; }
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new SubscriptionPreferenceBusinessObjectAdapter(Factory, ZGuid.BrettsGuid, "CT");

		protected override void SetUp()
		{
			base.SetUp();
			var category1List = new CodeDescriptionBoolCollection
			{
				{ "CT", (NoResString)"CT Desc" },
				{ "AA", (NoResString)"AA Desc" }
			};
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, category1List);

			var category2List = new CodeDescriptionBoolCollection
			{
				{ "TP", (NoResString)"TP Desc" },
				{ "BB", (NoResString)"BB Desc" }
			};
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, category2List);
		}
	}
}
