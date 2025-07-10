using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class TransportCoConstants
	{
		public const DocAddressType AddressType = DocAddressType.TransportCompanyDocumentaryAddress;
		public const string AddressTypeCode = DocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

		public static JobDocAddressRequirement Requirement => new JobDocAddressRequirement(AddressType, ContactType.TransportServices);
		public static DocAddressType[] GetSupportedAddressTypes() => new[] { AddressType };
	}
}
