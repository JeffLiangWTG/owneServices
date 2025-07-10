using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentGatewayInfoDataObjectWriter : DataObjectWriter<ShipmentGateway, GatewayInfo>
	{
		public ShipmentGatewayInfoDataObjectWriter(IDataWritingManager manager)
			: base(manager) { }

		protected override GatewayInfo PopulateDataObject(ShipmentGateway shipmentGateway)
		{
			GatewayInfo gatewayInfo = null;

			if (shipmentGateway.ForwarderAddress != null)
			{
				var addressDO = OrganizationAddressHelper.GetAddressDataObject(shipmentGateway.ForwarderAddress, writeManager, nameof(DocAddressType.None));
				gatewayInfo = new GatewayInfo
				{
					Forwarder = addressDO,
					Order = shipmentGateway.JSG_Sequence
				};
			}

			return gatewayInfo;
		}
	}
}
