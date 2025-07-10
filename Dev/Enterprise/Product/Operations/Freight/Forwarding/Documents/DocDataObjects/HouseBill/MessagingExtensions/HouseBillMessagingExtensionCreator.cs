using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents
{
	public static class HouseBillMessagingExtensionCreator
	{
		public static IMessagingExtensions New(ForwardingShipment shipment, IDocument document, IMessageInstructions messageInstructions)
		{
			if (document?.Data?.Value is HouseBill houseBill)
			{
				if (houseBill.IsDraft)
				{
					return new CarrierHouseBillMessagingExtension(shipment, messageInstructions);
				}
			}

			var billOfLadingType = shipment?.JS_HouseBillOfLadingType ?? ZString.Empty;
			if (billOfLadingType == Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL)
			{
				return new FIATAHouseBillMessagingExtension(shipment, messageInstructions);
			}

			return new DefaultHouseBillMessagingExtension(shipment);
		}
	}
}
