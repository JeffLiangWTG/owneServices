using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class PreScreenMenuItemTest : TestCaseWithFactory
	{
		public void TestPreScreenHVLVDetails_EnablePreScreening()
		{
			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = false };
			HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration);

			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var originalNotificationsCount = UnitTestUserNotification.Instance.PreviousMessages.Length;
				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<PreScreenMenuItem>().Single();
				menuItem.OnPopup(EventArgs.Empty);

				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertEquals(@"HVLV Pre-Screening has not been enabled.
To enable go to Registry -> Freight -> HVLV -> HVLV Pre-Screening -> Enable HVLV Pre-Screening.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Notification should show when pre-screening is disabled", originalNotificationsCount + 1, UnitTestUserNotification.Instance.PreviousMessages.Length);

				preScreeningConfiguration.IsEnabled = true;
				var rule = preScreeningConfiguration.Rules.AddNew();
				rule.TransportMode = "SEA";

				var field = rule.Fields.AddNew();
				field.FieldDescription = "User Defined";
				field.MacrosScript = "\"<HVC_ConsigneeName>\".Contains(\"error\")";
				field.MessageText = "error";
				field.ValidationRule = "ERR";

				HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration);

				var consignment = shipment.HVLVConsignments.Cast<HVLVConsignment>().First();
				AssertNotNull(consignment);
				consignment.HVC_ConsigneeName = "errorName";

				Factory.Save();
				originalNotificationsCount = UnitTestUserNotification.Instance.PreviousMessages.Length;
				menuItem.PerformClick();
				AssertEquals("No notification should be shown when pre-screening is enabled even with flagged consignments", originalNotificationsCount, UnitTestUserNotification.Instance.PreviousMessages.Length);
			}
		}

		public void TestPreScreenHVLVDetails_OnlyAvailableToRunAfterDataSaved()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				form.BusinessEntity.HasChanges = true;
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<PreScreenMenuItem>().Single();

				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertEquals("Should save before Pre-Screening", @"Please save the form before Pre-Screening", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreScreenHVLVDetails_ShowsProgressForm_AddPreScreeningResults()
		{
			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = "SEA";

			var field = rule.Fields.AddNew();
			field.FieldDescription = "User Defined";
			field.MacrosScript = "\"<HVC_ConsigneeName>\" == \"AA\"";
			field.MessageText = "Add Pre-Screening result after Pre-Screening done";
			field.ValidationRule = "ERR";

			HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration);

			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var consignment = shipment.HVLVConsignments.Cast<HVLVConsignment>().First();

				AssertNotNull(consignment);
				consignment.HVC_ConsigneeName = "AA";

				Factory.Save();

				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<PreScreenMenuItem>().Single();
				AssertNotNull(menuItem);

				menuItem.PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest;

				CombineAssertions("Pre-Screening progress from", () =>
				{
					AssertType<ProgressForm>("Should have shown progress form.", lastForm);
					AssertEquals("progress form caption", "Pre-Screening in progress...", ((ProgressForm)lastForm).CaptionResourceString.Caption);
				});
			}
		}

		public void TestPreScreenHVLVDetails_AddPreScreeningResults()
		{
			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = "SEA";

			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "User Defined";
			field1.MacrosScript = "\"<HVC_ConsigneeName>\".Contains(\"error\")";
			field1.MessageText = "error";
			field1.ValidationRule = "ERR";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "User Defined";
			field2.MacrosScript = "\"<HVC_ConsigneeName>\".Contains(\"warning\")";
			field2.MessageText = "warning";
			field2.ValidationRule = "WRN";

			HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration);

			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var consignment = shipment.HVLVConsignments.Cast<HVLVConsignment>().First();

				AssertNotNull(consignment);
				consignment.HVC_ConsigneeName = "errorwarning";

				Factory.Save();

				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<PreScreenMenuItem>().Single();
				AssertNotNull(menuItem);

				menuItem.PerformClick();

				CombineAssertions("Pre-Screening results", () =>
				{
					AssertContains("error", consignment.PreScreeningErrorDetails);
					AssertContains("warning", consignment.PreScreeningWarningDetails);
				});
			}
		}

		public void TestPreScreenHVLVDetails_CreateHLREvent()
		{
			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = "SEA";

			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "User Defined";
			field1.MacrosScript = "\"<HVC_ConsigneeName>\".Contains(\"error\")";
			field1.MessageText = "error";
			field1.ValidationRule = "ERR";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "User Defined";
			field2.MacrosScript = "\"<HVC_ConsigneeName>\".Contains(\"warning\")";
			field2.MessageText = "warning";
			field2.ValidationRule = "WRN";

			HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration);

			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var consignment = shipment.HVLVConsignments.Cast<HVLVConsignment>().First();

				AssertNotNull(consignment);
				consignment.HVC_ConsigneeName = "errorwarning";

				Factory.Save();

				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<PreScreenMenuItem>().Single();
				AssertNotNull(menuItem);

				menuItem.PerformClick();

				Factory.Save();

				var hlrLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				hlrLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
				hlrLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var totalConsignments);

				CombineAssertions(() =>
				{
					AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
					AssertEquals("Total number of consignments screened is 1", "1", totalConsignments);
				});
			}
		}

		public void TestScreenHVLVDetails_AddHLRLogOnBookingHeader()
		{
			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				using (var form = new HVLVBookingHeaderForm(bookingHeader))
				{
					var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ScreeningHVLVDetails);
					AssertNotNull(menuItem);

					menuItem.PerformClick();

					Factory.Save();

					var hlrLog = bookingHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();

					hlrLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
					hlrLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var totalConsignments);

					CombineAssertions(() =>
					{
						AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
						AssertEquals("Total number of consignments screened is 1", "1", totalConsignments);
					});
				}
			}
		}

		public void TestScreenHVLVDetails_AddHLRLogOnShipment()
		{
			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew();

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var topLevelMenu = plugin.TopLevelMenu;
					topLevelMenu.PerformSelect();

					var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
					var menuItem = topLevelMenuItem.MenuItems.OfType<PreScreenMenuItem>().Single();
					AssertNotNull(menuItem);

					menuItem.PerformClick();

					Factory.Save();

					var hlrLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();

					hlrLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
					hlrLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var totalConsignments);

					CombineAssertions(() =>
					{
						AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
						AssertEquals("Total number of consignments screened is 1", "1", totalConsignments);
					});
				}
			}
		}
	}
}
