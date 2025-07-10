using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class GatewayShipmentRateLineConditionsSupporter : ShipmentRateLineConditionsSupporter
	{
		readonly CommonConsol gatewayConsol;

		public GatewayShipmentRateLineConditionsSupporter(CommonConsol gatewayConsol, CommonShipment shipment)
			: base(shipment)
		{
			Argument.NotNull(gatewayConsol, "gatewayConsol");
			this.gatewayConsol = gatewayConsol;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return gatewayConsol.SendingForwarder;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return gatewayConsol.ReceivingForwarder;
		}
	}
}
