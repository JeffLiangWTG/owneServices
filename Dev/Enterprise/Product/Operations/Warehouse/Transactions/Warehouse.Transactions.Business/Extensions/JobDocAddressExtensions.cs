using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class JobDocAddressExtensions
	{
		public static JobDocAddress LoadJobDocAddressQuickly(this IDocAddresses jobWithDocAddresses, DocAddressType addressType)
		{
			return jobWithDocAddresses.DocAddresses.FindByDocAddressType(addressType);
		}

		public static OrgHeader GetOrganisation(this JobDocAddress docAddress) => !docAddress.E2_AddressOverride ? docAddress.Organisation : null;
	}
}
