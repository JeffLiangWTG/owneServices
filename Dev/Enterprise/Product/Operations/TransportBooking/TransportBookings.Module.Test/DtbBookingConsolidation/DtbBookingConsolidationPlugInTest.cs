using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingConsolidationPlugInTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetNewTopLevelMenu()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			using (var form = new DtbBookingPlugInFormTest(dummy))
			{
				form.Show();

				var topLevelActionsMenu = form.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				AssertNotNull("topLevelMenuItem should have the correct text", transportBookingMenu);

				transportBookingMenu.OnPopup(EventArgs.Empty);
				AssertEquals(2, transportBookingMenu.MenuItems.Count);
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Transport Booking");
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Multi-Container Transport Booking");

				dummy.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.DLV };
				topLevelActionsMenu.OnPopup(EventArgs.Empty);
				transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);
				AssertEquals(2, transportBookingMenu.MenuItems.Count);
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Delivery Transport Booking");
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Delivery Multi-Container Transport Booking");
			}
		}

		public void TestGetNewTopLevelMenu_ReadOnlyMode()
		{
			using (var form = new DtbBookingPlugInFormTest(Factory.New<DummyWithDtbBooking>()))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
				form.Show();

				var topLevelActionsMenu = form.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				AssertNotNull(transportBookingMenu);
				AssertEquals("Transport Booking menu item should be enabled in View mode", true, transportBookingMenu.Enabled);
			}
		}

		public void TestGetNewTopLevelMenu_SavedBooking()
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
			using (var parentWithPluginForm = new DtbBookingPlugInFormTest(dummy))
			{
				parentWithPluginForm.Show();

				var topLevelActionsMenu = parentWithPluginForm.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);
				AssertNotNull("topLevelMenuItem should have the correct text", transportBookingMenu);
				AssertEquals(3, transportBookingMenu.MenuItems.Count);
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "View/Edit Pickup Transport Booking");
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create/Override Pickup Transport Booking");
				transportBookingMenu.MenuItems.Cast<MenuItem>().Single(i => i.Text == "Create Pickup Multi-Container Transport Booking");
			}
		}

		public void TestViewTransportBooking()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationA = Helper.CreateConsolidation(dummy);
			consolidationA.KB_JobDirection = "PIC";
			var bookingA = consolidationA.Bookings.AddNew();

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
			using (var bookingAFromControllerForm = (ZForm)controller.ShowEditForm(consolidationA))
			using (var parentWithPluginForm = new DtbBookingPlugInFormTest(dummy))
			{
				parentWithPluginForm.Show();

				var topLevelActionsMenu = parentWithPluginForm.TopLevelMenu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				transportBookingMenu.OnPopup(EventArgs.Empty);
				var pickupViewMenu = transportBookingMenu.MenuItems[0];
				pickupViewMenu.PerformClick();

				var dtbPlugin = (DtbBookingConsolidationPlugIn)parentWithPluginForm.PlugIns.GetPlugIn(ControllerIDs.DtbBookingConsolidation);
				var bookingFromParentForm = dtbPlugin.LastMenuProviderForTest.LastDeliveryManagerForTest.LastControllerForTest.LastShownForm;
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingMultiForm), bookingFromParentForm.GetType());

				bookingFromParentForm.Dispose();
			}
		}

		public void TestTransportBookingParent()
		{
			var dummy = Factory.New<MasterDummyBusinessObject>();

			using (DtbBookingPlugInFormForDependentForTest form = new DtbBookingPlugInFormForDependentForTest(dummy))
			{
				form.Show();

				var plugin = ((DtbBookingConsolidationPlugIn)form.ExposedPlugIns[0]);
				AssertNull(plugin.ExposedTransportBookingParent);

				var parent1 = dummy.BookingParents.AddNew();
				AssertEquals(parent1, plugin.ExposedTransportBookingParent);

				var parent2 = dummy.BookingParents.AddNew();
				AssertEquals(parent1, plugin.ExposedTransportBookingParent);

				form.Grid.CurrentRowIndex = 1;
				AssertEquals(parent2, plugin.ExposedTransportBookingParent);
			}
		}

		internal class DtbBookingPlugInFormTest : ZForm
		{
			internal DtbBookingPlugInFormTest(DummyWithDtbBooking dummyParent)
				: base(dummyParent)
			{
				PlugIns.Add(ControllerIDs.DtbBookingConsolidation);
			}

			public MainMenu TopLevelMenu
			{
				get { return MainMenu; }
			}
		}

		internal class DtbBookingPlugInForShipmentFormTest : ZForm
		{
			internal DtbBookingPlugInForShipmentFormTest(Forwarding.IForwardingShipment shipmentParent)
				: base(shipmentParent)
			{
				PlugIns.Add(ControllerIDs.DtbBookingConsolidation);
			}

			public MainMenu TopLevelMenu
			{
				get { return MainMenu; }
			}
		}

		class DtbBookingPlugInFormForDependentForTest : ZForm
		{
			public DtbBookingPlugInFormForDependentForTest(MasterDummyBusinessObject bO)
				: base(bO)
			{
				Grid = new ZGrid();
				Grid.BindTo = "BookingParents";

				var column = new ZTextBoxColumnStyleInfo();
				column.ColumnName = "Z0_Description";
				Grid.ColumnStyles.Add(column);

				Controls.Add(Grid);
				PlugIns.AddCurrentDependentPlugIn(ControllerIDs.DtbBookingConsolidation, Grid);
			}

			public readonly ZGrid Grid;

			public MainMenu TopLevelMenu
			{
				get { return MainMenu; }
			}

			public ZPlugIn[] ExposedPlugIns
			{
				get { return PlugIns.Instances; }
			}
		}

		class MasterDummyBusinessObject : DummyBaseBusinessObject
		{
			public MasterDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row) { }

			public BookingParentsCollection BookingParents
			{
				get { return bookingParents ?? (bookingParents = new BookingParentsCollection(new BusinessObjectFactory())); }
			}

			BookingParentsCollection bookingParents;

			public class BookingParentsCollection : ActiveBusinessObjectCollection<DummyWithDtbBooking>
			{
				public BookingParentsCollection(BusinessObjectFactory factory) : base(factory) { }

				protected override bool AllowNew
				{
					get { return false; }
				}
			}
		}
	}
}
