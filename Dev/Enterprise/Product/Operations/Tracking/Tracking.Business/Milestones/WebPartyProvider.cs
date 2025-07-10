using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public static class WebPartyProvider
	{
		public static WebPartyTypeOrgPairCollection GetWebParties(BusinessObject trackingObject)
		{
			if (trackingObject.TablePrefix == JobShipmentSchema.Constants.Prefix)
			{
				var shipment = trackingObject as ForwardingShipment;
				if (shipment != null && shipment.JS_IsForwardRegistered == true)
				{
					return GetWebParties(shipment);
				}
			}
			return new WebPartyTypeOrgPairCollection();
		}

		static WebPartyTypeOrgPairCollection GetWebParties(ForwardingShipment shipment)
		{
			var webParties = new WebPartyTypeOrgPairCollection();
			webParties.Add(WebPartyType.Shipper, shipment.Consignor);
			webParties.Add(WebPartyType.Consignee, shipment.Consignee);
			if (shipment.Job != null)
			{
				webParties.Add(WebPartyType.LocalClient, shipment.Job.LocalCharges);
			}

			foreach (ForwardingConsol consol in shipment.Consols)
			{
				webParties.Add(WebPartyType.SendingAgent, consol.SendingForwarder);
				webParties.Add(WebPartyType.ReceivingAgent, consol.ReceivingForwarder);
			}

			webParties.Add(WebPartyType.DeliveryAgent, shipment.DeliveryAgent);
			webParties.Add(WebPartyType.ImportBroker, shipment.ImportBroker);
			webParties.Add(WebPartyType.ExportBroker, shipment.ExportBroker);
			return webParties;
		}
	}
}
