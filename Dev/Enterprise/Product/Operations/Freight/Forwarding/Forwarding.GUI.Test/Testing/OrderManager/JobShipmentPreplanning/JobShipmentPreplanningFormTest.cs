using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(JobShipmentPreplanningForm))]
	class JobShipmentPreplanningFormTest : ZFormBasherTest
	{
		#region Notification

		[RequiresSTA]
		public void TestNotify()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Notify(new InfoNotification("hello"));
				AssertEquals("hello", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestQueryUser()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.QueryUser(new QueryUserMsgBoxEventArgs("hello zubs", false));
				AssertEquals("hello zubs", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveInternal()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = buyer.PK;
			preAdvice.EF_OH_Carrier = carrier.PK;
			preAdvice.EF_MasterBill = "MASTER";
			preAdvice.EF_RL_NKPortLoad = "AUSYD";
			preAdvice.EF_RL_NKPortDisch = "USLAX";

			var vessel = Factory.NewWithValidTestData<RefVessel>();

			var transport = preAdvice.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.CarrierPK = carrier.PK;
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2016, 8, 23);
			transport.JW_ETA = new ZDateTime(2016, 8, 25);

			var order = preAdvice.Orders.AddNew();
			order.BuyerPK = preAdvice.Buyer.PK;
			order.JD_OH_Carrier = preAdvice.Carrier.PK;

			using (var form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Show();

				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_Quantity = 2;
				orderLine.JO_QtyReceived = 1;
				form.FireSaveButton();

				AssertEquals(typeof(OrderSplitMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#endregion

		#region Actions Menu

		public void TestActionsMenu()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Show();
				var actionsMenuItem = form.GetProtectedField<MenuItem, ZForm>("ActionsMenuItem");

				AssertNotNull(actionsMenuItem.MenuItems.FindByText("Create Declaration"));
				AssertNotNull(actionsMenuItem.MenuItems.FindByText("Create Consol/Shipment/Declaration"));
				AssertNotNull(actionsMenuItem.MenuItems.FindByText("Single House Bill", true));
				AssertNotNull(actionsMenuItem.MenuItems.FindByText("House Bill per Buyer/Supplier", true));
				AssertNotNull(actionsMenuItem.MenuItems.FindByText("House Bill per Order", true));
				AssertNotNull(actionsMenuItem.MenuItems.FindByText("Add Commercial Invoice to Declaration"));
			}
		}

		public void TestCreateShipments()
		{
			JobShipmentPreplanning preplanning1 = Factory.New<JobShipmentPreplanning>();
			preplanning1.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			preplanning1.EF_MasterBill = "MASTER";
			preplanning1.EF_RL_NKPortDisch = "USLAX";

			Transport transport = preplanning1.PreAdviceTransports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "VESSEL2";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKDiscPort = "USLAX";

			Order preAdvice1Order = preplanning1.Orders.AddNew();
			preAdvice1Order.BuyerPK = preplanning1.Buyer.PK;
			preAdvice1Order.JD_OrderNumber = "353535";

			Factory.Save();

			AssertEquals(1, preplanning1.MatchingPreAdvices.Count);

			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				var actionsMenuItem = form.GetProtectedField<MenuItem, ZForm>("ActionsMenuItem");
				actionsMenuItem.MenuItems.FindByText("House Bill per Buyer/Supplier", true).PerformClick();

				AssertNull(ZFormModaliser.ActiveForm);
			}

			JobShipmentPreplanning preplanning2 = Factory.New<JobShipmentPreplanning>();
			preplanning2.BuyerPK = preplanning1.BuyerPK;
			preplanning2.EF_MasterBill = preplanning1.EF_MasterBill;

			transport = preplanning2.PreAdviceTransports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "VESSEL2";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKDiscPort = "USLAX";

			Order preAdvice2Order = preplanning2.Orders.AddNew();
			preAdvice2Order.BuyerPK = preplanning1.Buyer.PK;
			preAdvice2Order.JD_OrderNumber = "31111112";

			Factory.Save();

			AssertEquals(2, preplanning1.MatchingPreAdvices.Count);

			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				var actionsMenuItem = form.GetProtectedField<MenuItem, ZForm>("ActionsMenuItem");
				FindMenuItem(actionsMenuItem.MenuItems, "Create Consol/Shipment/Declaration", "House Bill per Buyer/Supplier").PerformClick();

				AssertEquals(typeof(PreAdviceExportToForwardingJobForm), ZFormModaliser.ActiveForm.GetType());
				ZFormModaliser.ActiveForm.Dispose();
			}
		}

		[RequiresSTA]
		public void TestCreateShipments_ManuallyEnterShipmentNumber()
		{
			bool oldValue = Env.Registry.AllowManualShipmentEntry;
			try
			{
				Env.Registry.AllowManualShipmentEntry = true;

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "CARRIER1";
				carrier.OH_IsShippingLine = true;
				carrier.OH_IsShippingProvider = true;

				JobShipmentPreplanning preplanning1 = Factory.New<JobShipmentPreplanning>();
				var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
				buyer.OH_IsConsignee = true;
				buyer.OH_IsForwarder = true;
				preplanning1.BuyerPK = buyer.PK;
				preplanning1.EF_MasterBill = "MASTER";
				preplanning1.EF_RL_NKPortLoad = "AUSYD";
				preplanning1.EF_RL_NKPortDisch = "USLAX";
				preplanning1.EF_OH_Carrier = carrier.PK;

				Transport transport = preplanning1.PreAdviceTransports[0];
				transport.JW_TransportMode = "SEA";
				transport.JW_Vessel = "GRUMANT";
				transport.JW_VoyageFlight = "QF123";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_ETD = ZDateTime.Today;

				Order preAdvice1Order1 = preplanning1.Orders.AddNew();
				preAdvice1Order1.BuyerPK = preplanning1.Buyer.PK;
				preAdvice1Order1.JD_OrderNumber = "353535";

				Order preAdvice1Order2 = preplanning1.Orders.AddNew();
				preAdvice1Order2.BuyerPK = preplanning1.Buyer.PK;
				preAdvice1Order2.JD_OrderNumber = "898989";

				Factory.Save();

				AssertEquals(1, preplanning1.MatchingPreAdvices.Count);

				using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
				using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
				{
					form.Show();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.DataSource.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerOrder, form, TryToFixConsolErrors_DontShowForm);

					AssertEquals(typeof(ManualShipmentNumberEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals("Heya0", preAdvice1Order1.Shipment.JS_UniqueConsignRef);
				}
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = oldValue;
			}
		}

		[RequiresSTA]
		public void TestCreateShipments_ConversionForm()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			JobShipmentPreplanning preplanning1 = Factory.New<JobShipmentPreplanning>();
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			buyer.OH_IsConsignee = true;
			buyer.OH_IsForwarder = true;
			preplanning1.BuyerPK = buyer.PK;
			preplanning1.EF_MasterBill = "MASTER";
			preplanning1.EF_RL_NKPortLoad = "AUSYD";
			preplanning1.EF_RL_NKPortDisch = "USLAX";
			preplanning1.EF_OH_Carrier = carrier.PK;

			Transport transport = preplanning1.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "GRUMANT";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = ZDateTime.Today;

			Order preAdvice1Order = preplanning1.Orders.AddNew();
			preAdvice1Order.BuyerPK = preplanning1.Buyer.PK;
			preAdvice1Order.JD_OrderNumber = "353535";

			preAdvice1Order.OrderLines.AddNew();
			preAdvice1Order.OrderLines.AddNew();

			Factory.Save();

			AssertEquals(1, preplanning1.MatchingPreAdvices.Count);

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				form.DataSource.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, form, TryToFixConsolErrors_DontShowForm);

				AssertNotNull("Conversion Form", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(OrderLineToPackLineConversionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new JobShipmentPreplanningForm(Factory.New<JobShipmentPreplanning>()));
		}

		public void TestCreateShipments_ConsolAndShipmentsCheck()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();

			var preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_HouseBill = "HOUSE";
			preplanning.EF_MasterBill = "MASTER";
			preplanning.BuyerPK = buyer.PK;
			preplanning.EF_OH_Carrier = carrier.PK;
			preplanning.EF_OH_SendingAgent = agent1.PK;
			preplanning.EF_OH_ReceivingAgent = agent2.PK;
			preplanning.EF_RL_NKPortLoad = "AUSYD";
			preplanning.EF_RL_NKPortDisch = "USLAX";

			var transport1 = preplanning.PreAdviceTransports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_Vessel = "VESSEL1";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "INBOM";
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport1.JW_ETD = new ZDateTime(2016, 8, 23);
			transport1.JW_ATD = new ZDateTime(2016, 8, 24);
			transport1.JW_ETA = new ZDateTime(2016, 8, 25);
			transport1.JW_ATA = new ZDateTime(2016, 8, 26);

			var order1 = preplanning.Orders.AddNew();
			order1.JD_OrderNumber = "ORDER1";
			order1.BuyerPK = buyer.PK;
			order1.SupplierPK = supplier1.PK;
			order1.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order1.JD_RL_NKGoodsDeliveredTo = "USLAX";
			order1.JD_TransportMode = "SEA";
			order1.JD_IncoTerm = "FCA";
			order1.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			order1.JD_RN_NKCountryOfSupply = "NZ";
			order1.JD_RS_NKServiceLevel_NI = "AAA";

			var order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_Quantity = 10m;
			order1Line1.JO_QtyReceived = 10m;
			order1Line1.JO_QtyInvoiced = 10m;
			order1Line1.JO_Description = "Order 1 Line 1";
			order1Line1.JO_LinePrice = 20;
			order1Line1.JO_CommercialInvoiceNo = "COMM1";
			order1Line1.JO_F3_NKPackType = "XX";
			order1Line1.JO_Partno = "ABC";
			order1Line1.JO_RN_NKCountryOfOrigin = "FI";

			var container1 = preplanning.Containers.AddNew();
			container1.J1_RC = new ZGuid();
			container1.J1_ContainerNumber = "TEST1";
			container1.J1_SealNum = "SEAL11";
			container1.J1_AdditionalSealNum = "SEAL12";
			container1.J1_Additional2SealNum = "SEAL13";

			preplanning.EF_ActualVolume = 30m;
			preplanning.EF_ActualWeight = 100m;
			preplanning.EF_Packs = 10;
			preplanning.EF_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			preplanning.EF_UnitOfWeight = Core.Constants.Weight.LongTons;
			preplanning.EF_F3_NKPackType = "BOX";
			preplanning.EF_MasterBill = "";
			Factory.Save();

			using (var form = new JobShipmentPreplanningForm(preplanning))
			{
				form.Show();
				var actionsMenuItem = form.GetProtectedField<MenuItem, ZForm>("ActionsMenuItem");
				FindMenuItem(actionsMenuItem.MenuItems, "Create Consol/Shipment/Declaration", "House Bill per Order").PerformClick();

				AssertNotNull("Consol Form", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(ConsolForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#endregion

		#region Delivery Wizard

		public void TestDeliveryWizard()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Show();
				Application.DoEvents(); // to allow OnShown() to fire

				MenuItem deliveryWizardItem = null;
				var ordersGrid = form.FindSingle<OrderModuleButtonGrid>(ctrl => ctrl.Name == "OrdersGrid");
				foreach (MenuItem item in ordersGrid.InnerGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Product Delivery Wizard")
					{
						deliveryWizardItem = item;
						break;
					}
				}
				AssertNotNull(deliveryWizardItem);

				deliveryWizardItem.PerformClick();
				AssertEquals(typeof(ProductDeliveryForm), ZFormModaliser.ActiveForm.GetType());
				ZFormModaliser.ActiveForm.Dispose();

				var deliveryWizardButton = form.FindSingle<ZButton>("DeliveryWizardButton");
				deliveryWizardButton.PerformClick();
				AssertEquals(typeof(ProductDeliveryForm), ZFormModaliser.ActiveForm.GetType());
				ZFormModaliser.ActiveForm.Dispose();
			}
		}

		#endregion

		#region Existing AIR Consol Warning Dialog

		[RequiresSTA]
		public void TestSingleBillWithExistingAIRConsolWithBlankMasterBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "";

			Factory.Save();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var preplanning1 = Factory.New<JobShipmentPreplanning>();
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			buyer.OH_IsConsignee = true;
			preplanning1.BuyerPK = buyer.PK;
			preplanning1.Orders.AddNew();
			preplanning1.EF_MasterBill = "";
			preplanning1.EF_RL_NKPortLoad = "AUSYD";
			preplanning1.EF_RL_NKPortDisch = "USLAX";
			preplanning1.EF_OH_Carrier = carrier.PK;

			Transport transport = preplanning1.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "GRUMANT";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = ZDateTime.Today;

			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				form.DataSource.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, form, TryToFixConsolErrors_DontShowForm);
				AssertContains("Shouldn't error", "and shipment(s) have been successfully created from this pre-advice.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSingleBillWithExistingAIRConsolWithBlankMasterBillAndMatchingPreAdvices()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "";

			Factory.Save();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var buyer1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			buyer1.OH_IsConsignee = true;

			var preplanning1 = Factory.New<JobShipmentPreplanning>();
			preplanning1.BuyerPK = buyer1.PK;
			var order1 = preplanning1.Orders.AddNew();
			order1.JD_OrderNumber = "o111";
			preplanning1.EF_MasterBill = "";
			preplanning1.EF_RL_NKPortLoad = "AUSYD";
			preplanning1.EF_RL_NKPortDisch = "USLAX";
			preplanning1.EF_OH_Carrier = carrier.PK;

			Transport transport = preplanning1.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "GRUMANT";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = ZDateTime.Today;

			var buyer2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			buyer2.OH_IsConsignee = true;

			var preplanning2 = Factory.New<JobShipmentPreplanning>();
			preplanning2.BuyerPK = buyer2.PK;
			var order2 = preplanning2.Orders.AddNew();
			order2.JD_OrderNumber = "o222";
			preplanning2.EF_MasterBill = "";

			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				form.DataSource.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, form, TryToFixConsolErrors_DontShowForm);
				AssertContains("Shouldn't error", "and shipment(s) have been successfully created from this pre-advice.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSingleBillWithExistingAIRConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MASTER";

			Factory.Save();

			JobShipmentPreplanning preplanning1 = Factory.New<JobShipmentPreplanning>();
			preplanning1.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			preplanning1.EF_MasterBill = "MASTER";

			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				var actionsMenuItem = form.GetProtectedField<MenuItem, ZForm>("ActionsMenuItem");
				FindMenuItem(actionsMenuItem.MenuItems, "Create Consol/Shipment/Declaration", "Single House Bill").PerformClick();

				AssertEquals("An air consol already exists for this Master Bill. Same Master Bill can be used only for one air consol.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMultipleBillsWithExistingAIRConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MASTER";

			Factory.Save();

			JobShipmentPreplanning preplanning1 = Factory.New<JobShipmentPreplanning>();
			preplanning1.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			preplanning1.EF_MasterBill = "MASTER";

			using (JobShipmentPreplanningForm form = new JobShipmentPreplanningForm(preplanning1))
			{
				form.Show();
				var actionsMenuItem = form.GetProtectedField<MenuItem, ZForm>("ActionsMenuItem");
				FindMenuItem(actionsMenuItem.MenuItems, "Create Consol/Shipment/Declaration", "House Bill per Buyer/Supplier").PerformClick();

				AssertEquals("An air consol already exists for this Master Bill. Same Master Bill can be used only for one air consol.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Add Existing Shipment to Pre Advice

		public void TestAddExistingShipmentToPreAdvice_ShouldShowConversionFormAndCreatePackLinesFromOrderLines()
		{
			var preAdvice = PreparePreAdvice();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			Factory.Save();

			using (var form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Show();
				form.DataSource.EF_JS = shipment.PK;

				using (var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals(typeof(OrderLineToPackLineConversionForm), lastShownDialog.GetType());
				}

				AssertEquals(2, shipment.OuterPackLines.Count);

				var packline1 = shipment.OuterPackLines[0];
				CombineAssertions(() =>
				{
					AssertEquals(new ZDecimal(12), packline1.JL_Length);
					AssertEquals(new ZDecimal(14), packline1.JL_Height);
					AssertEquals(new ZDecimal(24), packline1.JL_Width);
				});

				var packline2 = shipment.OuterPackLines[1];
				CombineAssertions(() =>
				{
					AssertEquals(new ZDecimal(19), packline2.JL_Length);
					AssertEquals(new ZDecimal(28), packline2.JL_Height);
					AssertEquals(new ZDecimal(23), packline2.JL_Width);
				});
			}
		}

		public void TestAddExistingShipmentToPreAdvice_ShouldNotShowConversionForm_WhenOrderDoesNotHaveAnyOrderLine()
		{
			var preAdvice = PreparePreAdvice(addOrderLines: false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			Factory.Save();

			using (var form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Show();
				form.DataSource.EF_JS = shipment.PK;

				using (var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNull(lastShownDialog);
				}

				AssertEquals(0, shipment.OuterPackLines.Count);
			}
		}

		[RequiresSTA]
		public void TestAddExistingShipmentToPreAdvice_ShouldNotShowConversionFormAndShouldNotCreatePackLinesFromOrderLines_WhenUserSelectNoAsAnswerOfDialogbox()
		{
			var preAdvice = PreparePreAdvice();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			Factory.Save();

			using (var form = new JobShipmentPreplanningForm(preAdvice))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.DataSource.EF_JS = shipment.PK;

				using (var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNull(lastShownDialog);
				}

				AssertEquals(0, shipment.OuterPackLines.Count);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			JobShipmentPreplanningForm result = new JobShipmentPreplanningForm(preAdvice);
			result.ControllerID = ControllerIDs.JobShipmentPreplanning;
			return result;
		}

		Func<ForwardingConsol, bool> TryToFixConsolErrors_DontShowForm =>
			consol =>
			{
				consol.JK_RL_NKLastForeignPort = "USLAX";

				var carrier = consol.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CARRIER1")).FirstOrDefault();
				if (carrier != null)
				{
					foreach (Transport transport in consol.Transports)
					{
						transport.CarrierPK = carrier.PK;
					}
				}
				consol.Factory.Save();
				return true;
			};

		static MenuItem FindMenuItem(Menu.MenuItemCollection items, params string[] path)
		{
			return FindMenuItem(items, path, 0);
		}

		static MenuItem FindMenuItem(Menu.MenuItemCollection items, string[] path, int index)
		{
			MenuItem next = null;

			foreach (MenuItem item in items)
			{
				string text = item.Text.Replace("&", "");
				if (text == path[index])
				{
					next = item;
					break;
				}
			}

			return index + 1 == path.Length || next == null ? next : FindMenuItem(next.MenuItems, path, index + 1);
		}

		JobShipmentPreplanning PreparePreAdvice(bool addOrderLines = true)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			buyer.OH_IsConsignee = true;

			var preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.BuyerPK = buyer.PK;
			preAdvice.EF_MasterBill = "";
			preAdvice.EF_RL_NKPortLoad = "AUSYD";
			preAdvice.EF_RL_NKPortDisch = "USLAX";
			preAdvice.EF_OH_Carrier = carrier.PK;

			var transport = preAdvice.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "GRUMANT";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = ZDateTime.Today;

			preAdvice.Orders.AddNew();
			var order = preAdvice.Orders.AddNew();
			order.BuyerPK = preAdvice.Buyer.PK;
			order.JD_OH_Carrier = preAdvice.Carrier.PK;

			if (addOrderLines)
			{
				var orderLine1 = order.OrderLines.AddNew();
				orderLine1.JO_Quantity = 2;
				orderLine1.JO_QtyReceived = 1;
				orderLine1.JO_OuterPackLength = 12;
				orderLine1.JO_OuterPackHeight = 14;
				orderLine1.JO_OuterPackWidth = 24;

				var orderLine2 = order.OrderLines.AddNew();
				orderLine2.JO_Quantity = 34;
				orderLine2.JO_QtyReceived = 21;
				orderLine2.JO_OuterPackLength = 19;
				orderLine2.JO_OuterPackHeight = 28;
				orderLine2.JO_OuterPackWidth = 23;
			}

			return preAdvice;
		}

		#endregion
	}
}
