using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentOrderManagementForm : ZChildForm
	{
		public ShipmentOrderManagementForm(ForwardingShipment shipment)
			: base(shipment)
		{
			InitializeComponent();
		}

		#region Close

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
