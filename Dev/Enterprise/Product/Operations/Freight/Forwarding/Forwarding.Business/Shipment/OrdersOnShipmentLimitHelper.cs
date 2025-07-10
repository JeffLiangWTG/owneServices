using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class OrdersOnShipmentLimitHelper : CollectionLimitHelperForPotentialHVLV
	{
		public OrdersOnShipmentLimitHelper(ForwardingShipment shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		protected override IList Collection
		{
			get { return shipment.GenericOrders; }
		}

		protected override int Limit
		{
			get { return FreightDataRegistry.Instance.OrdersPerShipmentLimit.Value; }
		}

		protected override DateTime LimitIntroductionTimeUtc
		{
			get { return FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.Value; }
		}

		protected override ZDateTime ParentCreationTimeUtc
		{
			get { return shipment.JS_SystemCreateTimeUtc; }
		}

		protected override string ChildPlural
		{
			get { return Res.GetString("b8cd787a-3264-4093-8655-bc45e8da24f0", "Orders"); }
		}

		protected override string ChildSingular
		{
			get { return Res.GetString("67ebee22-2492-4128-b5bb-35584e457117", "Order"); }
		}

		protected override string ParentSingular
		{
			get { return Res.GetString("5ef49c9d-c8f3-4796-b1f3-447dcd013ab1", "Shipment"); }
		}
	}
}
