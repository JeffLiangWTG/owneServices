using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.DataTransfer
{
	public static class ShipmentLocatorExtensions
	{
		public static T Find<T>(this ShipmentLocator<T> locator, Shipment shipmentValue) where T : CommonShipment
		{
			var agentReference = shipmentValue.ShipmentDetails.AgentReference;

			var houseIdentifier = shipmentValue.ShipmentIdentifier.FindFirst(ShipmentIdentifierType.Housebill);
			ZString houseBill = houseIdentifier != null ? houseIdentifier.Value : ZString.Empty;

			ZString origin = shipmentValue.ShipmentDetails.PortOfOriginSpecified && shipmentValue.ShipmentDetails.PortOfOrigin.PortSpecified ? shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value : ZString.Empty;
			ZString destination = shipmentValue.ShipmentDetails.PortofDestinationSpecified && shipmentValue.ShipmentDetails.PortofDestination.PortSpecified ? shipmentValue.ShipmentDetails.PortofDestination.Port.Value : ZString.Empty;
			ZDateTime etd = shipmentValue.ShipmentDetails.PortOfOriginSpecified && shipmentValue.ShipmentDetails.PortOfOrigin.EstimatedDateTimeSpecified ? shipmentValue.ShipmentDetails.PortOfOrigin.EstimatedDateTime : ZDateTime.Empty;

			ZString[] otherAgentReferences = shipmentValue.ShipmentDetails.ReferenceNumbers.Cast<ReferenceNumber>()
				.Where(refNumber => refNumber.Type == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference && !refNumber.Number.IsEmpty)
				.Select(refNumber => refNumber.Number)
				.ToArray();

			return locator.Find(agentReference, houseBill, otherAgentReferences, etd, origin, destination);
		}
	}
}
