using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentGatewayInfoCollectionReader : DataObjectCollectionReader<GatewayInfo, ShipmentGateway>
	{
		readonly ForwardingShipment shipment;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		public ShipmentGatewayInfoCollectionReader(GatewayInfo[] gatewayInfoDataObjects, ForwardingShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(gatewayInfoDataObjects)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.logger = logger;
			this.factory = factory;
		}

		protected override void AddToCollection(ShipmentGateway shipmentGateway)
		{
			shipment.Gateways.Add(shipmentGateway);
		}

		protected override void RemoveFromCollection(ShipmentGateway shipmentGateway)
		{
			shipment.Gateways.RemoveFromRelationship(shipmentGateway);
			shipmentGateway.Delete();
		}

		protected override ShipmentGateway[] BusinessObjects => shipment.Gateways.ToArray();

		protected override ShipmentGateway ReadIntoBusinessObject(GatewayInfo gatewayInfo, ShipmentGateway shipmentGateway)
		{
			var orgAddress = gatewayInfoAddressMap.GetValueSafe(gatewayInfo);

			if (orgAddress == null)
			{
				logger.Log(Enterprise.Integration.LogType.Information,
					string.Format(CultureInfo.InvariantCulture,
					"Failed to find OrgAddress {0} matching sequence number {1}",
					gatewayInfo.Forwarder?.CompanyName ?? string.Empty,
					gatewayInfo.Order));

				return null;
			}

			shipmentGateway = shipmentGateway ?? factory.New<ShipmentGateway>();
			shipmentGateway.JSG_OA_ForwarderAddress = orgAddress.PK;
			shipmentGateway.JSG_Sequence = gatewayInfo.Order.Value;

			return shipmentGateway;
		}

		protected override ShipmentGateway FindMatchingBusinessObject(GatewayInfo gatewayInfo)
		{
			var orgAddress = new OrganisationDataObjectReader(gatewayInfo.Forwarder, logger, factory).GetMatched();
			if (orgAddress == null)
			{
				return null;
			}

			gatewayInfoAddressMap.Add(gatewayInfo, orgAddress);
			return shipment.Gateways.FirstOrDefault((shipmentGateway) => shipmentGateway.JSG_OA_ForwarderAddress == orgAddress.PK);
		}

		readonly Dictionary<GatewayInfo, OrgAddress> gatewayInfoAddressMap = new Dictionary<GatewayInfo, OrgAddress>();
	}
}
