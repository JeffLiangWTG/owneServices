using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersAttachUserControl : ZUserControl
	{
		public OrdersAttachUserControl()
		{
			InitializeComponent();

			OrdersButtonGrid.OnAttach += new EventHandler<ModuleButtonGridOnAttachEventArgs>(OnOrdersAttach);
		}

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.AttachedOrders.CountChanged -= new EventHandler(AttachedOrders_CountChanged);
				if (CurrentDataItem is ForwardingShipment)
				{
					((ForwardingShipment)CurrentDataItem).OnOrderLineToPackLineConversion -= new EventHandler<OrderLineToPackLineConversionEventArgs>(OrderLineToPackLineConversion);
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.AttachedOrders.CountChanged += new EventHandler(AttachedOrders_CountChanged);
				AttachedOrders_CountChanged(this, null);
			}

			if (CurrentDataItem is ForwardingShipment)
			{
				((ForwardingShipment)CurrentDataItem).OnOrderLineToPackLineConversion -= new EventHandler<OrderLineToPackLineConversionEventArgs>(OrderLineToPackLineConversion);
				((ForwardingShipment)CurrentDataItem).OnOrderLineToPackLineConversion += new EventHandler<OrderLineToPackLineConversionEventArgs>(OrderLineToPackLineConversion);
			}
		}

		public new IAttachOrders CurrentDataItem
		{
			get { return (IAttachOrders)base.CurrentDataItem; }
		}

		#endregion

		#region Edit Button

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public bool ShowEditButton
		{
			get { return OrdersButtonGrid.ShowEditButton; }
			set { OrdersButtonGrid.ShowEditButton = value; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CurrentDataItem != null)
				{
					CurrentDataItem.AttachedOrders.CountChanged -= new EventHandler(AttachedOrders_CountChanged);
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		void AttachedOrders_CountChanged(object sender, EventArgs e)
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
			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				List<Order> attachedOrders = new List<Order>();
				foreach (BusinessObject businessObject in eventArgs.AttachedBusinessObjects)
				{
					Order attachedOrder = businessObject as Order;
					if (attachedOrder != null)
					{
						attachedOrders.Add(attachedOrder);
					}
				}

				shipment.CreatePackLinesFromOrderLines(attachedOrders.ToArray());
			}
		}

		void OrderLineToPackLineConversion(object sender, OrderLineToPackLineConversionEventArgs e)
		{
			var dialogCaption = Res.GetString("158BE79B-DC9F-46f1-8E50-7421ACB62A48", "Link order lines to pack-lines");

			if (string.IsNullOrEmpty(e.ReasonForNotAbleToConvert))
			{
				var dialogMessage = Res.GetString("43E6F96A-2620-43aa-AF2E-48C17C38ACB9", "Do you want to link the order lines from the recently attached orders to pack-lines on this shipment?");

				var dialogResult = Globals.Message.Show(dialogMessage, dialogCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				if (dialogResult == DialogResult.Yes)
				{
					e.ShouldCreatePacklines = true;
					ZFormModaliser.ShowDialogAndDispose(new OrderLineToPackLineConversionForm(e.Helper));
				}
			}
			else
			{
				Globals.Message.Show(e.ReasonForNotAbleToConvert, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		#endregion
	}
}
