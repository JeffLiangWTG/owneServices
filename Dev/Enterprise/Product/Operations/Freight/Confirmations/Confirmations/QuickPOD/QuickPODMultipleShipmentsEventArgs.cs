using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Confirmations.Business
{
	public delegate void QuickPODMultipleShipmentsEventHandler(object sender, QuickPODMultipleShipmentsEventArgs e);

	public class QuickPODMultipleShipmentsEventArgs : EventArgs
	{
		public QuickPODMultipleShipmentsEventArgs(ZString houseBill, ShipmentCollection shipments)
		{
			this.Shipments = shipments;
			this.houseBill = houseBill;
			SelectedShipment = shipments[0];
		}

		public readonly ShipmentCollection Shipments;
		public CommonShipment SelectedShipment;

		public ZString HouseBill
		{
			get { return houseBill; }
		}
		readonly ZString houseBill;
	}
}
