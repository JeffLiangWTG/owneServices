using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;
using Res = Enterprise.Freight.Forwarding.GUI.Res;
using ResString = Enterprise.Freight.Forwarding.GUI.ResString;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersForm : ZForm,
		INotifications,
		INotificationSubscriberQueryUser,
		IButtonPostTextOverride
	{
		public OrdersForm()
		{
			InitializeComponent();
			OrdersUserControl.Inner = NewOrdersUserControl();
		}

		enum CheckOrderAttachedToBookingOrShipmentAction
		{
			CreateStandaloneShipment,
			CreateShipment,
			CreateQuickBooking,
		}

		protected OrdersUserControlDecider OrdersUserControl;

		public OrdersForm(OrderLine line)
			: this(line.Factory.Load<Order>(line.JO_JD))
		{
			base.IdentifierForPersistingForm = line.PK.ToGuid();
		}

		public OrdersForm(Order bO)
			: base(bO)
		{
			InitializeComponent();

			Order = bO;
			OrdersUserControl.Inner = NewOrdersUserControl();
			OrdersUserControl.DockInside(OrdersTabPage);

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DataContext = Constants.DataContext.Order;
			MinimumSize = Size;
			SetupActionsMenu();

			PlugIns.Add(ControllerIDs.LandedCosting);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);

			if (GlowRegistry.Instance.NeoEnableConversations.Value)
			{
				PlugIns.Add(ControllerIDs.eConversationPlugIn);
			}

			Order.Validation.RunFormShowValidation();
			Order.OrderNumberChangeAttemptedWhileSplitsExist += new EventHandler(OnOrderNumberChangeAttemptedWhileSplitsExist);
			Order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += new CancelEventHandler(Order_OnDetachingPreAdviceAskToDetachShipmentAndDeclaration);
		}

		public readonly Order Order;

		protected virtual OrdersUserControl NewOrdersUserControl()
		{
			return new OrdersUserControl();
		}

		#region Actions Menu

		void SetupActionsMenu()
		{
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.SplitOrder", "Split Order"), new EventHandler(OnSplitOrder_Click)));
			ActionsMenuItem.Enabled = true;
			WorkflowTabPage.Initialize(Order);
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItems);
			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.CreateShipmentPreAdvice", "Create Shipment Pre Advice"), CreatePreAdvice_Click));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.CreateDeclaration", "Create Declaration"), CreateDec_Click));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.CreateConsolShipmentDeclaration", "Create Consol/Shipment/Declaration"), CreateShipment_Click));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.CreateStandaloneShipment", "Create Shipment"), CreateStandaloneShipment_Click));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.ACtions.CreateQuickBooking", "Create Quick Booking"), CreateQuickBooking_Click));

			warehouseReceiveMenuItem = new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.ViewWarehouseReceive", "View Warehouse Receive"), WarehouseReceive_Click);
			warehouseReceiveMenuItem.Visible = false;
			ActionsMenuItem.MenuItems.Add(warehouseReceiveMenuItem);
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.AddOrderLine", "Add Order Line to Commercial Invoice"), AddOrderLine_Click));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.Orders.Actions.AddCommercialInvoice", "Add Commercial Invoice to Declaration"), AddCommInvoice_Click));
			ActionsMenuItem.Popup += ActionsMenuItem_Popup;
		}

		ZMenuItem warehouseReceiveMenuItem;

		void CreatePreAdvice_Click(object sender, EventArgs args)
		{
			Order.CreateAndLinkPreAdviceToOrder(this, Order.TargetObjectForCreate.PreAdvice, ShowConfirmationForSplittingOrder);
		}

		void CreateDec_Click(object sender, EventArgs args)
		{
			var preAdvice = Order.CreateAndLinkPreAdviceToOrder(this, Order.TargetObjectForCreate.Declaration, ShowConfirmationForSplittingOrder);
			if (preAdvice != null)
			{
				preAdvice.CreateStandAloneBrokerage(this, Order);
			}
		}

		void CreateShipment_Click(object sender, EventArgs args)
		{
			if (!CheckOrderAttachedToBookingOrShipment(Res.GetString("23bee6e2-d796-486a-90fe-bbda802bcb0d", "Create Shipment"), CheckOrderAttachedToBookingOrShipmentAction.CreateShipment))
			{
				return;
			}

			var preAdvice = Order.CreateAndLinkPreAdviceToOrder(this, Order.TargetObjectForCreate.Shipment, ShowConfirmationForSplittingOrder);
			if (preAdvice != null)
			{
				if (preAdvice.MatchingPreAdvices.Count > 1)
				{
					ZFormModaliser.Show(new PreAdviceExportToForwardingJobForm(preAdvice, JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, this), this);
				}
				else
				{
					preAdvice.OnOrderLineToPackLineConversion -= AttachOrderToShipmentHelper.OrderLineToPackLineConversion;
					preAdvice.OnOrderLineToPackLineConversion += AttachOrderToShipmentHelper.OrderLineToPackLineConversion;
					preAdvice.OnAfterShipmentsCreated += ManualShipmentNumberEntry.ShipmentNumber_OnAfterShipmentsCreated;
					preAdvice.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, this, PreAdviceConversionHelper.ShowFormWithNewlyCreatedConsol());
					preAdvice.OnAfterShipmentsCreated -= ManualShipmentNumberEntry.ShipmentNumber_OnAfterShipmentsCreated;

					OrdersUserControl.Inner.RefreshPlanningTabText();
				}
			}
		}

		void CreateQuickBooking_Click(object sender, EventArgs args)
		{
			var createQuickBookingRes = Res.GetString("5e139b4d-d3c5-42cf-a615-e7c314e2cc25", "Create Quick Booking");
			if (Order.HasChanges || !Order.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("6ee148b5-a934-47c4-a233-9b7111b6effd",
					"Please save the order before creating a Quick Booking"),
					createQuickBookingRes, MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (!CheckOrderAttachedToBookingOrShipment(createQuickBookingRes, CheckOrderAttachedToBookingOrShipmentAction.CreateQuickBooking))
			{
				return;
			}

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quotedBooking = quotedBookingBuilder.CreateAndPopulateFromOrder(Order.PK, Order.Factory);

			ZControllerFactory.Create(ControllerIDs.QuotedBookings).ShowFormForNewEntity(quotedBooking as IBusiness);
		}

		void CreateStandaloneShipment_Click(object sender, EventArgs args)
		{
			if (Order.HasChanges || !Order.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("25cc4fe2-1bca-42d8-b788-647e6a9543c2",
						"Please save the order before creating a Shipment"),
					Res.GetString("a48474fb-ec23-45a5-84fa-cad2f53e5541", "Create Standalone Shipment"), MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (!CheckOrderAttachedToBookingOrShipment(Res.GetString("429dde3d-1f24-4087-9e90-fbee433e8c54", "Create Standalone Shipment"), CheckOrderAttachedToBookingOrShipmentAction.CreateStandaloneShipment))
			{
				return;
			}

			var controller = ZControllerFactory.Create(ControllerIDs.JobShipment);

			var shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(Order.PK, controller.Factory.New<ForwardingShipment>(), AttachOrderToShipmentHelper.OrderLineToPackLineConversion);

			controller.ShowFormForNewEntity(shipment);
		}

		bool CheckOrderAttachedToBookingOrShipment(string caption, CheckOrderAttachedToBookingOrShipmentAction action)
		{
			if (action is CheckOrderAttachedToBookingOrShipmentAction.CreateShipment || action is CheckOrderAttachedToBookingOrShipmentAction.CreateStandaloneShipment)
			{
				if (Order.IsBookingAttached)
				{
					var result = Globals.Message.Show(
						Res.GetString("8a841be1-2da8-4b42-9dc1-db721162dfe5", "Order is already linked to booking {0}. Convert booking to shipment?", Order.QuotedBooking.UniqueConsignRef),
						caption,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Information);
					if (result == DialogResult.Yes)
					{
						var booking = (QuotedBooking)Order.QuotedBooking;
						var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);

						if (converter.HasAnyErrors(out var errorMessage))
						{
							Globals.Message.Show(
								errorMessage,
								Res.GetString("da3c99bd-0ab1-42da-92c1-ce8e420240d6", "Convert to shipment"),
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning
							);
						}
						else
						{
							var shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
							var forwardingShipment = shipmentController.Factory.Load<ForwardingShipment>(booking.Booking.PK);

							if (forwardingShipment != null)
							{
								forwardingShipment.DebugLog.AppendLine((NoResString)"Before converting to shipment.");
								forwardingShipment.LogDebugInfo();
								converter.ConvertBookingToShipment(forwardingShipment);
								forwardingShipment.DebugLog.AppendLine((NoResString)"After converting to shipment.");
								forwardingShipment.LogDebugInfo();
								var shipmentForm = (ZForm)shipmentController.ShowEditForm(forwardingShipment);
								if (shipmentForm != null)
								{
									shipmentForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing)?.SetSecurityCheckpoint(forwardingShipment.InvoicingSupporter.JobInvoicingSecurity, Env.Security.None);
								}
							}
						}
						Close();
					}

					return false;
				}

				if (Order.IsShipmentAttached)
				{
					Globals.Message.Show(
						Res.GetString("f603d885-cdc6-4525-bfdb-120e2e0be256", "Order is already linked to shipment {0}. Detach this shipment before creating a new shipment.", Order.Shipment.JS_UniqueConsignRef),
						caption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return false;
				}
			}

			if (action is CheckOrderAttachedToBookingOrShipmentAction.CreateQuickBooking)
			{
				if (Order.IsBookingAttached)
				{
					Globals.Message.Show(
						Res.GetString("0a800b8e-63fd-4c4d-9948-c3e28cfad3be", "Order is already linked to booking {0}. Detach this booking before creating a new booking.", Order.QuotedBooking.UniqueConsignRef),
						caption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return false;
				}

				if (Order.IsShipmentAttached)
				{
					Globals.Message.Show(
						Res.GetString("994549f2-006e-4168-8a1a-66a3ea775c67", "Order is already linked to shipment {0}. Detach this shipment before creating a new booking.", Order.Shipment.JS_UniqueConsignRef),
						caption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return false;
				}
			}

			return true;
		}

		void AddCommInvoice_Click(object sender, EventArgs args)
		{
			var preAdvice = Order.CreateAndLinkPreAdviceToOrder(this, Order.TargetObjectForCreate.PreAdvice, ShowConfirmationForSplittingOrder);
			if (preAdvice != null)
			{
				preAdvice.AppendInvoiceLineToCustomsDeclaration(this);
			}
		}

		void AddOrderLine_Click(object sender, EventArgs args)
		{
			Order.AppendOrdersLineToInvoice(this);
		}

		#region ActionsMenuItem_Popup

		void ActionsMenuItem_Popup(object sender, EventArgs e)
		{
			var menuCaption = (Order.RelatedWarehouseReceive != null)
				? ResString.GetMultilingualString("OrderManager.Orders.Actions.ViewWarehouseReceive", "View Warehouse Receive")
				: ResString.GetMultilingualString("OrderManager.Orders.Actions.CreateWarehouseReceive", "Create Warehouse Receive");

			warehouseReceiveMenuItem.Caption = menuCaption;
			warehouseReceiveMenuItem.Visible = true;

			ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this);
		}

		#region WarehouseReceive_Click (Create / View)

		void WarehouseReceive_Click(object sender, EventArgs e)
		{
			if (Order.RelatedWarehouseReceive != null)
			{
				ShowForm(Order.RelatedWarehouseReceive);
			}
			else
			{
				CreateWarehouseReceiveFromOrder();
			}
		}

		void CreateWarehouseReceiveFromOrder()
		{
			if (Order.HasChanges)
			{
				var message = Res.GetString("cfb024e7-863f-4b9e-b2b2-7970b83bf2a6", "You must save this Order before creating a Warehouse Receive.");
				Globals.Message.ShowError(message, CreateWarehouseReceiveCaption);
			}
			else
			{
				var publishResult = new OrderModuleToModuleSender().CreateNewEntityFromParent(Order);

				switch (publishResult.ResultType)
				{
					case UniversalResult.Internal:
						var job = publishResult.FindJobIfExists();
						if (job != null)
						{
							ShowForm(job);
						}
						break;
					case UniversalResult.External:
						var message = Res.GetString("25fccb60-ec10-457a-a4a1-77fc90b60d5a", "Universal Shipment queued for sending to Organization [{0}].", Order.Warehouse.OH_Code);
						Globals.Message.ShowInformation(message, CreateWarehouseReceiveCaption);
						break;
					case UniversalResult.HadErrors:
						Globals.Message.ShowError(publishResult.ErrorMessage, CreateWarehouseReceiveCaption);
						break;
					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Publish result type {0} not supported.", publishResult.ResultType.ToString()));
				}
			}
		}

		static string CreateWarehouseReceiveCaption
		{
			get { return Res.GetString("bb16512e-0871-4720-8733-db56c7458f9e", "Create Warehouse Receive"); }
		}

		void ShowForm(BusinessObject relatedWarehouseReceive)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.WhsReceive);

			#region Testing
#if DEBUG
			LastReceiveFormShownForTesting =
#endif
			#endregion

 controller.ShowEditForm(relatedWarehouseReceive);
		}

