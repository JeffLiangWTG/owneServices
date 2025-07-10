using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCampaignContactCollection))]
	sealed class CampaignContactCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRemoveDuplicatesBasedOnEmailAddress()
		{
			var contacts = (GlbCampaignContactCollection)GetCollectionToTest();

			OrgContact contact1 = GetNewOrgContact();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "test@example.com";

			OrgContact contact2 = GetNewOrgContact();
			contact2.OC_ContactName = "Contact 2";
			contact2.OC_Email = "other@nowhere.com";

			OrgContact contact3 = GetNewOrgContact();
			contact3.OC_ContactName = "Contact 3";
			contact3.OC_Email = "test@example.com";

			OrgContact contact4 = GetNewOrgContact();
			contact4.OC_ContactName = "Contact 4";
			contact4.OC_Email = "test@example.com";

			OrgContact contact5 = GetNewOrgContact();
			contact5.OC_ContactName = "Contact 5";
			contact5.OC_Email = "other@nowhere.com";

			OrgContact contact6 = GetNewOrgContact();
			contact6.OC_ContactName = "Contact 6";
			contact6.OC_Email = "";

			OrgContact contact7 = GetNewOrgContact();
			contact7.OC_ContactName = "Contact 7";
			contact7.OC_Email = "";

			OrgColdCallRegister callRegister = GetNewOrgColdCallRegister();
			callRegister.O1_Email = "other@nowhere.com";
			callRegister.O1_ContactName = "Contact Rgd";

			Factory.Save();

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();

			contacts = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3, contact4, contact5, contact6, contact7, callRegister }, campaign);
			AssertEquals(7, contacts.Count);
			contacts.RemoveDuplicatesBasedOnEmailAddress(campaign.AdditionalFilter);
			AssertEquals(4, contacts.Count);
			Assert(!contacts.Any(contact => contact.PK == callRegister.PK));
		}

		public void TestRemoveDuplicatesBasedOnEmailAddress_CampaignItemWithoutRecipient()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			var contacts = new GlbCampaignContactCollection(campaign);
			contacts.RemoveDuplicatesBasedOnEmailAddress(new ZQuery(ViewCampaignContactSchema.VCC_ContactName, "AGENCY OFFICE"));

			AssertEquals(1, contacts.Count);
		}

		#region Implementation

		OrgContact GetNewOrgContact()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Title = "0826CB58";
			return orgContact;
		}

		OrgColdCallRegister GetNewOrgColdCallRegister()
		{
			var orgColdCallRegister = Factory.NewWithValidTestData<OrgColdCallRegister>();
			orgColdCallRegister.O1_Fax = "FAXFAX";
			return orgColdCallRegister;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new GlbCampaignContactCollection(campaign);
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public override void TestAddAndDeleteOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		public GlbCampaignContactCollection ContactCollection(List<BusinessObject> bizOList, GlbCompanyCampaign campaign)
		{
			GlbCampaignContactCollection contactCollection = new GlbCampaignContactCollection(campaign);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			if (bizOList != null)
			{
				bizOList.ForEach(item =>
				{
					if (!item.IsDeleted)
					{
						query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.PK, item.PK);
					}
				});
			}

			query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "0826CB58");

			if (!campaign.AdditionalFilter.IsEmpty)
			{
				query.AddToFilter(campaign.AdditionalFilter);
			}
			campaign.AdditionalFilter = query;
			contactCollection.Load(query);

			return contactCollection;
		}

		#endregion
	}
}
