using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	interface IAgencyShipmentDataObjectWriter : ITopLevelDataObjectWriter
	{
		void PopulateAgencyShipment(AgencyShipment shipmentBizObj, UniversalShipment dataObject);
	}
}
