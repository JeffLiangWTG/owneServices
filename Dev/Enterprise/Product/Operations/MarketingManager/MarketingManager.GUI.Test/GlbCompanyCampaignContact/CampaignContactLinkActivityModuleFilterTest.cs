using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignContactLinkActivityModuleFilter))]
	public class CampaignContactLinkActivityModuleFilterTest : ModuleFilterTestCase<CampaignContactLinkActivityModuleFilter>
	{
		protected override void SetUp()
		{
			base.SetUp();
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(FilterBusinessObject);
		}

		[TestDate(2015, 7, 5, 20, 10, 10)]
		[TestUtcOffset(11, 0, 0)]
		public void TestTrackingActivityDate_WithContext()
		{
			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHCD";

			var link = previousCampaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = previousCampaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var contact = org.Contacts.AddNew();

			var campaignItem = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Campaign.SourceCampaignPK = previousCampaign.PK;

			Factory.Save();

			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, Campaign);

			var result = (CampaignContactContextLinkActivityModuleFilter)FilterBusinessObject["Has Context Activity"];
			AssertNotNull(result);

			result.Property1 = new ZDateTime(2015, 7, 2, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			result.TypeProperty = "Google Images";
			result.IsActive = true;

			string expectedSql = @"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = '{0}' 
	AND
	(
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
)
";
			ZQuery query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.SourceCampaignPK.ToString()), result.Query.LiteralTextSqlFormatted);
			}
			collection.Load(query);
			AssertEquals(0, collection.Count);

			result.Property1 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 6, 12, 12, 1);
			result.IsActive = true;

			expectedSql = @"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = '{0}' 
	AND
	(
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
)
";
			collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, Campaign);

			query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.SourceCampaignPK.ToString()), result.Query.LiteralTextSqlFormatted);
			}
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		[TestDate(2015, 7, 5, 20, 10, 10)]
		[TestUtcOffset(11, 0, 0)]
		public void TestTrackingActivityDate_WithDestinationURL()
		{
			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHCD";

			var link = previousCampaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = previousCampaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var contact = org.Contacts.AddNew();

			var campaignItem = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link2.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Campaign.SourceCampaignPK = previousCampaign.PK;

			Factory.Save();

			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, Campaign);

			var result = (CampaignContactDestinationURLLinkActivityModuleFilter)FilterBusinessObject["Has Destination URL Activity"];
			AssertNotNull(result);

			result.Property1 = new ZDateTime(2015, 7, 2, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			result.TypeProperty = "http://www.yahoomail.com";
			result.IsActive = true;

			string expectedSql = @"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = '{0}' 
	AND
	(
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
)
";
			ZQuery query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.SourceCampaignPK.ToString()), result.Query.LiteralTextSqlFormatted);
			}
			collection.Load(query);
			AssertEquals(0, collection.Count);

			result.Property1 = new ZDateTime(2015, 7, 4, 12, 12, 1);
			result.Property2 = new ZDateTime(2015, 7, 6, 12, 12, 1);
			result.IsActive = true;

			expectedSql = @"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = '{0}' 
	AND
	(
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
)
";
			collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, Campaign);

			query = FilterBusinessObject.Filter;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Query string should be the same", string.Format(expectedSql, Campaign.SourceCampaignPK.ToString()), result.Query.LiteralTextSqlFormatted);
			}
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		#region LinkActivityModuleFilter

		GlbCompanyCampaignContactFilterBusinessObject FilterBusinessObject;
		GlbCompanyCampaign Campaign;

		#endregion

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		protected override CampaignContactLinkActivityModuleFilter GetNewModuleFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filter = new CampaignContactLinkActivityModuleFilter("moo", new GlbCompanyCampaignContactFilterBusinessObject(campaign), campaign);
			return filter;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
