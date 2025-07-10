using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class HVLVPreScreenMenuGroup : BaseHVLVMenuGroup
	{
		public HVLVPreScreenMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("1fbe837a-acd2-41ae-8fb5-56365ce6b3ad", "Pre-Screen"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			MenuItems.Add(new PreScreenMenuItem(shipment.GetHVLVConsignmentHeader()));
			MenuItems.Add(new AcceptAllPreScreeningErrorsAsAcceptedMenuItem(shipment));
		}
	}
}
