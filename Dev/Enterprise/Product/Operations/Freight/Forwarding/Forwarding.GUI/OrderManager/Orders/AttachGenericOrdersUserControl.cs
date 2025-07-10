using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class AttachGenericOrdersUserControl : ZUserControl
	{
		public AttachGenericOrdersUserControl()
		{
			InitializeComponent();

			GenericOrdersButtonGrid.OnAttach += new EventHandler<ModuleButtonGridOnAttachEventArgs>(OnOrdersAttach);
		}

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.GenericOrders.CountChanged -= AttachedOrders_CountChanged;

				var shipment = CurrentDataItem as ForwardingShipment;
				if (shipment != null)
				{
					shipment.OnOrderLineToPackLineConversion -= new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.GenericOrders.CountChanged += AttachedOrders_CountChanged;
				AttachedOrders_CountChanged(this, null);
			}

			var shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				shipment.OnOrderLineToPackLineConversion -= new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);
				shipment.OnOrderLineToPackLineConversion += new EventHandler<OrderLineToPackLineConversionEventArgs>(AttachOrderToShipmentHelper.OrderLineToPackLineConversion);
			}
		}

		public new IAttachGenericOrders CurrentDataItem
		{
			get { return (IAttachGenericOrders)base.CurrentDataItem; }
		}

		#endregion

		#region Edit Button

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public bool ShowEditButton
		{
			get { return GenericOrdersButtonGrid.ShowEditButton; }
			set { GenericOrdersButtonGrid.ShowEditButton = value; }
		}

		#endregion

		#region Events

		void AttachedOrders_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.DocsAndCartage.JP_OrderItemsAsStringInfo.RefreshBinding();
			}
		}

		void OrderReferencesButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				OrderItemCollectionForm.ShowDialog(CurrentDataItem.DocsAndCartage.OrderItems);
			}
		}

		void OnOrdersAttach(object sender, ModuleButtonGridOnAttachEventArgs eventArgs)
		{
			var shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				var attachedOrders = new List<Order>();
				var attachedWarehouseOrders = new List<BusinessObject>();
				foreach (var businessObject in eventArgs.AttachedBusinessObjects)
				{
					var attachedOrder = businessObject as Order;
					if (attachedOrder != null)
					{
						attachedOrders.Add(attachedOrder);

						attachedOrder.OnPopulateShipment -= AttachedOrder_OnPopulateShipment;
						attachedOrder.OnPopulateShipment += AttachedOrder_OnPopulateShipment;
					}
					else if (businessObject is IWhsOrder)
					{
						attachedWarehouseOrders.Add(businessObject);
					}
				}

				if (!shipment.IsMasterShipmentRepresentingAllChildShipments)
				{
					foreach (var warehouseOrder in attachedWarehouseOrders)
					{
						ForwardingShipmentToWarehouseOrderSender.UpdatePackLinesFromWarehouseOrder(shipment, warehouseOrder);
					}
				}

				shipment.CreatePackLinesFromOrderLines(attachedOrders.ToArray());
			}
		}

		void AttachedOrder_OnPopulateShipment(object sender, PopulateShipmentEventArgs e)
		{
			var dialogCaption = ResString.GetMultilingualString("158BE79B-DC9F-46f1-8E50-7421ACB62A48", "Link order lines to pack-lines");

			var dialogContext = new DialogDefaultContext(
				new ZGuid("b23960fd-4530-4d6f-8bd6-86c6098b7396"),
				dialogCaption,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);

			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, e.Message);
			if (dialogResult == ZDialogResult.No)
			{
				e.ShouldPopulateShipment = false;
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CurrentDataItem != null)
				{
					CurrentDataItem.GenericOrders.CountChanged -= AttachedOrders_CountChanged;
				}
			}
			base.Dispose(disposing);
		}
	}
}
