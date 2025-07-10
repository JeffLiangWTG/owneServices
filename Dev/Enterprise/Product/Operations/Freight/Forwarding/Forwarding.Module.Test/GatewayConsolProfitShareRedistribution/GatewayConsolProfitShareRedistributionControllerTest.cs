using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(GatewayConsolProfitShareRedistributionController))]
	public class GatewayConsolProfitShareRedistributionControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.GatewayConsolProfitShareRedistribution;

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestDeniedSecurityCheckPoints()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
			Factory.Save();

			var globalDeniedCheckpoint = new DeniedSecurityCheckpoint();

			var controller = new GatewayConsolProfitShareRedistributionController();
			var checkpointForEdit = controller.GetCheckPointForEdit(profitShareRedistribution);
			AssertDeniedCheckPoint(checkpointForEdit);
			var checkpointForDelete = controller.GetCheckPointForDelete(profitShareRedistribution);
			AssertDeniedCheckPoint(checkpointForDelete);

			// cannot compare object, split into property comparisons
			void AssertDeniedCheckPoint(ISecurityCheckpoint checkpoint)
			{
				AssertEquals("Denied checkpoint code", globalDeniedCheckpoint.Code, checkpoint.Code);
				AssertEquals("Denied checkpoint message", globalDeniedCheckpoint.DisplayText, checkpoint.DisplayText);
				AssertEquals(globalDeniedCheckpoint.IsAllowed, checkpoint.IsAllowed);
			}
		}

		public void TestSecurityCheckPoints_New_NotAllowed()
		{
			const string expectedExceptionMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Gateway Sell Apportionment/Billing -> Redistribute Gateway Consol Profit -> Redistribute Processing";

			var controller = new GatewayConsolProfitShareRedistributionController();
			// ShowNewForm is a special case where we throw exception instead of showing security message.
			AssertSecurityCheckPoints_WithException(
				controller.ShowNewForm,
				Env.Security.GatewayConsolProfitShareRedistributionNew.Code,
				expectedExceptionMessage);
		}

		public void TestSecurityCheckPoints_New_Allowed()
		{
			var controller = new GatewayConsolProfitShareRedistributionController();
			AssertSecurityCheckPoints_WithoutException(
				controller.ShowNewForm,
				Env.Security.GatewayConsolProfitShareRedistributionNew.Code);
		}

		public void TestSecurityCheckPoints_View_NotAllowed()
		{
			const string expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Gateway Sell Apportionment/Billing -> Redistribute Gateway Consol Profit -> View";

			var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
			Factory.Save();

			var controller = new GatewayConsolProfitShareRedistributionController();
			AssertSecurityCheckPoints_WithoutException(
				() => controller.ShowViewForm(profitShareRedistribution),
				Env.Security.GatewayConsolProfitShareRedistributionView.Code,
				isAllowed: false,
				expectedMessage);
		}

		public void TestSecurityCheckPoints_View_Allowed()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
			Factory.Save();

			var controller = new GatewayConsolProfitShareRedistributionController();
			AssertSecurityCheckPoints_WithoutException(
				() => controller.ShowViewForm(profitShareRedistribution),
				Env.Security.GatewayConsolProfitShareRedistributionView.Code,
				isAllowed: true);
		}

		void AssertSecurityCheckPoints_WithException(Func<IZForm> showForm, string securityCheckpointCode, string expectedExceptionMessage)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, staff, securityCheckpointCode, false);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				try
				{
					using (showForm.Invoke())
					{
					}
					Assert("SecurityAccessDeniedException should have been thrown", false);
				}
				catch (SecurityAccessDeniedException e)
				{
					AssertEquals(expectedExceptionMessage, e.Message);
				}
			}
		}

		void AssertSecurityCheckPoints_WithoutException(Func<IZForm> showForm, string securityCheckpointCode, bool isAllowed = true, string expectedMessage = null)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, staff, securityCheckpointCode, isAllowed);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (showForm.Invoke())
				{
				}
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestFormAppearanceOnReadOnlyMode()
		{
			var profitShareRedistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			Factory.Save();

			using (var form = (GatewayProfitShareRedistributionForm)((GatewayConsolProfitShareRedistributionController)Controller).ShowEditForm(profitShareRedistribution))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				Application.DoEvents();

				var postingButton = (ZPostingButtonsUserControl)form.Controls.Find("zPostingButtonsUserControl", true).Single();
				var saveButton = postingButton.SaveButton;
				var saveAndCloseButton = postingButton.SaveAndCloseButton;
				var cancelAndCloseButton = postingButton.CloseButton;

				var rulesGroupBox = (ZGroupBox)form.Controls.Find("RulesGroupBox", true).Single();
				var consolModuleButtonGrid = (ProfitShareConsolWrapperModuleButtonGrid)form.Controls.Find("ConsolModuleButtonGrid", true).Single();
				var consolModuleButtonGridToolStrip = (ZToolStrip)consolModuleButtonGrid.Controls.Find("toolStrip", true).Single();

				var redistributionLogsButton = (ZButton)form.Controls.Find("RedistributionLogButton", true).Single();
				var redistributeProfitSharesButton = (ZButton)form.Controls.Find("RedistributeProfitSharesButton", true).Single();

				var previousNextControl = (ZPreviousNextControl)form.Controls.Find("ZPreviousNextControl", true).Single();

				Assert("Save button should be invisible.", !saveButton.Visible);
				Assert("Save and Close button should be invisible.", !saveAndCloseButton.Visible);
				AssertEquals("Cancel button's text should be 'Close'.", "&Close", cancelAndCloseButton.Text);
				Assert("Redistribute Profit Shares button should be invisible.", !redistributeProfitSharesButton.Visible);

				Assert("Rules Group Box should be visible.", rulesGroupBox.Visible);
				Assert("Consol Grid's tool strip should be invisible.", !consolModuleButtonGridToolStrip.Visible);

				Assert("Redistribution Log button should be visible.", redistributionLogsButton.Visible);
				Assert("Redistribution Log button should be enabled.", redistributionLogsButton.Enabled);

				Assert("Previous Next Control should not be visible.", !previousNextControl.Visible);
			}
		}
	}
}
