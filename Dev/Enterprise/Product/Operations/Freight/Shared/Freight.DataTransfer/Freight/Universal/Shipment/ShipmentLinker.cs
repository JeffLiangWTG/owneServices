using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class ShipmentLinker<TShipment> : TransportAndContainerParentLinker<TShipment>
		where TShipment : CommonShipment
	{
		internal ShipmentLinker(BusinessObjectFactory factory, IUniversalFreightHelper helper)
			: base(factory, helper)
		{
		}

		internal BusinessObject[] GetLogParent(TShipment shipment, IXmlEventValueObject xmlEvent)
		{
			return GetLogParent(shipment, xmlEvent, shipment.TransportMode == Constants.TransportModes.Air);
		}

		protected override IContainerLinker GetContainerLinker(TShipment shipment)
		{
			return new ShipmentContainerLinker(shipment);
		}
	}
}