#if DEBUG
		internal IZForm LastReceiveFormShownForTesting;
#endif

		#endregion

		#endregion

		#endregion

		#region Captions

		public override string FormCaption
		{
			get { return Res.GetString("OrdersForm|FormCaption", "Order") + " " + Order.JD_OrderNumberAndSplit; }
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get { return IsPostOnly ? Res.GetString("OrdersForm|PostButtonText", "S&ave") : string.Empty; }
		}

		#endregion

		#region Delete Confirmation

		protected override DialogResult ShowConfirmationForDelete()
		{
			if (Order.IsShipmentAttached)
			{
				return Globals.Message.Show(Res.GetString("52e558ab-50b1-4c8f-b941-59d8b6664cd5", "This Order is attached to a Shipment. Are you sure you want to delete?"), Res.GetString("b9ce4b3f-0d03-4f78-9410-d555e500502a", "Delete Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
			}
			else
			{
				return base.ShowConfirmationForDelete();
			}
		}

		#endregion

		#region Order Split

		void OnOrderNumberChangeAttemptedWhileSplitsExist(object sender, EventArgs e)
		{
			Order.JD_OrderNumberInfo.RefreshBinding();
			Globals.Message.ShowError(Res.GetString("15c8454c-84b8-4c9c-a79d-2caea8d5fed3", "You cannot change the order number while the order is split"));
		}

		OrderSplitDialogResult ShowConfirmationForSplittingOrder()
		{
			using (var msgBox = new OrderSplitMessageBox(Order.JD_OrderNumber))
			{
				ZFormModaliser.ShowDialogWithoutDispose(msgBox);
				return msgBox.OrderSplitDialogResult;
			}
		}

		void CreateOrder(CreateOrderType splitType)
		{
			((IOrdersModule)ZModuleFactory.Instance.Create(ModuleIDs.Orders)).ShowFormForSplit(Order, splitType);
		}

		public string CanSplitOrder()
		{
			var result = "";
			if (!Order.IsInDatabase || Order.HasChanges)
			{
				result = Res.GetString("f39f2b1d-a3d3-4854-8311-c978fcc68b96", "The order must be saved before splitting can occur");
			}
			else if (!Order.IsNextOrderSplitNumberValid())
			{
				result = Res.GetString("8e105504-3748-4c22-9f35-76b79c4db43c", "The order has been split the maximum number of times. Please select a different action.");
			}

			return result;
		}

		void OnSplitOrder_Click(object sender, EventArgs e)
		{
			string errorMessage = this.CanSplitOrder();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				CreateOrder(CreateOrderType.Split);
			}
		}

		#endregion

		#region On Detaching PreAdvice

		void Order_OnDetachingPreAdviceAskToDetachShipmentAndDeclaration(object sender, CancelEventArgs e)
		{
			if (Order != null && !Order.Factory.IsInTransaction && (Order.IsShipmentAttached || Order.IsDeclarationAttached))
			{
				var caption = Res.GetString("7cb592a6-8b0a-4910-b0fd-2333bd13df50", "Detach confirmation");
				var message = Order.IsShipmentAttached
					? Res.GetString("509c6c24-3733-43b6-8e95-f96d8cf42332", "The order is also attached to Shipment {0}. Do you want to detach the shipment as well?", Order.Shipment.JS_UniqueConsignRef)
					: Res.GetString("80ee952a-3387-4dff-8613-d7b31c1fd0d6", "The order is also attached to Declaration {0}. Do you want to detach the declaration as well?", ((Enterprise.Integration.Customs.IBaseJobDeclaration)Order.Declaration).JE_DeclarationReference);

				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				e.Cancel = dialogResult != DialogResult.Yes;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Order != null)
				{
					Order.OrderNumberChangeAttemptedWhileSplitsExist -= new EventHandler(OnOrderNumberChangeAttemptedWhileSplitsExist);
					Order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration -= new CancelEventHandler(Order_OnDetachingPreAdviceAskToDetachShipmentAndDeclaration);
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Export to XML

		List<MenuItem> ExportToXmlMenuItems
		{
			get
			{
				return new ExportToXmlMenuItemSet<Order>(() => Exporter, (Order)BusinessEntity);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				var exporter = GetNewXmlDataTransferExportor(new OrderValueObjectDataAdapter(), true);
				exporter.DefaultFileName = Order.JD_OrderNumber + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
				if (!(new ZString(SystemDataRegistry.Instance.OrderExportDirectory.Value).IsEmpty))
				{
					exporter.InitialDirectory = SystemDataRegistry.Instance.OrderExportDirectory.Value;
				}
				return exporter;
			}
		}

#if DEBUG
		protected virtual
#endif
 XmlDataTransferExporter GetNewXmlDataTransferExportor(IValueObjectDataAdapter adapter, bool checkForLicence)
		{
			return new XmlDataTransferExporter(adapter, checkForLicence);
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification @event)
		{
			Globals.Message.Show(@event.Message);
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			var msgBoxArgs = e as QueryUserMsgBoxEventArgs;
			if (msgBoxArgs != null)
			{
				msgBoxArgs.Response = Globals.Message.Show(msgBoxArgs.Message, msgBoxArgs.Caption, MessageBoxButtons.YesNo, (msgBoxArgs.Response ? DialogResult.Yes : DialogResult.No)) == DialogResult.Yes;
			}

			var findboxArgs = e as QueryUserFindboxEventArgs;
			if (findboxArgs != null)
			{
				if (findboxArgs.List.TypeOfElements == ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)))
				{
					ZFormModaliser.ShowDialogAndDispose(new PreAdviceDeclarationsListForm(findboxArgs));
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(new OrdersInvoicesListForm(findboxArgs));
				}
			}
		}

		#endregion

		#region Saving

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (Order.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship)
			{
				if (ShowConfirmationForNewSupplierBuyerRelationship() == DialogResult.Yes)
				{
					Order.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
				}
			}

			return base.ShowPreSaveDialogs();
		}

		protected override void SaveInternal()
		{
			base.SaveInternal();

			if (Order.IsOrderPartiallyCompleteAndIsNotAlreadySplit)
			{
				switch (ShowConfirmationForSplittingOrder())
				{
					case OrderSplitDialogResult.SplitOrder:
						CreateOrder(CreateOrderType.Split);
						break;
					case OrderSplitDialogResult.CreateNewOrder:
						CreateOrder(CreateOrderType.New);
						break;
				}
			}
		}

		DialogResult ShowConfirmationForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(Res.GetString("ae292b32-6403-446d-a788-a4ef62b74491", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"), Res.GetString("0aa25125-8fd8-4855-8f70-240d1e400248", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
		}

		#endregion
	}
}
