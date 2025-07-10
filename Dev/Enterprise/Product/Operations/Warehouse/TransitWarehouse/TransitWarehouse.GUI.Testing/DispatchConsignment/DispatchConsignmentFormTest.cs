using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.GUI.Options;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(DispatchConsignmentForm))]
	public class DispatchConsignmentFormTest : ZFormBasherTest
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				AssertEquals("Dispatch Consignment", form.FormCaption);

				((WhsItemDispatchConsignment)form.BusinessEntity).WDC_JobID = "DC001";
				AssertEquals("Dispatch Consignment DC001", form.FormCaption);
			}
		}

		#endregion

		#region TestFormHasDocumentPlugIn

		public void TestFormHasDocumentPlugIn()
		{
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region TestFormHasTransportBookingPlugIn

		public void TestFormHasTransportBookingPlugIn()
		{
			GlbStaff.CurrentUser.GS_LoginName = "Other User";
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestAnyUserCanAccessTransportBookingPlugIn()
		{
			GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}

			GlbStaff.CurrentUser.GS_LoginName = "Other User";
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestTransportBookingPlugInCreate()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var dcn = helper.CreateDispatchConsignment("dcn01", warehouse.PK, jobID: "DC000001");
			using (var form = new DispatchConsignmentForm(dcn))
			{
				form.Show();
				var saveButton = form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				AssertNotNull("Transport Booking menu exists in Actions", transportBookingMenu);

				transportBookingMenu.OnPopup(EventArgs.Empty);
				var createDeliveryTransportBookingMenu = transportBookingMenu.MenuItems.FindByText("Create Delivery Transport Booking");
				AssertNotNull("Create Delivery Transport Booking menu exists in Transport Booking", createDeliveryTransportBookingMenu);

				createDeliveryTransportBookingMenu.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((transportForm) =>
				{
					var formAsDocumentForm = transportForm as TransportBookingDocumentForm;
					if (formAsDocumentForm != null)
					{
						formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
					}

					var manager = new DtbDeliveryManager(Factory, dcn, DtbBookingDirection.PIC, false);
					manager.CreateTransportBooking();
					Factory.Save();

					var booking = Factory.LoadTop1<DtbBooking>(new ZQuery());
					var consolidation = Factory.Load<DtbBookingConsolidation>(booking.KM_KB_Booking);
					AssertEquals("Template is DLTW", "DLTW", booking.KM_KT_NKBookingTemplate);
					AssertEquals("Parent is dcn", dcn.PK, consolidation.KB_ParentID);
					AssertEquals("Freight mode is loose package", "LSE", booking.KM_RatingFreightMode);

					formAsDocumentForm.Dispose();
				});
			}
		}

		#endregion

		#region TestCreateTransportBooking

		public void TestCreateTransportBooking_WhenParentIsShipment()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var dcn = helper.CreateDispatchConsignment("dcn01", warehouse.PK, jobID: "DC000001");
			var shipmentParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			dcn.WDC_ParentID = shipmentParent.PK;
			dcn.WDC_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			using (var form = new DispatchConsignmentForm(dcn))
			{
				form.Show();
				var saveButton = form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var topLevelActionsMenu = form.Menu.MenuItems.FindByText("Actions");
				topLevelActionsMenu.OnPopup(EventArgs.Empty);

				var transportBookingMenu = topLevelActionsMenu.MenuItems.FindByText("Transport Booking");
				AssertNotNull("Transport Booking menu exists in Actions", transportBookingMenu);

				transportBookingMenu.OnPopup(EventArgs.Empty);
				var createDeliveryTransportBookingMenu = transportBookingMenu.MenuItems.FindByText("Create Delivery Transport Booking");
				AssertNotNull("Create Delivery Transport Booking menu exists in Transport Booking", createDeliveryTransportBookingMenu);

				createDeliveryTransportBookingMenu.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((transportForm) =>
				{
					var formAsDocumentForm = transportForm as TransportBookingDocumentForm;
					if (formAsDocumentForm != null)
					{
						formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
					}

					var manager = new DtbDeliveryManager(Factory, dcn, DtbBookingDirection.PIC, false);
					manager.CreateTransportBooking();
					Factory.Save();

					var booking = Factory.LoadTop1<DtbBooking>(new ZQuery());
					var consolidation = Factory.Load<DtbBookingConsolidation>(booking.KM_KB_Booking);
					AssertEquals("Template is DLTW", "DLTW", booking.KM_KT_NKBookingTemplate);
					AssertEquals("Parent is shipment", shipmentParent.PK, consolidation.KB_ParentID);
					AssertEquals("Freight mode is loose package", "LSE", booking.KM_RatingFreightMode);

					var viewDeliveryTransportBookingMenu = transportBookingMenu.MenuItems.FindByText("View/Edit Delivery Transport Booking");
					AssertNotNull("View/Edit Delivery Transport Booking menu exists in Transport Booking", viewDeliveryTransportBookingMenu);

					formAsDocumentForm.Dispose();
				});
			}
		}

		public void TestCreateTransportBooking_WhenParentIsShipment_HandlingUnitAttachedMultipleDCN()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateTRWWarehouse();
			var shipmentPK = Guid.NewGuid();
			var shipmentTableCode = "JS";
			var rcn = helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var location = helper.CreateLocation(warehouse);
			var rtu = helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, location.PK);

			var dcn1 = helper.CreateDispatchConsignment("DC001", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);
			var dcn2 = helper.CreateDispatchConsignment("DC002", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);

			var package1 = helper.CreatePackageState(rcn, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn1);
			var package2 = helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn2);

			var handlingUnit = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu);

			helper.PackPackageIntoHandlingUnit(handlingUnit, package1, ZDateTimeOffset.Now, "ABC", handlingUnit);
			helper.PackPackageIntoHandlingUnit(handlingUnit, package2, ZDateTimeOffset.Now, "ABC", handlingUnit);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, dcn1, DtbBookingDirection.PIC, false);
			manager.DeliverTransportBooking();

			var bookings = Factory.Load<DtbBooking>(new ZQuery());
			AssertEquals("Should have only one booking", 1, bookings.Length);

			var packages = bookings.First().Instructions.First().GetPackages;
			AssertEquals("Should have only one package", 1, packages.Count());

			AssertEquals("The only one package should be PKG1", "PKG1", packages.First().KP_PackageID);
		}

		public void TestCreateTransportBooking_WhenParentIsShipment_MultipleHandlingUnits()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateTRWWarehouse();
			var shipmentPK = Guid.NewGuid();
			var shipmentTableCode = "JS";
			var rcn = helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var location = helper.CreateLocation(warehouse);
			var rtu = helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, location.PK);

			var dcn1 = helper.CreateDispatchConsignment("DC001", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);

			var package1 = helper.CreatePackageState(rcn, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn1);
			var package2 = helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn1);

			var handlingUnit1 = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu);
			var handlingUnit2 = helper.CreateHandlingUnitPackage("HU2", helper.CreatePackageHandlingUnit(), rtu);

			helper.PackPackageIntoHandlingUnit(handlingUnit1, package1, ZDateTimeOffset.Now, "ABC", handlingUnit1);
			helper.PackPackageIntoHandlingUnit(handlingUnit2, package2, ZDateTimeOffset.Now, "ABC", handlingUnit2);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, dcn1, DtbBookingDirection.PIC, false);
			manager.DeliverTransportBooking();

			var bookings = Factory.Load<DtbBooking>(new ZQuery());
			AssertEquals("Should have only one booking", 1, bookings.Length);

			var packages = bookings.First().Instructions.First().GetPackages;
			AssertEquals("Should have two packages", 2, packages.Count());

			AssertContainsExactElementsInAnyOrder("Packages should be HU1 and HU2", new[] { "HU1", "HU2" }, packages.Select(x => x.KP_PackageID));
		}

		public void TestCreateTransportBooking_WhenParentIsShipment_DCNHasTwoPacklinesInHUs()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateTRWWarehouse();
			var shipmentPK = Guid.NewGuid();
			var shipmentTableCode = "JS";
			var rcn = helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var location = helper.CreateLocation(warehouse);
			var rtu = helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, location.PK);

			var dcn1 = helper.CreateDispatchConsignment("DC001", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);
			var dcn2 = helper.CreateDispatchConsignment("DC002", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);

			var packline1 = helper.CreatePackageState(rcn, 10, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Putaway, rtu, dcn1, unitType: PackageStateUnitType.Codes.PackLine);
			var packline2 = helper.CreatePackageState(rcn, 10, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Putaway, rtu, dcn2, unitType: PackageStateUnitType.Codes.PackLine);

			var handlingUnit1 = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu);
			var handlingUnit2 = helper.CreateHandlingUnitPackage("HU2", helper.CreatePackageHandlingUnit(), rtu);

			helper.PackPackageIntoHandlingUnit(handlingUnit1, packline1, ZDateTimeOffset.Now, "ABC", handlingUnit1);
			helper.PackPackageIntoHandlingUnit(handlingUnit2, packline2, ZDateTimeOffset.Now, "ABC", handlingUnit2);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, dcn1, DtbBookingDirection.PIC, false);
			manager.DeliverTransportBooking();

			var manager2 = new DtbDeliveryManager(Factory, dcn2, DtbBookingDirection.PIC, false);
			manager2.DeliverTransportBooking();

			var bookings = Factory.Load<DtbBooking>(new ZQuery());
			AssertEquals("Should have two bookings", 2, bookings.Length);

			var bookingWithHU1 = bookings.Where(b => b.Instructions.SelectMany(i => i.GetPackages).All(p => p.KP_PackageID == "HU1"));
			AssertEquals("Should have only one booking that contain only HU1 package", 1, bookingWithHU1.Count());

			var bookingWitheHU2 = bookings.Where(b => b.Instructions.SelectMany(i => i.GetPackages).Any(p => p.KP_PackageID == "HU2"));
			AssertEquals("Should have only one booking that contain only HU2 package", 1, bookingWitheHU2.Count());
		}

		public void TestCreateTransportBooking_WhenParentIsShipment_DCNHasTwoPacklinesInOVPs()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateTRWWarehouse();
			var shipmentPK = Guid.NewGuid();
			var shipmentTableCode = "JS";
			var rcn = helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var location = helper.CreateLocation(warehouse);
			var rtu = helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, location.PK);

			var dcn1 = helper.CreateDispatchConsignment("DC001", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);
			var dcn2 = helper.CreateDispatchConsignment("DC002", warehouse.PK, parentPK: shipmentPK, parentCode: shipmentTableCode);

			var packline1 = helper.CreatePackageState(rcn, 10, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Putaway, rtu, dcn1, unitType: PackageStateUnitType.Codes.Overpack);
			var packline2 = helper.CreatePackageState(rcn, 10, "PLT", string.Empty, TransitWarehouseStatuses.Codes.Putaway, rtu, dcn2, unitType: PackageStateUnitType.Codes.Overpack);

			var ovp1 = helper.CreateOverpackPackage("OVP1", helper.CreatePackageHandlingUnit(), rtu, rcn: rcn, dcn: dcn1);
			var ovp2 = helper.CreateOverpackPackage("OVP2", helper.CreatePackageHandlingUnit(), rtu, rcn: rcn, dcn: dcn2);

			helper.PackPackageIntoHandlingUnit(ovp1, packline1, ZDateTimeOffset.Now, "ABC", ovp1);
			helper.PackPackageIntoHandlingUnit(ovp2, packline2, ZDateTimeOffset.Now, "ABC", ovp2);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = false;

			var manager = new DtbDeliveryManager(Factory, dcn1, DtbBookingDirection.PIC, false);
			manager.DeliverTransportBooking();

			var manager2 = new DtbDeliveryManager(Factory, dcn2, DtbBookingDirection.PIC, false);
			manager2.DeliverTransportBooking();

			var bookings = Factory.Load<DtbBooking>(new ZQuery());
			AssertEquals("Should have 2 bookings", 2, bookings.Length);

			var bookingWithOVP1 = bookings.Where(b => b.Instructions.SelectMany(i => i.GetPackages).All(p => p.KP_PackageID == "OVP1"));
			AssertEquals("Should only have 1 booking that contain only OVP1 package", 1, bookingWithOVP1.Count());

			var bookingWitheOVP2 = bookings.Where(b => b.Instructions.SelectMany(i => i.GetPackages).Any(p => p.KP_PackageID == "OVP2"));
			AssertEquals("Should only have 1 booking that contain only OVP2 package", 1, bookingWitheOVP2.Count());
		}

		#endregion

		#region TestMessageMenu

		public void TestMessageMenu()
		{
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				var messageMenu = form.Menu.MenuItems.FindByText("Message");
				AssertNull("Form should not have message menu", messageMenu);
			}
		}

		public void TestMessageMenu_RegistryIsEnabled()
		{
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = (DispatchConsignmentForm)GetFormToBash())
			{
				var messageMenu = form.Menu.MenuItems.FindByText("Message");
				messageMenu.OnPopup(EventArgs.Empty);
				AssertNotNull("Form should have message menu", messageMenu);
				AssertEquals("Message menu should have 4 menu items", messageMenu.MenuItems.Count, 4);

				var cinMenuItem = messageMenu.MenuItems.FindByText("CIN 750 Deconsolidation Notification");
				AssertNotNull("Message menu should contain CIN 750 Notification menu item", cinMenuItem);
				Assert("CIN 750 Deconsolidation Notification menu item should be visible", cinMenuItem.Visible);

				cinMenuItem = messageMenu.MenuItems.FindByText("CIN 750 Consolidation Notification");
				AssertNotNull("Message menu should contain CIN 750 Notification menu item", cinMenuItem);
				Assert("CIN 750 Consolidation Notification menu item should be visible", cinMenuItem.Visible);

				cinMenuItem = messageMenu.MenuItems.FindByText("CIN 750 Out Notification");
				AssertNotNull("Message menu should contain CIN 750 Notification menu item", cinMenuItem);
				Assert("CIN 750 Out Notification menu item should be visible", cinMenuItem.Visible);

				var noMessagesAvailableMenuItem = messageMenu.MenuItems.FindByText("No Messages Available");
				AssertNotNull("Message menu should contain No Messages Available menu item", noMessagesAvailableMenuItem);
				Assert("No Messages Available menu item should be invisible", !noMessagesAvailableMenuItem.Visible);
			}
		}

		#endregion

		#region Open in Browser

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (DispatchConsignmentForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);
				AssertEquals(@"This Dispatch Consignment cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var consignment = Factory.New<WhsItemDispatchConsignment>();

			using (var form = new DispatchConsignmentForm(consignment))
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);

				AssertEquals("Should not show error if registry item exists.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(consignment, glowPortalsUri: "address");
		}

		public static void AssertOpenedOnTheWeb(WhsItemDispatchConsignment consignment, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/DispatchConsignment", uri.AbsolutePath);
			AssertEquals(consignment.PK.ToString(), entityPK);
		}

		#endregion

		#region Implementation

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		protected override Form GetFormToBashCore()
		{
			var form = new DispatchConsignmentForm(Factory.New<WhsItemDispatchConsignment>());
			form.ControllerID = ControllerIDs.WhsTransitDispatchConsignment;
			return form;
		}

		#endregion
	}
}
