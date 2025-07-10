using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class PortMessagingForwardingHelper : Integration.Forwarding.IPortMessagingForwardingHelper
	{
		public PortMessagingForwardingHelper()
		{
		}

		public void SyncroniseMRN(BusinessObjectFactory factory, ZGuid shipmentPK, ZString oldMRNValue, ZString newMRNValue)
		{
			if (!newMRNValue.IsEmpty)
			{
				var shipment = factory.Load<ForwardingShipment>(shipmentPK);
				if (shipment != null)
				{
					var portMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
					if (portMessaging.JSM_MovementReferenceNumber.IsEmpty || portMessaging.JSM_MovementReferenceNumber == oldMRNValue)
					{
						portMessaging.JSM_MovementReferenceNumber = newMRNValue;
					}
				}
			}
		}

		public void OnPacklineDelete(BusinessObjectFactory factory, ZGuid packlinePK)
		{
			if (factory != null && packlinePK.IsValid)
			{
				var query = new ZQuery(JobPackLinePortMessagingSchema.JLM_JL_PackLine, packlinePK);
				var portMessaging = factory.LoadTop1<PackLinePortMessaging>(query);
				if (portMessaging != null && !portMessaging.IsDeleted)
				{
					portMessaging.Delete();
				}
			}
		}
	}
}
