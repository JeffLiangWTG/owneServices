using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class ForwardingShipmentDeliveryOrderProvider : IDeliveryOrderProvider
	{
		internal ForwardingShipmentDeliveryOrderProvider(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
		}

		public EnterpriseBusinessObject BusinessObject => shipment;

		public ZString MessageReferenceNumber { get => shipment.JS_DLO; set => shipment.JS_DLO = value; }

		public BusinessObjectFactory Factory => shipment.Factory;

		public EDIMessageCollection Messages => shipment.Messages;

		readonly ForwardingShipment shipment;
	}
}
