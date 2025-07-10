using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVShipmentACASStateDetailsProvider
	{
		string Populate(IForwardingShipment forwardingShipment, ZString acasShipmentNumber, ZPropertyInfo displayInformationInfo);
	}
}
