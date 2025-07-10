using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal class NonMatchingAgentsDialogTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();
			var agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper.Setup(m => m.CheckRelatedReceivingAgents(new[] { shipment }, new[] { consol })).Returns("message");
				agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
				AssertEquals("message", agentSecurity.Message);
			}

			AssertEquals("Precondition", true, Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed);
			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();

				AssertEquals(false, dialog.AuthorizationGroupBox.Enabled);
				AssertEquals(false, dialog.AuthorizationGroupBox.Visible);
				AssertEquals("message", dialog.MessageLabel.Text);
			}

			Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed = false;
			AssertEquals("Precondition", false, Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed);

			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();

				AssertEquals(true, dialog.AuthorizationGroupBox.Enabled);
				AssertEquals(true, dialog.AuthorizationGroupBox.Visible);
				AssertEquals("message" + "\r\n\r\n" + @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Allow attach Shipments with non-matching Sending/Receiving Agents

Supervisor access is required to proceed with the save or cancel to make the necessary changes.", dialog.MessageLabel.Text);
			}
			helper.VerifyAll();
		}

		public void TestConfirm_OK()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();

			var agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
			bool result = NonMatchingAgentsDialog.CheckAndConfirm(new[] { consol }, new[] { shipment });
			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				dialog.ClickConfirmButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Dialog returned true", true, result);
			}
		}

		[RequiresSTA]
		public void TestConfirm_Cancel()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;

			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();

			var agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				dialog.ClickCancelButton();

				AssertEquals("DialogResult", DialogResult.No, dialog.DialogResult);
				AssertNotNull("Dialog was shown", dialog);
			}
		}

		public void TestAttach_CurrentUserAllowedToAttach()
		{
			AssertEquals("Precondition", true, Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed);

			var dialog = new NonMatchingAgentsDialogForTest(new NonMatchingAgentsSecurity());
			dialog.Show();
			dialog.ClickConfirmButton();

			AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
			AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestAttach_WithAuthorization()
		{
			Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed = false;
			AssertEquals("Precondition", false, Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "dora.doll";
			staff.GS_Code = "DD";
			staff.StaffPlainTextPassword = "barbie";
			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();

			var agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				agentSecurity.Login = staff.GS_LoginName;
				agentSecurity.Password = staff.StaffPlainTextPassword;
				dialog.ClickConfirmButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error dora.doll is not authorized to attach shipments with consols.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				agentSecurity.Login = staff.GS_LoginName;
				agentSecurity.Password = staff.StaffPlainTextPassword;
				agentSecurity.UserSecurity.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed = true;
				dialog.ClickConfirmButton();

				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (NonMatchingAgentsDialogForTest dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				agentSecurity.Login = "hello";
				agentSecurity.Password = "bye";
				dialog.ClickConfirmButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error Invalid username / password or expired password.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestAttach_WithExpiredLogin()
		{
			Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed = false;
			AssertEquals("Precondition", false, Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "dora.doll";
			staff.GS_Code = "DD";
			staff.StaffPlainTextPassword = "barbie";
			staff.GS_ChangePasswordAtNextLogin = true;
			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();

			var agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
			using (var dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				agentSecurity.Login = staff.GS_LoginName;
				agentSecurity.Password = staff.StaffPlainTextPassword;
				dialog.ClickConfirmButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error Invalid username / password or expired password.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("UserSecurity", null, agentSecurity.UserSecurity);
			}

			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var dialog = new NonMatchingAgentsDialogForTest(agentSecurity))
			{
				dialog.Show();
				agentSecurity.Login = staff.GS_LoginName;
				agentSecurity.Password = staff.StaffPlainTextPassword;
				dialog.ClickConfirmButton();

				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error dora.doll is not authorized to attach shipments with consols.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#region Implementation

		class NonMatchingAgentsDialogForTest : NonMatchingAgentsDialog
		{
			public NonMatchingAgentsDialogForTest(NonMatchingAgentsSecurity agentsSecurity)
				: base(agentsSecurity)
			{
			}

			public void ClickConfirmButton()
			{
				this.AttachButton.PerformClick();
			}

			public new ZLabel MessageLabel
			{
				get { return base.MessageLabel; }
			}

			public new ZGroupBox AuthorizationGroupBox
			{
				get { return base.AuthorizationGroupBox; }
			}

			public void ClickCancelButton()
			{
				CancelButton.PerformClick();
			}
		}

		#endregion
	}
}
