using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class HVLVActionsTopLevelMenu : BaseHVLVMenuGroup
	{
		public HVLVActionsTopLevelMenu(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("dbf3164b-cb15-4c32-af0b-424621e29107", "HVLV"), shipment, false)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			MenuItems.Add(new CalculateLMCDepotDetailsMenuItem(shipment));
			MenuItems.Add(new HVLVPreScreenMenuGroup(shipment));
			MenuItems.Add(new HVLVManifestMenuItem(shipment));
			MenuItems.Add(new HVLVCustomsMenuGroup(shipment));
			MenuItems.Add(new NavigateToEcommerceWebPortalsMenuGroup(shipment));
			MenuItems.Add(new CreateTestConsignmentMenuItem(shipment));
		}
	}
}
