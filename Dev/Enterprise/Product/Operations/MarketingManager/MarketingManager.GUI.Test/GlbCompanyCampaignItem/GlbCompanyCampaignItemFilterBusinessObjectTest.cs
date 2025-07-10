using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemFilterBusinessObject))]
	public class GlbCompanyCampaignItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestFilterExistence_MasterList()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(campaign);

			AssertFilterExistence(filterBusinessObject, false);
		}

		public void TestFilterExistence_Touch()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = campaign.AllTouches.AddNew();
			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(touch);

			AssertFilterExistence(filterBusinessObject, true);
		}

		public void TestFilterExistence_Regular()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(campaign);

			AssertFilterExistence(filterBusinessObject, true);
		}

		void AssertFilterExistence(GlbCompanyCampaignItemFilterBusinessObject bizO, bool exists)
		{
			string[] filterNames = new[]
			{
				"Tracking Status",
				"Last Sent Time",
				"Scheduled Time",
				"Has Context Activity",
				"Has Destination URL Activity",
				"Unsubscribed state",
				"Unique Day(s) Activity Count"
			};

			foreach (var filterName in filterNames)
			{
				if (exists)
				{
					AssertNotNull(filterName, bizO[filterName]);
				}
				else
				{
					AssertNull(filterName, bizO[filterName]);
				}
			}
		}

		public void TestTouchPointsFilter_NormalCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(campaign);
			var filter = (TouchReceivedModuleFilter)filterBusinessObject["Touch Points"];

			AssertNull(filter);
		}

		public void TestTouchPointsFilter_TouchCampaign()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(helper.Touch1A);
			var filter = (TouchReceivedModuleFilter)filterBusinessObject["Touch Points"];

			AssertNull(filter);
		}

		public void TestTouchPointsFilter_Vertical()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(helper.Master);
			var filter = (TouchReceivedModuleFilter)filterBusinessObject["Touch Points"];

			var masterInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbCompanyCampaign>(helper.Master.PK);

			var collection = new GlbCompanyCampaignItemCampaignDependentCollection(masterInOtherFactory);

			filter.HorizontalId = "1";
			filter.VerticalId = "A";
			collection.Load(filter.Query);
			AssertEquals(2, collection.Count);

			filter.VerticalId = "B";
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.HorizontalId = "2";
			filter.VerticalId = "B";
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.HorizontalId = "3";
			filter.VerticalId = "A";
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);
		}

		public void TestTouchPointsFilter_Horizontal()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(helper.Master);
			var filter = (TouchReceivedModuleFilter)filterBusinessObject["Touch Points"];

			filter.HorizontalId = "1";
			helper.Master.CampaignsItemsSent.Load(filter.Query);
			AssertEquals(3, helper.Master.CampaignsItemsSent.Count);

			filter.HorizontalId = "2";
			helper.Master.CampaignsItemsSent.Load(filter.Query);
			AssertEquals(2, helper.Master.CampaignsItemsSent.Count);

			filter.HorizontalId = "3";
			helper.Master.CampaignsItemsSent.Load(filter.Query);
			AssertEquals(0, helper.Master.CampaignsItemsSent.Count);
		}

		public void TestTransitionStatusFilter()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			var contact = helper.Org.Contacts.AddNew();
			contact.OC_Email = "percy@test.com";
			contact.OC_ContactName = "Percy";

			var itemNDR = helper.Touch1A.CampaignsItemsSent.AddNew();
			itemNDR.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemNDR.G8_RecipientID = contact.PK;
			itemNDR.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			Factory.Save();

			AssertTransitionFilter(helper.Master, 3, 0);
			AssertTransitionFilter(helper.Touch1A, 1, 2);
			AssertTransitionFilter(helper.Touch1B, 1, 0);
			AssertTransitionFilter(helper.Touch2A, 0, 1);
			AssertTransitionFilter(helper.Touch2B, 0, 1);
			AssertTransitionFilter(helper.Touch3A, 0, 0);
		}

		public void TestTransitionStatus_SingleTouch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "thomas@test.com";
			contact1.OC_ContactName = "Thomas";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "gordon@test.com";
			contact2.OC_ContactName = "Gordon";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "viktor@test.com";
			contact3.OC_ContactName = "Viktor";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var itemMaster1 = master.CampaignsItemsSent.AddNew();
			itemMaster1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster1.G8_RecipientID = contact1.PK;
			itemMaster1.G8_TrackingStatus = "UNV";
			var itemMaster2 = master.CampaignsItemsSent.AddNew();
			itemMaster2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster2.G8_RecipientID = contact2.PK;
			itemMaster2.G8_TrackingStatus = "UNV";
			var itemMaster3 = master.CampaignsItemsSent.AddNew();
			itemMaster3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster3.G8_RecipientID = contact3.PK;
			itemMaster3.G8_TrackingStatus = "UNV";

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = master.PK;
			var item1_1a = touch1A.CampaignsItemsSent.AddNew();
			item1_1a.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1_1a.G8_RecipientID = contact1.PK;
			item1_1a.G8_TrackingStatus = "QUE";
			var item2_1a = touch1A.CampaignsItemsSent.AddNew();
			item2_1a.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2_1a.G8_RecipientID = contact2.PK;
			item2_1a.G8_TrackingStatus = "VER";
			var item3_1a = touch1A.CampaignsItemsSent.AddNew();
			item3_1a.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item3_1a.G8_RecipientID = contact3.PK;
			item3_1a.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			master.AllTouches.Add(touch1A);

			Factory.Save();

			AssertTransitionFilter(master, 3, 0);
			AssertTransitionFilter(touch1A, 0, 3);
		}

		static void AssertTransitionFilter(GlbCompanyCampaign campaign, int transitioned, int nonTransitioned)
		{
			var filter = new GlbCompanyCampaignItemFilterBusinessObject(campaign);
			var result = (ModuleFlagsFilter)filter["Transition Status"];
			AssertNotNull(result);

			result.Property0 = true;
			campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(transitioned, campaign.CampaignsItemsSent.Count);

			result.Property0 = false;
			campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(nonTransitioned, campaign.CampaignsItemsSent.Count);
		}

		public void TestHiddenCampaignFilter()
		{
			var result = (ModuleGuidFilter)CampaignFilter["Campaign"];
			AssertNotNull(result);
			Assert(result.Visibility == FilterVisibility.AlwaysAppliedAndHidden);
		}

		public void TestHiddenCampaignItemFilter()
		{
			var result = (ModuleGuidFilter)CampaignFilter["CampaignItem"];
			AssertNotNull(result);

			Assert(result.Visibility == FilterVisibility.AlwaysAppliedAndHidden);
			AssertNull("Precondition", CampaignFilter.CampaignItem);
			AssertMultilineASCIIEquals("Strings should be the same", string.Empty, result.Query.LiteralTextADOFormatted);

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Blob";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "Plonk";

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals("There should be 2 items in the list", 2, Campaign.CampaignsItemsSent.Count);

			CampaignFilter.CampaignItem = campaignItem1;
			string expectedSQL = string.Format(@"G8_PK = CONVERT
(
	'{0}', 'System.Guid'
)
", campaignItem1.PK.ToString());
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestOrganisationFilter()
		{
			var result = (ModuleNkFilter)CampaignFilter["Organization"];
			AssertNotNull(result);

			organisation.OH_Code = "NOISSUES";
			Contact.OC_OH = organisation.PK;

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = Contact.PK;

			Factory.Save();

			result.Property = "NOISSUES";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			string expectedSQL = @"VCC_OrgCode = 'NOISSUES'";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			ZQuery query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestCountryOrPort()
		{
			var result = (ModuleNkFilter)CampaignFilter["Country / Port"];
			AssertNotNull(result);

			var inquiryContact = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiryContact.O1_OH_ConvertedToQualifiedLead = organisation.PK;
			organisation.OH_RL_NKClosestPort = "CDKAS";
			Contact.OC_OH = organisation.PK;

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = inquiryContact.PK;
			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = Contact.PK;

			Factory.Save();

			result.Property = "CD";
			result.IsActive = true;
			string expectedSQL = @"G8_RecipientID IN 
(
	SELECT VCC_PK FROM dbo.ViewCampaignContact WHERE VCC_OH IN 
	(
		SELECT OH_PK FROM dbo.OrgHeader WHERE OH_RL_NKClosestPort IN 
		(
			SELECT RL_Code FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode = 'CD'
		)
	)
	OR
	VCC_RelatedPortCode like 'CD%'
)";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals("There should be 2 items in the list", 2, Campaign.CampaignsItemsSent.Count);
		}

		public void TestCountry()
		{
			Campaign.G0_IsSalesAndMarketing = false;
			AssertEquals(true, CampaignFilter.Campaign.IsHRCampaign);
			var result = (ModuleNkFilter)CampaignFilter["Country"];
			AssertNotNull(result);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_RN_NKCountryCode = "CD";

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = staff.PK;

			Factory.Save();

			result.Property = "CD";
			result.IsActive = true;

			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestTrackingStatusFilter()
		{
			var result = (ModuleTextFilter)CampaignFilter["Tracking Status"];
			AssertNotNull(result);

			Contact.OC_OH = organisation.PK;
			Contact.OC_Email = "Contact@org.net";

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = Contact.PK;
			campaignItem1.G8_TrackingStatus = "NDR";

			Factory.Save();

			result.Property = "NDR";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			string expectedSQL = @"G8_TrackingStatus = 'NDR'
";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestContactEmailAddress()
		{
			var result = (ModuleTextFilter)CampaignFilter["Email Address"];
			AssertNotNull(result);

			Contact.OC_OH = organisation.PK;
			Contact.OC_Email = "davinci@yahoo.com";

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = Contact.PK;

			Factory.Save();

			result.Property = "davinci@yahoo.com";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			string expectedSQL = @"VCC_Email = 'davinci@yahoo.com'
";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestContactName()
		{
			var result = (ModuleTextFilter)CampaignFilter["Contact Name"];
			AssertNotNull(result);

			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Edward Jones";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();

			result.Property = contact.OC_Email;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			ZQuery query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);

			result.Property = "Smith Rogers";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			ZQuery query1 = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query1);
			AssertEquals("No item matches collection items", 0, Campaign.CampaignsItemsSent.Count);
		}

		public void TestOrganisationNameFilter()
		{
			var result = (ModuleTextFilter)CampaignFilter["Organization Name"];
			AssertNotNull(result);

			organisation.OH_Code = "NTRIS";
			organisation.OH_FullName = "Novatris Animal Pty Ltd";
			Contact.OC_OH = organisation.PK;

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_CompanyName = "Novat Ltd";

			var inquiry1 = Factory.New<SalesEnquiry>();
			inquiry1.O1_CompanyName = "Globus Ltd";

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = Contact.PK;
			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = inquiry.PK;
			var campaignItem3 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = inquiry1.PK;

			Factory.Save();

			result.Property = "Nov";
			result.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals("There should be 2 items in the list", 2, Campaign.CampaignUnitsSent);

			result.Property = "Globu";
			result.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			result.IsActive = true;

			ZQuery query1 = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query1);
			AssertEquals("There should be 1 items in the list", 1, Campaign.CampaignsItemsSent.Count);
		}

		[TestDate(2015, 5, 21, 12, 12, 12)]
		[TestUtcOffset(10, 0, 0)]
		public void TestDistinctDaysActivityCount()
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

			organisation.OH_RL_NKClosestPort = "AUBNE";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "AA Pheobe";

			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var clickTime = ZDateTime.UtcNow;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link2.PK;
			campaignClick.GCC_ClickTimeUtc = clickTime;

			var campaignClick2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick2.GCC_G8_Recipient = campaignItem.PK;
			campaignClick2.GCC_GCL = link2.PK;
			campaignClick2.GCC_ClickTimeUtc = clickTime.AddHours(4);

			Factory.Save();

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNotNull(result);

			result.Property = 0;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);

			result.Property = 2;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(0, Campaign.CampaignsItemsSent.Count);

			campaignClick2.GCC_ClickTimeUtc = clickTime.AddHours(1);
			Factory.Save();

			query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(0, Campaign.CampaignsItemsSent.Count);

			result.Property = 1;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
		}

		[TestDate(2015, 5, 21, 12, 12, 12)]
		public void TestDistinctDaysActivityCount_WithInquiry()
		{
			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.google.com";
			link.GCL_Context = "Google Searcher";

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_ContactName = "Edward Johns";
			inquiry.O1_PortOrCountry = "";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = inquiry.PK;
			campaignItem.G8_RecipientTableCode = "O1";

			var clickTime = ZDateTime.UtcNow;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link2.PK;
			campaignClick.GCC_ClickTimeUtc = clickTime;

			var campaignClick2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick2.GCC_G8_Recipient = campaignItem.PK;
			campaignClick2.GCC_GCL = link2.PK;
			campaignClick2.GCC_ClickTimeUtc = clickTime.AddHours(4);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "EDW";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";  // -7 hours
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNotNull(result);

			result.Property = 0;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			Campaign.CampaignsItemsSent.Load(query);
			AssertEquals(0, Campaign.CampaignsItemsSent.Count);

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				// GetDateInLocalTime() SQL Function should be using branch UNLOCO
				result.Property = 2;
				result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
				result.IsActive = true;

				query = CampaignFilter.Filter;
				Campaign.CampaignsItemsSent.Load(query);
				AssertEquals(0, Campaign.CampaignsItemsSent.Count);

				result.Property = 1;
				result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
				result.IsActive = true;
				query = CampaignFilter.Filter;
				Campaign.CampaignsItemsSent.Load(query);
				AssertEquals(1, Campaign.CampaignsItemsSent.Count);

				inquiry.O1_PortOrCountry = "AUBNE";     //	10.00 hours
				Factory.Save();

				// GetDateInLocalTime SQL Function should be using inquiry UNLOCO
				result.Property = 2;
				result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
				result.IsActive = true;
				query = CampaignFilter.Filter;
				Campaign.CampaignsItemsSent.Load(query);
				AssertEquals(1, Campaign.CampaignsItemsSent.Count);
			}
		}

		public void TestUniqueDayActivityCountFilter_CampaignClickWithoutRecipient()
		{
			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.yahoomail.com";
			link.GCL_Context = "Yahoo Plus";

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_GCL = link.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNotNull(result);

			result.Property = 0;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
		}

		public void TestFiltersRemoved_WithTargetList()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			CampaignFilter = new GlbCompanyCampaignItemFilterBusinessObject(campaign);

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNull(result);

			var result2 = (ContextLinkActivityModuleFilter)CampaignFilter["Has Context Activity"];
			AssertNull(result2);

			var result3 = (DestinationURLLinkActivityModuleFilter)CampaignFilter["Has Destination URL Activity"];
			AssertNull(result2);

			var result4 = (ModuleTextFilter)CampaignFilter["Tracking Status"];
			AssertNull(result4);

			var result5 = (ModuleNkFilter)CampaignFilter["Sent By Person"];
			AssertNull(result5);
		}

		[TestDate(2015, 9, 2, 13, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestLastSentTime()
		{
			var result = (ModuleDateFilter)CampaignFilter["Last Sent Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Contact = org2.Contacts.AddNew();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org3Contact = org3.Contacts.AddNew();

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_LastSentTimeUtc = ZDateTime.UtcNow;

			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = org2Contact.PK;
			campaignItem2.G8_LastSentTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var campaignItem3 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = org3Contact.PK;
			campaignItem3.G8_LastSentTimeUtc = ZDateTime.UtcNow.AddDays(2);

			Factory.Save();

			result.IsActive = true;
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			result.Property1 = ZDateTime.Now;

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(3, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem1, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem2, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem3, Campaign.CampaignsItemsSent);

			result.Property1 = ZDateTime.Now;
			result.Property2 = ZDateTime.Now;
			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem1, Campaign.CampaignsItemsSent);

			result.Property1 = ZDateTime.Now.AddDays(1);
			result.Property2 = ZDateTime.Now.AddDays(2);
			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(2, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem2, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem3, Campaign.CampaignsItemsSent);
		}

		[TestDate(2015, 9, 2, 13, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestLastSentTime_HasDate()
		{
			var result = (ModuleDateFilter)CampaignFilter["Last Sent Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Contact = org2.Contacts.AddNew();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org3Contact = org3.Contacts.AddNew();

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_LastSentTimeUtc = ZDateTime.UtcNow;

			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = org2Contact.PK;

			var campaignItem3 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = org3Contact.PK;

			Factory.Save();

			result.IsActive = true;
			result.PropertySearch = ModuleDateFilter.HasDateEntered;

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem1, Campaign.CampaignsItemsSent);

			result.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(2, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem2, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem3, Campaign.CampaignsItemsSent);
		}

		[TestDate(2015, 9, 2, 0, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestScheduledTime()
		{
			var result = (ModuleDateFilter)CampaignFilter["Scheduled Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Contact = org2.Contacts.AddNew();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org3Contact = org3.Contacts.AddNew();

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = org2Contact.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var campaignItem3 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = org3Contact.PK;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);

			Factory.Save();

			result.IsActive = true;
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			result.Property1 = ZDateTime.Now;

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(3, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem1, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem2, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem3, Campaign.CampaignsItemsSent);

			result.Property1 = ZDateTime.Now;
			result.Property2 = ZDateTime.Now;
			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem1, Campaign.CampaignsItemsSent);

			result.Property1 = ZDateTime.Now.AddDays(1);
			result.Property2 = ZDateTime.Now.AddDays(2);
			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(2, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem2, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem3, Campaign.CampaignsItemsSent);
		}

		[TestDate(2015, 9, 2, 0, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestScheduledTime_HasDate()
		{
			var result = (ModuleDateFilter)CampaignFilter["Scheduled Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Contact = org2.Contacts.AddNew();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org3Contact = org3.Contacts.AddNew();

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = org2Contact.PK;

			var campaignItem3 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = org3Contact.PK;

			Factory.Save();

			result.IsActive = true;
			result.PropertySearch = ModuleDateFilter.HasDateEntered;

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(1, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem1, Campaign.CampaignsItemsSent);

			result.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			Campaign.CampaignsItemsSent.Load(result.Query);
			AssertEquals(2, Campaign.CampaignsItemsSent.Count);
			AssertCollectionContains(campaignItem2, Campaign.CampaignsItemsSent);
			AssertCollectionContains(campaignItem3, Campaign.CampaignsItemsSent);
		}

		public void TestIsUnsubscribedFilter()
		{
			var filter = (ModuleFlagsFilter)CampaignFilter["Unsubscribed state"];
			AssertNotNull(filter);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact1);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "contact2@email.em";
			org.Contacts.Add(contact2);

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_Email = contact1.OC_Email;
			subscription.GCS_IsSubscribed = false;
			subscription.GCS_G0 = Campaign.PK;

			Campaign.G0_Category = "AAA";
			Campaign.G0_Type = "BBB";

			Factory.Save();

			filter.IsActive = true;
			filter.Property0 = true;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
			AssertEquals(campaignItem1.PK, Campaign.CampaignsItemsSent[0].PK);

			filter.IsActive = true;
			filter.Property0 = false;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
			AssertEquals(campaignItem2.PK, Campaign.CampaignsItemsSent[0].PK);

			filter.IsActive = false;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 2 items in the list", 2, Campaign.CampaignsItemsSent.Count);

			var subscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription2.GCS_Email = contact1.OC_Email;
			subscription2.GCS_IsSubscribed = true;
			subscription2.GCS_MediaCategory = Campaign.G0_Category;
			subscription2.GCS_MediaType = Campaign.G0_Type;
			subscription2.GCS_G0 = Campaign.PK;

			Factory.Save();

			filter.IsActive = true;
			filter.Property0 = true;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 0 item in the list", 0, Campaign.CampaignsItemsSent.Count);

			filter.IsActive = true;
			filter.Property0 = false;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 2 item in the list", 2, Campaign.CampaignsItemsSent.Count);
		}

		public void TestIsUnsubscribedFilterForHRCampaign()
		{
			Campaign.G0_IsSalesAndMarketing = false;
			AssertEquals("Precondition", true, CampaignFilter.Campaign.IsHRCampaign);
			var filter = (ModuleFlagsFilter)CampaignFilter["Unsubscribed state"];
			AssertNotNull(filter);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "contact1@email.em";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_EmailAddress = "contact2@email.em";

			var campaignItem1 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = staff1.TablePrefix;
			campaignItem1.G8_RecipientID = staff1.PK;
			var campaignItem2 = Campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = staff2.TablePrefix;
			campaignItem2.G8_RecipientID = staff2.PK;

			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_Email = staff1.GS_EmailAddress;
			subscription.GCS_IsSubscribed = false;
			subscription.GCS_G0 = Campaign.PK;

			Campaign.G0_Category = "AAA";
			Campaign.G0_Type = "BBB";

			Factory.Save();

			filter.IsActive = true;
			filter.Property0 = true;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
			AssertEquals(campaignItem1.PK, Campaign.CampaignsItemsSent[0].PK);

			filter.IsActive = true;
			filter.Property0 = false;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 1 item in the list", 1, Campaign.CampaignsItemsSent.Count);
			AssertEquals(campaignItem2.PK, Campaign.CampaignsItemsSent[0].PK);

			filter.IsActive = false;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 2 items in the list", 2, Campaign.CampaignsItemsSent.Count);

			var subscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription2.GCS_Email = staff1.GS_EmailAddress;
			subscription2.GCS_IsSubscribed = true;
			subscription2.GCS_MediaCategory = Campaign.G0_Category;
			subscription2.GCS_MediaType = Campaign.G0_Type;
			subscription2.GCS_G0 = Campaign.PK;

			Factory.Save();

			filter.IsActive = true;
			filter.Property0 = true;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 0 item in the list", 0, Campaign.CampaignsItemsSent.Count);

			filter.IsActive = true;
			filter.Property0 = false;
			Campaign.CampaignsItemsSent.Load(CampaignFilter.Filter);
			AssertEquals("There should be 2 item in the list", 2, Campaign.CampaignsItemsSent.Count);
		}

		public void TestHRCampaignShouldHideFilters()
		{
			Campaign.G0_IsSalesAndMarketing = false;
			AssertEquals("Precondition", true, CampaignFilter.Campaign.IsHRCampaign);
			var filter = CampaignFilter["Organization"];
			AssertNull(filter);
			filter = CampaignFilter["Country / Port"];
			AssertNull(filter);
			filter = CampaignFilter["Organization Name"];
			AssertNull(filter);
		}

		#endregion

		#region Implementation

		OrgHeader organisation;
		GlbCompanyCampaign Campaign;
		GlbCompanyCampaignItemFilterBusinessObject CampaignFilter;
		OrgContact Contact;

		protected override void SetUp()
		{
			base.SetUp();
			SetupBusinessObjects();
		}

		void SetupBusinessObjects()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			Contact = Factory.NewWithValidTestData<OrgContact>();
			CampaignFilter = new GlbCompanyCampaignItemFilterBusinessObject(Campaign);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbCompanyCampaignItemFilterBusinessObject(Factory.NewWithValidTestData<GlbCompanyCampaign>());
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(ViewCampaignContactSchema.Constants.TableName, "Country / Port"));
			result.Add(TableFilter(RefUNLOCOSchema.Constants.TableName, "Country / Port"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Country / Port"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Has Context Activity"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Has Context Activity"));
			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Has Context Activity"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Has Destination URL Activity"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Has Destination URL Activity"));
			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Has Destination URL Activity"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Unsubscribed state"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Unsubscribed state"));
			result.Add(TableFilter(GlbCompanyCampaignSubscriptionSchema.Constants.TableName, "Unsubscribed state"));
			result.Add(TableFilter(OrgContactSchema.Constants.TableName, "Unsubscribed state"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Unique Day(s) Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Unique Day(s) Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Unique Day(s) Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Unique Day(s) Activity Count"));

			return result;
		}

		#endregion
	}
}
