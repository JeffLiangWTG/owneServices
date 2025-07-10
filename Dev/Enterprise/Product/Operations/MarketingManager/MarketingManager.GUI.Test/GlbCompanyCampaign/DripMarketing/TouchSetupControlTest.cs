using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TouchSetupControlTest : TestCaseWithFactory
	{
		public void TestValidateEmptyDatasourceDRMMasterCampaign()
		{
			AssertValidateEmptyDatasource(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Broadcast, CampaignTypeList.Codes.Broadcast);
		}

		public void TestValidateEmptyDatasourceINSMasterCampaign()
		{
			AssertValidateEmptyDatasource(CampaignTypeList.Codes.InsideSales, InsideSalesTouchTypeList.Codes.PreApproachEmail, InsideSalesTouchTypeList.Codes.OpportunityCreation);
		}

		public void AssertValidateEmptyDatasource(string masterCampaignType, string touch1AType, string touch2AType)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = masterCampaignType;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_BroadcastVoteSurveyExam = touch1AType;
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1A);

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_BroadcastVoteSurveyExam = touch2AType;
			touch2A.G0_Type = "PREAP";
			touch2A.G0_Category = "PRINT";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2A.G0_CampaignName = "Touch 2A";
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2A);
			Factory.Save();

			using (var form = new ZForm(touch2A))
			using (var control = new TouchSetupControl())
			{
				control.SetDataBinding(touch2A, "");
				form.Controls.Add(control);
				form.Show();

				control.TransitionRulesGrid.Select(0);
				control.TransitionRulesGrid.ContextMenu.MenuItems.FindByText("&Delete").PerformClick();

				control.SetSources();
				Assert(touch2A.TouchSourceCampaignPKsForValidationInfo.HasErrors());
			}
		}

		public void TestDripRulesBinding()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			helper.Touch1A.TransitionRulesToThisCampaign.AddNew();

			using (var form = new ZForm(helper.Touch1A))
			using (var control = new TouchSetupControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.TransitionRulesGrid.ListManager.Position = 0;
				AssertEquals(helper.Touch1A.TransitionRulesToThisCampaign[0].FilterRule.PK, control.SendCampaignControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);

				control.TransitionRulesGrid.ListManager.Position = 1;

				AssertEquals(helper.Touch1A.TransitionRulesToThisCampaign[1].FilterRule.PK, control.SendCampaignControl.FilterStripControl.FilterBusinessObject.LastUsedLayout.PK);
			}
		}
	}
}
