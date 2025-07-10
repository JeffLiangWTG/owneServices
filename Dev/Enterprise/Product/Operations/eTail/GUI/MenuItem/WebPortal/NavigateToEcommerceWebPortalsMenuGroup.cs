using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class NavigateToEcommerceWebPortalsMenuGroup : BaseHVLVMenuGroup
	{
		public NavigateToEcommerceWebPortalsMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("29553A5A-E2FF-4820-8C8F-26717C8A0FDF", "Ecommerce Web Portals"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			MenuItems.Add(new NavigateToEcommerceOriginDepotMenuItem(shipment));
			MenuItems.Add(new NavigateToEcommerceDestinationDepotMenuItem(shipment));
		}
	}
}
