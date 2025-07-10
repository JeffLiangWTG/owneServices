using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ShipmentTypeChangingCancelEventArgs : CancelEventArgs
	{
		public ShipmentTypeChangingCancelEventArgs(ZString oldShipmentType, ZString newShipmentType)
		{
			OldShipmentType = oldShipmentType;
			NewShipmentType = newShipmentType;
		}

		public ZString OldShipmentType { get; private set; }
		public ZString NewShipmentType { get; private set; }
	}

	public delegate void ShipmentTypeChangingCancelEventHandler(CommonShipment sender, ShipmentTypeChangingCancelEventArgs args);
}
