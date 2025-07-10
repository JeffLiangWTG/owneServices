using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class ShippersSecurityEndorsementBuilder
	{
		public ShippersSecurityEndorsementBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;

		public ShippersSecurityEndorsement Build()
		{
			var endorsement = new ShippersSecurityEndorsement(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);

			if (shipment.Consignor != null)
			{
				endorsement.Consignor = AddressBuilder.Create(context, shipment.Consignor.MainAddress);
			}

			endorsement.ForwardingAgentReference = AWBNumber();
			return endorsement;
		}

		ZString AWBNumber()
		{
			if (!shipment.Consols.Any())
			{
				return ZString.Empty;
			}

			return shipment.Consols[0].AWBHeader?.EH_BillNumber ?? ZString.Empty;
		}
	}
}
