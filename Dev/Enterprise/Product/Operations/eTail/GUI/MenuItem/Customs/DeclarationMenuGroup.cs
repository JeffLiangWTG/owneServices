using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class DeclarationMenuGroup : BaseHVLVMenuGroup
	{
		public DeclarationMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("001a5145-c5a2-442d-b05c-11a99baab2cf", "Declaration"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			MenuItems.Add(new ViewEditCustomsDeclarationsMenuItem(shipment));
			MenuItems.Add(new ConvertConsignmentsToStandAloneDeclarationMenuItem(shipment));
		}
	}
}
