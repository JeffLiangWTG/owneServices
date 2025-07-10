using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI;

public class CreateTestHVLVShipmentMenuGroup : ZMenuItem
{
	public CreateTestHVLVShipmentMenuGroup(ForwardingConsol consol)
		: base(ResString.GetMultilingualString("41ca5aa6-dc87-40ed-82db-425745be9dc8", "Create HVLV Test Data"))
	{
		MenuItems.Add(new CreateTestHVMShipmentMenuItem(consol));
		MenuItems.Add(new CreateTestHVLShipmentMenuItem(consol));
	}
}
