using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class GenericOrderManagementControl : ZUserControl
	{
		public GenericOrderManagementControl()
		{
			InitializeComponent();
		}

		#region Bind

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				ShipmentDomainService.GetInstance(Shipment.Factory).AttachRelatedOrderRequest -= new EventHandler<AttachRelatedOrderRequestEventArgs>(Shipment_AttachRelatedOrderRequest);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				ShipmentDomainService.GetInstance(Shipment.Factory).AttachRelatedOrderRequest += new EventHandler<AttachRelatedOrderRequestEventArgs>(Shipment_AttachRelatedOrderRequest);
			}
		}

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)CurrentDataItem; }
		}

		#endregion

		#region Related Orders

		void Shipment_AttachRelatedOrderRequest(object sender, EventArgs e)
		{
			if (!IsDisposed && !Disposing)
			{
				SelectOrdersForm.ShowForm(Shipment);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (Shipment != null)
				{
					ShipmentDomainService.GetInstance(Shipment.Factory).AttachRelatedOrderRequest -= new EventHandler<AttachRelatedOrderRequestEventArgs>(Shipment_AttachRelatedOrderRequest);
				}
			}

			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
