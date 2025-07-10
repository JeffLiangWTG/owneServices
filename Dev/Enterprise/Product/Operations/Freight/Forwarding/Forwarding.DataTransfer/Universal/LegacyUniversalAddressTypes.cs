using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class LegacyUniversalAddressTypes
	{
		public static ZString LegacyShipmentControllingPartyAddressType
		{
			get { return "ShipmentControllingParty"; }
		}

		public static ZString LegacyOrderControllingPartyAddressType
		{
			get { return "OrderControllingParty"; }
		}
	}
}
