using Enterprise.MasterFiles.Business;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentWithTransportCompanyAndBookingParty : IPackingParent
	{
		OrgAddress GetTransportCompany(string packageId, string partyType);
		OrgAddress GetBookingParty(string packageId, string partyType);
	}
}
