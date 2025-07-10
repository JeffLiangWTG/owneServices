using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SalesDashboardFilterBusinessObject))]
	public class SalesDashboardFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestAllPossibleCodesInSalesDashboardActivityTypeListHaveSupportedWorkflowTypes()
		{
			var existingMappings = new Dictionary<string, string>();
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.Campaign, WorkflowDescriptors.CampaignWorkflowDescriptorCode);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.Communication, WorkflowDescriptors.CommunicationWorkflowDescriptorCode);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.EDocsUpdate, null);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.Inquiry, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.OneOffQuote, WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.Opportunity, WorkflowDescriptors.OpportunityWorkflowDescriptorCode);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.Project, SalesDashboardFilterBusinessObject.ProjectWorkflowType);
			existingMappings.Add(SalesDashboardActivityTypeCodeList.Codes.Quotation, WorkflowDescriptors.QuotationWorkflowDescriptorCode);
			var codesWithNoMapping = new List<string>() { SalesDashboardActivityTypeCodeList.Codes.EDocsUpdate };

			var salesDashboardFilter = new SalesDashboardFilterBusinessObject();
			var codeList = new SalesDashboardActivityTypeCodeList();
			AssertEquals("SalesDashboardActivityTypeCodeList contains all codes which are known for mapping with SupportedWorkflowTypes", codeList.Count, existingMappings.Keys.Count);
			AssertEquals("SalesDashboardFilterBusinessObject contains all SupportedWorkflowTypes with known existing mappings",
				existingMappings.Count - codesWithNoMapping.Count, salesDashboardFilter.OverriddenSupportedWorkflowTypes.Count);

			foreach (var code in codeList.GetAllCodes())
			{
				if (!codesWithNoMapping.Contains(code))
				{
					Assert(@"The reason to have this unit test is to ensure if SalesDashboardActivityTypeCodeList will change, the addition/modification/deletion of code should have corresponding mapping to be added/modified/deleted 
										in the SalesDashboardFilterBusinessObject.SupportedWorkflowTypes list.

										We need to meaningfully map Sales Dashboard Activity Type to the Workflow Type, because no one to one corresponding mapping in the system between these lists.

								For example: 
									SalesDashboardActivityTypeCodeList.Codes.Campaign maps to Workflow Type 'CAM'
									SalesDashboardActivityTypeCodeList.Codes.Project maps to Workflow Type 'WKP' etc.",

					existingMappings.ContainsKey(code) && salesDashboardFilter.OverriddenSupportedWorkflowTypes.Contains(existingMappings[code]));
				}
			}
		}

		public void TestFilters_ShouldNotExistWithoutBMSForUnsupportedWorkflowTypes()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			var salesDashboard = new SalesDashboardFilterBusinessObject();
			var filterList = salesDashboard.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available when not even a single supported workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available when not even a single supported workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));

			testHelper.CreateSystem(Factory, new string[] { "ORG" });
			Factory.Save();
			salesDashboard = new SalesDashboardFilterBusinessObject();
			filterList = salesDashboard.ModuleFilters.Filter_List;
			Assert("PAVE-related filters should NOT be available when not even a single supported workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
			Assert("PAVE-related filters should NOT be available when not even a single supported workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));
		}

		public void TestFilters_ShouldExistWithBMSForSupportedWorkflowTypes()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();

			var overriddenSupportedWorkflowTypes = new SalesDashboardFilterBusinessObject().OverriddenSupportedWorkflowTypes;

			foreach (var overriddenSupportedWorkflowType in overriddenSupportedWorkflowTypes)
			{
				var salesDashboard = new SalesDashboardFilterBusinessObject();
				var filterList = salesDashboard.ModuleFilters.Filter_List;
				Assert("PAVE-related filters should NOT be available when not even a single supported workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Definition Code")));
				Assert("PAVE-related filters should NOT be available when not even a single supported workflow type is not registered in a buffer management system.", !(filterList.ContainsCode("Tag Magnitude")));

				IBMSystem system = testHelper.CreateSystem(Factory, overriddenSupportedWorkflowType);
				Factory.Save();
				salesDashboard = new SalesDashboardFilterBusinessObject();
				filterList = salesDashboard.ModuleFilters.Filter_List;
				Assert("PAVE-related filters should be available when even a single supported workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Definition Code")));
				Assert("PAVE-related filters should be available when even a single supported workflow type is registered in a buffer management system.", (filterList.ContainsCode("Tag Magnitude")));
				system.Delete();
				Factory.Save();
			}
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestActivityDate()
		{
			var activity1 = Factory.New<SalesDashboardActivity>();
			activity1.VSA_ActivityDateLocal = new ZDateTime(2013, 10, 4);
			var activity2 = Factory.New<SalesDashboardActivity>();
			activity2.VSA_ActivityDateLocal = new ZDateTime(2013, 11, 2);

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var activityDateFilter = (ModuleDateFilter)dashboardFilter["Activity Date"];
			activityDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertNotNull(activityDateFilter);
			AssertEquals("Activity Date", activityDateFilter.MultilingualDescription);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert(activityCollection.Contains(activity1));
			Assert(activityCollection.Contains(activity2));

			activityDateFilter.Property1 = new ZDateTime(2013, 11, 1, 20, 0, 0);
			activityDateFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert(!activityCollection.Contains(activity1));
			Assert(activityCollection.Contains(activity2));

			activityDateFilter.Property1 = ZDateTime.Empty;
			activityDateFilter.Property2 = new ZDateTime(2013, 10, 4, 1, 0, 0);
			activityDateFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert(activityCollection.Contains(activity1));
			Assert(!activityCollection.Contains(activity2));
		}

		public void TestOverallActivityDispostionFilter()
		{
			var overallActivityDispositionCollection1 = new OpportunityStatusCollection();
			overallActivityDispositionCollection1.Add("CRT", (NoResString)"Current", false);
			overallActivityDispositionCollection1.Add("OPN", (NoResString)"Open", false);
			overallActivityDispositionCollection1.Add("CLS", (NoResString)"Closed", true);
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overallActivityDispositionCollection1);

			var overallActivityDispositionCollection2 = new CommunicationStatusCollection();
			overallActivityDispositionCollection2.Add("CRT", (NoResString)"Current", false, true);
			overallActivityDispositionCollection2.Add("OPN", (NoResString)"Open", false, true);
			overallActivityDispositionCollection2.Add("CLS", (NoResString)"Closed", true, true);
			OrganisationsDataRegistry.Instance.CommunicationStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overallActivityDispositionCollection2);

			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			OrgOpportunity opportunity1 = organisation1.SalesOpportunities.AddNew();
			OrgOpportunity opportunity2 = organisation2.SalesOpportunities.AddNew();
			OrgOpportunity opportunity3 = organisation3.SalesOpportunities.AddNew();
			opportunity1.P8_Status = "CRT";
			opportunity2.P8_Status = "CLS";
			opportunity3.P8_Status = "CLS";

			ProcessTask opportunityTask1 = opportunity1.WorkflowItems.AddNew();
			ProcessTask opportunityTask2 = opportunity2.WorkflowItems.AddNew();
			ProcessTask opportunityTask3 = opportunity3.WorkflowItems.AddNew();
			opportunityTask1.P9_Status = "WRK";
			opportunityTask2.P9_Status = "WRK";
			opportunityTask3.P9_Status = "WRK";

			SalesEnquiry inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			SalesEnquiry inquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry1.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			inquiry2.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			inquiry3.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;

			ProcessTask inquiryTask1 = inquiry1.WorkflowItems.AddNew();
			ProcessTask inquiryTask2 = inquiry2.WorkflowItems.AddNew();
			ProcessTask inquiryTask3 = inquiry3.WorkflowItems.AddNew();
			inquiryTask1.P9_Status = "WRK";
			inquiryTask2.P9_Status = "WRK";
			inquiryTask3.P9_Status = "WRK";

			OrgSalesCall salesCall1 = organisation1.SalesCalls.AddNew();
			OrgSalesCall salesCall2 = organisation2.SalesCalls.AddNew();
			OrgSalesCall salesCall3 = organisation3.SalesCalls.AddNew();
			salesCall1.OQ_Status = "CLS";
			salesCall2.OQ_Status = "OPN";
			salesCall3.OQ_Status = "OPN";

			var campaignItem1 = CreateCampaignItem(false);
			var campaignItem2 = CreateCampaignItem(false);
			var campaignItem3 = CreateCampaignItem(false);
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			var campaignItem11 = CreateCampaignItem(true);
			var campaignItem21 = CreateCampaignItem(true);
			var campaignItem31 = CreateCampaignItem(true);
			campaignItem11.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem21.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			campaignItem31.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			var oneOffQuote1 = CreateOneOffQuote();
			var oneOffQuote2 = CreateOneOffQuote();
			var oneOffQuote3 = CreateOneOffQuote();
			oneOffQuote2.ParentQuote.TH_IsOneOffQuoteConsumed = true;

			var quotation1 = Factory.NewWithValidTestData<Quote>();
			var quotation2 = Factory.NewWithValidTestData<Quote>();
			var quotation3 = Factory.NewWithValidTestData<Quote>();
			quotation1.TH_IsCancelled = true;
			quotation2.TH_IsCancelled = false;
			quotation3.TH_IsCancelled = false;

			var project1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			var project2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			var project3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			project1.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Assigned);
			project2.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Working);
			project3.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var overallActivityDispositionFilter = (ModuleTextFilter)dashboardFilter["Overall Activity Disposition"];
			AssertNotNull(overallActivityDispositionFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			var open = new List<BusinessObject> { opportunity1, inquiry1, salesCall2, salesCall3, campaignItem1, campaignItem11, oneOffQuote1, oneOffQuote3, quotation2, quotation3, project1, project2 };
			var closed = new List<BusinessObject> { salesCall1, opportunity2, inquiry2, opportunity3, inquiry3, campaignItem2, campaignItem3, campaignItem21, campaignItem31, oneOffQuote2, quotation1, project3 };

			overallActivityDispositionFilter.Property = "OPN";
			overallActivityDispositionFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);

			AssertActivityCollectionItems("Disposition 'Open'", activityCollection, open, closed);

			overallActivityDispositionFilter.Property = "CLS";
			activityCollection.Load(dashboardFilter.Filter);

			AssertActivityCollectionItems("Disposition 'Closed'", activityCollection, closed, open);
		}

		public void TestCreateCallAndOpportunityForASingleContact()
		{
			var overallActivityDispositionCollection1 = new OpportunityStatusCollection();
			overallActivityDispositionCollection1.Add("CRT", (NoResString)"Current", false);
			overallActivityDispositionCollection1.Add("OPN", (NoResString)"Open", false);
			overallActivityDispositionCollection1.Add("CLS", (NoResString)"Closed", true);
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overallActivityDispositionCollection1);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact";
			contact.OC_Email = "email@contact.com";

			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_Status = "CLS";
			opportunity.P8_OC = contact.PK;

			ProcessTask opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";

			SalesEnquiry inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;

			ProcessTask inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "WRK";

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_Status = "CLS";

			var register = Factory.NewWithValidTestData<OrgColdCallRegister>();
			register.O1_OC_LinkedContact = contact.PK;

			var pivot1 = Factory.New<RelatedActivityPivot>();
			pivot1.RAP_ChildActivityID = salesCall.PK;
			pivot1.RAP_ChildActivityTableCode = "OQ";
			pivot1.RAP_ParentActivityID = register.PK;
			pivot1.RAP_ParentActivityTableCode = "O1";

			var pivot2 = Factory.New<RelatedActivityPivot>();
			pivot2.RAP_ChildActivityID = salesCall.PK;
			pivot2.RAP_ChildActivityTableCode = "OQ";
			pivot2.RAP_ParentActivityID = opportunity.PK;
			pivot2.RAP_ParentActivityTableCode = "P8";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var overallActivityDispositionFilter = (ModuleTextFilter)dashboardFilter["Overall Activity Disposition"];
			var activityTypeFilter = (ModuleFlagsFilter)dashboardFilter["Activity Type"];

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			overallActivityDispositionFilter.Property = "OPN";
			overallActivityDispositionFilter.IsActive = true;
			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = true;
			activityTypeFilter.Property2 = true;

			AssertNoExceptionThrown(delegate
			{ activityCollection.Load(dashboardFilter.Filter); });
		}

		public void TestActivityParentIDFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_OpportunityID = "O00001000";
			inquiry.O1_LeadUniqueReference = "I00001000";
			salesCall.OQ_CommunicationID = "CMM00001000";
			campaignItem.CompanyCampaign.G0_CampaignID = "CAM001000";
			oneOffQuote.ParentQuote.TH_QuoteNumber = "QBK001000";
			quotation.TH_QuoteNumber = "QTE001000";
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_ProjectNumber, "PRJ001000");

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var activityParentIdFilter = (ModuleTextFilter)dashboardFilter["Activity Parent ID"];
			AssertNotNull(activityParentIdFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItemsContainsAllTestObjects(activityCollection);

			activityParentIdFilter.Property = "O00001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("O00001000", activityCollection, new List<BusinessObject> { opportunity }, new List<BusinessObject> { inquiry, salesCall, campaignItem, oneOffQuote, quotation, project });

			activityParentIdFilter.Property = "I00001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("I00001000", activityCollection, new List<BusinessObject> { inquiry }, new List<BusinessObject> { opportunity, salesCall, campaignItem, oneOffQuote, quotation, project });

			activityParentIdFilter.Property = "CMM00001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("CMM00001000", activityCollection, new List<BusinessObject> { salesCall }, new List<BusinessObject> { opportunity, inquiry, campaignItem, oneOffQuote, quotation, project });

			activityParentIdFilter.Property = "CAM001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("CAM001000", activityCollection, new List<BusinessObject> { campaignItem }, new List<BusinessObject> { opportunity, inquiry, salesCall, oneOffQuote, quotation, project });

			activityParentIdFilter.Property = "QBK001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("QBK001000", activityCollection, new List<BusinessObject> { oneOffQuote }, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, quotation, project });

			activityParentIdFilter.Property = "QTE001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("QTE001000", activityCollection, new List<BusinessObject> { quotation }, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, oneOffQuote, project });

			activityParentIdFilter.Property = "PRJ001000";
			activityParentIdFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("PRJ001000", activityCollection, new List<BusinessObject> { project }, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, oneOffQuote, quotation });
		}

		public void TestActivityTypeFilter()
		{
			CreateSalesActivitiesForTest();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var activityTypeFilter = (ModuleFlagsFilter)dashboardFilter["Activity Type"];
			AssertNotNull(activityTypeFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItemsContainsAllTestObjects(activityCollection);

			activityTypeFilter.Property0 = true;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("opportunity", activityCollection, new List<BusinessObject> { opportunity }, new List<BusinessObject> { inquiry, salesCall, campaignItem, oneOffQuote, quotation, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = true;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("inquiry", activityCollection, new List<BusinessObject> { inquiry }, new List<BusinessObject> { opportunity, salesCall, campaignItem, oneOffQuote, quotation, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = true;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("salesCall", activityCollection, new List<BusinessObject> { salesCall }, new List<BusinessObject> { opportunity, inquiry, campaignItem, oneOffQuote, quotation, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = true;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("campaignItem", activityCollection, new List<BusinessObject> { campaignItem }, new List<BusinessObject> { opportunity, inquiry, salesCall, oneOffQuote, quotation, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = true;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("oneOffQuote", activityCollection, new List<BusinessObject> { oneOffQuote }, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, quotation, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = true;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("quotation", activityCollection, new List<BusinessObject> { quotation }, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, oneOffQuote, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = true;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("project", activityCollection, new List<BusinessObject> { project }, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, oneOffQuote, quotation });

			activityTypeFilter.Property0 = true;
			activityTypeFilter.Property1 = true;
			activityTypeFilter.Property2 = true;
			activityTypeFilter.Property3 = false;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = false;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("opportunity, inquiry, salesCall", activityCollection, new List<BusinessObject> { opportunity, inquiry, salesCall }, new List<BusinessObject> { campaignItem, oneOffQuote, quotation, project });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = false;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = true;
			activityTypeFilter.Property4 = true;
			activityTypeFilter.Property5 = true;
			activityTypeFilter.Property6 = true;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("campaignItem, oneOffQuote, quotation, project", activityCollection, new List<BusinessObject> { campaignItem, oneOffQuote, quotation, project }, new List<BusinessObject> { opportunity, inquiry, salesCall });

			activityTypeFilter.Property0 = false;
			activityTypeFilter.Property1 = true;
			activityTypeFilter.Property2 = false;
			activityTypeFilter.Property3 = true;
			activityTypeFilter.Property4 = false;
			activityTypeFilter.Property5 = true;
			activityTypeFilter.Property6 = false;
			activityTypeFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("inquiry, campaignItem, quotation", activityCollection, new List<BusinessObject> { inquiry, campaignItem, quotation }, new List<BusinessObject> { opportunity, salesCall, oneOffQuote, project });
		}

		public void TestOpportunityStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";

			var opportunityStatusCollection = new OpportunityStatusCollection();
			opportunityStatusCollection.AddNew().Code = "CRT";
			opportunityStatusCollection.AddNew().Code = "LOS";
			opportunityStatusCollection.AddNew().Code = "WON";
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, opportunityStatusCollection);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var opportunityStatusFilter = (ModuleTextFilter)dashboardFilter["Opportunity Status"];
			AssertNotNull(opportunityStatusFilter);
			Assert("Should contain all OpportunityStatus values", opportunityStatusFilter.List.Cast<CodeDescriptionBool>().Any(x => x.Code == "CRT"));
			Assert("Should contain all OpportunityStatus values", opportunityStatusFilter.List.Cast<CodeDescriptionBool>().Any(x => x.Code == "LOS"));
			Assert("Should contain all OpportunityStatus values", opportunityStatusFilter.List.Cast<CodeDescriptionBool>().Any(x => x.Code == "WON"));
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", opportunityStatusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			opportunityStatusFilter.Property = "CRT";
			opportunityStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with status of 'CRT'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with status of 'CRT'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with status of 'CRT'", !activityCollection.Contains(salesCall.PK));

			opportunityStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			opportunityStatusFilter.Property = "OPN";
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain activities with status not 'LOS'", activityCollection.Contains(opportunity.PK));
			Assert("Should contain activities with status not 'LOS'", !activityCollection.Contains(inquiry.PK));
			Assert("Should contain activities with status not 'LOS'", !activityCollection.Contains(salesCall.PK));

			opportunityStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with status of 'LOS'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with status of 'LOS'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with status of 'LOS'", !activityCollection.Contains(salesCall.PK));
		}

		public void TestOpportunityStagesFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			opportunity.P8_Stage = "OPN";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";

			var opportunityStageCollection = new CodeDescriptionBoolCollection(OrgOpportunitySchema.P8_Stage.MaxLength);
			opportunityStageCollection.AddNew().Code = "OPN";
			opportunityStageCollection.AddNew().Code = "CON";
			opportunityStageCollection.AddNew().Code = "QUO";
			OrganisationsDataRegistry.Instance.OpportunityStages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, opportunityStageCollection);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var opportunityStageFilter = (ModuleTextFilter)dashboardFilter["Opportunity Stage"];
			AssertNotNull(opportunityStageFilter);
			Assert("Should contain all OpportunityStages values", opportunityStageFilter.List.Cast<CodeDescriptionBool>().Any(x => x.Code == "OPN"));
			Assert("Should contain all OpportunityStages values", opportunityStageFilter.List.Cast<CodeDescriptionBool>().Any(x => x.Code == "CON"));
			Assert("Should contain all OpportunityStages values", opportunityStageFilter.List.Cast<CodeDescriptionBool>().Any(x => x.Code == "QUO"));
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", opportunityStageFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			opportunityStageFilter.Property = "OPN";
			opportunityStageFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain opportunities with stage of 'OPN'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain opportunities with stage of 'OPN'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain opportunities with stage of 'OPN'", !activityCollection.Contains(salesCall.PK));

			opportunityStageFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			opportunityStageFilter.Property = "CON";
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain opportunities with stage not 'CON'", activityCollection.Contains(opportunity.PK));
			Assert("Should contain opportunities with stage not 'CON'", !activityCollection.Contains(inquiry.PK));
			Assert("Should contain opportunities with stage not 'CON'", !activityCollection.Contains(salesCall.PK));

			opportunityStageFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain opportunities with stage of 'CON'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain opportunities with stage of 'CON'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain opportunities with stage of 'CON'", !activityCollection.Contains(salesCall.PK));
		}

		public void TestInquiryStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var inquiryStatusFilter = (ModuleTextFilter)dashboardFilter["Inquiry Status"];
			AssertNotNull(inquiryStatusFilter);
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", inquiryStatusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			inquiryStatusFilter.Property = "CNV";
			inquiryStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with status of 'CNV'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with status of 'CNV'", activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with status of 'CNV'", !activityCollection.Contains(salesCall.PK));

			inquiryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			inquiryStatusFilter.Property = "CLS";
			inquiryStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain activities with status not 'CLS'", !activityCollection.Contains(opportunity.PK));
			Assert("Should contain activities with status not 'CLS'", activityCollection.Contains(inquiry.PK));
			Assert("Should contain activities with status not 'CLS'", !activityCollection.Contains(salesCall.PK));

			inquiryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			inquiryStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with status of 'CLS'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with status of 'CLS'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with status of 'CLS'", !activityCollection.Contains(salesCall.PK));
		}

		public void TestCommunicationStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var salesCallsStatusFilter = (ModuleTextFilter)dashboardFilter["Communication Status"];
			AssertNotNull(salesCallsStatusFilter);
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", salesCallsStatusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			salesCallsStatusFilter.Property = "HOT";
			salesCallsStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with status of 'HOT'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with status of 'HOT'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with status of 'HOT'", activityCollection.Contains(salesCall.PK));

			salesCallsStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			salesCallsStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain activities with status not 'HOT'", !activityCollection.Contains(opportunity.PK));
			Assert("Should contain activities with status not 'HOT'", !activityCollection.Contains(inquiry.PK));
			Assert("Should contain activities with status not 'HOT'", !activityCollection.Contains(salesCall.PK));

			salesCallsStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			salesCallsStatusFilter.Property = "CLD";
			salesCallsStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with status of 'CLD'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with status of 'CLD'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with status of 'CLD'", !activityCollection.Contains(salesCall.PK));
		}

		public void TestCampaignStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			quotation.TH_IsCancelled = true;
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)dashboardFilter["Campaign Status"];
			AssertNotNull(statusFilter);
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", statusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItemsContainsAllTestObjects(activityCollection);

			statusFilter.Property = TrackingStatusCodes.Codes.VER;
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("VER", activityCollection, new List<BusinessObject> { campaignItem }, new List<BusinessObject> { inquiry, opportunity, salesCall, oneOffQuote, quotation, project });

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			statusFilter.Property = "HOT";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);
		}

		public void TestOneOffQuoteStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			quotation.TH_IsCancelled = true;
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)dashboardFilter["One Off Quote Status"];
			AssertNotNull(statusFilter);
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", statusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItemsContainsAllTestObjects(activityCollection);

			statusFilter.Property = "BKD";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("BKD", activityCollection, new List<BusinessObject> { oneOffQuote }, new List<BusinessObject> { inquiry, opportunity, salesCall, campaignItem, quotation, project });

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			statusFilter.Property = "HOT";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);
		}

		public void TestQuotationStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			quotation.TH_IsCancelled = true;
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)dashboardFilter["Quotation Status"];
			AssertNotNull(statusFilter);
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", statusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItemsContainsAllTestObjects(activityCollection);

			statusFilter.Property = "CAN";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("CAN", activityCollection, new List<BusinessObject> { quotation }, new List<BusinessObject> { inquiry, opportunity, salesCall, campaignItem, oneOffQuote, project });

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			statusFilter.Property = "HOT";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);
		}

		public void TestProjectStatusFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_Status = "CRT";
			inquiry.O1_LeadStatus = "CNV";
			salesCall.OQ_Status = "HOT";
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			quotation.TH_IsCancelled = true;
			project.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_Status, ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)dashboardFilter["Project Status"];
			AssertNotNull(statusFilter);
			Assert("Should only contain 'Exact' and 'Not Equal' comparison operator.", statusFilter.ComparisonOperator_List.ContainsOnly(ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual));

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItemsContainsAllTestObjects(activityCollection);

			statusFilter.Property = "CAN";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertActivityCollectionItems("CAN", activityCollection, new List<BusinessObject> { project }, new List<BusinessObject> { inquiry, opportunity, salesCall, campaignItem, oneOffQuote, quotation });

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			statusFilter.Property = "HOT";
			statusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals("Should be empty", 0, activityCollection.Count);
		}

		public void TestOrganizationFilter()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation1.SalesOpportunities.AddNew();
			var opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "WRK";
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = organisation2.SalesCalls.AddNew();

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var organisationFilter = (ModuleGuidFilter)dashboardFilter["Organization"];
			AssertNotNull(organisationFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			organisationFilter.Property = organisation1.PK;
			organisationFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with organisation of 'organisation1'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with organisation of 'organisation1'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with organisation of 'organisation1'", !activityCollection.Contains(salesCall.PK));

			organisationFilter.Property = organisation2.PK;
			organisationFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with organisation of 'organisation2'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with organisation of 'organisation2'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with organisation of 'organisation2'", activityCollection.Contains(salesCall.PK));
		}

		public void TestOrganizationNameFilter()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.OH_FullName = "ORG FULL NAME 1";
			var opportunity = organisation1.SalesOpportunities.AddNew();
			var opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "WRK";
			inquiry.O1_CompanyName = "ORG FULL NAME 2";
			var organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation3.OH_FullName = "ORG FULL NAME 3";
			var salesCall = organisation3.SalesCalls.AddNew();

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var organisationNameFilter = (ModuleTextFilter)dashboardFilter["Organization Name"];
			AssertNotNull(organisationNameFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			organisationNameFilter.Property = "ORG FULL NAME 1";
			organisationNameFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 1'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 1'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 1'", !activityCollection.Contains(salesCall.PK));

			organisationNameFilter.Property = "ORG FULL NAME 2";
			organisationNameFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 2'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 2'", activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 2'", !activityCollection.Contains(salesCall.PK));

			organisationNameFilter.Property = "ORG FULL NAME 3";
			organisationNameFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 3'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 3'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with organisation name of 'ORG FULL NAME 3'", activityCollection.Contains(salesCall.PK));
		}

		public void TestActivityDescriptionFilter()
		{
			CreateSalesActivitiesForTest();

			opportunity.P8_OpportunityDescription = "My Opportunity Description";
			salesCall.OQ_CallSummary = "My Call Summary";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var activityDescriptionFilter = (ModuleTextFilter)dashboardFilter["Activity Description"];
			AssertNotNull(activityDescriptionFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			activityDescriptionFilter.Property = "My Opportunity Description";
			activityDescriptionFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with description of 'My Opportunity Description'", activityCollection.Contains(opportunity.PK));
			Assert("Inquiries do not have a description", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with description of 'My Opportunity Description'", !activityCollection.Contains(salesCall.PK));

			activityDescriptionFilter.Property = "My Call Summary";
			activityDescriptionFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with description of 'My Call Summary'", !activityCollection.Contains(opportunity.PK));
			Assert("Inquiries do not have a description", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with description of 'My Call Summary'", activityCollection.Contains(salesCall.PK));
		}

		public void TestActivityStaffAssignmentFilter()
		{
			CreateSalesActivitiesForTest();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			opportunity.P8_GS_NKPrimarySalesPerson = staff1.GS_Code;
			inquiry.O1_GS_NKRepAssigned = staff1.GS_Code;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "LDA";
			salesCall.OQ_GS_NKSalesRep = staff2.GS_Code;

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var activityStaffAssignmentFilter = (ModuleNkFilter)dashboardFilter[SalesDashboardFilterBusinessObject.ActivityStaffAssignmentFilterName];
			AssertNotNull(activityStaffAssignmentFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			activityStaffAssignmentFilter.Property = "ADL";
			activityStaffAssignmentFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with assigned staff of 'ADL'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with assigned staff of 'ADL'", activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with assigned staff of 'ADL'", !activityCollection.Contains(salesCall.PK));

			activityStaffAssignmentFilter.Property = "LDA";
			activityStaffAssignmentFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with assigned staff of 'LDA'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with assigned staff of 'LDA'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with assigned staff of 'LDA'", activityCollection.Contains(salesCall.PK));
		}

		public void TestSalesTeamFilter()
		{
			CreateSalesActivitiesForTest();

			var salesTeam1 = Factory.NewWithValidTestData<GlbGroup>();
			salesTeam1.GG_IsSales = true;
			salesTeam1.GG_Code = "SALES1";
			var staff1 = salesTeam1.Staff.AddNew();
			staff1.GS_Code = "ADL";
			staff1.GS_LoginName = "ADL.login";
			opportunity.P8_GS_NKPrimarySalesPerson = staff1.GS_Code;

			var salesTeam2 = Factory.NewWithValidTestData<GlbGroup>();
			salesTeam2.GG_IsSales = true;
			salesTeam2.GG_Code = "SALES2";
			var staff2 = salesTeam2.Staff.AddNew();
			staff2.GS_Code = "SWC";
			staff1.GS_LoginName = "SWC.login";
			salesCall.OQ_GS_NKSalesRep = staff2.GS_Code;

			var nonSalesGroup = Factory.NewWithValidTestData<GlbGroup>();
			nonSalesGroup.GG_Code = "NOSALES";
			var nonSalesStaff = nonSalesGroup.Staff.AddNew();
			nonSalesStaff.GS_Code = "RIS";
			nonSalesStaff.GS_LoginName = "RIS.login";
			inquiry.O1_GS_NKRepAssigned = nonSalesStaff.GS_Code;

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var salesTeamStaffFilter = (ModuleNkFilter)dashboardFilter["Sales Team"];
			AssertNotNull(salesTeamStaffFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			salesTeamStaffFilter.Property = "SALES1";
			salesTeamStaffFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with assigned staff that is in sales team 'SALES1'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with assigned staff that is in sales team 'SALES1'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with assigned staff that is in sales team 'SALES1'", !activityCollection.Contains(salesCall.PK));

			salesTeamStaffFilter.Property = "SALES2";
			salesTeamStaffFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with assigned staff that is in sales team 'SALES2'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with assigned staff that is in sales team 'SALES2'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with assigned staff that is in sales team 'SALES2'", activityCollection.Contains(salesCall.PK));

			salesTeamStaffFilter.Property = "NOSALES";
			salesTeamStaffFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain any sales activities if group is not a sales team", !activityCollection.Contains(opportunity.PK));
			Assert("Should not contain any sales activities if group is not a sales team", !activityCollection.Contains(inquiry.PK));
			Assert("Should not contain any sales activities if group is not a sales team", !activityCollection.Contains(salesCall.PK));
		}

		public void TestContactNameFilter()
		{
			CreateSalesActivitiesForTest();

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			opportunity.P8_OC = contact1.PK;
			inquiry.O1_ContactName = "Andrew";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "Luong";
			salesCall.OQ_OC = contact2.PK;

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var contactNameFilter = (ModuleTextFilter)dashboardFilter["Contact Name"];
			AssertNotNull(contactNameFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(salesCall.PK));

			contactNameFilter.Property = "Andrew";
			contactNameFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with contact name of 'Andrew'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with contact name of 'Andrew'", activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with contact name of 'Andrew'", !activityCollection.Contains(salesCall.PK));

			contactNameFilter.Property = "Luong";
			contactNameFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with contact name of 'Luong'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with contact name of 'Luong'", !activityCollection.Contains(inquiry.PK));
			Assert("Should only contain activities with contact name of 'Luong'", activityCollection.Contains(salesCall.PK));
		}

		public void TestAnyOpenTaskAssignedToFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SWC";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "RIS";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var firstOpenTask = opportunity.WorkflowItems.AddNew();
			firstOpenTask.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			firstOpenTask.P9_Status = "ASN";
			firstOpenTask.P9_Sequence = 1;
			var secondOpenTask = opportunity.WorkflowItems.AddNew();
			secondOpenTask.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			secondOpenTask.P9_Status = "WRK";
			secondOpenTask.P9_Sequence = 2;
			var closedTask = opportunity.WorkflowItems.AddNew();
			closedTask.P9_GS_NKAssignedStaffMember = staff3.GS_Code;
			closedTask.P9_Status = "CLS";
			closedTask.P9_Sequence = 3;

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var anyOpenTaskAssignedToFilter = (ModuleNkFilter)dashboardFilter["Any Open Task Assigned To"];
			AssertNotNull(anyOpenTaskAssignedToFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity.PK));

			anyOpenTaskAssignedToFilter.Property = "ADL";
			anyOpenTaskAssignedToFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should still contain the activity as 'ADL' task is open", activityCollection.Contains(opportunity.PK));

			anyOpenTaskAssignedToFilter.Property = "SWC";
			anyOpenTaskAssignedToFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should still contain the activity as 'SWC' task is open", activityCollection.Contains(opportunity.PK));

			anyOpenTaskAssignedToFilter.Property = "RIS";
			anyOpenTaskAssignedToFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain the activity as 'RIS' task is closed", !activityCollection.Contains(opportunity.PK));
		}

		public void TestCurrentTaskAssignedTo()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SWC";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = organisation.SalesOpportunities.AddNew();
			var task1 = opportunity1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1.P9_Status = "ASN";
			task1.P9_Sequence = 1;
			var task2 = opportunity1.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = "WRK";
			task2.P9_Sequence = 2;

			var opportunity2 = organisation.SalesOpportunities.AddNew();

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var filter = (ModuleNkFilter)dashboardFilter[SalesDashboardFilterBusinessObject.CurrentTaskAssignedToFilterName];
			AssertNotNull(filter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity1.PK));
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity2.PK));

			filter.Property = "ADL";
			filter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should still contain the activity as 'ADL' is assigned to current task", activityCollection.Contains(opportunity1.PK));
			Assert("Should not contain the activity as there is no current task", !activityCollection.Contains(opportunity2.PK));

			filter.Property = "SWC";
			filter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain the activity as 'SCW' is not assigned to current task", !activityCollection.Contains(opportunity1.PK));
			Assert("Should not contain the activity as 'SCW' is not assigned to current task (there is no current task)", !activityCollection.Contains(opportunity2.PK));

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;
			filter.Property = "";
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain the activity as 'ADL' is assigned to current task", !activityCollection.Contains(opportunity1.PK));
			Assert("Should contain the activity as current task assignment is blank", activityCollection.Contains(opportunity2.PK));

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
			filter.Property = "SWC";
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain the activity as 'ADL' is assigned to current task", activityCollection.Contains(opportunity1.PK));
			Assert("Should not contain the activity as current task assignment is blank", !activityCollection.Contains(opportunity2.PK));
		}

		public void TestAnyTaskAssignedTo()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SWC";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "RIS";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var firstOpenTask = opportunity.WorkflowItems.AddNew();
			firstOpenTask.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			firstOpenTask.P9_Status = "ASN";
			firstOpenTask.P9_Sequence = 1;
			var secondOpenTask = opportunity.WorkflowItems.AddNew();
			secondOpenTask.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			secondOpenTask.P9_Status = "WRK";
			secondOpenTask.P9_Sequence = 2;

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var taskAssignedToFilter = (ModuleNkFilter)dashboardFilter["Any Task Assigned To"];
			AssertNotNull(taskAssignedToFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity.PK));

			taskAssignedToFilter.Property = "ADL";
			taskAssignedToFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should still contain the activity as 'ADL' is assigned to a task of the activity", activityCollection.Contains(opportunity.PK));

			taskAssignedToFilter.Property = "SWC";
			taskAssignedToFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should still contain the activity as 'SWC' is assigned to a task of the activity", activityCollection.Contains(opportunity.PK));

			taskAssignedToFilter.Property = "RIS";
			taskAssignedToFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain the activity as 'RIS' is not assigned to a task of the activity", !activityCollection.Contains(opportunity.PK));
		}

		public void TestAnyTaskAssignedGroupFilter()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1a = organisation.SalesOpportunities.AddNew();
			var task1a = opportunity1a.WorkflowItems.AddNew();
			task1a.P9_GG_AssignedGroup = group1.PK;
			task1a.P9_Status = "ASN";

			var opportunity1b = organisation.SalesOpportunities.AddNew();
			var task1b = opportunity1b.WorkflowItems.AddNew();
			task1b.P9_GG_AssignedGroup = group1.PK;
			task1b.P9_Status = "ASN";

			var opportunity2 = organisation.SalesOpportunities.AddNew();
			var task2 = opportunity2.WorkflowItems.AddNew();
			task2.P9_GG_AssignedGroup = group2.PK;
			task2.P9_Status = "ASN";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var taskAssignedGroupFilter = (ModuleGuidFilter)dashboardFilter["Any Task Assigned Group"];
			AssertNotNull(taskAssignedGroupFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity1a.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity1b.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity2.PK));

			taskAssignedGroupFilter.Property = group1.PK;
			taskAssignedGroupFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain activities with assigned group of group1", activityCollection.Contains(opportunity1a.PK));
			Assert("Should contain activities with assigned group of group1", activityCollection.Contains(opportunity1b.PK));
			Assert("Should contain activities with assigned group of group1", !activityCollection.Contains(opportunity2.PK));

			taskAssignedGroupFilter.Property = group2.PK;
			taskAssignedGroupFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain activities with assigned group of group2", !activityCollection.Contains(opportunity1a.PK));
			Assert("Should contain activities with assigned group of group2", !activityCollection.Contains(opportunity1b.PK));
			Assert("Should contain activities with assigned group of group2", activityCollection.Contains(opportunity2.PK));
		}

		[TestDate(2013, 2, 6)]
		public void TestTaskOverdueFilter()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "ASN";
			inquiryTask.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2013, 1, 1)));

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var taskOverdueFilter = (ModuleFlagsFilter)dashboardFilter["Task Overdue"];
			AssertNotNull(taskOverdueFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));

			taskOverdueFilter.Property0 = true;
			taskOverdueFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with overdue task", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with overdue task", activityCollection.Contains(inquiry.PK));
		}

		public void TestTaskStatusFilter()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "ASN";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var taskStatusFilter = (ModuleTextFilter)dashboardFilter["Task Status"];
			AssertNotNull(taskStatusFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));

			taskStatusFilter.Property = "WRK";
			taskStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with task status of 'WRK'", activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with task status of 'WRK'", !activityCollection.Contains(inquiry.PK));

			taskStatusFilter.Property = "ASN";
			taskStatusFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should only contain activities with task status of 'ASN'", !activityCollection.Contains(opportunity.PK));
			Assert("Should only contain activities with task status of 'ASN'", activityCollection.Contains(inquiry.PK));
		}

		public void TestCurrentTaskAssignedGroup()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var task1 = opportunity.WorkflowItems.AddNew();
			task1.P9_GG_AssignedGroup = group1.PK;
			task1.P9_Status = "ASN";
			task1.P9_Sequence = 1;
			var task2 = opportunity.WorkflowItems.AddNew();
			task2.P9_GG_AssignedGroup = group2.PK;
			task2.P9_Status = "WRK";
			task2.P9_Sequence = 2;

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var filter = (ModuleGuidFilter)dashboardFilter["Current Task Assigned Group"];
			AssertNotNull(filter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity.PK));

			filter.Property = group1.PK;
			filter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should still contain the activity as group 1 is assigned to current task", activityCollection.Contains(opportunity.PK));

			filter.Property = group2.PK;
			filter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain the activity as group 2 is not assigned to current task", !activityCollection.Contains(opportunity.PK));
		}

		public void TestCurrentTaskAssignedGroupNotStaff()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "RIS";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = organisation.SalesOpportunities.AddNew();
			{
				var task = opportunity1.WorkflowItems.AddNew();
				task.P9_GG_AssignedGroup = group1.PK;
				task.P9_Status = "ASN";
				task.P9_Sequence = 1;

				task = opportunity1.WorkflowItems.AddNew();
				task.P9_GG_AssignedGroup = group2.PK;
				task.P9_Status = "WRK";
				task.P9_Sequence = 2;
			}

			var opportunity2 = organisation.SalesOpportunities.AddNew();
			{
				var task = opportunity2.WorkflowItems.AddNew();
				task.P9_GG_AssignedGroup = group2.PK;
				task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
				task.P9_Status = "ASN";
				task.P9_Sequence = 1;
			}

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var filter = (ModuleGuidFilter)dashboardFilter["Current Group With Staff Blank"];
			AssertNotNull(filter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity1.PK));
			Assert("Precondition: Should contain the activity", activityCollection.Contains(opportunity2.PK));

			filter.Property = group1.PK;
			filter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should contain the activity as group 1 is assigned to current task and no staff", activityCollection.Contains(opportunity1.PK));
			Assert("Should not contain the activity as group 1 is not assigned to current task", !activityCollection.Contains(opportunity2.PK));

			filter.Property = group2.PK;
			filter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Should not contain the activity as group 2 is not assigned to current task", !activityCollection.Contains(opportunity1.PK));
			Assert("Should not contain the activity as group 2 is assigned to current task, but so is a staff", !activityCollection.Contains(opportunity1.PK));
		}

		public void TestSystemDefinedLayout()
		{
			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)dashboardFilter).LayoutContext = ModuleIDs.SalesDashboard.Name;
			var layout = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(ModuleIDs.SalesDashboard.Name, "My Activity Tasks", true);
			AssertNotNull("System defined layout My Activity Tasks should exisit", layout);
			dashboardFilter.LoadLayout(layout);
			var filter1 = (ModuleNkFilter)dashboardFilter.ModuleFilters[SalesDashboardFilterBusinessObject.ActivityStaffAssignmentFilterName];
			var filter2 = (ModuleNkFilter)dashboardFilter.ModuleFilters[SalesDashboardFilterBusinessObject.CurrentTaskAssignedToFilterName];
			var filter3 = (ModuleTextFilter)dashboardFilter.ModuleFilters[SalesDashboardFilterBusinessObject.OverallActivityDispositionFilterName];
			Assert(filter1.IsActive);
			Assert(filter2.IsActive);
			Assert(filter3.IsActive);
			AssertEquals(Env.CurrentUser.Initials, filter1.Property);
			AssertEquals(Env.CurrentUser.Initials, filter2.Property);
			AssertEquals("OPN", filter3.Property);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "RIS";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				dashboardFilter.LoadLayout(layout);
				filter1 = (ModuleNkFilter)dashboardFilter.ModuleFilters[SalesDashboardFilterBusinessObject.ActivityStaffAssignmentFilterName];
				filter2 = (ModuleNkFilter)dashboardFilter.ModuleFilters[SalesDashboardFilterBusinessObject.CurrentTaskAssignedToFilterName];
				Assert(filter1.IsActive);
				Assert(filter2.IsActive);
				Assert(filter3.IsActive);
				AssertEquals("RIS", filter1.Property);
				AssertEquals("RIS", filter2.Property);
				AssertEquals("OPN", filter3.Property);
			}
		}

		public void TestColdCallRegisterInIncluded()
		{
			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry1.O1_EnquiryType = "INQ";
			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry2.O1_EnquiryType = "CCR";

			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();
			var activityTypeFilter = (ModuleFlagsFilter)dashboardFilter["Activity Type"];
			AssertNotNull(activityTypeFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should INQ", activityCollection.Contains(inquiry1.PK));
			Assert("Precondition: Should CCR", activityCollection.Contains(inquiry2.PK));
		}

		[TestDate(2018, 9, 5)]
		public void TestTagAndTagGroupFilter()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = testHelper.CreateJobHeader<OrgOpportunity>(Factory);
			var processHeader = testHelper.CreateWorkflow(jobHeader, "it is done");

			var jobHeader2 = testHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var processHeader2 = testHelper.CreateWorkflow(jobHeader2, "wow done");

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";
			opportunityTask.P9_FH_ProcessHeader = processHeader.PK;

			var opportunityWorkflow = (ITagable)opportunityTask.ProcessHeader;

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "WRK";
			inquiryTask.P9_FH_ProcessHeader = processHeader2.PK;

			var inquiryWorkflow = (ITagable)inquiryTask.ProcessHeader;

			var tagDefinition1 = Factory.New<ITagDefinition>();
			tagDefinition1.TGD_Code = "AAA";
			var tagMagnitude1 = Factory.New<ITagMagnitude>();
			tagMagnitude1.TGM_Code = "ZZZ";
			tagMagnitude1.TGM_TGD_Tag = tagDefinition1.PK;

			var tagDefinition2 = Factory.New<ITagDefinition>();
			tagDefinition2.TGD_Code = "BBB";
			var tagMagnitude2 = Factory.New<ITagMagnitude>();
			tagMagnitude2.TGM_Code = "XXX";
			tagMagnitude2.TGM_TGD_Tag = tagDefinition2.PK;

			opportunityWorkflow.AddTag(tagMagnitude1);
			opportunityTask.AddTag(tagMagnitude1);

			inquiryWorkflow.AddTag(tagMagnitude2);
			inquiryTask.AddTag(tagMagnitude2);

			testHelper.EnableBMSInRegistry();
			testHelper.CreateSystem(Factory, "INQ");
			Factory.Save();

			var dashboardFilter = new SalesDashboardFilterBusinessObject();

			// TAG
			var tagFilter = (ModuleGuidFilter)dashboardFilter["Tag Magnitude"];
			AssertNotNull(tagFilter);

			var activityCollection = new SalesDashboardActivityCollection(Factory);
			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));

			tagFilter.Property = tagMagnitude1.PK;
			tagFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals(1, activityCollection.Count);
			AssertEquals("Should only contain activities with activity type of 'OPP'", "OPP", activityCollection[0].ActivityType);

			tagFilter.Property = tagMagnitude2.PK;
			tagFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals(1, activityCollection.Count);
			AssertEquals("Should only contain activities with activity type of 'INQ'", "INQ", activityCollection[0].ActivityType);

			tagFilter.Property = ZGuid.Empty;

			// TAG GROUP
			var tagGroupFilter = (ModuleGuidFilter)dashboardFilter["Tag Definition Code"];
			AssertNotNull(tagGroupFilter);

			activityCollection.Load(dashboardFilter.Filter);
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(opportunity.PK));
			Assert("Precondition: Should contain all sales activities", activityCollection.Contains(inquiry.PK));

			tagGroupFilter.Property = tagDefinition1.PK;
			tagGroupFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals(1, activityCollection.Count);
			AssertEquals("Should only contain activities with activity type of 'OPP'", "OPP", activityCollection[0].ActivityType);

			tagGroupFilter.Property = tagDefinition2.PK;
			tagGroupFilter.IsActive = true;
			activityCollection.Load(dashboardFilter.Filter);
			AssertEquals(1, activityCollection.Count);
			AssertEquals("Should only contain activities with activity type of 'INQ'", "INQ", activityCollection[0].ActivityType);
		}

		#endregion

		#region CRM Security

		[ExpectNoExceptions]
		public void TestCRMSecurityFilters()
		{
			Env.Security.InquiryManagerCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.InquiryManagerCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.InquiryManagerCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			Env.Security.OpportunityManagementCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.OpportunityManagementCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.OpportunityManagementCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			Env.Security.CommunicationManagerCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.CommunicationManagerCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.CommunicationManagerCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			Env.Security.CampaignManagementCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.CampaignManagementCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			Env.Security.OneOffQuoteCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.OneOffQuoteCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.OneOffQuoteCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			Env.Security.QuotationCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.QuotationCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.QuotationCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			Env.Security.ProjectCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.ProjectCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.ProjectCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			var filters = GetNewFilterStripBusinessObject();
			var filterTypes = new string[]
			{
				SalesDashboardActivityTypeCodeList.Descriptions.Campaign,
				SalesDashboardActivityTypeCodeList.Descriptions.Communication,
				SalesDashboardActivityTypeCodeList.Descriptions.Inquiry,
				SalesDashboardActivityTypeCodeList.Descriptions.OneOffQuote,
				SalesDashboardActivityTypeCodeList.Descriptions.Opportunity,
				SalesDashboardActivityTypeCodeList.Descriptions.Project,
				SalesDashboardActivityTypeCodeList.Descriptions.Quotation
			};

			var filter = filters["SalesDashboardCRMSecurityFilter"] as ModuleFlagsFilter;
			AssertNotNull("SalesDashboardCRMSecurityFilter", filter);
			AssertEquals(FilterOrCategory.MandatoryFilterOrCategory, filter.OrCategory);
			var bizObjs = Factory.Load<SalesDashboardActivity>(filter.Query);

			foreach (var type in filterTypes)
			{
				Assert("Enforced CRM Security for " + type, filter.DefaultProperties[type]);
			}
		}

		#endregion

		#region Implementation

		void AssertActivityCollectionItemsPresent(string message, SalesDashboardActivityCollection collection, List<BusinessObject> present)
		{
			present.ForEach(t => Assert(message + ": Should contain activity at index " + present.IndexOf(t), collection.Contains(t.PK)));
		}
		void AssertActivityCollectionItemsAbsent(string message, SalesDashboardActivityCollection collection, List<BusinessObject> absent)
		{
			absent.ForEach(t => Assert(message + ": Should not contain activity at index " + absent.IndexOf(t), !collection.Contains(t.PK)));
		}
		void AssertActivityCollectionItems(string message, SalesDashboardActivityCollection collection, List<BusinessObject> present, List<BusinessObject> absent)
		{
			AssertActivityCollectionItemsPresent(message, collection, present);
			AssertActivityCollectionItemsAbsent(message, collection, absent);
		}
		void AssertActivityCollectionItemsContainsAllTestObjects(SalesDashboardActivityCollection collection)
		{
			AssertActivityCollectionItemsPresent("*", collection, new List<BusinessObject> { opportunity, inquiry, salesCall, campaignItem, oneOffQuote, quotation, project });
		}

		void CreateSalesActivitiesForTest()
		{
			organisation = Factory.NewWithValidTestData<OrgHeader>();

			opportunity = organisation.SalesOpportunities.AddNew();
			opportunityTask = opportunity.WorkflowItems.AddNew();
			opportunityTask.P9_Status = "WRK";

			inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiryTask = inquiry.WorkflowItems.AddNew();
			inquiryTask.P9_Status = "WRK";

			salesCall = organisation.SalesCalls.AddNew();

			campaignItem = CreateCampaignItem(false);

			oneOffQuote = CreateOneOffQuote();
			oneOffQuoteTask = oneOffQuote.ParentQuote.WorkflowItems.AddNew();
			oneOffQuoteTask.P9_Status = "WRK";

			quotation = Factory.NewWithValidTestData<Quote>();
			quotationTask = quotation.WorkflowItems.AddNew();
			quotationTask.P9_Status = "WRK";

			project = CreateProject();

			Factory.Save();
		}

		GlbCompanyCampaignItem CreateCampaignItem(bool orgColdCallRegister, OrgHeader org = null)
		{
			OrgContact contact;
			if (org != null)
			{
				contact = org.Contacts.AddNew();
				contact.OC_ContactName = "test" + org.Contacts.Count;
				Factory.Save();
			}
			else
			{
				contact = Factory.NewWithValidTestData<OrgContact>();
			}
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var result = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			result.G8_G0 = campaign.PK;
			if (orgColdCallRegister)
			{
				var register = Factory.NewWithValidTestData<OrgColdCallRegister>();
				register.O1_OC_LinkedContact = contact.PK;
				register.O1_OH_ConvertedToQualifiedLead = contact.OC_OH;
				result.G8_RecipientID = register.PK;
				result.G8_RecipientTableCode = "O1";
				Factory.Save();
			}
			else
			{
				result.G8_RecipientID = contact.PK;
				result.G8_RecipientTableCode = "OC";
			}
			return result;
		}

		RateOneOffShipment CreateOneOffQuote(OrgHeader org = null)
		{
			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quotedBooking = quotedBookingBuilder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var result = Factory.LoadTop1<RateOneOffShipment>(new ZQuery(RateOneOffShipmentSchema.TT_TH, quotedBooking.ViewPK));
			if (org != null)
			{
				result.ParentQuote.TH_OH = org.PK;
			}

			return result;
		}

		BusinessObject CreateProject(OrgHeader org = null)
		{
			var result = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			if (org != null)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_OH = org.PK;
				result.SetPossiblyCustomProperty(WorkProjectSchema.Constants.WKP_OC_Contact, contact.PK);
			}
			return result;
		}

		OrgHeader organisation;
		OrgOpportunity opportunity;
		ProcessTask opportunityTask;
		SalesEnquiry inquiry;
		ProcessTask inquiryTask;
		OrgSalesCall salesCall;
		GlbCompanyCampaignItem campaignItem;
		RateOneOffShipment oneOffQuote;
		ProcessTask oneOffQuoteTask;
		Quote quotation;
		ProcessTask quotationTask;
		BusinessObject project;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SalesDashboardFilterBusinessObject();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(GlbGroupSchema.Constants.TableName, "Sales Team"));
			result.Add(TableFilter(GlbGroupLinkSchema.Constants.TableName, "Sales Team"));
			result.Add(TableFilter(GlbStaffSchema.Constants.TableName, "Sales Team"));

			result.Add(TableFilter(ProcessHeaderSchema.Constants.TableName, "Tag"));
			result.Add(TableFilter(TagLinkSchema.Constants.TableName, "Tag"));

			return result;
		}

		#endregion
	}
}
