using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class WhsPickingUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestControl

		[ExpectNoExceptions]
		public void TestControl()
		{
			var client = Helper.CreateClient();
			var pickingParams = WhsClientPickingParams.GetClientPickingParams(client);
			using (var form = new ZForm(pickingParams))
			{
				form.Controls.Add(new WhsPickingUserControl());
				form.Show();
			}
		}

		public void TestEnforceScanningForLoad()
		{
			var client = Helper.CreateClient();
			var pickingParams = WhsClientPickingParams.GetClientPickingParams(client);
			using (var form = new ZForm(pickingParams))
			{
				var userControl = new WhsPickingUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var grid = userControl.FindSingle<ZGrid>("PickPackGrid");
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsClientPickPackParamsByWhsSchema.Constants.WPP_EnforceScanningForLoad);
				AssertEquals("Column is available.", false, column.IsUnavailable);
			}
		}

		#region Notifications

		public void TestWhsPickingUserControlTestIsINotifications()
		{
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				Assert("WhsPickingUserControl implements INotifications.", whsPickingUserControl is INotifications);
			}
		}

		public void TesWhsPickingUserControlNotifications_Error()
		{
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)whsPickingUserControl;
				notify.AddError("This is error.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", "This is error.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWhsPickingUserControlNotifications_Info()
		{
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)whsPickingUserControl;
				notify.AddInformation("This is info.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Notification message is correct.", "This is info.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWhsPickingUserControlNotifications_Warning()
		{
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)whsPickingUserControl;
				notify.AddWarning("This is warning.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Notification message is correct.", "This is warning.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestWhsPickingUserControlClickPicking()
		{
			using (var form = new ZForm())
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				form.Controls.Add(whsPickingUserControl);
				form.Show();

				var label = whsPickingUserControl.FindSingle<ZLinkLabel>("ProductionRulesEngineLinkLabel");
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
					AssertEquals("Launched uri is correct.", "/goto/ProductWarehouseAllocation", uri.AbsolutePath);
				}
			}
		}

		#region PickPackGrid Elements

		public void TestGridColumnsInPickPackGrid()
		{
			var checkboxNamesAndDefaultValuesboxNamesAndDefaultValues = new string[] {
				WhsClientPickPackParamsByWhsSchema.WPP_WW_Warehouse.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_WSH_SalesChannel.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_IsPickAndPackEnabled.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_IsUsingOwnLabel.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_F3_NKPackType.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_NumberOfLabelsToPrintOnNew.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_NumberOfLabelsToPrintOnClose.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_CartoniseByArea.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_IsUsingCartonSizes.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_PromptForWeightAndDimensions.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_EnforceScanningForLoad.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_EnforceTransportReferenceForLoad.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_CartonizeByProduct.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_CartonizeByProductCategory.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_CycleCountOnShort.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_UseDirectedPackingConsolidation.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_SplitOrdersFromPartiallyReplenishedPicks.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_EnableAutoPackageCreationOnPicking.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_AllowPickFinalizationWithUnpackedTotes.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_DetachWavedOrdersWithBlockingShortfall.Name,
				WhsClientPickPackParamsByWhsSchema.WPP_AllowPickDockDoorLocationOverride.Name,
			};

			using (var form = new ZForm())
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				form.Controls.Add(whsPickingUserControl);
				form.Show();
				var styles = whsPickingUserControl.FindSingle<ZGrid>(c => c.Name == "PickPackGrid").ColumnStyles.Cast<ZGridColumnInfo>();

				foreach (var attributeName in checkboxNamesAndDefaultValuesboxNamesAndDefaultValues)
				{
					var checkboxColumn = styles.SingleOrDefault(s => s.ColumnName == attributeName);
					AssertEquals($"{attributeName} should be available.", false, checkboxColumn.IsUnavailable);
				}
			}
		}

		public void TestWPP_AllowPickDockDoorLocationOverride()
		{
			using (var form = new ZForm())
			using (var whsPickingUserControl = new WhsPickingUserControl())
			{
				form.Controls.Add(whsPickingUserControl);
				form.Show();
				var styles = whsPickingUserControl.FindSingle<ZGrid>(c => c.Name == "PickPackGrid").ColumnStyles.Cast<ZGridColumnInfo>();

				var dockDoorOverride = styles.SingleOrDefault(s => s.ColumnName == WhsClientPickPackParamsByWhsSchema.WPP_AllowPickDockDoorLocationOverride.Name);
				AssertEquals("WPP_AllowPickDockDoorLocationOverride should be correctly available", false, dockDoorOverride.IsUnavailable);
			}
		}

		#endregion

		#endregion
	}
}
