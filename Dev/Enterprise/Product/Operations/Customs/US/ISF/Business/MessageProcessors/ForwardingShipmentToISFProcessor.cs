using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ISF.Business.MessageProcessors
{
	public class ForwardingShipmentToISFProcessor : IProcessor
	{
		public ForwardingShipmentToISFProcessor(ForwardingShipment shipment)
		{
			this.shipment = shipment;
			creator = ISFFromShipmentCreator.GetCreatorForShipment(shipment);
		}

		readonly ForwardingShipment shipment;
		readonly IISFFromShipmentCreator creator;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			using (shipment.SuspendDeclarationForDocuments())
			{
				creator.Create(shipment.Factory);
			}
		}
	}
}
