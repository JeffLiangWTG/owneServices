using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class JobShipmentPreplanningForm : ZTemplateForm, INotifications, INotificationSubscriberQueryUser
	{
		protected JobShipmentPreplanningForm()
		{
			InitializeComponent();
		}

		public JobShipmentPreplanningForm(JobShipmentPreplanning preAdvice)
			: base(preAdvice)
		{
			InitializeComponent();
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 1);
			SetupActionsMenu();
			WorkflowTabPage.Initialize(preAdvice);

			DeliveryWizardButton.AllowOverlap(OrdersGrid);
		}

		public new JobShipmentPreplanning DataSource
		{
			get { return (JobShipmentPreplanning)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.OnAfterShipmentsCreated -= ManualShipmentNumberEntry.ShipmentNumber_OnAfterShipmentsCreated;
				DataSource.OnOrderLineToPackLineConversion -= PreAdvice_OrderLineToPackLineConversion;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				DataSource.OnAfterShipmentsCreated += ManualShipmentNumberEntry.ShipmentNumber_OnAfterShipmentsCreated;
				DataSource.OnOrderLineToPackLineConversion -= PreAdvice_OrderLineToPackLineConversion;
				DataSource.OnOrderLineToPackLineConversion += PreAdvice_OrderLineToPackLineConversion;
				DataSource.EF_JSInfo.ValueChanged += new EventHandler(CreatePackLinesFromOrderLines);
			}
		}

		void CreatePackLinesFromOrderLines(object sender, EventArgs e)
		{
			var shipment = DataSource.Shipment;

			if (shipment != null)
			{
				shipment.OnOrderLineToPackLineConversion -= new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);
				shipment.OnOrderLineToPackLineConversion += new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);
				shipment.CreatePackLinesFromOrderLines(DataSource.Orders);
			}
		}

		#region Actions Menu

		void SetupActionsMenu()
		{
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.ShipmentPreAdvice.Actions.CreateDeclaration", "Create Declaration"), CreateDec_Click));

			var createShipmentAndAttachMenuItem = new ZMenuItem(ResString.GetMultilingualString("OrderManager.ShipmentPreAdvice.Actions.CreateShipmentAndAttach", "Create Shipment and attach to Consol"));
			ActionsMenuItem.MenuItems.Add(createShipmentAndAttachMenuItem);
			createShipmentAndAttachMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("52392ccd-6607-4c29-95f4-2c99be97d7b5", "Single House Bill"), CreateOneShipment_ConsolAttach_Click));
			createShipmentAndAttachMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e4b35cd1-7dbc-450b-a494-4e1069b3ef31", "House Bill per Buyer/Supplier"), CreateMultipleShipmentsBuyerSupplier_ConsolAttach_Click));
			createShipmentAndAttachMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("826d40e6-26ac-49c4-a9c7-a93e4a128568", "House Bill per Order"), CreateMultipleShipmentsOrder_ConsolAttach_Click));

			var createHousebillMenuItem = new ZMenuItem(ResString.GetMultilingualString("OrderManager.ShipmentPreAdvice.Actions.CreateHouseBill", "Create Consol/Shipment/Declaration"));
			ActionsMenuItem.MenuItems.Add(createHousebillMenuItem);
			createHousebillMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.ShipmentPreAdvice.Actions.CreateSingleHouseBill", "Single House Bill"), CreateOneShipment_Click));
			createHousebillMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("A3A2B7EB-0266-489E-BA1B-E6AAEEC8EE78", "House Bill per Buyer/Supplier"), CreateMultipleShipmentsBuyerSupplier_Click));
			createHousebillMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("718D3FDC-6F16-4861-9F7C-DDB0CE929BB3", "House Bill per Order"), CreateMultipleShipmentsOrder_Click));

			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.ShipmentPreAdvice.Actions.AddCommercialInvoice", "Add Commercial Invoice to Declaration"), AddCommInvoice_Click));

			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItems);

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		List<MenuItem> ExportToXmlMenuItems
		{
			get
			{
				return new ExportToXmlMenuItemSet<JobShipmentPreplanning>(() => Exporter, DataSource);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				return new XmlDataTransferExporter(new ShipmentPreAdviceValueObjectDataAdapter(), true);
			}
		}

		void CreateDec_Click(object sender, EventArgs args)
		{
			DataSource.CreateStandAloneBrokerage(this);
		}

		void CreateOneShipment_Click(object sender, EventArgs args)
		{
			CreateShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment);
		}

		void CreateMultipleShipmentsBuyerSupplier_Click(object sender, EventArgs args)
		{
			CreateShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier);
		}

		void CreateMultipleShipmentsOrder_Click(object sender, EventArgs args)
		{
			CreateShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerOrder);
		}

		void CreateOneShipment_ConsolAttach_Click(object sender, EventArgs args)
		{
			var consol = DataSource.GetConsolidationToAttachShipmentTo(this);
			if (consol != null)
			{
				var errors = DataSource.GetConsolMatchingErrors(consol);
				if (errors.IsNullOrEmpty())
				{
					CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, consol);
				}
				else
				{
					Globals.Message.ShowWarning(errors);
					CreateOneShipment_ConsolAttach_Click(sender, args);
				}
			}
		}

		void CreateMultipleShipmentsBuyerSupplier_ConsolAttach_Click(object sender, EventArgs args)
		{
			var consol = DataSource.GetConsolidationToAttachShipmentTo(this);
			if (consol != null)
			{
				var errors = DataSource.GetConsolMatchingErrors(consol);
				if (errors.IsNullOrEmpty())
				{
					CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, consol);
				}
				else
				{
					Globals.Message.ShowWarning(errors);
					CreateMultipleShipmentsBuyerSupplier_ConsolAttach_Click(sender, args);
				}
			}
		}

		void CreateMultipleShipmentsOrder_ConsolAttach_Click(object sender, EventArgs args)
		{
			var consol = DataSource.GetConsolidationToAttachShipmentTo(this);
			if (consol != null)
			{
				var errors = DataSource.GetConsolMatchingErrors(consol);
				if (errors.IsNullOrEmpty())
				{
					CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerOrder, consol);
				}
				else
				{
					Globals.Message.ShowWarning(errors);
					CreateMultipleShipmentsOrder_ConsolAttach_Click(sender, args);
				}
			}
		}

		void AddCommInvoice_Click(object sender, EventArgs args)
		{
			DataSource.AppendInvoiceLineToCustomsDeclaration(this);
		}

		void PreAdvice_OrderLineToPackLineConversion(object sender, OrderLineToPackLineConversionEventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new OrderLineToPackLineConversionForm(e.Helper));
		}

		#endregion

		#region Implmentation

		void CreateShipment(JobShipmentPreplanning.OrderShipmentCreationMode creationMode)
		{
			if (!DataSource.Validation.HasAirConsolMatchesWithSameMAWB)
			{
				if (DataSource.MatchingPreAdvices.Count > 1)
				{
					ZFormModaliser.Show(new PreAdviceExportToForwardingJobForm(DataSource, creationMode, this), this);
				}
				else
				{
					DataSource.CreateConsolAndShipment(creationMode, this, PreAdviceConversionHelper.ShowFormWithNewlyCreatedConsol());
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("4a10eec2-32d7-44d0-94d7-09825fb5b670", "An air consol already exists for this Master Bill. Same Master Bill can be used only for one air consol."));
			}
		}

		void CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode creationMode, ForwardingConsol consol)
		{
			if (!DataSource.Validation.HasAirConsolMatchesWithSameMAWB)
			{
				if (DataSource.MatchingPreAdvices.Count > 1)
				{
					ZFormModaliser.Show(new PreAdviceExportToForwardingJobForm(DataSource, creationMode, this), this);
				}
				else
				{
					DataSource.CreateShipmentAndAttachToConsol(creationMode, this, consol, PreAdviceConversionHelper.ShowFormWithEditedConsol());
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("4a10eec2-32d7-44d0-94d7-09825fb5b670", "An air consol already exists for this Master Bill. Same Master Bill can be used only for one air consol."));
			}
		}

		#endregion

		#region Delivery Wizard

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			OrdersGrid.InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			OrdersGrid.InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("OrderManager.ShipmentPreAdvice.ProductDeliveryWizard", "Product Delivery Wizard"), DeliveryWizard_Click));
		}

		void DeliveryWizard_Click(object sender, EventArgs e)
		{
			ProductDeliveryForm form = new ProductDeliveryForm(DataSource);
			ZFormModaliser.Show(form, this);
		}

		void DeliveryWizardButton_Click(object sender, EventArgs e)
		{
			DeliveryWizard_Click(sender, e);
		}

		#endregion

		#region Caption

		public override string FormCaption
		{
			get { return Res.GetString("def6f166-9038-4466-ad66-a6bc21b36197", "Pre Advice") + " " + DataSource.EF_PreshipID; }
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification @event)
		{
			Globals.Message.Show(@event.Message);
		}

		#endregion

		#region INotificationSubscriberQueryUser Members

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			QueryUserMsgBoxEventArgs msgBoxArgs = e as QueryUserMsgBoxEventArgs;
			if (msgBoxArgs != null)
			{
				msgBoxArgs.Response = Globals.Message.Show(msgBoxArgs.Message, msgBoxArgs.Caption, MessageBoxButtons.YesNo, (msgBoxArgs.Response ? DialogResult.Yes : DialogResult.No)) == DialogResult.Yes;
			}

			QueryUserFindboxEventArgs findboxArgs = e as QueryUserFindboxEventArgs;
			if (findboxArgs != null)
			{
				if (findboxArgs.List.TypeOfElements == typeof(ForwardingConsol))
				{
					ZFormModaliser.ShowDialogAndDispose(new PreAdviceConsolsListForm(findboxArgs));
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(new PreAdviceDeclarationsListForm(findboxArgs));
				}
			}
		}

		#endregion

		#region Saving

		protected override void SaveInternal()
		{
			base.SaveInternal();

			var ordersCanBeSplitted = DataSource.Orders.Where(order => order.IsOrderPartiallyCompleteAndIsNotAlreadySplit);

			if (ordersCanBeSplitted.Any())
			{
				var splitType = ShowConfirmationForSplittingOrder(ordersCanBeSplitted);
				ordersCanBeSplitted.ForEach(order => order.SplitOrder(splitType));
			}
		}

		OrderSplitDialogResult ShowConfirmationForSplittingOrder(IEnumerable<Order> ordersCanBeSplitted)
		{
			using (var msgBox = new OrderSplitMessageBox(ordersCanBeSplitted.Select(order => order.JD_OrderNumber).ToArray()))
			{
				ZFormModaliser.ShowDialogWithoutDispose(msgBox);
				return msgBox.OrderSplitDialogResult;
			}
		}

		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (DataSource != null)
				{
					DataSource.OnAfterShipmentsCreated -= ManualShipmentNumberEntry.ShipmentNumber_OnAfterShipmentsCreated;
				}
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
