using CargoWise.Types;
using static Enterprise.Integration.Customs.CA;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.eTail.Integration
{
	public interface ICAeManifestHouseBillFromHVLVShipmentPopulator
	{
		ICusCAeMHMaster Populate(IForwardingShipment shipment);

		ZString GetCCN(string houseCCNSuffix);
	}
}
