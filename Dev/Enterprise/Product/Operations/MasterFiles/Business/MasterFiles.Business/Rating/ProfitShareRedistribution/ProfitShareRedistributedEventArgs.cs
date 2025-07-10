using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ProfitShareRedistributedEventArgs : CancelEventArgs
	{
		public ProfitShareRedistributedEventArgs(ZGuid shipmentPK)
		{
			ShipmentPK = shipmentPK;
		}

		public ZGuid ShipmentPK { get; }
	}
}