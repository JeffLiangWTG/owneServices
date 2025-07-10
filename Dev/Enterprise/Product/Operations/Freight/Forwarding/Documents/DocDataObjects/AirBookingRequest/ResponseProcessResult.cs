using CargoWise.Common;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ResponseProcessResult
	{
		public ResponseProcessResult(ResponseType responseType, string message = null, bool importedUniversalShipment = false)
		{
			ResponseType = responseType;
			UserMessage = message;
			ImportedUniversalShipment = importedUniversalShipment;
		}

		ResponseProcessResult(ResponseType responseType, UShipment shipment)
		{
			ResponseType = responseType;
			Shipment = Argument.NotNull(shipment, nameof(shipment));
		}

		public static ResponseProcessResult ResponseWithRates(UShipment shipmentWithRates) => new ResponseProcessResult(ResponseType.Rates, shipmentWithRates);

		public ResponseType ResponseType { get; }

		public string UserMessage { get; }

		public bool ImportedUniversalShipment { get; }

		public UShipment Shipment { get; }
	}
}
