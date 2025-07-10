using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TouchSummaryViewModelTest : TestCaseWithFactory
	{
		public void TestHasDataContext()
		{
			var viewModel = new TouchSummaryViewModel();

			AssertEquals(false, viewModel.HasDataContext);

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "test name";
			viewModel.SetDataSource(campaign);

			AssertEquals(true, viewModel.HasDataContext);
		}

		public void TestCampaignName()
		{
			var viewModel = new TouchSummaryViewModel();
			AssertNoExceptionThrown(delegate
			{
				var x = viewModel.CampaignName;
			});

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "test name";
			viewModel.SetDataSource(campaign);

			AssertEquals("test name", viewModel.CampaignName);
		}

		public void TestAccessMasterCampaign()
		{
			var viewModel = new TouchSummaryViewModel();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "test name";

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			viewModel.SetDataSource(master);
			var accessMaster = viewModel.MasterCampaign;
			AssertEquals("test name", accessMaster.G0_CampaignName);
			AssertEquals(CampaignTypeList.Codes.DripMarketing, accessMaster.G0_BroadcastVoteSurveyExam);
			AssertEquals(1, accessMaster.AllTouches.Count);
		}

		public void TestHorizontals()
		{
			var viewModel = new TouchSummaryViewModel();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);

			viewModel.SetDataSource(master);

			AssertEquals(2, viewModel.Horizontals.Count);
			AssertEquals((byte)1, viewModel.Horizontals[0].Id);
			AssertEquals(2, viewModel.Horizontals[0].Campaigns.Count);
			AssertEquals((byte)2, viewModel.Horizontals[1].Id);
			AssertEquals(1, viewModel.Horizontals[1].Campaigns.Count);
		}

		public void TestAddHorizontal()
		{
			var viewModel = new TouchSummaryViewModel();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			viewModel.SetDataSource(master);

			AssertEquals(0, viewModel.Horizontals.Count);
			AssertEquals(0, master.Horizontals.Count());

			viewModel.AddHorizontal();

			AssertEquals("Should save before adding horizontals", 0, viewModel.Horizontals.Count);
			AssertEquals("Should save before adding horizontals", 0, master.Horizontals.Count());

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "CMS";
			staff.GS_EmailAddress = "cms@test.org";

			Factory.Save();

			viewModel.AddHorizontal();

			var form = viewModel.LastController.LastShownForm as GlbCompanyCampaignForm;
			var campaign = form.BusinessEntity as GlbCompanyCampaign;
			campaign.G0_CampaignName = "touch";
			campaign.HtmlDocumentBlob = new ZBlob(new byte[] { 1, 1, 1, 1 });
			campaign.G0_Category = "PRINT";
			campaign.G0_Type = "EXIST";
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			form.FireSaveButton();

			AssertEquals(1, viewModel.Horizontals.Count);
			AssertEquals(1, master.Horizontals.Count());

			form.Close();
			form.Dispose();
			viewModel.LastController = null;
		}

		public void TestRemoveCampaign()
		{
			var viewModel = new TouchSummaryViewModel();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var touch1bPK = touch1b.PK;
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			viewModel.SetDataSource(master);
			AssertEquals(1, viewModel.Horizontals.Count);
			AssertEquals(2, viewModel.Horizontals[0].Campaigns.Count);
			AssertEquals(2, master.AllTouches.Count);

			viewModel.RemoveCampaign(touch1b);
			AssertEquals(1, viewModel.Horizontals.Count);
			AssertEquals(1, viewModel.Horizontals[0].Campaigns.Count);
			AssertEquals(1, master.AllTouches.Count);

			AssertEquals(true, touch1b.IsDeleted);
			AssertEquals(false, viewModel.Horizontals.Any(h => h.Campaigns.Any(c => c.PK == touch1bPK)));
			AssertEquals(false, master.Horizontals.Any(h => h.Campaigns.Any(c => c.PK == touch1bPK)));
		}

		public void TestChangePosition()
		{
			var viewModel = new TouchSummaryViewModel();

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			var touch1c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1c.G0_HorizontalId = 1;
			touch1c.G0_VerticalId = "C";
			master.AllTouches.Add(touch1c);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);

			viewModel.SetDataSource(master);

			viewModel.ChangeTouchPosition(touch1b, touch1c);

			AssertEquals((byte)1, touch1a.G0_HorizontalId);
			AssertEquals("A", touch1a.G0_VerticalId);
			AssertEquals((byte)1, touch1b.G0_HorizontalId);
			AssertEquals("C", touch1b.G0_VerticalId);
			AssertEquals((byte)1, touch1c.G0_HorizontalId);
			AssertEquals("B", touch1c.G0_VerticalId);
			AssertEquals((byte)2, touch2a.G0_HorizontalId);
			AssertEquals("A", touch2a.G0_VerticalId);

			viewModel.ChangeTouchPosition(touch1a, touch1b);

			AssertEquals((byte)1, touch1a.G0_HorizontalId);
			AssertEquals("C", touch1a.G0_VerticalId);
			AssertEquals((byte)1, touch1b.G0_HorizontalId);
			AssertEquals("B", touch1b.G0_VerticalId);
			AssertEquals((byte)1, touch1c.G0_HorizontalId);
			AssertEquals("A", touch1c.G0_VerticalId);
			AssertEquals((byte)2, touch2a.G0_HorizontalId);
			AssertEquals("A", touch2a.G0_VerticalId);

			viewModel.ChangeTouchPosition(touch1a, touch2a);

			//no change
			AssertEquals((byte)1, touch1a.G0_HorizontalId);
			AssertEquals("C", touch1a.G0_VerticalId);
			AssertEquals((byte)1, touch1b.G0_HorizontalId);
			AssertEquals("B", touch1b.G0_VerticalId);
			AssertEquals((byte)1, touch1c.G0_HorizontalId);
			AssertEquals("A", touch1c.G0_VerticalId);
			AssertEquals((byte)2, touch2a.G0_HorizontalId);
			AssertEquals("A", touch2a.G0_VerticalId);
		}

		[TestDate(2019, 2, 25)]
		public void TestLaunch_ReturnsError()
		{
			var viewModel = new TouchSummaryViewModel();
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			helper.Touch1A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.BAT;
			helper.Touch1A.SendSettings.GSC_ContactLimitEachBatch = 0;
			helper.Factory.Save();
			Assert("Pre-condition: GSC_ContactLimitEachBatch should have error", helper.Touch1A.SendSettings.GSC_ContactLimitEachBatchInfo.HasErrors());

			viewModel.SetDataSource(helper.Master);
			var errors = viewModel.Launch();
			AssertEquals($@"190225_***_***_Touch 1A_1A: Cannot Send Campaigns All errors on this campaign must be corrected before campaigns can be sent.
- Please set at least one batch limit or change your Schedule Type.
This touch campaign can be accessed through its master campaign with ID {helper.Master.G0_CampaignID}.", errors);
		}
	}
}
