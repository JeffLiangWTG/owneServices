using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(LinkActivityModuleFilter))]
	public class LinkActivityModuleFilterTest : ModuleFilterTestCase<LinkActivityModuleFilter>
	{
		protected override void SetUp()
		{
			base.SetUp();
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(Campaign);
		}

		[TestDate(2015, 7, 5, 20, 10, 10)]
		[TestUtcOffset(11, 0, 0)]
		public void TestTrackingActivityDate_WithContext()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHCD";

			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var contact = org.Contacts.AddNew();

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			var result = (ContextLinkActivityModuleFilter)FilterBusinessObject["Has Context Activity"];
			AssertNotNull(result);

			result.Property1 = new ZDateTime(2015, 7, 2, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			result.TypeProperty = "Google Images";
			result.IsActive = true;

			string expectedSql = @"(
	G8_PK IN 
	(
		SELECT GCC_G8_Recipient FROM dbo.GlbCompanyCampaignClick WHERE GCC_G8_Recipient IS NOT NULL 
		AND
		GCC_ClickTimeUtc >= '2015-07-01 13:00:00.000' 
		AND
		GCC_ClickTimeUtc < '2015-07-04 12:59:00.000' 
		AND
		GCC_GCL IN 
		(
			SELECT GCL_PK FROM dbo.GlbCompanyCampaignLink WHERE GCL_Context = N'Google Images'
		)
	)
)
AND
G8_G0 = '{0}'
";
			ZQuery query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.PK.ToString()), query.LiteralTextSqlFormatted);
			}
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(0, Campaign.CampaignsItemsSent.Count);

			result.Property1 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 6, 12, 12, 1);
			result.IsActive = true;

			expectedSql = @"(
	G8_PK IN 
	(
		SELECT GCC_G8_Recipient FROM dbo.GlbCompanyCampaignClick WHERE GCC_G8_Recipient IS NOT NULL 
		AND
		GCC_ClickTimeUtc >= '2015-07-03 13:00:00.000' 
		AND
		GCC_ClickTimeUtc < '2015-07-06 12:59:00.000' 
		AND
		GCC_GCL IN 
		(
			SELECT GCL_PK FROM dbo.GlbCompanyCampaignLink WHERE GCL_Context = N'Google Images'
		)
	)
)
AND
G8_G0 = '{0}'
";

			query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.PK.ToString()), query.LiteralTextSqlFormatted);
			}
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
		}

		[TestDate(2015, 7, 5, 20, 10, 10)]
		[TestUtcOffset(11, 0, 0)]
		public void TestTrackingActivityDate_WithDestinationURL()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHCD";

			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var contact = org.Contacts.AddNew();

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link2.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			var result = (DestinationURLLinkActivityModuleFilter)FilterBusinessObject["Has Destination URL Activity"];
			AssertNotNull(result);

			result.Property1 = new ZDateTime(2015, 7, 2, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			result.TypeProperty = "http://www.yahoomail.com";
			result.IsActive = true;

			string expectedSql = @"(
	G8_PK IN 
	(
		SELECT GCC_G8_Recipient FROM dbo.GlbCompanyCampaignClick WHERE GCC_G8_Recipient IS NOT NULL 
		AND
		GCC_ClickTimeUtc >= '2015-07-01 13:00:00.000' 
		AND
		GCC_ClickTimeUtc < '2015-07-04 12:59:00.000' 
		AND
		GCC_GCL IN 
		(
			SELECT GCL_PK FROM dbo.GlbCompanyCampaignLink WHERE GCL_URL = N'http://www.yahoomail.com'
		)
	)
)
AND
G8_G0 = '{0}'
";
			ZQuery query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.PK.ToString()), query.LiteralTextSqlFormatted);
			}
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(0, Campaign.CampaignsItemsSent.Count);

			result.Property1 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 6, 12, 12, 1);
			result.IsActive = true;

			expectedSql = @"(
	G8_PK IN 
	(
		SELECT GCC_G8_Recipient FROM dbo.GlbCompanyCampaignClick WHERE GCC_G8_Recipient IS NOT NULL 
		AND
		GCC_ClickTimeUtc >= '2015-07-03 13:00:00.000' 
		AND
		GCC_ClickTimeUtc < '2015-07-06 12:59:00.000' 
		AND
		GCC_GCL IN 
		(
			SELECT GCL_PK FROM dbo.GlbCompanyCampaignLink WHERE GCL_URL = N'http://www.yahoomail.com'
		)
	)
)
AND
G8_G0 = '{0}'
";

			query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.PK.ToString()), query.LiteralTextSqlFormatted);
			}
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestDeserializePropertiesFromToXml_Context()
		{
			ContextLinkActivityModuleFilter filter = new ContextLinkActivityModuleFilter("Has Context Activity", FilterBusinessObject, Campaign);

			var filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filter);
			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.TypeProperty = "Yahoo Plus";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			ContextLinkActivityModuleFilter loadedFilter = (ContextLinkActivityModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Yahoo Plus", loadedFilter.TypeProperty);
		}

		public void TestDeserializePropertiesFromToXml_DestinationURL()
		{
			DestinationURLLinkActivityModuleFilter filter = new DestinationURLLinkActivityModuleFilter("Has Destination URL Activity", FilterBusinessObject, Campaign);

			var filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filter);
			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.TypeProperty = "http://abc.org";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			DestinationURLLinkActivityModuleFilter loadedFilter = (DestinationURLLinkActivityModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("http://abc.org", loadedFilter.TypeProperty);
		}

		#region Clear / IsEmpty

		public void TestClear()
		{
			Filter.PropertySearch = ModuleDateFilter.Future;
			Filter.Property1 = new ZDateTime(2013, 1, 25);
			Filter.Property2 = new ZDateTime(2014, 4, 13);
			Filter.TypeProperty = "ABC Search";

			Filter.Clear();

			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", ModuleDateFilter.SpecifiedDateRange, Filter.PropertySearch);
				AssertEquals("Property1", ZDateTime.Empty, Filter.Property1);
				AssertEquals("Property2", ZDateTime.Empty, Filter.Property2);
				AssertEquals("TypeProperty", ZString.Empty, Filter.TypeProperty);
			});
		}

		public void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			Filter.TypeProperty = ZString.Empty;

			AssertEquals(true, Filter.IsEmpty);

			Filter.Property1 = new ZDateTime(2013, 1, 25);
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2014, 4, 13);
			AssertEquals(false, Filter.IsEmpty);

			Filter.TypeProperty = "XXX";
			AssertEquals(true, Filter.IsEmpty);
		}

		#endregion

		#region LinkActivityModuleFilter

		GlbCompanyCampaignItemFilterBusinessObject FilterBusinessObject;
		GlbCompanyCampaign Campaign;

		#endregion

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override LinkActivityModuleFilter GetNewModuleFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filter = new LinkActivityModuleFilter("moo", new GlbCompanyCampaignItemFilterBusinessObject(campaign), campaign);
			return filter;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
