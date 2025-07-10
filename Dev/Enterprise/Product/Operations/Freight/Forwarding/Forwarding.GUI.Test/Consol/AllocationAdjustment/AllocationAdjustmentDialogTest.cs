using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal class AllocationAdjustmentDialogTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			string labelTextExpected = "For the following consolidation(s) some of the pre-allocated values exceeds the registry-specified percentage. To continue saving you can increase pre-allocated values to stay within permitted percentage of pre-allocations or you can cancel the save and put a shipment on another consol.";

			AssertEquals("Precondition", true, Env.Security.ConsolPreAllocationEditing.IsAllowed);

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(new AllocationAdjustmentsSecurity()))
			{
				dialog.Show();

				AssertEquals(false, dialog.AuthorizationGroupBox.Enabled);
				AssertEquals(false, dialog.AuthorizationGroupBox.Visible);
				AssertEquals(labelTextExpected, dialog.MessageLabel.Text);
			}

			Env.Security.ConsolPreAllocationEditing.IsAllowed = false;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = false;
			AssertEquals("Precondition", false, Env.Security.ConsolPreAllocationEditing.IsAllowed);

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(new AllocationAdjustmentsSecurity()))
			{
				dialog.Show();

				AssertEquals(true, dialog.AuthorizationGroupBox.Enabled);
				AssertEquals(true, dialog.AuthorizationGroupBox.Visible);
				AssertEquals(labelTextExpected + "\r\n\r\nIf you want to increase the pre-allocated value, you will need authorization from someone with permission to adjust the pre-allocations.\r\n\r\nNote: Only gateway agent is authorized to edit pre-allocations.", dialog.MessageLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestConfirmAllocationAdjustments_OK()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();

			bool result = AllocationAdjustmentDialog.ConfirmAllocationAdjustments(new ForwardingConsol[] { consol1, consol2, consol3 });

			AllocationAdjustmentDialog dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;
			AssertNotNull("Dialog was shown", dialog);
			AssertEquals("Dialog returned true", true, result);

			AllocationAdjustmentsSecurity adjustmentsSecurity = (AllocationAdjustmentsSecurity)dialog.LastDataSourceForTest;
			AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol1, consol2, consol3 }, adjustmentsSecurity.Adjustments.Cast<AllocationAdjustment>().Select(x => x.Consol));
		}

		[RequiresSTA]
		public void TestConfirmAllocationAdjustments_Cancel()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			bool result = AllocationAdjustmentDialog.ConfirmAllocationAdjustments(new ForwardingConsol[] { Factory.New<ForwardingConsol>() });

			AllocationAdjustmentDialog dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;
			AssertNotNull("Dialog was shown", dialog);
			AssertEquals("Dialog returned false", false, result);
		}

		public void TestAdjust_CurrentUserAllowedToAdjustAllocations()
		{
			AssertEquals("Precondition", true, Env.Security.ConsolPreAllocationEditing.IsAllowed);

			var adjustmentsMock = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock.CallBase = true;
			adjustmentsMock.Setup(m => m.AdjustAll(GlbStaff.CurrentUser));

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock.Object))
			{
				dialog.Show();
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock.VerifyAll();
			}
		}

		[RequiresSTA]
		public void TestAdjust_InvalidBusinessEntity()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 1234567m;
			var consols = new List<ForwardingConsol>
			{
				consol
			};

			AllocationAdjustmentsSecurity adjustmentsSecurity = new AllocationAdjustmentsSecurity(consols);

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsSecurity))
			{
				dialog.Show();
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestAdjust_WithAuthorization()
		{
			Env.Security.ConsolPreAllocationEditing.IsAllowed = false;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = false;
			AssertEquals("Precondition", false, Env.Security.ConsolPreAllocationEditing.IsAllowed);
			AssertEquals("Precondition", false, Env.Security.GatewayConsolPreAllocationEditing.IsAllowed);

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "mickey.mouse";
			staff.GS_Code = "MM";
			staff.StaffPlainTextPassword = "bigears";
			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();

			var adjustmentsMock1 = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock1.CallBase = true;
			adjustmentsMock1.Verify(m => m.AdjustAll(It.IsAny<GlbStaff>()), Times.Never);

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock1.Object))
			{
				dialog.Show();
				adjustmentsMock1.Object.Login = staff.GS_LoginName;
				adjustmentsMock1.Object.Password = staff.StaffPlainTextPassword;
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error mickey.mouse is not authorized to adjust the allocations.", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock1.VerifyAll();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var adjustmentsMock2 = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock2.CallBase = true;
			adjustmentsMock2.Setup(m => m.AdjustAll(It.IsAny<GlbStaff>()));

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock2.Object))
			{
				dialog.Show();
				adjustmentsMock2.Object.Login = staff.GS_LoginName;
				adjustmentsMock2.Object.Password = staff.StaffPlainTextPassword;
				adjustmentsMock2.Object.UserSecurity.ConsolPreAllocationEditing.IsAllowed = true;
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock2.VerifyAll();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var adjustmentsMock3 = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock3.CallBase = true;
			adjustmentsMock3.Setup(m => m.AdjustAll(It.IsAny<GlbStaff>()));

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock3.Object))
			{
				dialog.Show();
				adjustmentsMock3.Object.Login = staff.GS_LoginName;
				adjustmentsMock3.Object.Password = staff.StaffPlainTextPassword;
				adjustmentsMock3.Object.UserSecurity.ConsolPreAllocationEditing.IsAllowed = true;
				adjustmentsMock3.Object.UserSecurity.GatewayConsolPreAllocationEditing.IsAllowed = true;
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock3.VerifyAll();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var adjustmentsMock4 = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock4.CallBase = true;
			adjustmentsMock4.Verify(m => m.AdjustAll(It.IsAny<GlbStaff>()), Times.Never);

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock4.Object))
			{
				dialog.Show();
				adjustmentsMock4.Object.SetUserSecurityForTests(null);
				adjustmentsMock4.Object.Login = "john";
				adjustmentsMock4.Object.Password = "doe";
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error Invalid username / password or expired password.", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock4.VerifyAll();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var adjustmentsMock5 = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock5.CallBase = true;
			adjustmentsMock5.Setup(m => m.AdjustAll(It.IsAny<GlbStaff>()));
			var gatewayConsol = Factory.New<ForwardingConsol>();
			var creator = new TestObjectCreator(Factory);
			var gatewayOrgProxy = creator.CreateOrgHeader("GTWORG", true, true);
			creator.CreateNewCompany("CGW", orgProxy: gatewayOrgProxy);
			gatewayConsol.JK_OA_SendingForwarderAddress = gatewayOrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			adjustmentsMock5.Object.Adjustments.Add(new AllocationAdjustment(gatewayConsol));
			Env.Security.ConsolPreAllocationEditing.IsAllowed = false;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = true;

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock5.Object))
			{
				dialog.Show();
				adjustmentsMock5.Object.Login = staff.GS_LoginName;
				adjustmentsMock5.Object.Password = staff.StaffPlainTextPassword;
				adjustmentsMock5.Object.UserSecurity.ConsolPreAllocationEditing.IsAllowed = false;
				adjustmentsMock5.Object.UserSecurity.GatewayConsolPreAllocationEditing.IsAllowed = true;
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock5.VerifyAll();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var adjustmentsMock6 = new Mock<AllocationAdjustmentsSecurity>();
			adjustmentsMock6.CallBase = true;
			adjustmentsMock6.Setup(m => m.AdjustAll(It.IsAny<GlbStaff>()));
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = gatewayOrgProxy.MainAddress.PK;
			adjustmentsMock6.Object.Adjustments.Add(new AllocationAdjustment(consol));
			Env.Security.ConsolPreAllocationEditing.IsAllowed = true;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = false;

			using (AllocationAdjustmentDialogForTest dialog = new AllocationAdjustmentDialogForTest(adjustmentsMock6.Object))
			{
				dialog.Show();
				adjustmentsMock6.Object.Login = staff.GS_LoginName;
				adjustmentsMock6.Object.Password = staff.StaffPlainTextPassword;
				adjustmentsMock6.Object.UserSecurity.ConsolPreAllocationEditing.IsAllowed = true;
				adjustmentsMock6.Object.UserSecurity.GatewayConsolPreAllocationEditing.IsAllowed = false;
				dialog.ClickAdjustButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());

				adjustmentsMock6.VerifyAll();
			}
		}

		public void TestUnitsColumnsExist()
		{
			var adjustmentsMock = new Mock<AllocationAdjustmentsSecurity>();
			using (var dialog = new AllocationAdjustmentDialog(adjustmentsMock.Object))
			{
				var adjustmentsGrid = dialog.FindSingle<ZGrid>("AdjustmentsGrid");

				var weightUnitColumn = adjustmentsGrid.GetColumnStyle("AllocatedWeightUnit");
				AssertNotNull("Allocated weight unit column should exist.", weightUnitColumn);
				Assert("Allocated weight unit column should be visible.", weightUnitColumn.IsVisible);

				var volumeUnitColumn = adjustmentsGrid.GetColumnStyle("AllocatedVolumeUnit");
				AssertNotNull("Allocated volume unit column should exist.", volumeUnitColumn);
				Assert("Allocated volume unit column should be visible.", volumeUnitColumn.IsVisible);

				var chargeableUnitColumn = adjustmentsGrid.GetColumnStyle("AllocatedChargeableUnit");
				AssertNotNull("Allocated volume unit column should exist.", chargeableUnitColumn);
				Assert("Allocated volume unit column should be visible.", chargeableUnitColumn.IsVisible);
			}
		}

		#region Implementation

		class AllocationAdjustmentDialogForTest : AllocationAdjustmentDialog
		{
			public AllocationAdjustmentDialogForTest(AllocationAdjustmentsSecurity adjustmentsSecurity)
				: base(adjustmentsSecurity)
			{
			}

			public void ClickAdjustButton()
			{
				this.AdjustButton.PerformClick();
			}

			public new ZLabel MessageLabel
			{
				get { return base.MessageLabel; }
			}

			public new ZGroupBox AuthorizationGroupBox
			{
				get { return base.AuthorizationGroupBox; }
			}
		}

		#endregion
	}
}
