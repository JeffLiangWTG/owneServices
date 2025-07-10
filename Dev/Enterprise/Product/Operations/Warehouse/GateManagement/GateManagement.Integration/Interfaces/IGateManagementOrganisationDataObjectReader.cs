using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.GateManagement.Integration
{
	public interface IGateManagementOrganisationDataObjectReader
	{
		IOrgAddress GetMatched();
		IJobDocAddress GetMatchedOrNew(IDocAddresses jobDocAddressParent);
		void PopulateJobDocAddress(IOrgAddress orgAddress, IJobDocAddress jobDocAddress);
	}
}
