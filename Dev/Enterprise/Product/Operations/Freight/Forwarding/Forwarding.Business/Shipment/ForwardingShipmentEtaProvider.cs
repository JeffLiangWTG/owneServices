using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class ForwardingShipmentETAProvider : IETAProvider
	{
		public ForwardingShipmentETAProvider(ForwardingShipment shipment)
		{
			Shipment = shipment;
		}

		public IDeliveryDueDateCalculationResult CalculateTimeOfArrivalToAirport()
		{
			if (Shipment == null)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorNoShipment, ETAProviderConstants.ErrorNoShipment);
			}

			if (Shipment.Consols.Count == 0)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorNoConsol, ETAProviderConstants.ErrorNoConsol);
			}

			if (Shipment.Consols.Count > 1)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorMultipleConsols, ETAProviderConstants.ErrorMultipleConsols);
			}

			var consol = Shipment.Consols[0];

			if (consol.JK_AgentType != Core.Constants.AgentType.Direct)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorNotDirectAgent, ETAProviderConstants.ErrorNotDirectAgent);
			}

			var transports = Shipment.Consols.Cast<ForwardingConsol>()
				.SelectMany(consol => consol.Transports.Cast<Transport>()).ToArray();
			var orderedTransports = new TransportOrderHelper(transports);
			var dischargeTransport = orderedTransports.LastOrDefault();

			if (dischargeTransport == default)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorNoMatchingTransportLeg, ETAProviderConstants.ErrorNoMatchingTransportLeg);
			}

			var deliveryDueDate = dischargeTransport.JW_ETA;

			if (deliveryDueDate.IsEmpty)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorETABlank, ETAProviderConstants.ErrorETABlank);
			}

			return DeliveryDueDateCalculationResult.Success(deliveryDueDate, ETAProviderConstants.SuccessETALastLeg);
		}

		ForwardingShipment Shipment { get; set; }
	}
}
