using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.GUI.Testing
{
	[TestedType(typeof(TransportBookingForm))]
	public class TransportBookingFormTest : DtbBookingZFormBasherTest
	{
		public void TestControllerID()
		{
			using (var form = new TransportBookingFormForTest(Helper.CreateBooking()))
			{
				form.Show();
				AssertEquals(false, form.InstructionViewsControlForTest.IsAdditionalReferencesVisible);
				AssertEquals(ControllerIDs.DtbBooking, form.ControllerID);
			}
		}

		public void TestDeactivateBooking_WithNoParent()
		{
			var booking = Helper.CreateBooking();
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				var deactivateMenuItem = form.FindDeactivateActionsMenuItem();
				deactivateMenuItem.PerformClick();

				AssertEquals("Booking should be cancelled.", true, booking.IsCancelled);
				AssertEquals("Standalone Booking should not have Billing PlugIn disabled.", true, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled);
			}
		}

		public void TestDeactivateBooking_WithParent()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				var deactivateMenuItem = form.FindDeactivateActionsMenuItem();
				deactivateMenuItem.PerformClick();

				AssertEquals("Booking should be cancelled.", true, booking.IsCancelled);
				AssertEquals("Booking with Parent should have Billing PlugIn disabled.", false, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled);
			}
		}

		public void TestFormCaption()
		{
			var booking = Factory.New<DtbBooking>();
			booking.KM_JobID = "TB012345678";
			using (var form = new TransportBookingFormForTest(booking))
			{
				AssertEquals("Transport Booking TB012345678", form.FormCaption);
			}
		}

		public void TestHoldMenuItem()
		{
			var booking = Helper.CreateBooking();
			using (var form = new TransportBookingForm(booking))
			{
				AssertEquals("Precondition", booking.IsHeld, false);

				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Hold Booking");
				transportBookingMenu.PerformClick();
				AssertEquals("Booking held", booking.IsHeld, true);

				transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Remove Held Status");
				transportBookingMenu.PerformClick();
				AssertEquals("Booking unheld", booking.IsHeld, false);
			}
		}

		public void TestBookingWithoutParentHasRoutingTabAndNoSailingDetails()
		{
			var booking = Helper.CreateBooking();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertNull(form.RoutingScheduleTabPageForTest);
				var routing = form.PlugIns.GetPlugIn(ControllerIDs.Routing);
				AssertNotNull(routing);
				Assert(routing.UserControl.Enabled);
			}
		}

		public void TestBookingWithQuotedBookingParent()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var builder = ObjectFactory.New<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
			var quotedBooking = (BusinessObject)builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			consolidation.KB_ParentID = quotedBooking.PK;
			consolidation.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertNotNull(form.RoutingScheduleTabPageForTest);
				var routing = form.PlugIns.GetPlugIn(ControllerIDs.Routing);
				AssertNull(routing);
			}
		}

		public void TestValidateSendingToCTOMenuItem_Exists()
		{
			var booking = Helper.CreateBooking();

			using (var form = new TransportBookingForm(booking))
			{
				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");

				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var validateMessageToCTOMenuItem = topLevelActionsMenu.MenuItems.FindByName("ValidateMessageToCTO");

				AssertNotNull("'ValidateMessageToCTO' control should exist in the Actions menu", validateMessageToCTOMenuItem);
				AssertEquals("'Validate Message To CTO' button text should show correctly", "Validate Message To CTO", validateMessageToCTOMenuItem.Text);
			}
		}

		public void TestValidateSendingToCTO_Passes()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var validateMessageToCTOMenuItem = form.ActionsMenuForTest.MenuItems.FindByName("ValidateMessageToCTO");

				validateMessageToCTOMenuItem.PerformClick();

				var lastMessageAfterClick = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("NotificationBufferForSendingXUSToCTO should be empty since there should be no errors after validation", 0, booking.NotificationBufferForSendingXUSToCTO.Events.Count());
					AssertEquals("Validation successful message caption should display correctly", "Transport Booking Validation For Sending To CTO Successful", lastMessageAfterClick.Caption);
					AssertEquals("Validation successful message text should display correctly", "Transport Booking Successfully Validated\r\n", lastMessageAfterClick.Text);
					AssertEquals("IsValidatingSendingXUSToCTO should be reset to false after validation", false, booking.IsValidatingSendingXUSToCTO);
					AssertEquals("Validation successful message should display with information logo", true, lastMessageAfterClick.WasInformation);
				});
			}
		}

		public void TestValidateSendingToCTO_Fails()
		{
			var consol = Factory.New<IForwardingConsol>() as IDtbBookingParent;
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO(consol);

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var validateMessageToCTOMenuItem = form.ActionsMenuForTest.MenuItems.FindByName("ValidateMessageToCTO");

				validateMessageToCTOMenuItem.PerformClick();

				var lastMessageAfterClick = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertGreaterThan("NotificationBufferForSendingXUSToCTO should not be empty since there should be errors after validation", booking.NotificationBufferForSendingXUSToCTO.Events.Count(), 0);
					AssertEquals("Validation unsuccessful message caption should display correctly", "Transport Booking Failed Validation For Sending To CTO", lastMessageAfterClick.Caption);
					AssertContains("Validation unsuccessful message text should display correctly", "Transport Booking Validation failed for the following reasons:", lastMessageAfterClick.Text);
					AssertEquals("IsValidatingSendingXUSToCTO should be reset to false after validation", false, booking.IsValidatingSendingXUSToCTO);
					AssertEquals("Validation failed message should display with error logo", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				});
			}
		}

		public void TestIsValidatingSendingXUSToCTO_ResetToFalseAfter_ValidateAll_ThrowsException()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			var mockValidation = new Mock<DtbBookingValidation>(booking);

			mockValidation.Setup(b => b.ValidateAll()).Throws<Exception>();
			booking.ValidationForTest = mockValidation.Object;

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var validateMessageToCTOMenuItem = form.ActionsMenuForTest.MenuItems.FindByName("ValidateMessageToCTO");

				CombineAssertions(() =>
				{
					AssertExceptionThrown<Exception>(() => validateMessageToCTOMenuItem.PerformClick());
					AssertEquals("IsValidatingSendingXUSToCTO should be reset to false even if an exception is thrown by ValidateAll()", false, booking.IsValidatingSendingXUSToCTO);
				});
			}
		}

		public void TestParentBookingWithRouting()
		{
			var parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var support = (IRoutingSupport)parent;
			var booking = Helper.CreateBooking(Helper.CreateConsolidation((IDtbBookingParent)parent));
			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			AssertEquals("Precondition: New Transports are allowed", true, support.TransportsIncludingRelated.AllowNew);

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertNull(form.RoutingScheduleTabPageForTest);
				var routing = form.PlugIns.GetPlugIn(ControllerIDs.Routing);
				form.MainTabControlForTest.SelectedTab = routing.TabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Should not allow new Transports", false, support.TransportsIncludingRelated.AllowNew);
					AssertCorrectControlsAreDisabled(routing.UserControl);
					CheckButtonWithMessage(routing, "SelectScheduleButton", "Error You can only view Schedules via the parent");
					CheckButtonWithMessage(routing, "GlobalSchedulesButton", "Error You can only import Global Flight Schedules via the parent");
					CheckButtonWithMessage(routing, "ImportGlobalScheduleButton", "Error You can only import Global Sailing Schedules via the parent");
				});
			}
		}

		public void AssertCorrectControlsAreDisabled(Control control)
		{
			var controlsToExclude = new List<string>() { "SelectScheduleButton", "GlobalSchedulesButton", "ImportGlobalScheduleButton" };

			if (control is ZGrid grid)
			{
				AssertEquals(control.Name + " should be ReadOnly.", true, grid.ReadOnly);
				var notReadOnlyColumnStyles = Enumerable.Where(grid.ColumnStyles.ToArray(), x => !((ZGridColumnInfo)x).IsReadOnly);
				AssertContainsExactElementsInAnyOrder("There should be no ColumnStyles where ReadOnly equals false.", Enumerable.Empty<ZGridColumnInfo>(), notReadOnlyColumnStyles);
				AssertEquals(grid.Name + " should be enabled", true, grid.Enabled);
			}
			else if ((control is ZCodeFindBox || control.Controls.Count == 0) && !controlsToExclude.Contains(control.Name) && !(control is DataGridTextBox))
			{
				AssertEquals(control.Name + " should be disabled.", false, control.Enabled);
			}

			foreach (Control subControl in control.Controls)
			{
				AssertCorrectControlsAreDisabled(subControl);
			}
		}

		static void CheckButtonWithMessage(ZPlugIn routing, string buttonName, string message)
		{
			var button = (Button)routing.UserControl.Controls.Find(buttonName, true).First();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			button.PerformClick();
			AssertEquals("Reason", message, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestNotifications_Add()
		{
			var booking = Factory.New<DtbBooking>();
			booking.KM_JobID = "TB012345678";
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Add(CargoWise.ComponentModel.NotificationType.Warning, "You're doing something crazy!");

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("You're doing something crazy!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNotifyReceived_WhenChangeBookingTemplate()
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageID = "PKG00001";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageID = "PKG00002";
			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG00003";

			var booking = consolidation.Bookings.AddNew();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package1, 1);
			AssertEquals(1, booking.AssignedPackages.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new TransportBookingFormForTest(booking))
			{
				var template = Helper.CreateTransportBookingTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);

				booking.KM_KT_NKBookingTemplate = "IFFF";
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Could not find Containers/Outer Packages based on Instruction Template Package Type. See Packages Tab."));
			}
		}

		public void TestOverrideAddress()
		{
			var booking = Helper.CreateBooking();
			using (var form = new TransportBookingForm(booking))
			{
				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Override Transport Company Address");
				transportBookingMenu.PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(typeof(QuickAddressForm), ZFormModaliser.LastFormShownForTest.GetType());

				transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Override Booking Party Address");
				transportBookingMenu.PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(typeof(QuickAddressForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestPlugIns_BookingIsNotMaster()
		{
			var booking = Helper.CreateBooking();
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertFormContainsPlugIn(form, ControllerIDs.DocDataPlugIn);
				AssertFormContainsPlugIn(form, ControllerIDs.DocumentVisualizer);
				AssertFormContainsPlugIn(form, ControllerIDs.DocAddresses);
				AssertFormContainsPlugIn(form, ControllerIDs.Routing);
				AssertFormContainsPlugIn(form, ControllerIDs.Apportionment);
				AssertFormContainsPlugIn(form, ControllerIDs.JobProfitLossConsol);
				AssertFormContainsPlugIn(form, ControllerIDs.JobInvoicingConsol);

				AssertFormContainsPlugIn(form, ControllerIDs.PackingPlugIn);
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.MasterBookingPackingPlugIn);
				AssertNull(String.Format("{0} Should NOT be plugged in", ControllerIDs.MasterBookingPackingPlugIn), plugIn);

				AssertEquals(true, form.IsResizableByTabPageAllowed);
			}
		}

		public void TestPlugIns_BookingIsMaster()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = true;

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertFormContainsPlugIn(form, ControllerIDs.DocDataPlugIn);
				AssertFormContainsPlugIn(form, ControllerIDs.DocumentVisualizer);
				AssertFormContainsPlugIn(form, ControllerIDs.MasterBookingPackingPlugIn);
				AssertFormContainsPlugIn(form, ControllerIDs.DocAddresses);
				AssertFormContainsPlugIn(form, ControllerIDs.Routing);
				AssertFormContainsPlugIn(form, ControllerIDs.Apportionment);
				AssertFormContainsPlugIn(form, ControllerIDs.JobProfitLossConsol);
				AssertFormContainsPlugIn(form, ControllerIDs.JobInvoicingConsol);
				AssertEquals(true, form.IsResizableByTabPageAllowed);
			}
		}

		void AssertFormContainsPlugIn(ZForm form, ControllerID controllerId)
		{
			var plugIn = form.PlugIns.GetPlugIn(controllerId);
			AssertNotNull(String.Format("{0} Should be plugged in", controllerId), plugIn);
		}

		public void TestFormBookingHasConsolParentAccountingTabAndPlugins()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consol = Helper.CreateForwardingConsol(shipment, "C01", "SEA", "M01");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)consol);
			var booking = Helper.CreateBooking(consolidation);
			AssertAccountingTabAndPlugins(booking, true, false);
		}

		public void TestFormBookingHasConsolGatewayParentAccountingTabAndPlugins()
		{
			var agentPort = GlbCompany.CurrentCompany.OrgProxy.AppointedGatewayAgentPorts.AddNew();
			agentPort.O5_OA_AgentOfficeAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPort.O5_SeaAgentStatus = "GTA";
			agentPort.O5_AirAgentStatus = "GTA";
			agentPort.O5_RailAgentStatus = "GTA";
			agentPort.O5_RoadAgentStatus = "GTA";
			agentPort.Factory.Save();

			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consol = Helper.CreateForwardingConsol(shipment, "C01", "SEA", "M01");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)consol);
			var booking = Helper.CreateBooking(consolidation);
			((BusinessObject)consol)[JobConsolSchema.JK_AgentType] = "AGT";
			((BusinessObject)consol)[JobConsolSchema.JK_SendingForwarderHandlingType] = "GTA";
			((BusinessObject)consol)[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
			((BusinessObject)consol)[JobConsolSchema.JK_OA_SendingForwarderAddress] = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertAccountingTabAndPlugins(booking, true, true);
		}

		public void TestFormBookingHasNonConsolParentAccountingTabAndPlugins()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			AssertAccountingTabAndPlugins(booking, false, true);
		}

		public void TestFormBookingIsStandaloneAccountingTabAndPlugins()
		{
			var booking = Helper.CreateBooking();
			AssertAccountingTabAndPlugins(booking, false, true);
		}

		public void AssertAccountingTabAndPlugins(DtbBooking booking, bool expectedVisibleTab, bool expectedJobInvoicingEnabled)
		{
			var expectedPluginsEnabled = expectedVisibleTab;
			var expectedTabStateDescription = expectedVisibleTab ? "visible" : "hidden";
			var expectedPluginStateDescription = expectedPluginsEnabled ? "enabled" : "disabled";
			var expectedJobInvoicingEnabledDescription = expectedJobInvoicingEnabled ? "enabled" : "disabled";

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				CombineAssertions("Check that accounting tab and plugins are " + expectedTabStateDescription, () =>
				{
					var accountingTabInvisibleOrNotInFormControls = form.AccountingTabPageForTest?.TabVisible ?? false;
					AssertEquals("Accounting tab should be " + expectedTabStateDescription, expectedVisibleTab, accountingTabInvisibleOrNotInFormControls);
					AssertEquals("Apportionment Plugin should be " + expectedPluginStateDescription, expectedPluginsEnabled, form.PlugIns.GetPlugIn(ControllerIDs.Apportionment).Enabled);
					AssertEquals("JobProfitLossConsol Plugin should be " + expectedPluginStateDescription, expectedPluginsEnabled, form.PlugIns.GetPlugIn(ControllerIDs.JobProfitLossConsol).Enabled);
					AssertEquals("JobInvoicingConsol Plugin should be " + expectedPluginStateDescription, expectedPluginsEnabled, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicingConsol).Enabled);
					AssertEquals("JobInvoicing Plugin should be " + expectedJobInvoicingEnabledDescription, expectedJobInvoicingEnabled, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled);
				});
			}
		}

		public void TestSetInstructionView()
		{
			var consolidation = Helper.CreateConsolidation();
			var packingJob = Helper.CreatePackageJob(consolidation);
			var container1 = packingJob.Packages.AddNew("CNT");
			var container2 = packingJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");

			var booking = Helper.CreateBooking(consolidation);
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.SetInstructionView(TransportBookingInstructionView.Standard);
				AssertEquals(form.InstructionViewsControl.StandardViewTabPage, form.InstructionViewsControl.ViewTabControl.SelectedTab);
			}

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.SetInstructionView(TransportBookingInstructionView.Instruction);
				AssertEquals(form.InstructionViewsControl.InstructionViewTabPage, form.InstructionViewsControl.ViewTabControl.SelectedTab);
			}
		}

		public void TestTabPages()
		{
			using (var form = new TransportBookingFormForTest(Factory.New<DtbBooking>()))
			{
				AssertEquals(true, form.ShowNotesTabForTest);
			}
		}

		public void TestViewCreateTransportJob()
		{
			var booking = Helper.CreateBooking();
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJob = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				AssertNotNull(createTransportJob);
			}
		}

		public void TestCreateTransportJobWhenBookingHasChanges()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();
			booking.KM_Description = "The description has been changed!";

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				var createDefaultTransportJob = createTransportJobMenu.MenuItems.FindByText("Create Default Transport Job");
				createDefaultTransportJob.PerformClick();

				AssertEquals("Please save the Booking before creating a Transport Job.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTransportJobWhenBookingHasInactiveLTC()
		{
			var booking = Helper.CreateBooking();
			var inactiveLTC = Factory.New<IDtbConsignment>();
			inactiveLTC.LTC_Direction = "LOC";
			inactiveLTC.LTC_IsActive = false;
			inactiveLTC.LTC_KM_Booking = booking.PK;
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertEquals("A new transport job should be able to be made when the only attached transport job is an inactive LTC", true, form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job").Enabled);
			}
		}

		public void TestCreateTransportJobWhenBookingHasBothInactiveAndActiveLTC()
		{
			var booking = Helper.CreateBooking();
			var inactiveLTC = Factory.New<IDtbConsignment>();
			inactiveLTC.LTC_Direction = "LOC";
			inactiveLTC.LTC_IsActive = false;
			inactiveLTC.LTC_KM_Booking = booking.PK;
			var activeLTC = Factory.New<IDtbConsignment>();
			activeLTC.LTC_Direction = "LOC";
			activeLTC.LTC_KM_Booking = booking.PK;
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertEquals("A new transport job should not be able to be made when there is both an inactive LTC and an active LTC as attached transport jobs", false, form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job").Enabled);
			}
		}

		public void TestActionMenuMakeAvailable()
		{
			ActionMenuMakeAvailableCore(TransportStatuses.Codes.Quote);
			ActionMenuMakeAvailableCore(TransportStatuses.Codes.Incomplete);
			ActionMenuMakeAvailableCore(TransportStatuses.Codes.ActionRequired);
		}

		void ActionMenuMakeAvailableCore(string status)
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = status;
			Factory.Save();

			using (var form = new TransportBookingFormForTest(booking))
			{
				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var makeAvailableMenu = topLevelActionsMenu.MenuItems.FindByText("Make Available");
				AssertNotNull(makeAvailableMenu);
				AssertEquals("The Menu Item should be enabled", true, makeAvailableMenu.Enabled);
				makeAvailableMenu.PerformClick();

				var bookingAfterAvailable = Factory.Load<DtbBooking>(booking.PK);
				AssertEquals("Status should be changed to 'Available'", bookingAfterAvailable.KM_Status, TransportStatuses.Codes.Available);
				AssertEquals("The Menu should be deactivated", false, makeAvailableMenu.Enabled);
			}

			using (var form = new TransportBookingFormForTest(booking))
			{
				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var makeAvailableMenu = topLevelActionsMenu.MenuItems.FindByText("Make Available");
				AssertNotNull(makeAvailableMenu);
				AssertEquals("The Menu should be deactivated", false, makeAvailableMenu.Enabled);
			}
		}

		public void TestBookingForGroupBox_ParentLinkLabel()
		{
			var parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var bookingWithoutParent = Helper.CreateBooking(Helper.CreateConsolidation());
			var bookingWithParent = Helper.CreateBooking(Helper.CreateConsolidation((IDtbBookingParent)parent));
			var masterBooking = Helper.CreateBooking(Helper.CreateConsolidation());

			masterBooking.KM_IsMaster = true;

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);

			using (var form = (TransportBookingForm)controller.ShowEditForm(bookingWithoutParent))
			{
				Assert(form.Visible);
				var bookingForGroupBox = GUITestHelper.FindControl<ZGroupBox>(form.Controls, "BookingForGroupBox");
				AssertEquals("Booking", bookingForGroupBox.Text);
				var parentLinkLabel = GUITestHelper.FindControl<ZLinkLabel>(form.Controls, "ParentLinkLabel");
				AssertEquals("The parent link label should not be visible if there is no parent.", false, parentLinkLabel.Visible);
			}

			using (var form = (TransportBookingForm)controller.ShowEditForm(bookingWithParent))
			{
				Assert(form.Visible);
				var bookingForGroupBox = GUITestHelper.FindControl<ZGroupBox>(form.Controls, "BookingForGroupBox");
				AssertEquals("Booking for", bookingForGroupBox.Text);
				var parentLinkLabel = GUITestHelper.FindControl<ZLinkLabel>(form.Controls, "ParentLinkLabel");
				AssertEquals("The parent link label should contain the correct parent job description for bookings with a parent.", bookingWithParent.ConsolidationSingleJob.ParentJobDescription, parentLinkLabel.Text);
			}

			using (var form = (TransportBookingForm)controller.ShowEditForm(masterBooking))
			{
				Assert(form.Visible);
				var bookingForGroupBox = GUITestHelper.FindControl<ZGroupBox>(form.Controls, "BookingForGroupBox");
				AssertEquals("Master Booking", bookingForGroupBox.Text);
				var parentLinkLabel = GUITestHelper.FindControl<ZLinkLabel>(form.Controls, "ParentLinkLabel");
				AssertEquals("The parent link label should not be visible for master bookings.", false, parentLinkLabel.Visible);
			}
		}

		public void TestMasterTBTextBoxVisibility_MasterTBTextBoxNotVisibleWhenRegistryNotEnabled()
		{
			var bookingController = ZControllerFactory.Create(ControllerIDs.DtbBooking);

			var booking = Helper.CreateBooking();

			Factory.Save();

			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = (TransportBookingForm)bookingController.ShowEditForm(booking))
				{
					Assert(form.Visible);
					var masterTBTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "MasterTBTextBox");
					AssertEquals("When registry condition is enabled, the Master TB text box should be visible.", true, masterTBTextBox.Visible);
				}
			}

			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = (TransportBookingForm)bookingController.ShowEditForm(booking))
				{
					Assert(form.Visible);
					var masterTBTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "MasterTBTextBox");
					AssertEquals("When registry condition is disabled, the Master TB text box should not be visible.", false, masterTBTextBox.Visible);
				}
			}
		}

		public void TestMasterTBTextBoxVisibility_MasterTBTextBoxNotVisibleForMasterBookings()
		{
			var bookingController = ZControllerFactory.Create(ControllerIDs.DtbBooking);

			var booking = Helper.CreateBooking();
			var subBooking = Helper.CreateBooking();
			var masterBooking = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = (TransportBookingForm)bookingController.ShowEditForm(booking))
				{
					Assert(form.Visible);
					var masterTBTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "MasterTBTextBox");
					AssertEquals("When registry condition is enabled, the Master TB text box should be visible for normal bookings.", true, masterTBTextBox.Visible);
				}

				using (var form = (TransportBookingForm)bookingController.ShowEditForm(subBooking))
				{
					Assert(form.Visible);
					var masterTBTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "MasterTBTextBox");
					AssertEquals("When registry condition is enabled, the Master TB text box should be visible for sub bookings.", true, masterTBTextBox.Visible);
				}

				using (var form = (TransportBookingForm)bookingController.ShowEditForm(masterBooking))
				{
					Assert(form.Visible);
					var masterTBTextBox = GUITestHelper.FindControl<ZTextBox>(form.Controls, "MasterTBTextBox");
					AssertEquals("When registry condition is enabled, the Master TB text box should not be visible for master bookings.", false, masterTBTextBox.Visible);
				}
			}
		}

		public void TestMultiBookingButton_Click_ConsolidationNotOpen_NoneChildExceptItselfOpen()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var form = (TransportBookingForm)controller.ShowEditForm(booking))
			{
				AssertEquals(true, form.Visible);
				var multiBookingButton = GUITestHelper.FindControl<ZButton>(form.Controls, "MultiBookingButton");
				multiBookingButton.PerformClick();

				AssertEquals("Transport Booking form should be closed.", false, form.Visible);

				var lastController = form.LastControllerForTest;
				AssertEquals(typeof(TransportBookingMultiForm), lastController.LastShownForm.GetType());
				var multiForm = (TransportBookingMultiForm)lastController.LastShownForm;
				AssertEquals(true, multiForm.Visible);
				multiForm.Dispose();
			}
		}

		public void TestMultiBookingButton_Click_ConsolidationNotOpenButAnotherChildBookingAlreadyOpen()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm1 = (TransportBookingForm)controller.ShowEditForm(booking1))
			{
				var singleForm2 = (TransportBookingForm)controller.ShowEditForm(booking2);
				AssertEquals(true, singleForm1.Visible);
				AssertEquals(true, singleForm2.Visible);

				AssertEquals(true, OpenedFormCache.GetInstance().Contains(booking1.PK.ToGuid(), singleForm1.ControllerID.ToString()));
				AssertEquals(true, OpenedFormCache.GetInstance().Contains(booking2.PK.ToGuid(), singleForm2.ControllerID.ToString()));

				var multiBookingButton = GUITestHelper.FindControl<ZButton>(singleForm1.Controls, "MultiBookingButton");
				multiBookingButton.PerformClick();
				var expectedMessage =
@"A Transport Booking screen belonging to the same consolidation is open.

Close the Transport Booking screen and try again.";
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));

				singleForm2.Dispose();
			}
		}

		public void TestMultiBookingButton_Enabled_WhenBookingIsMasterShouldBeFalse()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_IsMaster = true;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm = (TransportBookingForm)controller.ShowEditForm(booking))
			{
				var multiBookingButton = GUITestHelper.FindControl<ZButton>(singleForm.Controls, "MultiBookingButton");
				AssertEquals(false, multiBookingButton.Enabled);
			}
		}

		public void TestMultiBookingButton_Enabled_WhenBookingIsNotMasterShouldBeTrue()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm = (TransportBookingForm)controller.ShowEditForm(booking))
			{
				var multiBookingButton = GUITestHelper.FindControl<ZButton>(singleForm.Controls, "MultiBookingButton");
				AssertEquals(true, multiBookingButton.Enabled);
			}
		}

		public void TestAutoRatingResultNotFoundMessage_EmptyFreightMode()
		{
			var cnr = Helper.CreateOrganisation("CNR");
			var cne = Helper.CreateOrganisation("CNE");
			var client = Helper.CreateOrganisation("CLT");

			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB1";
			Factory.Save();

			var jobHeader = new JobHeader.Loader(booking).TryLoadOrCreate();
			jobHeader.JH_OA_LocalChargesAddr = client.MainAddress.PK;

			var pickup = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var delivery = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			pickup.Address.OrganisationPK = cnr.PK;
			delivery.Address.OrganisationPK = cne.PK;

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var expectedMessage = @"Freight Mode for Transport Booking TB1 is empty. Cannot continue.";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var quoteCostingMenu = form.Menu.MenuItems.FindByText("Quote and Costing");
				var autoRatingMenu = quoteCostingMenu.MenuItems.FindByText("Autorate Quote Charges");
				autoRatingMenu.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			}
		}

		public void TestAutoRatingResultNotFoundMessage_WithoutInstructions()
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB1";
			Factory.Save();

			var client = Helper.CreateOrganisation("CLT");
			var jobHeader = new JobHeader.Loader(booking).TryLoadOrCreate();
			jobHeader.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			AssertEquals("Pre-condition: No Instructions created yet.", 0, booking.Instructions.Count);

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var expectedMessage1 = @"Transport Booking TB1 doesn't have at least 2 Instructions. Cannot continue.";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var quoteCostingMenu = form.Menu.MenuItems.FindByText("Quote and Costing");
				var autoRatingMenu = quoteCostingMenu.MenuItems.FindByText("Autorate Quote Charges");
				autoRatingMenu.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage1));

				var cnr = Helper.CreateOrganisation("CNR");
				var cne = Helper.CreateOrganisation("CNE");
				var pickup = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
				var delivery = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
				pickup.Address.OrganisationPK = cnr.PK;
				delivery.Address.OrganisationPK = cne.PK;

				var expectedMessage2 = @"Freight Mode for Transport Booking TB1 is empty. Cannot continue.";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				autoRatingMenu.PerformClick();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
			}
		}

		public void TestSavingWithoutInstructionDoesNotThrowException()
		{
			var booking = Helper.CreateBooking();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				AssertNoExceptionThrown(() => form.FireSaveButton());
			}
		}

		public void TestNoExceptionThrown_WhenInstructionIsDeleted()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var cnrOrg = Helper.CreateOrganisation("CNR");
			cnrOrg.OH_IsConsignor = true;
			var cneOrg = Helper.CreateOrganisation("CNE");
			cneOrg.OH_IsConsignee = true;

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, cnrOrg.Addresses.MainAddress);
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, cneOrg.Addresses.MainAddress);

			Factory.Save();

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.SetInstructionView(TransportBookingInstructionView.Standard);
				AssertEquals(form.InstructionViewsControl.StandardViewTabPage, form.InstructionViewsControl.ViewTabControl.SelectedTab);

				AssertEquals(1, pickupInstruction.KN_Sequence);
				AssertEquals(2, deliveryInstruction.KN_Sequence);

				form.SetInstructionView(TransportBookingInstructionView.Standard);
				Application.DoEvents();
				System.Threading.Thread.Sleep(1000);

				var standardViewControl = (DtbInstructionStandardViewControl)form.InstructionViewsControl.Controls.Find("StandardInstructionViewPanel", true).Single();
				var dlvInstructionTile = standardViewControl.Controls.Find("DtbInstructionControl", true).Cast<ZUserControl>().Single(c => c.CurrentDataItem == deliveryInstruction);
				var docAddressControl = dlvInstructionTile.Controls.Find("DocAddressControl", true).Single();
				var defaultGroupBox = docAddressControl.Controls.Find("DefaultGroupBox", true).Single();
				var popupButton = (ZButton)defaultGroupBox.Controls.Find("PopupButton", true).Single();

				form.SetInstructionView(TransportBookingInstructionView.Instruction);
				Application.DoEvents();
				System.Threading.Thread.Sleep(1000);

				var instructionViewControl = (TransportBookingInstructionViewControl)form.InstructionViewsControl.Controls.Find("InstructionViewUserControl", true).Single();
				var instructionsGrid = instructionViewControl.InstructionsGrid;
				AssertEquals("Precondition: should have 2 instructions in the grid.", 2, instructionsGrid.List.Count);

				// add a new instruction (DLV, CNE) and delete original delivery instruction
				var columnValues = new Tuple<string, string>[]
				{
					new Tuple<string, string>("KN_InstructionType", "DLV"),
					new Tuple<string, string>("OrganisationType", "CNE"),
					new Tuple<string, string>("Address+OrganisationNameOrPK", ""),
				};
				FindGridColumnsAndSetValues(instructionsGrid, 3, columnValues);

				instructionsGrid.SelectSingleElement(deliveryInstruction);
				var deleteMenuItem = instructionsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(i => i.Text == "&Delete");
				deleteMenuItem.PerformClick();
				Application.DoEvents();

				System.Threading.Thread.Sleep(1000);

				var updatedInstructionControls = standardViewControl.Controls.Find("DtbInstructionControl", true);
				AssertEquals(3, updatedInstructionControls.Length);
				AssertEquals(2, booking.Instructions.Count);
				AssertEquals(true, deliveryInstruction.IsDeleted);
				AssertEquals(1, pickupInstruction.KN_Sequence);

				form.SetInstructionView(TransportBookingInstructionView.Standard);
				Application.DoEvents();
				System.Threading.Thread.Sleep(1000);

				AssertNoExceptionThrown(() =>
				{
					popupButton.PerformClick();
					Application.DoEvents();
					System.Threading.Thread.Sleep(1000);
				});
			}
		}

		void FindGridColumnsAndSetValues(ZGrid grid, int row, Tuple<string, string>[] columnAndValues)
		{
			int foundedColumns = 0;

			int column = 0;
			grid.CurrentCell = new DataGridCell(row, column);

			foreach (var col in grid.Columns)
			{
				if (foundedColumns >= columnAndValues.Length)
				{
					break;
				}

				foreach (var columnValue in columnAndValues)
				{
					var columnName = columnValue.Item1;
					var value = columnValue.Item2;

					if (col.ColumnName == columnName)
					{
						grid.CurrentCell = new DataGridCell(grid.CurrentRowIndex, column);
						var columnStyle = (ZCustomControlColumnStyle)col.ColumnStyle;
						grid.BeginEdit(columnStyle, grid.CurrentRowIndex);
						columnStyle.EditControl.Text = value;
						foundedColumns++;
					}
				}
				column++;

				Application.DoEvents();
				System.Threading.Thread.Sleep(1000);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Factory.New<DtbBooking>();
			using (booking.SuspendSettingHasChanges())
			{
				consolidation.Bookings.Add(booking);
			}
			return new TransportBookingForm(booking);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Transport);
			return factory;
		}

		public void TestDisplayCorrectMenuItems_WhenNoExistingJob_RegistryDisabled()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");

				var createPortTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Port Transport Job");
				AssertNotNull("Create Port Transport Menu item should exist.", createPortTransportMenu);
				AssertEquals("Create Port Transport Menu item should be enabled.", expected: true, createPortTransportMenu.Enabled);

				var createDefaultTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Default Transport Job");
				AssertNull("Create Default Transport Menu item should not exist.", createDefaultTransportMenu);

				var createLandTransportConsignmentMenu = createTransportJobMenu.MenuItems.FindByText("Create Land Transport Consignment");
				AssertNull("Create Land Consignment Menu item should not exist.", createLandTransportConsignmentMenu);
			}
		}

		public void TestDisplayCorrectMenuItems_WhenNoExistingJob_RegistryEnabled()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");

				var createDefaultTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Default Transport Job");
				AssertNotNull("Create Default Transport Menu item should exist.", createDefaultTransportMenu);
				AssertEquals("Create Default Transport Menu item should be enabled.", expected: true, createDefaultTransportMenu.Enabled);

				var createLandTransportConsignmentMenu = createTransportJobMenu.MenuItems.FindByText("Create Land Transport Consignment");
				AssertNotNull("Create Land Consignment Menu item should exist.", createLandTransportConsignmentMenu);
				AssertEquals("Create Land Consignment Menu item should be enabled.", expected: true, createLandTransportConsignmentMenu.Enabled);

				var createPortTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Port Transport Job");
				AssertNotNull("Create Port Transport Menu item should exist.", createPortTransportMenu);
				AssertEquals("Create Port Transport Menu item should be enabled.", expected: true, createPortTransportMenu.Enabled);
			}
		}

		DtbBooking CreateNewBooking()
		{
			var transportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var validConfirmationDate = new ZDateTime(2013, 1, 1, 2, 20, 0, DateTimeKind.Utc); // one hour in the future
			var validJobCreatedDate = new ZDateTime(2013, 1, 1, 0, 50, 0, DateTimeKind.Utc); // 30 minutes old

			var consolidation = Helper.CreateConsolidation();
			var container1 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT123");
			var container2 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT456");
			var container3 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT789");
			container1.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;
			container2.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;
			container3.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;

			var pallet1 = container1.Packages.AddNew("PLT", 1);
			var pallet2 = container2.Packages.AddNew("BAG", 1);
			var pallet3 = container3.Packages.AddNew("BND", 1);
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet2.Packages.AddNew("CTN");
			var box3 = pallet2.Packages.AddNew("PCE");

			var topLevelPallet = consolidation.PackageJob.Packages.AddNew("DRM", "TopLevelDrum");

			var booking = Helper.CreateBooking(consolidation);
			booking.Address.OrganisationPK = transportCompany.PK;
			booking.KM_KT_NKBookingTemplate = "MPDV";
			booking.Address.Organisation.OH_IsLocalTransport = true;
			booking.Address.Organisation.OH_IsShippingProvider = true;

			var instructions = booking.Instructions.OrderBy(b => b.KN_Sequence);
			instructions.ElementAt(0).DivotsWithPackages.RemovePackage(topLevelPallet);
			instructions.ElementAt(1).PackageDivots.DeleteAll();
			instructions.ElementAt(1).DivotsWithPackages.AddPackage(pallet1);
			instructions.ElementAt(2).PackageDivots.DeleteAll();
			instructions.ElementAt(2).DivotsWithPackages.AddPackage(pallet2);
			instructions.ElementAt(3).PackageDivots.DeleteAll();
			instructions.ElementAt(3).DivotsWithPackages.AddPackage(pallet3);

			foreach (var instruction in booking.Instructions)
			{
				OrgHeader org;
				if (instruction.IsPickUp)
				{
					if (instruction.OrganisationType == LocalCartageJobOrgTypeList.Codes.CFS)
					{
						org = Helper.CreateOrLoadOrganisation("CFS");
						org.OH_IsMiscFreightServices = true;
						org.OH_IsPackDepot = true;
					}
					else
					{
						org = Helper.CreateOrLoadOrganisation("PICKUP");
					}

					instruction.Address.OrganisationPK = org.PK;
				}
				else if (instruction.IsDelivery)
				{
					org = Helper.CreateOrLoadOrganisation("DELIVERY");
					instruction.Address.OrganisationPK = org.PK;
				}
			}

			return booking;
		}

		public void TestMenuDisabled_OnClick_WhenPortTransportJobCreated()
		{
			var booking = CreateNewBooking();
			Factory.Save();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				AssertNotNull(createTransportJobMenu);
				AssertEquals(true, createTransportJobMenu.Enabled);

				var createPortTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Port Transport Job");
				AssertNotNull("Create Port Transport Menu item should exist.", createPortTransportMenu);
				AssertEquals("Create Port Transport Menu item should be enabled.", expected: true, createPortTransportMenu.Enabled);

				createPortTransportMenu.PerformClick();

				var query = new ZQuery(JobCartageSchema.JJ_ParentID, booking.PK);
				query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);
				var portTransport = Factory.LoadTop1<ICommonCartage>(query);
				AssertNotNull("Port transport job should exist in database.", portTransport);

				var lastMessageAfterClick = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals($"Successfully created Port Transport {((BusinessObject)portTransport)[JobCartageSchema.JJ_ConsignmentID]} from Transport Booking {booking.KM_JobID}\r\n", lastMessageAfterClick);

				createTransportJobMenu.ShowPopupMenu();

				AssertNotNull("Create Transport Job submenu should exist.", createTransportJobMenu);
				AssertEquals("Create Transport Job submenu should not be enabled.", expected: false, createTransportJobMenu.Enabled);
			}
		}

		public void TestMenuDisabled_OnClick_WhenLandTransportConsignmentCreated()
		{
			var booking = CreateNewBooking();
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				var createLandTransportConsignmentMenu = createTransportJobMenu.MenuItems.FindByText("Create Land Transport Consignment");
				AssertNotNull("Create Land Consignment Menu item should exist.", createLandTransportConsignmentMenu);
				AssertEquals("Create Land Consignment Menu item should be enabled.", true, createLandTransportConsignmentMenu.Enabled);

				createLandTransportConsignmentMenu.PerformClick();

				var landTransportConsignment = (BusinessObject)Factory.New<IDtbConsignment>();
				landTransportConsignment[DtbConsignmentSchema.LTC_KM_Booking] = booking.PK;

				var lastMessageAfterClick = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Successfully created Land Transport Consignment CN00000001, Land Transport Consignment CN00000002, Land Transport Consignment CN00000003 from Transport Booking TB00000001\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Land transport job should exist in database.", landTransportConsignment);

				createTransportJobMenu.ShowPopupMenu();

				AssertNotNull("Create Transport Job submenu should exist.", createTransportJobMenu);
				AssertEquals("Create Transport Job submenu should not be enabled.", expected: false, createTransportJobMenu.Enabled);
			}
		}

		public void TestMenuDisabled_WhenPortTransportJobExists()
		{
			var booking = Helper.CreateBooking();
			var portTransportJob = Helper.CreatePortTransport(booking);
			Factory.Save();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");

				createTransportJobMenu.ShowPopupMenu();

				AssertNotNull("Create Transport Job submenu should exist.", createTransportJobMenu);
				AssertEquals("Create Transport Job submenu should not be enabled.", expected: false, createTransportJobMenu.Enabled);
			}
		}

		public void TestMenuDisabled_WhenLandTransportConsigmentExists()
		{
			var booking = Helper.CreateBooking();

			var landTransportConsignment = (BusinessObject)Factory.New<IDtbConsignment>();
			landTransportConsignment[DtbConsignmentSchema.LTC_KM_Booking] = booking.PK;
			landTransportConsignment.FillWithValidTestData();

			Factory.Save();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");

				createTransportJobMenu.ShowPopupMenu();

				AssertNotNull("Create Transport Job submenu should exist.", createTransportJobMenu);
				AssertEquals("Create Transport Job submenu should not be enabled.", expected: false, createTransportJobMenu.Enabled);
			}
		}

		public void TestSecurityCheckpoint_OnCreatePortTransportMenuItem()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				var createPortTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Port Transport Job");
				var portTransportAccessModuleCheckpoint = Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.TransportJob);
				var portTransportCheckpoint = Env.Security.TransportJob;
				var portTransportNewCheckpoint = Env.Security.TransportJobNew;

				Assert("Precondition: Port Transport Module security checkpoint should be on by default", portTransportAccessModuleCheckpoint.IsAllowed);
				Assert("Precondition: Port Transport security checkpoint should be on by default", portTransportCheckpoint.IsAllowed);
				Assert("Precondition: Port Transport New security checkpoint should be on by default", portTransportNewCheckpoint.IsAllowed);
				createPortTransportMenu.PerformClick();
				AssertNotContains("Should have permission to use the Port Transport Menu Item by default", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportAccessModuleCheckpoint.IsAllowed = false;
				createPortTransportMenu.PerformClick();
				AssertContains("Can't use the Port Transport Menu Item when the Port Transport Module isn't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportAccessModuleCheckpoint.IsAllowed = true;
				portTransportCheckpoint.IsAllowed = false;
				createPortTransportMenu.PerformClick();
				AssertContains("Can't use the Port Transport Menu Item when port transports aren't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportCheckpoint.IsAllowed = true;
				portTransportNewCheckpoint.IsAllowed = false;
				createPortTransportMenu.PerformClick();
				AssertContains("Can't use the Port Transport Menu Item when new port transports aren't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportAccessModuleCheckpoint.IsAllowed = false;
				portTransportCheckpoint.IsAllowed = false;
				createPortTransportMenu.PerformClick();
				AssertContains("Can't use the Port Transport Menu Item when no checkpoints pass", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportAccessModuleCheckpoint.IsAllowed = true;
				portTransportCheckpoint.IsAllowed = true;
				portTransportNewCheckpoint.IsAllowed = true;
				createPortTransportMenu.PerformClick();
				AssertNotContains("Should have permission to use the Port Transport Menu Item when all checkpoints pass", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityCheckpoint_OnCreateLandTransportConsignmentMenuItem()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				var createLandTransportConsignmentMenu = createTransportJobMenu.MenuItems.FindByText("Create Land Transport Consignment");
				var landTransportAccessModuleCheckpoint = Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.DtbConsignment);
				var landTransportCheckpoint = Env.Security.DtbConsignment;

				Assert("Precondition: Land Transport Module security checkpoint should be on by default", landTransportAccessModuleCheckpoint.IsAllowed);
				Assert("Precondition: Land Transport security checkpoint should be on by default", landTransportCheckpoint.IsAllowed);
				createLandTransportConsignmentMenu.PerformClick();
				AssertNotContains("Should have permission to use the Land Transport Menu Item by default", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				landTransportAccessModuleCheckpoint.IsAllowed = false;
				createLandTransportConsignmentMenu.PerformClick();
				AssertContains("Can't use the Land Transport Menu Item when the Land Transport Module isn't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				landTransportAccessModuleCheckpoint.IsAllowed = true;
				landTransportCheckpoint.IsAllowed = false;
				createLandTransportConsignmentMenu.PerformClick();
				AssertContains("Can't use the Land Transport Menu Item when land transports aren't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				landTransportAccessModuleCheckpoint.IsAllowed = false;
				createLandTransportConsignmentMenu.PerformClick();
				AssertContains("Can't use the Land Transport Menu Item when no checkpoints pass", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				landTransportAccessModuleCheckpoint.IsAllowed = true;
				landTransportCheckpoint.IsAllowed = true;
				createLandTransportConsignmentMenu.PerformClick();
				AssertNotContains("Should have permission to use the Land Transport Menu Item when all checkpoints pass", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityCheckpoint_OnCreateDefaultTransportMenuItem_PortTransportJob()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				var createDefaultTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Default Transport Job");
				var portTransportAccessModuleCheckpoint = Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.TransportJob);

				portTransportAccessModuleCheckpoint.IsAllowed = false;
				createDefaultTransportMenu.PerformClick();
				AssertContains("Can't use the Port Transport Module when the Port Transport Module isn't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportAccessModuleCheckpoint.IsAllowed = true;
				createDefaultTransportMenu.PerformClick();
				AssertNotContains("Should have permission to use the Port Transport Module as only the Land Transport Module is restricted", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				portTransportAccessModuleCheckpoint.IsAllowed = false;
				Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.DtbConsignment).IsAllowed = true;
				createDefaultTransportMenu.PerformClick();
				AssertContains("Can't use the Port Transport Module when the Port Transport Module isn't permitted by security checkpoints, even though Land Transport Module is allowed", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityCheckpoint_OnCreateDefaultTransportMenuItem_LandTransportConsignment()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			serviceTaskOptions.Add(new ServiceTaskCreatorOption() { ContainerMode = AutoCreatorContainerModes.Codes.Loose, TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment });

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();

				var createTransportJobMenu = form.ActionsMenuForTest.MenuItems.FindByText("Create Transport Job");
				var createDefaultTransportMenu = createTransportJobMenu.MenuItems.FindByText("Create Default Transport Job");
				var landTransportAccessModuleCheckpoint = Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.DtbConsignment);

				landTransportAccessModuleCheckpoint.IsAllowed = false;
				createDefaultTransportMenu.PerformClick();
				AssertContains("Can't use the Land Transport Menu Item when the Land Transport Module isn't permitted by security checkpoints", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				landTransportAccessModuleCheckpoint.IsAllowed = true;
				createDefaultTransportMenu.PerformClick();
				AssertNotContains("Should have permission to use the Land Transport Module as only the Port Transport Module is restricted", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				landTransportAccessModuleCheckpoint.IsAllowed = false;
				Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.TransportJob).IsAllowed = true;
				createDefaultTransportMenu.PerformClick();
				AssertContains("Can't use the Land Transport Module when the Land Transport Module isn't permitted by security checkpoints, even though Port Transport Module is allowed", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_Available_WhenRegistryEnabled()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				// Act
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				var menuItem = form.ActionsMenuForTest.MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
				// Assert
				AssertNotNull("Calculate CO2 Emission (CO2e) is available when registry is enabled", menuItem);
				AssertNotNull("CO2e Plugin is added when registry is enabled", co2ePlugin);
			}
		}

		public void TestCalculateCO2eEmissionMenuItem_NotAvailable_WhenRegistryDisabled()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				// Act
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				var menuItem = form.ActionsMenuForTest.MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
				// Assert
				AssertNull("Calculate CO2 Emission (CO2e) is not available when registry is disabled", menuItem);
				AssertNull("CO2e Plugin is not added when registry is disabled", co2ePlugin);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_EHub()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			Factory.Save();

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();

				// Act
				form.ActionsMenuForTest.MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				// Assert
				CombineAssertions(() =>
				{
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
				AssertEquals("TotalCO2e should be 0", 0m, booking.GetTotalCO2e());
				AssertEquals("TotalCO2eForBinding should be set", "Pending", booking.TotalCO2eForBinding);
				AssertEquals("CO2eDistanceInKM should be 0", 0m, booking.GetCO2eDistanceInKM());
				AssertEquals("CO2eStatus should be set", CO2eStatusList.Codes.Pending, booking.GetCO2eStatus());
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_Api()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			Factory.Save();

			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => CO2eTestHelper.GenerateEmissionResponse("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - CO2e UniversalShipment Response.xml"));

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();

				// Act
				form.ActionsMenuForTest.MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				// Assert
				AssertEquals("TotalCO2eForBinding should be set", 10000m, booking.GetTotalCO2e());
				AssertEquals("TotalCO2eForBinding should be set", "10,000", booking.TotalCO2eForBinding);
				AssertEquals("CO2eDistanceInKM should be set", 1500m, booking.GetCO2eDistanceInKM());
				AssertEquals("Shipment CO2eStatus should be set", CO2eStatusList.Codes.Current, booking.GetCO2eStatus());
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenMandatoryDataMissing()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			Factory.Save();

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				var menuItem = form.ActionsMenuForTest.MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");

				void TestMissingField(Action change, string text)
				{
					change.Invoke();
					Factory.Save();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
						AssertContains("Check message text", "The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:", lastMessage.Text);
						AssertContains("Check mandatory field", text, lastMessage.Text);
					});
				}

				// Act & Assert
				TestMissingField(() => { }, "Transport Booking > Details > Instructions is empty");
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenBookingNotSaved()
		{
			// Arrange
			var booking = Helper.CreateBooking();

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new TransportBookingFormForTest(booking))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();

				// Act
				form.ActionsMenuForTest.MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				// Assert
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
					AssertEquals("Check message text", "Please save before calculating greenhouse gas emissions.", lastMessage.Text);
				});
			}
		}

		public void TestJobInvoicingMenuItemExists_WhenBookingIsMaster()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = true;

			using (var form = new TransportBookingFormForTest(booking))
			{
				AssertEquals("Precondition: Ensure that booking has no parent that will trigger accounting tab visibility", null, booking.ConsolidationSingleJob.Parent);
				var billingPagePlugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertEquals("Apportionment is enabled", billingPagePlugIn.Enabled, true);
			}
		}

		public void TestReadOnlyStatusOfBooking_IsUpdatedOnFormLoad()
		{
			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			AssertEquals("Precondition: Booking should be being managed by an authorised Carrier Booking Agent.", true, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
			AssertEquals("Precondition: Booking should not be ReadOnly yet , despite being managed by an authorised Carrier Booking Agent.", false, booking.ReadOnly);

			using (new TransportBookingForm(booking))
			{
				AssertEquals("Booking should now be ReadOnly.", true, booking.ReadOnly);
			}
		}

		public void TestCorrectActionMenuItemsAreDisabled_WhenActionsMenuIsOpened()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.Quote;

			Factory.Save();

			AssertEquals("Precondition: Booking should not be being managed by an authorised Carrier Booking Agent.", false, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
			AssertEquals("Precondition: Booking should not be a Sub Booking.", false, booking.IsSub);

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();

				var topLevelActionsMenu = form.Menu.MenuItems.FindByName("ActionsMenuItem");
				var actionsMenuItems = topLevelActionsMenu.MenuItems;

				CombineAssertions("Precondition: All menu items should be enabled since the booking is not locked down.", () =>
				{
					foreach (MenuItem menuItem in actionsMenuItems)
					{
						AssertEquals(menuItem.Text + " menu item should be enabled.", true, menuItem.Enabled);
					}
				});

				Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
				booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

				AssertEquals("Precondition: Booking should be being managed by an authorised Carrier Booking Agent.", true, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);

				CombineAssertions("No menu items should have been disabled yet since the actions menu hasn't been opened.", () =>
				{
					foreach (MenuItem menuItem in actionsMenuItems)
					{
						AssertEquals(menuItem.Text + " menu item should be enabled.", true, menuItem.Enabled);
					}
				});

				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				CombineAssertions(FormattableString.Invariant($"Only menu items on the {nameof(ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking)} list (except for 'Remove from Favorites') should still be enabled."), () =>
				{
					foreach (MenuItem menuItem in actionsMenuItems)
					{
						var shouldBeEnabled = ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking.Contains(menuItem.Name) && menuItem.Name != "Actions.DeleteFromFavoriteMenuItem";
						AssertEquals(menuItem.Text + " menu item should have correct enabled status.", shouldBeEnabled, menuItem.Enabled);
					}

					foreach (var menuItemName in ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking)
					{
						var menuItem = actionsMenuItems.FindByName(menuItemName);
						AssertNotNull(menuItemName + " menu item should be in Actions menu.", menuItem);
					}
				});

				actionsMenuItems.FindByName("Actions.AddToFavoriteMenuItem").PerformClick();
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				CombineAssertions(FormattableString.Invariant($"Only menu items on the {nameof(ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking)} list (except for 'Add to Favorites') should still be enabled."), () =>
				{
					foreach (MenuItem menuItem in actionsMenuItems)
					{
						var shouldBeEnabled = ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking.Contains(menuItem.Name) && menuItem.Name != "Actions.AddToFavoriteMenuItem";
						AssertEquals(menuItem.Text + " menu item should have correct enabled status.", shouldBeEnabled, menuItem.Enabled);
					}
				});
			}
		}

		public void TestCorrectActionMenuItemsAreDisabled_WhenActionsMenuIsOpenedAndBookingIsLockedDownAndLanguageIsNotEnglish()
		{
			var booking = Helper.CreateBooking();

			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			Factory.Save();

			AssertEquals("Precondition: Booking should be being managed by an authorised Carrier Booking Agent.", true, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.French))
			{
				using (var form = new TransportBookingForm(booking))
				{
					form.Show();

					var topLevelActionsMenu = form.Menu.MenuItems.FindByName("ActionsMenuItem");
					var actionsMenuItems = topLevelActionsMenu.MenuItems;
					topLevelActionsMenu.ShowPopupMenu();

					CombineAssertions(FormattableString.Invariant($"Only menu items on the {nameof(ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking)} list (except for 'Remove from Favorites') should be enabled."), () =>
					{
						foreach (MenuItem menuItem in actionsMenuItems)
						{
							var shouldBeEnabled = ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking.Contains(menuItem.Name) && menuItem.Name != "Actions.DeleteFromFavoriteMenuItem";
							AssertEquals(menuItem.Text + " menu item should have correct enabled status.", shouldBeEnabled, menuItem.Enabled);
						}

						foreach (var menuItemName in ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking)
						{
							var menuItem = actionsMenuItems.FindByName(menuItemName);
							AssertNotNull(menuItemName + " menu item should be in Actions menu.", menuItem);
						}
					});

					actionsMenuItems.FindByName("Actions.AddToFavoriteMenuItem").PerformClick();
					topLevelActionsMenu.OnPopup(EventArgs.Empty);

					CombineAssertions(FormattableString.Invariant($"Now only menu items on the {nameof(ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking)} list (except for 'Add to Favorites') should be enabled."), () =>
					{
						foreach (MenuItem menuItem in actionsMenuItems)
						{
							var shouldBeEnabled = ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking.Contains(menuItem.Name) && menuItem.Name != "Actions.AddToFavoriteMenuItem";
							AssertEquals(menuItem.Text + " menu item should have correct enabled status.", shouldBeEnabled, menuItem.Enabled);
						}
					});
				}
			}
		}

		string[] ActionMenuItemsThatShouldAlwaysBeEnabledOnBooking
		{
			get
			{
				return new string[] {
					"CopyHyperlinkToClipboard",
					"CopyIdToClipboard",
					"CreateDesktopShortcut",
					"CopyFormToClipboardMenuItem",
					"ResetFormSizeToDefault",
					"ViewRelatedCommunications",
					"Actions.AddToFavoriteMenuItem",
					"Actions.DeleteFromFavoriteMenuItem"
				};
			}
		}

		public void TestAllActionsMenuItemsAreEnabled_WhenBookingIsNotSub()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_Status = TransportStatuses.Codes.Quote;

			AssertEquals("Precondition: Booking should not be a Sub.", false, booking.IsSub);

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var actionsMenuItems = form.Menu.MenuItems.FindByText("Actions").MenuItems;
					foreach (MenuItem menuItem in actionsMenuItems)
					{
						AssertEquals(menuItem.Text + " menu item should be enabled.", true, menuItem.Enabled);
					}
				});
			}
		}

		public void TestOrganisationsTabVisibility_OrganisationsTabNotVisibleForMasterBookings()
		{
			var subBooking = Helper.CreateBooking();
			var normalBooking = Helper.CreateBooking();

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			AssertOrganisationsTabVisibility(normalBooking, "When opening a normal booking form, the organisations tab should be visible.", true);
			AssertOrganisationsTabVisibility(subBooking, "When opening a sub booking form, the organisations tab should be visible.", true);
			AssertOrganisationsTabVisibility(masterBooking, "When opening a master booking form, the organisations tab should not be visible.", false);
		}

		void AssertOrganisationsTabVisibility(DtbBooking booking, string message, bool isTabVisible)
		{
			using (var form = new DummyBookingFormForControlBinding(booking))
			{
				var bookingControl = new BookingControl();
				form.AddControlAndShowForm(bookingControl);
				form.Show();
				Application.DoEvents();

				var orgsTab = bookingControl.OrganisationsTab;
				AssertEquals(message, isTabVisible, orgsTab.TabVisible);
			}
		}

		public void TestBookingPartyVisibility_BookingPartyNotVisibleWhenBookingHasParent()
		{
			var parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var bookingWithParent = Helper.CreateBooking(Helper.CreateConsolidation((IDtbBookingParent)parent));
			var bookingWithoutParent = Helper.CreateBooking();

			Factory.Save();

			AssertNotNull("Booking with parent should have a not null parent on its consolidation.", bookingWithParent.ConsolidationSingleJob?.Parent);
			AssertEquals("Booking without parent should have a null parent on its consolidation.", bookingWithoutParent.ConsolidationSingleJob?.Parent, null);

			using (var form = new DummyBookingFormForControlBinding(bookingWithParent))
			{
				var bookingControl = new BookingControl();
				form.AddControlAndShowForm(bookingControl);
				form.Show();
				Application.DoEvents();

				var orgsTab = bookingControl.OrganisationsTab;
				orgsTab.Show();
				var bookingPartyControl = GUITestHelper.FindControl<ZDocAddressControl>(form.Controls, "BookingPartyDocumentaryAddressControl");
				AssertEquals("Booking Party should not be visible as this booking has a parent.", false, bookingPartyControl.Visible);
			}

			using (var form = new DummyBookingFormForControlBinding(bookingWithoutParent))
			{
				var bookingControl = new BookingControl();
				form.AddControlAndShowForm(bookingControl);
				form.Show();
				Application.DoEvents();

				var orgsTab = bookingControl.OrganisationsTab;
				orgsTab.Show();
				var bookingPartyControl = GUITestHelper.FindControl<ZDocAddressControl>(form.Controls, "BookingPartyDocumentaryAddressControl");
				AssertEquals("Booking Party should be visible as this booking has no parent.", true, bookingPartyControl.Visible);
			}
		}
	}

	[TestedType(typeof(TransportBookingForm))]
	public class TransportBookingFormTest_MasterBooking : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = 1;
			Factory.Save();
			return new TransportBookingForm(booking);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Transport);
			return factory;
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
