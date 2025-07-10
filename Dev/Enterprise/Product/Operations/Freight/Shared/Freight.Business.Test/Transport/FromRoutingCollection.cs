using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FromRoutingCollection : MovementOrderHelperTest
	{
		#region Implementation

		protected override Transport NewLeg(CommonShipment shipment, ZString load, ZString discharge, byte order)
		{
			Transport transport = base.NewLeg(shipment, load, discharge, order);
			transport.JW_LegOrder = 0;
			transport.JW_ETD = ZDateTime.Now.AddDays(order);
			transport.JW_ETA = transport.JW_ETD.AddHours(6);
			return transport;
		}

		protected override TransportOrderHelper NewOrderHelper(CommonShipment shipment)
		{
			return new TransportOrderHelper(shipment.TransportsIncludingRelated);
		}

		#endregion
	}
}
