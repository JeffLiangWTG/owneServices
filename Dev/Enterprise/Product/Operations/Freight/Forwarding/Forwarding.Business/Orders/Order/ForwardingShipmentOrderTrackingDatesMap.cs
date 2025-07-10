using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	sealed class ForwardingShipmentOrderTrackingDatesMap : OrderTrackingDatesMap
	{
		public ForwardingShipmentOrderTrackingDatesMap(ForwardingShipment shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public override ZDateTime DepartureActualDate
		{
			get
			{
				var transport = shipment.TransportsIncludingRelated.DepartureTransport;
				return transport != null ? transport.JW_ATD : ZDateTime.Empty;
			}
		}

		public override ZDateTime ArrivalActualDate
		{
			get
			{
				var transport = shipment.TransportsIncludingRelated.ArrivalTransport;
				return transport != null ? transport.JW_ATA : ZDateTime.Empty;
			}
		}

		public override ZDateTime CargoAvailableActualDate
		{
			get
			{
				var cargoAvailableActualDate = ZDateTime.Empty;

				if (!shipment.DocsAndCartage.JP_FCLAvailable.IsEmpty
						&& (shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL
						|| shipment.JS_PackingMode == Core.Constants.ContainerModes.BuyersConsol))
				{
					cargoAvailableActualDate = shipment.DocsAndCartage.JP_FCLAvailable;
				}
				else if (!shipment.DocsAndCartage.JP_LCLAvailable.IsEmpty)
				{
					cargoAvailableActualDate = shipment.DocsAndCartage.JP_LCLAvailable;
				}

				return cargoAvailableActualDate;
			}
		}

		public override ZDateTime DeliveryCartageAdvisedActualDate
		{
			get { return shipment.DocsAndCartage.JP_DeliveryCartageAdvised; }
		}

		public override ZDateTime DeliveryCartageCompleteFinalizedActualDate
		{
			get { return shipment.DocsAndCartage.JP_DeliveryCartageCompleted; }
		}

		public override ZDateTime DepartureScheduledDate
		{
			get
			{
				var transport = shipment.TransportsIncludingRelated.DepartureTransport;
				return transport != null ? transport.JW_ETD : shipment.JS_E_DEP;
			}
		}

		public override ZDateTime ArrivalScheduledDate
		{
			get
			{
				var transport = shipment.TransportsIncludingRelated.ArrivalTransport;
				return transport != null ? transport.JW_ETA : shipment.JS_E_ARV;
			}
		}

		public override ZDateTime DeliveryCartageCompleteFinalizedScheduledDate
		{
			get { return shipment.DocsAndCartage.JP_EstimatedDelivery; }
		}
	}
}
