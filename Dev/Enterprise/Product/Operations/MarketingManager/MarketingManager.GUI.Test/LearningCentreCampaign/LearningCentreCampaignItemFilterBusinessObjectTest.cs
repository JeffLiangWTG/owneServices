using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(LearningCentreCampaignItemFilterBusinessObject))]
	public class LearningCentreCampaignItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestContactName()
		{
			var result = (ModuleTextFilter)CampaignFilter["Contact Name"];
			AssertNotNull(result);

			Contact.OC_ContactName = "edwin mills";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			Factory.Save();

			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(Campaign);

			result.Property = "xwinter";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			var query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			result.Property = Contact.OC_ContactName;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		public void TestEmailAddress()
		{
			var result = (ModuleTextFilter)CampaignFilter["Contact Email"];
			AssertNotNull(result);

			Contact.OC_Email = "edwin@yahoo.com";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			Factory.Save();

			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(Campaign);

			result.Property = "edwin@google.com";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			var query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			result.Property = Contact.OC_Email;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		#region Implementation

		GlbCompanyCampaign Campaign;
		LearningCentreCampaignItemFilterBusinessObject CampaignFilter;
		OrgContact Contact;

		void CreateContactsForTest()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Contact = Factory.NewWithValidTestData<OrgContact>();
			CampaignFilter = new LearningCentreCampaignItemFilterBusinessObject();

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateContactsForTest();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LearningCentreCampaignItemFilterBusinessObject();
		}

		#endregion
	}
}
