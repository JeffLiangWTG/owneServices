using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI.Testing
{
	public class HRGlbCompanyCampaignContactFilterControlTest : TestCaseWithFactory
	{
		public void TestSetFilterData()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			Factory.Save();
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				Assert("Precondition", form.HRCampaignsControl.DataSourceExposed().IsUsingStaffDataSource);
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Precondition", FilterOrCategory.None, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 4 filter strips set", DefaultStaffFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;
				filterStripControl.SaveDefaultLayout();
				AssertEquals(FilterOrCategory.Green, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			var queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, "GlbCompanyCampaignContact_Staff");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_IsPublished, false);
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, Campaign.PK);
			var reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
			AssertNotNull("Precondition", reloadedFilter);
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Saved last used layout loaded", reloadedFilter.PK, filterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, filterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals(FilterOrCategory.Green, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				var filter = Factory.New<StmModuleFilter>();
				filterStripControl.SetFilterData(filter);
				AssertEquals("Should be set to a new filter", filter.PK, filterStripControl.FilterBusinessObject.LastUsedLayout.PK);
			}
		}

		public void TestLoaded()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			FilterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObjectForTest(Campaign);
			Factory.Save();
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Precondition", FilterOrCategory.None, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 4 filter strips set (3 from default layout + 1 always visible Subscription status)", DefaultStaffFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				HRGlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterStripControl.FilterBusinessObject);
				AssertEquals("There should be 3 filter strips set", DefaultStaffFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;
				filterStripControl.SaveDefaultLayout();
				AssertEquals(FilterOrCategory.Green, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			var queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, "GlbCompanyCampaignContact_Staff");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_IsPublished, false);
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, Campaign.PK);
			var reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
			AssertNotNull("Precondition", reloadedFilter);
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Saved last used layout loaded", reloadedFilter.PK, filterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, filterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals(FilterOrCategory.Green, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";
			branch.GB_GC = company.PK;
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "EDW";
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (var form = new FormForTest(Campaign))
			{
				var original = Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.HRCampaignManagement).IsAllowed;
				try
				{
					Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.HRCampaignManagement).IsAllowed = true;
					form.Show();
					var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
					AssertEquals("Saved last used layout loaded", reloadedFilter.PK, filterStripControl.FilterBusinessObject.LastUsedLayout.PK);
					AssertEquals("Saved last used layout not under Find Button", 2, filterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
					AssertEquals(FilterOrCategory.Green, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				}
				finally
				{
					Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.HRCampaignManagement).IsAllowed = original;
				}
			}
		}

		public void TestLoaded_WithContactDataSource()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			FilterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObjectForTest(Campaign);
			Factory.Save();
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Default Data source should be Staff", HRContactDataSourceList.Codes.Staff, Campaign.ContactDataSource);
				AssertEquals("Precondition", FilterOrCategory.None, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 4 filter strips set (3 from default layout + 1 always visible Subscription status)", DefaultStaffFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				HRGlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterStripControl.FilterBusinessObject);
				AssertEquals("There should be 3 filter strips set", DefaultStaffFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;
				filterStripControl.SaveDefaultLayout();
				AssertEquals(FilterOrCategory.Green, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				filterStripControl.FilterBusinessObject.LoadLayout(null);
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
				AssertEquals("Precondition", FilterOrCategory.None, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals("There should be 1 filter strip set (0 from default layout + 1 always visible Subscription status)", DefaultApplicantFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				HRGlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterStripControl.FilterBusinessObject);
				AssertEquals("There should be 0 filter strips set", DefaultApplicantFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Blue;
				filterStripControl.SaveDefaultLayout();
			}

			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals(FilterOrCategory.Blue, filterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory);
				AssertEquals(HRContactDataSourceList.Codes.JobApplicant, Campaign.ContactDataSource);
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_SaveDefaultLayout()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			Factory.Save();
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Precondition", HRContactDataSourceList.Codes.Staff, Campaign.ContactDataSource);
				AssertEquals("Precondition: System Default Layout loaded (3 filters loaded from default layout + 1 always visible for Subscription status)", DefaultStaffFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				HRGlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterStripControl.FilterBusinessObject);
				AssertEquals("Precondition: System Default Layout loaded", DefaultStaffFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				var queryFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.StartsWith, ModuleIDs.GlbCompanyCampaignContact.Name).AddToFilter(StmModuleFilterSchema.S9_IsSystem, false);
				AssertNull(Factory.LoadTop1<StmModuleFilter>(queryFilter));
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
				AssertEquals("No layout created because last selected layout is already saved", 0, Factory.Load<StmModuleFilter>(queryFilter).Length);
				filterStripControl.FilterBusinessObject.LoadLayout(null);
				filterStripControl.SaveDefaultLayout();
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
				filterStripControl.FilterBusinessObject.LoadLayout(null);
				filterStripControl.SaveDefaultLayout();
				AssertEquals("Layout created because last selected layout was not saved", 2, Factory.Load<StmModuleFilter>(queryFilter).Length);
				var jobApplicantFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.JobApplicant);
				jobApplicantFilter.AddToFilter(queryFilter);
				AssertNotNull(Factory.LoadTop1<StmModuleFilter>(jobApplicantFilter));
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
				AssertEquals("Layout created because last selected layout was not saved", 2, Factory.Load<StmModuleFilter>(queryFilter).Length);
				var staffFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.Staff);
				staffFilter.AddToFilter(queryFilter);
				AssertNotNull(Factory.LoadTop1<StmModuleFilter>(staffFilter));
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_UpdateLayoutContext()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mocksChs = Res.UseMockData())
			{
				mocksChs.Put("HRContactDataSourceList|Staff", new ResourceStringData("HRContactDataSourceList|Staff", "Staff (Chinese - Simplified)"));
				AssertEquals("Precondition", "Staff (Chinese - Simplified)", HRContactDataSourceList.Codes.Staff);
				mocksChs.Put("HRContactDataSourceList|JobApplicant", new ResourceStringData("HRContactDataSourceList|JobAppplicant", "Job Applicant (Chinese - Simplified)"));
				AssertEquals("Precondition", "Job Applicant (Chinese - Simplified)", HRContactDataSourceList.Codes.JobApplicant);
				Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
				using (var form = new FormForTest(Campaign))
				{
					// ensure S9_ModuleIDs stay the same even when translations for the Contact Data Source list get added so existing layouts continue to be loaded
					form.Show();
					var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
					AssertEquals("Precondition", HRContactDataSourceList.Codes.Staff, Campaign.ContactDataSource);
					AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.Staff, ((IFilterStripBusinessObjectInternals)filterStripControl.FilterBusinessObject).LayoutContext);
					Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
					AssertEquals(ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.JobApplicant, ((IFilterStripBusinessObjectInternals)filterStripControl.FilterBusinessObject).LayoutContext);
				}
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_LoadDefaultLayoutAndHideItUnderFindButton()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			Factory.Save();
			var queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			var staffFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.Staff);
			staffFilter.AddToFilter(queryFilter);
			var applicantFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.JobApplicant);
			applicantFilter.AddToFilter(queryFilter);
			using (var form = new FormForTest(Campaign))
			{
				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Precondition", HRContactDataSourceList.Codes.Staff, Campaign.ContactDataSource);
				AssertEquals("Precondition: System Default Layout loaded (3 filters loaded from default layout + 1 always visible for Subscription status)", DefaultStaffFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				HRGlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterStripControl.FilterBusinessObject);
				AssertEquals("Precondition: System Default Layout loaded", DefaultStaffFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				filterStripControl.SaveDefaultLayout();
				filterStripControl.FilterBusinessObject.LoadLayout(null);
				// create last selected layouts by switching between data sources
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
				AssertEquals("Chosen filters are replaced with ones in System Default Layout (+ 1 always visible for Subscription status)", DefaultApplicantFilterCount + DefaultSubscriptionFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				HRGlbCompanyCampaignContactFilterBusinessObjectTestBase.SwitchOffSubscriptionFilter(filterStripControl.FilterBusinessObject);
				AssertEquals("Chosen filters are replaced with ones in System Default Layout", DefaultApplicantFilterCount, filterStripControl.FilterBusinessObject.ActiveModuleFilters.Count);
				filterStripControl.SaveDefaultLayout();
				filterStripControl.FilterBusinessObject.LoadLayout(null);
				// check these last selected layouts are loaded the next time the data source is selected
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
				var filter = Factory.LoadTop1<StmModuleFilter>(staffFilter);
				AssertNotNull(filter);
				AssertEquals("Saved last used layout loaded", filter.PK, filterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 2, filterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
				AssertEquals("System Default Layout", filterStripControl.ToolStripFindDropButtonExposed.DropDownItems[1].Text.Trim());
				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
				filter = Factory.LoadTop1<StmModuleFilter>(applicantFilter);
				AssertNotNull(filter);
				AssertEquals("Saved last used layout loaded", filter.PK, filterStripControl.FilterBusinessObject.LastUsedLayout.PK);
				AssertEquals("Saved last used layout not under Find Button", 0, filterStripControl.ToolStripFindDropButtonExposed.DropDownItems.Count);
			}
		}

		public void TestContactDataSourceInfo_ValueChanged_ReloadFilter()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			Factory.Save();

			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var form = new FormForTest(Campaign))
			{
				security.StaffViewOtherStaffDetails.IsAllowed = true;
				security.HRJobApplicantView.IsAllowed = false;

				form.Show();
				var filterStripControl = form.HRCampaignsControl.FilterStripControlExposed();
				AssertEquals("Precondition", HRContactDataSourceList.Codes.Staff, Campaign.ContactDataSource);

				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;

				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

				Campaign.ContactDataSource = HRContactDataSourceList.Codes.CampaignTracking;

				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

				security.StaffViewOtherStaffDetails.IsAllowed = false;
				security.HRJobApplicantView.IsAllowed = true;

				Campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;

				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);

				Campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;

				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.City]);
				AssertNotNull(HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State, filterStripControl.FilterBusinessObject[HRGlbCompanyCampaignContactFilterBusinessObject.HRFilterDescription.State]);
			}
		}

		public void TestSaveDefaultLayout()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			FilterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObjectForTest(Campaign);
			using (var control = new GlbCompanyCampaignContactFilterControl(null, FilterStripBusinessObject, Campaign))
			{
				var queryFilter = new ZQuery(StmModuleFilterSchema.S9_ModuleID, ModuleIDs.GlbCompanyCampaignContact.Name + "_" + HRContactDataSourceList.FilterModuleIDSuffixes.Staff).AddToFilter(StmModuleFilterSchema.S9_IsSystem, false);
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

		public void TestShouldPerformSearch_Touch()
		{
			var master = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch1a = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);
			var otherCampaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			Factory.Save();
			FilterStripBusinessObject = new HRGlbCompanyCampaignContactFilterBusinessObject(touch1a);
			using (var control = new HRGlbCompanyCampaignContactFilterControlForTest(FilterStripBusinessObject, touch1a))
			{
				AssertEquals(false, control.ShouldPerformSearch_Exposed());
				Assert("Precondition", touch1a.IsUsingCampaignTrackingDataSource);
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

		public void TestShouldPerformSearchSecurity_Staff()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			var staffWithoutStaffSourcePermissions = Factory.NewWithValidTestData<GlbStaff>();
			var staffWithStaffSourcePermissions = Factory.NewWithValidTestData<GlbStaff>();
			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.HRCampaignManagementEditFindByStaff.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutStaffSourcePermissions.PK;
			staffWithoutStaffSourcePermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);
			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.HRCampaignManagementEditFindByStaff.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithStaffSourcePermissions.PK;
			staffWithStaffSourcePermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);
			Factory.Save();
			var filterStrip = new HRGlbCompanyCampaignContactFilterBusinessObject(campaign);
			using (var control = new HRGlbCompanyCampaignContactFilterControlForTest(filterStrip, campaign))
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithStaffSourcePermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
					AssertEquals(true, control.ShouldPerformSearch_Exposed());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutStaffSourcePermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
					AssertEquals(false, control.ShouldPerformSearch_Exposed());
					AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestShouldPerformSearchSecurity_Applicants()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			var staffWithoutStaffSourcePermissions = Factory.NewWithValidTestData<GlbStaff>();
			var staffWithStaffSourcePermissions = Factory.NewWithValidTestData<GlbStaff>();
			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.HRCampaignManagementEditFindByApplicants.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutStaffSourcePermissions.PK;
			staffWithoutStaffSourcePermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);
			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.HRCampaignManagementEditFindByApplicants.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithStaffSourcePermissions.PK;
			staffWithStaffSourcePermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);
			var filterStrip = new HRGlbCompanyCampaignContactFilterBusinessObject(campaign);
			Factory.Save();
			using (var control = new HRGlbCompanyCampaignContactFilterControlForTest(filterStrip, campaign))
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithStaffSourcePermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
					AssertEquals(true, control.ShouldPerformSearch_Exposed());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutStaffSourcePermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
					AssertEquals(false, control.ShouldPerformSearch_Exposed());
					AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestLastUsedContactDataSource()
		{
			Campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			var filterStrip = new HRGlbCompanyCampaignContactFilterBusinessObject(Campaign);
			var data = Factory.NewWithValidTestData<StmData>();
			data.SD_DepartmentGuid = Env.Instance.CurrentCompany.PK;
			data.SD_Type = "CUR";
			data.SD_Owner = Env.CurrentUser.PK;
			data.SD_Name = "GlbCompanyCampaignContact_Staff";
			var data2 = Factory.NewWithValidTestData<StmData>();
			data2.SD_DepartmentGuid = Env.Instance.CurrentCompany.PK;
			data2.SD_Type = "";
			data2.SD_Owner = Campaign.PK;
			data2.SD_Name = "GlbCompanyCampaignContact_JobApplicant";
			Factory.Save();
			var result = HRCampaignContactFilterDataSourceHelper.GetCampaignContactDataSource(filterStrip.LastUsedCampaignContactLayoutFactory);
			AssertEquals("Staff", result);
			data.SD_Type = "";
			data2.SD_Type = "CUR";
			Factory.Save();
			result = HRCampaignContactFilterDataSourceHelper.GetCampaignContactDataSource(filterStrip.LastUsedCampaignContactLayoutFactory);
			AssertEquals("Job Applicant", result);
		}

		#region Implementation
		const int DefaultStaffFilterCount = 3;
		const int DefaultApplicantFilterCount = 0;
		const int DefaultSubscriptionFilterCount = 1;
		HRGlbCompanyCampaign Campaign;
		HRGlbCompanyCampaignContactFilterBusinessObject FilterStripBusinessObject;
		public class FormForTest : ZForm
		{
			public FormForTest(HRGlbCompanyCampaign businessEntity) : base(businessEntity)
			{
				HRCampaignsControl = new HRSendCampaignsControlForTest();
				Controls.Add(HRCampaignsControl);
				HRCampaignsControl.SetDataBinding(businessEntity, null);
			}

			public HRSendCampaignsControlForTest HRCampaignsControl;
		}

		public class HRGlbCompanyCampaignContactFilterControlForTest : HRGlbCompanyCampaignContactFilterControl
		{
			public HRGlbCompanyCampaignContactFilterControlForTest(HRGlbCompanyCampaignContactFilterBusinessObject strip, HRGlbCompanyCampaign campaign) : base(null, strip, campaign)
			{
			}

			public ZBool ShouldPerformSearch_Exposed()
			{
				return base.ShouldPerformSearch();
			}
		}

		public class HRGlbCompanyCampaignContactFilterBusinessObjectForTest : HRGlbCompanyCampaignContactFilterBusinessObject
		{
			public HRGlbCompanyCampaignContactFilterBusinessObjectForTest(HRGlbCompanyCampaign campaign) : base(campaign)
			{
			}

			public HRGlbCompanyCampaign CampaignExposed()
			{
				return (HRGlbCompanyCampaign)Campaign;
			}
		}

		public class HRSendCampaignsControlForTest : HRSendCampaignsControl
		{
			public HRGlbCompanyCampaignContactFilterControl FilterStripControlExposed()
			{
				return (HRGlbCompanyCampaignContactFilterControl)FilterStripControl;
			}

			public HRGlbCompanyCampaign DataSourceExposed()
			{
				return (HRGlbCompanyCampaign)DataSource;
			}
		}
		#endregion
	}
}
