using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	public class GlbCompanyCampaignValidationTest : BusinessObjectValidationTestCase
	{
		public void TestContactDataSource_NormalCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			campaign.G0_UseLastEmailSenderAddress = false;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);

			campaign.G0_UseLastEmailSenderAddress = true;

			AssertHasWarnings(campaign.ContactDataSourceInfo);
			AssertHasWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
		}

		public void TestG0_UseLastEmailSenderAddress_NormalCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			campaign.G0_UseLastEmailSenderAddress = true;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.ContactDataSourceInfo);

			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;

			AssertHasWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
			AssertHasWarnings(campaign.ContactDataSourceInfo);
		}

		public void TestContactDataSource_MasterCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			campaign.G0_UseLastEmailSenderAddress = false;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);

			campaign.G0_UseLastEmailSenderAddress = true;

			AssertHasWarnings(campaign.ContactDataSourceInfo);
			AssertHasWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
		}

		public void TestG0_UseLastEmailSenderAddress_MasterCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			campaign.G0_UseLastEmailSenderAddress = true;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.ContactDataSourceInfo);

			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;

			AssertHasWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
			AssertHasWarnings(campaign.ContactDataSourceInfo);
		}

		public void TestContactDataSource_TouchCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			campaign.G0_UseLastEmailSenderAddress = false;
			campaign.G0_G0_Master = master.PK;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);

			campaign.G0_UseLastEmailSenderAddress = true;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
		}

		public void TestG0_UseLastEmailSenderAddress_TouchCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			campaign.G0_UseLastEmailSenderAddress = false;
			campaign.G0_G0_Master = master.PK;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.ContactDataSourceInfo);

			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;

			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
			AssertNoWarnings(campaign.ContactDataSourceInfo);
		}

		public void TestG0_GroupRatio()
		{
			var other = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			other.G0_GroupRatio = 0;
			AssertNoErrors(other.G0_GroupRatioInfo);

			touch1a.G0_GroupRatio = 0;
			AssertNoErrors(touch1a.G0_GroupRatioInfo);

			touch1a.CurrentGroupColor = 1;

			touch1a.G0_GroupRatio = 101;
			AssertHasErrors(touch1a.G0_GroupRatioInfo);

			touch1a.G0_GroupRatio = 100;
			AssertNoErrors(touch1a.G0_GroupRatioInfo);
		}

		public void TestG0_DocumentBlob()
		{
			var other = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			touch1a.Validation.ValidateG0_DocumentBlob();
			AssertEquals(true, touch1a.G0_DocumentBlobInfo.HasErrors());

			master.Validation.ValidateG0_DocumentBlob();
			AssertEquals(false, master.G0_DocumentBlobInfo.HasErrors());

			other.Validation.ValidateG0_DocumentBlob();
			AssertEquals(false, other.G0_DocumentBlobInfo.HasErrors());
		}

		public void TestCheckG0_DocumentBlob_MandatoryForEmailTouchCampaigns()
		{
			var masterDRMCampaign = GetCampaignForTest();
			masterDRMCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var masterINSCampaign = GetCampaignForTest();
			masterINSCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			Factory.Save();

			foreach (var campaignType in masterDRMCampaign.EmailCampaigns)
			{
				var emailCampaign = GetCampaignForTest();
				emailCampaign.G0_BroadcastVoteSurveyExam = campaignType;
				emailCampaign.G0_HorizontalId = 1;
				emailCampaign.G0_VerticalId = "A";

				if (emailCampaign.IsPreApproachEmailCampaign)
				{
					masterINSCampaign.AllTouches.Add(emailCampaign);
				}
				else
				{
					masterDRMCampaign.AllTouches.Add(emailCampaign);
				}

				emailCampaign.Validation.ValidateG0_DocumentBlob();
				AssertHasError($"Empty Email Content for type {campaignType} should throw error", emailCampaign.G0_DocumentBlobInfo, "Please enter an Email content.");

				emailCampaign.Validation.ValidateIsDocumentAttached();
				if (campaignType == CampaignTypeList.Codes.Survey || campaignType == CampaignTypeList.Codes.Voting)
				{
					AssertHasError("It should report both non-empty and macro-url error together.", emailCampaign.IsDocumentAttachedInfo, string.Format("HTML document should include field (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName));
				}

				emailCampaign.HtmlDocumentBlob = ZBlob.FromAscii(CampaignEmailTemplateEditor.SkeletonHtml);
				AssertHasError($"Empty Email Content for type {campaignType} should throw error", emailCampaign.G0_DocumentBlobInfo, "Please enter an Email content.");

				emailCampaign.HtmlDocumentBlob = ZBlob.FromAscii("Test G0_DocumentBlob");
				Assert(!emailCampaign.G0_DocumentBlobInfo.HasErrors());
			}
		}

		public void TestCheckG0_DocumentBlob_MandatoryForScheduledCampaign()
		{
			var emailCampaignRequireContent = GetCampaignForTest();
			emailCampaignRequireContent.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			AssertNoError("Empty Email Content without Queue has no error", emailCampaignRequireContent.G0_DocumentBlobInfo, GlbCompanyCampaignValidation.EmailContentIsNotSetWhenScheduledText);

			var item = emailCampaignRequireContent.CampaignsItemsSent.AddNew();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			item.G8_ScheduleTimeUtc = DateTime.UtcNow.AddDays(1);
			emailCampaignRequireContent.HtmlDocumentBlob = ZBlob.FromAscii(string.Empty);

			emailCampaignRequireContent.Validation.ValidateG0_DocumentBlob();
			AssertHasError($"Empty Email Content with Queue should throw error", emailCampaignRequireContent.G0_DocumentBlobInfo, GlbCompanyCampaignValidation.EmailContentIsNotSetWhenScheduledText);

			var emailCampaignNotRequireContent = GetCampaignForTest();
			emailCampaignNotRequireContent.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			AssertNoError("Empty Email Content without Queue has no error", emailCampaignNotRequireContent.G0_DocumentBlobInfo, GlbCompanyCampaignValidation.EmailContentIsNotSetWhenScheduledText);

			var item2 = emailCampaignRequireContent.CampaignsItemsSent.AddNew();
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			item2.G8_ScheduleTimeUtc = DateTime.UtcNow.AddDays(1);
			emailCampaignNotRequireContent.HtmlDocumentBlob = ZBlob.FromAscii(string.Empty);

			emailCampaignNotRequireContent.Validation.ValidateG0_DocumentBlob();
			AssertNoError("EmailContentIsNotRequired type Empty Email Content with Queue has no error", emailCampaignNotRequireContent.G0_DocumentBlobInfo, GlbCompanyCampaignValidation.EmailContentIsNotSetWhenScheduledText);
		}

		public void TestCheckG0_DocumentBlob_MalformedMacros()
		{
			Campaign.FillWithValidTestData();
			AssertNoErrors(Campaign.G0_DocumentBlobInfo);

			Campaign.HtmlDocumentBlob = ZBlob.FromAscii("(*MalformedMacro");
			Campaign.Factory.Save();

			AssertHasError("Malformed Macro error should be present.", Campaign.G0_DocumentBlobInfo, GlbCompanyCampaignValidation.EmailContentMacrosMalformedErrorText);

			Campaign.HtmlDocumentBlob = ZBlob.FromAscii("(*ValidMacro*)");
			Campaign.Factory.Save();

			AssertNoErrors(Campaign.G0_DocumentBlobInfo);
		}

		public void TestCheckG0_DocumentBlob_MalformedMacros_TouchCampaigns()
		{
			var masterCampaign = GetCampaignForTest();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AssertNoErrors(masterCampaign.G0_DocumentBlobInfo);

			var touch = GetCampaignForTest();
			touch.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.HtmlDocumentBlob = ZBlob.FromAscii("(*MalformedMacro");

			masterCampaign.AllTouches.Add(touch);

			touch.Validation.ValidateG0_DocumentBlob();
			AssertHasError("Malformed Macro error should be present.", touch.G0_DocumentBlobInfo, GlbCompanyCampaignValidation.EmailContentMacrosMalformedErrorText);

			touch.HtmlDocumentBlob = ZBlob.FromAscii("(*ValidMacro*)");
			touch.Validation.ValidateG0_DocumentBlob();

			AssertNoErrors(touch.G0_DocumentBlobInfo);
		}

		protected virtual GlbCompanyCampaign GetCampaignForTest()
		{
			return Factory.NewWithValidTestData<GlbCompanyCampaign>();
		}

		public void TestG0_DocumentBlob_ValidationNotRunForLinkTrackingCampaign()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_Code = "COR";
			coordinator.GS_EmailAddress = "unit.test@cw1.com";

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "MGR";

			var parentCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			parentCampaign.G0_CampaignName = "Test Campaign Name";
			parentCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			parentCampaign.G0_Category = "PRINT";
			parentCampaign.G0_EstimatedStartedDate = ZDateTime.Today;
			parentCampaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			parentCampaign.G0_GS_NKCampaignManager = manager.GS_Code;
			parentCampaign.G0_Type = "PREAP";

			Factory.Save();

			var campaignTouch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaignTouch.G0_CampaignName = "Test Campaign Touch A";
			campaignTouch.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Broadcast;
			campaignTouch.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(5);
			campaignTouch.G0_HorizontalId = 1;
			campaignTouch.G0_VerticalId = "A";
			campaignTouch.G0_G0_Master = parentCampaign.PK;
			campaignTouch.G0_Category = "PRINT";
			campaignTouch.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaignTouch.G0_GS_NKCampaignManager = manager.GS_Code;
			campaignTouch.G0_Type = "PREAP";
			campaignTouch.SourceCampaignPK = parentCampaign.PK;
			parentCampaign.AllTouches.Add(campaignTouch);

			campaignTouch.Validation.ValidateAll();
			Assert("Campaign should have errors for G0_DocumentBlob", campaignTouch.G0_DocumentBlobInfo.HasErrors());

			campaignTouch.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.LinkTracking;
			Assert("G0_DocumentBlob shouldn't have errors.", !campaignTouch.G0_DocumentBlobInfo.HasErrors());
		}

		public void TestValidateTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			master.Validation.ValidateAll();
			AssertEquals(false, master.HasRowErrors);

			touch1a.TransitionRulesToThisCampaign.DeleteAll();
			master.Validation.ValidateAll();
			AssertEquals(true, master.HasRowErrors);
		}

		public void TestCheckCampaignID()
		{
			var categoryCollection = new CodeDescriptionBoolCollection();
			categoryCollection.Add("ABC", (NoResString)"Category1");
			categoryCollection.Add("DEF", (NoResString)"Category2");
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryCollection);

			var typeCollection = new CodeDescriptionBoolCollection();
			typeCollection.Add("UVW", (NoResString)"Type1");
			typeCollection.Add("XYZ", (NoResString)"Type2");
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, typeCollection);

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var testDate = ZDateTime.Today;

			GlbCompanyCampaign campaign1 = Factory.New<GlbCompanyCampaign>();
			campaign1.G0_BroadcastVoteSurveyExam = "BRD";
			campaign1.G0_Category = "ABC";
			campaign1.G0_Type = "XYZ";
			campaign1.G0_CampaignName = "John's 1st Company";
			campaign1.G0_GS_NKCampaignManager = manager.GS_Code;
			campaign1.G0_GS_NKCampaignCoordinator = manager.GS_Code;
			campaign1.G0_EstimatedStartedDate = testDate;
			AssertNoErrors(campaign1.CampaignIDInfo);

			Factory.Save();

			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			campaign2.G0_BroadcastVoteSurveyExam = "BRD";
			campaign2.G0_Category = "ABC";
			campaign2.G0_Type = "XYZ";
			campaign2.G0_CampaignName = "John's 2nd Company";
			campaign2.G0_GS_NKCampaignManager = manager.GS_Code;
			campaign2.G0_GS_NKCampaignCoordinator = manager.GS_Code;
			campaign2.G0_EstimatedStartedDate = testDate;
			AssertNoErrors(campaign2.CampaignIDInfo);

			CombineAssertions("Test with Campaign Name", () =>
			{
				campaign2.G0_CampaignName = "John's 1st Company";
				AssertHasError(campaign2.CampaignIDInfo, "A campaign already exists with the same Name, Estimated Start Date, Media Type and Media Category. Please ensure you do not enter duplicate campaigns.");

				campaign2.G0_CampaignName = "John's 2nd Company";
				AssertNoErrors(campaign2.CampaignIDInfo);
			});

			CombineAssertions("Test with Media Type", () =>
			{
				campaign2.G0_CampaignName = "John's 1st Company";
				AssertHasError(campaign2.CampaignIDInfo, "A campaign already exists with the same Name, Estimated Start Date, Media Type and Media Category. Please ensure you do not enter duplicate campaigns.");

				campaign2.G0_Type = "UVW";
				AssertNoErrors(campaign2.CampaignIDInfo);
			});

			CombineAssertions("Test with Media Category", () =>
			{
				campaign2.G0_Type = "XYZ";
				AssertHasError(campaign2.CampaignIDInfo, "A campaign already exists with the same Name, Estimated Start Date, Media Type and Media Category. Please ensure you do not enter duplicate campaigns.");

				campaign2.G0_Category = "DEF";
				AssertNoErrors(campaign2.CampaignIDInfo);
			});

			CombineAssertions("Test with Estimated Start Date", () =>
			{
				campaign2.G0_Category = "ABC";
				AssertHasError(campaign2.CampaignIDInfo, "A campaign already exists with the same Name, Estimated Start Date, Media Type and Media Category. Please ensure you do not enter duplicate campaigns.");

				campaign2.G0_EstimatedStartedDate = new ZDateTime("2020-02-02");
				AssertNoErrors(campaign2.CampaignIDInfo);
			});

			campaign1.Delete();
			campaign2.G0_EstimatedStartedDate = testDate;
			AssertNoErrors(campaign2.CampaignIDInfo);
		}

		public void TestValidateBroadcastVoteSurveyExam()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = "";
			AssertMandatoryValidationError(campaign.G0_BroadcastVoteSurveyExamInfo, true);

			campaign.G0_BroadcastVoteSurveyExam = "meh";
			AssertMandatoryValidationError(campaign.G0_BroadcastVoteSurveyExamInfo, false);
			AssertListValidationInvalidCodeError(campaign.G0_BroadcastVoteSurveyExamInfo, true);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertNoErrors(campaign.G0_BroadcastVoteSurveyExamInfo);
		}

		public void TestValidateBroadcastVoteSurveyExam_IsDripMarketingMasterCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			AssertHasErrors(touch.G0_BroadcastVoteSurveyExamInfo);

			touch.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.TargetList;
			AssertNoErrors(touch.G0_BroadcastVoteSurveyExamInfo);

			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			AssertHasErrors(touch.G0_BroadcastVoteSurveyExamInfo);

			touch.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Survey;
			AssertNoErrors(touch.G0_BroadcastVoteSurveyExamInfo);
		}

		public void TestValidateBroadcastVoteSurveyExam_IsInsideSalesMasterCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = master.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.TargetList;
			AssertHasErrors(touch.G0_BroadcastVoteSurveyExamInfo);

			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			AssertNoErrors(touch.G0_BroadcastVoteSurveyExamInfo);

			touch.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Survey;
			AssertHasErrors(touch.G0_BroadcastVoteSurveyExamInfo);

			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			AssertNoErrors(touch.G0_BroadcastVoteSurveyExamInfo);
		}

		public void TestCampaignCurrency()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_RX_NKCampaignCurrency = "";

			campaign.Validation.ValidateAll();
			AssertNoErrors(campaign.G0_RX_NKCampaignCurrencyInfo);

			campaign.G0_RX_NKCampaignCurrency = "ZZ1";
			AssertHasErrors(campaign.G0_RX_NKCampaignCurrencyInfo);

			campaign.G0_RX_NKCampaignCurrency = "USD";
			AssertNoErrors(campaign.G0_RX_NKCampaignCurrencyInfo);
		}

		public void TestEstimatedStartDateValidation()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();

			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			AssertNoErrors(campaign.G0_EstimatedStartedDateInfo);

			campaign.G0_EstimatedStartedDate = ZDateTime.Empty;
			AssertHasErrors(campaign.G0_EstimatedStartedDateInfo);
		}

		public void TestCheckG0_GS_NKCampaignManager()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaign.G0_GS_NKCampaignManager = manager.GS_Code;
			AssertNoErrors("Staff without email address is OK for manager", campaign.G0_GS_NKCampaignManagerInfo);

			campaign.G0_GS_NKCampaignManager = ZString.Empty;
			AssertHasErrors("Campaign Manager is mandatory", campaign.G0_GS_NKCampaignManagerInfo);

			Factory.Save();

			string managerCode = manager.GS_Code;
			manager.Delete();
			Factory.Save();

			campaign.G0_GS_NKCampaignManager = managerCode;
			AssertHasErrors("Manager has been removed, invalid staff code", campaign.G0_GS_NKCampaignManagerInfo);
		}

		public void TestCheckG0_GS_NKCampaignCoordinator()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = ZString.Empty;
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			AssertHasErrors("Staff without email address", campaign.G0_GS_NKCampaignCoordinatorInfo);

			staff.GS_EmailAddress = "Frog";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			AssertHasErrors("Staff without valid email address", campaign.G0_GS_NKCampaignCoordinatorInfo);

			staff.GS_EmailAddress = "Frog@email.com";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			AssertNoErrors("Staff with valid email", campaign.G0_GS_NKCampaignCoordinatorInfo);

			campaign.G0_GS_NKCampaignCoordinator = ZString.Empty;
			AssertHasErrors("Campaign Coordinator is mandatory", campaign.G0_GS_NKCampaignCoordinatorInfo);

			Factory.Save();

			string staffCode = staff.GS_Code;
			staff.Delete();
			Factory.Save();

			campaign.G0_GS_NKCampaignCoordinator = staffCode;
			AssertHasErrors("Coordinator has been removed, invalid staff code", campaign.G0_GS_NKCampaignCoordinatorInfo);
		}

		public void TestCheckG0_GS_NKCampaignCoordinator_isNDR()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = ZString.Empty;
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			staff.GS_EmailAddress = "Frog@email.com";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			AssertNoWarnings("Staff with valid email", campaign.G0_GS_NKCampaignCoordinatorInfo);

			staff.EmailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main).IsNDR = true;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			AssertHasWarnings("Email should not be marked as an NDR", campaign.G0_GS_NKCampaignCoordinatorInfo);

			staff.EmailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main).IsNDR = false;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			AssertNoWarnings("Email not marked as an NDR (Valid Email)", campaign.G0_GS_NKCampaignCoordinatorInfo);
		}

		public void TestCheckG0_EmailSubject()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EmailSubject = ZString.Empty;
			AssertHasErrors("Email subject cannot be empty", campaign.G0_EmailSubjectInfo);
			campaign.G0_EmailSubject = "dsfsdffdsfs";
			AssertNoErrors("Email subject entered", campaign.G0_EmailSubjectInfo);
		}

		public void TestCheckG0_EmailSubject_MalformedMacro()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EmailSubject = "Test Email Subject with (*ValidMacro*)";
			AssertNoErrors("Email subject entered", campaign.G0_EmailSubjectInfo);

			campaign.G0_EmailSubject = "Test Email Subject with (*MalformedMacro";
			AssertHasError("Malformed Macro error should be present.", campaign.G0_EmailSubjectInfo, GlbCompanyCampaignValidation.EmailSubjectMacrosMalformedErrorText);
		}

		public void TestCheckG0_EmailSenderOption()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = "";
			AssertHasErrors("Email sender option cannot be empty", campaign.G0_EmailSenderOptionInfo);
			campaign.G0_EmailSenderOption = "UAD";
			AssertHasErrors("The value entered is invalid", campaign.G0_EmailSenderOptionInfo);
			campaign.G0_EmailSenderOption = "COR";
			AssertNoErrors("Email sender role entered correctly", campaign.G0_EmailSenderOptionInfo);
		}

		public void TestCheckG0_EmailSenderOptionEmptySenderPool()
		{
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertHasErrors("Empty pool", Campaign.G0_EmailSenderOptionInfo);
			Campaign.Validation.ValidateAll();
			AssertHasErrors("Empty pool", Campaign.G0_EmailSenderOptionInfo);

			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertNoErrors(Campaign.G0_EmailSenderOptionInfo);

			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertHasErrors("Empty pool", Campaign.G0_EmailSenderOptionInfo);
		}

		public void TestCheckG0_EmailSenderOptionWithBrokenPool()
		{
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			var poolItem = Campaign.SenderPool.AddNew();
			Campaign.Validation.ValidateSenderPool();
			AssertHasErrors("Broken pool", Campaign.G0_EmailSenderOptionInfo);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "e@ma.il";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			poolItem.GCP_GS_NKSender = staff.GS_Code;
			Campaign.Validation.ValidateSenderPool();
			AssertNoErrors("Working pool", Campaign.G0_EmailSenderOptionInfo);
		}

		public void TestCheckG0_EmailSenderOptionEmptySenderPoolThenAddValues()
		{
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertHasErrors("Empty pool", Campaign.G0_EmailSenderOptionInfo);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			glbStaff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			var poolItem = Campaign.SenderPool.AddNew();
			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;

			Campaign.Validation.ValidateAll();
			AssertNoErrors(Campaign.G0_EmailSenderOptionInfo);
		}

		public void TestCheckG0_EmailSenderOptionFillSenderPoolBeforeSelect()
		{
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertNoErrors(Campaign.G0_EmailSenderOptionInfo);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			glbStaff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			var poolItem = Campaign.SenderPool.AddNew();
			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;

			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertNoErrors(Campaign.G0_EmailSenderOptionInfo);
			Campaign.Validation.ValidateAll();
			AssertNoErrors(Campaign.G0_EmailSenderOptionInfo);
		}

		public void TestCheckG0_EmailSenderRole()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = ZString.Empty;
			AssertHasErrors("Email sender role cannot be empty if G0_EmailSenderOption is ORG", campaign.G0_EmailSenderRoleInfo);
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "VVV";
			AssertHasErrors("Email sender role has an invalid value", campaign.G0_EmailSenderRoleInfo);
			campaign.G0_EmailSenderRole = "ACT";
			AssertNoErrors("Email sender role entered accordingly", campaign.G0_EmailSenderRoleInfo);
		}

		public void TestCheckSenderEmailAddress()
		{
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			Campaign.SenderEmailAddress = "xrm@cargowise.com";
			AssertNoErrors("There should be no errors", Campaign.SenderEmailAddressInfo);
			Campaign.SenderEmailAddress = "";
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertHasErrors("The sender's email address is invalid.", Campaign.SenderEmailAddressInfo);
			Campaign.SenderEmailAddress = "xrm@cargowise.com";
			AssertNoErrors(Campaign.SenderEmailAddressInfo);
			AssertHasNotifications("The email address entered does not match the SMTP Server configured. Emails sent from this address may be rejected.", Campaign.SenderEmailAddressInfo);
		}

		public void TestCheckG0_ReplyToEmail()
		{
			Campaign.UseEmailSenderAddressAsReplyTo = true;
			AssertNoErrors(Campaign.G0_ReplyToEmailInfo);
			Campaign.UseEmailSenderAddressAsReplyTo = false;
			Campaign.G0_ReplyToEmail = "";
			AssertHasErrors(Campaign.G0_ReplyToEmailInfo);
			Campaign.G0_ReplyToEmail = "xrm@cargowise.com";
			AssertNoErrors(Campaign.G0_ReplyToEmailInfo);
		}

		public void TestCheckG0_SenderEmail()
		{
			string server = Env.Registry.SMTPServer;

			Campaign.G0_SenderEmail = "xrm@ggggmail.com";
			AssertHasWarning(Campaign.G0_SenderEmailInfo, "The email address entered does not match the SMTP Server configured. Emails sent from this address may be rejected.");

			Campaign.G0_SenderEmail = "xrm@" + server;
			AssertNoWarning(Campaign.G0_SenderEmailInfo, "The email address entered does not match the SMTP Server configured. Emails sent from this address may be rejected.");
		}

		public void TestBatchCount()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BatchCountDefault = 9;
			AssertNoErrors("There should be no errors when batch count is greater than 0", campaign.G0_BatchCountDefaultInfo);

			campaign.G0_BatchCountDefault = 0;
			AssertHasErrors("There should be an error when batch count is less than 1", campaign.G0_BatchCountDefaultInfo);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.LinkTracking;
			campaign.Validation.ValidateG0_BatchCountDefault();
			AssertNoErrors("LNK campaign should have no errors for batch count", campaign.G0_BatchCountDefaultInfo);
		}

		public void TestBatchCount_TouchCampaign()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BatchCountDefault = 0;
			AssertHasErrors("Precondition: There should be an error when batch count is less than 1", campaign.G0_BatchCountDefaultInfo);

			var masterCampaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_G0_Master = masterCampaign.PK;
			campaign.Validation.ValidateG0_BatchCountDefault();
			AssertNoErrors("Touch campaign should have no errors for batch count", campaign.G0_BatchCountDefaultInfo);
		}

		public void TestCheckG0_Type()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("AAA", (NoResString)"Category A", true);
			collection.Add("BBB", (NoResString)"Category B", false);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_IsSalesAndMarketing = true;
			campaign.G0_Type = "AAA";
			AssertNoErrors("Campaign Category is valid, should not have errors", campaign.G0_TypeInfo);
			campaign.G0_Type = "CCC";
			AssertHasErrors("Campaign Category is NOT valid, should have errors", campaign.G0_TypeInfo);
			campaign.G0_Type = ZString.Empty;
			AssertHasErrors("Campaign Category should be mandatory", campaign.G0_TypeInfo);

			campaign.G0_Type = "BBB";
			Factory.Save();
			campaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			campaign.Validation.ValidateG0_Type();
			AssertNoErrors("Campaign is saved, should not have errors", campaign.G0_TypeInfo);
			AssertHasWarnings("Campaign Category is inactive, should have warning", campaign.G0_TypeInfo);

			campaign.G0_Type = "AAA";
			campaign.Factory.Save();
			campaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			campaign.G0_Type = "BBB";
			campaign.Validation.ValidateG0_Type();
			AssertHasErrors("Campaign Category is NOT active, should have errors", campaign.G0_TypeInfo);

			campaign.Factory.Save();
			collection = new CodeDescriptionBoolCollection();
			collection.Add("AAA", (NoResString)"Category A", true);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			campaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			campaign.Validation.ValidateG0_Type();
			AssertHasErrors("Campaign Category is NOT valid, should have errors", campaign.G0_TypeInfo);
		}

		public void TestCheckG0_Category()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("***", (NoResString)"Category");
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_Category = "***";
			AssertNoErrors("Campaign Type is valid, should not have errors", campaign.G0_CategoryInfo);
			campaign.G0_Category = ";;;";
			AssertHasErrors("Campaign Type is NOT valid, should have errors", campaign.G0_CategoryInfo);
			campaign.G0_Category = ZString.Empty;
			AssertHasErrors("Campaign Category should be mandatory", campaign.G0_CategoryInfo);
		}

		public void TestG0_ActualCompletedDate()
		{
			ZDateTime refDate = ZDateTime.Today;
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_ActualStartedDate = refDate;
			campaign.G0_ActualCompletedDate = refDate.AddDays(2);

			AssertNoErrors("Completion date after start date", campaign.G0_ActualCompletedDateInfo);

			campaign.G0_ActualStartedDate = refDate.AddDays(4);
			campaign.G0_ActualCompletedDate = refDate.AddDays(2);
			AssertHasErrors("Completion date before start date", campaign.G0_ActualCompletedDateInfo);

			campaign.G0_ActualStartedDate = ZDateTime.Invalid;

			campaign.G0_ActualCompletedDate = refDate;
			AssertNoErrors("start date invalid, completed date range not validated", campaign.G0_ActualCompletedDateInfo);

			campaign.G0_ActualStartedDate = ZDateTime.Empty;
			campaign.G0_ActualCompletedDate = refDate;
			AssertNoErrors("start date empty, completed date range not validated", campaign.G0_ActualCompletedDateInfo);

			campaign.G0_ActualStartedDate = refDate;
			campaign.G0_ActualCompletedDate = refDate;
			AssertNoErrors("start date same as end date - this is ok", campaign.G0_ActualCompletedDateInfo);
		}

		public void TestG0_CampaignName()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = ZString.Empty;
			AssertHasErrors("Campaign name should be mandatory", campaign.G0_CampaignNameInfo);

			campaign.G0_CampaignName = "blob";
			AssertNoErrors("Campaign name entered, should not have errors", campaign.G0_CampaignNameInfo);
		}

		public void TestCheckG0_Stage()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("***", "Campaign Status");
			OrganisationsDataRegistry.Instance.CampaignStageList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_Stage = "***";
			AssertNoErrors("Campaign Status is valid, should not have errors", campaign.G0_StageInfo);
			campaign.G0_Stage = ";;;";
			AssertHasErrors("Campaign Status is NOT valid, should have errors", campaign.G0_StageInfo);
		}

		public void TestValidateG0_QuestionsPerWebPage()
		{
			AssertNoErrors("Pre-condition", Campaign.G0_QuestionsPerWebPageInfo);

			Campaign.G0_QuestionsPerWebPage = 0;

			Campaign.Validation.ValidateG0_QuestionsPerWebPage();
			AssertNoErrors("Should not validate Broadcast campaign", Campaign.G0_QuestionsPerWebPageInfo);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Campaign.Validation.ValidateG0_QuestionsPerWebPage();
			AssertHasErrors("less than the min allowable value", Campaign.G0_QuestionsPerWebPageInfo);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.Validation.ValidateG0_QuestionsPerWebPage();
			AssertHasErrors("less than the min allowable value", Campaign.G0_QuestionsPerWebPageInfo);

			Campaign.G0_QuestionsPerWebPage = 1;
			AssertNoErrors("Within range", Campaign.G0_QuestionsPerWebPageInfo);

			Campaign.G0_QuestionsPerWebPage = 1000;
			AssertHasErrors("greater than max allowable value", Campaign.G0_QuestionsPerWebPageInfo);

			Campaign.G0_QuestionsPerWebPage = 999;
			AssertNoErrors("Within range", Campaign.G0_QuestionsPerWebPageInfo);
		}

		public void TestValidateDocumentAttachedMessage_Voting()
		{
			TestValidateDocumentAttachedMessage(CampaignTypeList.Codes.Voting);
		}

		public void TestValidateDocumentAttachedMessage_Survey()
		{
			TestValidateDocumentAttachedMessage(CampaignTypeList.Codes.Survey);
		}

		void TestValidateDocumentAttachedMessage(string campaignType)
		{
			Campaign.Validation.ValidateIsDocumentAttached();
			AssertNoErrors("Doesn't require CampaignURL, this should not be validated", Campaign.IsDocumentAttachedInfo);

			Campaign.G0_BroadcastVoteSurveyExam = campaignType;
			Campaign.Validation.ValidateIsDocumentAttached();
			AssertNoErrors("DocumentBlob has not been assigned, this should not be validated", Campaign.IsDocumentAttachedInfo);

			Campaign.HtmlDocumentBlob = ZBlob.FromAscii("MEH MEH");
			string expectedErrorMessage = string.Format("HTML document should include field (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName);
			AssertHasError("CampaignURL field should be included", Campaign.IsDocumentAttachedInfo, expectedErrorMessage);

			Campaign.HtmlDocumentBlob = ZBlob.FromAscii(string.Format("MEH MEH (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName));
			AssertNoErrors(Campaign.IsDocumentAttachedInfo);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			Campaign.Validation.ValidateIsDocumentAttached();
			AssertNoErrors("DocumentBlob has not been assigned, this should not be validated", Campaign.IsDocumentAttachedInfo);
		}

		public void TestIsCampaignURLFieldMissing()
		{
			Campaign.HtmlDocumentBlob = ZBlob.FromAscii("MEH MEH");
			var result = Campaign.Validation.IsCampaignURLFieldMissing(out var message);

			string expectedErrorMessage = string.Format("HTML document should include field (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName);
			AssertEquals("CampaignURL field should be included", message, expectedErrorMessage);
			Assert("CampaignURL field should be included", result);

			Campaign.HtmlDocumentBlob = ZBlob.FromAscii(string.Format("MEH MEH (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName));
			var result2 = Campaign.Validation.IsCampaignURLFieldMissing(out var message2);

			AssertEquals("CampaignURL field should be included", message2, string.Empty);
			Assert("CampaignURL field should be included", !result2);
		}

		public void TestValidateInvalidCoordinatorEmailAddressMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "john@gmail.com";
			staff.GS_FullName = "My full name";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GS_NKCampaignCoordinator = "TST";

			var emailAddress1 = staff.EmailAddresses.AddNew();
			emailAddress1.GSE_EmailAddress = "test@gmail.com";
			emailAddress1.GSE_Type = "FIR";

			var emailAddress2 = staff.EmailAddresses.AddNew();
			emailAddress2.GSE_EmailAddress = "test2@gmail.com";
			emailAddress2.GSE_Type = "SEC";

			Factory.Save();

			campaign.CoordinatorEmailAddress = emailAddress1.GSE_EmailAddress;
			AssertNoErrors(campaign.CoordinatorEmailAddressInfo);

			campaign.CoordinatorEmailAddress = emailAddress2.GSE_EmailAddress;
			AssertNoErrors(campaign.CoordinatorEmailAddressInfo);

			campaign.CoordinatorEmailAddress = "NotInEmailList@gmail.com";
			AssertHasError("Email should be chosen from the coordinator's email addresses", campaign.CoordinatorEmailAddressInfo, "Invalid Coordinator Email address.");
		}

		public void TestValidateContactDataSource()
		{
			Campaign.ContactDataSource = "AAA";
			Campaign.Validation.ValidateContactDataSource();
			AssertEquals("Precondition", Campaign.ContactDataSource, ContactDataSourceList.Codes.ClientIntelligence);
			AssertNoErrors("Value becomes valid, should not have errors", Campaign.ContactDataSourceInfo);

			Campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			Campaign.Validation.ValidateContactDataSource();
			AssertEquals("Precondition", Campaign.ContactDataSource, ContactDataSourceList.Codes.Inquiries);
			AssertNoErrors("Value is valid, should not have errors", Campaign.ContactDataSourceInfo);
		}

		public void TestCheckSourceCampaignPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";

			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign3 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignUnrelated = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			Factory.Save();

			campaign2.RelatedParentActivityPivotCollection.AddActivity(campaign1);
			campaign3.RelatedParentActivityPivotCollection.AddActivity(campaign2);

			Factory.Save();

			campaign2.SourceCampaignPK = campaignUnrelated.PK;
			AssertNoErrors("No relations between campaign2 and campaignUnrelated", campaign2.SourceCampaignPKInfo);

			campaign2.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Assert("Precondition", campaign2.IsUsingCampaignTrackingDataSource);
			campaign2.SourceCampaignPK = campaign2.PK;
			AssertHasErrors("campaign2.SourceCampaignPK shouldn't refer to itself", campaign2.SourceCampaignPKInfo);

			campaign2.SourceCampaignPK = campaign1.PK;
			AssertNoErrors("campaign1 can still be be used as campaign2's Source Campaign", campaign2.SourceCampaignPKInfo);

			campaign2.SourceCampaignPK = campaign3.PK;
			AssertHasErrors("campaign3 is a follow up of campaign2 -- inconsistent relations could be created if proceeded", campaign2.SourceCampaignPKInfo);

			campaign1.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Assert("Precondition", campaign1.IsUsingCampaignTrackingDataSource);
			campaign1.SourceCampaignPK = campaign3.PK;
			AssertHasErrors("campaign3 is a follow up of campaign2, which is a follow up of campaign1 -- inconsistent relations could be created if proceeded", campaign1.SourceCampaignPKInfo);
		}

		public void TestCheckTouchSourceCampaignPKsForValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			touch1a.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			touch1b.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			master.AllTouches.Add(touch1b);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);

			var campaignUnrelated = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			Factory.Save();

			master.TouchSourceCampaignPKs = new[] { campaignUnrelated.PK };
			AssertEquals("Master Campaign is not validated", false, master.HasRowErrors);

			touch1a.TouchSourceCampaignPKs = new[] { ZGuid.Invalid };
			AssertEquals("Error for invalid guid", true, touch1a.HasRowErrors);

			touch1a.TouchSourceCampaignPKs = Array.Empty<ZGuid>();
			AssertEquals("Error for empty", true, touch1a.HasRowErrors);

			touch1a.TouchSourceCampaignPKs = null;
			AssertEquals("Error for empty", true, touch1a.HasRowErrors);

			touch1a.TouchSourceCampaignPKs = new[] { master.PK };
			AssertEquals("No errors", false, touch1a.HasRowErrors);

			touch1b.TouchSourceCampaignPKs = new[] { master.PK, campaignUnrelated.PK };
			AssertEquals("Error for source outside of drip campaign", true, touch1b.HasRowErrors);

			touch1b.TouchSourceCampaignPKs = new[] { master.PK };
			AssertEquals("No errors", false, touch1b.HasRowErrors);

			touch2a.TouchSourceCampaignPKs = new[] { touch1a.PK, touch2a.PK };
			AssertEquals("Cannot have itself as a source", true, touch2a.HasRowErrors);

			touch2a.TouchSourceCampaignPKs = new[] { touch1a.PK, touch1b.PK };
			AssertEquals("No errors", false, touch2a.HasRowErrors);
		}

		public void TestValidateSendScheduledCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1a, Factory);

			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			touch1a.G0_Stage = "AA";
			master.AllTouches.Add(touch1a);

			Factory.Save();

			touch1a.Validation.ValidateSendSchedule();
			AssertEquals(1, touch1a.NotificationsIncludingChildren.Count());
			AssertHasError(touch1a.G0_StageInfo, "Enter a valid Stage.");

			touch1a.Validation.ValidateAll();
			AssertEquals(3, touch1a.NotificationsIncludingChildren.Count());

			AssertHasError(touch1a.TouchSourceCampaignPKsForValidationInfo, "Enter a valid Source Campaigns.");
			AssertHasRowError(touch1a, "Enter a valid Source Campaigns.");
			AssertHasError(touch1a.G0_StageInfo, "Enter a valid Stage.");
		}

		public void TestOpportunityCreationTemplateValidateAll()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			campaign.Validation.ValidateAll();

			var registryValue = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;

			AssertEquals(registryValue.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_OpportunityDescription), campaign.OpportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());
			AssertEquals(registryValue.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_PackageType), campaign.OpportunityCreationTemplate.PackageTypeInfo.HasErrors());
			AssertEquals(registryValue.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_GS_NKPrimarySalesPerson), campaign.OpportunityCreationTemplate.SalesPersonInfo.HasErrors());
			AssertEquals(registryValue.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_Source), campaign.OpportunityCreationTemplate.SourceInfo.HasErrors());
			Assert(campaign.OpportunityCreationTemplate.OpportunityTypeInfo.HasErrors());
			Assert(campaign.OpportunityCreationTemplate.OpportunityStageInfo.HasErrors());
			Assert(campaign.OpportunityCreationTemplate.OpportunityStatusInfo.HasErrors());
			Assert(campaign.OpportunityCreationTemplate.OpportunityAssignmentInfo.HasErrors());
		}

		public void TestOpportunityCreationTemplateValidateG0_DocumentBlob()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			campaign.Validation.ValidateAll();
			Assert(!campaign.G0_DocumentBlobInfo.HasErrors());
		}

		public void TestCheckG0_DocumentBlob_NoErrorWithoutEmbeddedImages()
		{
			AssertNoErrors(Campaign.G0_DocumentBlobInfo);

			Campaign.HtmlDocumentBlob = ZBlob.FromAscii(@"
<html><head><meta charset=""utf - 8""></head><body>
	<img src = ""http://somewhere/logo.svg"" embedded = ""false"" > &nbsp;
</body ></html>");
			AssertNoErrors(Campaign.G0_DocumentBlobInfo);
		}

		public void TestCheckG0_DocumentBlob_NoErrorWithEmbeddedImages()
		{
			using (var tempDirectory = new TempDirectory())
			using (PrepareTestFiles(tempDirectory, "file1.jpg", "file2.jpg"))
			{
				Campaign.SetDocumentData(ZBlob.FromAscii($@"
<html><head><meta charset=""utf - 8""></head><body>
	<img src = ""http://somewhere/logo.svg"" embedded = ""false"" > &nbsp;
	<img src = ""{Path.Combine(tempDirectory.DirectoryName, "file1")}.jpg"" embedded = ""true"" > &nbsp;
	<img src = ""{Path.Combine(tempDirectory.DirectoryName, "file2")}.jpg"" embedded = ""true"" > &nbsp;
</body ></html>"));
				AssertNoErrors(Campaign.G0_DocumentBlobInfo);
			}
		}

		static IDisposable PrepareTestFiles(TempDirectory embeddedImagesDirectory, params string[] fileNames)
		{
			fileNames
				.Select((fileName, fileNumber) => new { fileName, fileNumber = fileNumber + 1 })
				.Where(file => !string.IsNullOrEmpty(file.fileName))
				.ForEach(file => File.WriteAllBytes(Path.Combine(embeddedImagesDirectory.DirectoryName, file.fileName), new byte[400000 * file.fileNumber]));

			return new DisposableAction(() =>
			{
				fileNames.Where(fileName => !string.IsNullOrEmpty(fileName) && File.Exists(fileName)).ForEach(fileName => File.Delete(fileName));
			});
		}

		GlbCompanyCampaign Campaign
		{
			get { return fCampaign ?? (fCampaign = Factory.New<GlbCompanyCampaign>()); }
		}
		GlbCompanyCampaign fCampaign;
	}
}
