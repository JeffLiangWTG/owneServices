using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentDocManagerInfo : ShipmentDocManagerInfo
	{
		public CFSShipmentDocManagerInfo(CommonShipment parent)
			: base(parent, Constants.DocManagerCodes.CFSShipmentReceival)
		{ }

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = base.GetRelatedObjects().ToList();

			result.AddRange(Shipment.DeliveryConfirms);
			result.AddRange(Shipment.DestinationCFSArrivals);
			result.AddRange(Shipment.DestinationCFSDepartures);

			result.AddRange(Shipment.PickupConfirms);
			result.AddRange(Shipment.OriginCFSArrivals);
			result.AddRange(Shipment.OriginCFSDepartures);

			return result.ToArray();
		}

		CFSShipment Shipment
		{
			get { return (CFSShipment)BusinessEntity; }
		}
	}
}
