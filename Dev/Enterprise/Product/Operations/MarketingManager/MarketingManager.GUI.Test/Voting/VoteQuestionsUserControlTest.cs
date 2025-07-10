using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class VoteQuestionsUserControlTest : ZFormBasherTest
	{
		public void TestShowVoteSettingsControl()
		{
			using (Form form = GetFormToBash())
			{
				form.Show();

				Control control = form.Controls.Find("MaxVotesCalcEdit", true)[0];
				Assert(control.Visible);

				VoteQuestionsUserControl userControl = (VoteQuestionsUserControl)form.Controls.Find("VoteQuestionsUserControl", true)[0];
				userControl.ShowVoteSettingsControls = false;
				Assert(!control.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return GetTestForm(campaign);
		}

		ZForm GetTestForm(GlbCompanyCampaign campaign)
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			VoteQuestionsUserControl userControl = new VoteQuestionsUserControl();
			userControl.Name = "VoteQuestionsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}

		public void TestRandomizeWithinHeadersCheckBox()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			using (Form form = GetTestForm(campaign))
			{
				form.Show();

				var control = form.Controls.Find("RandomizeWithinHeaderCheckBox", true).Single() as ZCheckBox;
				Assert(control.Visible);
				Assert("By default, the questions are randomized (existing functionality for voting campaigns)", control.Checked);
				AssertEquals(false, control.ReadOnly);

				control.CheckState = System.Windows.Forms.CheckState.Unchecked;
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				Assert(control.Visible);
				Assert("It should be checked even if it switches between campaign types.", control.Checked);
			}
		}
	}
}
