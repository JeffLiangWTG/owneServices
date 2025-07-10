using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class AddressProvider : INCTSAddress
{
	public AddressProvider(OrgAddress address)
	{
		Argument.NotNull(address, nameof(address));
		this.address = address;
	}
	readonly OrgAddress address;

	public AddressProvider(JobDocAddress docAddress)
	{
		Argument.NotNull(docAddress, nameof(docAddress));
		this.docAddress = docAddress;
	}
	readonly JobDocAddress docAddress;

	public string Country => address?.OA_RN_NKCountryCode ?? docAddress?.E2_RN_NKCountryCode;

	public string PostCode => address?.OA_PostCode ?? docAddress?.E2_Postcode;

	public string City => address?.OA_City ?? docAddress?.E2_City;

	public string StreetAndNumber => address != null ? address.OA_Address1 + address.OA_Address2 : docAddress.E2_Address1AndE2_Address2;
}
