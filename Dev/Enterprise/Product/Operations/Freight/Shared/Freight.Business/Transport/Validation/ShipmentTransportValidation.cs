using CargoWise.ComponentModel;

namespace Enterprise.Freight.Business
{
	public class ShipmentTransportValidation : JobConsolTransportValidation
	{
		public ShipmentTransportValidation(Transport transport)
			: base(transport) { }

		protected override void CheckJW_RL_NKLoadPort()
		{
			base.CheckJW_RL_NKLoadPort();

			if (!Parent.JW_RL_NKLoadPortInfo.HasErrors())
			{
				CommonShipment shipment = (CommonShipment)Parent.Parent;
				if (shipment != null && RoutingLegLoadsAtPort(shipment.JS_RL_NKDestination))
				{
					if (!Parent.IsRailOrRoadOrInlandWaterway || (Parent.JW_RL_NKLoadPort != Parent.JW_RL_NKDiscPort))
					{
						Parent.JW_RL_NKLoadPortInfo.AddError(Res.GetString("cc0abe79-8a3f-4570-9983-297ee4cbbb35", "A routing leg cannot load at the shipment's destination."));
					}
				}
			}
		}

		protected override void CheckJW_RL_NKDiscPort()
		{
			base.CheckJW_RL_NKDiscPort();

			if (!Parent.JW_RL_NKDiscPortInfo.HasErrors())
			{
				CommonShipment shipment = (CommonShipment)Parent.Parent;
				if (shipment != null && RoutingLegDischargesAtPort(shipment.JS_RL_NKOrigin))
				{
					if (!Parent.IsRailOrRoadOrInlandWaterway || (Parent.JW_RL_NKLoadPort != Parent.JW_RL_NKDiscPort))
					{
						Parent.JW_RL_NKDiscPortInfo.AddError(Res.GetString("1035e726-cb98-4bd4-b60e-39b60ef79ca4", "A routing leg cannot discharge at the shipment's origin."));
					}
				}
			}
		}
	}
}
