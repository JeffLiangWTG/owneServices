using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingMenuProviderTest : DtbBookingTestCaseWithFactory
	{
		public void TestConstructMenu_CallbackIsCalled()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy.SupportedDirections = new[] { DtbBookingDirection.PIC };

			Factory.Save();

			var callbackWasCalled = false;
			Func<bool> callback = () =>
			{
				callbackWasCalled = true;
				return true;
			};

			ZFormModaliser.ShowDialogsInTest = false;

			var provider = new DtbBookingMenuProvider(() => dummy);

			using (var menu = provider.ConstructMenu(callback))
			{
				menu.OnPopup(EventArgs.Empty);

				var createPickupMenu = menu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Transport Booking");
				createPickupMenu.PerformClick();

				AssertEquals(true, callbackWasCalled);
			}
		}

		public void TestSaveBeforeCreatingTransportBooking()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			using (var form = new DtbBookingConsolidationPlugInTest.DtbBookingPlugInFormTest(dummy))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var topLevelActionsMenu = form.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);

				var pickupMenu = transportBookingMenu.MenuItems[0];
				pickupMenu.PerformClick();

				AssertEquals(string.Format("Please save this {0} before creating a Transport Booking.", dummy.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTransportBookingUsesSeparateFactory()
		{
			var setupShipment = SetupParentShipmentForTest();
			var factory = new BusinessObjectFactory();
			var shipment = factory.Load<Forwarding.IForwardingShipment>(setupShipment.PK);
			var shipmentAsBusinessObject = (BusinessObject)shipment;
			var shipmentFactory = shipmentAsBusinessObject.Factory;
			var initialSavesOnShipmentFactory = shipmentFactory.SaveCount;

			using (var form = new DtbBookingConsolidationPlugInTest.DtbBookingPlugInForShipmentFormTest(shipment))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

				var topLevelActionsMenu = form.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);

				AssertEquals("Precondition: Should not have saved any more times on the shipment factory", initialSavesOnShipmentFactory, shipmentFactory.SaveCount);
				var pickupMenu = transportBookingMenu.MenuItems[0];
				pickupMenu.PerformClick();
				var plugin = (DtbBookingConsolidationPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DtbBookingConsolidation);
				plugin.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose();

				var dtbBookingsUnderShipmentQuery = new ZDBOnlyQuery(typeof(DtbBooking));
				var dtbBookingConsolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
				dtbBookingConsolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, shipment.PK);
				dtbBookingsUnderShipmentQuery.AddSubQuery(dtbBookingConsolidationSubQuery, JoinCondition.And);
				var dtbBookingsUnderShipment = factory.Load<DtbBooking>(dtbBookingsUnderShipmentQuery);
				AssertEquals("Should have created booking", 1, dtbBookingsUnderShipment.Length);

				AssertEquals("Creation of Transport Booking process should not have used the shipment factory to save (number of saves on shipment factory should still be the same)", initialSavesOnShipmentFactory, shipmentFactory.SaveCount);
			}
		}

		Forwarding.IForwardingShipment SetupParentShipmentForTest()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipmentBO = helper.CreateForwardingShipment("S123", "", "SEA", "FCL");
			var consolBO = helper.CreateForwardingConsol(shipmentBO, "C123", "SEA", "");
			var containerBO = helper.CreateForwardingContainer(consolBO, "XYZ876", "20GP");
			var packLineBO = helper.CreateForwardingPackline(shipmentBO, containerBO, 1);
			setupFactory.Save();

			return shipmentBO;
		}

		public void TestShowTransportBookingForm_DontOpenDuplicateForm()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationA = Helper.CreateConsolidation(dummy);
			consolidationA.KB_JobDirection = "PIC";
			var bookingA = consolidationA.Bookings.AddNew();

			var consolidationB = Helper.CreateConsolidation();
			var bookingB = consolidationB.Bookings.AddNew();

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var bookingAFromControllerForm = (ZForm)controller.ShowEditForm(bookingA))
			using (var bookingBFromControllerForm = (ZForm)controller.ShowEditForm(bookingB))
			using (var parentWithPluginForm = new DtbBookingConsolidationPlugInTest.DtbBookingPlugInFormTest(dummy))
			{
				parentWithPluginForm.Show();

				var topLevelActionsMenu = parentWithPluginForm.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);

				var pickupMenu = transportBookingMenu.MenuItems[0];
				pickupMenu.PerformClick();

				var dtbPlugin = (DtbBookingConsolidationPlugIn)parentWithPluginForm.PlugIns.GetPlugIn(ControllerIDs.DtbBookingConsolidation);
				var bookingFromParentForm = dtbPlugin.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm;

				AssertNotNull(bookingAFromControllerForm);
				AssertNotNull(bookingBFromControllerForm);
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Same consolidation, so should be same form", bookingAFromControllerForm, bookingFromParentForm);
				AssertNotEquals("Different consolidation, so should be a different form", bookingBFromControllerForm, bookingFromParentForm);
			}
		}

		public void TestShowTransportBookingForm_NoBookings()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";

			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_JobDirection = "PIC";

			Factory.Save();

			AssertEquals("Should be no bookings", 0, consolidation.Bookings.Count);

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var parentWithPluginForm = new DtbBookingConsolidationPlugInTest.DtbBookingPlugInFormTest(dummy))
			{
				parentWithPluginForm.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

				var topLevelActionsMenu = parentWithPluginForm.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);

				var pickupMenu = transportBookingMenu.MenuItems[0];
				pickupMenu.PerformClick();

				var dtbPlugin = (DtbBookingConsolidationPlugIn)parentWithPluginForm.PlugIns.GetPlugIn(ControllerIDs.DtbBookingConsolidation);
				var bookingFromParentForm = dtbPlugin.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm;

				AssertNotNull(bookingFromParentForm);

				var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).DataSource;
				AssertNotNull("Should now have a booking.", booking);
				AssertEquals("Should now have 1 booking.", 1, booking.ConsolidationSingleJob.Bookings.Count);

				bookingFromParentForm.Dispose();
			}
		}

		public void TestShowTransportBookingForm_MultiContainerBooking()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC };
			var consolidation = Helper.CreateConsolidation(bookingParent);
			Factory.Save();
			AssertEquals("Precondition - should be no bookings", 0, consolidation.Bookings.Count);

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var parentWithPluginForm = new DtbBookingConsolidationPlugInTest.DtbBookingPlugInFormTest(bookingParent))
			{
				parentWithPluginForm.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				var plugIn = (DtbBookingConsolidationPlugIn)parentWithPluginForm.PlugIns.GetPlugIn(ControllerIDs.DtbBookingConsolidation);
				var topLevelActionsMenu = parentWithPluginForm.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);
				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);

				// Creating TB with correct parameters
				var createPickupMenu = transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Transport Booking");
				createPickupMenu.PerformClick();
				AssertEquals(false, plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.CombineContainersForTest);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose();

				var createMultiContainerPickupMenu = transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Multi-Container Transport Booking");
				createMultiContainerPickupMenu.PerformClick();
				AssertEquals(true, plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.CombineContainersForTest);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose();

				// Viewing TB with correct parameters
				var singleContainerBooking = Helper.CreateBooking(consolidation);
				transportBookingMenu.OnPopup(EventArgs.Empty);
				var viewPickupMenu = transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "View/Edit Pickup Transport Booking");
				viewPickupMenu.PerformClick();
				AssertEquals(false, plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.CombineContainersForTest);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose();
			}
		}

		public void TestShowTransportBookingForm_SecurityCheck()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.Z0_Description = "DUM456";

			bookingParent.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC };
			var consolidation = Helper.CreateConsolidation(bookingParent);
			Factory.Save();
			AssertEquals("Precondition - should be no bookings", 0, consolidation.Bookings.Count);

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var parentWithPluginForm = new DtbBookingConsolidationPlugInTest.DtbBookingPlugInFormTest(bookingParent))
			{
				parentWithPluginForm.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				var plugIn = (DtbBookingConsolidationPlugIn)parentWithPluginForm.PlugIns.GetPlugIn(ControllerIDs.DtbBookingConsolidation);
				var topLevelActionsMenu = parentWithPluginForm.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);
				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);

				// Creating New TBs.

				// No security
				Env.Security.DtbBookingNew.IsAllowed = false;
				var createPickupMenu = transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Transport Booking");
				createPickupMenu.PerformClick();
				AssertEquals("No permision to create new TBs.", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers(); // clean up

				// With security
				Env.Security.DtbBookingNew.IsAllowed = true;
				createPickupMenu.PerformClick();
				AssertEquals("No security errors expected.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose(); // clean up

				// Viewing/Editing TBs

				// No security
				Env.Security.DtbBookingView.IsAllowed = false;
				Env.Security.DtbBookingEdit.IsAllowed = false;
				Env.Security.DtbBookingCRMSecurity.DisableCRMSecurityForTesting(false);
				//var booking1 = Helper.CreateBooking(consolidation);
				//Factory.Save();
				transportBookingMenu.OnPopup(EventArgs.Empty);
				var viewPickupMenu = transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "View/Edit Pickup Transport Booking");
				viewPickupMenu.PerformClick();
				AssertEquals("No permision to viewing/editing TBs.", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers(); // clean up

				// With security View
				Env.Security.DtbBookingView.IsAllowed = true;
				viewPickupMenu.PerformClick();
				AssertEquals("No security errors expected.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose(); // clean up
				Env.Security.DtbBookingView.IsAllowed = false; // clean up

				// With security Edit
				Env.Security.DtbBookingEdit.IsAllowed = true;
				Env.Security.DtbBookingCRMSecurity.DisableCRMSecurityForTesting(true);
				Env.Security.DtbBookingConsolidationEdit.IsAllowed = false;
				viewPickupMenu.PerformClick();
				AssertEquals("No security errors expected.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose(); // clean up
				Env.Security.DtbBookingEdit.IsAllowed = false; // clean up
				Env.Security.DtbBookingCRMSecurity.DisableCRMSecurityForTesting(false);

				// Viewing/Editing Consolidation

				// No security
				Env.Security.DtbBookingConsolidationView.IsAllowed = false;
				Env.Security.DtbBookingConsolidationEdit.IsAllowed = false;
				var booking2 = Helper.CreateBooking(consolidation);
				Factory.Save();
				transportBookingMenu.OnPopup(EventArgs.Empty);
				viewPickupMenu = transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "View/Edit Pickup Transport Booking");
				viewPickupMenu.PerformClick();
				AssertEquals("No permision to viewing/editing Consolidation.", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers(); // clean up

				// With security View
				Env.Security.DtbBookingConsolidationView.IsAllowed = true;
				viewPickupMenu.PerformClick();
				AssertEquals("No security errors expected.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose(); // clean up
				Env.Security.DtbBookingConsolidationView.IsAllowed = false; // clean up

				// With security Edit
				Env.Security.DtbBookingConsolidationEdit.IsAllowed = true;
				viewPickupMenu.PerformClick();
				AssertEquals("No security errors expected.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				plugIn.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm.Dispose(); // clean up
			}
		}

		public void TestRefreshMenuItems_ParentWithoutBookings()
		{
			// If parent is active and does not have any transport bookings then child menus should have 'create' transport booking entries (pick and delivery) only
			IDtbBookingParent selectedBookingParent = null;
			var provider = new DtbBookingMenuProvider(() => selectedBookingParent);

			using (var menu = provider.ConstructMenu())
			{
				AssertEquals("Menu should be named Transport Booking", "Transport Booking", menu.Text);

				menu.OnPopup(EventArgs.Empty);
				AssertEquals(1, menu.MenuItems.Count);
				AssertEquals("Menu should have an item displaying No Actions Available", "No Actions Available", menu.MenuItems[0].Text);

				var parent1 = Factory.New<DummyWithDtbBooking>();
				parent1.SupportedDirections = new[] { DtbBookingDirection.PIC };
				selectedBookingParent = parent1;

				menu.OnPopup(EventArgs.Empty);
				AssertRefreshMenuItems_ParentWithoutBookings(selectedBookingParent, menu);
				AssertEquals("Menu should have two items", 2, menu.MenuItems.Count);

				// with a parent that does not support IsCancelled still can create
				var parent2 = Factory.New<DummyWithDtbBookingWithoutICancellable>();
				selectedBookingParent = parent2;
				parent2.SupportedDirections = new[] { DtbBookingDirection.PIC };

				menu.OnPopup(EventArgs.Empty);
				AssertRefreshMenuItems_ParentWithoutBookings(selectedBookingParent, menu);
				AssertEquals("Menu should have two items", 2, menu.MenuItems.Count);

				var parent3 = Factory.New<DummyWithDtbBooking>();
				parent3.SupportedDirections = new[] { DtbBookingDirection.DLV };
				selectedBookingParent = parent3;

				menu.OnPopup(EventArgs.Empty);
				AssertRefreshMenuItems_ParentWithoutBookings(selectedBookingParent, menu);
				AssertEquals("Menu should have two items", 2, menu.MenuItems.Count);

				// with a parent that does not support IsCancelled still can create
				var parent4 = Factory.New<DummyWithDtbBookingWithoutICancellable>();
				selectedBookingParent = parent4;
				parent4.SupportedDirections = new[] { DtbBookingDirection.DLV };

				menu.OnPopup(EventArgs.Empty);
				AssertRefreshMenuItems_ParentWithoutBookings(selectedBookingParent, menu);
				AssertEquals("Menu should have two items", 2, menu.MenuItems.Count);

				var parent5 = Factory.New<DummyWithDtbBooking>();
				parent5.SupportedDirections = new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
				selectedBookingParent = parent5;

				menu.OnPopup(EventArgs.Empty);
				AssertRefreshMenuItems_ParentWithoutBookings(selectedBookingParent, menu);
				var menuItems = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have a separator item", 1, menuItems.Count(i => i.Text == "-"));
				AssertEquals("Menu should have five items", 5, menuItems.Count());

				var parent6 = Factory.New<DummyWithDtbBookingWithoutICancellable>();
				parent6.SupportedDirections = new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
				selectedBookingParent = parent6;

				menu.OnPopup(EventArgs.Empty);
				AssertRefreshMenuItems_ParentWithoutBookings(selectedBookingParent, menu);
				menuItems = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have a separator item", 1, menuItems.Count(i => i.Text == "-"));
				AssertEquals("Menu should have five items", 5, menuItems.Count());
			}
		}

		void AssertRefreshMenuItems_ParentWithoutBookings(IDtbBookingParent selectedBookingParent, MenuItem menu)
		{
			var menuItems = menu.MenuItems.Cast<MenuItem>();

			foreach (var direction in selectedBookingParent.GetSupportedDirections())
			{
				if (direction == DtbBookingDirection.PIC)
				{
					AssertEquals("Menu should have one item displaying Create Pickup Transport Booking", 1, menuItems.Count(i => i.Text == "Create Pickup Transport Booking"));
					AssertEquals("Menu should have one item displaying Create Pickup Multi-Container Transport Booking", 1, menuItems.Count(i => i.Text == "Create Pickup Multi-Container Transport Booking"));
				}
				else if (direction == DtbBookingDirection.DLV)
				{
					AssertEquals("Menu should have one item displaying Create Delivery Transport Booking", 1, menuItems.Count(i => i.Text == "Create Delivery Transport Booking"));
					AssertEquals("Menu should have one item displaying Create Delivery Multi-Container Transport Booking", 1, menuItems.Count(i => i.Text == "Create Delivery Multi-Container Transport Booking"));
				}
				else
				{
					Assert("Unrecognised Direction: " + direction, true);
				}
			}
		}

		public void TestRefreshMenuItems_CancelledParentWithoutBookings()
		{
			// If parent is cancelled and does not have any bookings then child menu should show “No transport bookings” and be disabled (same as “No Actions Available”)
			IDtbBookingParent selectedBookingParent = null;
			var provider = new DtbBookingMenuProvider(() => selectedBookingParent);

			using (var menu = provider.ConstructMenu())
			{
				AssertEquals("Menu should be named Transport Booking", "Transport Booking", menu.Text);

				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu should have the first item displaying No Actions Available", "No Actions Available", menu.MenuItems[0].Text);
				AssertEquals("Menu item should be disabled", false, menu.MenuItems[0].Enabled);
				AssertEquals("Menu should have one item", 1, menu.MenuItems.Count);

				var cancelledBookingParent1 = CreateCancelledBookingParent();
				selectedBookingParent = cancelledBookingParent1;
				AssertRefreshMenuItems_CancelledParentWithoutBookings(cancelledBookingParent1, new[] { DtbBookingDirection.PIC }, menu);

				var cancelledBookingParent2 = CreateCancelledBookingParent();
				selectedBookingParent = cancelledBookingParent2;
				AssertRefreshMenuItems_CancelledParentWithoutBookings(cancelledBookingParent2, new[] { DtbBookingDirection.DLV }, menu);

				var cancelledBookingParent3 = CreateCancelledBookingParent();
				selectedBookingParent = cancelledBookingParent3;
				AssertRefreshMenuItems_CancelledParentWithoutBookings(cancelledBookingParent3, new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, menu);
			}
		}

		void AssertRefreshMenuItems_CancelledParentWithoutBookings(DummyWithDtbBooking cancelledBookingParent, DtbBookingDirection[] supportedDirections, MenuItem menu)
		{
			Assert("Precondition: Parent should be cancelled.", cancelledBookingParent.IsCancelled);
			cancelledBookingParent.SupportedDirections = supportedDirections;

			menu.OnPopup(EventArgs.Empty);
			AssertEquals("Menu should have the first item displaying No Transport Bookings", "No Transport Bookings", menu.MenuItems[0].Text);
			AssertEquals("Menu item should be disabled", false, menu.MenuItems[0].Enabled);
			AssertEquals("Menu should have one item", 1, menu.MenuItems.Count);
		}

		public void TestRefreshMenuItems_ParentWithExistingTransportBookings()
		{
			// If parent is active and parent has transport booking then child menu should have create and view transport booking (pickup and delivery)(no change)
			IDtbBookingParent selectedBookingParent = null;
			var provider = new DtbBookingMenuProvider(() => selectedBookingParent);

			using (var menu = provider.ConstructMenu())
			{
				AssertEquals("Menu should be named Transport Booking", "Transport Booking", menu.Text);

				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu should have one item", 1, menu.MenuItems.Count);
				AssertEquals("Menu should have the first item displaying No Actions Available", "No Actions Available", menu.MenuItems[0].Text);

				// boxes
				var bookingParent1 = Factory.New<DummyWithDtbBooking>();
				selectedBookingParent = bookingParent1;
				AssertRefreshMenuItems_ParentWithExistingTransportBoxBookings(bookingParent1, new[] { DtbBookingDirection.PIC }, menu);

				var bookingParent2 = Factory.New<DummyWithDtbBooking>();
				selectedBookingParent = bookingParent2;
				AssertRefreshMenuItems_ParentWithExistingTransportBoxBookings(bookingParent2, new[] { DtbBookingDirection.DLV }, menu);

				var bookingParent3 = Factory.New<DummyWithDtbBooking>();
				selectedBookingParent = bookingParent3;
				AssertRefreshMenuItems_ParentWithExistingTransportBoxBookings(bookingParent3, new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, menu);

				// containers
				var bookingParent4 = Factory.New<DummyWithDtbBooking>();
				selectedBookingParent = bookingParent4;
				AssertRefreshMenuItems_ParentWithExistingTransportContainerBookings(bookingParent4, new[] { DtbBookingDirection.PIC }, menu);

				var bookingParent5 = Factory.New<DummyWithDtbBooking>();
				selectedBookingParent = bookingParent5;
				AssertRefreshMenuItems_ParentWithExistingTransportContainerBookings(bookingParent5, new[] { DtbBookingDirection.DLV }, menu);

				var bookingParent6 = Factory.New<DummyWithDtbBooking>();
				selectedBookingParent = bookingParent6;
				AssertRefreshMenuItems_ParentWithExistingTransportContainerBookings(bookingParent6, new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, menu);
			}
		}

		void AssertRefreshMenuItems_ParentWithExistingTransportBoxBookings(DummyWithDtbBooking bookingParent, DtbBookingDirection[] supportedDirections, MenuItem menu)
		{
			bookingParent.SupportedDirections = supportedDirections;

			foreach (var direction in supportedDirections)
			{
				var directionDescription = DtbBookingDirectionDescription.GetDescription(direction);
				var viewEditText = string.Format("View/Edit {0} Transport Booking", directionDescription);
				var createBookingText = string.Format("Create {0} Transport Booking", directionDescription);
				var createContainerBookingText = string.Format("Create {0} Multi-Container Transport Booking", directionDescription);
				var createOverrideBookingText = string.Format("Create/Override {0} Transport Booking", directionDescription);
				var createOverrideContainerBookingText = string.Format("Create/Override {0} Multi-Container Transport Booking", directionDescription);

				menu.OnPopup(EventArgs.Empty);
				var menuItems1 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have one item displaying " + createBookingText, 1, menuItems1.Count(i => i.Text == createBookingText));
				AssertEquals("Menu should have one item displaying " + createContainerBookingText, 1, menuItems1.Count(i => i.Text == createContainerBookingText));

				var consol = Helper.CreateConsolidation(bookingParent);
				consol.KB_JobDirection = direction.ToString();
				var box1 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Box, "BX123");
				var box2 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Box, "BX456");

				var singleBoxBooking = Helper.CreateBooking(consol);
				Factory.Save();

				menu.OnPopup(EventArgs.Empty);
				var menuItems2 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have one item displaying " + viewEditText, 1, menuItems2.Count(i => i.Text == viewEditText));
				AssertEquals("Menu should have one item displaying " + createOverrideBookingText, 1, menuItems2.Count(i => i.Text == createOverrideBookingText));
				AssertEquals("Menu should have one item displaying " + createContainerBookingText, 1, menuItems2.Count(i => i.Text == createContainerBookingText));
				AssertEquals("Menu should not have any item displaying " + createBookingText, 0, menuItems2.Count(i => i.Text == createBookingText));
				AssertEquals("Menu should not have any item displaying " + createOverrideContainerBookingText, 0, menuItems2.Count(i => i.Text == createOverrideContainerBookingText));
				AssertEquals("Menu should not have any item displaying No Transport Bookings", 0, menuItems2.Count(i => i.Text == "No Transport Bookings"));

				var multiBoxBooking = Helper.CreateBooking(consol, "IFFD", "Test", direction.ToString(), "REF");

				var box3 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Box, "BX789");
				var box4 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Box, "BX012");

				multiBoxBooking.Instructions[0].DivotsWithPackages.AddPackage(box3);
				multiBoxBooking.Instructions[0].DivotsWithPackages.AddPackage(box4);
				Factory.Save();

				menu.OnPopup(EventArgs.Empty);
				var menuItems3 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have one item displaying " + viewEditText, 1, menuItems3.Count(i => i.Text == viewEditText));
				AssertEquals("Menu should have one item displaying " + createOverrideBookingText, 1, menuItems3.Count(i => i.Text == createOverrideBookingText));
				AssertEquals("Menu should have one item displaying " + createContainerBookingText, 1, menuItems3.Count(i => i.Text == createContainerBookingText));
				AssertEquals("Menu should not have any item displaying " + createBookingText, 0, menuItems3.Count(i => i.Text == createBookingText));
				AssertEquals("Menu should not have any item displaying " + createOverrideContainerBookingText, 0, menuItems3.Count(i => i.Text == createOverrideContainerBookingText));
				AssertEquals("Menu should not have an item displaying No Transport Bookings", 0, menuItems3.Count(i => i.Text == "No Transport Bookings"));
			}
		}

		void AssertRefreshMenuItems_ParentWithExistingTransportContainerBookings(DummyWithDtbBooking bookingParent, DtbBookingDirection[] supportedDirections, MenuItem menu)
		{
			bookingParent.SupportedDirections = supportedDirections;

			foreach (var direction in supportedDirections)
			{
				var directionDescription = DtbBookingDirectionDescription.GetDescription(direction);
				var viewEditText = string.Format("View/Edit {0} Transport Booking", directionDescription);
				var createBookingText = string.Format("Create {0} Transport Booking", directionDescription);
				var createContainerBookingText = string.Format("Create {0} Multi-Container Transport Booking", directionDescription);
				var createOverrideBookingText = string.Format("Create/Override {0} Transport Booking", directionDescription);
				var createOverrideContainerBookingText = string.Format("Create/Override {0} Multi-Container Transport Booking", directionDescription);

				menu.OnPopup(EventArgs.Empty);
				var menuItems1 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have one item displaying " + createBookingText, 1, menuItems1.Count(i => i.Text == createBookingText));
				AssertEquals("Menu should have one item displaying " + createContainerBookingText, 1, menuItems1.Count(i => i.Text == createContainerBookingText));

				var consol = Helper.CreateConsolidation(bookingParent);
				consol.KB_JobDirection = direction.ToString();
				var container1 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT123");
				var container2 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT456");

				var twentyGPO = Helper.LoadRefContainer("20GP");
				container1.Container.K0_RC_ContainerType = twentyGPO.PK;
				container2.Container.K0_RC_ContainerType = twentyGPO.PK;

				var singleContainerBooking = Helper.CreateBooking(consol);
				Factory.Save();

				menu.OnPopup(EventArgs.Empty);
				var menuItems2 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have one item displaying " + viewEditText, 1, menuItems2.Count(i => i.Text == viewEditText));
				AssertEquals("Menu should have one item displaying " + createOverrideBookingText, 1, menuItems2.Count(i => i.Text == createOverrideBookingText));
				AssertEquals("Menu should have one item displaying " + createContainerBookingText, 1, menuItems2.Count(i => i.Text == createContainerBookingText));
				AssertEquals("Menu should have one item displaying " + createOverrideContainerBookingText, 0, menuItems2.Count(i => i.Text == createOverrideContainerBookingText));
				AssertEquals("Menu should not have any item displaying " + createBookingText, 0, menuItems2.Count(i => i.Text == createBookingText));
				AssertEquals("Menu should not have any item displaying No Transport Bookings", 0, menuItems2.Count(i => i.Text == "No Transport Bookings"));

				var multiContainerBooking = Helper.CreateBooking(consol, "IFFD", "Test", direction.ToString(), "REF");
				multiContainerBooking.Instructions[0].DivotsWithPackages.AddPackage(container1);
				multiContainerBooking.Instructions[0].DivotsWithPackages.AddPackage(container2);
				Factory.Save();

				menu.OnPopup(EventArgs.Empty);
				var menuItems3 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have one item displaying " + viewEditText, 1, menuItems3.Count(i => i.Text == viewEditText));
				AssertEquals("Menu should have one item displaying " + createOverrideBookingText, 1, menuItems3.Count(i => i.Text == createOverrideBookingText));
				AssertEquals("Menu should have one item displaying " + createOverrideContainerBookingText, 1, menuItems3.Count(i => i.Text == createOverrideContainerBookingText));
				AssertEquals("Menu should not have any item displaying " + createBookingText, 0, menuItems3.Count(i => i.Text == createBookingText));
				AssertEquals("Menu should not have any item displaying " + createContainerBookingText, 0, menuItems3.Count(i => i.Text == createContainerBookingText));
				AssertEquals("Menu should not have an item displaying No Transport Bookings", 0, menuItems3.Count(i => i.Text == "No Transport Bookings"));
			}
		}

		public void TestRefreshMenuItems_CancelledParentWithExistingTransportBookings()
		{
			// If parent is cancelled and has transport booking(s) then it should have view transport booking (pickup and delivery) and not have create transport booking.
			IDtbBookingParent selectedBookingParent = null;
			var provider = new DtbBookingMenuProvider(() => selectedBookingParent);

			using (var menu = provider.ConstructMenu())
			{
				AssertEquals("Menu should be named Transport Booking", "Transport Booking", menu.Text);

				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu should have one item", 1, menu.MenuItems.Count);
				AssertEquals("Menu should have the first item displaying No Actions Available", "No Actions Available", menu.MenuItems[0].Text);

				var cancelledBookingParent1 = CreateCancelledBookingParent();
				selectedBookingParent = cancelledBookingParent1;
				AssertRefreshMenuItems_CancelledParentWithExistingTransportBookings(cancelledBookingParent1, new[] { DtbBookingDirection.PIC }, menu);

				var cancelledBookingParent2 = CreateCancelledBookingParent();
				selectedBookingParent = cancelledBookingParent2;
				AssertRefreshMenuItems_CancelledParentWithExistingTransportBookings(cancelledBookingParent2, new[] { DtbBookingDirection.DLV }, menu);

				var cancelledBookingParent3 = CreateCancelledBookingParent();
				selectedBookingParent = cancelledBookingParent3;
				AssertRefreshMenuItems_CancelledParentWithExistingTransportBookings(cancelledBookingParent3, new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, menu);
			}
		}

		void AssertRefreshMenuItems_CancelledParentWithExistingTransportBookings(DummyWithDtbBooking cancelledBookingParent, DtbBookingDirection[] supportedDirections, MenuItem menu)
		{
			cancelledBookingParent.SupportedDirections = supportedDirections;

			foreach (var direction in supportedDirections)
			{
				var consol = Helper.CreateConsolidation(cancelledBookingParent);
				consol.KB_JobDirection = direction.ToString();
				consol.KB_IsOverridden = true;
				var container1 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT123");
				var container2 = consol.PackageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT456");

				var twentyGPO = Helper.LoadRefContainer("20GP");
				container1.Container.K0_RC_ContainerType = twentyGPO.PK;
				container2.Container.K0_RC_ContainerType = twentyGPO.PK;

				var singleContainerBooking = Helper.CreateBooking(consol);
				Factory.Save();

				var directionDescription = DtbBookingDirectionDescription.GetDescription(direction);
				var viewBookingText = string.Format("View {0} Transport Booking", directionDescription);

				menu.OnPopup(EventArgs.Empty);
				var menuItems1 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have an item displaying " + viewBookingText, 1, menuItems1.Count(i => i.Text == viewBookingText));
				AssertEquals("Menu should not have any 'Create' items", 0, menuItems1.Count(i => i.Text.Contains("Create", StringComparison.OrdinalIgnoreCase)));
				AssertEquals("Menu should not have an item displaying No Transport Bookings", 0, menuItems1.Count(i => i.Text == "No Transport Bookings"));

				var multiContainerBooking = Helper.CreateBooking(consol, "IFFD", "Test", direction.ToString(), "REF");
				multiContainerBooking.Instructions[0].DivotsWithPackages.AddPackage(container1);
				multiContainerBooking.Instructions[0].DivotsWithPackages.AddPackage(container2);
				Factory.Save();

				menu.OnPopup(EventArgs.Empty);
				var menuItems2 = menu.MenuItems.Cast<MenuItem>();
				AssertEquals("Menu should have an item displaying " + viewBookingText, 1, menuItems2.Count(i => i.Text == viewBookingText));
				AssertEquals("Menu should not have any 'Create' items", 0, menuItems2.Count(i => i.Text.Contains("Create", StringComparison.OrdinalIgnoreCase)));
				AssertEquals("Menu should not have an item displaying No Transport Bookings", 0, menuItems2.Count(i => i.Text == "No Transport Bookings"));
			}
		}

		public void TestAddMenuItems_ParentRequiresMultiContainerBooking_AddMultiContainerMenuItem()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var provider = new DtbBookingMenuProvider(() => dummy);

			using (var menu = provider.ConstructMenu())
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(2, menu.MenuItems.Count);

				var createPickupMultiContainerMenu = menu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Multi-Container Transport Booking");
				AssertNotNull(createPickupMultiContainerMenu);
			}
		}

		public void TestAddMenuItems_ParentNotRequiresMultiContainerBooking_DoNotAddMultiContainerMenuItem()
		{
			var dummy = Factory.New<DummyWithDtbBookingNotRequireMultiContainer>();
			var provider = new DtbBookingMenuProvider(() => dummy);

			using (var menu = provider.ConstructMenu())
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(1, menu.MenuItems.Count);

				var createPickupMultiContainerMenu = menu.MenuItems.Cast<MenuItem>().SingleOrDefault(i => i.Text == "Create Pickup Multi-Container Transport Booking");
				AssertNull(createPickupMultiContainerMenu);
			}
		}

		DummyWithDtbBooking CreateCancelledBookingParent()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.IsCancelled = true;

			return bookingParent;
		}

		DummyWithDtbBooking CreateBookingParent_ParentWithCanCreateTransportBooking_IsFalse()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.CanCreateTransportBooking = false;

			return bookingParent;
		}

		public void TestRefreshMenuItems_ParentWithCanCreateTransportBooking_IsFalse()
		{
			IDtbBookingParent selectedBookingParent = null;
			var provider = new DtbBookingMenuProvider(() => selectedBookingParent);

			using (var menu = provider.ConstructMenu())
			{
				AssertEquals("Menu should be named Transport Booking", "Transport Booking", menu.Text);

				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu should have one item", 1, menu.MenuItems.Count);
				AssertEquals("Menu should have the first item displaying No Actions Available", "No Actions Available", menu.MenuItems[0].Text);

				var bookingCreationDisabledBookingParent = CreateBookingParent_ParentWithCanCreateTransportBooking_IsFalse();
				selectedBookingParent = bookingCreationDisabledBookingParent;

				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu should have one item", 1, menu.MenuItems.Count);
				AssertEquals("Menu should have the first item displaying No Actions Available", "No Actions Available", menu.MenuItems[0].Text);
			}
		}
	}
}
