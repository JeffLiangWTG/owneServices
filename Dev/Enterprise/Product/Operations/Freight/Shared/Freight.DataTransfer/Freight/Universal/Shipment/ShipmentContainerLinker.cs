using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ShipmentContainerLinker : IContainerLinker
	{
		public ShipmentContainerLinker(CommonShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
		}

		readonly CommonShipment shipment;

		public CommonContainer[] GetLogParent(IXmlEventValueObject xmlEvent)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");
			var eventContainerNumbers = xmlEvent.Context.ContainerNumbers;
			var containerIDs = !xmlEvent.Context.MAWBNumber.IsEmpty
				? xmlEvent.Context.ULDIdentifications
				: eventContainerNumbers != null && eventContainerNumbers.Any() ? xmlEvent.Context.ContainerNumbers : null;

			if (containerIDs != null && containerIDs.Any())
			{
				return shipment.Containers.Where(c => containerIDs.Contains(c.JC_ContainerNum)).ToArray();
			}

			return null;
		}
	}
}
