using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	public class ChildShipmentsParent
	{
		ChildShipmentsParent(CFSShipment[] shipments, CFSLoadListConsol consol)
		{
			this.Shipments = shipments;
			this.Consol = consol;
		}

		internal CFSLoadListConsol Consol
		{
			get;
			private set;
		}

		internal readonly CFSShipment[] Shipments;

		public static ChildShipmentsParent FromLoadList(CFSLoadListConsol consol)
		{
			return new ChildShipmentsParent(consol.TopLevelShipments.ToArray<CFSShipment>(), consol);
		}

		public static ChildShipmentsParent FromShipment(CFSShipment shipment)
		{
			return new ChildShipmentsParent(shipment.CoLoadShipments.ToArray<CFSShipment>(), null);
		}
	}
}
