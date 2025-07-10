using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CampaignContactFilterCategories = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class GlbCompanyCampaignContactFilterControlTest : TestCaseWithFactory
	{
		public void TestSetFilterData()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterStripBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			Factory.Save();

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();
				AssertEquals("Precondition", FilterOrCategory.None, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 5 filter strips set", 6, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			ZQuery queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, "GlbCompanyCampaignContact");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_IsPublished, false);
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, Campaign.PK);
			var reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
			AssertNotNull("Precondition", reloadedFilter);

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();

				AssertEquals("Saved last used layout loaded", reloadedFilter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);

				var filter = Factory.New<StmModuleFilter>();
				form.CampaignsControl.FilterStripControl.SetFilterData(filter);

				AssertEquals("Should be set to a new filter", filter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
			}
		}

		public void TestLoaded()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterStripBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			Factory.Save();

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();
				AssertEquals("Precondition", FilterOrCategory.None, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 6 filter strips set (5 from default layout + 1 always visible Subscription status)", 6, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("There should be 5 filter strips set", 5, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			ZQuery queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, "GlbCompanyCampaignContact");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_IsPublished, false);
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, Campaign.PK);
			var reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
			AssertNotNull("Precondition", reloadedFilter);

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();

				AssertEquals("Saved last used layout loaded", reloadedFilter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ABC";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";
			branch.GB_GC = company.PK;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "EDW";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FormForTest form = new FormForTest(Campaign))
			{
				var original = Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.CampaignManagement).IsAllowed;
				try
				{
					Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.CampaignManagement).IsAllowed = true;
					form.Show();

					AssertEquals("Saved last used layout loaded", reloadedFilter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
					AssertEquals("Saved last used layout not under Find Button", 2, form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
					AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				}
				finally
				{
					Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.CampaignManagement).IsAllowed = original;
				}
			}
		}

		public void TestLoaded_WithContactDataSource()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterStripBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			Factory.Save();

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();
				AssertEquals("Default Data source should be Organizations", ContactDataSourceList.Codes.ClientIntelligence, Campaign.ContactDataSource);
				AssertEquals("Precondition", FilterOrCategory.None, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 6 filter strips set (5 from default layout + 1 always visible Subscription status)", 6, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("There should be 5 filter strips set", 5, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();

				AssertEquals(FilterOrCategory.Green, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals(ContactDataSourceList.Codes.ClientIntelligence, Campaign.ContactDataSource);

				Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				AssertEquals("Precondition", FilterOrCategory.None, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 5 filter strips set (4 from default layout + 1 always visible Subscription status)", 5, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("There should be 4 filter strips set", 4, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Blue;
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
			}

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();
				AssertEquals(FilterOrCategory.Blue, form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals(ContactDataSourceList.Codes.Inquiries, Campaign.ContactDataSource);
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_SaveDefaultLayout()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();
				AssertEquals("Precondition", ContactDataSourceList.Codes.ClientIntelligence, Campaign.ContactDataSource);
				AssertEquals("Precondition: System Default Layout loaded (5 filters loaded from default layout + 1 always visible for Subscription status)", 6, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("Precondition: System Default Layout loaded", 5, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);

				var queryFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.StartsWith, ModuleIDs.GlbCompanyCampaignContact.Name).AddToFilter(StmModuleFilterSchema.S9_IsSystem, false);
				AssertNull(Factory.LoadTop1<StmModuleFilter>(queryFilter));

				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				AssertEquals("No layout created because last selected layout is already saved", 0, Factory.Load<StmModuleFilter>(queryFilter).Length);
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.LoadLayout(null);
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();

				Campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.LoadLayout(null);
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				AssertEquals("Layout created because last selected layout was not saved", 2, Factory.Load<StmModuleFilter>(queryFilter).Length);
				ZQuery campaignTrackingFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking);
				campaignTrackingFilter.AddToFilter(queryFilter);
				AssertNotNull(Factory.LoadTop1<StmModuleFilter>(campaignTrackingFilter));

				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				AssertEquals("Layout created because last selected layout was not saved", 2, Factory.Load<StmModuleFilter>(queryFilter).Length);
				ZQuery clientIntelFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name);
				clientIntelFilter.AddToFilter(queryFilter);
				AssertNotNull(Factory.LoadTop1<StmModuleFilter>(clientIntelFilter));

				Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				AssertEquals("No layout created because last selected layout is already saved", 2, Factory.Load<StmModuleFilter>(queryFilter).Length);
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.LoadLayout(null);

				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;

				Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				AssertEquals("Layout created because last selected layout was not saved", 3, Factory.Load<StmModuleFilter>(queryFilter).Length);
				ZQuery inquiryFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.Inquiries);
				inquiryFilter.AddToFilter(queryFilter);
				AssertNotNull(Factory.LoadTop1<StmModuleFilter>(inquiryFilter));
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_UpdateLayoutContext()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mocksChs = Res.UseMockData())
			{
				mocksChs.Put("ContactDataSourceList|CampaignTracking", new ResourceStringData("ContactDataSourceList|CampaignTracking", "Campaign Tracking (Chinese - Simplified)"));
				AssertEquals("Precondition", "Campaign Tracking (Chinese - Simplified)", ContactDataSourceList.Codes.CampaignTracking);
				mocksChs.Put("ContactDataSourceList|ClientIntelligence", new ResourceStringData("ContactDataSourceList|ClientIntelligence", "Client Intelligence (Chinese - Simplified)"));
				AssertEquals("Precondition", "Client Intelligence (Chinese - Simplified)", ContactDataSourceList.Codes.ClientIntelligence);
				mocksChs.Put("ContactDataSourceList|Inquiries", new ResourceStringData("ContactDataSourceList|Inquiries", "Inquiries (Chinese - Simplified)"));
				AssertEquals("Precondition", "Inquiries (Chinese - Simplified)", ContactDataSourceList.Codes.Inquiries);

				Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

				using (FormForTest form = new FormForTest(Campaign))
				{
					// ensure S9_ModuleIDs stay the same even when translations for the Contact Data Source list get added so existing layouts continue to be loaded
					form.Show();
					AssertEquals("Precondition", ContactDataSourceList.Codes.ClientIntelligence, Campaign.ContactDataSource);
					AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name, ((IFilterStripBusinessObjectInternals)form.CampaignsControl.FilterStripControl.FilterBusinessObject).LayoutContext);

					Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
					AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking, ((IFilterStripBusinessObjectInternals)form.CampaignsControl.FilterStripControl.FilterBusinessObject).LayoutContext);

					Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
					AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.Inquiries, ((IFilterStripBusinessObjectInternals)form.CampaignsControl.FilterStripControl.FilterBusinessObject).LayoutContext);
				}
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_LoadDefaultLayoutAndHideItUnderFindButton()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			ZQuery queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			ZQuery clientIntelFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name);
			clientIntelFilter.AddToFilter(queryFilter);
			ZQuery campaignTrackingFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking);
			campaignTrackingFilter.AddToFilter(queryFilter);
			ZQuery inquiryFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.Inquiries);
			inquiryFilter.AddToFilter(queryFilter);

			using (FormForTest form = new FormForTest(Campaign))
			{
				form.Show();
				AssertEquals("Precondition", ContactDataSourceList.Codes.ClientIntelligence, Campaign.ContactDataSource);
				AssertEquals("Precondition: System Default Layout loaded (5 filters loaded from default layout + 1 always visible for Subscription status)", 6, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("Precondition: System Default Layout loaded", 5, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.LoadLayout(null);

				// create last selected layouts by switching between data sources
				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				AssertEquals("Chosen filters are replaced with ones in System Default Layout (+ 1 always visible for Subscription status)", 4, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("Chosen filters are replaced with ones in System Default Layout", 3, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				AssertEquals("Available filters are changed appropriately", true, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ModuleFilters
.Any(x => x.Category == CampaignContactFilterCategories.CampaignTracking));
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.LoadLayout(null);

				Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				AssertEquals("Chosen filters are replaced with ones in System Default Layout (+ 1 always visible for Subscription status)", 5, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				GlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(form.CampaignsControl.FilterStripControl.FilterBusinessObject);
				AssertEquals("Chosen filters are replaced with ones in System Default Layout", 4, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				AssertEquals("Available filters are changed appropriately", true, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ModuleFilters
.Any(x => x.Category == CampaignContactFilterCategories.Inquiries));
				form.CampaignsControl.FilterStripControl.SaveDefaultLayout();
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.LoadLayout(null);

				// check these last selected layouts are loaded the next time the data source is selected
				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				var filter = Factory.LoadTop1<StmModuleFilter>(campaignTrackingFilter);
				AssertNotNull(filter);
				AssertEquals("Saved last used layout loaded", filter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals("System Default Layout", form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems[1].Text.Trim());

				Campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
				filter = Factory.LoadTop1<StmModuleFilter>(clientIntelFilter);
				AssertNotNull(filter);
				AssertEquals("Saved last used layout loaded", filter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals("System Default Layout", form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems[1].Text.Trim());

				Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				filter = Factory.LoadTop1<StmModuleFilter>(inquiryFilter);
				AssertNotNull(filter);
				AssertEquals("Saved last used layout loaded", filter.PK, form.CampaignsControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals("System Default Layout", form.CampaignsControl.FilterStripControl.ToolStripFindDropButtonExposed.DropDownItems[1].Text.Trim());
			}
		}

		public void TestSaveDefaultLayout()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterStripBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			using (var control = new GlbCompanyCampaignContactFilterControl(null, FilterStripBusinessObject, Campaign))
			{
				var queryFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name).AddToFilter(StmModuleFilterSchema.S9_IsSystem, false);
				AssertNull(Factory.LoadTop1<StmModuleFilter>(queryFilter));

				control.SaveDefaultLayout();

				Assert("Precondition", !Campaign.IsInDatabase);
				AssertNull("Campaign needs to be saved into DB first", Factory.LoadTop1<StmModuleFilter>(queryFilter));

				Campaign.Factory.Save();
				control.SaveDefaultLayout();

				var layout = Factory.LoadTop1<StmModuleFilter>(queryFilter);
				Assert("Precondition", Campaign.IsInDatabase);
				AssertNotNull(layout);
				AssertEquals(ZGuid.Empty, layout.S9_GC);

				Campaign = null;
				FilterStripBusinessObject = null;
			}
		}

		public void TestShouldPerformSearch()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			FilterStripBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);
			using (var control = new GlbCompanyCampaignContactFilterControlForTest(FilterStripBusinessObject, Campaign))
			{
				AssertEquals(true, control.ShouldPerformSearch_Exposed());

				Campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				Assert("Precondition", Campaign.IsUsingCampaignTrackingDataSource);

				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertEquals(false, control.ShouldPerformSearch_Exposed());
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				Campaign.SourceCampaignPK = campaign2.PK;
				AssertEquals(true, control.ShouldPerformSearch_Exposed());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				Campaign.SourceCampaignPK = Campaign.PK;
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertEquals(false, control.ShouldPerformSearch_Exposed());
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				Campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
				Assert("Precondition", !Campaign.IsUsingCampaignTrackingDataSource);
				AssertEquals(true, control.ShouldPerformSearch_Exposed());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				FilterStripBusinessObject = null;
			}
		}

		public void TestShouldPerformSearch_Touch()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var otherCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			FilterStripBusinessObject = new GlbCompanyCampaignContactFilterBusinessObject(touch1a);
			using (var control = new GlbCompanyCampaignContactFilterControlForTest(FilterStripBusinessObject, touch1a))
			{
				AssertEquals(false, control.ShouldPerformSearch_Exposed());

				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertEquals(false, control.ShouldPerformSearch_Exposed());
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				touch1a.SourceCampaignPK = master.PK;
				AssertEquals(true, control.ShouldPerformSearch_Exposed());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				touch1a.SourceCampaignPK = otherCampaign.PK;
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertEquals(false, control.ShouldPerformSearch_Exposed());
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestInitializeFilterStripsOnImport()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "Test Test";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			var campaign = Factory.New<GlbCompanyCampaign>();
			((IImportParentRelatedActivityInfoOnNew)campaign).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals("TESTAA", campaign.ImportOrgCode);
			AssertEquals("Test Test", campaign.ImportContactName);
			AssertEquals(ZGuid.Empty, campaign.ImportInquiryPK);

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();
				AssertEquals("There should be 2 filter strips set", 2, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				AssertEquals("TESTAA", ((ModuleNkFilter)form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property);
				AssertEquals("Test Test", ((ModuleTextFilter)form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[1].CurrentModuleFilter).Property);
				AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name, ((IFilterStripBusinessObjectInternals)form.CampaignsControl.FilterStripControl.FilterBusinessObject).LayoutContext);
			}

			var inquiryWithNoContactLink = Factory.New<SalesEnquiry>();
			inquiryWithNoContactLink.O1_Email = "eddie@hotmail.com";
			inquiryWithNoContactLink.O1_ContactName = "Eddie Tan";
			var campaign1 = Factory.New<GlbCompanyCampaign>();
			((IImportParentRelatedActivityInfoOnNew)campaign1).ImportParentInfo(inquiryWithNoContactLink, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals("", campaign1.ImportOrgCode);
			AssertEquals("Eddie Tan", campaign1.ImportContactName);
			AssertEquals(inquiryWithNoContactLink.PK, campaign1.ImportInquiryPK);

			using (FormForTest form = new FormForTest(campaign1))
			{
				form.Show();
				AssertEquals("There should be 2 filter strips set", 2, form.CampaignsControl.FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				AssertEquals(inquiryWithNoContactLink.PK, ((ModuleGuidFilter)form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property);
				AssertEquals("Eddie Tan", ((ModuleTextFilter)form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[1].CurrentModuleFilter).Property);
				AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name + "_" + ContactDataSourceList.FilterModuleIDSuffixes.Inquiries, ((IFilterStripBusinessObjectInternals)form.CampaignsControl.FilterStripControl.FilterBusinessObject).LayoutContext);
			}
		}

		public void TestLastUsedContactDataSource()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignContactFilterBusinessObject filterStrip = new GlbCompanyCampaignContactFilterBusinessObject(Campaign);

			StmData data = Factory.NewWithValidTestData<StmData>();
			data.SD_DepartmentGuid = Env.Instance.CurrentCompany.PK;
			data.SD_Type = "CUR";
			data.SD_Owner = Env.CurrentUser.PK;
			data.SD_Name = "GlbCompanyCampaignContact";

			StmData data2 = Factory.NewWithValidTestData<StmData>();
			data2.SD_DepartmentGuid = Env.Instance.CurrentCompany.PK;
			data2.SD_Type = "";
			data2.SD_Owner = Campaign.PK;
			data2.SD_Name = "GlbCompanyCampaignContact_Inquiries";

			Factory.Save();

			var result = CampaignContactFilterDataSourceHelper.GetCampaignContactDataSource(filterStrip.LastUsedCampaignContactLayoutFactory);
			AssertEquals("Organization", result);

			data.SD_Type = "";
			data2.SD_Type = "CUR";

			Factory.Save();

			result = CampaignContactFilterDataSourceHelper.GetCampaignContactDataSource(filterStrip.LastUsedCampaignContactLayoutFactory);
			AssertEquals("Inquiry Manager", result);
		}

		#region Implementation

		GlbCompanyCampaign Campaign;
		GlbCompanyCampaignContactFilterBusinessObject FilterStripBusinessObject;

		public class FormForTest : ZForm
		{
			public FormForTest(GlbCompanyCampaign businessEntity)
				: base(businessEntity)
			{
				CampaignsControl = new SendCampaignsControl();
				Controls.Add(CampaignsControl);
				CampaignsControl.SetDataBinding(businessEntity, null);
			}

			public SendCampaignsControl CampaignsControl;
		}

		class GlbCompanyCampaignContactFilterControlForTest : GlbCompanyCampaignContactFilterControl
		{
			public GlbCompanyCampaignContactFilterControlForTest(GlbCompanyCampaignContactFilterBusinessObject strip, GlbCompanyCampaign campaign)
				: base(null, strip, campaign)
			{
			}

			public ZBool ShouldPerformSearch_Exposed()
			{
				return base.ShouldPerformSearch();
			}
		}

		#endregion
	}
}
