using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FromTransportCollection : MovementOrderHelperTest
	{
		#region Implementation

		protected override Transport NewLeg(CommonShipment shipment, ZString load, ZString discharge, byte order)
		{
			Transport transport = base.NewLeg(shipment, load, discharge, order);
			transport.JW_LegOrder = order;
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;
			return transport;
		}

		protected override TransportOrderHelper NewOrderHelper(CommonShipment shipment)
		{
			return new TransportOrderHelper(shipment.Transports);
		}

		#endregion
	}
}
