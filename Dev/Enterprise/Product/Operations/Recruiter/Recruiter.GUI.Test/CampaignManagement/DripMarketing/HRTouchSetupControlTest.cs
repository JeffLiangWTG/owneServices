using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI.Testing
{
	public class HRTouchSetupControlTest : TestCaseWithFactory
	{
		//This test is here because it can't be in TouchSummaryViewModel
		public void TestAddHorizontalForHRCampaign()
		{
			var viewModel = new TouchSummaryViewModel();
			var master = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_IsSalesAndMarketing = false;
			viewModel.SetDataSource(master);
			AssertEquals(0, viewModel.Horizontals.Count);
			AssertEquals(0, master.Horizontals.Count());
			viewModel.AddHorizontal();
			AssertEquals("Should save before adding horizontals", 0, viewModel.Horizontals.Count);
			AssertEquals("Should save before adding horizontals", 0, master.Horizontals.Count());
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "CMS";
			staff.GS_EmailAddress = "cms@test.org";
			Factory.Save();
			viewModel.AddHorizontal();
			var form = viewModel.LastController.LastShownForm as HRGlbCompanyCampaignForm;
			AssertNotNull(form);
			var campaign = form.BusinessEntity as HRGlbCompanyCampaign;
			AssertNotNull(campaign);
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

		public void TestSendCampaignsControlOverride()
		{
			using (var control = new HRTouchSetupControl())
			{
				AssertType("SendCampaignsControl should be overridden in the constructor", typeof(HRSendCampaignsControl), control.InternalSendCampaignControl);
			}
		}
	}
}
