using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(OrdersForm))]
	public class OrdersFormTest : ZFormBasherTest
	{
		#region Notification

		public void TestNotify()
		{
			Order order = Factory.New<Order>();
			using (OrdersForm form = new OrdersForm(order))
			{
				form.Notify(new InfoNotification("hello"));
				AssertEquals("hello", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueryUser()
		{
			var order = Factory.New<Order>();
			using (var form = new OrdersForm(order))
			{
				form.QueryUser(new QueryUserMsgBoxEventArgs("hello zubs", false));
				AssertEquals("hello zubs", UnitTestUserNotification.Instance.LastMessage.Text);

				var args = new QueryUserFindboxEventArgs(ModuleIDs.Customs.JobDeclaration, order.JD_Declaration_List);
				form.QueryUser(args);
				AssertEquals(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)), args.List.TypeOfElements);
				AssertEquals(typeof(PreAdviceDeclarationsListForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				args = new QueryUserFindboxEventArgs(ModuleIDs.CommercialInvoice, order.InvoiceList);
				form.QueryUser(args);
				AssertEquals(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)), args.List.TypeOfElements);
				AssertEquals(typeof(OrdersInvoicesListForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#endregion

		#region Actions Menu

		public void TestActionsMenu()
		{
			Order order = Factory.New<Order>();
			using (OrdersFormForTesting form = new OrdersFormForTesting(order))
			{
				form.Show();
				form.ActionsMenuItem.OnPopup(EventArgs.Empty);

				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Create Declaration"));
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration"));
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Add Commercial Invoice to Declaration"));
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Add Order Line to Commercial Invoice"));
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Create Quick Booking"));
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Create Shipment"));
			}
		}

		public void TestCreateWarehouseReceiveActionMenuItem()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.OH_IsWarehouseClient = true;
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = new OrdersFormForTesting(order))
			{
				var warehouseReceiveMenuItem = form.ActionsMenuItem.MenuItems.FindByText("Create Warehouse Receive");
				AssertNull(warehouseReceiveMenuItem);

				warehouseReceiveMenuItem = form.ActionsMenuItem.MenuItems.FindByText("View Warehouse Receive");
				AssertNotNull(warehouseReceiveMenuItem);
				AssertEquals(false, warehouseReceiveMenuItem.Visible);

				form.ActionsMenuItem.OnPopup(EventArgs.Empty);
				var createWarehouseReceiveMenuItem = form.ActionsMenuItem.MenuItems.FindByText("Create Warehouse Receive");
				AssertNotNull(createWarehouseReceiveMenuItem);
				AssertEquals(true, createWarehouseReceiveMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createWarehouseReceiveMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Warehouse Receive", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("You must save this Order before creating a Warehouse Receive.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createWarehouseReceiveMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Warehouse Receive", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Failed to create Warehouse Receive:\r\nNo Warehouse is specified.", UnitTestUserNotification.Instance.LastMessage.Text);

				var warehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
				warehouseOrg.OH_Code = "SAM";
				var communicationMode = warehouseOrg.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "Universal";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "ORD"; //Orders
				order.WarehouseDocAddress.E2_OA_Address = warehouseOrg.MainAddress.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createWarehouseReceiveMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Create Warehouse Receive", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Universal Shipment queued for sending to Organization [SAM].", UnitTestUserNotification.Instance.LastMessage.Text);

				order.WarehouseDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				var warehouse = helper.CreateWarehouse("WHS", "A");
				warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = order.WarehouseAddress.PK;
				Factory.Save();
				createWarehouseReceiveMenuItem.PerformClick();
				using (var receiveForm = form.LastReceiveFormShownForTesting)
				{
					form.LastReceiveFormShownForTesting = null;
					AssertEquals(ControllerIDs.WhsReceive, receiveForm.ControllerID);
				}

				form.ActionsMenuItem.OnPopup(EventArgs.Empty);
				var viewWarehouseReceiveMenuItem = form.ActionsMenuItem.MenuItems.FindByText("View Warehouse Receive");
				AssertNotNull(viewWarehouseReceiveMenuItem);
				viewWarehouseReceiveMenuItem.PerformClick();
				using (var receiveForm = form.LastReceiveFormShownForTesting)
				{
					form.LastReceiveFormShownForTesting = null;
					AssertEquals(ControllerIDs.WhsReceive, receiveForm.ControllerID);
				}
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new OrdersForm(Factory.New<Order>()));
		}

		public void TestCreateShipmentActionMenuItem()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();
			using (var form = new OrdersFormForTesting(order))
			{
				var createShipmentMenuItem = form.ActionsMenuItem.MenuItems.FindByText("Create Shipment");
				AssertNotNull(createShipmentMenuItem);
				AssertEquals(true, createShipmentMenuItem.Visible);

				var orderLine1 = order.OrderLines.AddNew();
				orderLine1.JO_Quantity = 2;
				orderLine1.JO_QtyReceived = 1;
				orderLine1.JO_Description = "Fus Ro Dah";

				var orderLine2 = order.OrderLines.AddNew();
				orderLine2.JO_Quantity = 3;
				orderLine2.JO_QtyReceived = 2;
				orderLine2.JO_Description = "Dovahkiin";

				order.Factory.Save();

				createShipmentMenuItem.PerformClick();

				var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
				AssertNotNull(shipmentForm);
				try
				{
					var shipment = (ForwardingShipment)shipmentForm.DataSource;

					CombineAssertions(() =>
					{
						foreach (var orderLine in order.OrderLines)
						{
							Assert(string.Format("All orderlines should be copied as packlines. Missing packline with description: {0}", orderLine.JO_Description), shipment.OuterPackLines.Any(x => ((ForwardingPackLine)x).JL_Description.Equals(orderLine.JO_Description)));
						}
					});
				}
				finally
				{
					shipmentForm.Close();
				}
			}
		}

		Order GenerateOrderWithShipmentAttached(string shipmentNo)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentNo;
			order.JD_JS = shipment.PK;

			Factory.Save();

			return order;
		}

		(Order, QuotedBooking) GenerateOrderWithBookingAttached(string bookingNo)
		{
			var order = Factory.NewWithValidTestData<Order>();

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var bookingWithQuote = quotedBookingBuilder.CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote.UniqueConsignRef = bookingNo;
			order.JD_JS = bookingWithQuote.ForwardingShipment.PK;

			Factory.Save();

			return (order, (QuotedBooking)bookingWithQuote);
		}

		OrgHeader SetupOrgProxy()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DXZENTBNE";
			org.OH_FullName = "DXZ Enterprises";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_IsForwarder = true;
			org.MainAddress.OA_Address1 = "1 Street St";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "DXZ";
			newCompany.GC_Name = "DXZ Enterprises";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = org.PK;

			return org;
		}

		[RequiresSTA]
		public void TestCreateShipmentActionMenuItem_BookingAttached_HaveCorrectAlert()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_HaveCorrectAlert_Core("Create Shipment");
		}

		public void TestCreateStandaloneShipmentActionMenuItem_BookingAttached_HaveCorrectAlert()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_HaveCorrectAlert_Core("Create Consol/Shipment/Declaration");
		}

		void TestCreateShipmentActionMenuItem_BookingAttached_HaveCorrectAlert_Core(string actionText)
		{
			var (order, _) = GenerateOrderWithBookingAttached("b0001");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", true, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText(actionText).PerformClick);
				AssertEquals("Order is already linked to booking b0001. Convert booking to shipment?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNull(shipmentForm);
		}

		public void TestCreateShipmentActionMenuItem_BookingAttached_ConverterHasError()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_ConverterHasError_Core("Create Shipment");
		}

		public void TestCreateStandaloneShipmentActionMenuItem_BookingAttached_ConverterHasError()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_ConverterHasError_Core("Create Consol/Shipment/Declaration");
		}

		void TestCreateShipmentActionMenuItem_BookingAttached_ConverterHasError_Core(string actionText)
		{
			var (order, booking) = GenerateOrderWithBookingAttached("b001");

			using (var form = new OrdersFormForTesting(order))
			{
				AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
				AssertEquals("Order Is Booking Attached", true, order.IsBookingAttached);

				var createShipmentMenuItem = form.ActionsMenuItem.MenuItems.FindByText(actionText);

				booking.Booking.JS_IsDirectBooking = false;
				booking.Booking.JS_ScreeningStatus = "CLR";
				booking.ShipmentStatus = null;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createShipmentMenuItem.PerformClick();
				AssertEquals("A message should have popped up", "The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", UnitTestUserNotification.Instance.LastMessage.Text);

				var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
				AssertNull("Shipment Form should not show", shipmentForm);
			}
		}

		public void TestCreateShipmentActionMenuItem_BookingAttached()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_Core("Create Shipment");
		}

		public void TestCreateStandaloneShipmentActionMenuItem_BookingAttached()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_Core("Create Consol/Shipment/Declaration");
		}

		void TestCreateShipmentActionMenuItem_BookingAttached_Core(string actionText)
		{
			var (order, booking) = GenerateOrderWithBookingAttached("b001");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", true, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				var createShipmentMenuItem = form.ActionsMenuItem.MenuItems.FindByText(actionText);

				AssertNotNull(createShipmentMenuItem);
				AssertEquals(true, createShipmentMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(createShipmentMenuItem.PerformClick);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNotNull(shipmentForm);

			shipmentForm.BusinessEntity.Factory.Save();
			shipmentForm.Close();

			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", false, order.IsBookingAttached);

			var shipment = Factory.Load<ForwardingShipment>(order.Shipment.PK);
			Assert("The shipment was a booking", shipment.JS_IsBooking);
			Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);
		}

		public void TestCreateShipmentActionMenuItem_BookingAttached_SecurityCheckpoint()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_SecurityCheckpoint_Core("Create Shipment");
		}

		public void TestCreateStandaloneShipmentActionMenuItem_BookingAttached_SecurityCheckpoint()
		{
			TestCreateShipmentActionMenuItem_BookingAttached_Core("Create Consol/Shipment/Declaration");
		}

		void TestCreateShipmentActionMenuItem_BookingAttached_SecurityCheckpoint_Core(string actionText)
		{
			var org = SetupOrgProxy();

			var (order, booking) = GenerateOrderWithBookingAttached("b001");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", true, order.IsBookingAttached);

			booking.Booking.JS_ScreeningStatus = "CLR";
			booking.Booking.JS_OA_ExportReceivingDepot = org.MainAddress.PK;

			Factory.Save();

			using (var form = new OrdersFormForTesting(order))
			{
				var createShipmentMenuItem = form.ActionsMenuItem.MenuItems.FindByText(actionText);

				AssertNotNull(createShipmentMenuItem);
				AssertEquals(true, createShipmentMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(createShipmentMenuItem.PerformClick);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNotNull(shipmentForm);

			shipmentForm.BusinessEntity.Factory.Save();
			shipmentForm.Close();

			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", false, order.IsBookingAttached);

			var shipment = Factory.Load<ForwardingShipment>(order.Shipment.PK);
			Assert("The shipment was a booking", shipment.JS_IsBooking);
			Assert("The booking has been converted to a shipment", shipment.JS_IsForwardRegistered);
			Assert("JS_IsCFSRegistered should be true", shipment.JS_IsCFSRegistered);

			var billingPlugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			AssertCFSJobInvoicingSecurityCheckpoint(billingPlugIn);
		}

		public void TestCreateShipmentActionMenuItem_ShipmentAttached()
		{
			var order = GenerateOrderWithShipmentAttached("S0001");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", false, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Shipment").PerformClick);
				AssertEquals("Order is already linked to shipment S0001. Detach this shipment before creating a new shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNull(shipmentForm);
		}

		public void TestCreateStandaloneShipmentActionMenuItem_ShipmentAttached()
		{
			var order = GenerateOrderWithShipmentAttached("S0002");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", false, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration").PerformClick);
				AssertEquals("Order is already linked to shipment S0002. Detach this shipment before creating a new shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNull(shipmentForm);
		}

		public void TestCreateQuickBookingActionMenuItem_BookingAttached()
		{
			var (order, _) = GenerateOrderWithBookingAttached("b0001");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", true, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Quick Booking").PerformClick);
				AssertEquals("Order is already linked to booking b0001. Detach this booking before creating a new booking.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNull(shipmentForm);
		}

		public void TestCreateQuickBookingActionMenuItem_ShipmentAttached()
		{
			var order = GenerateOrderWithShipmentAttached("S0002");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Not Booking Attached", false, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Quick Booking").PerformClick);
				AssertEquals("Order is already linked to shipment S0002. Detach this shipment before creating a new booking.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNull(shipmentForm);
		}

		public void TestCreateQuickBookingActionMenuItem_BookingAndShipmentAttached()
		{
			var (order, _) = GenerateOrderWithBookingAttached("b0001");
			AssertEquals("Order Is Shipment Attached", true, order.IsShipmentAttached);
			AssertEquals("Order Is Booking Attached", true, order.IsBookingAttached);

			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Quick Booking").PerformClick);
				AssertEquals("Order is already linked to booking b0001. Detach this booking before creating a new booking.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipmentForm = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault();
			AssertNull(shipmentForm);
		}

		void AssertCFSJobInvoicingSecurityCheckpoint(ZPlugIn billingPlugIn)
		{
			AssertNotNull(billingPlugIn);

			var securityCheckpoint = billingPlugIn.SecurityCheckpoint;
			AssertNotNull(securityCheckpoint);
			AssertEquals("JobInvoicingSecurityCheckpoint should be updated", "MaintainShipmentJobInvoicing", securityCheckpoint.Code);

			var plugInType = billingPlugIn.GetType().BaseType?.BaseType;
			AssertNotNull(plugInType);

			var fieldInfo = plugInType.GetField("SecurityCheckpointEdit", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(fieldInfo);

			var securityCheckpointEdit = fieldInfo.GetValue(billingPlugIn) as SecurityCheckpoint;
			AssertNotNull(securityCheckpointEdit);
			AssertEquals("JobInvoicingSecurityCheckpoint should be None", "None", securityCheckpointEdit.Code);
		}

		#endregion

		#region OnDetachingPreAdviceAskToDetachShipmentOrDeclaration

		public void TestOnDetachingPreAdviceAskToDetachShipmentAndDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var preplanning = Factory.New<JobShipmentPreplanning>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B0000001";
			shipment.JS_UniqueConsignRef = "S0000001";

			var order = Factory.New<Order>();
			order.JD_EF_ShipmentPrePlanning = preplanning.PK;
			order.JD_JS = shipment.PK;

			using (var form = new OrdersForm(order))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;

				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			order = Factory.New<Order>();
			order.JD_EF_ShipmentPrePlanning = preplanning.PK;
			order.JD_JE = declaration.PK;

			using (var form = new OrdersForm(order))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("The order is also attached to Declaration B0000001. Do you want to detach the declaration as well?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region IdentifierForPersistingForm

		public void TestIdentifierForPersistingForm()
		{
			Order order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			using (OrdersForm form = new OrdersForm(orderLine))
			{
				AssertEquals(orderLine.PK, form.IdentifierForPersistingForm);
			}
		}

		#endregion

		public void TestUnsavedOrder_CreateStandaloneShipment()
		{
			var order = Factory.New<Order>();
			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Shipment").PerformClick);
				AssertEquals("Please save the order before creating a Shipment", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestUnsavedOrder_CreateQuickBooking()
		{
			var order = Factory.New<Order>();
			using (var form = new OrdersFormForTesting(order))
			{
				AssertNoExceptionThrown(form.ActionsMenuItem.MenuItems.FindByText("Create Quick Booking").PerformClick);
				AssertEquals("Please save the order before creating a Quick Booking", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanSplitOrder()
		{
			var factory = new BusinessObjectFactory();
			var newOrder = factory.New<Order>();
			using (var form = new OrdersForm(newOrder))
			{
				newOrder.JD_OrderNumber = "order1";
				var org = factory.LoadTop1<OrgHeader>(new ZQuery());
				newOrder.BuyerPK = org.PK;
				newOrder.SupplierPK = org.PK;
				Assert("Cannot split an unsaved order", !string.IsNullOrEmpty(form.CanSplitOrder()));

				factory.Save();
				var line = newOrder.OrderLines.AddNew();
				line.JO_LineNo = 2;
				Assert("Cannot split an order that has changes", !string.IsNullOrEmpty(form.CanSplitOrder()));

				factory.Save();
				Assert("Can split an order that doesn't have incomplete order lines", string.IsNullOrEmpty(form.CanSplitOrder()));
				line.JO_Quantity = 5;
				line.JO_QtyReceived = 2;

				factory.Save();
				Assert("Can split an order with incomplete order lines", string.IsNullOrEmpty(form.CanSplitOrder()));

				line.JO_QtyReceived = 5;
				factory.Save();
				Assert("Can split an order with complete order lines", string.IsNullOrEmpty(form.CanSplitOrder()));

				line.Delete();
				factory.Save();
				Assert("Can split an order without lines", string.IsNullOrEmpty(form.CanSplitOrder()));
			}
		}

		public void TestCanSplitOrder_OrderSplitNumberMaxedOut()
		{
			var order = Factory.New<Order>();
			using (var form = new OrdersForm(order))
			{
				order.JD_OrderNumber = "order";

				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				order.BuyerPK = org.PK;
				order.SupplierPK = org.PK;
				order.JD_OrderNumberSplit = 100;

				Factory.Save();

				AssertEquals("Split is valid. 100 is less than Byte.MaxValue (255)", string.Empty, form.CanSplitOrder());

				order.JD_OrderNumberSplit = byte.MaxValue;

				Factory.Save();

				AssertEquals("Split is invalid when JD_OrderNumberSplit is Byte.MaxValue (255)", "The order has been split the maximum number of times. Please select a different action.", form.CanSplitOrder());
			}
		}

		[RequiresSTA]
		public void TestShowOrderSplitMessageBox()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 2;
			orderLine.JO_QtyReceived = 1;
			Factory.Save();

			using (var form = new OrdersFormForTesting(order))
			{
				form.ActionsMenuItem.MenuItems.FindByText("Create Shipment Pre Advice").PerformClick();

				AssertEquals(typeof(OrderSplitMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			using (var form = new OrdersFormForTesting(order))
			{
				form.ActionsMenuItem.MenuItems.FindByText("Create Declaration").PerformClick();

				AssertEquals(typeof(OrderSplitMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			using (var form = new OrdersFormForTesting(order))
			{
				form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration").PerformClick();

				AssertEquals(typeof(OrderSplitMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			using (var form = new OrdersFormForTesting(order))
			{
				form.ActionsMenuItem.MenuItems.FindByText("Add Commercial Invoice to Declaration").PerformClick();

				AssertEquals(typeof(OrderSplitMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSaveInternal()
		{
			var order = Factory.NewWithValidTestData<Order>();
			using (var form = new OrdersForm(order))
			{
				form.Show();
				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_Quantity = 2;
				orderLine.JO_QtyReceived = 1;
				form.FireSaveButton();

				AssertEquals(typeof(OrderSplitMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestDoNotShowMessageFormDuringInTransaction()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B1000001";
			shipment.JS_UniqueConsignRef = "S10000001";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_EF_ShipmentPrePlanning = preplanning.PK;
			order.JD_JE = declaration.PK;

			using (var form = new OrdersForm(order))
			{
				form.Show();

				try
				{
					Db.Connection.BeginTransaction();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					order.JD_EF_ShipmentPrePlanning = ZGuid.Empty;

					AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestEditFormBashing()
		{
			using (Form testForm = GetEditFormToBash())
			{
				testForm.Show();
				ExposeAllTabPages(testForm);

				Application.DoEvents(); // required for binding to start

				ThrowFailureException();
			}
		}

		public void TestLCTabShowsForAllCountries()
		{
			AssertLCTabOnOrder(Core.Constants.CountryCodes.Australia);
			AssertLCTabOnOrder(Core.Constants.CountryCodes.Sudan);
			AssertLCTabOnOrder(Core.Constants.CountryCodes.NewZealand);
			AssertLCTabOnOrder(Core.Constants.CountryCodes.KoreaNorth);
		}

		void AssertLCTabOnOrder(string countryCode)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			Order bO = Factory.New<Order>();

			using (OrdersForm testOrderForm = new OrdersForm(bO))
			{
				testOrderForm.Show();

				ZPlugIn plugin = testOrderForm.PlugIns.GetPlugIn(ControllerIDs.LandedCosting);

				Assert(!plugin.Enabled);

				bO.JD_RL_NKPortOfDischarge = Factory.LoadTop1<RefUNLOCO>(new ZQuery(Enterprise.ZArchitecture.Schema.RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).RL_Code;
				Assert(plugin.Enabled);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestAttachingDeclarationDoesntCauseStackOverFlow()
		{
			Order bO = Factory.New<Order>();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			using (OrdersForm testOrderForm = new OrdersForm(bO))
			{
				testOrderForm.Show();
				bO.JD_JE = declaration.PK;
			}
		}

		[RequiresSTA]
		public void TestErrorMessageShownWhenOrderNumberChangeAttemptedWhenOrderSplit()
		{
			Order order = Factory.New<Order>();
			using (OrdersForm form = new OrdersForm(order))
			{
				form.Show();
				Application.DoEvents();

				order.JD_OrderNumber = "SPLATY";
				AssertEquals("No error shown if order is not split", null, UnitTestUserNotification.Instance.LastMessage.Text);

				order.SplitOrder(CreateOrderType.Split);
				order.JD_OrderNumber = "meh";
				AssertEquals("Error message should be shown", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error message should be shown", "You cannot change the order number while the order is split", UnitTestUserNotification.Instance.LastMessage.Text);

				ZTextBox orderNumberTextBox = FindOrderNumberTextBox(form);
				AssertEquals("OrderNumber on the text box should still be correct", order.JD_OrderNumber, orderNumberTextBox.Text);
			}
		}

		class OrdersFormForTesting : OrdersForm
		{
			public OrdersFormForTesting(Order order)
				: base(order)
			{
			}
			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}

			public ZTabPage PlanningTab => (ZTabPage)Controls.Find("PlanningTab", true).First();

			protected override XmlDataTransferExporter GetNewXmlDataTransferExportor(IValueObjectDataAdapter adapter, bool checkForLicence)
			{
				return new XmlDataTransferExporter(adapter, false);
			}
		}

		[TestDate(2007, 6, 23, 12, 0, 0)]
		public void TestFileExportedIntoNominatedDiry()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "S001000";
			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Factory.Save();

			SystemDataRegistry.Instance.OrderExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			string expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, order.JD_OrderNumber + "_20070623120000.xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				using (OrdersFormForTesting form = new OrdersFormForTesting(order))
				{
					MenuItem item = form.ActionsMenuItem.MenuItems.FindByText("Export to XML (Verbose)");
					item.PerformClick();
					Assert("File should have been created", File.Exists(expectedFileName));
				}
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		ZTextBox FindOrderNumberTextBox(Control currentControl)
		{
			ZTextBox result = null;
			foreach (Control control in currentControl.Controls)
			{
				if (control is ZTextBox && ((ZTextBox)control).BindTo == Order.Schema.JD_OrderNumber)
				{
					result = (ZTextBox)control;
					break;
				}
				else
				{
					result = FindOrderNumberTextBox(control);
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		#region CreateShipments ManuallyEnterShipmentNumber

		public void TestCreateShipments_ManuallyEnterShipmentNumber()
		{
			Env.Registry.AllowManualShipmentEntry = true;

			var order = Factory.NewWithValidTestData<Order>();
			SetupOrder(order, "ORD1111", "BUYERCODE1");

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_ActualWeight = 1;
			orderLine.JO_UnitOfWeight = Constants.Weight.Kilograms;
			orderLine.JO_ActualVolume = 2;
			orderLine.JO_UnitOfVolume = Constants.Volume.CubicMetres;
			orderLine.JO_OuterPacks = 3;
			order.OrderLines.Add(orderLine);

			Factory.Save();

			using (var form = new OrdersFormForTesting(order))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration").PerformClick();
				AssertEquals("User enters the shipment number manually", "Heya0", order.Shipment.JS_UniqueConsignRef);

				AssertEquals(1, order.Shipment.OuterPackLines.Count);
				AssertEquals(1m, order.Shipment.OuterPackLines[0].JL_ActualWeight);
				AssertEquals(2m, order.Shipment.OuterPackLines[0].JL_ActualVolume);
				AssertEquals(3, order.Shipment.OuterPackLines[0].JL_PackageCount);
			}

			Env.Registry.AllowManualShipmentEntry = false;
			order = Factory.NewWithValidTestData<Order>();
			SetupOrder(order, "ORD2222", "BUYERCODE2");
			Factory.Save();

			using (var form = new OrdersFormForTesting(order))
			{
				form.Show();
				form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration").PerformClick();

				AssertEquals("Shipment number automatically generated", "S00001000", order.Shipment.JS_UniqueConsignRef);
			}
		}

		public void TestCreateShipments_UpdatePlanningTabText()
		{
			Env.Registry.AllowManualShipmentEntry = true;

			var order = Factory.NewWithValidTestData<Order>();
			SetupOrder(order, "ORD1111", "BUYERCODE1");

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_ActualWeight = 1;
			orderLine.JO_UnitOfWeight = Constants.Weight.Kilograms;
			orderLine.JO_ActualVolume = 2;
			orderLine.JO_UnitOfVolume = Constants.Volume.CubicMetres;
			orderLine.JO_OuterPacks = 3;
			order.OrderLines.Add(orderLine);

			Factory.Save();

			using (var form = new OrdersFormForTesting(order))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var planningTab = form.PlanningTab;
				AssertNotNull(planningTab);
				AssertEquals("Planning", planningTab.Text);

				form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration").PerformClick();

				AssertEquals("User enters the shipment number manually", "Heya0", order.Shipment.JS_UniqueConsignRef);

				AssertEquals("Planning - Refer to Shipment Details", planningTab.Text);
			}
		}

		void SetupOrder(Order order, ZString orderNumber, ZString buyerCode)
		{
			order.JD_TransportMode = "AIR";
			order.JD_ContainerMode = "LSE";
			order.JD_OrderNumber = orderNumber;
			order.JD_RL_NKPortOfLoading = "AUBNE";
			order.JD_RL_NKPortOfDischarge = "NZAKL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = buyerCode;
			buyer.OH_IsConsignee = ZBool.True;
			buyer.OH_IsForwarder = ZBool.True;

			var address = buyer.Addresses.AddNewMainAddress();
			address.OA_Code = "Address1 Code";
			address.OA_Address1 = "address1";

			order.BuyerPK = buyer.PK;
		}

		#endregion

		public void TestCreateShipments_ConsolAndShipmentsCheck()
		{
			Env.Registry.AllowManualShipmentEntry = true;

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORD1111";
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			using (var form = new OrdersFormForTesting(order))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.ActionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration").PerformClick();

				AssertEquals(typeof(ConsolForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(order.Shipment);
			}
		}

		[RequiresSTA]
		public void TestGlobalFilterStillWorksWithUpdatedKey()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer3 = Factory.NewWithValidTestData<OrgHeader>();

			var labelString1 = buyer1.CustomLabels.AddNew();
			labelString1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
			labelString1.OT_Caption = "STRING1";

			var labelString2 = buyer2.CustomLabels.AddNew();
			labelString2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute2;
			labelString2.OT_Caption = "STRING2";

			var order1 = Factory.NewWithValidTestData<Order>();
			var order2 = Factory.NewWithValidTestData<Order>();
			var order3 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "ORD1111";
			order2.JD_OrderNumber = "ORD2222";
			order3.JD_OrderNumber = "ORD3333";
			order1.BuyerPK = buyer1.PK;
			order2.BuyerPK = buyer2.PK;
			order3.BuyerPK = buyer3.PK;

			Factory.Save();

			ZGridColumns columns1 = null;
			ZGridColumns columns2 = null;
			ZGridColumns columns3 = null;

			ZGridCustomiseBizObj customiseBizObj1 = null;
			ZGridCustomiseBizObj customiseBizObj2 = null;
			ZGridCustomiseBizObj customiseBizObj3 = null;

			using (var form1 = new OrdersForm_ForTesting(order1))
			using (var form2 = new OrdersForm_ForTesting(order2))
			using (var form3 = new OrdersForm_ForTesting(order3))
			{
				#region Form1
				form1.Show();

				var grid1 = form1.OrdersUserControl_ForTesting.GridExposed;
				grid1.Columns[JobOrderLineSchema.Constants.JO_CustomAttrib1].IsVisible = true;

				columns1 = grid1.Columns;
				var keyProvider1 = new LegacyDataGridLayoutContextKeyProvider(grid1);
				var legacyGridID1 = keyProvider1.ContextKeyForStmModuleFilter;
				customiseBizObj1 = new ZGridCustomiseBizObj(new string[] { legacyGridID1 }, new string[] { keyProvider1.ContextKeyForStmData }, null, ZGuid.Empty);

				var layoutSavedWithLegacyKey1 = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey1.S9_ModuleID = legacyGridID1;
				layoutSavedWithLegacyKey1.S9_FilterName = "ForTest1";
				layoutSavedWithLegacyKey1.S9_GC = ZGuid.Empty;
				layoutSavedWithLegacyKey1.S9_RelatedEntityID = ZGuid.Empty;
				layoutSavedWithLegacyKey1.S9_SaveColumnLayout = true;
				layoutSavedWithLegacyKey1.S9_IsPublished = true;
				layoutSavedWithLegacyKey1.S9_FilterData = ZBlob.Empty;

				columns1 = grid1.Columns;
				layoutSavedWithLegacyKey1.S9_ColumnLayoutData = GetColumnLayoutData(columns1);

				var gridLayoutManageable1 = new ZColumnsLayoutModification(columns1.ToArray(), Factory, null, keyProvider1.ContextKeyForStmModuleFilter);
				var layoutBizO1 = new SaveLayoutBizO(gridLayoutManageable1) { LayoutName = "TEST1", PublishLayout = true };

				Factory.Save();
				form1.Hide();
				#endregion

				#region Form2
				form2.Show();

				var grid2 = form2.OrdersUserControl_ForTesting.GridExposed;
				grid2.Columns[JobOrderLineSchema.Constants.JO_CustomAttrib2].IsVisible = true;

				columns2 = grid2.Columns;
				var keyProvider2 = new LegacyDataGridLayoutContextKeyProvider(grid2);
				var legacyGridID2 = keyProvider2.ContextKeyForStmModuleFilter;
				customiseBizObj2 = new ZGridCustomiseBizObj(new string[] { legacyGridID2 }, new string[] { keyProvider2.ContextKeyForStmData }, null, ZGuid.Empty);

				var layoutSavedWithLegacyKey2 = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey2.S9_ModuleID = legacyGridID2;
				layoutSavedWithLegacyKey2.S9_FilterName = "ForTest2";
				layoutSavedWithLegacyKey2.S9_GC = ZGuid.Empty;
				layoutSavedWithLegacyKey2.S9_RelatedEntityID = ZGuid.Empty;
				layoutSavedWithLegacyKey2.S9_SaveColumnLayout = true;
				layoutSavedWithLegacyKey2.S9_IsPublished = true;
				layoutSavedWithLegacyKey2.S9_FilterData = ZBlob.Empty;

				columns2 = grid2.Columns;
				layoutSavedWithLegacyKey2.S9_ColumnLayoutData = GetColumnLayoutData(columns2);

				var gridLayoutManageable2 = new ZColumnsLayoutModification(columns2.ToArray(), Factory, null, keyProvider2.ContextKeyForStmModuleFilter);
				var layoutBizO2 = new SaveLayoutBizO(gridLayoutManageable2) { LayoutName = "TEST2", PublishLayout = true };

				Factory.Save();
				form2.Hide();
				#endregion

				#region Form3
				form3.Show();

				var grid3 = form3.OrdersUserControl_ForTesting.GridExposed;

				var keyProvider3 = new LegacyDataGridLayoutContextKeyProvider(grid3);
				var legacyGridID3 = keyProvider3.ContextKeyForStmModuleFilter;
				customiseBizObj3 = new ZGridCustomiseBizObj(new string[] { legacyGridID3 }, new string[] { keyProvider3.ContextKeyForStmData }, null, ZGuid.Empty);

				var layoutSavedWithLegacyKey3 = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey3.S9_ModuleID = legacyGridID3;
				layoutSavedWithLegacyKey3.S9_FilterName = "ForTest3";
				layoutSavedWithLegacyKey3.S9_GC = ZGuid.Empty;
				layoutSavedWithLegacyKey3.S9_RelatedEntityID = ZGuid.Empty;
				layoutSavedWithLegacyKey3.S9_SaveColumnLayout = true;
				layoutSavedWithLegacyKey3.S9_IsPublished = true;
				layoutSavedWithLegacyKey3.S9_FilterData = ZBlob.Empty;

				columns3 = grid3.Columns;
				layoutSavedWithLegacyKey3.S9_ColumnLayoutData = GetColumnLayoutData(columns3);

				var gridLayoutManageable3 = new ZColumnsLayoutModification(columns3.ToArray(), Factory, null, keyProvider3.ContextKeyForStmModuleFilter);
				var layoutBizO3 = new SaveLayoutBizO(gridLayoutManageable3) { LayoutName = "TEST3", PublishLayout = true };

				Factory.Save();
				form3.Hide();
				#endregion

				//ZGridCustomise Forms
				using (var customiseForm1 = new ZGridCustomiseTester(columns1, columns1, customiseBizObj1))
				{
					customiseForm1.Show();
				}

				using (var customiseForm2 = new ZGridCustomiseTester(columns2, columns2, customiseBizObj2))
				{
					customiseForm2.Show();
				}
			}

			var filters = Factory.Load(typeof(StmModuleFilter), new ZQuery(StmModuleFilterSchema.S9_FilterName, SQLComparisonOperator.Like, "ForTest%"));
			AssertEquals(3, filters.Length);
			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"DataGridLayout|d8b61363-6171-4020-a349-b4d91a08a432|" + buyer1.PK.ToString(),
					"DataGridLayout|d8b61363-6171-4020-a349-b4d91a08a432|" + buyer2.PK.ToString(),
					"DataGridLayout|d8b61363-6171-4020-a349-b4d91a08a432"
				},
				filters.Select(f => ((StmModuleFilter)f).S9_ModuleID));
		}

		byte[] GetColumnLayoutData(ZGridColumns columns)
		{
			var columnLayoutData = new MemoryStream();
			var columnSettingTable = new DataTable("OGridColumnSettings");
			columnSettingTable.Columns.Add("MappingName", typeof(string));
			columnSettingTable.Columns.Add("Width", typeof(int)); // Programmatic constant
			columnSettingTable.Columns.Add("IsVisible", typeof(bool));
			foreach (var col in columns)
			{
				columnSettingTable.Rows.Add(new object[] { col.ColumnStyle.MappingName, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(col.ColumnStyle.Width), col.IsVisible });
			}
			columnSettingTable.WriteXml(columnLayoutData, XmlWriteMode.IgnoreSchema);
			columnLayoutData.Position = 0;
			return columnLayoutData.ToArray();
		}

		public void TestFilterBecomesGlobalWhenCustomColumnsRemoved()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var labelString = buyer.CustomLabels.AddNew();
			labelString.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
			labelString.OT_Caption = "STRING1";

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORD1111";
			order.BuyerPK = buyer.PK;

			Factory.Save();

			string gridID;

			using (var form = new OrdersForm_ForTesting(order))
			{
				form.Show();

				var grid = form.OrdersUserControl_ForTesting.GridExposed;
				grid.Columns[JobOrderLineSchema.Constants.JO_CustomAttrib1].IsVisible = true;

				var columns = grid.Columns;
				var keyProvider = new DataGridLayoutContextKeyProvider(grid, true);
				gridID = keyProvider.ContextKeyForStmModuleFilter;
				var customiseBizObj = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { keyProvider.ContextKeyForStmData }, null, ZGuid.Empty);

				var layoutSavedWithLegacyKey = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey.S9_ModuleID = gridID;
				layoutSavedWithLegacyKey.S9_FilterName = "Filter";
				layoutSavedWithLegacyKey.S9_GC = ZGuid.Empty;
				layoutSavedWithLegacyKey.S9_RelatedEntityID = ZGuid.Empty;
				layoutSavedWithLegacyKey.S9_SaveColumnLayout = true;
				layoutSavedWithLegacyKey.S9_IsPublished = true;
				layoutSavedWithLegacyKey.S9_FilterData = ZBlob.Empty;

				columns = grid.Columns;
				layoutSavedWithLegacyKey.S9_ColumnLayoutData = GetColumnLayoutData(columns);

				var gridLayoutManageable = new ZColumnsLayoutModification(columns.ToArray(), Factory, null, keyProvider.ContextKeyForStmModuleFilter);
				var layoutBizO = new SaveLayoutBizO(gridLayoutManageable) { LayoutName = "TEST", PublishLayout = true };

				Factory.Save();

				using (var customiseForm = new ZGridCustomiseTester(columns, columns, customiseBizObj))
				{
					customiseForm.Show();
					var filter = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "Filter", true);
					AssertNotNull(filter);
					AssertEquals("Layout should work with saved form", "DataGridLayout|d8b61363-6171-4020-a349-b4d91a08a432|" + buyer.PK.ToString(), filter.S9_ModuleID);
				}
			}

			var otherBuyer = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrder = Factory.NewWithValidTestData<Order>();
			otherOrder.JD_OrderNumber = "ORD2222";
			otherOrder.BuyerPK = otherBuyer.PK;

			using (var otherForm = new OrdersForm_ForTesting(otherOrder))
			{
				otherForm.Show();

				var otherGrid = otherForm.OrdersUserControl_ForTesting.GridExposed;

				var otherColumns = otherGrid.Columns;
				var otherKeyProvider = new DataGridLayoutContextKeyProvider(otherGrid);
				var otherGridID = otherKeyProvider.ContextKeyForStmModuleFilter;

				otherColumns = otherGrid.Columns;

				Factory.Save();

				using (var customiseForm = new ZGridCustomiseTester(otherColumns, otherColumns,
					new ZGridCustomiseBizObj(new string[] { otherGridID }, new string[] { otherKeyProvider.ContextKeyForStmData }, null, ZGuid.Empty)))
				{
					customiseForm.Show();
					var filter = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "Filter", true);
					AssertNotEquals("Layout should not work with saved form", "DataGridLayout|d8b61363-6171-4020-a349-b4d91a08a432", filter.S9_ModuleID);
				}
			}

			buyer.CustomLabels.RemoveAndDelete(labelString);

			Factory.Save();

			using (var form = new OrdersForm_ForTesting(order))
			{
				form.Show();

				var grid = form.OrdersUserControl_ForTesting.GridExposed;
				grid.Columns[JobOrderLineSchema.Constants.JO_CustomAttrib1].IsVisible = true;

				var columns = grid.Columns;
				var keyProvider = new DataGridLayoutContextKeyProvider(grid, true);
				gridID = keyProvider.ContextKeyForStmModuleFilter;
				var customiseBizObj = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { keyProvider.ContextKeyForStmData }, null, ZGuid.Empty);

				Factory.Save();

				var gridIDNew = new DataGridLayoutContextKeyProvider(grid, false).ContextKeyForStmModuleFilter;

				using (var customiseForm = new ZGridCustomiseTester(columns, columns, customiseBizObj))
				{
					customiseForm.Show();
					var filter = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridIDNew, "Filter", true);
					AssertNotNull(filter);
					AssertEquals("Layout should work with saved form", "DataGridLayout|d8b61363-6171-4020-a349-b4d91a08a432", filter.S9_ModuleID);
				}
			}
		}

		#region Test Saving

		public void TestSaving()
		{
			var order = Factory.NewWithValidTestData<Order>();

			using (var form = new OrdersFormForTesting(order))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				supplier.OH_Code = "SUPAAASUP";

				var buyer = Factory.NewWithValidTestData<OrgHeader>();
				buyer.OH_Code = "BUYAAABUY";

				order.SupplierPK = supplier.PK;
				order.BuyerPK = buyer.PK;

				form.FireSaveButton();

				AssertEquals("Should show to Save Buyer-Supplier link dialog", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.No, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("Should not be linked", 0, order.Buyer.SupplierLinks.Count);

				supplier = Factory.NewWithValidTestData<OrgHeader>();
				supplier.OH_Code = "SUPBBBSUP";

				order.SupplierPK = supplier.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FireSaveButton();

				AssertEquals("Should show to Save Buyer-Supplier link dialog", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.Yes, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("Should be linked now", 1, order.Buyer.SupplierLinks.Count);
			}
		}

		#endregion

		#region eConversations

		public void TestEConversationsPlugIn_Visible()
		{
			AssertEConversationPlugInVisibility(true);
		}

		public void TestEConversationsPlugIn_Hidden()
		{
			AssertEConversationPlugInVisibility(false);
		}

		void AssertEConversationPlugInVisibility(bool registryValue)
		{
			var order = Factory.NewWithValidTestData<Order>();

			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new OrdersFormForTesting(order))
			{
				var eConverationPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);

				if (registryValue)
				{
					AssertNotNull(eConverationPlugIn);
				}
				else
				{
					AssertNull(eConverationPlugIn);
				}
			}
		}

		#endregion

		#region Implementation

		class OrdersForm_ForTesting : OrdersForm
		{
			public OrdersForm_ForTesting(Order bO) : base(bO)
			{
				OrdersUserControl_ForTesting = new OrdersUserControl_ForTesting();
			}

			public OrdersUserControl_ForTesting OrdersUserControl_ForTesting
			{
				get { return ordersUserControl_ForTesting; }
				set
				{
					if (ordersUserControl_ForTesting != null)
					{
						OrdersUserControl.Controls.Remove(ordersUserControl_ForTesting);
						ordersUserControl_ForTesting.Dispose();
					}
					OrdersUserControl.Controls.Add(value);
					ordersUserControl_ForTesting = value;
					value.Dock = DockStyle.Fill;
				}
			}

			OrdersUserControl_ForTesting ordersUserControl_ForTesting;

			protected override void Dispose(bool disposing)
			{
				if (OrdersUserControl_ForTesting != null)
				{
					OrdersUserControl_ForTesting.Dispose();
				}

				base.Dispose(disposing);
			}
		}

		class OrdersUserControl_ForTesting : OrdersUserControl
		{
			public OrdersUserControl_ForTesting() : base() { }

			public ZGrid GridExposed => OrderLinesBoundButtonGrid.InnerGrid;
		}

		protected override Form GetFormToBashCore()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Order bO = factory.New<Order>();
			var shipment = factory.New<ForwardingShipment>();
			bO.JD_JS = shipment.PK;

			foreach (var log in bO.Logs.GetAllLogs())
			{
				log.HasChanges = false;
			}

			shipment.Consols.AddNew();

			foreach (ProcessTask task in bO.WorkflowItems)
			{
				task.HasChanges = false;
			}

			shipment.HasChanges = false;
			bO.HasChanges = false;

			OrdersForm result = new OrdersForm(bO);
			result.ControllerID = ControllerIDs.Orders;
			return result;
		}

		protected Form GetEditFormToBash()
		{
			var factory = new BusinessObjectFactory();
			var order = factory.NewWithValidTestData<Order>();

			var filter = new ZQuery();
			filter.MaximumRows = 2;

			var orgs = factory.Load<OrgHeader>(filter);

			order.BuyerPK = orgs[0].PK;
			order.SupplierPK = orgs[1].PK;

			order.JD_OrderNumber = "99999";
			var shipment = factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			order.JD_JS = shipment.PK;
			shipment.Consols.AddNew();

			factory.Save();

			var result = new OrdersForm(order);
			result.ControllerID = ControllerIDs.Orders;

			return result;
		}

		#endregion
	}
}
