using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CampaignContactFilterCategories = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignContactFilterBusinessObject))]
	public class GlbCompanyCampaignContactFilterBusinessObjectTest : GlbCompanyCampaignContactFilterBusinessObjectTestBase
	{
		#region Filters

		public void TestSameContactDifferentOrgs_OrgLevelSubscription()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var subs = org2.Subscriptions.AddNew();
			subs.GCS_IsSubscribed = true;

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "master yoda 1";
			contact1.OC_Email = "yoda@jedi.com";

			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "master yoda 2";
			contact2.OC_Email = "yoda@jedi.com";

			Factory.Save();

			var filter = (ModuleTextFilter)CampaignFilter["SubscriptionStatus"];
			filter.Property = "SUB";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(filter.Query);

			AssertEquals(1, collection.Count);
			AssertEquals("master yoda 2", collection[0].Name);
		}

		public void TestEmailAddressFilter()
		{
			var result = (ModuleTextFilter)CampaignFilter["Email Address"];
			AssertNotNull(result);

			Contact.OC_Email = "edward.onwodi@wisetechglobal.com";
			Factory.Save();

			result.Property = Contact.OC_Email;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			AssertContains("VCC_Email = 'edward.onwodi@wisetechglobal.com'", query.LiteralTextADOFormatted);
			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			collection.Load(query);

			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestStmModuleFilterDeletedBusinessObject()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch1A.G0_G0_Master = master.PK;

			master.AllTouches.Add(touch1A);
			Factory.Save();

			using (var form = new ZForm(touch1A))
			using (var control = new TouchSetupControl())
			{
				control.SetDataBinding(touch1A, "");
				form.Controls.Add(control);
				form.Show();

				control.TransitionRulesGrid.Select(0);
				control.TransitionRulesGrid.ContextMenu.MenuItems.FindByText("&Delete").PerformClick();

				AssertNoExceptionThrown(() => control.SetSources());
			}
		}

		public void TestActiveQueryFilter()
		{
			var result = (ModuleTextFilter)CampaignFilter["ActiveQuery"];
			AssertNotNull(result);
			Assert(result.Visibility == FilterVisibility.AlwaysAppliedAndHidden);

			var expectedQuery = @"VCC_OrgIsActive = 1 
OR
VCC_OH is null";

			AssertMultilineASCIIEquals("SQL query should match", expectedQuery, result.Query.LiteralTextADOFormatted);
		}

		public void TestActiveQueryFilterWithIgnoreDuplicateEmails()
		{
			Campaign.G0_DeDuplicateContacts = ZBool.True;

			var result = (ModuleTextFilter)CampaignFilter["ActiveQuery"];
			AssertNotNull(result);
			Assert(result.Visibility == FilterVisibility.AlwaysAppliedAndHidden);

			var expectedQuery = @"(
	VCC_PK IN
	(
				select PkForMinRank
				FROM
		(
						SELECT VCC_Email, PkForMinRank = min
			(
				VCC_PK
			)
						FROM
						dbo.ViewCampaignContact contacts
						LEFT JOIN
			(
								SELECT DISTINCT SentEmail = VCC_Email FROM dbo.ViewCampaignContact WHERE VCC_PK IN
				(
										SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_G0 = CONVERT
					(
						'{0}', 'System.Guid'
					)
				)
								AND
								VCC_Email != ''
			)
			 sentItems ON contacts.VCC_Email = sentItems.SentEmail
						WHERE
			(
								sentItems.SentEmail is null
			)
						AND
			(
								VCC_Email != '' 
				OR
				VCC_PK NOT IN
				(
										SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_G0 = CONVERT
					(
						'{0}', 'System.Guid'
					)
					AND
					G8_RecipientID is not null
				)
			)
						AND
			{1} 
			AND
			(
				VCC_OrgIsActive = 1 
				OR
				VCC_OH is NULL
			)
			GROUP BY VCC_Email, case VCC_Email when '' then VCC_PK else null end
		)
		 a
	)
)
AND
{1}
";

			AssertMultilineASCIIEquals("SQL query should match for organization", string.Format(expectedQuery, Campaign.PK.ToString(), "VCC_TableCode = 'OC'"), CampaignFilter.Filter.LiteralTextADOFormatted);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;

			AssertMultilineASCIIEquals("SQL query should match for Inquiries", string.Format(expectedQuery, Campaign.PK.ToString(), "VCC_TableCode = 'O1'"), CampaignFilter.Filter.LiteralTextADOFormatted);
		}

		public void TestActiveQueryFilterWithIgnoreDuplicateEmails_CampaignTracking()
		{
			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Campaign.G0_DeDuplicateContacts = ZBool.True;

			var result = (ModuleTextFilter)CampaignFilter["ActiveQuery"];
			AssertNotNull(result);
			Assert(result.Visibility == FilterVisibility.AlwaysAppliedAndHidden);

			string expectedQuery = @"
VCC_PK IN
(
			select PkForMinRank
			FROM
	(
					SELECT VCC_Email, PkForMinRank = min
		(
			VCC_PK
		)
					FROM
					dbo.ViewCampaignContact contacts
					LEFT JOIN
		(
							SELECT DISTINCT SentEmail = VCC_Email FROM dbo.ViewCampaignContact WHERE VCC_PK IN
			(
									SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_G0 = CONVERT
				(
					'{0}', 'System.Guid'
				)
			)
							AND
							VCC_Email != ''
		)
		 sentItems ON contacts.VCC_Email = sentItems.SentEmail
					WHERE
		(
							sentItems.SentEmail is null
		)
					AND
		(
							VCC_Email != '' 
			OR
			VCC_PK NOT IN
			(
									SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_G0 = CONVERT
				(
					'{0}', 'System.Guid'
				)
				AND
				G8_RecipientID is not null
			)
		)
					AND
		VCC_PK IN 
		(
			SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
			AND
			G8_G0 = CONVERT
			(
				'00000000-0000-0000-0000-000000000000', 'System.Guid'
			)
		)
		AND
		(
			VCC_OrgIsActive = 1 
			OR
			VCC_OH is NULL
		)
		GROUP BY VCC_Email, case VCC_Email when '' then VCC_PK else null end
	)
	 a
)";

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				AssertMultilineASCIIEquals("SQL query should match for campaign tracking", string.Format(expectedQuery, Campaign.PK.ToString()), CampaignFilter.Filter.LiteralTextADOFormatted);
			}
		}

		public void TestIgnoreDuplicatesWhenEmailExistsInCampaignItem()
		{
			var result = (ModuleNkFilter)CampaignFilter["Organization"];
			AssertNotNull(result);

			organisation.OH_Code = "FLNFLN";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_Email = "eddy@yahoo.com";
			contact2.OC_ContactName = "Teddy";
			var contact3 = organisation.Contacts.AddNew();
			contact3.OC_Email = "th@thanks.com";
			contact3.OC_ContactName = "Richard";

			Contact.OC_Email = "eddy@yahoo.com";
			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact2.PK;
			campaignItem.G8_RecipientTableCode = "OC";

			Factory.Save();

			result.Property = organisation.OH_Code;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			ZQuery query = CampaignFilter.Filter;
			collection.Load(query);

			AssertEquals("There should be only 1 item in the list", 1, collection.Count);
			AssertEquals("Contact3 should be the lucky one in the list", contact3.PK, collection[0].PK);
		}

		public void TestIgnoreDuplicatesWhenContactExistsInCampaignItem()
		{
			var result = (ModuleNkFilter)CampaignFilter["Organization"];
			AssertNotNull(result);

			organisation.OH_Code = "FLNFLN";

			Contact.OC_Email = "eddy@yahoo.com";
			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = Contact.PK;
			campaignItem.G8_RecipientTableCode = "OC";

			Factory.Save();

			result.Property = organisation.OH_Code;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);
			ZQuery query = CampaignFilter.Filter;
			collection.Load(query);

			AssertEquals("There should be no item in the list", 0, collection.Count);
		}

		public void TestDataFromTableDeciderFilter()
		{
			var result = (ModuleTextFilter)CampaignFilter["TableDecider"];
			AssertNotNull(result);
			Assert(result.Visibility == FilterVisibility.AlwaysAppliedAndHidden);
		}

		public void TestContactDetailsVerifiedDateFilter()
		{
			var result = (ModuleDateFilter)CampaignFilter["Contact Details Verified Date"];
			AssertNotNull(result);
		}

		public void TestInquiryPKFilter()
		{
			var result = (ModuleGuidFilter)CampaignFilter["Inquiry PK"];
			AssertNotNull(result);

			var inquiry = Factory.New<SalesEnquiry>();
			Factory.Save();

			((IImportParentRelatedActivityInfoOnNew)Campaign).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			result = (ModuleGuidFilter)campaignFilter["Inquiry PK"];
			SwitchOffSubscriptionFilter(campaignFilter);
			AssertNotNull(result);
			Assert(result.Visibility != FilterVisibility.AlwaysAppliedAndHidden);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Factory);
			collection.Load(campaignFilter.Filter);
			AssertEquals(1, collection.Count);
		}

		public void TestVerifyMaxLengthSizeSalesTradeLane()
		{
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var salesTradeLaneFilterMaxLength = new[] { RefUNLOCOSchema.RL_Code.MaxLength, RefCountrySchema.RN_Code.MaxLength, RefZoneHeaderSchema.FZ_Code.MaxLength }.Max();

			AssertEquals(campaignFilter[GlbCompanyCampaignContactFilterBusinessObject.FilterDescription.SalesTradeLaneOriginPort].MaxLength, salesTradeLaneFilterMaxLength);
			AssertEquals(campaignFilter[GlbCompanyCampaignContactFilterBusinessObject.FilterDescription.SalesTradeLaneDestinationPort].MaxLength, salesTradeLaneFilterMaxLength);
		}

		#region Sales

		public void TestSalesAchievableBusiness()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Achievable Business"];
			AssertNotNull(result);

			result.Property = 55;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;

			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMAcheivableClientRevenue >= 55)", result.Query.LiteralTextADO);
		}

		public void TestSalesClientSize()
		{
			var result = (ModuleTextFilter)CampaignFilter["Sales - Client Size"];
			AssertNotNull(result);

			result.Property = "MED";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			AssertEquals("OM_CMClientSize = 'MED'", result.Query.LiteralTextADO);
		}

		public void TestSalesClientRelationship()
		{
			var result = (CampaignContactTextRangeFilter)CampaignFilter["Sales - Client Relationship"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMOverallClientRelation = 2;
			Factory.Save();

			result.Property = "2";
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMOverallClientRelation >= '2')", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesClientDesireToRemainWithCompany()
		{
			var result = (CampaignContactTextRangeFilter)CampaignFilter["Sales - Client Desire to Remain with Company"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMClientsDesireToRemain = 6;
			Factory.Save();

			result.Property = "6";
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMClientsDesireToRemain >= '6')", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesDifficultyWhichClientCanBePoached()
		{
			var result = (CampaignContactTextRangeFilter)CampaignFilter["Sales - Difficulty which Client can be poached"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMEaseClientCanBePoached = 6;
			Factory.Save();

			result.Property = "6";
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMEaseClientCanBePoached >= '6')", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void SalesAmountOfClientElectronicIntegration()
		{
			var result = (CampaignContactTextRangeFilter)CampaignFilter["Sales - Amount of Client Electronic Integration"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMAmountOfElectronicIntegration = 6;
			Factory.Save();

			result.Property = "6";
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMAmountOfElectronicIntegration >= '6')", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesConsultingRevenue()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Consulting Revenue"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMConsultingRevenue = 6000;
			Factory.Save();

			result.Property = 6000;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMConsultingRevenue >= 6000)", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesConversionCertainty()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Conversion Certainty"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMPercentage = 40;
			Factory.Save();

			result.Property = 40;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMPercentage >= 40)", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesEstimatedProfit()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Estimated Profit"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMEstimatedProfit = 400;
			Factory.Save();

			result.Property = 400;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMEstimatedProfit >= 400)", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesGrowthOutlook()
		{
			var result = (ModuleTextFilter)CampaignFilter["Sales - Growth Outlook"];
			AssertNotNull(result);

			CampaignFilter.ActiveModuleFilters.DeleteAll();

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMGrowthOutlook = "SDC";
			Factory.Save();

			result.Property = "SDC";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			AssertEquals("OM_CMGrowthOutlook = 'SDC'", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			ZQuery query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesPercentageWon()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Percentage Won"];
			AssertNotNull(result);

			CampaignFilter.ActiveModuleFilters.DeleteAll();

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMAmountOfBusinessWon = 90;
			Factory.Save();

			result.Property = 90;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMAmountOfBusinessWon >= '90')", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesRelatedStaff()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Related Staff"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMNoOfEmployees = 3;
			Factory.Save();

			result.Property = 3;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMNoOfEmployees >= 3)", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesSalesCategory()
		{
			var result = (ModuleTextFilter)CampaignFilter["Sales - Sales Category"];
			AssertNotNull(result);
		}

		public void TestVerticalMarket()
		{
			var assignedVerticalMarketType = "AERO";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			org1.MiscServ.OM_CMIndustryVertical = assignedVerticalMarketType;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			org2.MiscServ.OM_CMIndustryVertical = "FAKE";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			org3.MiscServ.OM_CMIndustryVertical = string.Empty;

			Factory.Save();

			var filter = (ModuleTextFilter)CampaignFilter["Vertical Market"];

			filter.Property = "AERO";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(CampaignFilter.Filter);

			Assert(collection.Contains(contact1));
			Assert(!collection.Contains(contact2));
			Assert(!collection.Contains(contact3));
		}

		public void TestSalesSalesTerritory()
		{
			var result = (ModuleTextFilter)CampaignFilter["Sales - Sales Territory"];
			AssertNotNull(result);
		}

		public void TestSalesTotalRevenue()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Total Revenue"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMTotalClientRevenue = 1305;
			Factory.Save();

			result.Property = 1305;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMTotalClientRevenue >= 1305)", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestSalesWarehouseRevenue()
		{
			var result = (CampaignContactNumberFilter)CampaignFilter["Sales - Warehouse Revenue"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_OH = organisation.PK;
			miscServ.OM_CMWarehouseRevenue = 13000;
			Factory.Save();

			result.Property = 13000;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;
			AssertEquals("VCC_TableCode = 'OC' and VCC_OH IN (SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_CMWarehouseRevenue >= 13000)", result.Query.LiteralTextADO);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		#endregion

		#region RelatedItemFilters

		public void TestContactsRelatedAccreditationAttemptsFilter()
		{
			var contactsfilter = (PersonAccreditationAttemptsFilter)CampaignFilter["Accreditation Attempts (by Contact)"];
			contactsfilter.IsActive = true;
			AssertContains("VCC_TableCode = 'OC' and VCC_PK IN (SELECT OC_PK FROM dbo.OrgContact WHERE OC_PER IN (SELECT PER_PK FROM dbo.GlbPerson WHERE PER_PK IN (SELECT HAA_PER FROM dbo.GlbAccreditationAttempt)))", CampaignFilter.Filter.LiteralTextADO);
		}

		public void TestContactsFilter()
		{
			var filter = CampaignFilter.OfType<ContactsModuleFilter>().FirstOrDefault(x => x.Description == "Contacts");
			AssertNotNull(filter);
			filter.IsActive = true;
			AssertContains("VCC_PK IN (SELECT OC_PK FROM dbo.OrgContact WHERE OC_OH IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_IsActive = 1) and OC_IsActive = 1))", CampaignFilter.Filter.LiteralTextADO);

			AssertEquals(filter.FilterColumn, ViewCampaignContactSchema.PK);
			AssertEquals(filter.ForeignKeyColumn, OrgContactSchema.PK);

			AssertEquals(true, filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.NoneMatch));
			AssertEquals(true, filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.AnyMatch));
			AssertEquals(false, filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.AllMatch));
		}

		#endregion

		#region StatusAndFlags

		public void TestContactSourceFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			CreateContactsForTest();

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";

			Factory.Save();

			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(campaign);
			var contactSourceFilter = (ModuleTextFilter)campaignFilter["Contact Source"];
			AssertNotNull(contactSourceFilter);
		}

		public void TestJobCategory()
		{
			var result = (ModuleTextFilter)CampaignFilter["Job Category"];
			AssertNotNull(result);
			AssertEquals(CampaignContactFilterCategories.ClientIntelligenceAndInquiries, result.Category);
		}

		public void TestContactAttributesFilter()
		{
			var orgForTest = Factory.NewWithValidTestData<OrgHeader>();
			var contactWithNoAttributes = orgForTest.Contacts.AddNew();
			contactWithNoAttributes.OC_ContactName = "Andrew";

			var contactWithUnsAttributeOnly = orgForTest.Contacts.AddNew();
			contactWithUnsAttributeOnly.OC_ContactName = "Richard";
			contactWithUnsAttributeOnly.Attributes.AddNew().PC_Type = "UNS";

			var contactWithUnsAndArtAttributes = orgForTest.Contacts.AddNew();
			contactWithUnsAndArtAttributes.OC_ContactName = "Edward";
			contactWithUnsAndArtAttributes.Attributes.AddNew().PC_Type = "UNS";
			contactWithUnsAndArtAttributes.Attributes.AddNew().PC_Type = "ART";

			Factory.Save();

			var contactAttributesFilter = (ModuleDependentItemTextFilter)CampaignFilter["Contact Attribute"];
			AssertNotNull(contactAttributesFilter);
			contactAttributesFilter.IsActive = true;

			var orgForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, orgForTest.PK);
			{
				contactAttributesFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				contactAttributesFilter.Property = "UNS";
				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contactWithUnsAttributeOnly,
						contactWithUnsAndArtAttributes,
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				contactAttributesFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				contactAttributesFilter.Property = "ART";
				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contactWithUnsAndArtAttributes
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				contactAttributesFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				contactAttributesFilter.Property = "UNS";
				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contactWithNoAttributes
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}
		}

		public void TestDocumentGroupsFilter()
		{
			var orgForTest = Factory.NewWithValidTestData<OrgHeader>();
			var contactNotInAnyDocGroups = orgForTest.Contacts.AddNew();
			contactNotInAnyDocGroups.OC_ContactName = "Andrew";

			var contactInCsvDocGroupOnly = orgForTest.Contacts.AddNew();
			contactInCsvDocGroupOnly.OC_ContactName = "Richard";
			contactInCsvDocGroupOnly.Documents.AddNew().OD_DocumentGroup = "CSV";

			var contactInCsvDocGroupButDoNotDeliver = orgForTest.Contacts.AddNew();
			contactInCsvDocGroupButDoNotDeliver.OC_ContactName = "Samuel";
			var doNotDeliverCsvGroup = contactInCsvDocGroupButDoNotDeliver.Documents.AddNew();
			doNotDeliverCsvGroup.OD_DocumentGroup = "CSV";
			doNotDeliverCsvGroup.OD_DeliverBy = Core.Constants.ContactNotifyModes.DoNotDeliver;

			var contactInCsvAndSalDocGroups = orgForTest.Contacts.AddNew();
			contactInCsvAndSalDocGroups.OC_ContactName = "Edward";
			contactInCsvAndSalDocGroups.Documents.AddNew().OD_DocumentGroup = "CSV";
			contactInCsvAndSalDocGroups.Documents.AddNew().OD_DocumentGroup = "SAL";

			Factory.Save();

			var documentGroupFilter = (ModuleDependentItemTextFilter)CampaignFilter["Document Group"];
			AssertNotNull(documentGroupFilter);
			documentGroupFilter.IsActive = true;

			var orgForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, orgForTest.PK);
			{
				documentGroupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				documentGroupFilter.Property = "CSV";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contactInCsvDocGroupOnly,
						contactInCsvAndSalDocGroups
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				documentGroupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				documentGroupFilter.Property = "SAL";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contactInCsvAndSalDocGroups
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				documentGroupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				documentGroupFilter.Property = "CSV";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contactNotInAnyDocGroups
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}
		}

		public void TestOfficialContactForDocumentGroup()
		{
			var result = (ModuleTextFilter)CampaignFilter["Official Contact for Document Group"];
			AssertNotNull(result);

			var contact1 = organisation.Contacts.AddNew();
			var orgDocument = contact1.Documents.AddNew();
			orgDocument.OD_DocumentGroup = "MRK";
			Factory.Save();

			result.Property = "MRK";
			result.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			result.IsActive = true;

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 1 item in the list", 1, collection.Count);
		}

		public void TestPayablesReceivablesOrganisationFilters()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = org1.OH_FullName = "Org1";
			org1.OH_IsDebtor = true;
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = org2.OH_FullName = "Org2";
			org2.OH_IsCreditor = true;
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";

			Factory.Save();

			var filter = (ModuleTextFilter)CampaignFilter["Organization Type"];
			filter.Property = "Receivables";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(filter.Query);

			Assert("Expect collection to contain contact1", collection.Contains(contact1));
			Assert("Expect collection to contain contact2", !collection.Contains(contact2));

			filter.Property = "Payables";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			collection.Load(filter.Query);

			Assert("Expect collection to contain contact1", !collection.Contains(contact1));
			Assert("Expect collection to contain contact2", collection.Contains(contact2));
		}

		public void TestOrganisationType()
		{
			var result = (ModuleTextFilter)CampaignFilter["Organization Type"];
			AssertNotNull(result);
		}

		public void TestMarketingOption()
		{
			var result = (ModuleTextFilter)CampaignFilter["Marketing Options"];
			AssertNotNull(result);
		}

		public void TestSecondaryType()
		{
			var result = (ModuleTextFilter)CampaignFilter["Secondary Type"];
			AssertNotNull(result);
		}

		public void TestMainCompetitorActivity()
		{
			var result = (ModuleTextFilter)CampaignFilter["Main Competitor Activity"];
			AssertNotNull(result);
		}

		public void TestSalesTradeLaneStatus()
		{
			var result = (ModuleTextFilter)CampaignFilter["Sales Trade Lane - Status"];
			AssertNotNull(result);
		}

		public void TestSalesTradeLaneProduct()
		{
			var result = (ModuleGuidFilter)CampaignFilter["Sales Trade Lane - Product"];
			AssertNotNull(result);
		}

		public void TestCloseReason()
		{
			var result = (ModuleTextFilter)CampaignFilter["Close Reason"];
			AssertNotNull(result);
			AssertEquals(CampaignContactFilterCategories.Inquiries, result.Category);
		}

		#endregion StatusAndFlags

		#region RelationshipOrgAndStaffCategory

		public void TestContactDetailsVerifiedBy()
		{
			var result = (ModuleNkFilter)CampaignFilter["Contact Details Verified By"];
			AssertNotNull(result);
		}

		public void TestOrganisation()
		{
			var result = (ModuleNkFilter)CampaignFilter["Organization"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AORG";
			var orgContact = org1.Contacts.AddNew();
			orgContact.OC_ContactName = "orgcontact";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "AINQUIRY";
			var orgInquiry = org2.Contacts.AddNew();
			orgInquiry.OC_ContactName = "orgInquiry";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org2.PK;
			inquiry.O1_OC_LinkedContact = orgInquiry.PK;

			Factory.Save();

			var expectedOrgContact = Factory.Load<CampaignContact>(orgContact.PK);
			var expectedOrgInquiry = Factory.Load<CampaignContact>(orgInquiry.PK);
			var expectedInquiryContact = Factory.Load<CampaignContact>(inquiry.PK);
			var collection = new GlbCampaignContactCollection(Campaign);

			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			result.Property = "AORG";

			string expectedSQL = @"VCC_OrgCode = 'AORG' 
AND
VCC_TableCode = 'OC'
";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedOrgContact, collection);

			result.Property = "AINQUIRY";
			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedOrgInquiry, collection);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Assert("Precondition", Campaign.IsUsingCampaignTrackingDataSource);

			result.Property = "AORG";
			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedOrgContact, collection);

			result.Property = "AINQUIRY";
			collection.Load(result.Query);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(expectedOrgInquiry, collection);
			AssertCollectionContains(expectedInquiryContact, collection);

			result.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			expectedSQL = "VCC_OrgCode <> 'AINQUIRY'";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);
		}

		public void TestBranch()
		{
			var result = (ModuleNkFilter)CampaignFilter["Branch"];
			AssertNotNull(result);
		}

		public void TestCountryOrPort()
		{
			organisation.OH_RL_NKClosestPort = "AUSYD";

			var contactInSYD = organisation.Contacts.AddNew();
			contactInSYD.OC_ContactName = "contactInSYD";
			contactInSYD.OC_Title = "TestTitle";
			var contactInNoBranch = organisation.Contacts.AddNew();
			contactInNoBranch.OC_ContactName = "contactInNoBranch";
			contactInNoBranch.OC_Title = "TestTitle";
			var contactInLHR = organisation.Contacts.AddNew();
			contactInLHR.OC_ContactName = "contactInLHR";
			contactInLHR.OC_Title = "TestTitle";

			var officeAddress1 = organisation.Addresses.AddNew();
			officeAddress1.OA_Address1 = "Main Office One";
			officeAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			var postalAddress = organisation.Addresses.AddNew();
			postalAddress.OA_Address1 = "Main Postal Address";
			postalAddress.OA_RL_NKRelatedPortCode = "GBLHR";
			var officeAddress2 = organisation.Addresses.AddNew();
			officeAddress2.OA_Address1 = "Main Office Two";
			officeAddress2.OA_RL_NKRelatedPortCode = "SGSIN";

			contactInSYD.OC_OA_OrgAddress = officeAddress1.PK;
			contactInLHR.OC_OA_OrgAddress = postalAddress.PK;

			var officeAddress1Capability = Factory.NewWithValidTestData<OrgAddressCapability>();
			officeAddress1Capability.PZ_OA = officeAddress1.PK;
			officeAddress1Capability.PZ_IsMainAddress = true;
			officeAddress1Capability.PZ_AddressType = OrgConstants.AddressType.Office;

			var postalAddressCapability = Factory.NewWithValidTestData<OrgAddressCapability>();
			postalAddressCapability.PZ_OA = postalAddress.PK;
			postalAddressCapability.PZ_IsMainAddress = true;
			postalAddressCapability.PZ_AddressType = OrgConstants.AddressType.Postal;

			var officeAddress2Capability = Factory.NewWithValidTestData<OrgAddressCapability>();
			officeAddress2Capability.PZ_OA = officeAddress2.PK;
			officeAddress2Capability.PZ_IsMainAddress = true;
			officeAddress2Capability.PZ_AddressType = OrgConstants.AddressType.Office;

			Factory.Save();

			var result = (ModuleNkFilter)CampaignFilter["Country / Port"];
			AssertNotNull(result);

			result.Property = "AU";
			result.IsActive = true;
			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 2, collection.Count);
			AssertEquals("Should contain contactInSYD", true, collection.Contains(contactInSYD));
			AssertEquals("Should contain contactInNoBranch", true, collection.Contains(contactInNoBranch));
			AssertEquals("Should contain contactInLHR", false, collection.Contains(contactInLHR));

			result.Property = "GB";
			result.IsActive = true;
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 1, collection.Count);
			AssertEquals("Should contain contactInSYD", false, collection.Contains(contactInSYD));
			AssertEquals("Should contain contactInNoBranch", false, collection.Contains(contactInNoBranch));
			AssertEquals("Should contain contactInLHR", true, collection.Contains(contactInLHR));

			result.Property = "";
			result.IsActive = true;
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 3, collection.Count);
			AssertEquals("Should contain contactInSYD", true, collection.Contains(contactInSYD));
			AssertEquals("Should contain contactInNoBranch", true, collection.Contains(contactInNoBranch));
			AssertEquals("Should contain contactInLHR", true, collection.Contains(contactInLHR));

			result.Property = "AUSYD";
			result.IsActive = true;
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 2, collection.Count);
			AssertEquals("Should contain contactInSYD", true, collection.Contains(contactInSYD));
			AssertEquals("Should contain contactInNoBranch", true, collection.Contains(contactInNoBranch));
			AssertEquals("Should contain contactInLHR", false, collection.Contains(contactInLHR));

			result.Property = "GBLHR";
			result.IsActive = true;
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 1, collection.Count);
			AssertEquals("Should contain contactInSYD", false, collection.Contains(contactInSYD));
			AssertEquals("Should contain contactInNoBranch", false, collection.Contains(contactInNoBranch));
			AssertEquals("Should contain contactInLHR", true, collection.Contains(contactInLHR));

			result.Property = "SGSIN";
			result.IsActive = true;
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 1, collection.Count);
			AssertEquals("Should contain contactInSYD", false, collection.Contains(contactInSYD));
			AssertEquals("Should contain contactInNoBranch", true, collection.Contains(contactInNoBranch));
			AssertEquals("Should contain contactInLHR", false, collection.Contains(contactInLHR));

			result.Property = "CN";
			result.IsActive = true;
			collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));
			AssertEquals("Number of items in the list", 0, collection.Count);
		}

		public void TestCountryOrPortWithOrgPortFallback()
		{
			OrgContact contact = null;

			Action<string, bool> assertContact = (filterValue, contactFound) =>
			{
				var result = (ModuleNkFilter)CampaignFilter["Country / Port"];
				result.Property = filterValue;
				result.IsActive = true;
				var collection = new GlbCampaignContactCollection(Campaign);
				collection.Load(result.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "TestTitle"));

				if (!contactFound)
				{
					AssertEquals(0, collection.Count);
				}
				else
				{
					AssertEquals(1, collection.Count);
					AssertEquals(contact.PK, collection[0].PK);
				}
			};

			//1.OC_OA_OrgAddress IS NOT NULL AND OA_RN_NKCountryCode = 'NZ'
			organisation.OH_RL_NKClosestPort = "AUSYD";
			contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Contact 001";
			contact.OC_Title = "TestTitle";

			var officeAddress1 = organisation.Addresses.AddNew();
			officeAddress1.OA_Address1 = "Main Office One";
			officeAddress1.OA_RN_NKCountryCode = "NZ";
			officeAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			officeAddress1.OA_RN_NKCountryCode = "NZ";

			var officeAddress1Capability = Factory.NewWithValidTestData<OrgAddressCapability>();
			officeAddress1Capability.PZ_OA = officeAddress1.PK;
			officeAddress1Capability.PZ_IsMainAddress = true;
			officeAddress1Capability.PZ_AddressType = OrgConstants.AddressType.Office;

			contact.OC_OA_OrgAddress = officeAddress1.PK;
			Factory.Save();

			AssertEquals("AUSYD", organisation.OH_RL_NKClosestPort);
			AssertEquals("NZ", officeAddress1.OA_RN_NKCountryCode);
			AssertEquals("AUSYD", officeAddress1.OA_RL_NKRelatedPortCode);
			AssertEquals(officeAddress1.PK, contact.OC_OA_OrgAddress);
			assertContact("NZ", true);
			assertContact("NZAKL", false);
			assertContact("NZAHU", false);

			officeAddress1.OA_RN_NKCountryCode = "AU";
			Factory.Save();
			AssertEquals("AUSYD", organisation.OH_RL_NKClosestPort);
			AssertEquals("AU", officeAddress1.OA_RN_NKCountryCode);
			AssertEquals("AUSYD", officeAddress1.OA_RL_NKRelatedPortCode);
			AssertEquals(officeAddress1.PK, contact.OC_OA_OrgAddress);
			assertContact("NZ", false);
			assertContact("NZAKL", false);
			assertContact("NZAHU", false);

			//2.OC_OA_OrgAddress IS NOT NULL AND OA_RL_NKRelatedPortCode = '' AND OH_RL_NKClosestPort LIKE 'NZ%'
			organisation.OH_RL_NKClosestPort = "NZAKL";
			officeAddress1.OA_RN_NKCountryCode = "AU";
			officeAddress1.OA_RL_NKRelatedPortCode = "";
			contact.OC_OA_OrgAddress = officeAddress1.PK;
			Factory.Save();

			AssertEquals("NZAKL", organisation.OH_RL_NKClosestPort);
			AssertEquals("AU", officeAddress1.OA_RN_NKCountryCode);
			AssertEquals("", officeAddress1.OA_RL_NKRelatedPortCode);
			AssertEquals(officeAddress1.PK, contact.OC_OA_OrgAddress);
			assertContact("NZ", true);
			assertContact("NZAKL", true);
			assertContact("NZAHU", false);

			organisation.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			AssertEquals("AUSYD", organisation.OH_RL_NKClosestPort);
			AssertEquals("AU", officeAddress1.OA_RN_NKCountryCode);
			AssertEquals("", officeAddress1.OA_RL_NKRelatedPortCode);
			AssertEquals(officeAddress1.PK, contact.OC_OA_OrgAddress);
			assertContact("NZ", false);
			assertContact("NZAKL", false);
			assertContact("NZAHU", false);

			//3.OC_OA_OrgAddress IS NULL AND OH_RL_NKClosestPort LIKE 'NZ%'
			organisation.OH_RL_NKClosestPort = "NZAKL";
			officeAddress1.OA_RN_NKCountryCode = "AU";
			officeAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			contact.OC_OA_OrgAddress = ZGuid.Empty;
			Factory.Save();

			AssertEquals("NZAKL", organisation.OH_RL_NKClosestPort);
			AssertEquals("AU", officeAddress1.OA_RN_NKCountryCode);
			AssertEquals("AUSYD", officeAddress1.OA_RL_NKRelatedPortCode);
			AssertEquals(ZGuid.Empty, contact.OC_OA_OrgAddress);
			assertContact("NZ", true);
			assertContact("NZAKL", true);
			assertContact("NZAHU", false);

			organisation.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			AssertEquals("AUSYD", organisation.OH_RL_NKClosestPort);
			AssertEquals("AU", officeAddress1.OA_RN_NKCountryCode);
			AssertEquals("AUSYD", officeAddress1.OA_RL_NKRelatedPortCode);
			AssertEquals(ZGuid.Empty, contact.OC_OA_OrgAddress);
			assertContact("NZ", false);
			assertContact("NZAKL", false);
			assertContact("NZAHU", false);
		}

		public void TestSalesTradeLaneOriginPort()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			var sales1 = org1.SalesCollection.AddNew();
			sales1.OW_OriginID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN").PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = false;
			var sales2 = org2.SalesCollection.AddNew();
			sales2.OW_OriginID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN").PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			var sales3 = org3.SalesCollection.AddNew();
			sales3.OW_OriginID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_IsSalesLead = false;
			var sales4 = org4.SalesCollection.AddNew();
			sales4.OW_OriginID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";
			var contact4 = org4.Contacts.AddNew();
			contact4.OC_ContactName = "contact4";

			Factory.Save();

			var filter = (ModuleNkFilter)CampaignFilter["Sales Trade Lane - Origin Port"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var orgsForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, new[] { org1.PK, org2.PK, org3.PK, org4.PK });
			{
				filter.Property = "CN";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact1,
						contact2
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.Property = "AU";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact3,
						contact4
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.Property = "GB";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					Array.Empty<OrgContact>(),
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}
		}

		public void TestSalesTradeLaneDestinationPort()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			var sales1 = org1.SalesCollection.AddNew();
			sales1.OW_DestinationID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN").PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = false;
			var sales2 = org2.SalesCollection.AddNew();
			sales2.OW_DestinationID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN").PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			var sales3 = org3.SalesCollection.AddNew();
			sales3.OW_DestinationID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_IsSalesLead = false;
			var sales4 = org4.SalesCollection.AddNew();
			sales4.OW_DestinationID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";
			var contact4 = org4.Contacts.AddNew();
			contact4.OC_ContactName = "contact4";

			Factory.Save();

			var filter = (ModuleNkFilter)CampaignFilter["Sales Trade Lane - Destination Port"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var orgsForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, new[] { org1.PK, org2.PK, org3.PK, org4.PK });
			{
				filter.Property = "CN";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact1,
						contact2
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.Property = "AU";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact3,
						contact4
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.Property = "GB";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					Array.Empty<OrgContact>(),
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}
		}

		public void TestSalesMainExportCommodity()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			org1.MiscServ.OM_RH_NKCMMainExportCmdty = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = true;
			org2.MiscServ.OM_RH_NKCMMainExportCmdty = "AAA";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			org3.MiscServ.OM_RH_NKCMMainExportCmdty = "BBB";
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_IsSalesLead = true;
			org4.MiscServ.OM_RH_NKCMMainExportCmdty = "";

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Richard";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "Edward";
			var contact4 = org4.Contacts.AddNew();
			contact4.OC_ContactName = "Samuel";

			Factory.Save();

			var filter = (ModuleNkFilter)CampaignFilter["Sales Main Export Commodity"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var orgsForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, new[] { org1.PK, org2.PK, org3.PK, org4.PK });
			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
				filter.Property = "AAA";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact1,
						contact2
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
				filter.Property = "AAA";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact3,
						contact4
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact4
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsNotBlank;

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact1,
						contact2,
						contact3
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}
		}

		public void TestSalesMainImportCommodity()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			org1.MiscServ.OM_RH_NKCMMainImportCmdty = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = true;
			org2.MiscServ.OM_RH_NKCMMainImportCmdty = "AAA";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			org3.MiscServ.OM_RH_NKCMMainImportCmdty = "BBB";
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_IsSalesLead = true;
			org4.MiscServ.OM_RH_NKCMMainImportCmdty = "";

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Richard";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "Edward";
			var contact4 = org4.Contacts.AddNew();
			contact4.OC_ContactName = "Samuel";

			Factory.Save();

			var filter = (ModuleNkFilter)CampaignFilter["Sales Main Import Commodity"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var orgsForTestFilter = new ZQuery(ViewCampaignContactSchema.VCC_OH, new[] { org1.PK, org2.PK, org3.PK, org4.PK });
			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
				filter.Property = "AAA";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact1,
						contact2
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
				filter.Property = "AAA";

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact3,
						contact4
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact4
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}

			{
				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsNotBlank;

				var combinedFilter = new ZQuery(CampaignFilter.Filter, orgsForTestFilter);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgContact>.PKOnlyComparer, x => x.OC_ContactName,
					new[]
					{
						contact1,
						contact2,
						contact3
					},
					Factory.Load<CampaignContact>(combinedFilter).Select(x => Factory.Load<OrgContact>(x.PK)));
			}
		}

		public void TestSalesTradeLaneCommodity()
		{
			var result = (ModuleNkFilter)CampaignFilter["Sales Trade Lane - Commodity"];
			AssertNotNull(result);
		}

		public void TestNotReceiveCampaign()
		{
			var result = (ModuleGuidFilter)CampaignFilter["Has Not Received Campaign"];
			AssertNotNull(result);
		}

		public void TestHasReceiveCampaign()
		{
			var result = (ModuleGuidFilter)CampaignFilter["Has Received Campaign"];
			AssertNotNull(result);
		}

		public void TestState()
		{
			var filter = (ModuleGuidFilter)CampaignFilter["State"];
			filter.IsActive = true;
			AssertNotNull(filter);

			var state01InJapan = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_Code, "01"), JoinCondition.And, new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, "JP")));
			var state01InMalaysia = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_Code, "01"), JoinCondition.And, new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, "MY")));

			var contact1 = GetContact("AU", "", "", "Somewhere with no state code", "Contact1", "Weber");
			var contact2 = GetContact(state01InJapan.RW_RN_NKCountryCode, state01InJapan.RW_Code, "JPAAE", "State 01 in Japan", "Contact2", "Weber", organisation);
			var contact3 = GetContact(state01InMalaysia.RW_RN_NKCountryCode, state01InMalaysia.RW_Code, "MYKUL", "State 01 in Malaysia", "Contact3", "Weber");

			Factory.Save();

			var collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, Campaign);

			AssertStateFilter(SQLComparisonOperator.IsBlank, null, new OrgContact[] { contact1 });
			AssertStateFilter(SQLComparisonOperator.IsNotBlank, null, new OrgContact[] { contact2, contact3 });
			AssertStateFilter(SQLComparisonOperator.Equal, state01InJapan, new OrgContact[] { contact2 });
			AssertStateFilter(SQLComparisonOperator.NotEqual, state01InJapan, new OrgContact[] { contact1, contact3 });

			organisation.OH_RL_NKClosestPort = "AUBNE";
			Factory.Save();

			AssertStateFilter(SQLComparisonOperator.Equal, state01InJapan, Array.Empty<OrgContact>());

			void AssertStateFilter(SQLComparisonOperator comparitomOperator, RefCountryStates state, OrgContact[] expectedContacts)
			{
				filter.SqlComparisonOperator = comparitomOperator;
				filter.Property = state?.PK ?? ZGuid.Empty;

				var query = filter.Query.AddToFilter(ViewCampaignContactSchema.VCC_Title, "Weber");
				collection.Load(query);

				AssertContainsExactElementsInAnyOrder(expectedContacts.Select(c => c.OC_ContactName), collection.Select(c => c.VCC_ContactName));
			}

			OrgContact GetContact(string countryCode, string stateCode, string port, string address1, string contactName, string contactTitle, OrgHeader org = null)
			{
				org = org ?? Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = port;
				var address = org.Addresses.AddNew();
				address.OA_State = stateCode;
				address.OA_Address1 = address1;
				address.OA_RN_NKCountryCode = countryCode;

				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = contactName;
				contact.OC_Title = contactTitle;
				contact.OC_OA_OrgAddress = address.PK;

				return contact;
			}
		}

		public void TestStaffAssignmentPersonAndRole()
		{
			var result = (StaffAssignmentPersonAndRoleModuleFilter)CampaignFilter["Staff Assignment Person And Role"];
			AssertNotNull(result);
		}

		public void TestReferringOrganisation()
		{
			var filter = (ModuleGuidFilter)CampaignFilter["Referring Organization"];
			AssertNotNull(filter);
			AssertEquals(CampaignContactFilterCategories.Inquiries, filter.Category);

			var refOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var refOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry1.O1_OH_SourceOfLead = refOrg1.PK;
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry2.O1_OH_SourceOfLead = refOrg2.PK;
			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			filter.Property = refOrg1.PK;
			filter.IsActive = true;

			Func<ZQuery> getCombinedQuery = () =>
			{
				var subGroupFilter = new ZQuery().AddToFilter(filter.FilterColumn, filter.SqlComparisonOperator, filter.Property);

				if (SpecialComparisonOperator.IsBlank == filter.SqlComparisonOperator)
				{
					subGroupFilter = new ZQuery().AddToFilter(filter.FilterColumn, null);
				}

				var combinedQuery = ((ModuleFilterSubGroup)filter.SubGroup).GetSubQuery(subGroupFilter);
				return combinedQuery;
			};

			var collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(getCombinedQuery());
			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection to not contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to not contain enquiry3", !collection.Contains(enquiry3));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(getCombinedQuery());
			Assert("Expect collection to not contain enquiry1", !collection.Contains(enquiry1));
			Assert("Expect collection to contain enquiry2", collection.Contains(enquiry2));
			Assert("Expect collection to not contain enquiry3", !collection.Contains(enquiry3));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			collection.Load(getCombinedQuery());
			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection to contain enquiry2", collection.Contains(enquiry2));
			Assert("Expect collection to not contain enquiry3", !collection.Contains(enquiry3));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			collection.Load(getCombinedQuery());
			Assert("Expect collection to not contain enquiry1", !collection.Contains(enquiry1));
			Assert("Expect collection to not contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to contain enquiry3", collection.Contains(enquiry3));
		}

		public void TestSalesMainCompetitorOnForwarding()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org2);
			var customsCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);

			var orgContact1 = CreateOrgContact(forwardingCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(forwardingCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(forwardingCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(customsCompetitor, "orgContact4");

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Forwarding, orgContact1, orgContact2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Forwarding, orgContact3, org2);
		}

		public void TestSalesMainCompetitorOnCustoms()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var customsCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org2);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			var orgContact1 = CreateOrgContact(customsCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(customsCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(customsCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(forwardingCompetitor, "orgContact4");

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Customs, orgContact1, orgContact2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Customs, orgContact3, org2);
		}

		public void TestSalesMainCompetitorOnWarehouse()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var warehouseCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org2);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			var orgContact1 = CreateOrgContact(warehouseCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(warehouseCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(warehouseCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(forwardingCompetitor, "orgContact4");

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Warehouse, orgContact1, orgContact2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Warehouse, orgContact3, org2);
		}

		public void TestSalesMainCompetitorOnLandTransport()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var landTransportCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			var orgContact1 = CreateOrgContact(landTransportCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(landTransportCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(landTransportCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(forwardingCompetitor, "orgContact4");

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.LandTransport, orgContact1, orgContact2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.LandTransport, orgContact3, org2);
		}

		public void TestSalesMainCompetitorOnCustomCompetitorType()
		{
			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)"TST REGISTRY", false);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();

				var customCompetitor1 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customCompetitor2 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customCompetitor3 = SetCompetitorToOrg(testCompetitorCode, org2);
				var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

				var orgContact1 = CreateOrgContact(customCompetitor1, "orgContact1");
				var orgContact2 = CreateOrgContact(customCompetitor2, "orgContact2");
				var orgContact3 = CreateOrgContact(customCompetitor3, "orgContact3");
				var orgContact4 = CreateOrgContact(forwardingCompetitor, "orgContact4");

				Factory.Save();

				AssertSalesMainCompetitorOnFilter(testCompetitorCode, orgContact1, orgContact2, 2, org1);
				AssertSalesMainCompetitorOnFilter(testCompetitorCode, orgContact3, org2);
			}
		}

		OrgContact CreateOrgContact(OrgHeader org, string contactName)
		{
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = contactName;

			return orgContact;
		}

		OrgHeader SetCompetitorToOrg(string competitorType, OrgHeader org)
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgCompetitor>();
			competitor.OCP_Type = competitorType;
			competitor.OCP_OH_Competitor = org.PK;
			competitor.OCP_OH_Parent = parentOrg.PK;

			return parentOrg;
		}

		void AssertSalesMainCompetitorOnFilter(ZString competitorType, OrgContact expected, OrgHeader targetOrg)
		{
			AssertSalesMainCompetitorOnFilter(competitorType, expected, null, 1, targetOrg);
		}

		void AssertSalesMainCompetitorOnFilter(ZString competitorType, OrgContact expected1, OrgContact expected2, int expectedCount, OrgHeader targetOrg)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (OrgSalesMainCompetitorModuleFilter)filterBizO["Sales Main Competitor On"];
			filter.IsActive = true;
			filter.CompetitorType = competitorType;
			filter.Competitor = targetOrg.PK;

			var campaignCollection = new GlbCampaignContactCollection(Factory, filterBizO.Filter);
			campaignCollection.Load();
			Assert("Collection contains the expected number of orgs", campaignCollection.Count == expectedCount);
			if (expectedCount == 2)
			{
				AssertEquals(campaignCollection.Where(x => x.VCC_ContactName == expected1.OC_ContactName).Count(), 1);
				AssertEquals(campaignCollection.Where(x => x.VCC_ContactName == expected2.OC_ContactName).Count(), 1);
			}
			else if (expectedCount == 1)
			{
				AssertEquals(campaignCollection.Where(x => x.VCC_ContactName == expected1.OC_ContactName).Count(), 1);
			}
		}

		public void TestHasMainCompetitorOnCustoms()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var customsCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			var orgContact1 = CreateOrgContact(customsCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(customsCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(customsCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(forwardingCompetitor1, "orgContact4");
			var orgContact5 = CreateOrgContact(forwardingCompetitor2, "orgContact5");

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.Customs, allOrgs: new[] { customsCompetitor1, customsCompetitor2, customsCompetitor3, forwardingCompetitor1, forwardingCompetitor2 },
				expectedOrgContactsWithCompetitor: new[] { orgContact1, orgContact2, orgContact3 }, expectedOrgContactsWithoutCompetitor: new[] { orgContact4, orgContact5 });
		}

		public void TestHasMainCompetitorOnForwarding()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org2);
			var customsCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org3);

			var orgContact1 = CreateOrgContact(forwardingCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(forwardingCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(forwardingCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(customsCompetitor1, "orgContact4");
			var orgContact5 = CreateOrgContact(customsCompetitor2, "orgContact5");

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.Forwarding, allOrgs: new[] { forwardingCompetitor1, forwardingCompetitor2, forwardingCompetitor3, customsCompetitor1, customsCompetitor2 },
				expectedOrgContactsWithCompetitor: new[] { orgContact1, orgContact2, orgContact3 }, expectedOrgContactsWithoutCompetitor: new[] { orgContact4, orgContact5 });
		}

		public void TestHasMainCompetitorOnLandTransport()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var landTransportCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			var orgContact1 = CreateOrgContact(landTransportCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(landTransportCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(landTransportCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(forwardingCompetitor1, "orgContact4");
			var orgContact5 = CreateOrgContact(forwardingCompetitor2, "orgContact5");

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.LandTransport, allOrgs: new[] { landTransportCompetitor1, landTransportCompetitor2, landTransportCompetitor3, forwardingCompetitor1, forwardingCompetitor2 },
				expectedOrgContactsWithCompetitor: new[] { orgContact1, orgContact2, orgContact3 }, expectedOrgContactsWithoutCompetitor: new[] { orgContact4, orgContact5 });
		}

		public void TestHasMainCompetitorOnWarehouse()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var warehouseCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			var orgContact1 = CreateOrgContact(warehouseCompetitor1, "orgContact1");
			var orgContact2 = CreateOrgContact(warehouseCompetitor2, "orgContact2");
			var orgContact3 = CreateOrgContact(warehouseCompetitor3, "orgContact3");
			var orgContact4 = CreateOrgContact(forwardingCompetitor1, "orgContact4");
			var orgContact5 = CreateOrgContact(forwardingCompetitor2, "orgContact5");

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.Warehouse, allOrgs: new[] { warehouseCompetitor1, warehouseCompetitor2, warehouseCompetitor3, forwardingCompetitor1, forwardingCompetitor2 },
				expectedOrgContactsWithCompetitor: new[] { orgContact1, orgContact2, orgContact3 }, expectedOrgContactsWithoutCompetitor: new[] { orgContact4, orgContact5 });
		}

		public void TestHasMainCompetitorOnCustomizedCompetitor()
		{
			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)"TST REGISTRY", false);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();

				var customizedCompetitorType1 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customizedCompetitorType2 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customizedCompetitorType3 = SetCompetitorToOrg(testCompetitorCode, org2);
				var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
				var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

				var orgContact1 = CreateOrgContact(customizedCompetitorType1, "orgContact1");
				var orgContact2 = CreateOrgContact(customizedCompetitorType2, "orgContact2");
				var orgContact3 = CreateOrgContact(customizedCompetitorType3, "orgContact3");
				var orgContact4 = CreateOrgContact(forwardingCompetitor1, "orgContact4");
				var orgContact5 = CreateOrgContact(forwardingCompetitor2, "orgContact5");

				Factory.Save();

				AssertHasMainCompetitorTypeOnFilter(testCompetitorCode, allOrgs: new[] { customizedCompetitorType1, customizedCompetitorType2, customizedCompetitorType3, forwardingCompetitor1, forwardingCompetitor2 },
					expectedOrgContactsWithCompetitor: new[] { orgContact1, orgContact2, orgContact3 }, expectedOrgContactsWithoutCompetitor: new[] { orgContact4, orgContact5 });
			}
		}

		void AssertHasMainCompetitorTypeOnFilter(ZString competitorType, OrgHeader[] allOrgs, OrgContact[] expectedOrgContactsWithCompetitor, OrgContact[] expectedOrgContactsWithoutCompetitor)
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (OrgHasMainCompetitorModuleFilter)filterBizO["Has Main Competitor On"];
			filter.IsActive = true;
			filter.CompetitorType = competitorType;
			filter.HasMainCompetitor = true;

			var filterQuery = filterBizO.Filter;
			filterQuery.AddToFilter(ViewCampaignContactSchema.VCC_OH, allOrgs.Select(org => org.PK));

			var campaignCollection = new GlbCampaignContactCollection(Factory);
			campaignCollection.Load(filterQuery);

			AssertEquals("There should be 3 campaign contacts with competitor in the collection", campaignCollection.Count, 3);

			foreach (var orgContact in expectedOrgContactsWithCompetitor)
			{
				AssertEquals(campaignCollection.Where(x => x.VCC_ContactName == orgContact.OC_ContactName).Count(), 1);
			}

			filter.HasMainCompetitor = false;
			filterQuery = filterBizO.Filter;
			filterQuery.AddToFilter(ViewCampaignContactSchema.VCC_OH, allOrgs.Select(org => org.PK));
			campaignCollection.Load(filterQuery);

			AssertEquals("There should be 2 campaign contacts without competitor in the collection", campaignCollection.Count, 2);

			foreach (var orgContact in expectedOrgContactsWithoutCompetitor)
			{
				AssertEquals(campaignCollection.Where(x => x.VCC_ContactName == orgContact.OC_ContactName).Count(), 1);
			}
		}

		#endregion RelationshipOrgAndStaffCategory

		#region TextFilters

		public void TestCity()
		{
			var result = (ModuleTextFilter)CampaignFilter["City"];
			AssertNotNull(result);

			CampaignFilter.ActiveModuleFilters.DeleteAll();

			OrgAddress address = organisation.Addresses.AddNew();
			address.OA_City = "DUMMY_CITY";
			address.OA_Address1 = "O'Riordan";

			OrgAddressCapability orgAddressCapability = Factory.NewWithValidTestData<OrgAddressCapability>();
			orgAddressCapability.PZ_OA = address.PK;
			orgAddressCapability.PZ_AddressType = "OFC";

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_OA_OrgAddress = address.PK;

			var inquiryContact = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiryContact.O1_City = "DUMMY_CITY";

			Factory.Save();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			Assert("Precondition", Campaign.IsUsingInquiryDataSource);

			result.Property = "DUMMY_CITY";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;
			string expectedSQL = "VCC_City = 'DUMMY_CITY'";
			AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

			GlbCampaignContactCollection collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals("There should be 2 item in the list", 2, collection.Count);
		}

		public void TestContactNameFilter()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			CreateContactsForTest();

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";

			Factory.Save();

			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(campaign);
			var contactNameFilter = (ModuleTextFilter)campaignFilter["Contact Name"];
			AssertNotNull(contactNameFilter);
		}

		public void TestLeadInterest()
		{
			var filter = (ModuleTextFilter)CampaignFilter["Lead Interest"];
			AssertNotNull(filter);
			AssertEquals(CampaignContactFilterCategories.Inquiries, filter.Category);
		}

		public void TestOrganizationName()
		{
			var filter = (ModuleTextFilter)CampaignFilter["Organization Name"];
			AssertNotNull(filter);
			AssertEquals(CampaignContactFilterCategories.CommonTypes, filter.Category);

			var enquiry1Org = Factory.NewWithValidTestData<OrgHeader>();
			enquiry1Org.OH_FullName = "AAA";
			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry1.O1_OH_ConvertedToQualifiedLead = enquiry1Org.PK;

			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry2.O1_CompanyName = "BBB";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "CCC";
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "CCC Contact";

			Factory.Save();

			var collection = new GlbCampaignContactCollection(Campaign);
			filter.Property = "AAA";
			collection.Load(filter.Query);
			Assert("Expect collection to contain enquiry1", collection.Contains(enquiry1));
			Assert("Expect collection to not contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to not contain contact", !collection.Contains(contact));

			filter.Property = "BBB";
			collection.Load(filter.Query);
			Assert("Expect collection to not contain enquiry1", !collection.Contains(enquiry1));
			Assert("Expect collection to contain enquiry2", collection.Contains(enquiry2));
			Assert("Expect collection to not contain contact", !collection.Contains(contact));

			filter.Property = "CCC";
			collection.Load(filter.Query);
			Assert("Expect collection to not contain enquiry1", !collection.Contains(enquiry1));
			Assert("Expect collection to not contain enquiry2", !collection.Contains(enquiry2));
			Assert("Expect collection to contain contact", collection.Contains(contact));
		}

		#endregion TextFilters

		#region Campaign Tracker

		public void TestSourceCampaignPK()
		{
			var result = (ModuleGuidFilter)CampaignFilter["Source Campaign"];
			AssertNotNull(result);
			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, result.Visibility);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			AssertEquals("Precondition", true, Campaign.IsUsingCampaignTrackingDataSource);
			Campaign.SourceCampaignPK = Guid.NewGuid();

			string expectedSQL = string.Format(@"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = CONVERT
	(
		'{0}', 'System.Guid'
	)
)", Campaign.SourceCampaignPK.ToString());

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);
			}
		}

		public void TestSentByPerson()
		{
			var result = (ModuleNkFilter)CampaignFilter["Sent By Person"];
			AssertNotNull(result);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JNG";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact1 = org.Contacts.AddNew();
			orgContact1.OC_ContactName = "orgContact1";
			var orgContact2 = org.Contacts.AddNew();
			orgContact2.OC_ContactName = "orgContact2";
			var inquiryContact1 = org.Contacts.AddNew();
			inquiryContact1.OC_ContactName = "inquiryContact";
			var inquiryContact2 = org.Contacts.AddNew();
			inquiryContact2.OC_ContactName = "inquiryContact2";

			Factory.Save();

			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry1.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry1.O1_OC_LinkedContact = inquiryContact1.PK;

			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry2.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry2.O1_OC_LinkedContact = inquiryContact2.PK;

			Factory.Save();

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = orgContact1.PK;
			campaignItem1.G8_SystemCreateUser = "JNG";
			var campaignItem2 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = inquiryContact1.PK;
			campaignItem2.G8_SystemCreateUser = "JNG";
			var campaignItem3 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = orgContact2.PK;
			var campaignItem4 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = inquiryContact2.PK;

			Factory.Save();

			var expectedOrgContact1 = Factory.Load<CampaignContact>(orgContact1.PK);
			var expectedInquiryContact1 = Factory.Load<CampaignContact>(inquiryContact1.PK);

			Campaign.SourceCampaignPK = previousCampaign.PK;

			result.Property = "JNG";
			result.IsActive = true;

			string expectedSQL = string.Format(@"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_SystemCreateUser = 'JNG' 
	AND
	G8_G0 = CONVERT
	(
		'{0}', 'System.Guid'
	)
)", previousCampaign.PK.ToString());

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);
			}

			var collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(expectedOrgContact1, collection);
			AssertCollectionContains(expectedInquiryContact1, collection);
		}

		public void TestLastEditUserFilter()
		{
			var result = (ModuleNkFilter)CampaignFilter["Last Edit User"];
			AssertNotNull(result);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "EDN";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact1 = org.Contacts.AddNew();
			orgContact1.OC_ContactName = "orgContact1";
			var orgContact2 = org.Contacts.AddNew();
			orgContact2.OC_ContactName = "orgContact2";
			var inquiryContact1 = org.Contacts.AddNew();
			inquiryContact1.OC_ContactName = "inquiryContact";
			var inquiryContact2 = org.Contacts.AddNew();
			inquiryContact2.OC_ContactName = "inquiryContact2";

			Factory.Save();

			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry1.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry1.O1_OC_LinkedContact = inquiryContact1.PK;

			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry2.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry2.O1_OC_LinkedContact = inquiryContact2.PK;

			Factory.Save();

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = orgContact1.PK;
			campaignItem1.G8_SystemLastEditUser = "EDN";
			var campaignItem2 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = inquiryContact1.PK;
			campaignItem2.G8_SystemLastEditUser = "EDN";
			var campaignItem3 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = orgContact2.PK;
			var campaignItem4 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = inquiryContact2.PK;

			Factory.Save();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Campaign.SourceCampaignPK = previousCampaign.PK;

			result.Property = "EDN";
			result.IsActive = true;

			var collection = new GlbCampaignContactCollection(Campaign);
			collection.Load(result.Query);
			AssertEquals(0, collection.Count);

			result.Property = "E";
			result.IsActive = true;
			collection.Load(result.Query);
			AssertEquals("Last Edit User updates all campaign items created based on currently logged on user", 4, collection.Count);
		}

		public void TestPrimaryWorkplaceContactsOnly()
		{
			var result = (ModuleTextFilter)CampaignFilter["Is Primary Workplace Contact"];
			AssertNotNull(result);

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Test User Only - Fred Nerk";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Test User Only - Jason Smith";

			var glbStaff1 = Factory.New<GlbStaff>();
			var glbStaff2 = Factory.New<GlbStaff>();

			glbStaff1.GS_PER = person1.PK;
			glbStaff1.GS_Title = "boss";
			glbStaff1.GS_IsActive = false;
			glbStaff1.GS_Code = "aaa";
			glbStaff1.GS_LoginName = "login1";

			glbStaff2.GS_PER = person2.PK;
			glbStaff2.GS_Title = "developer";
			glbStaff2.GS_Code = "bbb";
			glbStaff2.GS_LoginName = "login2";

			var primaryRelationshipPivot = Factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationshipPivot.PPR_PER = person1.PK;
			primaryRelationshipPivot.Primary = glbStaff1;

			Factory.Save();

			var expectedContact1 = Factory.Load<CampaignContact>(glbStaff1.PK);
			var expectedContact2 = Factory.Load<CampaignContact>(glbStaff2.PK);

			result.Property = OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.PrimaryWorkplaceContactsOnly;
			result.IsActive = true;

			var collection = new GlbCampaignContactCollectionTest(Campaign);
			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);

			result.Property = OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.NonPrimaryWorkplaceContacts;

			collection = new GlbCampaignContactCollectionTest(Campaign);
			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact2, collection);

			result.Property = OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.AllContacts;

			collection = new GlbCampaignContactCollectionTest(Campaign);
			collection.Load(result.Query);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
			AssertCollectionContains(expectedContact2, collection);
		}

		public void TestTrackingStatus()
		{
			var result = (ModuleTextFilter)CampaignFilter["Tracking Status"];
			AssertNotNull(result);

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();
			org1Contact.OC_Email = "org1Contact@org.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Contact = org2.Contacts.AddNew();
			org2Contact.OC_Email = "org2Contact@org.com";

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_TrackingStatus = "NDR";
			var campaignItem2 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = org2Contact.PK;
			campaignItem2.G8_TrackingStatus = "UNV";

			Factory.Save();

			Campaign.SourceCampaignPK = previousCampaign.PK;

			result.Property = "NDR";
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			string expectedSQL = string.Format(@"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_TrackingStatus = 'NDR' 
	AND
	G8_G0 = CONVERT
	(
		'{0}', 'System.Guid'
	)
)", previousCampaign.PK.ToString());

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

				var expectedOrg1Contact = Factory.Load<CampaignContact>(org1Contact.PK);
				var expectedOrg2Contact = Factory.Load<CampaignContact>(org2Contact.PK);

				var collection = new GlbCampaignContactCollection(Campaign);
				collection.Load(result.Query);
				AssertEquals(1, collection.Count);
				AssertCollectionContains(expectedOrg1Contact, collection);

				result.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

				expectedSQL = string.Format(@"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_TrackingStatus <> 'NDR' 
	AND
	G8_G0 = CONVERT
	(
		'{0}', 'System.Guid'
	)
)", previousCampaign.PK.ToString());

				AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);

				collection.Load(result.Query);
				AssertEquals(1, collection.Count);
				AssertCollectionContains(expectedOrg2Contact, collection);
			}
		}

		[TestDate(2015, 2, 19, 13, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestCreatedTime()
		{
			var result = (ModuleDateFilter)CampaignFilter["Created Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Contact = org2.Contacts.AddNew();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org3Contact = org3.Contacts.AddNew();

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var campaignItem2 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = org2Contact.PK;
			campaignItem2.G8_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var campaignItem3 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = org3Contact.PK;
			campaignItem3.G8_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(2);

			Factory.Save();

			var expectedContact1 = Factory.Load<CampaignContact>(org1Contact.PK);
			var expectedContact2 = Factory.Load<CampaignContact>(org2Contact.PK);
			var expectedContact3 = Factory.Load<CampaignContact>(org3Contact.PK);

			Campaign.SourceCampaignPK = previousCampaign.PK;

			var collection = new GlbCampaignContactCollection(Campaign);
			result.IsActive = true;
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			result.Property1 = ZDateTime.Now;
			string expectedSQL = string.Format(@"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = CONVERT
	(
		'{0}', 'System.Guid'
	)
	AND
	G8_SystemCreateTimeUtc >= #2015-02-19 13:00:00.000#
)", previousCampaign.PK.ToString());

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);
				collection.Load(result.Query);
				AssertEquals(3, collection.Count);
				AssertCollectionContains(expectedContact1, collection);
				AssertCollectionContains(expectedContact2, collection);
				AssertCollectionContains(expectedContact3, collection);

				result.Property1 = ZDateTime.Now;
				result.Property2 = ZDateTime.Now;
				expectedSQL = string.Format(@"VCC_PK IN 
(
	SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_RecipientID IS NOT NULL 
	AND
	G8_G0 = CONVERT
	(
		'{0}', 'System.Guid'
	)
	AND
	G8_SystemCreateTimeUtc >= #2015-02-19 13:00:00.000# 
	AND
	G8_SystemCreateTimeUtc < #2015-02-20 12:59:00.000#
)", previousCampaign.PK.ToString());

				AssertMultilineASCIIEquals("Strings should be the same", expectedSQL, result.Query.LiteralTextADOFormatted);
				collection.Load(result.Query);
				AssertEquals(1, collection.Count);
				AssertCollectionContains(expectedContact1, collection);

				result.Property1 = ZDateTime.Now.AddDays(1);
				result.Property2 = ZDateTime.Now.AddDays(2);
				collection.Load(result.Query);
				AssertEquals(2, collection.Count);
				AssertCollectionContains(expectedContact2, collection);
				AssertCollectionContains(expectedContact3, collection);
			}
		}

		[TestDate(2015, 2, 19, 13, 0, 0)]
		public void TestLastEditTimeFilter()
		{
			var result = (ModuleDateFilter)CampaignFilter["Last Edit Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;

			Factory.Save();

			var expectedContact1 = Factory.Load<CampaignContact>(org1Contact.PK);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Campaign.SourceCampaignPK = previousCampaign.PK;

			var collection = new GlbCampaignContactCollection(Campaign);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			result.Property1 = ZDateTime.Now;
			result.IsActive = true;

			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
		}

		[TestDate(2015, 10, 22, 8, 9, 10)]
		public void TestLastSentTimeFilter()
		{
			var result = (ModuleDateFilter)CampaignFilter["Last Sent Time"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;
			campaignItem1.G8_LastSentTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			var expectedContact1 = Factory.Load<CampaignContact>(org1Contact.PK);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Campaign.SourceCampaignPK = previousCampaign.PK;

			var collection = new GlbCampaignContactCollection(Campaign);
			result.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			result.Property1 = ZDateTime.Now;
			result.IsActive = true;

			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
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

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;

			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "AA Pheobe";

			var campaignItem2 = previousCampaign.CampaignsItemsSent.AddNew();
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

			Campaign.SourceCampaignPK = previousCampaign.PK;

			Factory.Save();

			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { Contact, contact2 }, Campaign);

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNotNull(result);

			result.Property = 0;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);

			result.Property = 2;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;

			collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { Contact, contact2 }, Campaign);

			query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			campaignClick2.GCC_ClickTimeUtc = clickTime.AddHours(1);
			Factory.Save();

			query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			result.Property = 1;
			result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
			result.IsActive = true;

			query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(1, collection.Count);
		}

		[TestDate(2015, 5, 21, 12, 12, 12)]
		public void TestDistinctDaysActivityCount_WithInquiry()
		{
			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var link = previousCampaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.google.com";
			link.GCL_Context = "Google Searcher";

			var link2 = previousCampaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_ContactName = "Edward Johns";
			inquiry.O1_PortOrCountry = "";

			var campaignItem = previousCampaign.CampaignsItemsSent.AddNew();
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

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Campaign.SourceCampaignPK = previousCampaign.PK;

			Factory.Save();

			GlbCampaignContactCollection collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { inquiry }, Campaign);

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNotNull(result);

			result.Property = 0;
			result.SqlComparisonOperator = SQLComparisonOperator.Equal;
			result.IsActive = true;

			ZQuery query = CampaignFilter.Filter;
			collection.Load(query);
			AssertEquals(0, collection.Count);

			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = true;

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				// GetDateInLocalTime() SQL Function should be using branch UNLOCO
				result.Property = 2;
				result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
				result.IsActive = true;
				collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { inquiry }, Campaign);
				query = CampaignFilter.Filter;
				collection.Load(query);
				AssertEquals(0, collection.Count);

				result.Property = 1;
				result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
				result.IsActive = true;
				collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { inquiry }, Campaign);
				query = CampaignFilter.Filter;
				collection.Load(query);
				AssertEquals(1, collection.Count);

				inquiry.O1_PortOrCountry = "AUBNE";     //	10.00 hours
				Factory.Save();

				// GetDateInLocalTime SQL Function should be using inquiry UNLOCO
				result.Property = 2;
				result.SqlComparisonOperator = SQLComparisonOperator.GreaterThanOrEqualTo;
				result.IsActive = true;
				collection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { inquiry }, Campaign);
				query = CampaignFilter.Filter;
				collection.Load(query);
				AssertEquals(1, collection.Count);
			}
		}

		public void TestFiltersRemoved_WithTargetList()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			CampaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(campaign);

			var result = (CampaignContactNumberFilter)CampaignFilter["Unique Day(s) Activity Count"];
			AssertNull(result);

			var result2 = (CampaignContactContextLinkActivityModuleFilter)CampaignFilter["Has Context Activity"];
			AssertNull(result2);

			var result3 = (CampaignContactDestinationURLLinkActivityModuleFilter)CampaignFilter["Has Destination URL Activity"];
			AssertNull(result2);
		}

		public void TestCreatedOnWebFilter()
		{
			var result = (ModuleTextFilter)CampaignFilter["Created On Web/Internal"];
			AssertNotNull(result);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Contact = org1.Contacts.AddNew();

			var previousCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = previousCampaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = org1Contact.PK;

			Factory.Save();

			var expectedContact1 = Factory.Load<CampaignContact>(org1Contact.PK);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Campaign.SourceCampaignPK = previousCampaign.PK;

			var collection = new GlbCampaignContactCollection(Campaign);

			result.IsActive = true;
			AssertEquals("Default Property", "ALL", result.Property);

			result.Property = "ALL";
			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);

			result.Property = "WEB";
			collection.Load(result.Query);
			AssertEquals(0, collection.Count);

			result.Property = "ENT";
			collection.Load(result.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(expectedContact1, collection);
		}

		#endregion

		#region DateFilters

		public void TestNotReceiveCampaignsByDate()
		{
			var result = (ModuleDateFilter)CampaignFilter["Has Not Received Campaigns By Date"];
			AssertNotNull(result);
		}

		public void TestHasReceivedCampaignsByDate()
		{
			var result = (ModuleDateFilter)CampaignFilter["Has Received Campaigns By Date"];
			AssertNotNull(result);
		}

		#endregion

		#region InquiryFilters

		public void TestInquiryType()
		{
			var result = (ModuleTextFilter)CampaignFilter["Inquiry Type"];
			AssertNotNull(result);
		}

		public void TestInquirySourceType()
		{
			var result = (ModuleTextFilter)CampaignFilter["Inquiry Source Type"];
			AssertNotNull(result);
		}

		public void TestInquirySourceDetails()
		{
			var result = (ModuleTextFilter)CampaignFilter["Inquiry Source Details"];
			AssertNotNull(result);
		}

		public void TestInquiryOriginalCallDate()
		{
			var result = (ModuleDateFilter)CampaignFilter["Inquiry Original Call Date"];
			AssertNotNull(result);
		}

		public void TestInquiryStatus()
		{
			var result = (ModuleTextFilter)CampaignFilter["Inquiry Status"];
			AssertNotNull(result);
		}

		public void TestInquiryAssignedSalesRep()
		{
			var result = (ModuleNkFilter)CampaignFilter["Inquiry Assigned Sales Rep"];
			AssertNotNull(result);
		}

		public void TestInquiryHasLinkedToOrganisation()
		{
			var result = (ModuleTextFilter)CampaignFilter["Inquiry Has Linked to Organisation"];
			AssertNotNull(result);
		}

		#endregion

		#region Custom Sql Filter

		public void TestCustomSqlFilter_CampaignTracking()
		{
			using (var module = new GlbCompanyCampaignContactModule())
			{
				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				module.Campaign = Campaign;

				AssertEquals(true, module.FilterBusinessObject.HasCustomSqlFilter);
				AssertCollectionContains(
					module.FilterBusinessObject.ModuleFilters.Filter_List.Cast<ICodeDescription>(),
					x => x.Code == FilterStripBusinessObject.CustomSqlFilterDescription);
			}
		}

		public void TestCustomSqlFilter_ClientIntelligence()
		{
			using (var module = new GlbCompanyCampaignContactModule())
			{
				Campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
				module.Campaign = Campaign;

				AssertEquals(true, module.FilterBusinessObject.HasCustomSqlFilter);
				AssertCollectionContains(
					module.FilterBusinessObject.ModuleFilters.Filter_List.Cast<ICodeDescription>(),
					x => x.Code == FilterStripBusinessObject.CustomSqlFilterDescription);
			}
		}

		public void TestCustomSqlFilter_Inquiries()
		{
			using (var module = new GlbCompanyCampaignContactModule())
			{
				Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				module.Campaign = Campaign;

				AssertEquals(true, module.FilterBusinessObject.HasCustomSqlFilter);
				AssertCollectionContains(
					module.FilterBusinessObject.ModuleFilters.Filter_List.Cast<ICodeDescription>(),
					x => x.Code == FilterStripBusinessObject.CustomSqlFilterDescription);
			}
		}

		#endregion

		#region BMS filters for Campaign Tracking Data Source

		public void TestFilters_ShouldNotExistWithoutBMSForCampaignTracking()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available for Campaing Tracking data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available for Campaing Tracking data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));

			testHelper.CreateSystem(Factory, new string[] { "INQ", "CAM" });
			Factory.Save();
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available for Campaing Tracking data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available for Campaing Tracking data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));
		}

		public void TestFilters_ShouldExistWithBMSForCampaignTracking()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var filterList = campaignFilter.ModuleFilters.Filter_List;

			testHelper.CreateSystem(Factory, "ORG");
			Factory.Save();
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should be available for Campaing Tracking data source when ORG workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should be available for Campaing Tracking data source when ORG workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Magnitude")));
		}

		#endregion

		#region BMS filters for Client Intelligence Data Source

		public void TestFilters_ShouldNotExistWithoutBMSForClientIntelligence()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available for Organization data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available for Organization data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));

			testHelper.CreateSystem(Factory, new string[] { "CAM", "INQ" });
			Factory.Save();
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available for Organization data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available for Organization data source when ORG workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));
		}

		public void TestFilters_ShouldExistWithBMSForClientIntelligence()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var filterList = campaignFilter.ModuleFilters.Filter_List;

			testHelper.CreateSystem(Factory, "ORG");
			Factory.Save();
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should be available for Organization data source when ORG workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should be available for Organization data source when ORG workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Magnitude")));
		}

		#endregion

		#region BMS filters for Inquiries Data Source

		public void TestFilters_ShouldNotExistWithoutBMSForInquiries()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available for Inquiry data source when INQ workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available for Inquiry data source when INQ workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));

			testHelper.CreateSystem(Factory, new string[] { "CAM", "ORG" });
			Factory.Save();
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available for Inquiry data source when INQ workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available for Inquiry data source when INQ workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));
		}

		public void TestFilters_ShouldExistWithBMSForInquiries()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var filterList = campaignFilter.ModuleFilters.Filter_List;

			testHelper.CreateSystem(Factory, "INQ");
			Factory.Save();
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterList = campaignFilter.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should be available for Inquiry data source when INQ workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should be available for Inquiry data source when INQ workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Magnitude")));
		}

		#endregion

		#region Tag and Tag Group Filters

		public void TestTagAndTagGroupFilter_InquiryManager()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			testHelper.CreateSystem(Factory, "INQ");

			var tagGroup1 = testHelper.CreateTagDefinition(Factory, "GR1");
			var tag1_1 = testHelper.CreateTagMagnitude(tagGroup1, "AA1");
			var tag1_2 = testHelper.CreateTagMagnitude(tagGroup1, "BB1");

			var tagGroup2 = testHelper.CreateTagDefinition(Factory, "GR2");
			var tag2_1 = testHelper.CreateTagMagnitude(tagGroup2, "AA2");
			var tag2_2 = testHelper.CreateTagMagnitude(tagGroup2, "BB2");

			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();

			var inquiryTask1 = inquiry1.WorkflowItems.AddNew();
			var inquiryWorkflow1 = (ITagable)inquiryTask1.ProcessHeader;

			var inquiryTask2 = inquiry2.WorkflowItems.AddNew();
			var inquiryWorkflow2 = (ITagable)inquiryTask2.ProcessHeader;

			var inquiryTask3 = inquiry3.WorkflowItems.AddNew();
			var inquiryWorkflow3 = (ITagable)inquiryTask3.ProcessHeader;

			var inquiryTask4 = inquiry4.WorkflowItems.AddNew();
			var inquiryWorkflow4 = (ITagable)inquiryTask4.ProcessHeader;

			inquiryWorkflow1.AddTag(tag1_1);
			inquiryWorkflow2.AddTag(tag1_1);
			inquiryWorkflow3.AddTag(tag2_1);
			Factory.Save();

			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			var tagFilter = (ModuleGuidFilter)campaignFilter["Tag Magnitude"];
			var tagGroupFilter = (ModuleGuidFilter)campaignFilter["Tag Definition Code"];
			AssertNotNull("Precondition: New campaign filters initialised after creating BMS should include PAVE-related filters.", tagFilter);
			AssertNotNull("Precondition: New campaign filters initialised after creating BMS should include PAVE-related filters.", tagGroupFilter);

			var results = Factory.Load<CampaignContact>(campaignFilter.Filter);
			AssertEquals("Precondition: Should contain all sales inquiries objects", 4, results.Length);

			tagFilter.Property = tag1_1.PK;
			tagFilter.IsActive = true;
			tagFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			results = Factory.Load<CampaignContact>(campaignFilter.Filter);
			var expectedGuid = new[] { inquiry1.PK, inquiry2.PK };
			AssertContainsExactElementsInAnyOrder("Should only contain inquiry1 and inquiry2 that have tag1_1 applied", expectedGuid, results.Select(x => x.PK));

			tagFilter.Property = ZGuid.Empty;

			tagGroupFilter.Property = tagGroup2.PK;
			tagGroupFilter.IsActive = true;
			tagGroupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			results = Factory.Load<CampaignContact>(campaignFilter.Filter);
			expectedGuid = new[] { inquiry3.PK };
			AssertContainsExactElementsInAnyOrder("Should only contain inquiry3 that has tagGroup2 applied", expectedGuid, results.Select(x => x.PK));
		}

		public void TestBufferManagementComponentFilter_InquiryManager()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;

			var system = testHelper.CreateSystem(Factory, "INQ");
			var component1 = (IBMComponent)system.Components.AddNew();
			component1.FC_Name = "Component-1";
			var component2 = (IBMComponent)system.Components.AddNew();
			component2.FC_Name = "Component-2";

			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask1 = inquiry1.WorkflowItems.AddNew();
			var inquiryWorkflow1 = inquiryTask1.ProcessHeader;
			inquiryWorkflow1.FH_FC_CurrentComponent = component1.PK;

			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask2 = inquiry2.WorkflowItems.AddNew();
			var inquiryWorkflow2 = inquiryTask2.ProcessHeader;
			inquiryWorkflow2.FH_FC_CurrentComponent = component1.PK;

			var inquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask3 = inquiry3.WorkflowItems.AddNew();
			var inquiryWorkflow3 = inquiryTask3.ProcessHeader;
			inquiryWorkflow3.FH_FC_CurrentComponent = component2.PK;

			var inquiry4 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask4 = inquiry1.WorkflowItems.AddNew();
			var inquiryWorkflow4 = inquiryTask4.ProcessHeader;
			Factory.Save();

			var campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var componentFilter = (ModuleGuidFilter)campaignFilter["Buffer Management Component"];
			AssertNotNull("Precondition: New campaign filters initialised after creating BMS should include PAVE-related filters.", componentFilter);

			var list = componentFilter.List;
			AssertEquals("Should contain all buffer management components", 2, list.Count);
			AssertEquals(true, list.Cast<IBMComponent>().Any(x => x.FC_Name == "Component-1"));
			AssertEquals(true, list.Cast<IBMComponent>().Any(x => x.FC_Name == "Component-2"));

			componentFilter.IsActive = true;
			componentFilter.Property = component1.PK;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;

			var results = Factory.Load<CampaignContact>(campaignFilter.Filter);
			var expectedGuid = new[] { inquiry1.PK, inquiry2.PK };
			AssertContainsExactElementsInAnyOrder("Should contain inquiry1 and inquiry2 with component1 applied", expectedGuid, results.Select(x => x.PK));

			componentFilter.IsActive = true;
			componentFilter.Property = component2.PK;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;

			results = Factory.Load<CampaignContact>(campaignFilter.Filter);
			expectedGuid = new[] { inquiry3.PK };
			AssertContainsExactElementsInAnyOrder("Should contain inquiry3 with component2 applied", expectedGuid, results.Select(x => x.PK));
		}

		#endregion

		#endregion

		public void TestFindLayout()
		{
			GlbCompanyCampaignContactFilterBusinessObject campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext = "GlbCompanyCampaignContact";

			StmModuleFilter filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_FilterName = "Name One";
			filter1.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;

			StmModuleFilter filter2 = Factory.New<StmModuleFilter>();
			filter2.S9_FilterName = "Name Two";
			filter2.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter2.S9_IsPublished = true;
			filter2.S9_RelatedEntityID = Campaign.PK;

			StmModuleFilter filter3 = Factory.New<StmModuleFilter>();
			filter3.S9_FilterName = "Name Three";
			filter3.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter3.S9_IsPublished = true;
			filter3.S9_RelatedEntityID = ZGuid.NewZGuid();

			StmModuleFilter filter4 = Factory.New<StmModuleFilter>();
			filter4.S9_FilterName = "Name Four";
			filter4.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter4.S9_IsSystem = true;

			Factory.Save();

			var layout = campaignContactFilterStrip.FindLayout("Name Two", true);
			AssertNotNull(layout);
			AssertEquals("Name Two", layout.S9_FilterName);

			var layoutWithIsSystem = campaignContactFilterStrip.FindLayout("Name Four", true);
			AssertNotNull(layoutWithIsSystem);
			AssertEquals("Name Four", layoutWithIsSystem.S9_FilterName);

			var layoutWithoutRelatedEntityPK = campaignContactFilterStrip.FindLayout("Name Three", true);
			AssertNotNull(layoutWithoutRelatedEntityPK);
			AssertEquals("Still the last used layout", "Name Four", layoutWithIsSystem.S9_FilterName);
		}

		public void TestGetLastUsedLayout()
		{
			GlbCompanyCampaignContactFilterBusinessObject campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext = "GlbCompanyCampaignContact";

			StmModuleFilter filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_FilterName = "Name One";
			filter1.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter1.S9_IsPublished = true;
			filter1.S9_RelatedEntityID = Campaign.PK;
			Factory.Save();

			campaignContactFilterStrip.SaveLastUsedLayout(filter1.PK);

			var result = campaignContactFilterStrip.GetLastUsedLayout();
			AssertEquals(filter1.PK, result.PK);

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			StmModuleFilter filter2 = Factory.New<StmModuleFilter>();
			filter2.S9_FilterName = "Name Two";
			filter2.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter2.S9_IsPublished = true;
			filter2.S9_RelatedEntityID = campaign2.PK;
			Factory.Save();

			result = campaignContactFilterStrip.GetLastUsedLayout();
			AssertEquals(filter1.PK, result.PK);

			campaignContactFilterStrip.SaveLastUsedLayout(filter2.PK);

			result = campaignContactFilterStrip.GetLastUsedLayout();
			AssertEquals(filter1.PK, result.PK);
		}

		[ExpectNoExceptions]
		public void TestGetLastUsedLayoutWithUserData()
		{
			var campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var module = new GlbCompanyCampaignContactModule();
			campaignContactFilterStrip.ParentModule = module;
			((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext = "GlbCompanyCampaignContact_CampaignTracking";
			campaignContactFilterStrip.QueryObjectType = typeof(CampaignContact);

			const string testValue = "IM A TEST VALUE";

			const string userDefinedFilterData = @"<?xml version=""1.0""?>  <FilterLayoutSerializer>    <FilterStrips>      <FilterStrip>        <FilterDescription>SubscriptionStatus</FilterDescription>        <OrCategory>None</OrCategory>        <GroupOrCategory>None</GroupOrCategory>        <GroupName />        <AdditionalColourName />        <AdditionalGroupColourName />      </FilterStrip>      <FilterStrip>        <FilterDescription>Contact Name</FilterDescription>        <OrCategory>None</OrCategory>        <GroupOrCategory>None</GroupOrCategory>        <GroupName />        <AdditionalColourName />        <AdditionalGroupColourName />      </FilterStrip>    </FilterStrips>  </FilterLayoutSerializer>";
			var userDefinedFilter = Factory.New<StmModuleFilter>();
			userDefinedFilter.S9_FilterType = "USR";
			userDefinedFilter.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			userDefinedFilter.S9_IsPublished = true;
			userDefinedFilter.S9_FilterName = "Test1";
			userDefinedFilter.S9_FilterData = ZBlob.FromUTF8(userDefinedFilterData);

			const string filterData = @"<?xml version=""1.0""?>  <FilterLayoutSerializer>    <FilterStrips>      <FilterStrip>        <FilterDescription>SubscriptionStatus</FilterDescription>        <OrCategory>None</OrCategory>        <GroupOrCategory>None</GroupOrCategory>        <GroupName />        <AdditionalColourName />        <AdditionalGroupColourName />      </FilterStrip>      <FilterStrip>        <FilterDescription>[USR]Test1</FilterDescription>        <OrCategory>None</OrCategory>        <GroupOrCategory>None</GroupOrCategory>        <GroupName />        <AdditionalColourName />        <AdditionalGroupColourName />      </FilterStrip>    </FilterStrips>  </FilterLayoutSerializer>";
			var filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "Name One";
			filter.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter.S9_IsPublished = true;
			filter.S9_RelatedEntityID = Campaign.PK;
			filter.S9_FilterData = ZBlob.FromUTF8(filterData);

			var userDefinedFilterDataValues = $@"<?xml version=""1.0""?>  <FilterLayoutValuesSerializer>    <ModuleFilters>      <ModuleFilter>        <Comparer>not equal</Comparer>        <Property>UNS</Property>      </ModuleFilter>      <ModuleFilter>        <Comparer>starts with</Comparer>        <Property>{testValue}</Property>      </ModuleFilter>    </ModuleFilters>  </FilterLayoutValuesSerializer>";
			var userDefinedFilterUserData = Factory.New<StmModuleFilterUserData>();
			userDefinedFilterUserData.S0_FilterDataValues = ZBlob.FromUTF8(userDefinedFilterDataValues);
			userDefinedFilterUserData.S0_RelatedEntityTableCode = Campaign.TablePrefix;
			userDefinedFilterUserData.S0_RelatedEntityID = Env.CurrentUserPK;
			userDefinedFilterUserData.S0_S9 = userDefinedFilter.PK;

			var filterDataValues = $@"<?xml version=""1.0""?>  <FilterLayoutValuesSerializer>    <ModuleFilters>      <ModuleFilter>        <Comparer>not equal</Comparer>        <Property>UNS</Property>      </ModuleFilter>      <ModuleFilter>        <Comparer>filters match</Comparer>        <Property>00000000-0000-0000-0000-000000000000</Property>        <PropertySelectedFiltersData>&lt;?xml version=""1.0""?&gt;  &lt;FilterLayoutSerializer&gt;    &lt;FilterStrips&gt;      &lt;FilterStrip&gt;        &lt;FilterDescription&gt;Contact Name&lt;/FilterDescription&gt;        &lt;OrCategory&gt;None&lt;/OrCategory&gt;        &lt;GroupOrCategory&gt;None&lt;/GroupOrCategory&gt;        &lt;GroupName /&gt;        &lt;AdditionalColourName /&gt;        &lt;AdditionalGroupColourName /&gt;      &lt;/FilterStrip&gt;    &lt;/FilterStrips&gt;  &lt;/FilterLayoutSerializer&gt;</PropertySelectedFiltersData>        <PropertySelectedFiltersValues>&lt;?xml version=""1.0""?&gt;  &lt;FilterLayoutValuesSerializer&gt;    &lt;ModuleFilters&gt;      &lt;ModuleFilter&gt;        &lt;Comparer&gt;starts with&lt;/Comparer&gt;        &lt;Property&gt;{testValue}&lt;/Property&gt;      &lt;/ModuleFilter&gt;    &lt;/ModuleFilters&gt;  &lt;/FilterLayoutValuesSerializer&gt;</PropertySelectedFiltersValues>        <PropertySelectedFiltersModuleId>GlbCompanyCampaignContact</PropertySelectedFiltersModuleId>      </ModuleFilter>    </ModuleFilters>  </FilterLayoutValuesSerializer>";
			var filterUserData = Factory.New<StmModuleFilterUserData>();
			filterUserData.S0_FilterDataValues = ZBlob.FromUTF8(filterDataValues);
			filterUserData.S0_RelatedEntityTableCode = Campaign.TablePrefix;
			filterUserData.S0_RelatedEntityID = Campaign.PK;
			filterUserData.S0_S9 = filter.PK;

			Factory.Save();

			campaignContactFilterStrip.SaveLastUsedLayout(filter.PK);
			campaignContactFilterStrip.LoadLayout(campaignContactFilterStrip.GetLastUsedLayout());

			AssertEquals(2, campaignContactFilterStrip.ActiveModuleFilters.Count);

			var userDefinedModuleFilter = campaignContactFilterStrip.ActiveModuleFilters[1];
			Assert(userDefinedModuleFilter.Query.LiteralTextSqlFormatted.Contains(testValue));
			module.Dispose();
		}

		[ExpectNoExceptions]
		public void TestCurrentLayoutContextShouldNotThrowExceptionWhenCampaignIsNull()
		{
			var campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(null);
			var layoutContext = campaignContactFilterStrip.CurrentLayoutContext;
			campaignContactFilterStrip.SetCampaignFilterLayoutContext();
		}

		public void TestGetLastUsedLayout_WithUserPreferredLayout()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "RTG";

			GlbCompanyCampaignContactFilterBusinessObject campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext = "GlbCompanyCampaignContact";

			StmModuleFilter filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_FilterName = "Name One";
			filter1.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter1.S9_IsPublished = true;
			filter1.S9_RelatedEntityID = Campaign.PK;
			Factory.Save();

			StmModuleFilter moduleFilter = null;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				campaignContactFilterStrip.SaveLastUsedLayout(filter1.PK);

				var result = campaignContactFilterStrip.GetLastUsedLayout();
				AssertEquals(result.PK, filter1.PK);

				moduleFilter = campaignContactFilterStrip.GetLastUsedLayout();
				AssertEquals(moduleFilter.PK, filter1.PK);

				var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(campaign2);
				((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext = "GlbCompanyCampaignContact";
				campaignContactFilterStrip.layoutsHelper = new GlbCompanyCampaignFilterStripLayoutsHelper();
				campaignContactFilterStrip.LastUsedLayout = filter1;
				var stmData2 = campaignContactFilterStrip.GetLastUsedLayout();
				AssertEquals(stmData2.PK, filter1.PK);
			}

			//	Switch user
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "EO";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				moduleFilter = campaignContactFilterStrip.GetLastUsedLayout();
				AssertNotNull(moduleFilter);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				moduleFilter = campaignContactFilterStrip.GetLastUsedLayout();
				AssertEquals(moduleFilter.PK, filter1.PK);
			}
		}

		public void TestLastUsedContactDataSource()
		{
			GlbCompanyCampaignContactFilterBusinessObject campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			var data = Factory.NewWithValidTestData<StmData>();
			data.SD_DepartmentGuid = Env.Instance.CurrentCompany.PK;
			data.SD_Type = "CUR";
			data.SD_Owner = Env.CurrentUser.PK;
			data.SD_Name = "GlbCompanyCampaignContact";

			var data2 = Factory.NewWithValidTestData<StmData>();
			data2.SD_DepartmentGuid = Env.Instance.CurrentCompany.PK;
			data2.SD_Type = "";
			data2.SD_Owner = Campaign.PK;
			data2.SD_Name = "GlbCompanyCampaignContact_Inquiries";

			Factory.Save();

			var result = CampaignContactFilterDataSourceHelper.GetCampaignContactDataSource(campaignContactFilterStrip.LastUsedCampaignContactLayoutFactory);
			AssertEquals("Organization", result);

			data.SD_Type = "";
			data2.SD_Type = "CUR";

			Factory.Save();

			result = CampaignContactFilterDataSourceHelper.GetCampaignContactDataSource(campaignContactFilterStrip.LastUsedCampaignContactLayoutFactory);
			AssertEquals("Inquiry Manager", result);
		}

		#region Transitioning

		public void TestCampaignTransitioning_CustomFilter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			GlbCompanyCampaign master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "touch1";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "a";
			touch1.G0_G0_Master = master.PK;
			touch1.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			master.AllTouches.Add(touch1);

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_CampaignName = "touch2a";
			touch2A.G0_Type = "PREAP";
			touch2A.G0_Category = "PRINT";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2A.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2A.SourceCampaignPK = touch1.PK;
			touch2A.HtmlDocumentBlob = ZBlob.FromAscii("click me");
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "a";
			touch2A.G0_G0_Master = master.PK;

			var touch2B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2B.G0_CampaignName = "touch2b";
			touch2B.G0_Type = "PREAP";
			touch2B.G0_Category = "PRINT";
			touch2B.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2B.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2B.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2B.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2B.SourceCampaignPK = touch1.PK;
			touch2B.HtmlDocumentBlob = ZBlob.FromAscii("I'm here");
			touch2B.G0_HorizontalId = 2;
			touch2B.G0_VerticalId = "b";
			touch2B.G0_G0_Master = master.PK;

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "contact1@test.com";

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "contact2@test.com";

			var item2A = Factory.New<GlbCompanyCampaignItem>();
			item2A.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2A.G8_RecipientID = contact1.PK;
			item2A.G8_G0 = touch1.PK;

			var item2B = Factory.New<GlbCompanyCampaignItem>();
			item2B.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2B.G8_RecipientID = contact2.PK;
			item2B.G8_G0 = touch1.PK;

			master.AllTouches.Add(touch2A);
			master.AllTouches.Add(touch2B);
			var rule_2A = touch2A.TransitionRulesToThisCampaign[0];
			var rule_2B = touch2B.TransitionRulesToThisCampaign[0];

			touch2A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.FIX;
			touch2A.SendSettings.GSC_ScheduleTime = new ZDateTime(2017, 1, 1);

			touch2B.SendSettings.DaysOffset = 5;
			touch2B.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			touch2B.SendSettings.IsUseCurrentTime = true;

			touch2A.SourceCampaignPK = touch1.PK;
			touch2B.SourceCampaignPK = touch1.PK;

			var filterBizObj_2A = new GlbCompanyCampaignContactFilterBusinessObject(touch2A);
			filterBizObj_2A.QueryObjectType = typeof(CampaignContact);
			((ISetCampaignFilterLayoutContext)filterBizObj_2A).SetContext();
			var strip_2A = filterBizObj_2A.FilterStrips.AddNew("Custom SQL Filter");
			var filter_2A = (ModuleSQLFilter)strip_2A.CurrentModuleFilter;
			filter_2A.IsActive = true;
			filter_2A.Property1 = "VCC_Email <> 'contact2@test.com'";
			filterBizObj_2A.FillLayoutValues(rule_2A.FilterRule, ModuleIDs.DripMarketingFilterRule);

			Factory.Save();
			touch1.CampaignsItemsSent.Reload(true);
			touch1.TransitionAndSchedule();

			var newFactory = new BusinessObjectFactory();
			touch2A = newFactory.Load<GlbCompanyCampaign>(touch2A.PK);
			touch2B = newFactory.Load<GlbCompanyCampaign>(touch2B.PK);

			AssertEquals(1, touch2A.CampaignsItemsSent.Count);
			AssertEquals(1, touch2B.CampaignsItemsSent.Count);
			AssertEquals(contact1.PK, touch2A.CampaignsItemsSent[0].Recipient.PK);
			AssertEquals(contact2.PK, touch2B.CampaignsItemsSent[0].Recipient.PK);

			AssertEquals(new ZDateTime(2017, 1, 1).ToUniversalBranchTime(), touch2A.CampaignsItemsSent[0].ScheduleTimeUtc);
			AssertZDatesWithin5Minutes("", ZDateTime.UtcNow.AddHours(touch2B.SendSettings.GSC_HoursOffset), touch2B.CampaignsItemsSent[0].ScheduleTimeUtc);
		}

		public void TestCampaignTransitioning()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			GlbCompanyCampaign master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "touch1";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "a";
			touch1.G0_G0_Master = master.PK;
			touch1.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			master.AllTouches.Add(touch1);

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_CampaignName = "touch2a";
			touch2A.G0_Type = "PREAP";
			touch2A.G0_Category = "PRINT";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2A.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2A.SourceCampaignPK = touch1.PK;
			touch2A.HtmlDocumentBlob = ZBlob.FromAscii("click me");
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "a";
			touch2A.G0_G0_Master = master.PK;

			var touch2B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2B.G0_CampaignName = "touch2b";
			touch2B.G0_Type = "PREAP";
			touch2B.G0_Category = "PRINT";
			touch2B.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2B.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2B.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2B.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2B.SourceCampaignPK = touch1.PK;
			touch2B.HtmlDocumentBlob = ZBlob.FromAscii("I'm here");
			touch2B.G0_HorizontalId = 2;
			touch2B.G0_VerticalId = "b";
			touch2B.G0_G0_Master = master.PK;

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "contact1@test.com";

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "contact2@test.com";

			var item2A = Factory.New<GlbCompanyCampaignItem>();
			item2A.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2A.G8_RecipientID = contact1.PK;
			item2A.G8_G0 = touch1.PK;

			var item2B = Factory.New<GlbCompanyCampaignItem>();
			item2B.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2B.G8_RecipientID = contact2.PK;
			item2B.G8_G0 = touch1.PK;

			master.AllTouches.Add(touch2A);
			master.AllTouches.Add(touch2B);
			var rule_2A = touch2A.TransitionRulesToThisCampaign[0];
			var rule_2B = touch2B.TransitionRulesToThisCampaign[0];

			touch2A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.FIX;
			touch2A.SendSettings.GSC_ScheduleTime = new ZDateTime(2017, 1, 1);

			touch2B.SendSettings.DaysOffset = 5;
			touch2B.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			touch2B.SendSettings.IsUseCurrentTime = true;

			touch2A.SourceCampaignPK = touch1.PK;
			touch2B.SourceCampaignPK = touch1.PK;

			var filterBizObj_2A = new GlbCompanyCampaignContactFilterBusinessObject(touch2A);
			((ISetCampaignFilterLayoutContext)filterBizObj_2A).SetContext();
			var strip_2A = filterBizObj_2A.FilterStrips.AddNew("Email Address");
			var filter_2A = (ModuleTextFilter)strip_2A.CurrentModuleFilter;
			filter_2A.IsActive = true;
			filter_2A.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter_2A.Property = "contact1";
			filterBizObj_2A.FillLayoutValues(rule_2A.FilterRule, ModuleIDs.DripMarketingFilterRule);

			var filterBizObj_2B = new GlbCompanyCampaignContactFilterBusinessObject(touch2B);
			((ISetCampaignFilterLayoutContext)filterBizObj_2B).SetContext();
			var strip_2B = filterBizObj_2B.FilterStrips.AddNew("Email Address");
			var filter_2B = (ModuleTextFilter)strip_2B.CurrentModuleFilter;
			filter_2B.IsActive = true;
			filter_2B.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter_2B.Property = "contact2";
			filterBizObj_2B.FillLayoutValues(rule_2B.FilterRule, ModuleIDs.DripMarketingFilterRule);

			Factory.Save();
			touch1.CampaignsItemsSent.Reload(true);
			touch1.TransitionAndSchedule();

			var newFactory = new BusinessObjectFactory();
			touch2A = newFactory.Load<GlbCompanyCampaign>(touch2A.PK);
			touch2B = newFactory.Load<GlbCompanyCampaign>(touch2B.PK);

			AssertEquals(1, touch2A.CampaignsItemsSent.Count);
			AssertEquals(1, touch2B.CampaignsItemsSent.Count);
			AssertEquals(contact1.PK, touch2A.CampaignsItemsSent[0].Recipient.PK);
			AssertEquals(contact2.PK, touch2B.CampaignsItemsSent[0].Recipient.PK);

			AssertEquals(new ZDateTime(2017, 1, 1).ToUniversalBranchTime(), touch2A.CampaignsItemsSent[0].ScheduleTimeUtc);
			AssertZDatesWithin5Minutes("", ZDateTime.UtcNow.AddHours(touch2B.SendSettings.GSC_HoursOffset), touch2B.CampaignsItemsSent[0].ScheduleTimeUtc);

			//retransfer
			contact2.OC_Email = "contact12@test.com";
			touch1.CampaignsItemsSent[1].RecipientFromView.VCC_Email = "contact12@test.com";
			Factory.Save();
			touch1.TransitionAndSchedule();
			touch1.CampaignsItemsSent.Reload(true);

			newFactory = new BusinessObjectFactory();
			touch2A = newFactory.Load<GlbCompanyCampaign>(touch2A.PK);
			touch2B = newFactory.Load<GlbCompanyCampaign>(touch2B.PK);
			var recipientPks = touch2A.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
				.Select(item => item.Recipient.PK)
				.ToArray();

			AssertEquals(2, touch2A.CampaignsItemsSent.Count);
			AssertEquals(0, touch2B.CampaignsItemsSent.Count);
			AssertCollectionContains(contact1.PK, recipientPks);
			AssertCollectionContains(contact2.PK, recipientPks);

			AssertEquals(new ZDateTime(2017, 1, 1).ToUniversalBranchTime(), touch2A.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().First(x => x.Recipient.PK == contact1.PK).ScheduleTimeUtc);
			AssertZDatesWithin5Minutes("", ZDateTime.Empty, touch2A.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().First(x => x.Recipient.PK == contact2.PK).ScheduleTimeUtc);
		}

		#endregion

		public void TestSetContext()
		{
			var filterBizO = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var setContext = filterBizO as ISetCampaignFilterLayoutContext;

			AssertNotNull(setContext);
			AssertNotEquals(Campaign.PK, (filterBizO.LayoutsHelper as GlbCompanyCampaignFilterStripLayoutsHelper).BizObjPK);

			setContext.SetContext();
			AssertEquals(Campaign.PK, (filterBizO.LayoutsHelper as GlbCompanyCampaignFilterStripLayoutsHelper).BizObjPK);
		}

		public void TestSetContext_Group()
		{
			Campaign.G0_GCG_Group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>().PK;

			var filterBizO = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var setContext = filterBizO as ISetCampaignFilterLayoutContext;

			AssertNotNull(setContext);
			AssertNotEquals(Campaign.G0_GCG_Group, (filterBizO.LayoutsHelper as GlbCompanyCampaignFilterStripLayoutsHelper).BizObjPK);

			setContext.SetContext();
			AssertEquals(Campaign.G0_GCG_Group, (filterBizO.LayoutsHelper as GlbCompanyCampaignFilterStripLayoutsHelper).BizObjPK);
		}

		public void TestRelatedModuleFilter()
		{
			var filterBizO = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			var related = filterBizO as IRelatedModuleFilterBusinessObject;

			AssertNotNull(related);
		}

		public void TestSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions_CampaignTracking()
		{
			AssertSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions(ContactDataSourceList.Codes.CampaignTracking);
		}

		public void TestSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions_ClientIntelligence()
		{
			AssertSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions(ContactDataSourceList.Codes.ClientIntelligence);
		}

		public void TestSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions_Inquiries()
		{
			AssertSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions(ContactDataSourceList.Codes.Inquiries);
		}

		void AssertSavedLayouts_WhenUserDefinedFiltersValidatedInOtherModules_ShouldNotThrowExceptions(ZString contactDataSource)
		{
			Campaign.ContactDataSource = contactDataSource;
			var filterBizo = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			filterBizo.AddTextFilterStrip("Organization Name", "Squanchy");

			var layout = filterBizo.SaveLayout("Squanch");
			layout.S9_IsPublished = true;
			layout.Factory.Save();

			var moduleId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(x => x.Name == layout.S9_ModuleID);
			AssertNull("The StmModuleFilters saved by this filter bizo have invalid values in the ModuleID column. We have taken steps to avoid exceptions being thrown as a result. This unit test is one of them. If you're trying to change this to use real moduleIDs, then by all means change this test.", moduleId);

			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy))
			{
				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject);
				saveLayout.LayoutName = "Name";
				AssertNoExceptionThrown("Validating the user defined filter should not throw an exception just because there is an StmModuleFilter (with an invalid ModuleID) in the database that has a filter matching the layout's name. SAD!", () => saveLayout.IsUserDefinedFilter = true);
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}
		}

		public void TestValueAnalysisModuleFilters()
		{
			var filterBizObj = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};
			foreach (var module in modules)
			{
				var filter = filterBizObj[module.Description];
				AssertNotNull(module.Description, filter);
				AssertEquals("ValueAnalysisModuleFilter", filter.GetType().Name);
				AssertEquals("Value Analysis", (string)filter.Category.Description);
			}
		}

		public void TestUtcOffset()
		{
			void AssertUtcOffsetCollection(GlbCampaignContactCollection contacts, short from, short to)
			{
				var max = contacts.Select(x => x.VCC_OffsetMinutesFromUtc).Max();
				var min = contacts.Select(x => x.VCC_OffsetMinutesFromUtc).Min();

				AssertLessThanOrEqualTo<short>(max, to);
				AssertGreaterThanOrEqualTo<short>(min, from);
			}

			var filter = (UtcOffsetFilter)CampaignFilter["UTC Offset"];
			AssertNotNull(filter);
			AssertEquals(FilterCategories.Dates, filter.Category);

			var collection = new GlbCampaignContactCollection(Campaign);
			filter.UtcOffsetFrom = "-300";
			filter.UtcOffsetTo = "+300";
			collection.Load(filter.Query);
			AssertUtcOffsetCollection(collection, -300, 300);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SwitchOffSubscriptionFilter(CampaignFilter);
		}

		#endregion
	}
}
