using System;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class ProductDeliveryForm : ZChildForm
	{
		public ProductDeliveryForm(JobShipmentPreplanning preAdvice)
			: base(preAdvice)
		{
			OrderLinesGrid.DoubleClick += new EventHandler(OrderLinesGrid_DoubleClick);
			this.zLabel4.Text = Res.GetString("ProductDeliveryForm|Description1", "The Product Delivery Wizard allows you to quickly deliver multiple order lines. You can either...");
		}

		#region Order Line Form

		void OrderLinesGrid_DoubleClick(object sender, EventArgs e)
		{
			if (OrderLinesGrid.ListManager.Position > -1)
			{
				OrderDeliveryLine line = (OrderDeliveryLine)OrderLinesGrid.ListManager.GetCurrent();
				if (line.Line != null)
				{
					ZControllerFactory.Create(ControllerIDs.OrderLineFromOrder).ShowEditForm(line.Line);
				}
				else
				{
					Globals.Message.Show(Res.GetString("40afddde-8b8e-4687-9c41-5a646a59c9c4", "The line you have selected does not match a line that exists on the orders on this Pre Advice. Please fill in a valid Order Number and Order Line Number."));
				}
			}
		}

		#endregion

		#region Receive All

		void ReceiveAllButton_Click(object sender, EventArgs e)
		{
			foreach (OrderDeliveryLine line in PreAdvice.OrderLines)
			{
				line.ReceiveAllIfNonReceived();
			}
			OrderLinesGrid.Invalidate();
		}

		#endregion

		#region Implementation

		public override string FormCaption
		{
			get { return Res.GetString("8397ccd2-3cc6-4d2d-9917-cc6d265cdd3d", "Product Delivery Wizard"); }
		}

		public JobShipmentPreplanning PreAdvice
		{
			get { return (JobShipmentPreplanning)base.DataSource; }
		}

		#endregion

		#region Close

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			PreAdvice.OrderLines.RemoveAll();
			foreach (ZBoolDescriptionPair pair in PreAdvice.OrderNumberList)
			{
				pair.Value = false;
			}

			base.OnClosed(e);
		}

		#endregion

		#region Custom Labels

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (GridLayoutPersister != null)
			{
				GridLayoutPersister.Dispose();
			}
			base.SetDataBinding(dataSource, dataMember);
			if (PreAdvice != null)
			{
				if (PreAdvice.Orders.Count > 0)
				{
					GridLayoutPersister = new CustomLabelsGridLayoutPersister(this.OrderLinesGrid, OrderLine.NewCustomLabelsProvider(PreAdvice.Orders[0]), "Line+");
				}
				foreach (ZBoolDescriptionPair pair in PreAdvice.OrderNumberList)
				{
					pair.Value = true;
				}
			}
		}

		CustomLabelsGridLayoutPersister GridLayoutPersister;

		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (GridLayoutPersister != null)
				{
					GridLayoutPersister.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
