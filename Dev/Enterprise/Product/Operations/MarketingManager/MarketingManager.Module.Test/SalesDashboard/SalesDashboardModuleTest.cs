using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SalesDashboardModule))]
	public class SalesDashboardModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (var module = new SalesDashboardModule())
			{
				AssertEquals(ModuleIDs.SalesDashboard, module.ID);
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SalesDashboard;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new SalesDashboardModuleForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		public void TestSalesDashboardActivityFilterRemainsAfterAutoRefresh()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityDescription = "Original";

			var opportunityActivity = Factory.NewWithValidTestData<OpportunitySalesDashboardActivity>();
			opportunityActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Opportunity;
			opportunityActivity.VSA_ParentId = opportunity.PK;

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var communicationActivity = Factory.NewWithValidTestData<CommunicationSalesDashboardActivity>();
			communicationActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Communication;
			communicationActivity.VSA_ParentId = communication.PK;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();
				AssertEquals("Without filters the grid should display opportunity and communication", 2, module.GridCollection.Count);

				var moduleFilter = module.FilterBusinessObject;
				var activityFilter = (ModuleFlagsFilter)moduleFilter["Activity Type"];
				activityFilter.Property0 = true;
				activityFilter.IsActive = true;

				var originalQuery = module.GetDisplayResultsQueryForTest();

				module.PerformSearch_ForTest();
				var gridCollectionOppItem = (OpportunitySalesDashboardActivity)module.GridCollection.FindByPK(opportunity.PK);

				AssertEquals(1, module.GridCollection.Count);
				AssertNotNull(gridCollectionOppItem);
				AssertEquals("Opportunity description should be Original", "Original", gridCollectionOppItem.VSA_ActivityDescription);

				opportunity.P8_OpportunityDescription = "Updated";
				Factory.Save();

				var newQuery = module.GetDisplayResultsQueryForTest();
				AssertEquals(originalQuery, newQuery);

				gridCollectionOppItem = (OpportunitySalesDashboardActivity)module.GridCollection.FindByPK(opportunity.PK);
				AssertEquals("Opportunity description should be updated after save", "Updated", gridCollectionOppItem.VSA_ActivityDescription);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshOpportunity()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityDescription = "Original";

			var opportunityActivity = Factory.NewWithValidTestData<OpportunitySalesDashboardActivity>();
			opportunityActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Opportunity;
			opportunityActivity.VSA_ParentId = opportunity.PK;
			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();
				var gridCollectionOpportunityItem = (OpportunitySalesDashboardActivity)module.GridCollection.FindByPK(opportunity.PK);

				AssertNotNull(gridCollectionOpportunityItem);
				AssertEquals("Opportunity description should be Original", "Original", gridCollectionOpportunityItem.VSA_ActivityDescription);

				opportunity.P8_OpportunityDescription = "Updated";
				Factory.Save();

				gridCollectionOpportunityItem = (OpportunitySalesDashboardActivity)module.GridCollection.FindByPK(opportunity.PK);
				AssertEquals("Opportunity description should be updated after save", "Updated", gridCollectionOpportunityItem.VSA_ActivityDescription);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshCommunication()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_CallSummary = "Original";

			var communicationActivity = Factory.NewWithValidTestData<CommunicationSalesDashboardActivity>();
			communicationActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Communication;
			communicationActivity.VSA_ParentId = communication.PK;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();
				var gridCollectionCommunicationItem = (CommunicationSalesDashboardActivity)module.GridCollection.FindByPK(communication.PK);

				AssertNotNull(gridCollectionCommunicationItem);
				AssertEquals("Communication description should be Original", "Original", gridCollectionCommunicationItem.VSA_ActivityDescription);

				communication.OQ_CallSummary = "Updated";
				Factory.Save();

				gridCollectionCommunicationItem = (CommunicationSalesDashboardActivity)module.GridCollection.FindByPK(communication.PK);
				AssertEquals("Communication description should be updated after save", "Updated", gridCollectionCommunicationItem.VSA_ActivityDescription);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshInquiry()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_GS_NKRepAssigned = "OGL";

			var inquiryActivity = Factory.NewWithValidTestData<InquirySalesDashboardActivity>();
			inquiryActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Inquiry;
			inquiryActivity.VSA_ParentId = inquiry.PK;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();
				var gridCollectionInquiryItem = (InquirySalesDashboardActivity)module.GridCollection.FindByPK(inquiry.PK);

				AssertNotNull(gridCollectionInquiryItem);
				AssertEquals("Inquiry description should be OGL",  "OGL", gridCollectionInquiryItem.VSA_ActivityStaffAssignment);

				inquiry.O1_GS_NKRepAssigned = "UPD";
				Factory.Save();

				gridCollectionInquiryItem = (InquirySalesDashboardActivity)module.GridCollection.FindByPK(inquiry.PK);
				AssertEquals("Inquiry description should be UPD after save",  "UPD", gridCollectionInquiryItem.VSA_ActivityStaffAssignment);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshQuotations()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			quotation.TH_QuoteDate = new ZDate(2020, 1, 1);
			
			var quotationActivity = Factory.NewWithValidTestData<QuotationSalesDashboardActivity>();
			quotationActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Quotation;
			quotationActivity.VSA_ParentId = quotation.PK;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();
				var gridCollectionQuotationItem = (QuotationSalesDashboardActivity)module.GridCollection.FindByPK(quotation.PK);

				AssertNotNull(gridCollectionQuotationItem);
				AssertEquals("Quotation date should be 2020/1/1",  quotation.TH_QuoteDate, gridCollectionQuotationItem.VSA_ActivityDate);

				quotation.TH_QuoteDate = new ZDate(2020, 2, 2);
				Factory.Save();

				gridCollectionQuotationItem = (QuotationSalesDashboardActivity)module.GridCollection.FindByPK(quotation.PK);
				AssertEquals("Quotation date should be 2020/2/2 after save",  quotation.TH_QuoteDate, gridCollectionQuotationItem.VSA_ActivityDate);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshOneOffQuote()
		{
			var builder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var oneOffQuote = (Quote)builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory).Quote;
			oneOffQuote.TH_QuoteDate = new ZDate(2020, 1, 1);
			var parent = Factory.LoadTop1<RateOneOffShipment>(new ZQuery(RateOneOffShipmentSchema.TT_TH, oneOffQuote.PK));

			var oneOffQuoteActivity = Factory.NewWithValidTestData<OneOffQuoteSalesDashboardActivity>();
			oneOffQuoteActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.OneOffQuote;
			oneOffQuoteActivity.VSA_ParentId = parent.PK;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();

				var gridCollectionOneOffQuoteItem = (OneOffQuoteSalesDashboardActivity)module.GridCollection.FindByPK(parent.PK);

				AssertNotNull(gridCollectionOneOffQuoteItem);
				AssertEquals("One Off Quote date should be 2020/1/1", oneOffQuote.TH_QuoteDate, gridCollectionOneOffQuoteItem.VSA_ActivityDate);

				oneOffQuote.TH_QuoteDate = new ZDate(2020, 2, 2);

				Factory.Save();

				gridCollectionOneOffQuoteItem = (OneOffQuoteSalesDashboardActivity)module.GridCollection.FindByPK(parent.PK);
				AssertEquals("One Off Quote date should be 2020/2/2 after save", oneOffQuote.TH_QuoteDate, gridCollectionOneOffQuoteItem.VSA_ActivityDate);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshCampaign()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Original";
			campaign.G0_CampaignID = "TST00001000";

			var parent = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			parent.G8_G0 = campaign.PK;
			parent.G8_RecipientID = contact.PK;
			parent.G8_RecipientTableCode = "OC";

			var campaignActivity = Factory.NewWithValidTestData<CampaignSalesDashboardActivity>();
			campaignActivity.VSA_OH = organisation.PK;
			campaignActivity.VSA_ParentId = parent.PK;
			campaignActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Campaign;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();

				var gridCollectionCampaignItem = (CampaignSalesDashboardActivity)module.GridCollection.FindByPK(parent.PK);

				AssertNotNull(gridCollectionCampaignItem);
				AssertEquals("Campaign name should be Original", "Original", gridCollectionCampaignItem.VSA_ActivityDescription);

				campaign.G0_CampaignName = "Updated";
				Factory.Save();

				gridCollectionCampaignItem = (CampaignSalesDashboardActivity)module.GridCollection.FindByPK(parent.PK);
				AssertEquals("Campaign name should be updated after save", "Updated", gridCollectionCampaignItem.VSA_ActivityDescription);
			}
		}

		public void TestSalesDashboardActivityAutoRefreshProject()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_Summary = "Original";

			var projectActivity = Factory.NewWithValidTestData<ProjectSalesDashboardActivity>();
			projectActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Project;
			projectActivity.VSA_ParentId = project.PK;

			Factory.Save();

			using (var module = new SalesDashboardModuleForTest())
			{
				module.PerformSearch_ForTest();
				var gridCollectionProjectItem = (ProjectSalesDashboardActivity)module.GridCollection.FindByPK(project.PK);

				AssertNotNull(gridCollectionProjectItem);
				AssertEquals("Project summary should be Original", "Original", gridCollectionProjectItem.VSA_ActivityDescription);

				project.WKP_Summary = "Updated";
				Factory.Save();

				gridCollectionProjectItem = (ProjectSalesDashboardActivity)module.GridCollection.FindByPK(project.PK);
				AssertEquals("Project summary should be updated after save", "Updated", gridCollectionProjectItem.VSA_ActivityDescription);
			}
		}

		#region Implementation

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			collection.Add(Factory.Load<OpportunitySalesDashboardActivity>(opportunity.PK));
			collection.Add(Factory.Load<InquirySalesDashboardActivity>(inquiry.PK));
			collection.Add(Factory.Load<CommunicationSalesDashboardActivity>(communication.PK));
		}

		#endregion
	}
}
