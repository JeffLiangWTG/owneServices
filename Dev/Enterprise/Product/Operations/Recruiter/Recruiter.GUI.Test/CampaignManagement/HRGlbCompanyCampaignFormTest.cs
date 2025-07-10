using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(HRGlbCompanyCampaignForm))]
	public class HRGlbCompanyCampaignFormTest : ZFormBasherTest
	{
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		protected sealed override Form GetFormToBashCore()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			HRGlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign);
			form.ControllerID = ControllerIDs.HRGlbCompanyCampaign;
			return form;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;
				const int MinScreenWidthSupported = 1440;
				const int MinScreenHeightSupported = 811;
				const int TypicalTaskbarHeight = 43;
				int maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				int maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);
				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		public void TestOpenCampaignForHRCampaign()
		{
			var hrCampaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			hrCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AssertEquals(true, hrCampaign.IsHRCampaign);
			Factory.Save();
			var originalFormCount = Application.OpenForms.Count;
			TouchSummaryViewModel.OpenCampaignForTest(hrCampaign);
			AssertEquals(originalFormCount + 1, Application.OpenForms.Count);
			var form = Application.OpenForms.OfType<HRGlbCompanyCampaignForm>().Single();
			AssertNotNull(form);
			form.Close();
		}

		public void TestFormOverrides()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			using (var form = new HRGlbCompanyCampaignForm(campaign))
			{
				AssertType("SendCampaignsControl should be overridden in the constructor", typeof(HRSendCampaignsControl), form.InternalSendCampaignControl);
				AssertType("TouchSetupControl should be overridden in the constructor", typeof(HRTouchSetupControl), form.InternalTouchSetupControl);
				AssertType("TrackingStatusControl should be overridden in the constructor", typeof(HRTrackingStatusChartUserControl), form.InternalTrackingStatusControl);
				form.InternalTouchesTabPageOnBindingOrFirstShown(this, EventArgs.Empty);
				AssertType("TouchSummary should be overridden in TouchesTabPageOnBindingOrFirstShown", typeof(HRIntegratedTouchSummary), form.InternalTouchSummary);
			}
		}

		protected virtual HRGlbCompanyCampaignForm GetNewGlbCompanyCampaignForm(HRGlbCompanyCampaign campaign)
		{
			return new HRGlbCompanyCampaignForm(campaign, true);
		}
		#endregion
	}
}
