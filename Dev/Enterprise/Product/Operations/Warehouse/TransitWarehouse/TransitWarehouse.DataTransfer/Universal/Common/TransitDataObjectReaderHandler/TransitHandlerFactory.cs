using System;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitDataObjectReaderHandlerManager;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	class TransitHandlerFactory
	{
		public ITransitDataObjectReaderHandler BuildHandler(HandlerType handlerType, UniversalShipment dataObject)
		{
			switch (handlerType)
			{
				case HandlerType.Consol:
					return new TransitReceiveConsolHandler();
				case HandlerType.Container:
					return new TransitReceiveContainerHandler(dataObject);
				case HandlerType.Vehicle:
					return new TransitReceiveVehicleHandler(dataObject);
				case HandlerType.StoredProcedure:
					return new TransitStoredProcedureHandler();
				default:
					throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null);
			}
		}
	}
}
