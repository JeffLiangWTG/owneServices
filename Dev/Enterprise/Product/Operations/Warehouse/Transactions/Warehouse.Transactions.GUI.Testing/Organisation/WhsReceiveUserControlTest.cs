using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class WhsReceiveUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestReceiveUserControlIsINotifications()
		{
			using (var whsRecieveUserControl = new WhsReceiveUserControl())
			{
				Assert("WhsReceiveUserControl implements INotifications.", whsRecieveUserControl is INotifications);
			}
		}

		public void TestReceiveUserControlNotifications_Error()
		{
			using (var whsRecieveUserControl = new WhsReceiveUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)whsRecieveUserControl;
				notify.AddError("This is error.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", "This is error.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReceiveUserControlNotifications_Info()
		{
			using (var whsRecieveUserControl = new WhsReceiveUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)whsRecieveUserControl;
				notify.AddInformation("This is info.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Notification message is correct.", "This is info.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReceiveUserControlNotifications_Warning()
		{
			using (var whsRecieveUserControl = new WhsReceiveUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)whsRecieveUserControl;
				notify.AddWarning("This is warning.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Notification message is correct.", "This is warning.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSetupPutawayRules()
		{
			using (var form = new ZForm())
			using (var receiveUserControl = new WhsReceiveUserControl())
			{
				form.Controls.Add(receiveUserControl);
				form.Show();

				var label = receiveUserControl.FindSingle<ZLinkLabel>("ProductionRulesEngineLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);

				label.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", @"This Production Rules Engine portal cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);

				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

				var staff = Helper.CreateGlbStaff("ABC", "ABC");
				Factory.Save();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					label.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
					var launchedUrl = WebUrlLauncher.LastUrlLaunched;
					var uri = new Uri(launchedUrl, UriKind.Absolute);

					AssertEquals("Launched uri is correct.", "https", uri.Scheme);
					AssertEquals("Launched uri is correct.", "address", uri.Host);
					AssertEquals("Launched uri is correct.", "/goto/ProductWarehousePutaway", uri.AbsolutePath);
				}
			}
		}

		public void TestReceiveCategory()
		{
			using (var form = new ZForm())
			using (var receiveUserControl = new WhsReceiveUserControl())
			{
				form.Controls.Add(receiveUserControl);
				form.Show();
				var styles = receiveUserControl.FindSingle<ZGrid>(c => c.Name == "ReceiveParamsGrid").ColumnStyles.Cast<ZGridColumnInfo>();

				var receiveCategory = styles.SingleOrDefault(s => s.ColumnName == WhsClientParameterByWarehouseSchema.WY_ReceiveCategory.Name);
				var preventReceivingOvers = styles.SingleOrDefault(s => s.ColumnName == WhsClientParameterByWarehouseSchema.WY_PreventReceivingOvers.Name);
				var receiveOverageTolerancePercent = styles.SingleOrDefault(s => s.ColumnName == WhsClientParameterByWarehouseSchema.WY_ReceiveOverageTolerancePercent.Name);
				Assert("receiveCategory should be available", !receiveCategory.IsUnavailable);
				Assert("preventReceivingOvers should be available", !preventReceivingOvers.IsUnavailable);
				Assert("receiveOverageTolerancePercent should be available", !receiveOverageTolerancePercent.IsUnavailable);
			}
		}

		public void TestCycleCountAutomation()
		{
			using (var form = new ZForm())
			using (var receiveUserControl = new WhsReceiveUserControl())
			{
				form.Controls.Add(receiveUserControl);
				form.Show();
				var styles = receiveUserControl.FindSingle<ZGrid>(c => c.Name == "ReceiveParamsGrid").ColumnStyles.Cast<ZGridColumnInfo>();

				var cycleCountOnAlternatePutaway = styles.SingleOrDefault(s => s.ColumnName == WhsClientParameterByWarehouseSchema.WY_CycleCountOnAlternatePutaway.Name);
				AssertEquals($"cycleCountOnAlternatePutaway should be available", false, cycleCountOnAlternatePutaway.IsUnavailable);
			}
		}

		public void TestValidatePalletIDAsSSCCOnUnload()
		{
			using (var form = new ZForm())
			using (var receiveUserControl = new WhsReceiveUserControl())
			{
				form.Controls.Add(receiveUserControl);
				form.Show();
				var styles = receiveUserControl.FindSingle<ZGrid>(c => c.Name == "ReceiveParamsGrid").ColumnStyles.Cast<ZGridColumnInfo>();

				var validatePalletIDAsSSCCOnUnload = styles.SingleOrDefault(s => s.ColumnName == WhsClientParameterByWarehouseSchema.WY_ValidatePalletIDAsSSCCOnUnload.Name);
				AssertEquals($"cycleCountOnAlternatePutaway should be available", false, validatePalletIDAsSSCCOnUnload.IsUnavailable);
			}
		}
	}
}
