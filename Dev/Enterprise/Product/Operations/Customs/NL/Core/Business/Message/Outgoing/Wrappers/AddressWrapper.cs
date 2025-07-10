using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class AddressWrapper : IAddress
{
	public AddressWrapper(OrgAddress address)
	{
		this.address = Argument.NotNull(address, nameof(address));
	}
	readonly OrgAddress address;

	public string CityName => address.OA_City;

	public string CountryCode => address.OA_RN_NKCountryCode;

	public string Line => (address.OA_Address1 + " " + address.OA_Address2).Trim();

	public string PostcodeId => address.OA_PostCode;
}
