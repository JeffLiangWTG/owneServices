using System;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class ShipmentStatusEventArgs : EventArgs
	{
		public ShipmentStatusEventArgs(ZString status)
		{
			ShipmentStatus = status;
			StatusUpdatedReason = ZString.Empty;
		}

		public ZString ShipmentStatus { get; }

		public ZString StatusUpdatedReason { get; set; }
	}
}
