using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignClickFilterBusinessObject))]
	public class GlbCompanyCampaignClickFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTrackingImageClicksShouldBeIncluded()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.google.com";
			link.GCL_Context = "Google Searcher";

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.Value;
			link2.GCL_Context = CampaignEmailTemplateEditor.TrackingImageContext;

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			CampaignClick.GCC_G8_Recipient = campaignItem.PK;
			CampaignClick.GCC_GCL = link.PK;

			var campaignClick2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick2.GCC_G8_Recipient = campaignItem.PK;
			campaignClick2.GCC_GCL = link2.PK;

			Factory.Save();

			var result = (ModuleTextFilter)CampaignFilter["Tracking Context"];
			AssertNotNull(result);

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property = "";
			result.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			result.IsActive = true;

			var query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("Should include LinkTrackingImageUrl from registry", 2, collection.Count);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<GlbCompanyCampaignClick>.PKOnlyComparer, new[] { CampaignClick, campaignClick2 }, collection);

			result.Property = CampaignEmailTemplateEditor.TrackingImageContext;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals("Should not include LinkTrackingImageUrl from registry", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<GlbCompanyCampaignClick>.PKOnlyComparer, new[] { campaignClick2 }, collection);
		}

		public void TestContactName()
		{
			var result = (ModuleTextFilter)CampaignFilter["Contact Name"];
			AssertNotNull(result);

			Contact.OC_ContactName = "edwin mills";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;
			CampaignClick.GCC_G8_Recipient = campaignItem.PK;

			Factory.Save();

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property = "xwinter";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(0, collection.Count);

			result.Property = Contact.OC_ContactName;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		public void TestEmailAddress()
		{
			var result = (ModuleTextFilter)CampaignFilter["Email Address"];
			AssertNotNull(result);

			Contact.OC_Email = "edwin@yahoo.com";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;
			CampaignClick.GCC_G8_Recipient = campaignItem.PK;

			Factory.Save();

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property = "edwin@google.com";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(0, collection.Count);

			result.Property = Contact.OC_Email;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		public void TestOrgCode()
		{
			var result = (ModuleNkFilter)CampaignFilter["Organization"];
			AssertNotNull(result);

			organisation.OH_Code = "ACS";
			Contact.OC_OH = organisation.PK;

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;
			CampaignClick.GCC_G8_Recipient = campaignItem.PK;

			Factory.Save();

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property = "ENGRS";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(0, collection.Count);

			result.Property = "ACS";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		public void TestTrackingContext()
		{
			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.google.com";
			link.GCL_Context = "Google Searcher";

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			CampaignClick.GCC_G8_Recipient = campaignItem.PK;
			CampaignClick.GCC_GCL = link2.PK;

			Factory.Save();

			var result = (ModuleTextFilter)CampaignFilter["Tracking Context"];
			AssertNotNull(result);

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property = "Google Searcher";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(0, collection.Count);

			result.Property = "Yahoo Plus";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		public void TestDestinationURL()
		{
			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.google.com";
			link.GCL_Context = "Google Searcher";

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			CampaignClick.GCC_G8_Recipient = campaignItem.PK;
			CampaignClick.GCC_GCL = link2.PK;

			Factory.Save();

			var result = (ModuleTextFilter)CampaignFilter["Destination URL"];
			AssertNotNull(result);

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property = "http://www.google.com";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(0, collection.Count);

			result.Property = "http://www.yahoomail.com";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		public void TestImage()
		{
			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			CampaignClick.GCC_G8_Recipient = campaignItem.PK;
			CampaignClick.GCC_GCL = link.PK;

			var campaignClick2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick2.GCC_G8_Recipient = campaignItem.PK;
			campaignClick2.GCC_GCL = link2.PK;

			Factory.Save();

			var result = (ModuleFlagsFilter)CampaignFilter["Image"];
			AssertNotNull(result);

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property0 = false;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(2, collection.Count);

			result.Property0 = true;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		[TestDate(2015, 7, 5, 20, 10, 10)]
		[TestUtcOffset(11, 0, 0)]
		public void TestActivityDate()
		{
			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			CampaignClick.GCC_G8_Recipient = campaignItem.PK;
			CampaignClick.GCC_GCL = link.PK;
			CampaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			var result = (ModuleDateFilter)CampaignFilter["Activity Date"];
			AssertNotNull(result);

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaignItem);

			result.Property1 = new ZDateTime(2015, 7, 2, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(0, collection.Count);

			result.Property1 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 6, 12, 12, 1);
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.AdditionalFilter = query;
			AssertEquals(1, collection.Count);
		}

		#region Implementation

		OrgHeader organisation;
		GlbCompanyCampaign Campaign;
		GlbCompanyCampaignClickFilterBusinessObject CampaignFilter;
		OrgContact Contact;
		GlbCompanyCampaignClick CampaignClick;

		void CreateContactsForTest()
		{
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Contact = Factory.NewWithValidTestData<OrgContact>();
			CampaignFilter = new GlbCompanyCampaignClickFilterBusinessObject(Campaign);
			CampaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateContactsForTest();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbCompanyCampaignClickFilterBusinessObject();
		}

		#endregion
	}
}
