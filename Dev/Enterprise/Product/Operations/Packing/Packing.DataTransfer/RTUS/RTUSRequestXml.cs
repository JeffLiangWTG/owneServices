using CargoWise.Types;
using Enterprise.Packing.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer
{
	public class RTUSRequestXml
	{
		public RTUSRequestXml(ZString packageID, ZGuid packageJobPK, IPackingParent packingParent, UniversalShipment shipmentDataObject)
		{
			PackageID = packageID;
			PackageJobPK = packageJobPK;
			ShipmentDataObject = shipmentDataObject;
			PackingParent = packingParent;
		}

		public ZString PackageID { get; }
		public ZGuid PackageJobPK { get; }
		public UniversalShipment ShipmentDataObject { get; }
		public IPackingParent PackingParent { get; }
	}
}
