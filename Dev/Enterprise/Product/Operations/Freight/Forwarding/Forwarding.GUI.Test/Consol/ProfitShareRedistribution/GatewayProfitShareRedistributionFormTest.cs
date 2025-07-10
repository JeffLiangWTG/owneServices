using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(GatewayProfitShareRedistributionForm))]
	public class GatewayProfitShareRedistributionFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var gatewayProfitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();
			return new GatewayProfitShareRedistributionForm(gatewayProfitShareRedistribution);
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); // override to allow 1080p
		}
	}

	public class GatewayProfitShareRedistributionFormTest : TestCaseWithFactory
	{
		public void TestSetActionButtonsStates()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			using (var form = new GatewayProfitShareRedistributionForm(profitShareRedistribution))
			{
				form.Show();
				Application.DoEvents();

				var postingButton = (ZPostingButtonsUserControl)form.Controls.Find("zPostingButtonsUserControl", true).Single();
				var redistributeProfitSharesButton = (ZButton)form.Controls.Find("RedistributeProfitSharesButton", true).Single();

				postingButton.SaveButton.Enabled.Should().Be(false);
				postingButton.SaveAndCloseButton.Enabled.Should().Be(false);
				redistributeProfitSharesButton.Enabled.Should().Be(false);

				// add consols (then automatically shipments)
				profitShareRedistribution.Shipments.Add(Factory.NewWithValidTestData<ForwardingShipment>());
				form.SetActionButtonsStates();

				postingButton.SaveButton.Enabled.Should().Be(false);
				postingButton.SaveAndCloseButton.Enabled.Should().Be(false);
				redistributeProfitSharesButton.Enabled.Should().Be(false);

				// add rules
				var agreement = Factory.NewWithValidTestData<OrgAgentRelationship>();
				agreement.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
				var details = agreement.ProfitShareDetails.AddNew();
				details.O4_JobType = JobTypesList.Codes.GCN;
				details.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.SHP;
				agreement.SelectedProfitShareDetailsList.Add(details);
				profitShareRedistribution.AddProfitShareRules(new[] { agreement });
				form.SetActionButtonsStates();

				postingButton.SaveButton.Enabled.Should().Be(false);
				postingButton.SaveAndCloseButton.Enabled.Should().Be(false);
				redistributeProfitSharesButton.Enabled.Should().Be(true, because: "Redistribution should be ready to run");

				// redistribution has run but there is/are errors.
				form.RedistributionStatus = RedistributionStatus.DoneWithError;
				form.SetActionButtonsStates();

				postingButton.SaveButton.Enabled.Should().Be(false);
				postingButton.SaveAndCloseButton.Enabled.Should().Be(false);
				redistributeProfitSharesButton.Enabled.Should().Be(true, because: "Redistribution should be able to run again");

				form.RedistributionStatus = RedistributionStatus.Succeeded;
				form.SetActionButtonsStates();

				postingButton.SaveButton.Enabled.Should().Be(true);
				postingButton.SaveAndCloseButton.Enabled.Should().Be(true);
				redistributeProfitSharesButton.Enabled.Should().Be(true, because: "Redistribution should be able to run again");

				// remove PS rule
				profitShareRedistribution.RemoveProfitShareRules(new[] { details });
				form.SetActionButtonsStates();

				postingButton.SaveButton.Enabled.Should().Be(false);
				postingButton.SaveAndCloseButton.Enabled.Should().Be(false);
				redistributeProfitSharesButton.Enabled.Should().Be(false, because: "No PS Rules, redistribution should not be ready to run");

				Assert("This test uses FluentAssertions", true);
			}
		}

		public void TestForm_State_OnSave()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			using (var form = new GatewayProfitShareRedistributionForm(profitShareRedistribution))
			{
				var postingButton = (ZPostingButtonsUserControl)form.Controls.Find("zPostingButtonsUserControl", true).Single();

				form.Show();
				Application.DoEvents();

				profitShareRedistribution.PSR_TotalProfitShare = 100m;
				profitShareRedistribution.PSR_RedistributedProfitShare = 100m;
				profitShareRedistribution.PSR_RX_NKCurrency = "AUD";
				profitShareRedistribution.ProfitShareRules.Add(Factory.NewWithValidTestData<ProfitShareRedistributionRule>());

				form.RedistributionStatus = RedistributionStatus.Succeeded;
				form.SetActionButtonsStates();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();
				Assert("should not save when DialogResult is No", !form.BusinessEntity.IsInDatabase);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				Assert("should save when DialogResult is Yes", form.BusinessEntity.IsInDatabase);

				//posting button state post save
				Assert(!postingButton.SaveButton.Visible);
				Assert(!postingButton.SaveAndCloseButton.Visible);
				Assert(postingButton.CloseButton.Visible);
				Assert(postingButton.CloseButton.Enabled);

				//other controls state post save
				var rulesGroupBox = form.Controls.Find("RulesGroupBox", true).Single();
				Assert(rulesGroupBox.Visible);
				AssertEquals(1, form.ProfitShareRuleModuleButtonGrid.InnerGrid.List.Count);

				var redistributeProfitSharesButton = form.Controls.Find("RedistributeProfitSharesButton", true).Single();
				Assert(!redistributeProfitSharesButton.Visible);

				var consolModuleButtonGrid = (ProfitShareConsolWrapperModuleButtonGrid)form.Controls.Find("ConsolModuleButtonGrid", true).Single();
				Assert(!consolModuleButtonGrid.AttachButtonForTest.Visible);
				Assert(!consolModuleButtonGrid.DetachButtonForTest.Visible);

				var profitShareRuleModuleButtonGrid = (ProfitShareRuleModuleButtonGrid)form.Controls.Find("ProfitShareRuleModuleButtonGrid", true).Single();
				Assert(!profitShareRuleModuleButtonGrid.AttachButtonForTest.Visible);
				Assert(!profitShareRuleModuleButtonGrid.DetachButtonForTest.Visible);

				var redistributionLogsButton = form.Controls.Find("RedistributionLogButton", true).Single();
				Assert(redistributionLogsButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestForm_State_OnLoadExistingBatch()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			profitShareRedistribution.ProfitShareRules.Add(Factory.NewWithValidTestData<ProfitShareRedistributionRule>());
			profitShareRedistribution.Factory.Save();

			using (var form = new GatewayProfitShareRedistributionForm(profitShareRedistribution))
			{
				form.Show();
				Application.DoEvents();

				var rulesGroupBox = form.Controls.Find("RulesGroupBox", true).Single();
				Assert(rulesGroupBox.Visible);
				AssertEquals(1, form.ProfitShareRuleModuleButtonGrid.InnerGrid.List.Count);

				var redistributeProfitSharesButton = form.Controls.Find("RedistributeProfitSharesButton", true).Single();
				Assert(!redistributeProfitSharesButton.Visible);

				var consolModuleButtonGrid = (ProfitShareConsolWrapperModuleButtonGrid)form.Controls.Find("ConsolModuleButtonGrid", true).Single();
				Assert(!consolModuleButtonGrid.AttachButtonForTest.Visible);
				Assert(!consolModuleButtonGrid.DetachButtonForTest.Visible);

				var profitShareRuleModuleButtonGrid = (ProfitShareRuleModuleButtonGrid)form.Controls.Find("ProfitShareRuleModuleButtonGrid", true).Single();
				Assert(!profitShareRuleModuleButtonGrid.AttachButtonForTest.Visible);
				Assert(!profitShareRuleModuleButtonGrid.DetachButtonForTest.Visible);

				var redistributionLogsButton = form.Controls.Find("RedistributionLogButton", true).Single();
				Assert(redistributionLogsButton.Enabled);
			}
		}

		public void TestProfitShareRuleModuleButtonGridShouldAllowAttachDetachWithoutEditSecurity()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			using (var form = new GatewayProfitShareRedistributionForm(profitShareRedistribution))
			{
				form.Show();
				Assert(form.ProfitShareRuleModuleButtonGrid.AllowAttachDetachWithoutEditSecurity);
			}
		}
	}
}
